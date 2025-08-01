using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace HcomDiagUi
{
    public class RecvSerialComms
    {
        // Two kinds of frames come over the UART channel

        CommsManager _commsMgr;
        UiSupport _uiSupport;
        int _hcomHdrLen;
        Thread pullMsgThread;
        readonly AutoResetEvent dataAddedEvent = new AutoResetEvent(false);

        CircularBuffer msgCirBuffer;

        //StreamWriter streamWriter;

        // It seems that the .Net SerialPort class is not all it could be.
        // To acheive reliable operation some SerialPort class methods must
        // not be used. When receiving, the BaseStream must be used.
        // http://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport

        public bool Verbose { get; set; }

        //-------------------------------------------------------------
        // Constructor
        public RecvSerialComms(CommsManager commsMgr, UiSupport uiSupport)
        {
            _uiSupport = uiSupport;
            _commsMgr = commsMgr;

            _hcomHdrLen = Protocol.HcomProtoHdrMessageSize();

            //if (!displayOnConsole)
            //{
            //    // Create a stream to save received data
            //    DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            //    string fullFileName = $"{_fileName}-{dateTimeOffset:HHmmss}.txt";
            //    string msg = string.Format($"Output redirected to {fullFileName}");
            //    _uiSupport.WriteStatus(msg);

            //    // Support writing to file
            //    streamWriter = new StreamWriter(File.Open(fullFileName, FileMode.Create));
            //}

            msgCirBuffer = new CircularBuffer();
            msgCirBuffer.Init(Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE * 8, _uiSupport);

            //// Start a thread to processed received data
            //pullMsgThread = new Thread(new ThreadStart(PullAndProcessThread));
            //pullMsgThread.Start();

            //var t = ReadPortAsync();
        }

        internal void sendButtonClicked()
        {
            // Start a thread to processed received data
            pullMsgThread = new Thread(new ThreadStart(PullAndProcessThread));
            pullMsgThread.Start();

            var t = ReadPortAsync();
        }

        //-------------------------------------------------------------
        internal void Close()
        {
            //streamWriter.Close();
            _commsMgr.SerialPort.Close();
        }

        public void Shutdown()
        {
            Close();
        }

        //-------------------------------------------------------------
        // All received data handled here
        private async Task ReadPortAsync()
        {
            byte[] buffer = new byte[512];

            // Wait for port to open if need be
            while(!_commsMgr.SerialPort.IsOpen)
            {
                Thread.Sleep(50);
            }

            try
            {
                while (true)
                {
                    //var byteCount = Math.Min(_commsMgr.SerialPort.BytesToRead, buffer.Length);
                    var byteCount = buffer.Length;
                    if (byteCount > 0)
                    {
                        var receivedLength = await _commsMgr.SerialPort.BaseStream.ReadAsync(buffer, 0, byteCount).ConfigureAwait(false);
                        AddAndProcessData(buffer, receivedLength);
                    }
                    await Task.Delay(1).ConfigureAwait(false);
                }
            }
            catch (ThreadAbortException tae)
            {
                string msg = string.Format($"ThreadAbortException: {tae}");
                _uiSupport.WriteStatusTime(msg);
            }
            catch (InvalidOperationException ioe)
            {
                string msg = string.Format($"InvalidOperationException: {ioe}");
                _uiSupport.WriteStatusTime(msg);
            }
            catch (Exception ex)
            {
                string msg = string.Format($"Exception: {ex} may mean the target connection dropped");
                _uiSupport.WriteStatusTime(msg);
            }
        }

        //==================================================================
        void AddAndProcessData(byte[] buffer, int availableBytes)
        {
            HcomBufferReturn result;
            
            while (true)
            {
                result = msgCirBuffer.AddBytes(buffer, 0, availableBytes);
                dataAddedEvent.Set(); // wake up read buffer thread

                if (result == HcomBufferReturn.CIR_BUF_ADD_SUCCESS)
                {
                    //_uiSupport.WriteSendRecvTimeLine("--->CirBuf Add Success");
                    break;
                }
                else if (result == HcomBufferReturn.CIR_BUF_ADD_WONT_FIT)
                {
                    _uiSupport.WriteSendRecvTimeLine("--->CirBuf Add won't fit");

                    // Wasn't possible to put these bytes in the buffer. We need
                    // to wait for the other thread and then retry to add this
                    // data and set dataAddedEvent again.
                    Thread.Sleep(1);      // Time to see above message
                    continue;
                }
                else if (result == HcomBufferReturn.CIR_BUF_ADD_BAD_ARG)
                {
                    // Something wrong with implemenation
                    Debug.Assert(false);
                }
                else
                {
                    // Undefined return value????
                    Debug.Assert(false);
                }
            }
        }

        //===========================================================
        // Second thread for processing commands to be sent
        public void PullAndProcessThread()
        {
            HcomBufferReturn bufReturn;

            try
            {
                while (true)
                {
                    // Wait to be signaled that something is ready or timeout
                    var waitReturn = dataAddedEvent.WaitOne(5000);

                    // Loop till circular buffer empty
                    do
                    {
                        bufReturn = PullAndProcessAllPackets();

                    } while (bufReturn == HcomBufferReturn.CIR_BUF_GET_FOUND_MSG);

                    if (bufReturn != HcomBufferReturn.CIR_BUF_GET_NONE_FOUND)
                    {
                        // Unexpected return value from cirbuf
                        _uiSupport.WriteDebugLine($"Error: PullAndProcess returned:{bufReturn}. Ignored");
                    }
                }
            }
            catch (Exception e)
            {
                _uiSupport.WriteDebugLine($"Exception: PullAndProcess:{e}");
            }  
        }

        //===========================================================
        // Loop here finding all valid packets
        HcomBufferReturn PullAndProcessAllPackets()
        {
            byte[] packetBuffer = new byte[Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE];
            byte[] decodedBuffer = new byte[Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE];

            int packetLength;
            HcomBufferReturn result;
            ConsoleColor origForeground = Console.ForegroundColor;
            HostRqstType hostRqstType = HostRqstType.HCOM_HOST_REQUEST_UNDEFINED_REQUEST;

            while (true)
            {
                result = msgCirBuffer.GetNextPacket(packetBuffer,
                    Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE, out packetLength);

                if (result == HcomBufferReturn.CIR_BUF_GET_NONE_FOUND)
                    break;      // We've emptied buffer of all messages

                if (result == HcomBufferReturn.CIR_BUF_GET_BUF_NO_ROOM)
                {
                    // The buffer to receive the message is too small! Perhaps 
                    // corrupted data in buffer.
                    Debug.Assert(false);
                }

                // Process the message
                if (result == HcomBufferReturn.CIR_BUF_GET_FOUND_MSG)
                {
                    if (packetLength == 1)
                        continue;

                    // Don't include delimiter
                    int decodedSize = CobsTools.CobsDecoding(packetBuffer, --packetLength,
                        ref decodedBuffer);

                    // If a message is too short it is ignored. It's probably a
                    // fragment of a message that was partially queued. This
                    // happens when no receiver is connected to read the messages
                    // for a while.
                    if (decodedSize < Protocol.HcomProtoHdrMessageSize())
                        continue;

                    if (decodedSize > 0)
                    {
                        // The hcom header is present in all non-data messages from
                        // Meadow. There may be more information but this will tell
                        // us enough to continue processing.
                        HcomProtoHdrMessage hdrMsg;
                        int size = Marshal.SizeOf(typeof(HcomProtoHdrMessage));
                        IntPtr ptr = Marshal.AllocHGlobal(size);
                        Marshal.Copy(decodedBuffer, 0, ptr, size);
                        hdrMsg = (HcomProtoHdrMessage)Marshal.PtrToStructure(ptr,
                            typeof(HcomProtoHdrMessage));
                        Marshal.FreeHGlobal(ptr);
                        
                        UInt32 payloadLength = (UInt32)(decodedSize - _hcomHdrLen);
                        hostRqstType = (HostRqstType)hdrMsg.stdHeader.rqstType;

                        // Route message around. This mostly displays a message and returns.
                        // Unless we are doing something more complex. 
                        _commsMgr.ProcessRecvdMsgByType(hdrMsg, hostRqstType,
                            decodedBuffer, decodedSize, payloadLength);
                    }
                }
                else
                {
                    string msg = string.Format($"Orphan Data: 0x{packetBuffer[packetLength - 1]:x2}");
                    _uiSupport.WriteStatusLine(msg);
                }
            }

            return result;
        }
    }
}


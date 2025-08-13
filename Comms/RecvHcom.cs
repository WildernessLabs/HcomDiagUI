using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
// RJCP
// using RJCP.IO.Ports;
// .Net
using System.IO.Ports;

namespace HcomDiagUi
{
    public class RecvSerialComms
    {
        // Two kinds of frames come over the UART channel

        CommsManager _commsMgr;
        UiSupport _uiSupport;
        int _hcomHdrLen;
        bool _threadsCreated = false;
        CircularBuffer _msgCirBuffer;
        readonly ManualResetEvent _dataAddedEvent = new ManualResetEvent(false);
        Thread pullMsgThread;

        // RJCP for Sync read
        // Thread recvMsgThread;

        // It seems that the .Net SerialPort class is not all it could be.
        // To acheive reliable operation some SerialPort class methods must
        // not be used. When receiving, the BaseStream must be used.
        // http://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport

        public bool Verbose { get; set; }
        public CircularBuffer GetCircularBuffer()
        {
            return _msgCirBuffer;
        }

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

            _msgCirBuffer = new CircularBuffer();
            _msgCirBuffer.Init(Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE * 8, _uiSupport);
        }

        internal void sendButtonClicked()
        {
            if (_threadsCreated)
                return;

            _threadsCreated = true;

            // Start a thread to processed received data
            pullMsgThread = new Thread(new ThreadStart(PullAndProcessThread));
            pullMsgThread.Start();

            // RJCP
            //recvMsgThread = new Thread(new ThreadStart(ReceiveHcomMessagesThread));
            //recvMsgThread.Start();

            // .Net Async-This thread will do the receiving
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

        // RJCP
        //void ReceiveHcomMessagesThread()
        // .Net
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
                // JRCP Sync
                // This is a very basic way to receive serial
                //while (true)
                //{
                //    // This is a blocking call, ugly but it works.
                //    var bytesReceived = _commsMgr.SerialPort.Read(buffer, 0, buffer.Length);
                //    if (bytesReceived > 0)
                //    {
                //        AddRecvdDataToCirQueue(buffer, bytesReceived);
                //    }
                //}

                // .Net Async
                while (true)
                {
                    //var byteCount = Math.Min(_commsMgr.SerialPort.BytesToRead, buffer.Length);
                    var byteCount = buffer.Length;
                    if (byteCount > 0)
                    {
                        var receivedLength = await _commsMgr.SerialPort.BaseStream.ReadAsync(buffer, 0, byteCount).ConfigureAwait(false);
                        AddRecvdDataToCirQueue(buffer, receivedLength);
                    }
                    await Task.Delay(1).ConfigureAwait(false);
                }
            }
            catch (IOException ioex)
            {
                string msg = string.Format($"IOException: {ioex}");
                _uiSupport.WriteStatusTime(msg);
            }
            catch (ThreadAbortException tae)
            {
                string msg = string.Format($"ThreadAbortException: {tae}");
                _uiSupport.WriteStatusTime(msg);
            }
            catch (InvalidOperationException iope)
            {
                string msg = string.Format($"InvalidOperationException: {iope}");
                _uiSupport.WriteStatusTime(msg);
            }
            catch (Exception ex)
            {
                string msg = string.Format($"Exception: {ex} may mean the target connection dropped");
                _uiSupport.WriteStatusTime(msg);
            }
        }

        //==================================================================
        void AddRecvdDataToCirQueue(byte[] buffer, int availableBytes)
        {
            HcomBufferReturn result;
            
            while (true)
            {
                // For testing
                //_uiSupport.WriteSendRecvTimeLine($"Recvd {availableBytes}");

                if (availableBytes == 0)
                    return;

                result = _msgCirBuffer.AddBytes(buffer, 0, availableBytes);
                _dataAddedEvent.Set(); // wake up read buffer thread

                if (result == HcomBufferReturn.CIR_BUF_ADD_SUCCESS)
                {
                    // _uiSupport.WriteSendRecvTimeLine($"--->CirBuf Added {availableBytes} encoded bytes");
                    break;
                }
                else if (result == HcomBufferReturn.CIR_BUF_ADD_WONT_FIT)
                {
                    _uiSupport.WriteSendRecvTimeLine("--->CirBuf Add won't fit");

                    // Wasn't possible to put these bytes in the buffer. We need
                    // to wait for the other thread and then retry to add this
                    // data and set _dataAddedEvent again.
                    Thread.Sleep(1);      // Time to see above message
                    continue;
                }
                else if (result == HcomBufferReturn.CIR_BUF_ADD_BAD_ARG)
                {
                    // Something wrong with implementation
                    Thread.Sleep(300);
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
                    // Wait to be signaled that something has been put into
                    // the circular buffer or timeout
                    var waitReturn = _dataAddedEvent.WaitOne(5000);

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
                    // Needed for Manual Reset Event
                    _dataAddedEvent.Reset();
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
                result = _msgCirBuffer.GetNextPacket(packetBuffer,
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
                    // A single byte is probably the second delimiter added
                    // to the messages by HCOM
                    if (packetLength == 1)
                        continue;           // Too short

                    //_uiSupport.WriteSendRecvTimeLine($"--->Pulled encoded msg {packetLength} byte(s)");

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
                        // Meadow. There may be more information but thr header will
                        // tell us enough to continue processing.
                        HcomProtoHdrMessage hdrMsg;
                        int size = Marshal.SizeOf(typeof(HcomProtoHdrMessage));
                        IntPtr ptr = Marshal.AllocHGlobal(size);
                        Marshal.Copy(decodedBuffer, 0, ptr, size);
                        hdrMsg = (HcomProtoHdrMessage)Marshal.PtrToStructure(ptr,
                            typeof(HcomProtoHdrMessage));
                        Marshal.FreeHGlobal(ptr);
                        
                        UInt32 payloadLength = (UInt32)(decodedSize - _hcomHdrLen);
                        hostRqstType = (HostRqstType)hdrMsg.stdHeader.rqstType;

                        //_uiSupport.WriteSendRecvTimeLine($"-->COBS Decoded Rcv'd Message. RqstType:{hostRqstType}");

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


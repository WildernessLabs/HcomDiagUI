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
        Thread thread;

        // The port this receiver was created for. Captured so that after a
        // reconnect an old receiver never reads from the new port.
        readonly SerialPort _serialPort;

        // Set by Shutdown() so the read task and processing thread exit and
        // an intentional close is not mistaken for a dropped connection.
        volatile bool _stopping;
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
            _serialPort = commsMgr.SerialPort;

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

            // Start thread to processed received data
            // Background so it never keeps the process alive after the form closes
            thread = new Thread(new ThreadStart(PullAndProcessThread));
            thread.IsBackground = true;
            thread.Start();

            // Start receiving from meadow
            var t = ReadPortAsync();
        }

        //-------------------------------------------------------------
        internal void Close()
        {
            //streamWriter.Close();
            _serialPort.Close();
        }

        // Stops the read task and processing thread. Does not wait for them,
        // since the processing thread may be blocked in a UI Invoke.
        public void Shutdown()
        {
            _stopping = true;
            dataAddedEvent.Set();   // Wake the processing thread so it can exit
            Close();
        }

        //-------------------------------------------------------------
        // All received data handled here
        private async Task ReadPortAsync()
        {
            byte[] buffer = new byte[512];
            while(!_serialPort.IsOpen && !_stopping)
            {
                Thread.Sleep(50);
            }

            try
            {
                while (!_stopping)
                {
                    //var byteCount = Math.Min(_serialPort.BytesToRead, buffer.Length);
                    var byteCount = buffer.Length;
                    if (byteCount > 0)
                    {
                        var receivedLength = await _serialPort.BaseStream.ReadAsync(buffer, 0, byteCount).ConfigureAwait(false);
                        AddAndProcessData(buffer, receivedLength);
                    }
                    await Task.Delay(1).ConfigureAwait(false);
                }
            }
            catch (Exception) when (_stopping)
            {
                // Port closed by Shutdown(); not a dropped connection
            }
            catch (ThreadAbortException tae)
            {
                string msg = string.Format($"ThreadAbortException: {tae}");
                _uiSupport.WriteStatusLine(msg);
            }
            catch (InvalidOperationException ioe)
            {
                string msg = string.Format($"InvalidOperationException: {ioe}");
                _uiSupport.WriteStatusLine(msg);
            }
            catch (IOException ioException)
            {
                // Is it possible to retry the connection from here?
                string msg = string.Format($"IOException: {ioException}");
                _uiSupport.WriteStatusLine(msg);

                _commsMgr.ReOpenSerialPort(60);
            }
            catch (Exception ex)
            {
                string msg = string.Format($"Exception: {ex} may mean the target connection dropped");
                _uiSupport.WriteStatusLine(msg);
            }
        }

        //==================================================================
        void AddAndProcessData(byte[] buffer, int availableBytes)
        {
            HcomBufferReturn result;
            
            while (true)
            {
                result = msgCirBuffer.AddBytes(buffer, 0, availableBytes);
                dataAddedEvent.Set(); // wake up display thread

                if (result == HcomBufferReturn.CIR_BUF_ADD_SUCCESS)
                {
                    break;
                }
                else if (result == HcomBufferReturn.CIR_BUF_ADD_WONT_FIT)
                {
                    // Wasn't possible to put these bytes in the buffer. We need
                    // to wait for a few packets to be processed and then retry to
                    // add this data
                    dataAddedEvent.Set(); // wake up display thread if sleeping
                    if (_stopping)
                        break;
                    Thread.Sleep(250);
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
        // Second thread for display otherwise loose data
        public void PullAndProcessThread()
        {
            HcomBufferReturn bufReturn;

            while (true)
            {
                // Wait to be signaled that something is ready
                dataAddedEvent.WaitOne();
                if (_stopping)
                    break;

                bufReturn = PullAndProcessAllPackets();
                if(bufReturn != HcomBufferReturn.CIR_BUF_GET_NONE_FOUND)
                {
                    string msg = string.Format($"PullAndProcess returned:{bufReturn}");
                    _uiSupport.WriteStatus(msg);
                }
            }
        }

        //===========================================================
        HcomBufferReturn PullAndProcessAllPackets()
        {
            byte[] packetBuffer = new byte[Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE];
            byte[] decodedBuffer = new byte[Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE];

            int packetLength;
            HcomBufferReturn result;
            ConsoleColor origForeground = Console.ForegroundColor;

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
                        // The hcom header is present in all messages from Meadow.
                        // There may be more information but this will tell us
                        // enough to continue processing.
                        HcomProtoHdrMessage hdrMsg;
                        int size = Marshal.SizeOf(typeof(HcomProtoHdrMessage));
                        IntPtr ptr = Marshal.AllocHGlobal(size);
                        Marshal.Copy(decodedBuffer, 0, ptr, size);
                        hdrMsg = (HcomProtoHdrMessage)Marshal.PtrToStructure(ptr,
                            typeof(HcomProtoHdrMessage));
                        Marshal.FreeHGlobal(ptr);
                        
                        UInt32 payloadLength = (UInt32)(decodedSize - _hcomHdrLen);
                        HostRqstType hostRqstType = (HostRqstType)hdrMsg.stdHeader.rqstType;

                        if(hostRqstType == HostRqstType.HCOM_HOST_REQUEST_TEXT_CONCLUDED)
                        {
                            _commsMgr.CommandHasConcluded();
                        }

                        // Route message around
                        _commsMgr.ForwardToProcessMsgs(hdrMsg, hostRqstType,
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


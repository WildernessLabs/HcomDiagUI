using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// Install the nuget package.
// Note: this code only works on Windows and Linux.
// See https://github.com/jcurl/RJCP.DLL.SerialPortStream for details.
// In this solution find all the "// RJCP" entries and uncommment the
// code. Also comment out all the "// .Net" titled entries.
// RJCP
// using RJCP.IO.Ports;
// .Net
using System.IO.Ports;

namespace HcomDiagUi
{
    public class CommsManager
    {
        // Need to pass data from UI elements to another thread
        class NeededUiData
        {
            internal MeadowRqstData rqstInfo;
            internal UInt32 userData;
            internal UInt16 extraData;

            public NeededUiData(MeadowRqstData rqstInfo, UInt32 userData, UInt16 extraData)
            {
                this.rqstInfo = rqstInfo;
                this.userData = userData;
                this.extraData = extraData;
            }
        }

        UiSupport _uiSupport;
        HcomDiagUiForm _hcomUiForm;
        string _serialPortName;
        bool _cancelSending;
        bool _sendingInProgress;
        //SerialPortStream _serialPort; // RJCP
        SerialPort _serialPort;     // .Net

        // Note: using ManualResetEvent because the code setting the
        // event may execute before or after the wait.
        ManualResetEvent _concludedRecvdEvent;

        RecvSerialComms _recvSerialComms;
        SendSerialComms _sendSerialComms;
        NormalRcvdProc _recvNormalMsgs;
        UploadProcessing _recvUploadMsgs;
        DnldF7Files _recvF7DownloadMsgs;
        DnldEsp32Files _recvEspDownloadMsgs;

        public bool IsFileDnldActive
        {
            get { return _recvF7DownloadMsgs.IsFileDnldActive; }
            set { _recvF7DownloadMsgs.IsFileDnldActive = value; }
        }

        //==========================================================================
        // Constructor
        public CommsManager(HcomDiagUiForm hcomUiForm)
        {
            _hcomUiForm = hcomUiForm;
            _uiSupport = _hcomUiForm.UiSupport;
            _sendingInProgress = false;

            // Initially not signaled
            _concludedRecvdEvent = new ManualResetEvent(false);

            // Read the serial port name and open the serial port
            _serialPortName = _hcomUiForm.ReadPortName();

            bool result = OpenSerialPort(_serialPortName);
            if (result)
                _uiSupport.WriteStatusLine($"Com Port '{_serialPortName}' opened successfully");
            else
                _uiSupport.WriteStatusLine($"Com Port '{_serialPortName}' failed to open");

            // Port opened so we can proceed
            _recvSerialComms = new RecvSerialComms(this, _uiSupport);
            _sendSerialComms = new SendSerialComms(this, _uiSupport, _recvSerialComms.GetCircularBuffer());
            _recvNormalMsgs = new NormalRcvdProc(this, _uiSupport);
            _recvUploadMsgs = new UploadProcessing(this, _uiSupport);
            _recvF7DownloadMsgs = new DnldF7Files(this, _uiSupport, _sendSerialComms);
            _recvEspDownloadMsgs = new DnldEsp32Files(this, _uiSupport);
        }

        //=======================================================================
        // This is used by upload
        public void BuildHdrMsgToSend(MeadowRqstType rqstType, UInt32 userData, UInt16 extraData)
        {
            _sendSerialComms.BuildAndSendHdrMessage(rqstType, userData, extraData);
        }

        //=======================================================================
        // All 'Send' button clicks come here to begin sending to Meadow
        public void BuildMsgToSend()
        {
            lock(this)
            {
                if(_sendingInProgress)
                    return;     // All ready active
                else
                    _sendingInProgress = true;
            }

            _recvSerialComms.sendButtonClicked();

            // Necessary because only the UI thread can access UI elements
            MeadowRqstData rqstInfo = (MeadowRqstData)_hcomUiForm.commandListBox.SelectedItem;
            UInt32 userData = UInt32.Parse(_hcomUiForm.userDataTextBox.Text);
            UInt16 extraData = UInt16.Parse(_hcomUiForm.extraDataTextBox.Text);
            var UIData = new NeededUiData(rqstInfo, userData, extraData);

            // Execute requested action
            if (!ThreadPool.QueueUserWorkItem(BuildMsgIterations, UIData))
            {
                // Grabing a thread from the thread pool failed
                //_uiSupport.WriteDebugLine("ThreadPool.QueueUserWorkItem error");
                _sendingInProgress = false;
                _uiSupport.SendButtonEnable(true);

                throw new Exception("ThreadPool.QueueUserWorkItem(BuildMsgToSendHelper)");
            }
        }
        
        //=======================================================================
        // This code is executed by a thread pool thread.
        internal void BuildMsgIterations(Object state)
        {
            _uiSupport.WriteSendRecvTimeLine($"[{Thread.CurrentThread.ManagedThreadId}]==>Entered Do loop");

            bool waitResult;
            int displayCount = 0;
            int loopCount = _uiSupport.SendIterations() > 1 ? _uiSupport.SendIterations() : 1;

            _uiSupport.ClearExecutionTime();
            _uiSupport.Stopwatch.Restart();

            try
            {
                // This loop handle requested iterations
                do
                {
                    // If cancelled exit now
                    if (IsCancelSendingSet())
                        break;

                    loopCount--;
                    displayCount++;

                    // Update UI too
                    _uiSupport.WriteSendCount(displayCount);
                
                    // Send a single request to host
                    _uiSupport.WriteSendRecvTimeLine($"Iteration {displayCount} calling BuildMsgToSendHelper to send command");
                    BuildMsgToSendHelper(state);

                    // This Event will block the execution of the next command until
                    // the 'Concluded' message is received (or an error occurs) from
                    // Meadow. Because this is a ManualResetEvent it doesn't matter if
                    // it is signalled before or after the thread reaches this point,
                    // the thread will always be released, and only reset after it
                    // reaches this point.
                    waitResult = _concludedRecvdEvent.WaitOne(-1);     // -1 Wait forever
                    //waitResult = _concludedRecvdEvent.WaitOne(30 * 1000);

                    if(! waitResult)
                    {
                        _uiSupport.WriteSendRecvTimeLine($"[{Thread.CurrentThread.ManagedThreadId}]==>Concluded msg event timeout");
                    }

                    _concludedRecvdEvent.Reset();

                } while (loopCount > 0);
            }
            finally
            {
                _uiSupport.Stopwatch.Stop();

                // Allow restarting
                _uiSupport.SendButtonEnable(true);
                _sendingInProgress = false;
            }
            _uiSupport.WriteSendRecvTimeLine($"==>Exiting Interation Loop{Environment.NewLine}");
        }

        //=======================================================================
        // Send a single message to Meadow
        public void BuildMsgToSendHelper(Object state)
        {
            //_uiSupport.WriteSendRecvTimeLine("-->Entered BuildMsgToSendHelper");
            try
            {
                // Grab the UI information from the state
                NeededUiData uiData = (NeededUiData)state;
                MeadowRqstData rqstInfo = uiData.rqstInfo;
                UInt32 userData = uiData.userData;
                UInt16 extraData = uiData.extraData;

                // Pull together the needed information
                // The message classification determines what needs to be sent
                switch (rqstInfo.msgClass)
                {
                    case MessageClasses.Header:
                        _uiSupport.WriteSendRecvTimeLine("-->Sending MessageClass.Header");
                        _sendSerialComms.BuildAndSendHdrMessage(rqstInfo.rqstType, userData, extraData);
                        break;

                    case MessageClasses.Text:
                        _uiSupport.WriteSendRecvTimeLine("-->Sending MessageClass.Text");

                        // Pick up the WriteSendRecvTimeLine data that is in the UI
                        if (rqstInfo.rqstType == MeadowRqstType.HCOM_MDOW_REQUEST_EXEC_DIAG_APP_CMD)
                        {
                            _uiSupport.WriteSendRecvTimeLine("--> Text diagnostic message");
                            // Special case for of text message that includes a field for the number
                            // of arguments in the text.
                            _sendSerialComms.BuildAndSendDiagCmdMsg(rqstInfo.rqstType, userData, extraData,
                                _uiSupport.TextForMsg());
                        }
                        else if (rqstInfo.rqstType == MeadowRqstType.HCOM_MDOW_REQUEST_RTC_SET_TIME_CMD)
                        {
                            _uiSupport.WriteSendRecvTimeLine("--> Set Time text msg");
                            // For setting the time we'll read the time and send it immediately
                            // Special text - build and format current time as UTC
                            // Since NTP provides time in UTC we'll do the same
                            // Using UtcNow, the formats are:
                            // 's' = 2022-04-01T21:34:05
                            // 'o' = 2022-04-01T21:35:03.9174375+00:00
                            // 'u' = 2022-04-01 21:35:31Z - Notice no 'T' but a space
                            // Using Now (local time in Pacific timezone with DST true), the formats are:
                            // 's' = 2022-04-01T14:37:34
                            // 'o' = 2022-04-01T14:38:25.1838288-07:00
                            // 'u' = 2022-04-01 21:38:59Z - Notice UTC time & no 'T'
                            //
                            var timeStringUtc = DateTimeOffset.UtcNow.ToString("o");
                            _sendSerialComms.BuildAndSendTextMsg(rqstInfo.rqstType, userData, extraData,
                                        timeStringUtc);

                            // Send local time (both UTC and local can be sent at the same time)
                            //var timeString = DateTimeOffset.Now.ToString("o");
                            //_sendSerialComms.BuildAndSendTextMsg(rqstInfo.rqstType, userData,
                            //          timeString);
                        }
                        else
                        {
                            // Normal case UserTextBox string is sent
                            _uiSupport.WriteSendRecvTimeLine("--> Sending Normal userData text");
                            _sendSerialComms.BuildAndSendTextMsg(rqstInfo.rqstType, userData, extraData,
                                _uiSupport.TextForMsg());
                        }
                        break;

                    case MessageClasses.Binary:
                        _uiSupport.WriteSendRecvTimeLine("--> Sending MessageClass.Binary");
                        break;

                    case MessageClasses.FileInfo:
                        _uiSupport.WriteSendRecvTimeLine($"[{Thread.CurrentThread.ManagedThreadId}]==>Start of File Download");
                        // Need to initiate a file download.
                        // Note: This thread returns after only sending the start download
                        // message and receiving a response.
                        // After this a thread pool thread is used for the execution of actual data
                        // and end message download.
                        _recvF7DownloadMsgs.DnldStartFileInfo(rqstInfo.rqstType, userData,
                                        _uiSupport.TextForMsg(), _hcomUiForm.ReadHostFileInfo());
                        break;

                    case MessageClasses.NoHdrBin: // Needs to be Deprecated
                        _uiSupport.WriteSendRecvTimeLine("--> Sending MessageClass.NoHdrBin");
                        break;

                    default:
                        _uiSupport.WriteSendRecvTimeLine("--> Sending MessageClass.DEFAULT???");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An exception was caught in BuildMsgToSendHelper: {e}");
                throw;      // Pass it up
            }
            finally
            {
                _uiSupport.WriteExecutionTime();
            }
        }

        //=======================================================================
        // All received messages from Meadow come here for routing and processing
        public void ProcessRecvdMsgByType(HcomProtoHdrMessage hdrMsg, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, uint payloadLength)
        {
            HostRqstData hostRqstData = ProtocolHelper.GetHostRqstTypeData(hostRqstType);

            // Only a few messages are currently "complex" (i.e. needed 2 or
            // more messages to complete their purpose)
            switch (hostRqstData.procState)
            {
                case CurrentProcState.Normal:
                    // Only show normal traffic not data upload/download packets
                    _recvNormalMsgs.DisplayNormalMsgData(hdrMsg, hostRqstData, hostRqstType,
                        messageBuffer, messageLength, payloadLength);
                    break;

                case CurrentProcState.F7FileDnldRdy:
                    // We're doing a Meadow File Download
                    _uiSupport.WriteSendRecvTimeLine($"(r) HCOM ready for file data");

                    _recvF7DownloadMsgs.DnldChkF7ReadyMsg(hdrMsg, hostRqstData, hostRqstType,
                        messageBuffer, messageLength, payloadLength);
                    break;

                case CurrentProcState.EspFileDnld:
                    _recvEspDownloadMsgs.ProcessRcvdEsp32Dnld(hdrMsg, hostRqstData, hostRqstType,
                        messageBuffer, messageLength, payloadLength);
                    break;

                case CurrentProcState.F7FileUpload:
                    _recvUploadMsgs.ProcessRcvdUpload(hdrMsg, hostRqstData, hostRqstType,
                        messageBuffer, messageLength, payloadLength);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            // Special message processing
            if (hostRqstType == HostRqstType.HCOM_HOST_REQUEST_TEXT_CONCLUDED)
            {
                if (IsFileDnldActive)
                {
                    // End of download activity
                    IsFileDnldActive = false;
                }

                // Release the loop thread to send next Meadow message, if there is one
                setConcludedEvent();
            }
        }

        //-------------------------------------------------------------------------------
        // RJCP
        // public SerialPortStream SerialPort { get => _serialPort; set => _serialPort = value; }
        // .Net
        public SerialPort SerialPort { get => _serialPort; set => _serialPort = value; }


        //-------------------------------------------------------------------------------
        public bool IsCancelSendingSet()
        {
            return _cancelSending;
        }

        public void SetCancelSending()
        {
            _cancelSending = true;
        }

        public void ClearCancelSending()
        {
            _cancelSending = false;
        }

        //-------------------------------------------------------------------------------
        internal void setConcludedEvent()
        {
            _concludedRecvdEvent.Set();
            _uiSupport.WriteSendRecvTimeLine($"Set Concluded Event");
        }

        //-------------------------------------------------------------------------------
        // Called when port name changes
        public void UpdateSerialPortName()
        {
            _serialPortName = _hcomUiForm.ReadPortName();
        }

        //-------------------------------------------------------------------------------
        public bool OpenSerialPort(string portName)
        {
            try
            {
                // Create a new SerialPort object with default settings.
                // RJCP
                //_serialPort = new SerialPortStream();
                // .Net
                _serialPort = new SerialPort();
                _serialPort.PortName = portName;
                _serialPort.BaudRate = 115200;       // This value is ignored when using ACM
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                //_serialPort.Handshake = Handshake.XOnXOff;
                //_serialPort.DtrEnable = true;
                //_serialPort.RtsEnable = true;
                // Default is 2048 bytes buffer
                //_serialPort.WriteBufferSize = Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE * 2;

                // Set the read/write timeouts
                _serialPort.ReadTimeout = 10000;
                _serialPort.WriteTimeout = 10000;

                _serialPort.Open();

                return true;
            }
            catch (IOException ioe)
            {
                string msg = string.Format($"The specified port '{0}' could not be found or opened. {1}Exception:'{2}'",
                    portName, Environment.NewLine, ioe);
                _uiSupport.WriteStatus(msg);
                throw;
            }
            catch (Exception except)
            {
                string msg = string.Format($"Unknown Open Serial Port exception:{0}", except);
                _uiSupport.WriteStatus(msg);
                throw;
            }
        }
    }
}

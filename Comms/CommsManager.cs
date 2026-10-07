using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        SerialPort _serialPort;
        bool _cancelSending;
        ManualResetEventSlim _concludedRecvdEvent;

        // Serializes Connect, Disconnect and ReOpenSerialPort, which run on
        // the UI thread and the receive task respectively. The generation is
        // bumped on every Connect/Disconnect so an in-progress reopen loop can
        // tell that the user has since taken over the connection. Never make a
        // synchronous UI call (Invoke) while holding this lock: the UI thread
        // may be blocked waiting for it.
        readonly object _portLock = new object();
        int _connectionGeneration;

        // Most recently sent command, named in Meadow error messages
        volatile string _lastCommandName;

        // True while ReOpenSerialPort is retrying after a dropped connection
        volatile bool _reconnecting;

        RecvSerialComms _recvSerialComms;
        SendSerialComms _sendSerialComms;
        NormalRcvdProc _recvNormalMsgs;
        UploadProcessing _recvUploadMsgs;
        DnldF7Files _recvF7DownloadMsgs;
        DnldEsp32Files _recvEspDownloadMsgs;

        // Constructor
        //==========================================================================
        public CommsManager(HcomDiagUiForm hcomUiForm)
        {
            _hcomUiForm = hcomUiForm;
            _uiSupport = _hcomUiForm.UiSupport;

            // Initially nosignaled
            _concludedRecvdEvent = new ManualResetEventSlim();

            // The serial port is not opened here. The UI calls Connect() once
            // the user (or the remembered setting) has chosen a port.
        }

        //-------------------------------------------------------------------------------
        public string PortName => _serialPortName;
        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;
        public bool IsReconnecting => _reconnecting;

        //-------------------------------------------------------------------------------
        // Open the named port and start the receive/process handlers. Any
        // existing connection is closed first. Called on the UI thread.
        public bool Connect(string portName)
        {
            bool connected;

            lock (_portLock)
            {
                _connectionGeneration++;
                CloseConnection();

                _serialPortName = portName;
                connected = OpenSerialPort(portName);
                if (connected)
                {
                    ClearCancelSending();
                    CreateHandlers();
                }
            }

            if (connected)
                _uiSupport.WriteStatusLine($"Com Port '{portName}' opened successfully");
            else
                _uiSupport.WriteStatusLine($"Com Port '{portName}' failed to open");

            _hcomUiForm.ShowConnectionStateSafe();
            return connected;
        }

        //-------------------------------------------------------------------------------
        // Called on the UI thread. Also stops an automatic reconnect.
        public void Disconnect()
        {
            bool wasConnected;
            bool wasReconnecting;

            lock (_portLock)
            {
                _connectionGeneration++;
                wasConnected = IsConnected;
                wasReconnecting = _reconnecting;
                _reconnecting = false;
                CloseConnection();
            }

            // Release a send that is blocked waiting for a 'Concluded' message
            // that can no longer arrive.
            SetCancelSending();
            CommandHasConcluded();

            if (wasConnected)
                _uiSupport.WriteStatusLine($"Com Port '{_serialPortName}' closed");
            else if (wasReconnecting)
                _uiSupport.WriteStatusLine($"Reconnect to {_serialPortName} stopped");

            _hcomUiForm.ShowConnectionStateSafe();
        }

        //-------------------------------------------------------------------------------
        // Caller must hold _portLock
        void CreateHandlers()
        {
            _recvSerialComms = new RecvSerialComms(this, _uiSupport);
            _sendSerialComms = new SendSerialComms(this, _uiSupport);
            _recvNormalMsgs = new NormalRcvdProc(this, _uiSupport);
            _recvUploadMsgs = new UploadProcessing(this, _uiSupport);
            _recvF7DownloadMsgs = new DnldF7Files(this, _uiSupport, _sendSerialComms);
            _recvEspDownloadMsgs = new DnldEsp32Files(this, _uiSupport);
        }

        //-------------------------------------------------------------------------------
        // Stop the receive handlers and close the port. Caller must hold _portLock
        void CloseConnection()
        {
            // Closing a port whose device is already gone (e.g. Meadow reset)
            // can throw; nothing useful can be done about it here and it must
            // not stop a reconnect.
            if (_recvSerialComms != null)
            {
                try
                {
                    _recvSerialComms.Shutdown();
                }
                catch (Exception)
                {
                }
                _recvSerialComms = null;
            }

            if (_serialPort != null)
            {
                try
                {
                    _serialPort.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        //=======================================================================
        public void BuildHdrMsgToSend(MeadowRqstType rqstType, UInt32 userData, UInt16 extraData)
        {
            _sendSerialComms.BuildAndSendHdrMessage(rqstType, userData, extraData);
        }

        //=======================================================================
        // All send button clicks come here to begin sending to Meadow
        public void BuildMsgToSend()
        {
            // Necessary because only the UI thread can access UI elements
            MeadowRqstData rqstInfo = (MeadowRqstData)_hcomUiForm.commandListBox.SelectedItem;
            UInt32 userData = UInt32.Parse(_hcomUiForm.userDataTextBox.Text.Trim());
            UInt16 extraData = UInt16.Parse(_hcomUiForm.extraDataTextBox.Text.Trim());
            var UIData = new NeededUiData(rqstInfo, userData, extraData);

            // Some commands can take a minute or more. Therefore, we need to free this
            // UI thread to return to UI duties and not worry about sending messages.
            if (!ThreadPool.QueueUserWorkItem(BuildMsgToSendHelper, UIData))
            {
                // Grabing a thread from the thread pool failed
                //_uiSupport.WriteDebugLine("ThreadPool.QueueUserWorkItem error");
                _uiSupport.SendButtonEnable(true);
                throw new Exception("ThreadPool.QueueUserWorkItem(BuildMsgToSendHelper)");
            }
        }
        
        //=======================================================================
        // A thread pool thread will do the work
        public void BuildMsgToSendHelper(Object state)
        {
            // Grab the UI information
            NeededUiData uiData = (NeededUiData)state;
            MeadowRqstData rqstInfo = uiData.rqstInfo;
            UInt32 userData = uiData.userData;
            UInt16 extraData = uiData.extraData;
            int dispCount = 0;
            int iterations;

            _uiSupport.ClearExecutionTime();
            _uiSupport.Stopwatch.Restart();

            _concludedRecvdEvent.Reset();       // Reset to nonsignaled
            ClearCancelSending();               // A previous Cancel must not stop this command

            // Read UI iteration field. Always at least 1
            iterations = _uiSupport.SendIterations() > 1 ? _uiSupport.SendIterations() : 1;

            try
            {
                do
                {
                dispCount++;
                _uiSupport.WriteSendRecvTimeLine($"Iteration {dispCount}");

                // Update UI too
                _uiSupport.WriteSendCount(dispCount);
                iterations--;

                // Each command's table entry (MeadowCommands) knows how it's sent
                _lastCommandName = rqstInfo.rqstName;
                _uiSupport.WriteDebugLine($"--> Sending '{rqstInfo.rqstName}' ({rqstInfo.sender.Name})");
                var sendContext = new SendContext(rqstInfo.rqstType, userData, extraData,
                    _uiSupport, _sendSerialComms, _recvF7DownloadMsgs);
                rqstInfo.sender.Send(sendContext);

                // Nothing went to Meadow, so no 'Concluded' will come back
                if (sendContext.NothingSent)
                {
                    _uiSupport.WriteStatusLine($"'{rqstInfo.rqstName}' not sent");
                    break;
                }

                // Must consider that some commands return before the command is actually
                // finished. However, all are expected to send a 'Completed' message.
                //
                // This will block until the concluded message is received or an error occurs.
                _concludedRecvdEvent.Wait();        // Halts thread and waits till concluded recvd
                _concludedRecvdEvent.Reset();       // Manual reset event must be reset for next rep

                // If cancelled exit now
                if (IsCancelSendingSet())
                    break;

            } while (iterations > 0);
            }
            catch (NotConnectedException)
            {
                // Port dropped (e.g. Meadow reset) before or during the send
                _uiSupport.WriteStatusLine("Send failed: Com Port not connected");
            }
            finally
            {
                _uiSupport.Stopwatch.Stop();
                _uiSupport.WriteExecutionTime();
                _uiSupport.SendButtonEnable(true);
            }
        }

        //=======================================================================
        // All received messages from Meadow come here to be routed for processing
        public void ForwardToProcessMsgs(HcomProtoHdrMessage hdrMsg, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, uint payloadLength)
        {
            HostRqstData hostRqstData = HostMessages.Find(hostRqstType);
            _uiSupport.WriteDebugLine($"Received '{hostRqstData.msgComplex}' message");

            // Meadow explains a refused or failed request with an error text
            // (e.g. 'Mono must be disabled for file download'). It is also shown
            // in Send/Received, but there it's easy to miss.
            // An unknown request (e.g. one removed from Meadow.OS) is answered with
            // a rejected text
            if (hostRqstType == HostRqstType.HCOM_HOST_REQUEST_TEXT_ERROR)
                ReportMeadowError("error", messageBuffer, messageLength);
            else if (hostRqstType == HostRqstType.HCOM_HOST_REQUEST_TEXT_REJECTED)
                ReportMeadowError("rejected", messageBuffer, messageLength);

            try
            {
                ForwardToProcessMsgsHelper(hdrMsg, hostRqstData, hostRqstType, messageBuffer,
                    messageLength, payloadLength);
            }
            catch (NotConnectedException)
            {
                // A reply (e.g. upload acknowledgement) could not be sent because the
                // port dropped. Don't let it kill the receive processing thread.
                _uiSupport.WriteStatusLine($"Reply to '{hostRqstType}' not sent: Com Port not connected");
            }
        }

        void ReportMeadowError(string what, byte[] messageBuffer, int messageLength)
        {
            int hdrLen = Protocol.HcomProtoHdrMessageSize();
            string text = messageLength > hdrLen
                ? Encoding.ASCII.GetString(messageBuffer, hdrLen, messageLength - hdrLen).TrimEnd('\0', ' ', '\r', '\n')
                : "(no text)";

            string command = _lastCommandName;
            _uiSupport.WriteStatusLine(command == null
                ? $"Meadow {what}: {text}"
                : $"Meadow {what} ('{command}'): {text}");
        }

        void ForwardToProcessMsgsHelper(HcomProtoHdrMessage hdrMsg, HostRqstData hostRqstData,
            HostRqstType hostRqstType, byte[] messageBuffer, int messageLength, uint payloadLength)
        {
            // Only a few messages are currently complex (i.e. needed 2 or more messages
            // to complete their purpose)
            switch (hostRqstData.msgComplex)
            {
                case MessageComplex.Normal:
                    // Only show normal traffic not data upload/download packets
                    _recvNormalMsgs.DisplayNormalMsgData(hdrMsg, hostRqstData, hostRqstType,
                        messageBuffer, messageLength, payloadLength);
                    break;

                case MessageComplex.F7FileDnld:
                    _recvF7DownloadMsgs.ProcessRcvdF7Dnld(hdrMsg, hostRqstData, hostRqstType,
                        messageBuffer, messageLength, payloadLength);
                    break;

                case MessageComplex.EspFileDnld:
                    _recvEspDownloadMsgs.ProcessRcvdEsp32Dnld(hdrMsg, hostRqstData, hostRqstType,
                        messageBuffer, messageLength, payloadLength);
                    break;

                case MessageComplex.F7FileUpload:
                    _recvUploadMsgs.ProcessRcvdUpload(hdrMsg, hostRqstData, hostRqstType,
                        messageBuffer, messageLength, payloadLength);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        //-------------------------------------------------------------------------------
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

        // Cancel button: stop the remaining iterations and stop waiting for a
        // 'Concluded' message. Meadow doesn't always send one (e.g. after
        // rejecting an invalid path), which used to leave Send disabled until
        // Disconnect.
        public void CancelCommand()
        {
            SetCancelSending();
            CommandHasConcluded();
        }
        public void ClearCancelSending()
        {
            _cancelSending = false;
        }

        //-------------------------------------------------------------------------------
        public void CommandHasConcluded()
        {
            _concludedRecvdEvent.Set();
        }

        //-------------------------------------------------------------------------------
        // Called from the receive task when the connection drops (e.g. Meadow
        // reset). Retries the same port for up to reSeconds. Gives up early if
        // the user connects or disconnects in the meantime.
        public bool ReOpenSerialPort(int reSeconds)
        {
            const int sleepTimeMs = 100;

            const int MsPerSec = 1000;
            int sleepConst = MsPerSec / sleepTimeMs;
            int totalTryCount = reSeconds * sleepConst;
            int generation;

            lock (_portLock)
            {
                generation = _connectionGeneration;
                _reconnecting = true;
                CloseConnection();
            }
            _uiSupport.WriteStatusLine($"Reconnecting to {_serialPortName} (up to {reSeconds} s)...");
            _hcomUiForm.ShowConnectionStateSafe();

            // Release a send that is blocked waiting for a 'Concluded' message
            // that can no longer arrive.
            SetCancelSending();
            CommandHasConcluded();

            try
            {
                for(int i = 0; i < totalTryCount; i++)
                {
                    bool successOpen;

                    lock (_portLock)
                    {
                        // The user pressed Stop (Disconnect) or connected manually
                        if (generation != _connectionGeneration)
                            return false;

                        successOpen = OpenSerialPort(_serialPortName, true);
                        if(successOpen)
                        {
                            // Port opened so we can proceed
                            CreateHandlers();
                        }
                    }

                    if (successOpen)
                    {
                        string msg = string.Format($"Serial Port:{_serialPortName} reopened\n");
                        _uiSupport.WriteStatusLine(msg);
                        return true;
                    }
                    Thread.Sleep(sleepTimeMs);
                }

                _uiSupport.WriteStatusLine($"Serial Port:{_serialPortName} could not be reopened after {reSeconds} seconds");
                return false;
            }
            finally
            {
                // Only clear the flag if no newer Connect/Disconnect has taken
                // over (that may already have started a reconnect of its own).
                lock (_portLock)
                {
                    if (generation == _connectionGeneration)
                        _reconnecting = false;
                }
                _hcomUiForm.ShowConnectionStateSafe();
            }
        }

        //-------------------------------------------------------------------------------
        // Returns false if the port could not be opened; never throws for a
        // missing or busy port. Status is only reported when not retrying.
        bool OpenSerialPort(string portName, bool retrying = false)
        {
            _serialPort = null;
            try
            {
                // Create a new SerialPort object with default settings.
                _serialPort = new SerialPort();
                _serialPort.PortName = portName;
                _serialPort.BaudRate = 115200;       // This value is ignored when using ACM
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;

                // Set the read/write timeouts
                _serialPort.ReadTimeout = 10000;
                _serialPort.WriteTimeout = 10000;

                _serialPort.Open();

                return true;
            }
            catch (FileNotFoundException)
            {
                // .NET reports a COM port that doesn't exist as "Could not find file"
                _serialPort = null;
                if (retrying)
                    return false;

                _uiSupport.WriteStatusLine($"Com Port '{portName}' not found. Is the Meadow plugged in?");
                return false;
            }
            catch (IOException ioe)
            {
                _serialPort = null;
                if (retrying)
                    return false;

                string msg = string.Format("The specified port '{0}' could not be found or opened.{1}Exception:'{2}'{1}",
                    portName, Environment.NewLine, ioe.Message);
                _uiSupport.WriteStatus(msg);
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // Port exists but another program (e.g. Meadow.CLI) has it open
                _serialPort = null;
                if (retrying)
                    return false;

                string msg = string.Format("The specified port '{0}' is in use by another program.{1}",
                    portName, Environment.NewLine);
                _uiSupport.WriteStatus(msg);
                return false;
            }
            catch (Exception except)
            {
                // e.g. ArgumentException for an empty or malformed port name
                _serialPort = null;
                if (retrying)
                    return false;

                string msg = string.Format("Unknown Open Serial Port exception:{0}{1}", except,
                    Environment.NewLine);
                _uiSupport.WriteStatus(msg);
                return false;
            }
        }
    }
}

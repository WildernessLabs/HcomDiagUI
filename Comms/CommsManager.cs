using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    public class CommsManager
    {
        UiSupport _uiSupport;
        HcomDiagUiForm _hcomUiForm;
        string _serialPortName;
        SerialPort _serialPort;

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

            // Create the serial port and Receiver and Sender
            _serialPort = new SerialPort();

            // Read the serial port name and open the serial port
            _serialPortName = _hcomUiForm.ReadPortName();
            bool result = OpenSerialPort(_serialPortName);
            if (result)
                _uiSupport.WriteStatusLine($"Com Port '{_serialPortName}' opened successfully");
            else
                _uiSupport.WriteStatusLine($"Com Port '{_serialPortName}' failed to open");

            // Port opened so we can proceed
            _recvSerialComms = new RecvSerialComms(this, _uiSupport);
            _sendSerialComms = new SendSerialComms(this, _uiSupport);
            _recvNormalMsgs = new NormalRcvdProc(this, _uiSupport);
            _recvUploadMsgs = new UploadProcessing(this, _uiSupport);
            _recvF7DownloadMsgs = new DnldF7Files(this, _uiSupport, _sendSerialComms);
            _recvEspDownloadMsgs = new DnldEsp32Files(this, _uiSupport);
        }

        //=======================================================================
        public void BuildHdrMsgToSend(MeadowRqstType rqstType, UInt32 userData)
        {
            _sendSerialComms.BuildAndSendHdrMessage(rqstType, userData);
        }

        //=======================================================================
        // The message classification determines what needs to be sent
        public void BuildMsgToSend(MeadowRqstData rqstInfo, UInt32 userData)
        {
            // Pull together the needed information
            switch (rqstInfo.msgClass)
            {
                case MessageClasses.Header:
                    _sendSerialComms.BuildAndSendHdrMessage(rqstInfo.rqstType, userData);
                    break;

                case MessageClasses.Text:
                    // Pick up the Text data that is in the UI
                    if (rqstInfo.rqstType == MeadowRqstType.HCOM_MDOW_REQUEST_EXEC_DIAG_APP_CMD)
                    {
                        // Special case for of text message that includes a field for the number
                        // of arguments in the text.
                        _sendSerialComms.BuildAndSendDiagCmdMsg(rqstInfo.rqstType, userData,
                            _uiSupport.TextForMsg());
                    }
                    else if (rqstInfo.rqstType == MeadowRqstType.HCOM_MDOW_REQUEST_RTC_SET_TIME_CMD)
                    {   
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
                        _sendSerialComms.BuildAndSendTextMsg(rqstInfo.rqstType, userData,
                                  timeStringUtc);

                        // Send local time (both UTC and local can be sent at the same time)
                        //var timeString = DateTimeOffset.Now.ToString("o");
                        //_sendSerialComms.BuildAndSendTextMsg(rqstInfo.rqstType, userData,
                        //          timeString);
                    }
                    else
                    {
                        // Normal case UserTextBox
                        _sendSerialComms.BuildAndSendTextMsg(rqstInfo.rqstType, userData,
                            _uiSupport.TextForMsg());
                    }
                    break;

                case MessageClasses.Binary:
                    break;

                case MessageClasses.FileInfo:
                    // Need to initiate a file download (userData is partition)
                    _recvF7DownloadMsgs.FindDownloadFileInformation(rqstInfo.rqstType,
                                userData, _uiSupport.TextForMsg(),
                                _hcomUiForm.ReadHostFileInfo());
                    break;

                case MessageClasses.NoHdrBin: // Need to Deprecate
                    break;

                default:
                    break;
            }
        }

        //=======================================================================
        // All received messages from Meadow come here to be routed for processing
        public void ForwardToProcessMsgs(HcomProtoHdrMessage hdrMsg, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, uint payloadLength)
        {
            HostRqstData hostRqstData = ProtocolHelper.GetHostRqstTypeData(hostRqstType);
            _uiSupport.WriteDebugLine($"Received '{hostRqstData.msgComplex}' message");

            // Only 3 messages are currently complex (i.e. needed 2 or more messages
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
        // Called when port name changes
        public void UpdateSerialPortName()
        {
            _serialPortName = _hcomUiForm.ReadPortName();
        }

        //-------------------------------------------------------------------------------
        public bool OpenSerialPort(string portName)
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
                _serialPort.ReadTimeout = 1000;
                _serialPort.WriteTimeout = 1000;

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

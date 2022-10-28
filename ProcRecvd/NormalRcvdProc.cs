using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    public class NormalRcvdProc
    {
        private CommsManager _commsManager;
        private UiSupport _uiSupport;
        int _hcomHdrLen;

        public NormalRcvdProc(CommsManager commsManager, UiSupport uiSupport)
        {
            this._commsManager = commsManager;
            this._uiSupport = uiSupport;

            _hcomHdrLen = Protocol.HcomProtoHdrMessageSize();
        }

        public bool Verbose { get; set; }

        //===================================================================
        internal void DisplayNormalMsgData(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            // Display header parts
            if (Verbose)
            {
                string hdrAsStr = string.Format($"Received [Header-" +
                    $"Seq:{hdrMsg.stdHeader.seqNumber} ," +
                    $"Ver:{hdrMsg.stdHeader.version}, " +
                    $"Rqst:'{hostRqstData.rqstName}' (0x{hdrMsg.stdHeader.rqstType:x4}), " +
                    $"Extr:{hdrMsg.stdHeader.extraData}, " +
                    $"UDat:{hdrMsg.stdHeader.userData}]");

                _uiSupport.WriteSendRecvTimeLine(hdrAsStr);
                _uiSupport.WriteDebugLine($"Received Msg Class '{hostRqstData.msgClass}'");
            }

            switch (hostRqstData.msgClass)
            {
                case MessageClasses.None:
                    break;

                case MessageClasses.Header:     // Only
                    DisplayHdrInfo(hdrMsg, hostRqstData, hostRqstType, messageBuffer,
                        messageLength, payloadLength);
                    break;

                case MessageClasses.FileInfo:   // Header with file information
                    DisplayFileInfo(hdrMsg, hostRqstData, hostRqstType, messageBuffer,
                        messageLength, payloadLength);
                    break;

                case MessageClasses.Text:       // Header with text string
                    DisplayTextInfo(hdrMsg, hostRqstData, hostRqstType, messageBuffer,
                        messageLength, payloadLength);
                    break;

                case MessageClasses.Binary:     // Header with binary data
                    DisplayBinInfo(hdrMsg, hostRqstData, hostRqstType, messageBuffer,
                        messageLength, payloadLength);
                    break;

                    // This is only used for files downloaded to the F7
                case MessageClasses.NoHdrBin:    // Deprecated 2-byte header + binary data
                    Debug.Assert(false);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        //--------------------------------------------------------------------
        private void DisplayTextInfo(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            // This message may or may not have text. Depends on payloadLength.
            if (payloadLength == 0)
            {
                // This is a 'text' message without text. This not a problem
                // just that it might of had text.
                return;
            }

            // Convert everything after the header from a byte[] to a .net string
            var hdrText = Encoding.ASCII.GetString(messageBuffer, _hcomHdrLen,
                    messageLength - _hcomHdrLen);

            // The text is always shown
            _uiSupport.WriteSendRecvTime("'");
            _uiSupport.WriteSendRecv(hdrText);
            _uiSupport.WriteSendRecvLine("'");

            // Display hex if desired
            if (_uiSupport.RecvHexChecked)
            {
                _uiSupport.WriteSendRecv(" (");

                // BitConverter doesn't a have anyway to specify the size so we
                // must create the right sized buffer.
                byte[] displayBuffer = new byte[payloadLength];
                Buffer.BlockCopy(messageBuffer, _hcomHdrLen, displayBuffer, 0,
                    (int)payloadLength);
                string showHex = BitConverter.ToString(displayBuffer);
                _uiSupport.WriteSendRecv(showHex);
                _uiSupport.WriteSendRecvLine(")");
            }
        }

        //--------------------------------------------------------------------
        private void DisplayHdrInfo(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType, 
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            _uiSupport.WriteSendRecvTimeLine("Standard Header received");
        }

        //--------------------------------------------------------------------
        private void DisplayFileInfo(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            _uiSupport.WriteSendRecvTimeLine("File Information received");
        }

        private void DisplayBinInfo(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            _uiSupport.WriteSendRecv(".");
        }
    }
}

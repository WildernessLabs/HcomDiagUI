using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    public class NormalRcvdProc
    {
        private static readonly object writeMsgLock = new object();

        private CommsManager _commsManager;
        private UiSupport _uiSupport;
        int _hcomHdrLen;

        public NormalRcvdProc(CommsManager commsManager, UiSupport uiSupport)
        {
            this._commsManager = commsManager;
            this._uiSupport = uiSupport;

            _hcomHdrLen = Protocol.HcomProtoHdrMessageSize();

            //Verbose = true;
        }

        public bool Verbose { get; set; }

        //===================================================================
        internal void DisplayNormalMsgData(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            try
            {
                // Display header parts
                if (Verbose)
                {
                    string hdrAsStr = string.Format($"Processing [Header-" +
                        $"Seq:{hdrMsg.stdHeader.seqNumber} ," +
                        $"Ver:{hdrMsg.stdHeader.version}, " +
                        $"Rqst:'{hostRqstData.rqstName}' (0x{hdrMsg.stdHeader.rqstType:x4}), " +
                        $"Extr:{hdrMsg.stdHeader.extraData}, " +
                        $"UDat:{hdrMsg.stdHeader.userData}]");

                    _uiSupport.WriteSendRecvTimeLine(hdrAsStr);

                    // [--]
                    _uiSupport.WriteDebugLine($"(r)Received Msg Class '{hostRqstData.msgClass}'");
                }

                switch (hostRqstData.msgClass)
                {
                    case MessageClasses.None:
                        _uiSupport.WriteSendRecvTime("MessageClass.None");
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
                    case MessageClasses.NoHdrBin:    // Should be deprecated 2-byte header + binary data
                        Debug.Assert(false);
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            catch(Exception e)
            {
                _uiSupport.WriteSendRecvTime($"An exception was caught: {e}");
                Thread.Sleep(10 * 1000);
                throw;
            }
        }

        //--------------------------------------------------------------------
        // Just display the text
        private void DisplayTextInfo(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            // This message may or may not have text. Depends on payloadLength.
            if (payloadLength == 0)
            {
                // This is a 'text' message without text. This not a problem
                // just that could of had text.
                _uiSupport.WriteSendRecvTimeLine("(r) Msg Text:'<No Text>'");
                return;
            }

            // Convert everything after the header from a byte[] to a .net string
            var hdrText = Encoding.ASCII.GetString(messageBuffer, _hcomHdrLen,
                    messageLength - _hcomHdrLen);

            _uiSupport.WriteSendRecvTime("(r) Msg Text:<");
            _uiSupport.WriteSendRecvLine($"{hdrText}>");

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
        // Display header info
        private void DisplayHdrInfo(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType, 
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            string hdrAsStr = string.Format($"Processing [Header-" +
                $"Seq:{hdrMsg.stdHeader.seqNumber} ," +
                $"Ver:{hdrMsg.stdHeader.version}, " +
                $"Rqst:'{hostRqstData.rqstName}' (0x{hdrMsg.stdHeader.rqstType:x4}), " +
                $"Extr:{hdrMsg.stdHeader.extraData}, " +
                $"UDat:{hdrMsg.stdHeader.userData}]");

            _uiSupport.WriteSendRecvTimeLine(hdrAsStr);
        }

        //--------------------------------------------------------------------
        private void DisplayFileInfo(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            _uiSupport.WriteSendRecvTimeLine("File Information received");
        }

        //--------------------------------------------------------------------
        private void DisplayBinInfo(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            _uiSupport.WriteSendRecv(".");
        }
    }
}

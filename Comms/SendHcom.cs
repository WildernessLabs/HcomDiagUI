using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;

namespace HcomDiagUi
{
    public class SendSerialComms
    {
        CommsManager _commsMgr;
        UiSupport _uiSupport;
        int _protocolHdrSize;

        public bool Verbose { get; set; }

        //==========================================================================
        // Constructor
        public SendSerialComms(CommsManager commsMgr, UiSupport uiSupport)
        {
		    _uiSupport = uiSupport;
            _commsMgr = commsMgr;

            // Save this often used value
            _protocolHdrSize = Marshal.SizeOf(typeof(HcomProtoHdrMessage));
        }

        //==========================================================================
        // This is used for starting a file download.
        internal void BuildAndSendFileRelatedCommand(MeadowRqstType requestType,
            UInt32 userData, UInt32 fileSize, UInt32 fileCheckSum,
            UInt32 mcuAddr, string md5Hash, string destFileName)
        {
            _uiSupport.WriteDebugLine($"Entered BuildAndSendFileRelatedCommand with fileSize {fileSize}, CRC {fileCheckSum}");

            var hcomFsStartCmdMsg = new HcomProtoFSInfoMsg
            {
                seqNumber = 0,
                version = Protocol.HCOM_PROTOCOL_HCOM_VERSION_NUMBER,
                rqstType = (UInt16)requestType,
                extraData = 0,
                userData = userData,

                // Non-standard header information follows
                fileSize = fileSize,
                fileCheckSum = fileCheckSum,
                esp32FlashAddr = mcuAddr,
                esp32MD5Hash = new byte[32]
            };

            // Encode the md5 hash into the structure
            Encoding.ASCII.GetBytes(md5Hash, 0, md5Hash.Length, hcomFsStartCmdMsg.esp32MD5Hash, 0);

            // Determine the needed spaces lengths
            var fsInfoHdrSize = 56;

            var encodedFileNameLen = Encoding.ASCII.GetByteCount(destFileName);
            var totalMsgLen = fsInfoHdrSize + encodedFileNameLen;

            // Determine the length of the entire message
            byte[] msgBuffer = new byte[totalMsgLen];

            // Copy the class to a buffer
            IntPtr tempPtr = Marshal.AllocHGlobal(fsInfoHdrSize);
            Marshal.StructureToPtr(hcomFsStartCmdMsg, tempPtr, true);
            Marshal.Copy(tempPtr, msgBuffer, 0, fsInfoHdrSize);
            Marshal.FreeHGlobal(tempPtr);

            // Encode and copy the filename to the end of the msg buffer
            _ = Encoding.ASCII.GetBytes(destFileName, 0, encodedFileNameLen,
                msgBuffer, fsInfoHdrSize);

            EncodeAndSendPacket(msgBuffer, totalMsgLen);
        }

        //==========================================================================
        // Build and send a message with a header plus text
        internal void BuildAndSendTextMsg(MeadowRqstType requestType,
            UInt32 userData, string textData)
        {
            var hcomHdrMsg = new HcomProtoHdrMessage();
            hcomHdrMsg.stdHeader.seqNumber = 0;
            hcomHdrMsg.stdHeader.version = Protocol.HCOM_PROTOCOL_HCOM_VERSION_NUMBER;
            hcomHdrMsg.stdHeader.rqstType = (UInt16)requestType;
            hcomHdrMsg.stdHeader.extraData = 0;
            hcomHdrMsg.stdHeader.userData = userData;

            // Determine the needed buffer length
            int encodedLength = Encoding.ASCII.GetByteCount(textData);

            // Reserve space for the text string
            int totalSize = _protocolHdrSize + encodedLength;
            byte[] msgBuffer = new byte[totalSize];

            // Copy the header struct to buffer which cannot be done except like this
            IntPtr tempPtr = Marshal.AllocHGlobal(totalSize);
            Marshal.StructureToPtr(hcomHdrMsg, tempPtr, true);
            Marshal.Copy(tempPtr, msgBuffer, 0, totalSize);
            Marshal.FreeHGlobal(tempPtr);

            // Encode and copy the text to the buffer
            _ = Encoding.ASCII.GetBytes(textData, 0, textData.Length, msgBuffer, _protocolHdrSize);

            EncodeAndSendPacket(msgBuffer, totalSize);
        }

        //==========================================================================
        // Prepare a packet containing file data being downloaded
        public void BuildAndSendBinPacket(byte[] binDataBytes, int binDataOffset,
            int binDataSize, UInt16 seqNumb)
        {
            try
            {
                // Need to consider the sequence number too
                byte[] seqBytes = BitConverter.GetBytes(seqNumb);

                // Determine the needed buffer length
                int totalSize = binDataSize + seqBytes.Length;
                byte[] msgBuffer = new byte[totalSize];

                // Copy sequence number and data into the xmitBuffer
                Array.Copy(seqBytes, msgBuffer, seqBytes.Length);
                Array.Copy(binDataBytes, binDataOffset, msgBuffer, seqBytes.Length, binDataSize);

                //_uiSupport.WriteDebugLine($"seq {seqNumb}, len {seqBytes.Length}, totSize {totalSize}, msgSz {binDataSize}, msgLen {binDataBytes.Length}");

                EncodeAndSendPacket(msgBuffer, totalSize);
            }
            catch (Exception except)
            {
                Console.WriteLine($"An exception was caught: {except}");
                Thread.Sleep(10 * 60 * 1000);   // Sleep for 10 minutes
                throw;
            }
        }

        //==========================================================================
        // Build and send a "simple" message with only a header
        internal void BuildAndSendHdrMessage(MeadowRqstType requestType, UInt32 userData)
        {
            var hdrMsg = new HcomProtoHdrMessage();
            hdrMsg.stdHeader.seqNumber = 0;
            hdrMsg.stdHeader.version = Protocol.HCOM_PROTOCOL_HCOM_VERSION_NUMBER;
            hdrMsg.stdHeader.rqstType = (UInt16) requestType;
            hdrMsg.stdHeader.extraData = 0;
            hdrMsg.stdHeader.userData = userData;

            MeadowRqstData rqstData;
            rqstData = ProtocolHelper.DecodeMsgToMeadow(requestType);
            
            if (Verbose)
            {
                string xmitHeader = string.Format($"Sending  [Header-" +
                    $"Seq:{hdrMsg.stdHeader.seqNumber} ," +
                    $"Ver:{hdrMsg.stdHeader.version}, " +
                    $"Rqst:{rqstData.rqstName} (0x{hdrMsg.stdHeader.rqstType:x4}), " +
                    $"Extr:{hdrMsg.stdHeader.extraData}, " +
                    $"UDat:{hdrMsg.stdHeader.userData}]");

                _uiSupport.WriteSendReceiveTimeLine(xmitHeader);
            }

            byte[] msgBuffer = new byte[_protocolHdrSize];

            // Copy message to buffer for sending
            IntPtr tempBuf = Marshal.AllocHGlobal(_protocolHdrSize);
            Marshal.StructureToPtr(hdrMsg, tempBuf, true);
            Marshal.Copy(tempBuf, msgBuffer, 0, _protocolHdrSize);
            Marshal.FreeHGlobal(tempBuf);

            EncodeAndSendPacket(msgBuffer, _protocolHdrSize);
        }

        //==========================================================================
        // Build and send a message with a header, text length plus text
        internal void BuildAndSendDiagCmdMsg(MeadowRqstType requestType,
            UInt32 userData, string textData)
        {
            // Copy the encoded string to the buffer
            var hcomDiagCmdMsg = new HcomProtoDiagCmdMsg();

            var protocolDiagCmdSize = Marshal.SizeOf(typeof(HcomProtoDiagCmdMsg));

            hcomDiagCmdMsg.seqNumber = 0;
            hcomDiagCmdMsg.version = Protocol.HCOM_PROTOCOL_HCOM_VERSION_NUMBER;
            hcomDiagCmdMsg.rqstType = (UInt16)requestType;
            hcomDiagCmdMsg.extraData = 0;
            hcomDiagCmdMsg.userData = userData;

            // Determine the needed buffer for text length
            UInt16 encodedLength = (UInt16)Encoding.ASCII.GetByteCount(textData);
            hcomDiagCmdMsg.argListLen = encodedLength;    // This is the only difference

            // Reserve space for the string
            int totalSize = protocolDiagCmdSize + encodedLength;
            byte[] msgBuffer = new byte[totalSize];

            // Copy the structure to message buffer
            IntPtr structPtr = Marshal.AllocHGlobal(protocolDiagCmdSize);

            // Marshal data from managed structure to unmanaged memory
            Marshal.StructureToPtr(hcomDiagCmdMsg, structPtr, true);

            // Copy structure (in unmanaged memory) to the start of managed buffer
            Marshal.Copy(structPtr, msgBuffer, 0, protocolDiagCmdSize);
            Marshal.FreeHGlobal(structPtr);

            // Encode and put the text at the end of the message buffer, after
            // the structure
            _ = Encoding.ASCII.GetBytes(textData, 0, textData.Length, msgBuffer,
                        protocolDiagCmdSize);

            EncodeAndSendPacket(msgBuffer, totalSize);
        }

        //==========================================================================
        // Build and send a message with a header and binary data
        // Added for Visual Studio Debugging but used by others
        internal void BuildAndSendBinaryMsg(byte[] additionalData, MeadowRqstType requestType, UInt32 userData)
        {
            //int totalMsgLength = additionalData.Length + HCOM_PROTOCOL_COMMAND_REQUIRED_HEADER_LENGTH;
            //var messageBytes = new byte[totalMsgLength];

            //// Populate the header
            //BuildMeadowBoundSimpleCommand(requestType, userData, ref messageBytes);

            //// Copy the payload into the message
            //Array.Copy(additionalData, 0, messageBytes,
            //    HCOM_PROTOCOL_COMMAND_REQUIRED_HEADER_LENGTH, additionalData.Length);

            //EncodeAndSendPacket(messageBytes, 0, totalMsgLength);
        }

        //==========================================================================
        // Last stop before transmitting information
        // Note: Ultimately this format should be eleminated in favor of a standard header
        private void EncodeAndSendPacket(byte[] messageBytes, int messageSize, int messageOffset = 0)
        {
            try
            {
                int encodedToSend;
                byte[] encodedBytes;

                //Console.WriteLine($"Sending packet with {messageSize} bytes");

                // For testing calculate the crc including the sequence number
                //_packetCrc32 = NuttxCrc.Crc32part(messageBytes, messageSize, 0, _packetCrc32);
                try
                {
                    // The encoded size using COBS is just a bit more than the original size adding 1 byte
                    // every 254 bytes plus 1 and need room for beginning and ending delimiters.
                    encodedBytes = new byte[Protocol.HCOM_PROTOCOL_ENCODED_MAX_SIZE];

                    // Skip over first byte so it can be a start delimiter
                    encodedToSend = CobsTools.CobsEncoding(messageBytes, messageOffset, messageSize,
                        ref encodedBytes, 1);

                    // DEBUG TESTING
                    if(encodedToSend == -1)
                    {
                        _uiSupport.WriteDebugLine($"Error - encodedToSend == -1");
                        Thread.Sleep(500);    // Place for break point
                        return;
                    }

                    if (_commsMgr.SerialPort == null)
                    {
                        _uiSupport.WriteDebugLine($"Error - SerialPort == null");
                        Thread.Sleep(500);    // Place for break point
                        throw new NotConnectedException();
                    }
                }
                catch (Exception except)
                {
                    string msg = string.Format("Send setup Exception: {0}", except);
                    _uiSupport.WriteStatus(msg);
                    Thread.Sleep(500);    // Place for break point
                    throw;
                }

                // Add delimiters to packet boundaries
                try
                {
                    encodedBytes[0] = 0;                // Start delimiter
                    encodedToSend++;
                    encodedBytes[encodedToSend] = 0;    // End delimiter
                    encodedToSend++;
                }
                catch (Exception encodedBytesEx)
                {
                    // This should drop the connection and retry
                    Debug.WriteLine($"Adding encodeBytes delimiter threw: {encodedBytesEx}");
                    Thread.Sleep(500);    // Place for break point
                    throw;
                }

                try
                {
                    // Send the data to Meadow
                    _commsMgr.SerialPort.Write(encodedBytes, 0, encodedToSend);
                }
                catch (InvalidOperationException ioe)  // Port not opened
                {
                    string msg = string.Format("Write but port not opened. Exception: {0}", ioe);
                    _uiSupport.WriteStatus(msg);
                    Thread.Sleep(500);    // Place for break point
                    throw;
                }
                catch (ArgumentOutOfRangeException aore)  // offset or count don't match buffer
                {
                    string msg = string.Format("Write buffer, offset and count don't line up. Exception: {0}", aore);
                    _uiSupport.WriteStatus(msg);
                    Thread.Sleep(500);    // Place for break point
                    throw;
                }
                catch (ArgumentException ae)  // offset plus count > buffer length
                {
                    string msg = string.Format($"Write offset plus count > buffer length. Exception: {0}", ae);
                    _uiSupport.WriteStatus(msg);
                    Thread.Sleep(500);    // Place for break point
                    throw;
                }
                catch (TimeoutException te) // Took too long to send
                {
                    string msg = string.Format("Write took too long to send. Exception: {0}", te);
                    _uiSupport.WriteStatus(msg);
                    Thread.Sleep(500);    // Place for break point
                    throw;
                }
            }
            catch (Exception except)
            {
                // This should drop the connection and retry
                Debug.WriteLine($"EncodeAndSendPacket threw: {except}");
                Thread.Sleep(500);    // Place for break point
                throw;
            }
        }
    }

    public class NotConnectedException : Exception { }
}

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
        UInt32 _packetCrc32;
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
        // Build and send the Start, Data packets and the End
        public int SendTheFile(SerialPort meadow, MeadowRqstType requestType,
            string destFileName, uint partitionId, byte[] fileBytes, UInt32 mcuAddr, UInt32 payloadCrc32,
            string md5Hash, bool lastInSeries)
        {
            _packetCrc32 = 0;

            try
            {
                Console.WriteLine($"==> {DateTime.Now:HH:mm:ss.fff}-Sending START to Meadow");    // TESTING
                
                // // Build and send the header
                // BuildAndSendFileRelatedCommand(requestType,
                //    partitionId, (UInt32)fileBytes.Length, payloadCrc32,
                //    mcuAddr, md5Hash, destFileName);

                // // Wait for indication of success or failure for the start request before
                // // proceeding to send data.
                // var result = await MeadowDeviceManager.WaitForResponseMessage(meadow, p =>
                //     (p.MessageType == MeadowMessageType.Concluded ||
                //     p.MessageType == MeadowMessageType.DownloadStartOkay ||
                //     p.MessageType == MeadowMessageType.DownloadStartFail),
                //     4000);
                
                // var result = await MeadowDeviceManager.WaitForResponseMessage(meadow, p =>
                //         (p.MessageType == MeadowMessageType.DownloadStartOkay));
                
                // Build and send the file as a group of data packets
                int fileBufOffset = 0;
                //int numbToSend;
                UInt16 sequenceNumber = 1;

                // don't echo the device responses
                //WriteProgress(-1);
                //    WriteProgress(progress);
                //    fileBufOffset += numbToSend;
                //    sequenceNumber++;
                //WriteProgress(101);

                // echo the device responses
                Thread.Sleep(250); // if we're too fast, we'll finish and the device will still echo a little

                //--------------------------------------------------------------
                // Build and send the correct File End request
                switch (requestType)
                {
                    // Provide the correct message end depending on the reason the file
                    // is being downloaded to the F7 file system.
                    case MeadowRqstType.HCOM_MDOW_REQUEST_START_FILE_TRANSFER:
                        BuildAndSendHdrMessage(MeadowRqstType.HCOM_MDOW_REQUEST_END_FILE_TRANSFER,
                            lastInSeries ? (uint)1 : (uint)0);      // set UserData
                        break;
                    case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_UPDATE_RUNTIME:
                        BuildAndSendHdrMessage(MeadowRqstType.HCOM_MDOW_REQUEST_MONO_UPDATE_FILE_END,
                            lastInSeries ? (uint)1 : (uint)0);      // set UserData
                        break;
                    case MeadowRqstType.HCOM_MDOW_REQUEST_START_ESP_FILE_TRANSFER:
                        BuildAndSendHdrMessage(MeadowRqstType.HCOM_MDOW_REQUEST_END_ESP_FILE_TRANSFER,
                            lastInSeries ? (uint)1 : (uint)0);      // set UserData
                        break;
                    default:
                        {
                            string msg = string.Format($"File end Meadow request type of {requestType} not defined");
                            _uiSupport.WriteStatus(msg);
                        }
                        break;
                }

                // bufferOffset should point to the byte after the last byte
                Debug.Assert(fileBufOffset == fileBytes.Length);
                if (Verbose)
                {
                    string msg = string.Format($"Total bytes sent {fileBufOffset:N0} in {sequenceNumber:N0} packets. PacketCRC:{_packetCrc32:x08}");
                    _uiSupport.WriteSendReceiveLine(msg);
                }
            }
            catch (Exception except)
            {
                string msg = string.Format("{DateTime.Now:HH:mm:ss.fff}-Exception sending to Meadow:{0}", except);
                _uiSupport.WriteStatus(msg);
                throw;
            }
            return 0;
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
            
            string xmitHeader = string.Format($"Sending  [Header-" +
                $"Seq:{hdrMsg.stdHeader.seqNumber} ," +
                $"Ver:{hdrMsg.stdHeader.version}, " +
                $"Rqst:{rqstData.rqstName} (0x{hdrMsg.stdHeader.rqstType:x4}), " +
                $"Extr:{hdrMsg.stdHeader.extraData}, " +
                $"UDat:{hdrMsg.stdHeader.userData}]");

            if (Verbose)
            {
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
        // Build and send a message with a header plus text
        internal void BuildAndSendTextMsgOld(MeadowRqstType requestType,
            UInt32 userData, string textData)
        {
            // Copy the encoded string to the buffer
            var hcomHdrMsg = new HcomProtoHdrMessage();
            hcomHdrMsg.stdHeader.seqNumber = 0;
            hcomHdrMsg.stdHeader.version = Protocol.HCOM_PROTOCOL_HCOM_VERSION_NUMBER;
            hcomHdrMsg.stdHeader.rqstType = (UInt16)requestType;
            hcomHdrMsg.stdHeader.extraData = 0;
            hcomHdrMsg.stdHeader.userData = userData;

            // Determine the needed buffer length
            int encodedLength = Encoding.ASCII.GetByteCount(textData);
            // Reserve space for the string
            int totalSize = _protocolHdrSize + encodedLength;
            byte[] msgBuffer = new byte[totalSize];

            // Copy the header to buffer
            IntPtr tempPtr = Marshal.AllocHGlobal(totalSize);
            Marshal.StructureToPtr(hcomHdrMsg, tempPtr, true);
            Marshal.Copy(tempPtr, msgBuffer, 0, totalSize);
            Marshal.FreeHGlobal(tempPtr);

            // Encode and copy the text to the buffer
            _ = Encoding.ASCII.GetBytes(textData, 0, textData.Length, msgBuffer, _protocolHdrSize);

            EncodeAndSendPacket(msgBuffer, totalSize);
        }

        //==========================================================================
        // Build and send a message with a header plus text. The text is the current
        // date time encoded in the ISO 8601 format.
        internal void BuildAndSendRTCTimeCmdMsg(MeadowRqstType requestType)
        {
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
            var timeString = DateTimeOffset.UtcNow.ToString("o");
            Console.WriteLine($"UTC Time string {timeString}");

            var hcomSendTimeMsg = new HcomProtoTextMessage();
            var protocolMsgSize = Marshal.SizeOf(typeof(HcomProtoTextMessage));

            // Populate header
            hcomSendTimeMsg.seqNumber = 0;
            hcomSendTimeMsg.version = Protocol.HCOM_PROTOCOL_HCOM_VERSION_NUMBER;
            hcomSendTimeMsg.rqstType = (UInt16)requestType;
            hcomSendTimeMsg.extraData = 0;
            hcomSendTimeMsg.userData = 0;

            // Reserve space for time string
            UInt16 encodedLength = (UInt16)Encoding.ASCII.GetByteCount(timeString);
            int totalSize = protocolMsgSize + encodedLength;
            byte[] msgBuffer = new byte[totalSize];

            // Allocate unmanaged memory to copy the structure to message buffer
            IntPtr structPtr = Marshal.AllocHGlobal(protocolMsgSize);

            // Marshal data from managed structure to unmanaged memory
            Marshal.StructureToPtr(hcomSendTimeMsg, structPtr, true);

            // Copy header (in unmanaged memory) to the start of the buffer
            Marshal.Copy(structPtr, msgBuffer, 0, protocolMsgSize);
            Marshal.FreeHGlobal(structPtr);

            // Encode and put the text at the end of the message buffer, immediately
            // after the header structure
            _ = Encoding.ASCII.GetBytes(timeString, 0, timeString.Length, msgBuffer,
                protocolMsgSize);

            EncodeAndSendPacket(msgBuffer, totalSize);
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
            hcomDiagCmdMsg.argListLen = encodedLength;

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
        // Prepare a packet containing a file data being downloaded
        // NOTE: THIS NO HEADER STYLE OF MESSAGE SHOULD BE REPLACED WITH A MESSAGE
        // STARTING WITH A STANDARD HEADER
        private void BuildAndSendDataPacketRequest(byte[] messageBytes, int messageOffset,
            int messageSize, UInt16 seqNumb)
        {
            //try
            //{
            //    // Need to prepend the sequence number to the packet
            //    int xmitSize = messageSize + sizeof(UInt16);
            //    byte[] fullMsg = new byte[xmitSize];

            //    byte[] seqBytes = BitConverter.GetBytes(seqNumb);
            //    Array.Copy(seqBytes, fullMsg, sizeof(UInt16));
            //    Array.Copy(messageBytes, messageOffset, fullMsg, sizeof(UInt16), messageSize);

            //    EncodeAndSendPacket(fullMsg, 0, xmitSize);
            //}
            //catch (Exception except)
            //{
            //    Console.WriteLine($"An exception was caught: {except}");
            //    throw;
            //}
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
        // This is used for starting a file download.
        internal void BuildAndSendFileRelatedCommand(MeadowRqstType requestType,
            UInt32 userData, UInt32 fileSize, UInt32 fileCheckSum, UInt32 mcuAddr,
            string md5Hash, string destFileName)
        {
            // Future: Try to use the StructLayout attribute
            //Debug.Assert(md5Hash.Length == 0 || md5Hash.Length == HCOM_PROTOCOL_REQUEST_MD5_HASH_LENGTH);

            //// Allocate the correctly size message buffers
            //byte[] targetFileName = Encoding.ASCII.GetBytes(destFileName);
            //byte[] md5HashBytes = Encoding.ASCII.GetBytes(md5Hash);
            //int optionalDataLength = sizeof(UInt32) + sizeof(UInt32) + sizeof(UInt32) +
            //    HCOM_PROTOCOL_REQUEST_MD5_HASH_LENGTH + targetFileName.Length;
            //byte[] messageBytes = new byte[HCOM_PROTOCOL_COMMAND_REQUIRED_HEADER_LENGTH + optionalDataLength];

            //// Add the required header
            //int offset = BuildMeadowBoundSimpleCommand(requestType, userData, ref messageBytes);

            //// File Size
            //Array.Copy(BitConverter.GetBytes(fileSize), 0, messageBytes, offset, sizeof(UInt32));
            //offset += sizeof(UInt32);

            //// CRC32 checksum or delete file partition number
            //Array.Copy(BitConverter.GetBytes(fileCheckSum), 0, messageBytes, offset, sizeof(UInt32));
            //offset += sizeof(UInt32);

            //// MCU address for this file. Used for ESP32 file downloads
            //Array.Copy(BitConverter.GetBytes(mcuAddr), 0, messageBytes, offset, sizeof(UInt32));
            //offset += sizeof(UInt32);

            //// Include ESP32 MD5 hash if it's needed
            //if (string.IsNullOrEmpty(md5Hash))
            //    Array.Clear(messageBytes, offset, HCOM_PROTOCOL_REQUEST_MD5_HASH_LENGTH);
            //else
            //    Array.Copy(md5HashBytes, 0, messageBytes, offset, HCOM_PROTOCOL_REQUEST_MD5_HASH_LENGTH);
            //offset += HCOM_PROTOCOL_REQUEST_MD5_HASH_LENGTH;

            //// Destination File Name
            //Array.Copy(targetFileName, 0, messageBytes, offset, targetFileName.Length);
            //offset += targetFileName.Length;

            //Debug.Assert(offset == optionalDataLength + HCOM_PROTOCOL_COMMAND_REQUIRED_HEADER_LENGTH);
            //EncodeAndSendPacket(messageBytes, 0, offset);
        }

        //==========================================================================
        // Last stop before transmitting information
        private void EncodeAndSendPacket(byte[] messageBytes, int messageSize, int messageOffset = 0)
        {
            try
            {
                Console.WriteLine($"Sending packet with {messageSize} bytes");

                // For testing calculate the crc including the sequence number
                //_packetCrc32 = NuttxCrc.Crc32part(messageBytes, messageSize, 0, _packetCrc32);

                // Add 2, first to account for start delimiter and second for end
                byte[] encodedBytes = new byte[Protocol.HCOM_PROTOCOL_PACKET_MAX_SIZE + 2];
                // Skip first byte so it can be a start delimiter
                int encodedToSend = CobsTools.CobsEncoding(messageBytes, messageOffset, messageSize, ref encodedBytes, 1);

                // Terminate packet with delimiter so packet boundaries can be more easily found
                encodedBytes[0] = 0;                // Start delimiter
                encodedToSend++;
                encodedBytes[encodedToSend] = 0;    // End delimiter
                encodedToSend++;

                try
                {
                    if (_commsMgr.SerialPort == null)
                        throw new NotConnectedException();

                    _commsMgr.SerialPort.Write(encodedBytes, 0, encodedToSend);
                }
                catch (InvalidOperationException ioe)  // Port not opened
                {
                    string msg = string.Format("Write but port not opened. Exception: {0}", ioe);
                    _uiSupport.WriteStatus(msg);
                    throw;
                }
                catch (ArgumentOutOfRangeException aore)  // offset or count don't match buffer
                {
                    string msg = string.Format("Write buffer, offset and count don't line up. Exception: {0}", aore);
                    _uiSupport.WriteStatus(msg);
                    throw;
                }
                catch (ArgumentException ae)  // offset plus count > buffer length
                {
                    string msg = string.Format($"Write offset plus count > buffer length. Exception: {0}", ae);
                    _uiSupport.WriteStatus(msg);
                    throw;
                }
                catch (TimeoutException te) // Took too long to send
                {
                    string msg = string.Format("Write took too long to send. Exception: {0}", te);
                    _uiSupport.WriteStatus(msg);
                    throw;
                }
            }
            catch (Exception except)
            {
                Debug.WriteLine($"EncodeAndSendPacket threw: {except}");
                throw;
            }
        }
    }

    public class NotConnectedException : Exception { }
}

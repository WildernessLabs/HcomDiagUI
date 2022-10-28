using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace HcomDiagUi
{
    public class DnldF7Files
    {
        private CommsManager commsManager;
        private UiSupport uiSupport;
        private SendSerialComms sendSerialComms;
        private UInt32 _packetCrc32;

        public DnldF7Files(CommsManager commsManager, UiSupport uiSupport, SendSerialComms sendSerialComms)
        {
            this.commsManager = commsManager;
            this.sendSerialComms = sendSerialComms;
            this.uiSupport = uiSupport;
        }

        // This is start of file download
        public void FindDownloadFileInformation(MeadowRqstType requestType,
                UInt32 partition, string meadowFileName,
                string sourceFileName)
        {
            // Make sure file exists on this PC
            if (!File.Exists(sourceFileName))
            {
                string msgFail = string.Format($"Please enter a valid file name {sourceFileName} doesn't exist.");
                uiSupport.WriteStatus(msgFail);
                return;
            }

            commsManager.ClearCancelSending();

            // Read the entire file for size and CRC32 checksum
            byte[] dataBuff = File.ReadAllBytes(sourceFileName);

            UInt32 fileSize = (UInt32)dataBuff.Length;
            _packetCrc32 = NuttxCrc.Crc32part(dataBuff, (uint)dataBuff.Length, 0);

            // Call to send the initial header
            UInt32 espAddress = 0;
            string md5Hash = "12345678901234567890123456789012";        // Must be 32 bytes

            // This is the start download message. Meadow will respond with a go/no go reply
            sendSerialComms.BuildAndSendFileRelatedCommand(requestType, partition,
                fileSize, _packetCrc32, espAddress, md5Hash, meadowFileName);
        }

        // Wait for HCOM to respond with HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY or
        // HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL message.

        // Execute by the Receive Thread that pulls message from the circular queue
        public void ProcessRcvdF7Dnld(HcomProtoHdrMessage hdrMsg,
                HostRqstData hostRqstData, HostRqstType hostRqstType,
                byte[] messageBuffer, int messageLength, uint payloadLength)
        {
            // Received a message while doing file download. This must be the
            // start result. Do we stop or continue?
            switch (hostRqstType)
            {
                case HostRqstType.HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY:
                    uiSupport.WriteSendRecvTimeLine($"Request type {hostRqstType} Indicates dnld start success");

                    // We must allow the thread that pulls messages from the circular
                    // queue to return so it can get message to display, so we'll use
                    // a thread pool thread.
                    // Start sending file data to the Meadow using a thread pool thread
                    if (!ThreadPool.QueueUserWorkItem(SendFilePackets))
                    {
                        // Grabing a thread from the thread pool failed
                        throw new Exception("ThreadPool.QueueUserWorkItem(SendFilePackets)");
                    }
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL:
                    uiSupport.WriteSendRecvTimeLine($"Request type {hostRqstType} Indicates dnld start failure");
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_DNLD_FAIL_RESEND:
                    var headerLen = Protocol.HcomProtoHdrMessageSize();

                    // Convert everything after the header from a byte[] to a .net string
                    var hdrText = Encoding.ASCII.GetString(messageBuffer, headerLen,
                            messageLength - headerLen);
                    uiSupport.WriteSendRecvTimeLine($"Request type {hostRqstType} '{hdrText}'");
                    break;

                default:
                    break;
            }
        }

        // Reads the file data and sends it to Meadow, one packet at a time, using the
        // thread pool thread.
        public void SendFilePackets(Object state)
        {
            UInt16 seqNumb = 0;

            uiSupport.WriteDebugLine("Entered ReadPrepareFilePackets(), sending data");

            // Open the specified file by name and read the data
            string hostFileInfo = uiSupport.GetHostFileInfo();

            using (FileStream fs = File.OpenRead(hostFileInfo))
            {
                // The maximum data bytes is max packet size - 2 bytes for the sequence number
                byte[] dataBuff = new byte[Protocol.HCOM_PROTOCOL_PACKET_MAX_SIZE - 2];
                int bytesRead;
                do
                {
                    if(commsManager.IsCancelSendingSet())
                        return;

                    seqNumb++;
                    bytesRead = fs.Read(dataBuff, 0, dataBuff.Length);
                    sendSerialComms.BuildAndSendBinPacket(dataBuff, 0, bytesRead, seqNumb);
                }
                while (bytesRead > 0);
            }

            uiSupport.WriteDebugLine($"Sending End msg (sent {seqNumb} data packets");

            // Send the End message
            sendSerialComms.BuildAndSendHdrMessage(MeadowRqstType.HCOM_MDOW_REQUEST_END_FILE_TRANSFER, 0);
        }
    }
}

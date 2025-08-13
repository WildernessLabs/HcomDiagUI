using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace HcomDiagUi
{
    public class DnldF7Files
    {
        private CommsManager _commsMgr;
        private UiSupport uiSupport;
        private SendSerialComms sendSerialComms;
        private UInt32 _packetCrc32;
        private bool _isFileDnldActive;

        public DnldF7Files(CommsManager commsManager, UiSupport uiSupport, SendSerialComms sendSerialComms)
        {
            this._commsMgr = commsManager;
            this.sendSerialComms = sendSerialComms;
            this.uiSupport = uiSupport;
            _isFileDnldActive = false;
        }

        public bool IsFileDnldActive {
            get { return _isFileDnldActive; }
            set { _isFileDnldActive = value; } }

        //============================================================================
        // 1. This is start of F7 file download. We tell Meadow about the file
        public void DnldStartFileInfo(MeadowRqstType requestType,
                UInt32 userData, string meadowFileName,
                string sourceFileName)
        {
            // Make sure file to send exists on this PC
            if (!File.Exists(sourceFileName))
            {
                string msgFail = string.Format($"Please enter a valid sending file name. {sourceFileName} not found.");
                uiSupport.WriteStatus(msgFail);
                return;
            }

            _commsMgr.ClearCancelSending();

            // Read the entire file for size and CRC32 checksum
            byte[] dataBuff = File.ReadAllBytes(sourceFileName);

            UInt32 fileSize = (UInt32)dataBuff.Length;
            _packetCrc32 = NuttxCrc.Crc32part(dataBuff, (uint)dataBuff.Length, 0);

            // Call to send the initial header
            UInt32 espAddress = 0;
            string md5Hash = "12345678901234567890123456789012";    // Must be 32 bytes

            //uiSupport.WriteSendRecvTimeLine($"[{Thread.CurrentThread.ManagedThreadId}]Step #1 Send 'Start' msg to Meadow");

            // This is the start of a download. Meadow will respond with a go/no go reply
            sendSerialComms.BuildAndSendF7FileDnldCommand(requestType, userData,
                0, fileSize, _packetCrc32, espAddress, md5Hash, meadowFileName);
        }

        //============================================================================
        // 2. Wait for HCOM to respond with HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY
        // or HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL message.
        // Execute by the Receive Thread that pulls message from the circular queue
        public void DnldChkF7ReadyMsg(HcomProtoHdrMessage hdrMsg,
                HostRqstData hostRqstData, HostRqstType hostRqstType,
                byte[] messageBuffer, int messageLength, uint payloadLength)
        {
            uiSupport.WriteSendRecvTimeLine($"[{Thread.CurrentThread.ManagedThreadId}](s) Step #2 Recvd Meadow response");

            // Received a message while in file download mode. This must be the
            // start result. Do we stop or continue?
            switch (hostRqstType)
            {
                case HostRqstType.HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY:
                    uiSupport.WriteSendRecvTimeLine($"(r) Meadow msg indicating HCOM is ready for data");

                    lock (this)
                    {
                        if (_isFileDnldActive)
                            return;     // All ready active
                        else
                            _isFileDnldActive = true;
                    }

                    // We must allow the thread that pulls messages from the circular
                    // queue to return so it can get grab more messages to display, so we'll use
                    // a thread pool thread.
                    // Start sending file data to the Meadow using a thread pool thread
                    if (!ThreadPool.QueueUserWorkItem(DnldF7DataPackets))
                    {
                        // Getting a thread from the thread pool failed
                        _isFileDnldActive = false;
                        throw new Exception("ThreadPool.QueueUserWorkItem(DnldF7DataPackets)");
                    }
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL:
                    _isFileDnldActive = false;
                    uiSupport.WriteSendRecvTimeLine($"(r)FAILURE-HCOM couldn't get files system ready");
                    break;

                // This message indicates that during data download the Meadow watchdog
                // timer timed out, because no data was received for x seconds.
                case HostRqstType.HCOM_HOST_REQUEST_DNLD_FAIL_RESEND:
                    _isFileDnldActive = false;
                    var headerLen = Protocol.HcomProtoHdrMessageSize();

                    // Convert everything after the header from a byte[] to a .net string
                    var hdrText = Encoding.ASCII.GetString(messageBuffer, headerLen,
                            messageLength - headerLen);
                    uiSupport.WriteSendRecvTimeLine($"(r)FAILURE-Request type {hostRqstType} HCOM reported:{hdrText}");
                    break;

                default:
                    // How did this request type get here?
                    Debug.Assert(false);
                    break;
            }
        }

        //============================================================================
        // 3. Read the file data and sends it to Meadow, one packet at a time, using
        // the thread pool thread. Each download of file data gets a new thread.
        public void DnldF7DataPackets(Object state)
        {
            UInt16 seqNumb = 1;      // The first data packet needs to be 1

            //uiSupport.WriteSendRecvTimeLine($"[{Thread.CurrentThread.ManagedThreadId}]==>Start sending file data");

            // Open the specified file by name and read the data
            string hostFileInfo = uiSupport.GetHostFileInfo();
            using (FileStream fs = File.OpenRead(hostFileInfo))
            {
                // The maximum data bytes is max packet size - 2 bytes for the sequence number
                byte[] dataBuff = new byte[Protocol.HCOM_PROTOCOL_PACKET_MAX_SIZE - 2];
                int bytesRead;

                // While data send data packets, previous sent an extra
                // data packet
                while (true)
                {
                    // Did user cancel?
                    if (_commsMgr.IsCancelSendingSet())
                        return;

                    bytesRead = fs.Read(dataBuff, 0, dataBuff.Length);
                    if (bytesRead <= 0)
                        break;

                    sendSerialComms.BuildAndSendBinPacket(dataBuff, 0, bytesRead, seqNumb);
                    //uiSupport.WriteSendRecvTimeLine($"[{Thread.CurrentThread.ManagedThreadId}]==>Sent data packet {seqNumb}");
                    seqNumb++;
                }
            }

            //--------------------------------------------------------------
            // 4. Send the End message to Meadow so it can close the file and cleanup
            //uiSupport.WriteSendRecvTimeLine($"[{Thread.CurrentThread.ManagedThreadId}]==>Sending End msg to Meadow after {seqNumb} data packets");

            // Note: This is no longer needed, but left to simulate current
            // CLI behavior.
            sendSerialComms.BuildAndSendHdrMessage(MeadowRqstType.HCOM_MDOW_REQUEST_END_FILE_TRANSFER, 0, 0);

            _isFileDnldActive = false;
        }
    }
}

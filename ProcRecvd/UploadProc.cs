using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    public class UploadProcessing
    {
        UiSupport _uiSupport;
        CommsManager _commsMgr;
        int _hcomHdrLen;

        FileStream _fileStream;

        UInt32 _xferCalcFileSize;
        UInt32 _xferCalcFileCrc;
        UInt32 _F7CrcFromF7;
        UInt32 _F7LenFromF7;

        int recvPacketsCount;

        public UploadProcessing(CommsManager commsMgr, UiSupport uiSupport)
        {
            _uiSupport = uiSupport;
            _commsMgr = commsMgr;

            _hcomHdrLen = Protocol.HcomProtoHdrMessageSize();
        }

        //==============================================================================
        // The special messages for file upload from the Meadow F7 are processed
        // here based Request Type.
        public void ProcessRcvdUpload(HcomProtoHdrMessage hdrMsg,
            HostRqstData hostRqstData, HostRqstType hostRqstType,
            byte[] messageBuffer, int messageLength, UInt32 payloadLength)
        {
            // Once HCOM_MDOW_REQUEST_UPLOAD_FILE_INIT is sent the F7 will respond
            // with either HCOM_HOST_REQUEST_INIT_UPLOAD_OKAY or
            // HCOM_HOST_REQUEST_INIT_UPLOAD_FAIL.
            //
            // If we get HCOM_HOST_REQUEST_INIT_UPLOAD_OKAY we must open a file on
            // this machine and respond with HCOM_MDOW_REQUEST_UPLOAD_READY_SEND_DATA.
            //
            // The F7 will begin to send HCOM_HOST_REQUEST_UPLOADING_FILE_DATA which
            // contains the file data, which we must write to the open file.
            //
            // When the F7 has finished sending the data it will send a
            // HCOM_HOST_REQUEST_UPLOAD_FILE_COMPLETED message. When it is received
            // we then close the open file and the process is completed.

            // Is this message telling us to open a file?
            if(hostRqstType == HostRqstType.HCOM_HOST_REQUEST_INIT_UPLOAD_FAIL)
            {
                _uiSupport.WriteStatusLine("Stopping upload process. Upload Initialization failed.");
                return;
            }

            //--------------------------------------------------------------------
            // Meadow successfully initialized for sending file data
            if(hostRqstType == HostRqstType.HCOM_HOST_REQUEST_INIT_UPLOAD_OKAY)
            {
                int fileMsgSize = Protocol.HcomProtoFSInfoMsgSize();

                if (messageLength >= fileMsgSize)
                {
                    HcomProtoFSInfoMsg fsInfoMsg;

                    IntPtr ptr = Marshal.AllocHGlobal(fileMsgSize);
                    Marshal.Copy(messageBuffer, 0, ptr, fileMsgSize);
                    fsInfoMsg = (HcomProtoFSInfoMsg)Marshal.PtrToStructure(ptr,
                        typeof(HcomProtoFSInfoMsg));
                    Marshal.FreeHGlobal(ptr);

                    // We can now find the checksum (CRC) and the file size from
                    // the received message
                    _F7CrcFromF7 = fsInfoMsg.fileCheckSum;
                    _F7LenFromF7 = fsInfoMsg.fileSize;
                }

                recvPacketsCount = 0;
                _xferCalcFileSize = 0;
                _xferCalcFileCrc = 0;

                // Need to open the file so we can save it. Get the name from the UI.
                string hostFileName = _uiSupport.GetHostFileName();
                _fileStream = new FileStream(hostFileName, FileMode.Create);

                // Tell F7 we are ready so start sending data
                _commsMgr.BuildHdrMsgToSend(
                    MeadowRqstType.HCOM_MDOW_REQUEST_UPLOAD_START_DATA_SEND, 0);
                return;
            }

            //--------------------------------------------------------------------
            // Received another file data packet
            if(hostRqstType == HostRqstType.HCOM_HOST_REQUEST_UPLOADING_FILE_DATA)
            {
                // This is the sequence number (is pretty much ignored)
                UInt32 userData = hdrMsg.stdHeader.userData;

                // Calculate CRC and length on the fly
                _xferCalcFileCrc = NuttxCrc.Crc32part(messageBuffer, payloadLength,
                    (uint)_hcomHdrLen, _xferCalcFileCrc);
                _xferCalcFileSize += payloadLength;

                recvPacketsCount++;

                if(recvPacketsCount == 1 || recvPacketsCount % 100 == 0)
                    _uiSupport.WriteStatusLine($"Meadow F7 uploaded file data block" +
                        $" {userData} payloadLen {payloadLength}");

                // The messageBuffer has the entire message including file data
                byte[] fileData = new byte[payloadLength];
                Buffer.BlockCopy(messageBuffer, _hcomHdrLen, fileData, 0, (int)payloadLength);
                _fileStream.Write(fileData, 0, (int)payloadLength);
                return;
            }

            //--------------------------------------------------------------------
            // Meadow F7 finished sending the requested file data
            if(hostRqstType == HostRqstType.HCOM_HOST_REQUEST_UPLOAD_FILE_COMPLETED)
            {
                //_fileStream.Flush();
                _fileStream.Close();

                // Verify that file downloaded successfully
                if(_F7LenFromF7 != _xferCalcFileSize)
                {
                    // File size mismatch
                    _uiSupport.WriteStatusLine($"Meadow F7 reported file size {_F7LenFromF7} bytes" +
                        $" but calculated {_xferCalcFileSize} bytes");
                }
                else if(_F7CrcFromF7 != _xferCalcFileCrc)
                {
                    // CRC mismatch
                    _uiSupport.WriteStatusLine($"Meadow F7 reported file CRC {_F7CrcFromF7}" +
                        $" but calculated {_xferCalcFileCrc:x4}");
                }
                else
                {
                    _uiSupport.WriteStatusLine($"Meadow F7 has sent {_xferCalcFileSize} bytes" +
                        $" in {recvPacketsCount} packets and CRCs match as {_xferCalcFileCrc:x4}");
                }
                return;
            }
        }
    }
}

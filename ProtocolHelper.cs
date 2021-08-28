using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    // There are 5 Message Classifications
    // 1. Header only - Most often used. Simple request e.g. Device Info
    // 2. File Info - Specific file information e.g. name, size, checksum...
    //     Used for file download but not upload
    // 3. Text - A single text string e.g. All text msg to host, File upload
    // 4. Binary - A single block of binary data (e.g. debugging data)
    // 5. NoHdrBin - A 2-byte sequence number (1-n) followed by file data being
    //               downloaded. It does not use the standard header. And it should
    //               be replaced with a Binary message (#4). This change requires
    //               changes to HCOM and CLI, something that must be coordinated
    public enum MessageClass
    {
        None,
        Header,     // Only, no other data
        Text,       // Header with text string
        Binary,     // Header with binary data
        FileInfo,   // Header with file information (should be simplified)
        NoHdrBin    // 2-byte header + binary data (should be removed)
    }

    // Some message must be used in combination with other messages. So far,
    // all such combination involve file upload or download. For the purposes
    // of this application, all messages are "normal."
    // Note: this is not a protocol definition but a convenience for this app
    public enum MessageComplex
    {
        Normal,
        F7FileDnld,        // Send Start, Data & End
        F7FileUpload,      // Recv Start, Data & End
        EspFileDnld,       // Send Start, Data & End
    }

    public class MeadowRqstData
    {
        public string rqstName;
        public string rqstInfo;
        public MeadowRqstType rqstType;
        public MessageClass msgClass;
        public MessageComplex msgComplex;

        public MeadowRqstData(string rqstName, string rqstInfo,
            MeadowRqstType rqstType, MessageClass msgClass, MessageComplex msgComplex)
        {
            this.rqstName = rqstName;
            this.rqstInfo = rqstInfo;
            this.rqstType = rqstType;
            this.msgClass = msgClass;
            this.msgComplex = msgComplex;
        }

        public override string ToString()
        {
            return rqstName;
        }

        public string GetHexValue()
        {
            return string.Format($"0x{rqstType.ToString("x")}");
        }
    }

    public class HostRqstData
    {
        public string rqstName;
        public string rqstInfo;
        public HostRqstType rqstType;
        public MessageClass msgClass;
        public MessageComplex msgComplex;

        public HostRqstData(string rqstName, string rqstInfo, 
            HostRqstType rqstType, MessageClass msgClass, MessageComplex msgComplex)
        {
            this.rqstName = rqstName;
            this.rqstInfo = rqstInfo;
            this.rqstType = rqstType;
            this.msgClass = msgClass;
            this.msgComplex = msgComplex;
        }

        public override string ToString()
        {
            return rqstName;
        }

        public string GetHexValue()
        {
            return string.Format($"0x{rqstType.ToString("x")}");
        }
    }

    static class ProtocolHelper
    {
        // Takes a hcom request type and outputs a string containing the header information
        public static MeadowRqstData DecodeMsgToMeadow(MeadowRqstType rqstType)
        {
            return FindMeadowRqstInformation(rqstType);
        }

        static MeadowRqstData FindMeadowRqstInformation(MeadowRqstType meadowRqstType)
        {
            string commandName;
            string commandInfo;
            MessageClass msgClass;
            MessageComplex msgComplex;

            switch (meadowRqstType)
            {
                case MeadowRqstType.HCOM_MDOW_REQUEST_UNDEFINED_REQUEST:
                    commandName = "Undefined Command";
                    commandInfo = "...";
                    msgClass = MessageClass.None;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_CHANGE_TRACE_LEVEL:
                    commandName = "Change Trace Level";
                    commandInfo = "User Data holds the new level.\r\nThe value " +
                                  "of 2 is usually a good choice.";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_FORMAT_FLASH_FILE_SYS:
                    commandName = "format flash file sys";
                    commandInfo = "No longer used";
                              msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_END_FILE_TRANSFER:
                    commandName = "End of File download";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_RESTART_PRIMARY_MCU:
                    commandName = "Restart the STM32F777 MCU";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_VERIFY_ERASED_FLASH:
                    commandName = "Verify Erased Flash";
                    commandInfo = "Only useful after bulk erase of flash.\r\n " +
                                  "This will take a few minutes to execute.";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_BULK_FLASH_ERASE:
                    commandName = "Bulk Flash Erase";
                    commandInfo = "This will take a few minutes to execute.";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_ENTER_DFU_MODE:
                    commandName = "Enter DFU Mode";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_ENABLE_DISABLE_NSH:
                    commandName = "Enable Disable NuttShell (NSH)";
                    commandInfo = "User Data = 1 will enable. Other values " +
                                  "will report that it cannot be disabled\r\n" +
                                  "This will only work if NSH support is included " +
                                  "in the build.";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_LIST_PARTITION_FILES:
                    commandName = "list partition files";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_LIST_PART_FILES_AND_CRC:
                    commandName = "list part files and crc";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_DISABLE:
                    commandName = "mono disable";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_ENABLE:
                    commandName = "mono enable";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_RUN_STATE:
                    commandName = "mono run state";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_GET_DEVICE_INFORMATION:
                    commandName = "get device information";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_PART_RENEW_FILE_SYS:
                    commandName = "part renew file sys";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_NO_TRACE_TO_HOST:
                    commandName = "no trace to host";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_SEND_TRACE_TO_HOST:
                    commandName = "send trace to host";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_END_ESP_FILE_TRANSFER:
                    commandName = "end esp file transfer";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_READ_ESP_MAC_ADDRESS:
                    commandName = "read esp mac address";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_RESTART_ESP32:
                    commandName = "restart esp32";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_FLASH:
                    commandName = "mono flash";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_SEND_TRACE_TO_UART:
                    commandName = "send trace to uart";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_NO_TRACE_TO_UART:
                    commandName = "no trace to uart";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_UPDATE_RUNTIME:
                    commandName = "mono update runtime";
                    commandInfo = "...";
                    msgClass = MessageClass.FileInfo;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_UPDATE_FILE_END:
                    commandName = "mono update file end";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_START_DBG_SESSION:
                    commandName = "mono start dbg session";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;         // VERIFY
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_GET_DEVICE_NAME:
                    commandName = "get device name";
                    commandInfo = "gets the connected Meadow's name";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_GET_INITIAL_FILE_BYTES:
                    commandName = "get initial file bytes";
                    commandInfo = "Requires the Meadow file name in 'Text Data'";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_UPLOAD_START_DATA_SEND:
                    commandName = "Upload Tell F7, Ready for Data";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_UPLOAD_ABORT_DATA_SEND:
                    commandName = "Upload Abort File sending (not implemented)";
                    commandInfo = "Cancel the current file being uploaded";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_START_FILE_TRANSFER:
                    commandName = "start file transfer";
                    commandInfo = "...";
                    msgClass = MessageClass.FileInfo;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_DELETE_FILE_BY_NAME:
                    commandName = "delete file by name";
                    commandInfo = "...";
                    msgClass = MessageClass.FileInfo;       // VERIFY
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_START_ESP_FILE_TRANSFER:
                    commandName = "start esp file transfer";
                    commandInfo = "...";
                    msgClass = MessageClass.FileInfo;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_UPLOAD_FILE_INIT:
                    commandName = "Upload ->Initialize F7 for Upload";
                    commandInfo = "Requires the Meadow file name in 'Text Data'";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_EXEC_DIAG_APP_CMD:
                    commandName = "diag execute meadow app";
                    commandInfo = "Currently only 'ping' is implemented. " +
                                  "All elements must be comma separated (e.g. ping, 123.123.123.123).";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_DEBUGGING_DEBUGGER_DATA:
                    commandName = "start mono debugger";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_DEVELOPER_1:
                    commandName = "developer 1";
                    commandInfo = "Diagnostic\r\nuserData contains the needed value";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_DEVELOPER_2:
                    commandName = "developer 2";
                    commandInfo = "Diagnostic\r\nuserData contains the needed value";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_DEVELOPER_3:
                    commandName = "developer 3";
                    commandInfo = "Diagnostic\r\nuserData contains the needed value";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_DEVELOPER_4:
                    commandName = "developer 4";
                    commandInfo = "Diagnostic\r\nuserData contains the needed value";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_QSPI_FLASH_INIT:
                    commandName = "qspi flash init";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_QSPI_FLASH_WRITE:
                    commandName = "qspi flash write";
                    commandInfo = "For testing only";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_QSPI_FLASH_READ:
                    commandName = "qspi flash read";
                    commandInfo = "For testing only";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                default:
                    commandName = "Meadow Request Type not found";
                    commandInfo = "For testing only";
                    msgClass = MessageClass.None;
                    msgComplex = MessageComplex.Normal;
                    break;
            }

            return new MeadowRqstData(commandName, commandInfo, meadowRqstType,
                msgClass, msgComplex);
        }

        //======================================================================
        // Here is were we process message from Meadow to the PC
        static public HostRqstData GetHostRqstTypeData(HostRqstType hostRqstType)
        {
            return hcom_diag_find_host_request_type(hostRqstType);
        }

        //=====================================================================
        // Messages sent from meadow to host
        static HostRqstData hcom_diag_find_host_request_type(HostRqstType hostRqstType)
        {
            string commandName;
            string commandInfo;
            MessageClass msgClass;
            MessageComplex msgComplex;

            switch (hostRqstType)
            {
                case HostRqstType.HCOM_HOST_REQUEST_UNDEFINED_REQUEST:
                    commandName = "Undefined request";
                    commandInfo = "...";
                    msgClass = MessageClass.None;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_UPLOAD_FILE_COMPLETED:
                    commandName = "F7 Uploading is finished";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.F7FileUpload;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_REJECTED:
                    commandName = "Request Rejected (not implemented)";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;
                case HostRqstType.HCOM_HOST_REQUEST_TEXT_ACCEPTED:
                    commandName = "Request Accepted";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_CONCLUDED:
                    commandName = "Previous Request Concluded";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_ERROR:
                    commandName = "Error Message";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_INFORMATION:
                    commandName = "Information Message";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_LIST_HEADER:
                    commandName = "File List Header";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_LIST_MEMBER:
                    commandName = "File List member";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_CRC_MEMBER:
                    commandName = "File List member with CRC";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_MONO_STDOUT:
                    commandName = "Mono stdout Text";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_DEVICE_INFO:
                    commandName = "Device Info Text";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_TRACE_MSG:
                    commandName = "Trace Msg Text";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_RECONNECT:
                    commandName = "Text Reconnect Request";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_MONO_STDERR:
                    commandName = "Mono stderr Text";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_DEBUGGING_MONO_DATA:
                    commandName = "Debugging mono data";
                    commandInfo = "...";
                    msgClass = MessageClass.Binary;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY:
                    commandName = "Download Initialization Okay";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL:
                    commandName = "Download Initialization Failed";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_UPLOAD_OKAY:
                    commandName = "Upload Initialization Okay";
                    commandInfo = "...";
                    msgClass = MessageClass.Text;
                    msgComplex = MessageComplex.F7FileUpload;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_UPLOAD_FAIL:
                    commandName = "Upload Initialization Failed";
                    commandInfo = "...";
                    msgClass = MessageClass.Header;
                    msgComplex = MessageComplex.F7FileUpload;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_UPLOADING_FILE_DATA:
                    commandName = "Uploading file data";
                    commandInfo = "...";
                    msgClass = MessageClass.Binary;
                    msgComplex = MessageComplex.F7FileUpload;
                    break;

                default:
                    commandName = "Host Request Type not found";
                    commandInfo = "...";
                    msgClass = MessageClass.None;
                    msgComplex = MessageComplex.Normal;
                    break;
            }

            return new HostRqstData(commandName, commandInfo,
                 hostRqstType,msgClass, msgComplex);
        }
    }
}

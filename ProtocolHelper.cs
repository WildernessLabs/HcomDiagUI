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
    public enum MessageClasses
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
    // of this application, all other messages are "normal."
    // Note: this is not a protocol definition but a convenience for this app
    public enum CurrentProcState
    {
        Normal,
        F7FileDnldRdy,        // Send Start, Data & End
        F7FileUpload,      // Recv Start, Data & End
        EspFileDnld,       // Send Start, Data & End
    }

    public class MeadowRqstData
    {
        public string rqstName;
        public string rqstInfo;
        public MeadowRqstType rqstType;
        public MessageClasses msgClass;
        public CurrentProcState procState;

        public MeadowRqstData(string rqstName, string rqstInfo,
            MeadowRqstType rqstType, MessageClasses msgClass, CurrentProcState procState)
        {
            this.rqstName = rqstName;
            this.rqstInfo = rqstInfo;
            this.rqstType = rqstType;
            this.msgClass = msgClass;
            this.procState = procState;
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
        public MessageClasses msgClass;
        public CurrentProcState procState;

        public HostRqstData(string rqstName, string rqstInfo,
            HostRqstType rqstType, MessageClasses msgClass, CurrentProcState procState)
        {
            this.rqstName = rqstName;
            this.rqstInfo = rqstInfo;
            this.rqstType = rqstType;
            this.msgClass = msgClass;
            this.procState = procState;
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
            MessageClasses msgClass;
            CurrentProcState procState;

            switch (meadowRqstType)
            {
                case MeadowRqstType.HCOM_MDOW_REQUEST_UNDEFINED_REQUEST:
                    commandName = "Undefined Command";
                    commandInfo = "...";
                    msgClass = MessageClasses.None;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_CHANGE_TRACE_LEVEL:
                    commandName = "trace level";
                    commandInfo = "UserData field holds the new level.\r\nThe value " +
                                  "of 2 is usually a good choice.";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_FORMAT_FLASH_FILE_SYS:
                    commandName = "** not supported fs format";
                    commandInfo = "Was used to format file system, but no longer supported";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_END_FILE_TRANSFER:
                    commandName = "(internal) indicates EOF for download";
                    commandInfo = "When the CLI has sent all the F/S data for download, this is sent";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_RESTART_PRIMARY_MCU:
                    commandName = "device restart";
                    commandInfo = "Restarts the STM32F777 but not the ESP32";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_VERIFY_ERASED_FLASH:
                    commandName = "flash verify";
                    commandInfo = "Only useful after bulk erase of flash.\r\n " +
                                  "This will take a few minutes to execute.";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_BULK_FLASH_ERASE:
                    commandName = "flash erase";
                    commandInfo = "Format the file system (2-3 minutes)";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_ENTER_DFU_MODE:
                    commandName = "flash os";
                    commandInfo = "Enter DFU Mode";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_ENABLE_DISABLE_NSH:
                    commandName = "nsh enable/nsh disable";
                    commandInfo = "Enable/Disable NuttShell (NSH). " +
                                  "UserData = 1 will enable. Other values " +
                                  "will report that it cannot be disabled\r\n" +
                                  "This will only work if NSH support is included " +
                                  "in the build.";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_LIST_PARTITION_FILES:
                    commandName = "file list (CLIv1)";
                    commandInfo = "Returns a list of files in the file system";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_LIST_PART_FILES_AND_CRC:
                    commandName = "file list with CRC (CLIv1)";
                    commandInfo = "Returns a list of files in the file system with CRC checksum";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_LIST_FILES_SUBDIR:
                    commandName = "File List (CLIv2)";
                    commandInfo = "Returns a list of files and directories in the file system";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_LIST_FILES_SUBDIR_CRC:
                    commandName = "File List with CRC (CLIv2)";
                    commandInfo = "Returns a list of files and directories with file CRC checksums";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_DISABLE:
                    commandName = "mono disable";
                    commandInfo = "Prevents mono from running";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_ENABLE:
                    commandName = "mono enable";
                    commandInfo = "Enables mono to run";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_RUN_STATE:
                    commandName = "mono state";
                    commandInfo = "Reports if mono is enabled or disabled";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_GET_DEVICE_INFORMATION:
                    commandName = "device info";
                    commandInfo = "Displays device information";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_PART_RENEW_FILE_SYS:
                    commandName = "fs renew";
                    commandInfo = "Deletes all files quickly";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_NO_TRACE_TO_HOST:
                    commandName = "trace disable";
                    commandInfo = "Disables trace output from being displayed on CLI Host";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_SEND_TRACE_TO_HOST:
                    commandName = "trace enable";
                    commandInfo = "Enables trace output to be displayed on CLI Host";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_END_ESP_FILE_TRANSFER:
                    commandName = "(internal) indicates EOF for ESP32";
                    commandInfo = "When the CLI has sent all the ESP32 data for download, it sends this";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_READ_ESP_MAC_ADDRESS:
                    commandName = "device mac (esp32)";
                    commandInfo = "read esp mac address";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_RESTART_ESP32:
                    commandName = "esp32 restart";
                    commandInfo = "Restarts the ESP32 coprocessor";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_FLASH:
                    commandName = "mono flash (Not in CLI, may not work)";
                    commandInfo = "Flashes the mono image already download to file system. " +
                                  "Not used much since 'mono upload rt' downloads and flashes";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_SEND_TRACE_TO_UART:
                    commandName = "uart trace enable";
                    commandInfo = "Enables trace output to the UART defined in the build.\r\n" +
                                  "Not Implemented - In User Data field:UART1 [0 or 1],\r\n" +
                                  "UART4 [4], UART6 [6], Distant future SWO [15].";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_NO_TRACE_TO_UART:
                    commandName = "uart trace disable";
                    commandInfo = "Stops trace output from going to defined UART";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_UPDATE_RUNTIME:
                    commandName = "mono update rt";
                    commandInfo = "Downloads and flashes mono runtime image to Meadow " +
                                  "This command is not implemented in HcomDiagUi";
                    msgClass = MessageClasses.FileInfo;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_UPDATE_FILE_END:
                    commandName = "(internal) indicates mono RT downloaded";
                    commandInfo = "CLI sends this when mono RT has been downloaded";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_MONO_START_DBG_SESSION:
                    commandName = "debug";
                    commandInfo = "Start mono dbg session";
                    msgClass = MessageClasses.Header;         // VERIFY
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_GET_DEVICE_NAME:
                    commandName = "device name";
                    commandInfo = "Gets the connected Meadow's name (MeadowF7 is default)";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_GET_INITIAL_FILE_BYTES:
                    commandName = "file initial";
                    commandInfo = "Gets the initial bytes (upto 500).\r\n" +
                                  "Requires the Meadow file name in 'Text Data' field";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_UPLOAD_START_DATA_SEND:
                    commandName = "(internal) file read start";
                    commandInfo = "Internal used by Meadow to tell CLI it's ready for data to be sent";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_UPLOAD_ABORT_DATA_SEND:
                    commandName = "(new) file read abort";
                    commandInfo = "Cancel the current file being uploaded";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_START_FILE_TRANSFER:
                    commandName = "File Write";
                    commandInfo = "Downloads a file to the Meadow file system";
                    msgClass = MessageClasses.FileInfo;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_DELETE_FILE_BY_NAME:
                    commandName = "file delete";
                    commandInfo = "Deletes a file by name from the F7 file system";
                    msgClass = MessageClasses.FileInfo;       // VERIFY
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_START_ESP_FILE_TRANSFER:
                    commandName = "esp file write";
                    commandInfo = "start esp file transfer";
                    msgClass = MessageClasses.FileInfo;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_UPLOAD_FILE_INIT:
                    commandName = "(new) file read (aka upload)";
                    commandInfo = "Initiates reading a file from Meadow. Put host's destination pathname " +
                        "in 'Host File' field and the Meadow path name in the 'File Name' field";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_EXEC_DIAG_APP_CMD:
                    commandName = "(new) Send command-line";
                    commandInfo = "Only for Ethernet. Sends command (e.g. ping to web site via IP or name. " +
                                  "All elements must be space separated (e.g. ping abc.xyz).";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_RTC_SET_TIME_CMD:
                    commandName = "(new) Set RTC clock";
                    commandInfo = "Needs UTC time in ISO 8601 format (e.g. 2022-03-31T17:34:25-07:00)";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_RTC_READ_TIME_CMD:
                    commandName = "(new) Read RTC clock";
                    commandInfo = "Reads UTC time in ISO 8601 format (e.g. 2022-03-31T17:34:25-07:00)";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_RTC_WAKEUP_TIME_CMD:
                    commandName = "(new) Set RTC Alarm (wakeup)";
                    commandInfo = "Sends ISO 8601 time period (e.g. 3:19 P0Y0M0DT0H3M19S)";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_DEBUGGING_DEBUGGER_DATA:
                    commandName = "(internal) debugging data";
                    commandInfo = "Identifies to F7 debugging information";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                    // This replaces the 4 developer items above
               case MeadowRqstType.HCOM_MDOW_REQUEST_DEVELOPER:
                    commandName = "Developer";
                    commandInfo = "(new) Diagnostic\r\nExtraData is developer number, UserData contains the value";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

               case MeadowRqstType.HCOM_MDOW_REQUEST_GET_FILES_AND_FOLDERS:
                    commandName = "file list & dev nested";
                    commandInfo = "Returns a list of all devices and files";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_QSPI_FLASH_INIT:
                    commandName = "qspi init";
                    commandInfo = "...";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_QSPI_FLASH_WRITE:
                    commandName = "qspi write";
                    commandInfo = "For testing only";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_QSPI_FLASH_READ:
                    commandName = "qspi read";
                    commandInfo = "For testing only";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                case MeadowRqstType.HCOM_MDOW_REQUEST_OTA_REGISTER_DEVICE:
                    commandName = "OTA - Register device";
                    commandInfo = "For testing only";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.Normal;
                    break;

                default:
                    commandName = "Meadow Request Type not found";
                    commandInfo = "For testing only";
                    msgClass = MessageClasses.None;
                    procState = CurrentProcState.Normal;
                    break;
            }

            return new MeadowRqstData(commandName, commandInfo, meadowRqstType,
                msgClass, procState);
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
            MessageClasses msgClass;
            CurrentProcState procState;

            switch (hostRqstType)
            {
                case HostRqstType.HCOM_HOST_REQUEST_UNDEFINED_REQUEST:
                    commandName = "Undefined request";
                    commandInfo = "...";
                    msgClass = MessageClasses.None;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_UPLOAD_FILE_COMPLETED:
                    commandName = "(new-internal) F7 Uploading is finished";
                    commandInfo = "...";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.F7FileUpload;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_REJECTED:
                    commandName = "Request Rejected (not implemented)";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;
                case HostRqstType.HCOM_HOST_REQUEST_TEXT_ACCEPTED:
                    commandName = "Request Accepted";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_CONCLUDED:
                    commandName = "Concluded Msg Received";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_ERROR:
                    commandName = "Error Message";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_INFORMATION:
                    commandName = "Information Message";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_LIST_HEADER:
                    commandName = "File List Header";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_LIST_MEMBER:
                    commandName = "File List member";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_CRC_MEMBER:
                    commandName = "File List member with CRC";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_MONO_STDOUT:
                    commandName = "Mono stdout Text";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_DEVICE_INFO:
                    commandName = "Device Info Text";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_TRACE_MSG:
                    commandName = "Trace Msg Text";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_RECONNECT:
                    commandName = "Text Reconnect Request";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_TEXT_MONO_STDERR:
                    commandName = "Mono stderr Text";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_DEBUGGING_MONO_DATA:
                    commandName = "Debugging mono data";
                    commandInfo = "...";
                    msgClass = MessageClasses.Binary;
                    procState = CurrentProcState.Normal;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY:
                    commandName = "Download Initialization Okay";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.F7FileDnldRdy;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL:
                    commandName = "Download Initialization Failed";
                    commandInfo = "...";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.F7FileDnldRdy;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_UPLOAD_OKAY:
                    commandName = "(new-internal) Upload Initialization Okay";
                    commandInfo = "...";
                    msgClass = MessageClasses.Text;
                    procState = CurrentProcState.F7FileUpload;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_INIT_UPLOAD_FAIL:
                    commandName = "(new-internal) Upload Initialization Failed";
                    commandInfo = "...";
                    msgClass = MessageClasses.Header;
                    procState = CurrentProcState.F7FileUpload;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_UPLOADING_FILE_DATA:
                    commandName = "Uploading file data";
                    commandInfo = "...";
                    msgClass = MessageClasses.Binary;
                    procState = CurrentProcState.F7FileUpload;
                    break;

                case HostRqstType.HCOM_HOST_REQUEST_DNLD_FAIL_RESEND:
                    commandName = "Download failed and timer";
                    commandInfo = "...";
                    msgClass = MessageClasses.FileInfo;
                    procState = CurrentProcState.F7FileDnldRdy;
                    break;

                default:
                    commandName = "Host Request Type not found";
                    commandInfo = "...";
                    msgClass = MessageClasses.None;
                    procState = CurrentProcState.Normal;
                    break;
            }

            return new HostRqstData(commandName, commandInfo,
                 hostRqstType, msgClass, procState);
        }
    }
}

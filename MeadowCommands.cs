using System;
using System.Collections.Generic;
using System.Linq;
using static HcomDiagUi.MeadowRqstType;

namespace HcomDiagUi
{
    //==========================================================================
    // Everything a command's sender needs. Text and HostFile are read from
    // the UI only when a sender uses them.
    public sealed class SendContext
    {
        readonly UiSupport _uiSupport;
        readonly SendSerialComms _sendSerialComms;
        readonly DnldF7Files _f7Download;

        public MeadowRqstType RqstType { get; }
        public UInt32 UserData { get; }
        public UInt16 ExtraData { get; }

        // Set by a sender that decided not to send anything (e.g. the host file to
        // download doesn't exist). Meadow won't reply, so CommsManager must not
        // wait for a 'Concluded' message.
        public bool NothingSent { get; private set; }

        public void MarkNothingSent()
        {
            NothingSent = true;
        }

        // The 'Meadow: Text, Data, Path Name or CMD Line' field
        public string Text => _uiSupport.TextForMsg();

        // The 'Host File' field
        public string HostFile => _uiSupport.GetHostFileInfo();

        public SendContext(MeadowRqstType rqstType, UInt32 userData, UInt16 extraData,
            UiSupport uiSupport, SendSerialComms sendSerialComms, DnldF7Files f7Download)
        {
            RqstType = rqstType;
            UserData = userData;
            ExtraData = extraData;
            _uiSupport = uiSupport;
            _sendSerialComms = sendSerialComms;
            _f7Download = f7Download;
        }

        public void SendHeader()
        {
            _sendSerialComms.BuildAndSendHdrMessage(RqstType, UserData, ExtraData);
        }

        public void SendText(string text)
        {
            _sendSerialComms.BuildAndSendTextMsg(RqstType, UserData, ExtraData, text);
        }

        // Header only, with a fixed User Data instead of the User Data field
        public void SendHeader(UInt32 userData)
        {
            _sendSerialComms.BuildAndSendHdrMessage(RqstType, userData, ExtraData);
        }

        // File-info message with only the Meadow file name: size, CRC, ESP32
        // address and MD5 hash are all zero. Same as Meadow.CLI's FileDeleteRequest.
        public void SendFileInfoNameOnly(string meadowFileName)
        {
            _sendSerialComms.BuildAndSendFileRelatedCommand(RqstType, UserData, ExtraData,
                0, 0, 0, new string('\0', 32), meadowFileName);
        }

        // Sends the file start message. userData is still sent in the partition
        // field, which Meadow.OS no longer uses (partitioning was dropped; the
        // field is kept so the HCOM protocol doesn't change). Only returns
        // after Meadow has accepted or rejected it; the file data and end message
        // are then sent by a thread pool thread (see DnldF7Files).
        // With no Meadow file name the host file's name is used, as Meadow.CLI's
        // 'file write' does.
        public void StartF7FileDownload()
        {
            string hostFile = HostFile;
            string meadowFileName = Text;
            if (string.IsNullOrWhiteSpace(meadowFileName))
            {
                meadowFileName = HostFileName(hostFile);
                _uiSupport.WriteStatusLine($"Meadow file name not given, using host file name '{meadowFileName}'");
            }

            if (!_f7Download.FindDownloadFileInformation(RqstType, UserData, meadowFileName, hostFile))
                MarkNothingSent();
        }

        // Flashes the host file to the ESP32, as Meadow.CLI 'firmware write esp'
        // does. Flash address: the Meadow text field if given (e.g. 0x10000),
        // otherwise from the file name. User Data 1 = last file: Meadow then
        // finishes and restarts the ESP32 (Meadow.CLI always sends 0).
        public void StartEsp32FileDownload()
        {
            string hostFile = HostFile;
            string fileName = HostFileName(hostFile);
            string addressText = Text.Trim();

            UInt32 flashAddress;
            if (addressText.Length > 0)
            {
                if (!TryParseAddress(addressText, out flashAddress))
                {
                    _uiSupport.WriteStatusLine($"'{addressText}' is not a flash address (e.g. 0x10000)");
                    MarkNothingSent();
                    return;
                }
            }
            else if (!Esp32FlashAddresses.TryGetValue(fileName, out flashAddress))
            {
                _uiSupport.WriteStatusLine($"No known ESP32 flash address for '{fileName}'; " +
                    "enter it in the Meadow text field (e.g. 0x10000)");
                MarkNothingSent();
                return;
            }

            _uiSupport.WriteStatusLine($"Flashing '{fileName}' to ESP32 address 0x{flashAddress:x}" +
                (UserData == 1 ? ", last file (ESP32 restarts afterwards)" : ""));

            if (!_f7Download.StartFileDownload(RqstType,
                    MeadowRqstType.HCOM_MDOW_REQUEST_END_ESP_FILE_TRANSFER, 0, fileName, hostFile,
                    flashAddress, true, UserData))
                MarkNothingSent();
        }

        // ESP32 flash addresses of the coprocessor files, as Meadow.CLI's
        // MeadowDevice.GetFileTargetAddress()
        static readonly Dictionary<string, UInt32> Esp32FlashAddresses =
            new Dictionary<string, UInt32>(StringComparer.OrdinalIgnoreCase)
            {
                { "bootloader.bin", 0x1000 },
                { "partition-table.bin", 0x8000 },
                { "MeadowComms.bin", 0x10000 },
            };

        // Hex with 0x prefix, or decimal
        static bool TryParseAddress(string text, out UInt32 address)
        {
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return UInt32.TryParse(text.Substring(2), System.Globalization.NumberStyles.HexNumber,
                    null, out address);
            return UInt32.TryParse(text, out address);
        }

        // File name without its folder. Accepts either path separator, so it
        // doesn't depend on the host OS.
        static string HostFileName(string hostPath)
        {
            int lastSeparator = hostPath.LastIndexOfAny(new[] { '\\', '/' });
            return hostPath.Substring(lastSeparator + 1).Trim();
        }
    }

    //==========================================================================
    // What a command does with one of the UI's input fields
    public sealed class FieldUse
    {
        // null when the field is not used
        public string Label { get; }

        // Send is refused while a required text field is empty
        public bool Required { get; }

        public bool Used => Label != null;

        FieldUse(string label, bool required)
        {
            Label = label;
            Required = required;
        }

        public static readonly FieldUse NotUsed = new FieldUse(null, false);

        public static FieldUse Optional(string label) => new FieldUse(label, false);
        public static FieldUse Mandatory(string label) => new FieldUse(label, true);

        // Lets a table entry write userData: "Trace level"
        public static implicit operator FieldUse(string label) => Optional(label);
    }

    // What a command does with each input field. Commands whose use of the
    // fields hasn't been checked against Meadow.OS have Unspecified, and the
    // UI keeps its generic field labels for them.
    public sealed class CommandFields
    {
        public bool Specified { get; }
        public FieldUse UserData { get; }
        public FieldUse ExtraData { get; }
        public FieldUse HostFile { get; }
        public FieldUse Text { get; }

        public CommandFields(bool specified, FieldUse userData, FieldUse extraData,
            FieldUse hostFile, FieldUse text)
        {
            Specified = specified;
            UserData = userData;
            ExtraData = extraData;
            HostFile = hostFile;
            Text = text;
        }

        public static readonly CommandFields Unspecified = new CommandFields(false,
            FieldUse.NotUsed, FieldUse.NotUsed, FieldUse.NotUsed, FieldUse.NotUsed);
    }

    //==========================================================================
    // Internal and Unsupported commands are only listed when 'Show internal/
    // unsupported' is ticked
    public enum CommandCategory
    {
        Normal,
        Internal,       // Normally sent automatically as one step of a longer exchange
        Unsupported,    // No longer supported by Meadow, or not implemented here
    }

    //==========================================================================
    // How a command is sent. MsgClass is shown in the UI's 'Cmd Type' box.
    public sealed class CommandSender
    {
        readonly Action<SendContext> _send;

        public string Name { get; }
        public MessageClasses MsgClass { get; }

        // Set when the sender always sends this User Data (see Send.HeaderWithUserData)
        public UInt32? FixedUserData { get; }

        public CommandSender(string name, MessageClasses msgClass, Action<SendContext> send,
            UInt32? fixedUserData = null)
        {
            Name = name;
            MsgClass = msgClass;
            _send = send;
            FixedUserData = fixedUserData;
        }

        public void Send(SendContext ctx)
        {
            _send(ctx);
        }
    }

    // The standard ways of sending a command
    public static class Send
    {
        // Sends nothing
        public static readonly CommandSender Nothing =
            new CommandSender("Nothing", MessageClasses.None, ctx => ctx.MarkNothingSent());

        // Header only
        public static readonly CommandSender Header =
            new CommandSender("Header", MessageClasses.Header, ctx => ctx.SendHeader());

        // Header only, always with the given User Data. For several list entries
        // sharing one request type (e.g. NSH Enable / NSH Disable).
        public static CommandSender HeaderWithUserData(UInt32 userData) =>
            new CommandSender($"Header(UserData={userData})", MessageClasses.Header,
                ctx => ctx.SendHeader(userData), userData);

        // Header followed by the 'Meadow: Text...' field
        public static readonly CommandSender Text =
            new CommandSender("Text", MessageClasses.Text, ctx => ctx.SendText(ctx.Text));

        // File-info message carrying only the 'Meadow: Text...' file name
        public static readonly CommandSender FileInfoNameOnly =
            new CommandSender("FileInfoNameOnly", MessageClasses.FileInfo, ctx => ctx.SendFileInfoNameOnly(ctx.Text));

        // Flashes the 'Host File' to the ESP32 coprocessor
        public static readonly CommandSender Esp32FileDownload =
            new CommandSender("Esp32FileDownload", MessageClasses.FileInfo, ctx => ctx.StartEsp32FileDownload());

        // Downloads the 'Host File' to Meadow as the 'Meadow: Text...' file name
        public static readonly CommandSender F7FileDownload =
            new CommandSender("F7FileDownload", MessageClasses.FileInfo, ctx => ctx.StartF7FileDownload());
    }

    //==========================================================================
    // Every command that can be sent to Meadow. To add a command, add its
    // MeadowRqstType value (Protocol.cs) and one entry here. A command that
    // needs more than a standard sender (see Send) gets its own CommandSender,
    // like SendRtcSetTime below.
    //
    // Uses(...) says what the command does with each input field, as checked
    // against Meadow.OS (apps/system/hcom). Fields not named in Uses(...) are
    // shown as not used; Req(...) marks a text field that must not be empty.
    // Entries without Uses(...) keep the UI's generic field labels.
    public static class MeadowCommands
    {
        static MeadowRqstData Cmd(MeadowRqstType rqstType, string name, string info,
            CommandSender sender, CommandFields fields = null,
            CommandCategory category = CommandCategory.Normal)
        {
            return new MeadowRqstData(name, info, rqstType, sender,
                fields ?? CommandFields.Unspecified, category);
        }

        const CommandCategory Internal = CommandCategory.Internal;
        const CommandCategory Unsupported = CommandCategory.Unsupported;

        static CommandFields Uses(FieldUse userData = null, FieldUse extraData = null,
            FieldUse hostFile = null, FieldUse text = null)
        {
            return new CommandFields(true, userData ?? FieldUse.NotUsed,
                extraData ?? FieldUse.NotUsed, hostFile ?? FieldUse.NotUsed,
                text ?? FieldUse.NotUsed);
        }

        static FieldUse Req(string label) => FieldUse.Mandatory(label);

        // Sends the host's current time instead of the text field
        static readonly CommandSender SendRtcSetTime =
            new CommandSender("RtcSetTime", MessageClasses.Text, ctx =>
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
                ctx.SendText(DateTimeOffset.UtcNow.ToString("o"));

                // Send local time (both UTC and local can be sent at the same time)
                //ctx.SendText(DateTimeOffset.Now.ToString("o"));
            });

        public static readonly MeadowRqstData[] All =
        {
            Cmd(HCOM_MDOW_REQUEST_UNDEFINED_REQUEST, "Undefined Command",
                "...",
                Send.Nothing, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_CHANGE_TRACE_LEVEL, "Trace Level - Set",
                "UserData field holds the new level.\r\nThe value " +
                "of 2 is usually a good choice.",
                Send.Header, Uses(userData: "Trace level")),

            Cmd(HCOM_MDOW_REQUEST_FORMAT_FLASH_FILE_SYS, "** Not Supported FS Format",
                "Was used to format file system, but no longer supported",
                Send.Header, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_END_FILE_TRANSFER, "(Internal) Indicates EOF for F/S",
                "When the CLI has sent all the F/S data for download, this is sent",
                Send.Header, Uses(), category: Internal),

            Cmd(HCOM_MDOW_REQUEST_RESTART_PRIMARY_MCU, "Device Restart",
                "Restarts the STM32F777 but not the ESP32",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_VERIFY_ERASED_FLASH, "Flash Verify",
                "Only useful after bulk erase of flash.\r\n " +
                "This will take a few minutes to execute.",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_BULK_FLASH_ERASE, "Flash Erase",
                "Format the file system (2-3 minutes)",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_ENTER_DFU_MODE, "Flash OS",
                "Enter DFU Mode",
                Send.Header, Uses()),

            // Both send ENABLE_DISABLE_NSH; the User Data is set for you
            Cmd(HCOM_MDOW_REQUEST_ENABLE_DISABLE_NSH, "NSH Enable",
                "Enables NuttShell (NSH) (sends User Data 1).\r\n" +
                "This will only work if NSH support is included in the build.",
                Send.HeaderWithUserData(1), Uses()),

            Cmd(HCOM_MDOW_REQUEST_ENABLE_DISABLE_NSH, "NSH Disable",
                "Disables NuttShell (NSH) (sends User Data 0). Meadow currently " +
                "replies that NSH cannot be disabled.",
                Send.HeaderWithUserData(0), Uses()),

            Cmd(HCOM_MDOW_REQUEST_LIST_PARTITION_FILES, "File List (CLIv1)",
                "Returns a list of files in the file system (removed from Meadow.OS)",
                Send.Header, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_LIST_PART_FILES_AND_CRC, "File List with CRC (CLIv1)",
                "Returns a list of files in the file system with CRC checksum (removed from Meadow.OS)",
                Send.Header, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_LIST_FILES_SUBDIR, "File List",
                "Returns a list of files and directories in the file system",
                Send.Text, Uses(text: "directory, e.g. /meadow0/dir1x/ (must end with '/'; empty = /meadow0/)")),

            Cmd(HCOM_MDOW_REQUEST_LIST_FILES_SUBDIR_CRC, "File List with CRC",
                "Returns a list of files and directories with file CRC checksums",
                Send.Text, Uses(text: "directory, e.g. /meadow0/dir1x/ (must end with '/'; empty = /meadow0/)")),

            Cmd(HCOM_MDOW_REQUEST_MONO_DISABLE, "Mono Disable",
                "Prevents mono from running",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_MONO_ENABLE, "Mono Enable",
                "Enables mono to run",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_MONO_RUN_STATE, "Mono State",
                "Reports if mono is enabled or disabled",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_GET_DEVICE_INFORMATION, "Device Info",
                "Displays device information",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_PART_RENEW_FILE_SYS, "FS Renew",
                "Deletes all files quickly",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_NO_TRACE_TO_HOST, "Trace Disable",
                "Disables trace output from being displayed on CLI",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_SEND_TRACE_TO_HOST, "Trace Enable",
                "Enables trace output to be displayed on CLI",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_END_ESP_FILE_TRANSFER, "(Internal) Indicates EOF for ESP32",
                "When the CLI has sent all the ESP32 data for download, it sends this",
                Send.Header, Uses(userData: "1 = last file"), category: Internal),

            Cmd(HCOM_MDOW_REQUEST_READ_ESP_MAC_ADDRESS, "Device MAC (ESP32)",
                "read esp mac address",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_RESTART_ESP32, "ESP32 Restart",
                "Restarts the ESP32 coprocessor",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_MONO_FLASH, "Mono Flash",
                "Flashes the mono image already download to file system. " +
                "Not used much since 'Mono Update RT' downloads and flashes",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_SEND_TRACE_TO_UART, "UART Trace Enable",
                "Directs trace output to uart1",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_NO_TRACE_TO_UART, "UART Trace Disable",
                "Stops trace output from going to uart1",
                Send.Header, Uses()),

            // Meadow.CLI 'uart profiler enable/disable'. The profiler data itself
            // comes from the Mono runtime's log profiler (.mlpd) and is captured
            // from UART1 with a separate serial adapter, not by HcomDiagUi.
            Cmd(HCOM_MDOW_REQUEST_SEND_PROFILER_TO_UART, "UART Profiler Enable",
                "Makes UART1 exclusive for Mono profiler data; trace output stops going " +
                "to UART1. Survives a restart. Same as Meadow.CLI 'uart profiler enable'",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_NO_PROFILER_TO_UART, "UART Profiler Disable",
                "Returns UART1 to normal use. Same as Meadow.CLI 'uart profiler disable'",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_MONO_UPDATE_RUNTIME, "Mono Update RT",
                "Downloads and flashes mono runtime image to Meadow " +
                "This command is not implemented in HcomDiagUi",
                Send.F7FileDownload, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_MONO_UPDATE_FILE_END, "(Internal) Indicates Mono RT Downloaded",
                "CLI sends this when mono RT has been downloaded",
                Send.Header, Uses(), category: Internal),

            Cmd(HCOM_MDOW_REQUEST_MONO_START_DBG_SESSION, "Debug",
                "Start mono dbg session",
                Send.Header, Uses()),    // VERIFY

            Cmd(HCOM_MDOW_REQUEST_GET_DEVICE_NAME, "Device Name",
                "Gets the connected Meadow's name (MeadowF7 is default)",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_GET_INITIAL_FILE_BYTES, "File Initial",
                "Gets the initial bytes (upto 500).\r\n" +
                "Requires the Meadow file name in 'Text Data' field",
                Send.Text, Uses(text: Req("Meadow file name"))),

            Cmd(HCOM_MDOW_REQUEST_UPLOAD_START_DATA_SEND, "(Internal) File Read Start",
                "Internal used by Meadow to tell CLI it's ready for data to be sent",
                Send.Header, Uses(), category: Internal),

            Cmd(HCOM_MDOW_REQUEST_UPLOAD_ABORT_DATA_SEND, "File Read Abort",
                "Cancel the current file being uploaded",
                Send.Header, Uses()),

            Cmd(HCOM_MDOW_REQUEST_START_FILE_TRANSFER, "File Write",
                "Downloads a file to the Meadow file system",
                Send.F7FileDownload,
                Uses(hostFile: Req("file to send"), text: "Meadow file name (empty = host's file name)")),

            Cmd(HCOM_MDOW_REQUEST_DELETE_FILE_BY_NAME, "File Delete",
                "Deletes a file by name from the F7 file system",
                Send.FileInfoNameOnly, Uses(text: Req("Meadow file name"))),

            Cmd(HCOM_MDOW_REQUEST_START_ESP_FILE_TRANSFER, "ESP32 File Write",
                "Flashes the Host File (full path, e.g. ...\\WildernessLabs\\Firmware\\2.5.7.9\\MeadowComms.bin) " +
                "to the ESP32 coprocessor, as Meadow.CLI 'firmware write esp'. " +
                "Flash address: Meadow text field (e.g. 0x10000), or empty = from the file name " +
                "(bootloader.bin 0x1000, partition-table.bin 0x8000, MeadowComms.bin 0x10000). " +
                "User Data 1 = last file: Meadow finishes and restarts the ESP32. " +
                "Mono must be disabled; flashing can take a few minutes.",
                Send.Esp32FileDownload,
                Uses(userData: "1 = last file", hostFile: Req("full path of the file to flash"),
                    text: "flash address, e.g. 0x10000 (empty = from file name)")),

            Cmd(HCOM_MDOW_REQUEST_UPLOAD_FILE_INIT, "File Read (aka Upload)",
                "Initiates reading a file from Meadow. Put host's destination pathname " +
                "in 'Host File' field and the Meadow path name in the 'File Name' field",
                Send.Text,
                Uses(hostFile: Req("file to save to"), text: Req("Meadow file name"))),

            Cmd(HCOM_MDOW_REQUEST_RTC_SET_TIME_CMD, "Set RTC Clock",
                "Needs UTC time in ISO 8601 format (e.g. 2022-03-31T17:34:25-07:00)",
                SendRtcSetTime, Uses()),     // Sends the host's time

            Cmd(HCOM_MDOW_REQUEST_RTC_READ_TIME_CMD, "Read RTC Clock",
                "Reads UTC time in ISO 8601 format (e.g. 2022-03-31T17:34:25-07:00)",
                Send.Text, Uses()),

            Cmd(HCOM_MDOW_REQUEST_RTC_WAKEUP_TIME_CMD, "Set RTC Alarm (Wakeup)",
                "Sends ISO 8601 time period (e.g. 3:19 P0Y0M0DT0H3M19S)",
                Send.Text, Uses(text: Req("ISO 8601 period, e.g. P0Y0M0DT0H3M19S"))),

            Cmd(HCOM_MDOW_REQUEST_DEBUGGING_DEBUGGER_DATA, "(Internal) Debugging Data",
                "Identifies to F7 debugging information",
                Send.Header, category: Internal),

            // This replaces the 4 developer items above
            Cmd(HCOM_MDOW_REQUEST_DEVELOPER, "Developer",
                "Diagnostic\r\nExtraData is developer number, UserData contains the value",
                Send.Header, Uses(userData: "Value", extraData: "Developer no.")),

            Cmd(HCOM_MDOW_REQUEST_GET_FILES_AND_FOLDERS, "File List - Nested",
                "Returns a list of all devices and files",
                Send.Header, Uses(userData: "1234: show /proc")),

            Cmd(HCOM_MDOW_REQUEST_QSPI_FLASH_INIT, "QSPI Init",
                "For testing only (removed from Meadow.OS)",
                Send.Header, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_QSPI_FLASH_WRITE, "QSPI Write",
                "For testing only (removed from Meadow.OS)",
                Send.Header, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_QSPI_FLASH_READ, "QSPI Read",
                "For testing only (removed from Meadow.OS)",
                Send.Header, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_OTA_REGISTER_DEVICE, "OTA - Register Device",
                "For testing only",
                Send.Header, Uses()),
        };

        static readonly Dictionary<MeadowRqstType, MeadowRqstData> _byType =
            All.GroupBy(c => c.rqstType).ToDictionary(g => g.Key, g => g.First());

        public static MeadowRqstData Find(MeadowRqstType rqstType)
        {
            if (_byType.TryGetValue(rqstType, out MeadowRqstData command))
                return command;

            return new MeadowRqstData("Meadow Request Type not found", "For testing only",
                rqstType, Send.Nothing, CommandFields.Unspecified, CommandCategory.Unsupported);
        }

        // Problems with the table, e.g. a MeadowRqstType value without an entry.
        // Reported in the Status box at startup.
        public static IEnumerable<string> Validate()
        {
            foreach (MeadowRqstType rqstType in Enum.GetValues(typeof(MeadowRqstType)))
            {
                if (!_byType.ContainsKey(rqstType))
                    yield return $"MeadowCommands: no entry for {rqstType}";
            }

            // Several entries for one request type are only allowed when each sends
            // its own fixed User Data (e.g. NSH Enable / NSH Disable)
            foreach (var group in All.GroupBy(c => c.rqstType).Where(g => g.Count() > 1))
            {
                var fixedValues = group.Select(c => c.sender.FixedUserData).ToList();
                if (fixedValues.Any(v => v == null) || fixedValues.Distinct().Count() != fixedValues.Count)
                    yield return $"MeadowCommands: {group.Key} has {group.Count()} entries";
            }
        }
    }
}

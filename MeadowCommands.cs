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

        // Text message that also carries the length of the argument list
        public void SendDiagCmd(string text)
        {
            _sendSerialComms.BuildAndSendDiagCmdMsg(RqstType, UserData, ExtraData, text);
        }

        // Sends the file start message. userData is still sent in the partition
        // field, which Meadow.OS no longer uses (partitioning was dropped; the
        // field is kept so the HCOM protocol doesn't change). Only returns
        // after Meadow has accepted or rejected it; the file data and end message
        // are then sent by a thread pool thread (see DnldF7Files).
        public void StartF7FileDownload()
        {
            _f7Download.FindDownloadFileInformation(RqstType, UserData, Text, HostFile);
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

        public CommandSender(string name, MessageClasses msgClass, Action<SendContext> send)
        {
            Name = name;
            MsgClass = msgClass;
            _send = send;
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
            new CommandSender("Nothing", MessageClasses.None, ctx => { });

        // Header only
        public static readonly CommandSender Header =
            new CommandSender("Header", MessageClasses.Header, ctx => ctx.SendHeader());

        // Header followed by the 'Meadow: Text...' field
        public static readonly CommandSender Text =
            new CommandSender("Text", MessageClasses.Text, ctx => ctx.SendText(ctx.Text));

        // Like Text but with the argument list length (diagnostic command line)
        public static readonly CommandSender DiagCmd =
            new CommandSender("DiagCmd", MessageClasses.Text, ctx => ctx.SendDiagCmd(ctx.Text));

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

        // Sends the PC's current time instead of the text field
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

            Cmd(HCOM_MDOW_REQUEST_CHANGE_TRACE_LEVEL, "trace level",
                "UserData field holds the new level.\r\nThe value " +
                "of 2 is usually a good choice.",
                Send.Header, Uses(userData: "Trace level")),

            Cmd(HCOM_MDOW_REQUEST_FORMAT_FLASH_FILE_SYS, "** not supported fs format",
                "Was used to format file system, but no longer supported",
                Send.Header, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_END_FILE_TRANSFER, "(internal) indicates EOF for F/S",
                "When the CLI has sent all the F/S data for download, this is sent",
                Send.Header, category: Internal),

            Cmd(HCOM_MDOW_REQUEST_RESTART_PRIMARY_MCU, "device restart",
                "Restarts the STM32F777 but not the ESP32",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_VERIFY_ERASED_FLASH, "flash verify",
                "Only useful after bulk erase of flash.\r\n " +
                "This will take a few minutes to execute.",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_BULK_FLASH_ERASE, "flash erase",
                "Format the file system (2-3 minutes)",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_ENTER_DFU_MODE, "flash os",
                "Enter DFU Mode",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_ENABLE_DISABLE_NSH, "nsh enable/nsh disable",
                "Enable/Disable NuttShell (NSH). " +
                "UserData = 1 will enable. Other values " +
                "will report that it cannot be disabled\r\n" +
                "This will only work if NSH support is included " +
                "in the build.",
                Send.Header, Uses(userData: "1 = enable")),

            Cmd(HCOM_MDOW_REQUEST_LIST_PARTITION_FILES, "file list (CLIv1)",
                "Returns a list of files in the file system",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_LIST_PART_FILES_AND_CRC, "file list with CRC (CLIv1)",
                "Returns a list of files in the file system with CRC checksum",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_LIST_FILES_SUBDIR, "File List (CLIv2)",
                "Returns a list of files and directories in the file system",
                Send.Text, Uses(text: "Meadow directory (empty = /meadow0)")),

            Cmd(HCOM_MDOW_REQUEST_LIST_FILES_SUBDIR_CRC, "File List with CRC (CLIv2)",
                "Returns a list of files and directories with file CRC checksums",
                Send.Text, Uses(text: "Meadow directory (empty = /meadow0)")),

            Cmd(HCOM_MDOW_REQUEST_MONO_DISABLE, "mono disable",
                "Prevents mono from running",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_MONO_ENABLE, "mono enable",
                "Enables mono to run",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_MONO_RUN_STATE, "mono state",
                "Reports if mono is enabled or disabled",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_GET_DEVICE_INFORMATION, "device info",
                "Displays device information",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_PART_RENEW_FILE_SYS, "fs renew",
                "Deletes all files quickly",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_NO_TRACE_TO_HOST, "trace disable",
                "Disables trace output from being displayed on CLI",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_SEND_TRACE_TO_HOST, "trace enable",
                "Enables trace output to be displayed on CLI",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_END_ESP_FILE_TRANSFER, "(internal) indicates EOF for ESP32",
                "When the CLI has sent all the ESP32 data for download, it sends this",
                Send.Header, category: Internal),

            Cmd(HCOM_MDOW_REQUEST_READ_ESP_MAC_ADDRESS, "device mac (esp32)",
                "read esp mac address",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_RESTART_ESP32, "esp32 restart",
                "Restarts the ESP32 coprocessor",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_MONO_FLASH, "mono flash",
                "Flashes the mono image already download to file system. " +
                "Not used much since 'mono upload rt' downloads and flashes",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_SEND_TRACE_TO_UART, "uart trace -e",
                "Directs trace output to uart1",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_NO_TRACE_TO_UART, "uart trace",
                "Stops trace output from going to uart1",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_MONO_UPDATE_RUNTIME, "mono update rt",
                "Downloads and flashes mono runtime image to Meadow " +
                "This command is not implemented in HcomDiagUi",
                Send.F7FileDownload, category: Unsupported),

            Cmd(HCOM_MDOW_REQUEST_MONO_UPDATE_FILE_END, "(internal) indicates mono RT downloaded",
                "CLI sends this when mono RT has been downloaded",
                Send.Header, category: Internal),

            Cmd(HCOM_MDOW_REQUEST_MONO_START_DBG_SESSION, "debug",
                "Start mono dbg session",
                Send.Header),    // VERIFY

            Cmd(HCOM_MDOW_REQUEST_GET_DEVICE_NAME, "device name",
                "Gets the connected Meadow's name (MeadowF7 is default)",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_GET_INITIAL_FILE_BYTES, "file initial",
                "Gets the initial bytes (upto 500).\r\n" +
                "Requires the Meadow file name in 'Text Data' field",
                Send.Text, Uses(text: Req("Meadow file name"))),

            Cmd(HCOM_MDOW_REQUEST_UPLOAD_START_DATA_SEND, "(internal) file read start",
                "Internal used by Meadow to tell CLI it's ready for data to be sent",
                Send.Header, category: Internal),

            Cmd(HCOM_MDOW_REQUEST_UPLOAD_ABORT_DATA_SEND, "(new) file read abort",
                "Cancel the current file being uploaded",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_START_FILE_TRANSFER, "file write",
                "Downloads a file to the Meadow file system",
                Send.F7FileDownload,
                Uses(hostFile: Req("PC file to send"), text: Req("Meadow file name"))),

            Cmd(HCOM_MDOW_REQUEST_DELETE_FILE_BY_NAME, "file delete",
                "Deletes a file by name from the F7 file system",
                Send.F7FileDownload),    // VERIFY

            Cmd(HCOM_MDOW_REQUEST_START_ESP_FILE_TRANSFER, "esp file write",
                "start esp file transfer",
                Send.F7FileDownload),

            Cmd(HCOM_MDOW_REQUEST_UPLOAD_FILE_INIT, "(new) file read (aka upload)",
                "Initiates reading a file from Meadow. Put host's destination pathname " +
                "in 'Host File' field and the Meadow path name in the 'File Name' field",
                Send.Text,
                Uses(hostFile: Req("PC file to save to"), text: Req("Meadow file name"))),

            Cmd(HCOM_MDOW_REQUEST_EXEC_DIAG_APP_CMD, "(new) Send command-line",
                "Only for Ethernet. Sends command (e.g. ping to web site via IP or name. " +
                "All elements must be space separated (e.g. ping abc.xyz).",
                Send.DiagCmd, Uses(text: Req("Command line, e.g. ping abc.xyz"))),

            Cmd(HCOM_MDOW_REQUEST_RTC_SET_TIME_CMD, "(new) Set RTC clock",
                "Needs UTC time in ISO 8601 format (e.g. 2022-03-31T17:34:25-07:00)",
                SendRtcSetTime, Uses()),     // Sends the PC's time

            Cmd(HCOM_MDOW_REQUEST_RTC_READ_TIME_CMD, "(new) Read RTC clock",
                "Reads UTC time in ISO 8601 format (e.g. 2022-03-31T17:34:25-07:00)",
                Send.Text, Uses()),

            Cmd(HCOM_MDOW_REQUEST_RTC_WAKEUP_TIME_CMD, "(new) Set RTC Alarm (wakeup)",
                "Sends ISO 8601 time period (e.g. 3:19 P0Y0M0DT0H3M19S)",
                Send.Text, Uses(text: Req("ISO 8601 period, e.g. P0Y0M0DT0H3M19S"))),

            Cmd(HCOM_MDOW_REQUEST_DEBUGGING_DEBUGGER_DATA, "(internal) debugging data",
                "Identifies to F7 debugging information",
                Send.Header, category: Internal),

            // This replaces the 4 developer items above
            Cmd(HCOM_MDOW_REQUEST_DEVELOPER, "Developer",
                "(new) Diagnostic\r\nExtraData is developer number, UserData contains the value",
                Send.Header, Uses(userData: "Value", extraData: "Developer no.")),

            Cmd(HCOM_MDOW_REQUEST_GET_FILES_AND_FOLDERS, "file list & dev nested",
                "Returns a list of all devices and files",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_QSPI_FLASH_INIT, "qspi init",
                "...",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_QSPI_FLASH_WRITE, "qspi write",
                "For testing only",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_QSPI_FLASH_READ, "qspi read",
                "For testing only",
                Send.Header),

            Cmd(HCOM_MDOW_REQUEST_OTA_REGISTER_DEVICE, "OTA - Register device",
                "For testing only",
                Send.Header),
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

            foreach (var group in All.GroupBy(c => c.rqstType).Where(g => g.Count() > 1))
                yield return $"MeadowCommands: {group.Key} has {group.Count()} entries";
        }
    }
}

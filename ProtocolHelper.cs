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
    // of this application, all messages are "normal."
    // Note: this is not a protocol definition but a convenience for this app
    public enum MessageComplex
    {
        Normal,
        F7FileDnld,        // Send Start, Data & End
        F7FileUpload,      // Recv Start, Data & End
        EspFileDnld,       // Send Start, Data & End
    }

    // One command that can be sent to Meadow (see MeadowCommands)
    public class MeadowRqstData
    {
        public string rqstName;
        public string rqstInfo;
        public MeadowRqstType rqstType;
        public CommandSender sender;
        public CommandFields fields;
        public CommandCategory category;

        public MessageClasses msgClass => sender.MsgClass;

        public MeadowRqstData(string rqstName, string rqstInfo,
            MeadowRqstType rqstType, CommandSender sender, CommandFields fields,
            CommandCategory category)
        {
            this.category = category;
            this.rqstName = rqstName;
            this.rqstInfo = rqstInfo;
            this.rqstType = rqstType;
            this.sender = sender;
            this.fields = fields;
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

    // One message Meadow can send to the host (see HostMessages)
    public class HostRqstData
    {
        public string rqstName;
        public string rqstInfo;
        public HostRqstType rqstType;
        public MessageClasses msgClass;
        public MessageComplex msgComplex;

        public HostRqstData(string rqstName, string rqstInfo,
            HostRqstType rqstType, MessageClasses msgClass, MessageComplex msgComplex)
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
}

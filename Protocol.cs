using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    public class Protocol
    {
        // There is no length field. Since the packet boundaries are delimited and the
        // header is fixed length. Therefore, any additional data length is easily
        // determined.
        public const UInt16 HCOM_PROTOCOL_HCOM_VERSION_NUMBER = 0x0006;

        // COBS needs a specific delimiter. Zero seems to be traditional.
        public const UInt16 HCOM_PROTOCOL_COBS_ENCODING_DELIMITER_VALUE = 0x00;

        // What sequence number is used to identify a non-data message?
        public const UInt16 HCOM_PROTOCOL_NON_DATA_SEQUENCE_NUMBER = 0;

        // Note: while the MD5 hash is 128-bits (16-bytes), it is 32 character
        // hex string from ESP32
        public const UInt16 HCOM_PROTOCOL_COMMAND_MD5_HASH_LENGTH = 32;

        // Define the absolute maximum packet sizes for sent and receive.
        // Note: The length on the wire will be longer because it's encoded.
        public const int HCOM_PROTOCOL_PACKET_MAX_SIZE = 512;
        public const int HCOM_PROTOCOL_ENCODED_MAX_SIZE = HCOM_PROTOCOL_PACKET_MAX_SIZE + 8;

        static public int HcomProtoHdrMessageSize()
        {
            return Marshal.SizeOf(typeof(HcomProtoHdrMessage));
        }

        static public int HcomProtoFSInfoMsgSize()
        {
            return Marshal.SizeOf(typeof(HcomProtoFSInfoMsg));
        }
    }

    //-----------------------------------------------------------------------
    // [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    public struct HcomProtoStdHeader
    {
        // If the sequence number is zero (0), it indicates that this is a non-data
        // message.Non-data messages always contain basic message related
        // information. Most messages fit this category.
        [FieldOffset(0)] public UInt16 seqNumber;

        // The second header field is the 'Version' field. This value is updated for each
        // breaking change to the protocol.
        // The version field is considered a single number which is incremented for each
        // protocol change.
        [FieldOffset(2)] public UInt16 version;

        // The third header field is the 'Request Type' which defines the type of
        // message. Each message type must have a unique request type. These are
        // defined below.
        [FieldOffset(4)] public UInt16 rqstType;

        // The forth header field is called Extra Data. However, it is no longer
        // used and can be considered 'future'.
        [FieldOffset(6)] public UInt16 extraData;

        // The fifth and last header field is the 'User Data' field. This field can
        // be used for any purpose specified by the Request Type.
        [FieldOffset(8)] public UInt32 userData;
    }

    //--------------------------------------------------------------------
    // Header only
    //[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    public struct HcomProtoHdrMessage
    {
        // This is the only thing in a header only message
        [FieldOffset(0)] public HcomProtoStdHeader stdHeader;
    }

    //--------------------------------------------------------------------
    // This diagnostic command message allows HCOM to use code from NSH that
    // is not be able to be built because we are using the protected build.
    // This is initiall done to support ping for networking.
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    public struct HcomProtoDiagCmdMsg
    {
        [FieldOffset(0)] public UInt16 seqNumber;

        [FieldOffset(2)] public UInt16 version;

        [FieldOffset(4)] public UInt16 rqstType;

        [FieldOffset(6)] public UInt16 extraData;

        [FieldOffset(8)] public UInt32 userData;

        // Additional data fields contains the length of the following text
        [FieldOffset(12)] public UInt16 argListLen;

        //[FieldOffset(14)] public byte[] argListText;
    }

    //--------------------------------------------------------------------
    // Header plus File Info
    // Deprecated
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    struct HcomProtoFSInfoMsg
    {
        // If the sequence number is zero (0), it indicates that this is a non-data
        // message.Non-data messages always contain basic message related
        // information. Most messages fit this category.
        [FieldOffset(0)] public UInt16 seqNumber;

        // The second header field is the 'Version' field. This value is updated for each
        // breaking change to the protocol.
        // The version field is considered a single number which is incremented for each
        // protocol change.
        [FieldOffset(2)] public UInt16 version;

        // The third header field is the 'Request Type' which defines the type of
        // message. Each message type must have a unique request type. These are
        // defined below.
        [FieldOffset(4)] public UInt16 rqstType;

        // The forth header field is called Extra Data. However, it is no longer
        // used and can be considered 'future'.
        [FieldOffset(6)] public UInt16 extraData;

        // The fifth and last header field is the 'User Data' field. This field can
        // be used for any purpose specified by the Request Type.
        [FieldOffset(8)] public UInt32 userData;

        // --- The following are not part of the standard header ---
        // File length of the entire file
        [FieldOffset(12)] public UInt32 fileSize;

        // File checksum calculated by sender (not used by ESP32)
        [FieldOffset(16)] public UInt32 fileCheckSum;

        //// File name 
        //// Variable length
        //[FieldOffset(20)] public byte[] fileName;
    }

    //--------------------------------------------------------------------
    // Header plus Text  Info
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    struct HcomProtoTextMessage
    {
        [FieldOffset(0)] public HcomProtoStdHeader stdHeader;

        // Some 'simple' messages contain string information.
        // Note: This is a variable length field
        [FieldOffset(12)] public byte[] textData;
    }

    //--------------------------------------------------------------------
    // Header plus Binary Info
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    struct HcomProtoBinMessage_s
    {
        [FieldOffset(0)] HcomProtoStdHeader stdHeader;

        // Additional binary information for debugging or file data
        // Note: This is a variable length field
        [FieldOffset(12)] public Byte[] binData;
    }

    //--------------------------------------------------------------------------
    // HCOM Protocol message type definitions
    //--------------------------------------------------------------------------
    public enum MajorTypes : UInt16
    {
        // PeterM-When the time comes the following Major types should reflect the
        // name of the above structure is used to send it. 
        HCOM_PROTOCOL_HEADER_UNDEFINED_TYPE = 0x0000,

        // The header of all mesasges include a 4-byte field called user data. The
        // User data field's meaning is determined by the message type

        // Header only request types,
        HCOM_PROTOCOL_HEADER_ONLY_TYPE = 0x0100,

        // File related types includes 4-byte user data (used for the destination
        // partition id), 4-byte file size, 4-byte checksum, 4-byte destination address
        // and variable length destination file name. Note: The  4-byte destination address
        // is currently only used for the STM32F7 to ESP32 downloads.
        HCOM_PROTOCOL_HEADER_FILE_START_TYPE = 0x0200,

        // Simple text is a header followed by text without a terminating NULL.
        HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE = 0x0300,

        // Simple binary is a header followed by binary data. The size of the data
        // can be up to HCOM_PROTOCOL_PACKET_MAX_SIZE minus header size
        HCOM_PROTOCOL_HEADER_SIMPLE_BINARY_TYPE = 0x0400,
    }

    // Messages sent from host to Meadow 
    public enum MeadowRqstType : UInt16
    {
        HCOM_MDOW_REQUEST_UNDEFINED_REQUEST = 0x00 | MajorTypes.HCOM_PROTOCOL_HEADER_UNDEFINED_TYPE,

        // No longer supported
        // HCOM_MDOW_REQUEST_CREATE_ENTIRE_FLASH_FS  = 0x01 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_CHANGE_TRACE_LEVEL = 0x02 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_FORMAT_FLASH_FILE_SYS = 0x03 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_END_FILE_TRANSFER = 0x04 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_RESTART_PRIMARY_MCU = 0x05 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_VERIFY_ERASED_FLASH = 0x06 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        // No longer supported
        // HCOM_MDOW_REQUEST_PARTITION_FLASH_FS      = 0x07 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        // No longer supported
        // HCOM_MDOW_REQUEST_MOUNT_FLASH_FS          = 0x08 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        // No longer supported
        // HCOM_MDOW_REQUEST_INITIALIZE_FLASH_FS     = 0x09 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_BULK_FLASH_ERASE = 0x0a | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_ENTER_DFU_MODE = 0x0b | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_ENABLE_DISABLE_NSH = 0x0c | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_LIST_PARTITION_FILES = 0x0d | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_LIST_PART_FILES_AND_CRC = 0x0e | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_MONO_DISABLE = 0x0f | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_MONO_ENABLE = 0x10 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_MONO_RUN_STATE = 0x11 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_GET_DEVICE_INFORMATION = 0x12 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_PART_RENEW_FILE_SYS = 0x13 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_NO_TRACE_TO_HOST = 0x14 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_SEND_TRACE_TO_HOST = 0x15 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_END_ESP_FILE_TRANSFER = 0x16 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_READ_ESP_MAC_ADDRESS = 0x17 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_RESTART_ESP32 = 0x18 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_MONO_FLASH = 0x19 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_SEND_TRACE_TO_UART = 0x1a | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_NO_TRACE_TO_UART = 0x1b | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,

        // >>> Breaking protocol change.
        // ToDo: This message is miscategorized should be MajorTypes.HCOM_PROTOCOL_HEADER_FILE_START_TYPE
        // like HCOM_MDOW_REQUEST_START_FILE_TRANSFER.
        HCOM_MDOW_REQUEST_MONO_UPDATE_RUNTIME = 0x1c | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_MONO_UPDATE_FILE_END = 0x1d | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_MONO_START_DBG_SESSION = 0x1e | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_GET_DEVICE_NAME = 0x1f | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,

        // >>> Breaking protocol change.
        // ToDo: This message is miscategorized should be MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE
        // since it is a header followed by text (the file name)
        HCOM_MDOW_REQUEST_GET_INITIAL_FILE_BYTES = 0x20 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_UPLOAD_START_DATA_SEND = 0x21 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_UPLOAD_ABORT_DATA_SEND = 0x22 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,

        // The file types have the optional data field defined for sending file information
        HCOM_MDOW_REQUEST_START_FILE_TRANSFER = 0x01 | MajorTypes.HCOM_PROTOCOL_HEADER_FILE_START_TYPE,
        HCOM_MDOW_REQUEST_DELETE_FILE_BY_NAME = 0x02 | MajorTypes.HCOM_PROTOCOL_HEADER_FILE_START_TYPE,
        HCOM_MDOW_REQUEST_START_ESP_FILE_TRANSFER = 0x03 | MajorTypes.HCOM_PROTOCOL_HEADER_FILE_START_TYPE,

        // These message are a header followed by text, one contains the texts length too
        HCOM_MDOW_REQUEST_UPLOAD_FILE_INIT = 0x01 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_MDOW_REQUEST_EXEC_DIAG_APP_CMD = 0x02 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,

        // This is a simple type with binary data
        HCOM_MDOW_REQUEST_DEBUGGING_DEBUGGER_DATA = 0x01 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_BINARY_TYPE,

        // Only used for testing
        HCOM_MDOW_REQUEST_DEVELOPER_1 = 0xf0 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_DEVELOPER_2 = 0xf1 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_DEVELOPER_3 = 0xf2 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_DEVELOPER_4 = 0xf3 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        // Testing QSPI flash
        HCOM_MDOW_REQUEST_QSPI_FLASH_INIT = 0xf4 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_QSPI_FLASH_WRITE = 0xf5 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
        HCOM_MDOW_REQUEST_QSPI_FLASH_READ = 0xf6 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,
    }

    //============================================================================
    // Messages sent from meadow to host
    public enum HostRqstType : UInt16
    {
        HCOM_HOST_REQUEST_UNDEFINED_REQUEST = 0x00 | MajorTypes.HCOM_PROTOCOL_HEADER_UNDEFINED_TYPE,

        // Only header
        HCOM_HOST_REQUEST_UPLOAD_FILE_COMPLETED = 0x01 | MajorTypes.HCOM_PROTOCOL_HEADER_ONLY_TYPE,

        // Simple with some text message
        HCOM_HOST_REQUEST_TEXT_REJECTED = 0x01 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_ACCEPTED = 0x02 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_CONCLUDED = 0x03 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_ERROR = 0x04 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_INFORMATION = 0x05 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_LIST_HEADER = 0x06 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_LIST_MEMBER = 0x07 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_CRC_MEMBER = 0x08 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_MONO_STDOUT = 0x09 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_DEVICE_INFO = 0x0A | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_TRACE_MSG = 0x0B | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_RECONNECT = 0x0C | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_TEXT_MONO_STDERR = 0x0D | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,

        HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY = 0x0E | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL = 0x0F | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,

        HCOM_HOST_REQUEST_INIT_UPLOAD_OKAY = 0x10 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,
        HCOM_HOST_REQUEST_INIT_UPLOAD_FAIL = 0x11 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_TEXT_TYPE,

        // Simple with mono debug data
        HCOM_HOST_REQUEST_DEBUGGING_MONO_DATA = 0x01 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_BINARY_TYPE,
        HCOM_HOST_REQUEST_SEND_INITIAL_FILE_BYTES = 0x02 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_BINARY_TYPE,
        HCOM_HOST_REQUEST_UPLOADING_FILE_DATA = 0x03 | MajorTypes.HCOM_PROTOCOL_HEADER_SIMPLE_BINARY_TYPE,
    }
}

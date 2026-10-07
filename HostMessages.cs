using System;
using System.Collections.Generic;
using System.Linq;
using static HcomDiagUi.HostRqstType;

namespace HcomDiagUi
{
    //==========================================================================
    // Every message Meadow can send to the host. MessageClasses decides how it
    // is displayed and MessageComplex which processor it is routed to (see
    // CommsManager.ForwardToProcessMsgs).
    public static class HostMessages
    {
        static HostRqstData Msg(HostRqstType rqstType, string name, string info,
            MessageClasses msgClass, MessageComplex msgComplex)
        {
            return new HostRqstData(name, info, rqstType, msgClass, msgComplex);
        }

        public static readonly HostRqstData[] All =
        {
            Msg(HCOM_HOST_REQUEST_UNDEFINED_REQUEST, "Undefined request", "...",
                MessageClasses.None, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_UPLOAD_FILE_COMPLETED, "(new-internal) F7 Uploading is finished", "...",
                MessageClasses.Header, MessageComplex.F7FileUpload),
            Msg(HCOM_HOST_REQUEST_TEXT_REJECTED, "Request Rejected (not implemented)", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_ACCEPTED, "Request Accepted", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_CONCLUDED, "Previous Request Concluded", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_ERROR, "Error Message", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_INFORMATION, "Information Message", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_LIST_HEADER, "File List Header", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_LIST_MEMBER, "File List member", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_CRC_MEMBER, "File List member with CRC", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_MONO_STDOUT, "Mono stdout Text", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_DEVICE_INFO, "Device Info Text", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_TRACE_MSG, "Trace Msg Text", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_RECONNECT, "Text Reconnect Request", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_TEXT_MONO_STDERR, "Mono stderr Text", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_DEBUGGING_MONO_DATA, "Debugging mono data", "...",
                MessageClasses.Binary, MessageComplex.Normal),
            // Reply to 'file initial'. Meadow.CLI decodes it as text (ReadFileString)
            Msg(HCOM_HOST_REQUEST_SEND_INITIAL_FILE_BYTES, "Initial File Bytes", "...",
                MessageClasses.Text, MessageComplex.Normal),
            Msg(HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY, "Download Initialization Okay", "...",
                MessageClasses.Text, MessageComplex.F7FileDnld),
            Msg(HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL, "Download Initialization Failed", "...",
                MessageClasses.Header, MessageComplex.F7FileDnld),
            Msg(HCOM_HOST_REQUEST_INIT_UPLOAD_OKAY, "(new-internal) Upload Initialization Okay", "...",
                MessageClasses.Text, MessageComplex.F7FileUpload),
            Msg(HCOM_HOST_REQUEST_INIT_UPLOAD_FAIL, "(new-internal) Upload Initialization Failed", "...",
                MessageClasses.Header, MessageComplex.F7FileUpload),
            Msg(HCOM_HOST_REQUEST_UPLOADING_FILE_DATA, "Uploading file data", "...",
                MessageClasses.Binary, MessageComplex.F7FileUpload),
            Msg(HCOM_HOST_REQUEST_DNLD_FAIL_RESEND, "Download failed and timer", "...",
                MessageClasses.FileInfo, MessageComplex.F7FileDnld),
            // Reply to 'OTA - Register device' (hcom_ota.c). Meadow.CLI decodes it as text
            Msg(HCOM_HOST_REQUEST_DEVICE_PUBLIC_KEY, "Device Public Key", "...",
                MessageClasses.Text, MessageComplex.Normal),
            // 'Entering low-power mode', sent just before Stop mode (pwrmgmt_control.c)
            Msg(HCOM_HOST_REQUEST_TEXT_NEXT_LOW_PWR, "Entering Low Power", "...",
                MessageClasses.Text, MessageComplex.Normal),
        };

        static readonly Dictionary<HostRqstType, HostRqstData> _byType =
            All.GroupBy(m => m.rqstType).ToDictionary(g => g.Key, g => g.First());

        public static HostRqstData Find(HostRqstType rqstType)
        {
            if (_byType.TryGetValue(rqstType, out HostRqstData message))
                return message;

            return new HostRqstData("Host Request Type not found", "...", rqstType,
                MessageClasses.None, MessageComplex.Normal);
        }

        // Problems with the table, e.g. a HostRqstType value without an entry.
        // Reported in the Status box at startup.
        public static IEnumerable<string> Validate()
        {
            foreach (HostRqstType rqstType in Enum.GetValues(typeof(HostRqstType)))
            {
                if (!_byType.ContainsKey(rqstType))
                    yield return $"HostMessages: no entry for {rqstType}";
            }

            foreach (var group in All.GroupBy(m => m.rqstType).Where(g => g.Count() > 1))
                yield return $"HostMessages: {group.Key} has {group.Count()} entries";
        }
    }
}

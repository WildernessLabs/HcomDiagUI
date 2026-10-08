using System;
using System.Collections.Generic;
using System.Linq;
using static HcomDiagUi.HostRqstType;

namespace HcomDiagUi
{
    //==========================================================================
    // Every message Meadow can send to the host, with the class that decides
    // how it is displayed. Which processor handles it is decided by the
    // processors themselves (IHostMessageProcessor.Handles).
    public static class HostMessages
    {
        static HostRqstData Msg(HostRqstType rqstType, string name, string info,
            MessageClasses msgClass)
        {
            return new HostRqstData(name, info, rqstType, msgClass);
        }

        public static readonly HostRqstData[] All =
        {
            Msg(HCOM_HOST_REQUEST_UNDEFINED_REQUEST, "Undefined request", "...",
                MessageClasses.None),
            Msg(HCOM_HOST_REQUEST_UPLOAD_FILE_COMPLETED, "(new-internal) F7 Uploading is finished", "...",
                MessageClasses.Header),
            Msg(HCOM_HOST_REQUEST_TEXT_REJECTED, "Request Rejected (not implemented)", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_ACCEPTED, "Request Accepted", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_CONCLUDED, "Previous Request Concluded", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_ERROR, "Error Message", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_INFORMATION, "Information Message", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_LIST_HEADER, "File List Header", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_LIST_MEMBER, "File List member", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_CRC_MEMBER, "File List member with CRC", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_MONO_STDOUT, "Mono stdout Text", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_DEVICE_INFO, "Device Info Text", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_TRACE_MSG, "Trace Msg Text", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_RECONNECT, "Text Reconnect Request", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_TEXT_MONO_STDERR, "Mono stderr Text", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_DEBUGGING_MONO_DATA, "Debugging mono data", "...",
                MessageClasses.Binary),
            // Reply to 'file initial'. Meadow.CLI decodes it as text (ReadFileString)
            Msg(HCOM_HOST_REQUEST_SEND_INITIAL_FILE_BYTES, "Initial File Bytes", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY, "Download Initialization Okay", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_INIT_DOWNLOAD_FAIL, "Download Initialization Failed", "...",
                MessageClasses.Header),
            Msg(HCOM_HOST_REQUEST_INIT_UPLOAD_OKAY, "(new-internal) Upload Initialization Okay", "...",
                MessageClasses.Text),
            Msg(HCOM_HOST_REQUEST_INIT_UPLOAD_FAIL, "(new-internal) Upload Initialization Failed", "...",
                MessageClasses.Header),
            Msg(HCOM_HOST_REQUEST_UPLOADING_FILE_DATA, "Uploading file data", "...",
                MessageClasses.Binary),
            Msg(HCOM_HOST_REQUEST_DNLD_FAIL_RESEND, "Download failed and timer", "...",
                MessageClasses.FileInfo),
            // Reply to 'OTA - Register device' (hcom_ota.c). Meadow.CLI decodes it as text
            Msg(HCOM_HOST_REQUEST_DEVICE_PUBLIC_KEY, "Device Public Key", "...",
                MessageClasses.Text),
            // 'Entering low-power mode', sent just before Stop mode (pwrmgmt_control.c)
            Msg(HCOM_HOST_REQUEST_TEXT_NEXT_LOW_PWR, "Entering Low Power", "...",
                MessageClasses.Text),
        };

        static readonly Dictionary<HostRqstType, HostRqstData> _byType =
            All.GroupBy(m => m.rqstType).ToDictionary(g => g.Key, g => g.First());

        public static HostRqstData Find(HostRqstType rqstType)
        {
            if (_byType.TryGetValue(rqstType, out HostRqstData message))
                return message;

            return new HostRqstData("Host Request Type not found", "...", rqstType,
                MessageClasses.None);
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

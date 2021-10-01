using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    public class UiSupport
    {
        HcomDiagUiForm _hcomUiForm;

        public bool RecvHexChecked { get; internal set; }

        // Constructor
        public UiSupport(HcomDiagUiForm hcomUiForm)
        {
            _hcomUiForm = hcomUiForm;
        }

        internal void PopulateCommandListBox()
        {
            UInt16[] requestTypes = (ushort[])Enum.GetValues(typeof(MeadowRqstType));

            _hcomUiForm.CommandListBox.BeginUpdate();
            foreach (MeadowRqstType rqstType in requestTypes)
            {
                // Use the request type to find the command name and command information
                MeadowRqstData meadowRqstData = ProtocolHelper.DecodeMsgToMeadow(rqstType);
                _hcomUiForm.CommandListBox.Items.Add(meadowRqstData);
            }
            _hcomUiForm.CommandListBox.EndUpdate();
        }

        public string TextForMsg()
        {
            return _hcomUiForm.TextForMsg;
        }

        // These support the UI
        //---------------------------------------------
        public void WriteDebugLine(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now:HH:mm:ss.fff} debug-'{text}'");
            _hcomUiForm.WriteStatusSafe(sb.ToString());
        }

        //---------------------------------------------
        public void WriteStatus(string text)
        {
            _hcomUiForm.WriteStatusSafe(text);
        }

        public void WriteStatusTime(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{DateTime.Now:HH:mm:ss.fff} {text}");
            _hcomUiForm.WriteStatusSafe(sb.ToString());
        }

        public void WriteStatusLine(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now:HH:mm:ss.fff} {text}");
            _hcomUiForm.WriteStatusSafe(sb.ToString());
        }

        //---------------------------------------------
        public void WriteSendReceive(string text)
        {
            _hcomUiForm.WriteSendReceiveSafe(text);
        }

        public void WriteSendReceiveTime(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{DateTime.Now:HH:mm:ss.fff} {text}");
            _hcomUiForm.WriteSendReceiveSafe(sb.ToString());
        }

        public void WriteSendReceiveTimeLine(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now:HH:mm:ss.fff} {text}");
            _hcomUiForm.WriteSendReceiveSafe(sb.ToString());
        }

        public void WriteSendReceiveLine(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{text}");
            _hcomUiForm.WriteSendReceiveSafe(sb.ToString());
        }

        public string GetHostFileInfo()
        {
            return _hcomUiForm.ReadHostFileInfo();
        }

        //---------------------------------------------
        public void WriteCmdClass(string text)
        {
            _hcomUiForm.WriteMsgClassSafe(text);
        }

        //---------------------------------------------
        public void WriteCmdSel(string text)
        {
            _hcomUiForm.WriteCmdSelSafe(text);
        }

        //public void WriteCmdSelLine(string text)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine(text);
        //    _hcomUiForm.WriteCmdSelSafe(sb.ToString());
        //}

        //---------------------------------------------
        public void WriteCmdValue(string text)
        {
            _hcomUiForm.WriteCmdValueSafe(text);
        }

        //public void WriteCmdValueLine(string text)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine(text);
        //    _hcomUiForm.WriteCmdValueSafe(sb.ToString());
        //}

        //---------------------------------------------
        public void WriteCmdInfo(string text)
        {
            _hcomUiForm.WriteCmdInfoSafe(text);
        }

        public void WriteCmdInfoLine(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(text);
            _hcomUiForm.WriteCmdInfoSafe(sb.ToString());
        }

        internal void ShowRecvHex(bool @checked)
        {
            RecvHexChecked = @checked;
        }
    }
}

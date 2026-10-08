using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    public class UiSupport
    {
        HcomDiagUiForm _hcomUiForm;
        private Stopwatch stopwatch;

        public bool RecvHexChecked { get; internal set; }
        public Stopwatch Stopwatch { get => stopwatch; set => stopwatch = value; }

        // Constructor
        public UiSupport(HcomDiagUiForm hcomUiForm)
        {
            _hcomUiForm = hcomUiForm;
            Stopwatch = new Stopwatch();
        }

        // Report table mistakes, e.g. a new MeadowRqstType without an entry
        internal void ReportCommandTableProblems()
        {
            foreach (string problem in MeadowCommands.Validate().Concat(HostMessages.Validate()))
                WriteStatusLine($"Command table error - {problem}");
        }

        // Internal and Unsupported commands are only listed when showHidden is
        // true. The selected command stays selected if it is still listed.
        internal void PopulateCommandListBox(bool showHidden)
        {
            var listBox = _hcomUiForm.CommandListBox;
            object selected = listBox.SelectedItem;

            listBox.BeginUpdate();
            listBox.Items.Clear();
            foreach (MeadowRqstData command in MeadowCommands.All)
            {
                if (showHidden || command.category == CommandCategory.Normal)
                    listBox.Items.Add(command);
            }

            if (selected != null && listBox.Items.Contains(selected))
                listBox.SelectedItem = selected;
            listBox.EndUpdate();
        }

        public string TextForMsg()
        {
            return _hcomUiForm.TextForMsg;
        }

        //---------------------------------------------
        public void WriteSendReps(int sendRepetitions)
        {
            _hcomUiForm.WriteSendRepsSafe(sendRepetitions.ToString());
        }

        //---------------------------------------------
        public void WriteSendCount(int sendCount)
        {
            _hcomUiForm.WriteSendCountSafe(sendCount.ToString());
        }

        //---------------------------------------------
        public int SendIterations()
        {
            return _hcomUiForm.SendIterations;
        }

        // These support the UI
        //---------------------------------------------
        public void WriteDebugLine(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now:HH:mm:ss.fff} debug-'{text}'");
            _hcomUiForm.WriteStatusSafe(sb.ToString());
        }

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
        public void WriteSendRecv(string text)
        {
            _hcomUiForm.WriteSendRecvSafe(text);
        }

        // Iteration of the command being sent, shown as [n] after the timestamp
        // of Send/Received lines; 0 = don't show (a single iteration)
        public int CurrentIteration
        {
            get { return _currentIteration; }
            set { _currentIteration = value; }
        }
        volatile int _currentIteration;

        string TimeAndIteration()
        {
            int iteration = _currentIteration;
            return iteration > 0
                ? $"{DateTime.Now:HH:mm:ss.fff} [{iteration}] "
                : $"{DateTime.Now:HH:mm:ss.fff} ";
        }

        public void WriteSendRecvTime(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{TimeAndIteration()}{text}");
            _hcomUiForm.WriteSendRecvSafe(sb.ToString());
        }

        public void WriteSendRecvTimeLine(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{TimeAndIteration()}{text}");
            _hcomUiForm.WriteSendRecvSafe(sb.ToString());
        }

        public void WriteExecutionTime()
        {
            _hcomUiForm.WriteExecTimeSafe($"{Stopwatch.Elapsed}");
        }

        public void ClearExecutionTime()
        {
            _hcomUiForm.WriteExecTimeSafe("Working");
        }

        public void WriteSendRecvLine(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{text}");
            _hcomUiForm.WriteSendRecvSafe(sb.ToString());
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

        //---------------------------------------------
        public void SendButtonEnable(bool enabled)
        {
            _hcomUiForm.sendButtonEnabled(enabled);
        }
    }
}

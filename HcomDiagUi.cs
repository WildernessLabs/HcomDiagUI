using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace HcomDiagUi
{
    public partial class HcomDiagUiForm : Form
    {
        // Define a delegate
        private delegate void SafeCallDelegate(string text);
        private delegate void SafeCallDelegateBool(bool enabled);
        CommsManager _commsManager;
        readonly UiSupport _uiSupport;

        public UiSupport UiSupport => _uiSupport;
        public ListBox CommandListBox => commandListBox;

        public string TextForMsg => this.textForMsgTextBox.Text;
        public int SendIterations => int.Parse(this.sendIterationsTextBox.Text);

        // Send is only allowed while connected and no send is in progress
        bool _connected;
        bool _reconnecting;
        bool _sendBusy;

        // Designer label texts, shown for commands whose field use isn't
        // specified in MeadowCommands
        readonly string _userDataLabel;
        readonly string _extraDataLabel;
        readonly string _hostFileLabel;
        readonly string _textLabel;

        public HcomDiagUiForm()
        {
            InitializeComponent();

            _userDataLabel = label3.Text;
            _extraDataLabel = label8.Text;
            _hostFileLabel = label7.Text;
            _textLabel = label2.Text;

            // Set before _uiSupport exists, so CheckedChanged doesn't fill the list
            showHiddenCommandsCheckBox.Checked = Properties.Settings.Default.ShowHiddenCommands;

            _uiSupport = new UiSupport(this);
            _uiSupport.ReportCommandTableProblems();
            _uiSupport.PopulateCommandListBox(showHiddenCommandsCheckBox.Checked);
            _commsManager = new CommsManager(this);

            RefreshPortList(Properties.Settings.Default.LastComPort);
            ShowConnectionState();
        }

        //---------------------------------------------------------
        // Reconnect to the port used last time, if it is present
        private void HcomDiagUiForm_Shown(object sender, EventArgs e)
        {
            string lastPort = Properties.Settings.Default.LastComPort;
            if (!string.IsNullOrEmpty(lastPort) &&
                comPortComboBox.Items.Cast<string>().Contains(lastPort, StringComparer.OrdinalIgnoreCase))
            {
                ConnectToSelectedPort();
            }
            else
            {
                _uiSupport.WriteStatusLine("Not connected. Select a Com Port and press Connect.");
            }
        }

        private void HcomDiagUiForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _commsManager.Disconnect();
        }

        //---------------------------------------------------------
        // Fill the port list from the ports Windows currently reports.
        // preferredPort is selected if present, otherwise the current entry
        // is kept. The combo box stays editable so an unlisted port name can
        // still be typed in.
        private void RefreshPortList(string preferredPort = null)
        {
            string wanted = string.IsNullOrEmpty(preferredPort) ? comPortComboBox.Text : preferredPort;

            // Sort COM3 before COM10
            var ports = SerialPort.GetPortNames()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p.Length)
                .ThenBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            comPortComboBox.BeginUpdate();
            comPortComboBox.Items.Clear();
            comPortComboBox.Items.AddRange(ports);
            comPortComboBox.EndUpdate();

            int index = Array.FindIndex(ports, p => string.Equals(p, wanted, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
                comPortComboBox.SelectedIndex = index;
            else if (!string.IsNullOrEmpty(wanted))
                comPortComboBox.Text = wanted;
            else if (ports.Length > 0)
                comPortComboBox.SelectedIndex = 0;
        }

        private void refreshPortsButton_Click(object sender, EventArgs e)
        {
            RefreshPortList();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            // While reconnecting the button reads "Stop"; Disconnect stops it
            if (_connected || _reconnecting)
                _commsManager.Disconnect();
            else
                ConnectToSelectedPort();
        }

        private void ConnectToSelectedPort()
        {
            string portName = comPortComboBox.Text.Trim();
            if (portName.Length == 0)
            {
                _uiSupport.WriteStatusLine("PLEASE SELECT A COM PORT");
                return;
            }

            if (_commsManager.Connect(portName))
            {
                Properties.Settings.Default.LastComPort = portName;
                Properties.Settings.Default.Save();
            }
        }

        //---------------------------------------------------------
        // Called by CommsManager from any thread when the connection may have
        // changed. BeginInvoke (not Invoke) because CommsManager may call this
        // while the UI thread is waiting on one of its locks. The state is
        // read when the update runs, so a late update can't show stale state.
        internal void ShowConnectionStateSafe()
        {
            if (InvokeRequired)
            {
                if (IsHandleCreated && !IsDisposed)
                    BeginInvoke(new MethodInvoker(ShowConnectionState));
            }
            else
            {
                ShowConnectionState();
            }
        }

        private void ShowConnectionState()
        {
            bool connected = _commsManager.IsConnected;
            bool reconnecting = !connected && _commsManager.IsReconnecting;

            _connected = connected;
            _reconnecting = reconnecting;
            connectButton.Text = connected ? "Disconnect" : reconnecting ? "Stop" : "Connect";
            comPortComboBox.Enabled = !connected && !reconnecting;
            refreshPortsButton.Enabled = !connected && !reconnecting;
            UpdateSendButton();
        }

        private void UpdateSendButton()
        {
            sendButton.Enabled = _connected && !_sendBusy;
        }

        //internal string ReadTextInformation()
        //{
        //    return this.comPortTextBox.Text;
        //}

        //---------------------------------------------------------
        // Current number of iterations to be executed
        internal void WriteSendRepsSafe(string text)
        {
            if (sendIterationsTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteSendRepsSafe);
                sendIterationsTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                sendIterationsTextBox.Clear();
                sendIterationsTextBox.AppendText(text);
            }
        }

        //---------------------------------------------------------
        // Current number of iterations currently being executed
        internal void WriteSendCountSafe(string text)
        {
            if (sendCountTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteSendCountSafe);
                sendCountTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                sendCountTextBox.Clear();
                sendCountTextBox.AppendText(text);
            }
        }

        //---------------------------------------------------------
        internal void WriteMsgClassSafe(string text)
        {
            if (msgClassTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteMsgClassSafe);
                msgClassTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                msgClassTextBox.Text = text;
            }
        }

        //---------------------------------------------------------
        // Status, Receive and Send provide runtime status
        internal void WriteStatusSafe(string text)
        {
            if (statusTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteStatusSafe);
                statusTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                statusTextBox.AppendText(text);
            }
        }

        //---------------------------------------------------------
        // Execution Time of last command
        internal void WriteExecTimeSafe(string text)
        {
            if (execTimeTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteExecTimeSafe);
                execTimeTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                execTimeTextBox.Clear();
                execTimeTextBox.AppendText(text);
            }
        }

        //---------------------------------------------------------
        // This call uses the UI thread for the textbox
        internal void WriteSendRecvSafe(string text)
        {
            if (sendRecvTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteSendRecvSafe);
                sendRecvTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                sendRecvTextBox.AppendText(text);
            }
        }

        internal string ReadHostFileInfo()
        {
            return HostFileTextBox.Text;
        }

        //----------------------------------------------------------
        // The following provide information about the message being sent
        internal void WriteCmdSelSafe(string text)
        {
            if (cmdSelTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteCmdSelSafe);
                cmdSelTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                cmdInfoTextBox.Clear();
                cmdSelTextBox.Text = text;
            }
        }

        internal void WriteCmdValueSafe(string text)
        {
            if (cmdValueTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteCmdValueSafe);
                cmdValueTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                cmdInfoTextBox.Clear();
                cmdValueTextBox.Text = text;
            }
        }

        internal void WriteCmdInfoSafe(string text)
        {
            if (cmdInfoTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteCmdInfoSafe);
                cmdInfoTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                cmdInfoTextBox.Clear();
                cmdInfoTextBox.Text += text;
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (commandListBox.SelectedItem == null)
            {
                statusTextBox.AppendText("PLEASE SELECT A COMMAND");
                return;
            }

            if (!InputFieldsValid((MeadowRqstData)commandListBox.SelectedItem))
                return;

            _uiSupport.SendButtonEnable(false);

            // Tell the communication manager we have work to do
            _commsManager.BuildMsgToSend();

            // _uiSupport.WriteDebugLine("==> UI Thread back home.");
        }

        private void showHiddenCommandsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_uiSupport == null)
                return;     // Still in the constructor

            _uiSupport.PopulateCommandListBox(showHiddenCommandsCheckBox.Checked);
            Properties.Settings.Default.ShowHiddenCommands = showHiddenCommandsCheckBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void commandListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Refilling the list (Show internal/unsupported) can clear the selection
            if (commandListBox.SelectedItem == null)
                return;

            MeadowRqstData rqstInfo = (MeadowRqstData)commandListBox.SelectedItem;
            _uiSupport.WriteCmdSel(rqstInfo.rqstName);
            _uiSupport.WriteCmdValue(rqstInfo.GetHexValue());
            _uiSupport.WriteCmdInfo(rqstInfo.rqstInfo);
            _uiSupport.WriteCmdClass(rqstInfo.msgClass.ToString());
            ShowFieldLabels(rqstInfo.fields);
        }

        //---------------------------------------------------------
        // Label the input fields with what the selected command does with them.
        // The User/Extra Data labels sit in a narrow column, so they show only
        // the meaning; Host File and the Meadow text field have room for more.
        private void ShowFieldLabels(CommandFields fields)
        {
            if (!fields.Specified)
            {
                label3.Text = _userDataLabel;
                label8.Text = _extraDataLabel;
                label7.Text = _hostFileLabel;
                label2.Text = _textLabel;
                return;
            }

            label3.Text = fields.UserData.Used ? fields.UserData.Label : "(not used)";
            label8.Text = fields.ExtraData.Used ? fields.ExtraData.Label : "(not used)";
            label7.Text = WideLabel("Host File", fields.HostFile);
            label2.Text = WideLabel("Meadow", fields.Text);
        }

        private static string WideLabel(string fieldName, FieldUse use)
        {
            if (!use.Used)
                return $"{fieldName} (not used)";

            return use.Required ? $"{fieldName}: {use.Label} (required)" : $"{fieldName}: {use.Label}";
        }

        //---------------------------------------------------------
        // Check the input fields before sending. Problems are reported in the
        // Status box and nothing is sent.
        private bool InputFieldsValid(MeadowRqstData command)
        {
            bool valid = true;

            // CommsManager parses these; a non-number used to crash the app
            if (!UInt32.TryParse(userDataTextBox.Text.Trim(), out _))
            {
                _uiSupport.WriteStatusLine($"User Data must be a number from 0 to {UInt32.MaxValue}");
                valid = false;
            }

            if (!UInt16.TryParse(extraDataTextBox.Text.Trim(), out _))
            {
                _uiSupport.WriteStatusLine($"Extra Data must be a number from 0 to {UInt16.MaxValue}");
                valid = false;
            }

            CommandFields fields = command.fields;
            if (fields.Text.Required && string.IsNullOrWhiteSpace(textForMsgTextBox.Text))
            {
                _uiSupport.WriteStatusLine($"'{command.rqstName}' not sent: the Meadow text field is required ({fields.Text.Label})");
                valid = false;
            }

            if (fields.HostFile.Required && string.IsNullOrWhiteSpace(HostFileTextBox.Text))
            {
                _uiSupport.WriteStatusLine($"'{command.rqstName}' not sent: the Host File field is required ({fields.HostFile.Label})");
                valid = false;
            }

            return valid;
        }

        private void showRecvHexCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _uiSupport.ShowRecvHex(showRecvHexCheckBox.Checked);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            _commsManager.SetCancelSending();
        }

        // Prevent double clicking
        public void sendButtonEnabled(bool sendEnabled)
        {
            if (sendButton.InvokeRequired)
            {
                // A send released by Disconnect() while the form is closing
                if (IsDisposed || !IsHandleCreated)
                    return;

                var d = new SafeCallDelegateBool(sendButtonEnabled);
                sendButton.Invoke(d, new object[] { sendEnabled });
            }
            else
            {
                _sendBusy = !sendEnabled;
                UpdateSendButton();
            }
        }
    }
}

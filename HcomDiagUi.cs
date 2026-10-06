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
        bool _sendBusy;

        public HcomDiagUiForm()
        {
            InitializeComponent();

            _uiSupport = new UiSupport(this);
            _uiSupport.PopulateCommandListBox();
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
            if (_connected)
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

            _connected = connected;
            connectButton.Text = connected ? "Disconnect" : "Connect";
            comPortComboBox.Enabled = !connected;
            refreshPortsButton.Enabled = !connected;
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

            _uiSupport.SendButtonEnable(false);

            // Tell the communication manager we have work to do
            _commsManager.BuildMsgToSend();

            // _uiSupport.WriteDebugLine("==> UI Thread back home.");
        }

        private void commandListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MeadowRqstData rqstInfo = (MeadowRqstData)commandListBox.SelectedItem;
            _uiSupport.WriteCmdSel(rqstInfo.rqstName);
            _uiSupport.WriteCmdValue(rqstInfo.GetHexValue());
            _uiSupport.WriteCmdInfo(rqstInfo.rqstInfo);
            _uiSupport.WriteCmdClass(rqstInfo.msgClass.ToString());
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HcomDiagUi
{
    public partial class HcomDiagUiForm : Form
    {
        // Define a delegate
        private delegate void SafeCallDelegate(string text);
        CommsManager _commsManager;
        readonly UiSupport _uiSupport;

        public UiSupport UiSupport => _uiSupport;
        public ListBox CommandListBox => commandListBox;

        public string TextForMsg => this.textForMsgTextBox.Text;

        public HcomDiagUiForm()
        {
            InitializeComponent();
            
            _uiSupport = new UiSupport(this);
            _uiSupport.PopulateCommandListBox();
            _commsManager = new CommsManager(this);
        }

        private void comPortTextBox_TextChanged(object sender, EventArgs e)
        {
            _commsManager.UpdateSerialPortName();
        }

        internal string ReadPortName()
        {
            return this.comPortTextBox.Text;
        }

        internal string ReadTextInformation()
        {
            return this.comPortTextBox.Text;
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

        internal void WriteSendReceiveSafe(string text)
        {
            if (sendRecvTextBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteSendReceiveSafe);
                sendRecvTextBox.Invoke(d, new object[] { text });
            }
            else
            {
                sendRecvTextBox.AppendText(text);
            }
        }

        internal string ReadHostFileName()
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
            if(commandListBox.SelectedItem == null)
            {
                statusTextBox.AppendText("PLEASE SELECT A COMMAND");
                return;
            }

            MeadowRqstData rqstInfo = (MeadowRqstData)commandListBox.SelectedItem;
            UInt32 userData = UInt32.Parse(userDataTextBox.Text);
            _commsManager.BuildMsgToSend(rqstInfo, userData);
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
    }
}

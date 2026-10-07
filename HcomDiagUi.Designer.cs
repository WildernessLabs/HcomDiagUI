
namespace HcomDiagUi
{
    partial class HcomDiagUiForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            sendButton = new System.Windows.Forms.Button();
            comPortComboBox = new System.Windows.Forms.ComboBox();
            refreshPortsButton = new System.Windows.Forms.Button();
            connectButton = new System.Windows.Forms.Button();
            commandListBox = new System.Windows.Forms.ListBox();
            showHiddenCommandsCheckBox = new System.Windows.Forms.CheckBox();
            sendRecvTextBox = new System.Windows.Forms.TextBox();
            userDataTextBox = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            statusTextBox = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            cmdSelTextBox = new System.Windows.Forms.TextBox();
            cmdInfoTextBox = new System.Windows.Forms.TextBox();
            cmdValueTextBox = new System.Windows.Forms.TextBox();
            textForMsgTextBox = new System.Windows.Forms.TextBox();
            showRecvHexCheckBox = new System.Windows.Forms.CheckBox();
            msgClassTextBox = new System.Windows.Forms.TextBox();
            HostFileTextBox = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            execTimeTextBox = new System.Windows.Forms.TextBox();
            cancelButton = new System.Windows.Forms.Button();
            sendIterationsTextBox = new System.Windows.Forms.TextBox();
            sendCountTextBox = new System.Windows.Forms.TextBox();
            label8 = new System.Windows.Forms.Label();
            extraDataTextBox = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label6 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // sendButton
            // 
            sendButton.Location = new System.Drawing.Point(344, 149);
            sendButton.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            sendButton.Name = "sendButton";
            sendButton.Size = new System.Drawing.Size(56, 27);
            sendButton.TabIndex = 0;
            sendButton.Text = "Send";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += sendButton_Click;
            // 
            // comPortComboBox
            // 
            comPortComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            comPortComboBox.FormattingEnabled = true;
            comPortComboBox.Location = new System.Drawing.Point(5, 15);
            comPortComboBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            comPortComboBox.Name = "comPortComboBox";
            comPortComboBox.Size = new System.Drawing.Size(87, 23);
            comPortComboBox.TabIndex = 1;
            // 
            // refreshPortsButton
            // 
            refreshPortsButton.Location = new System.Drawing.Point(5, 71);
            refreshPortsButton.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            refreshPortsButton.Name = "refreshPortsButton";
            refreshPortsButton.Size = new System.Drawing.Size(85, 21);
            refreshPortsButton.TabIndex = 26;
            refreshPortsButton.Text = "Scan";
            refreshPortsButton.UseVisualStyleBackColor = true;
            refreshPortsButton.Click += refreshPortsButton_Click;
            // 
            // connectButton
            // 
            connectButton.Location = new System.Drawing.Point(4, 43);
            connectButton.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            connectButton.Name = "connectButton";
            connectButton.Size = new System.Drawing.Size(85, 21);
            connectButton.TabIndex = 27;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += connectButton_Click;
            // 
            // commandListBox
            // 
            commandListBox.AllowDrop = true;
            commandListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            commandListBox.FormattingEnabled = true;
            commandListBox.Location = new System.Drawing.Point(99, 10);
            commandListBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            commandListBox.Name = "commandListBox";
            commandListBox.ScrollAlwaysVisible = true;
            commandListBox.Size = new System.Drawing.Size(231, 154);
            commandListBox.Sorted = true;
            commandListBox.TabIndex = 2;
            commandListBox.SelectedIndexChanged += commandListBox_SelectedIndexChanged;
            // 
            // showHiddenCommandsCheckBox
            // 
            showHiddenCommandsCheckBox.AutoSize = true;
            showHiddenCommandsCheckBox.Location = new System.Drawing.Point(109, 181);
            showHiddenCommandsCheckBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            showHiddenCommandsCheckBox.Name = "showHiddenCommandsCheckBox";
            showHiddenCommandsCheckBox.Size = new System.Drawing.Size(208, 17);
            showHiddenCommandsCheckBox.TabIndex = 29;
            showHiddenCommandsCheckBox.Text = "Show internal/unsupported commands";
            showHiddenCommandsCheckBox.UseVisualStyleBackColor = true;
            showHiddenCommandsCheckBox.CheckedChanged += showHiddenCommandsCheckBox_CheckedChanged;
            // 
            // sendRecvTextBox
            // 
            sendRecvTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            sendRecvTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            sendRecvTextBox.Location = new System.Drawing.Point(11, 503);
            sendRecvTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            sendRecvTextBox.Multiline = true;
            sendRecvTextBox.Name = "sendRecvTextBox";
            sendRecvTextBox.ReadOnly = true;
            sendRecvTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            sendRecvTextBox.Size = new System.Drawing.Size(738, 171);
            sendRecvTextBox.TabIndex = 3;
            // 
            // userDataTextBox
            // 
            userDataTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            userDataTextBox.Location = new System.Drawing.Point(6, 166);
            userDataTextBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            userDataTextBox.Name = "userDataTextBox";
            userDataTextBox.Size = new System.Drawing.Size(88, 21);
            userDataTextBox.TabIndex = 7;
            userDataTextBox.Text = "0";
            userDataTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(0, 153);
            label3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(55, 13);
            label3.TabIndex = 5;
            label3.Text = "User Data";
            // 
            // statusTextBox
            // 
            statusTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            statusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            statusTextBox.Location = new System.Drawing.Point(4, 287);
            statusTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            statusTextBox.Multiline = true;
            statusTextBox.Name = "statusTextBox";
            statusTextBox.ReadOnly = true;
            statusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            statusTextBox.Size = new System.Drawing.Size(742, 173);
            statusTextBox.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(8, 273);
            label4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(37, 13);
            label4.TabIndex = 5;
            label4.Text = "Status";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(11, 479);
            label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(83, 13);
            label5.TabIndex = 5;
            label5.Text = "Send/Received";
            // 
            // cmdSelTextBox
            // 
            cmdSelTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cmdSelTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmdSelTextBox.Location = new System.Drawing.Point(418, 10);
            cmdSelTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            cmdSelTextBox.Name = "cmdSelTextBox";
            cmdSelTextBox.ReadOnly = true;
            cmdSelTextBox.Size = new System.Drawing.Size(332, 21);
            cmdSelTextBox.TabIndex = 9;
            cmdSelTextBox.Text = "Cmd Description";
            // 
            // cmdInfoTextBox
            // 
            cmdInfoTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cmdInfoTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmdInfoTextBox.Location = new System.Drawing.Point(418, 34);
            cmdInfoTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            cmdInfoTextBox.Multiline = true;
            cmdInfoTextBox.Name = "cmdInfoTextBox";
            cmdInfoTextBox.ReadOnly = true;
            cmdInfoTextBox.Size = new System.Drawing.Size(332, 105);
            cmdInfoTextBox.TabIndex = 10;
            // 
            // cmdValueTextBox
            // 
            cmdValueTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmdValueTextBox.Location = new System.Drawing.Point(334, 10);
            cmdValueTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            cmdValueTextBox.Name = "cmdValueTextBox";
            cmdValueTextBox.ReadOnly = true;
            cmdValueTextBox.Size = new System.Drawing.Size(78, 21);
            cmdValueTextBox.TabIndex = 11;
            cmdValueTextBox.Text = "Cmd Value";
            cmdValueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textForMsgTextBox
            // 
            textForMsgTextBox.AllowDrop = true;
            textForMsgTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textForMsgTextBox.Location = new System.Drawing.Point(12, 252);
            textForMsgTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            textForMsgTextBox.MaxLength = 512;
            textForMsgTextBox.Name = "textForMsgTextBox";
            textForMsgTextBox.Size = new System.Drawing.Size(601, 21);
            textForMsgTextBox.TabIndex = 12;
            // 
            // showRecvHexCheckBox
            // 
            showRecvHexCheckBox.AutoSize = true;
            showRecvHexCheckBox.Location = new System.Drawing.Point(114, 478);
            showRecvHexCheckBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            showRecvHexCheckBox.Name = "showRecvHexCheckBox";
            showRecvHexCheckBox.Size = new System.Drawing.Size(45, 17);
            showRecvHexCheckBox.TabIndex = 13;
            showRecvHexCheckBox.Text = "Hex";
            showRecvHexCheckBox.UseVisualStyleBackColor = true;
            showRecvHexCheckBox.CheckedChanged += showRecvHexCheckBox_CheckedChanged;
            // 
            // msgClassTextBox
            // 
            msgClassTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            msgClassTextBox.Location = new System.Drawing.Point(334, 47);
            msgClassTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            msgClassTextBox.Name = "msgClassTextBox";
            msgClassTextBox.ReadOnly = true;
            msgClassTextBox.Size = new System.Drawing.Size(78, 21);
            msgClassTextBox.TabIndex = 11;
            msgClassTextBox.Text = "Cmd Type";
            msgClassTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // HostFileTextBox
            // 
            HostFileTextBox.AllowDrop = true;
            HostFileTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            HostFileTextBox.Location = new System.Drawing.Point(14, 206);
            HostFileTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            HostFileTextBox.MaxLength = 512;
            HostFileTextBox.Name = "HostFileTextBox";
            HostFileTextBox.Size = new System.Drawing.Size(600, 21);
            HostFileTextBox.TabIndex = 12;
            HostFileTextBox.Text = "C:\\HcomFiles\\Write\\Text\\TestFileGen1-26.txt";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(9, 189);
            label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(48, 13);
            label7.TabIndex = 5;
            label7.Text = "Host File";
            // 
            // execTimeTextBox
            // 
            execTimeTextBox.Location = new System.Drawing.Point(418, 155);
            execTimeTextBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            execTimeTextBox.Name = "execTimeTextBox";
            execTimeTextBox.ReadOnly = true;
            execTimeTextBox.Size = new System.Drawing.Size(101, 20);
            execTimeTextBox.TabIndex = 14;
            execTimeTextBox.Text = "Execution Time";
            execTimeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cancelButton
            // 
            cancelButton.Location = new System.Drawing.Point(615, 149);
            cancelButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(50, 29);
            cancelButton.TabIndex = 15;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // sendIterationsTextBox
            // 
            sendIterationsTextBox.Location = new System.Drawing.Point(344, 95);
            sendIterationsTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            sendIterationsTextBox.Name = "sendIterationsTextBox";
            sendIterationsTextBox.Size = new System.Drawing.Size(58, 20);
            sendIterationsTextBox.TabIndex = 16;
            sendIterationsTextBox.Text = "1";
            sendIterationsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // sendCountTextBox
            // 
            sendCountTextBox.Location = new System.Drawing.Point(342, 121);
            sendCountTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            sendCountTextBox.Name = "sendCountTextBox";
            sendCountTextBox.ReadOnly = true;
            sendCountTextBox.Size = new System.Drawing.Size(60, 20);
            sendCountTextBox.TabIndex = 18;
            sendCountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(1, 112);
            label8.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(57, 13);
            label8.TabIndex = 19;
            label8.Text = "Extra Data";
            // 
            // extraDataTextBox
            // 
            extraDataTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            extraDataTextBox.Location = new System.Drawing.Point(6, 130);
            extraDataTextBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            extraDataTextBox.Name = "extraDataTextBox";
            extraDataTextBox.Size = new System.Drawing.Size(88, 21);
            extraDataTextBox.TabIndex = 20;
            extraDataTextBox.Text = "0";
            extraDataTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(17, 231);
            label2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(337, 13);
            label2.TabIndex = 5;
            label2.Text = "Meadow:Text, Data, Path Name or CMD Line (e.g. 'ping google.com') ";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(connectButton);
            groupBox1.Controls.Add(comPortComboBox);
            groupBox1.Controls.Add(refreshPortsButton);
            groupBox1.Location = new System.Drawing.Point(2, 10);
            groupBox1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(1, 1, 1, 1);
            groupBox1.Size = new System.Drawing.Size(94, 100);
            groupBox1.TabIndex = 28;
            groupBox1.TabStop = false;
            groupBox1.Text = "Comm Port";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(334, 81);
            label6.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(76, 13);
            label6.TabIndex = 17;
            label6.Text = "Iteration Count";
            // 
            // HcomDiagUiForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(758, 684);
            Controls.Add(extraDataTextBox);
            Controls.Add(label8);
            Controls.Add(sendCountTextBox);
            Controls.Add(label6);
            Controls.Add(sendIterationsTextBox);
            Controls.Add(cancelButton);
            Controls.Add(execTimeTextBox);
            Controls.Add(showRecvHexCheckBox);
            Controls.Add(HostFileTextBox);
            Controls.Add(textForMsgTextBox);
            Controls.Add(msgClassTextBox);
            Controls.Add(cmdValueTextBox);
            Controls.Add(cmdInfoTextBox);
            Controls.Add(cmdSelTextBox);
            Controls.Add(statusTextBox);
            Controls.Add(userDataTextBox);
            Controls.Add(label3);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label7);
            Controls.Add(label2);
            Controls.Add(sendRecvTextBox);
            Controls.Add(showHiddenCommandsCheckBox);
            Controls.Add(commandListBox);
            Controls.Add(sendButton);
            Controls.Add(groupBox1);
            Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            Name = "HcomDiagUiForm";
            Text = "HcomDiagUi";
            FormClosing += HcomDiagUiForm_FormClosing;
            Shown += HcomDiagUiForm_Shown;
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.ComboBox comPortComboBox;
        private System.Windows.Forms.Button refreshPortsButton;
        private System.Windows.Forms.Button connectButton;
        internal System.Windows.Forms.ListBox commandListBox;
        private System.Windows.Forms.CheckBox showHiddenCommandsCheckBox;
        private System.Windows.Forms.TextBox sendRecvTextBox;
        internal System.Windows.Forms.TextBox userDataTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox statusTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox cmdSelTextBox;
        private System.Windows.Forms.TextBox cmdInfoTextBox;
        private System.Windows.Forms.TextBox cmdValueTextBox;
        private System.Windows.Forms.TextBox textForMsgTextBox;
        private System.Windows.Forms.CheckBox showRecvHexCheckBox;
        private System.Windows.Forms.TextBox msgClassTextBox;
        private System.Windows.Forms.TextBox HostFileTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox execTimeTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox sendIterationsTextBox;
        private System.Windows.Forms.TextBox sendCountTextBox;
        private System.Windows.Forms.Label label8;
        internal System.Windows.Forms.TextBox extraDataTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label6;
    }
}


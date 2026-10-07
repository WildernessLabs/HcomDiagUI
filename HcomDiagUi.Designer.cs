
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
            sendButton.Location = new System.Drawing.Point(975, 471);
            sendButton.Name = "sendButton";
            sendButton.Size = new System.Drawing.Size(158, 85);
            sendButton.TabIndex = 0;
            sendButton.Text = "Send";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += sendButton_Click;
            // 
            // comPortComboBox
            // 
            comPortComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            comPortComboBox.FormattingEnabled = true;
            comPortComboBox.Location = new System.Drawing.Point(13, 48);
            comPortComboBox.Name = "comPortComboBox";
            comPortComboBox.Size = new System.Drawing.Size(240, 44);
            comPortComboBox.TabIndex = 1;
            // 
            // refreshPortsButton
            // 
            refreshPortsButton.Location = new System.Drawing.Point(13, 225);
            refreshPortsButton.Name = "refreshPortsButton";
            refreshPortsButton.Size = new System.Drawing.Size(240, 66);
            refreshPortsButton.TabIndex = 26;
            refreshPortsButton.Text = "Scan";
            refreshPortsButton.UseVisualStyleBackColor = true;
            refreshPortsButton.Click += refreshPortsButton_Click;
            // 
            // connectButton
            // 
            connectButton.Location = new System.Drawing.Point(12, 135);
            connectButton.Name = "connectButton";
            connectButton.Size = new System.Drawing.Size(241, 66);
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
            commandListBox.Location = new System.Drawing.Point(280, 32);
            commandListBox.Name = "commandListBox";
            commandListBox.ScrollAlwaysVisible = true;
            commandListBox.Size = new System.Drawing.Size(646, 508);
            commandListBox.Sorted = true;
            commandListBox.TabIndex = 2;
            commandListBox.SelectedIndexChanged += commandListBox_SelectedIndexChanged;
            // 
            // showHiddenCommandsCheckBox
            // 
            showHiddenCommandsCheckBox.AutoSize = true;
            showHiddenCommandsCheckBox.Location = new System.Drawing.Point(309, 571);
            showHiddenCommandsCheckBox.Name = "showHiddenCommandsCheckBox";
            showHiddenCommandsCheckBox.Size = new System.Drawing.Size(575, 45);
            showHiddenCommandsCheckBox.TabIndex = 29;
            showHiddenCommandsCheckBox.Text = "Show internal/unsupported commands";
            showHiddenCommandsCheckBox.UseVisualStyleBackColor = true;
            showHiddenCommandsCheckBox.CheckedChanged += showHiddenCommandsCheckBox_CheckedChanged;
            // 
            // sendRecvTextBox
            // 
            sendRecvTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            sendRecvTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            sendRecvTextBox.Location = new System.Drawing.Point(32, 1586);
            sendRecvTextBox.Multiline = true;
            sendRecvTextBox.Name = "sendRecvTextBox";
            sendRecvTextBox.ReadOnly = true;
            sendRecvTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            sendRecvTextBox.Size = new System.Drawing.Size(2085, 492);
            sendRecvTextBox.TabIndex = 3;
            // 
            // userDataTextBox
            // 
            userDataTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            userDataTextBox.Location = new System.Drawing.Point(17, 525);
            userDataTextBox.Margin = new System.Windows.Forms.Padding(1);
            userDataTextBox.Name = "userDataTextBox";
            userDataTextBox.Size = new System.Drawing.Size(241, 41);
            userDataTextBox.TabIndex = 7;
            userDataTextBox.Text = "0";
            userDataTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(1, 481);
            label3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(147, 41);
            label3.TabIndex = 5;
            label3.Text = "User Data";
            // 
            // statusTextBox
            // 
            statusTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            statusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            statusTextBox.Location = new System.Drawing.Point(11, 906);
            statusTextBox.Multiline = true;
            statusTextBox.Name = "statusTextBox";
            statusTextBox.ReadOnly = true;
            statusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            statusTextBox.Size = new System.Drawing.Size(2096, 537);
            statusTextBox.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(22, 861);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(98, 41);
            label4.TabIndex = 5;
            label4.Text = "Status";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(31, 1510);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(215, 41);
            label5.TabIndex = 5;
            label5.Text = "Send/Received";
            // 
            // cmdSelTextBox
            // 
            cmdSelTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cmdSelTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmdSelTextBox.Location = new System.Drawing.Point(1185, 32);
            cmdSelTextBox.Name = "cmdSelTextBox";
            cmdSelTextBox.ReadOnly = true;
            cmdSelTextBox.Size = new System.Drawing.Size(2022, 41);
            cmdSelTextBox.TabIndex = 9;
            cmdSelTextBox.Text = "Cmd Description";
            // 
            // cmdInfoTextBox
            // 
            cmdInfoTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cmdInfoTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmdInfoTextBox.Location = new System.Drawing.Point(1185, 107);
            cmdInfoTextBox.Multiline = true;
            cmdInfoTextBox.Name = "cmdInfoTextBox";
            cmdInfoTextBox.ReadOnly = true;
            cmdInfoTextBox.Size = new System.Drawing.Size(933, 323);
            cmdInfoTextBox.TabIndex = 10;
            // 
            // cmdValueTextBox
            // 
            cmdValueTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmdValueTextBox.Location = new System.Drawing.Point(946, 32);
            cmdValueTextBox.Name = "cmdValueTextBox";
            cmdValueTextBox.ReadOnly = true;
            cmdValueTextBox.Size = new System.Drawing.Size(213, 41);
            cmdValueTextBox.TabIndex = 11;
            cmdValueTextBox.Text = "Cmd Value";
            cmdValueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textForMsgTextBox
            // 
            textForMsgTextBox.AllowDrop = true;
            textForMsgTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textForMsgTextBox.Location = new System.Drawing.Point(34, 795);
            textForMsgTextBox.MaxLength = 512;
            textForMsgTextBox.Name = "textForMsgTextBox";
            textForMsgTextBox.Size = new System.Drawing.Size(1696, 41);
            textForMsgTextBox.TabIndex = 12;
            // 
            // showRecvHexCheckBox
            // 
            showRecvHexCheckBox.AutoSize = true;
            showRecvHexCheckBox.Location = new System.Drawing.Point(323, 1508);
            showRecvHexCheckBox.Name = "showRecvHexCheckBox";
            showRecvHexCheckBox.Size = new System.Drawing.Size(107, 45);
            showRecvHexCheckBox.TabIndex = 13;
            showRecvHexCheckBox.Text = "Hex";
            showRecvHexCheckBox.UseVisualStyleBackColor = true;
            showRecvHexCheckBox.CheckedChanged += showRecvHexCheckBox_CheckedChanged;
            // 
            // msgClassTextBox
            // 
            msgClassTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            msgClassTextBox.Location = new System.Drawing.Point(946, 147);
            msgClassTextBox.Name = "msgClassTextBox";
            msgClassTextBox.ReadOnly = true;
            msgClassTextBox.Size = new System.Drawing.Size(213, 41);
            msgClassTextBox.TabIndex = 11;
            msgClassTextBox.Text = "Cmd Type";
            msgClassTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // HostFileTextBox
            // 
            HostFileTextBox.AllowDrop = true;
            HostFileTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            HostFileTextBox.Location = new System.Drawing.Point(39, 649);
            HostFileTextBox.MaxLength = 512;
            HostFileTextBox.Name = "HostFileTextBox";
            HostFileTextBox.Size = new System.Drawing.Size(1692, 41);
            HostFileTextBox.TabIndex = 12;
            HostFileTextBox.Text = "C:\\HcomFiles\\Write\\Text\\TestFileGen1-26.txt";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(26, 596);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(133, 41);
            label7.TabIndex = 5;
            label7.Text = "Host File";
            // 
            // execTimeTextBox
            // 
            execTimeTextBox.Location = new System.Drawing.Point(1185, 489);
            execTimeTextBox.Margin = new System.Windows.Forms.Padding(1);
            execTimeTextBox.Name = "execTimeTextBox";
            execTimeTextBox.ReadOnly = true;
            execTimeTextBox.Size = new System.Drawing.Size(280, 47);
            execTimeTextBox.TabIndex = 14;
            execTimeTextBox.Text = "Execution Time";
            execTimeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cancelButton
            // 
            cancelButton.Location = new System.Drawing.Point(1742, 471);
            cancelButton.Margin = new System.Windows.Forms.Padding(1);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(141, 91);
            cancelButton.TabIndex = 15;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // sendIterationsTextBox
            // 
            sendIterationsTextBox.Location = new System.Drawing.Point(975, 301);
            sendIterationsTextBox.Name = "sendIterationsTextBox";
            sendIterationsTextBox.Size = new System.Drawing.Size(158, 47);
            sendIterationsTextBox.TabIndex = 16;
            sendIterationsTextBox.Text = "1";
            sendIterationsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // sendCountTextBox
            // 
            sendCountTextBox.Location = new System.Drawing.Point(970, 383);
            sendCountTextBox.Name = "sendCountTextBox";
            sendCountTextBox.ReadOnly = true;
            sendCountTextBox.Size = new System.Drawing.Size(163, 47);
            sendCountTextBox.TabIndex = 18;
            sendCountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(3, 352);
            label8.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(151, 41);
            label8.TabIndex = 19;
            label8.Text = "Extra Data";
            // 
            // extraDataTextBox
            // 
            extraDataTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            extraDataTextBox.Location = new System.Drawing.Point(17, 409);
            extraDataTextBox.Margin = new System.Windows.Forms.Padding(1);
            extraDataTextBox.Name = "extraDataTextBox";
            extraDataTextBox.Size = new System.Drawing.Size(241, 41);
            extraDataTextBox.TabIndex = 20;
            extraDataTextBox.Text = "0";
            extraDataTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(48, 729);
            label2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(935, 41);
            label2.TabIndex = 5;
            label2.Text = "Meadow:Text, Data, Path Name or CMD Line (e.g. 'ping google.com') ";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(connectButton);
            groupBox1.Controls.Add(comPortComboBox);
            groupBox1.Controls.Add(refreshPortsButton);
            groupBox1.Location = new System.Drawing.Point(7, 32);
            groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox1.Size = new System.Drawing.Size(267, 316);
            groupBox1.TabIndex = 28;
            groupBox1.TabStop = false;
            groupBox1.Text = "Comm Port";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(946, 257);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(218, 41);
            label6.TabIndex = 17;
            label6.Text = "Iteration Count";
            // 
            // HcomDiagUiForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(17F, 41F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(2148, 2148);
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


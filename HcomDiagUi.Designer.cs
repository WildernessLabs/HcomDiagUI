
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
            this.sendButton = new System.Windows.Forms.Button();
            this.comPortTextBox = new System.Windows.Forms.TextBox();
            this.commandListBox = new System.Windows.Forms.ListBox();
            this.sendRecvTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.userDataTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.statusTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmdSelTextBox = new System.Windows.Forms.TextBox();
            this.cmdInfoTextBox = new System.Windows.Forms.TextBox();
            this.cmdValueTextBox = new System.Windows.Forms.TextBox();
            this.textForMsgTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.showRecvHexCheckBox = new System.Windows.Forms.CheckBox();
            this.msgClassTextBox = new System.Windows.Forms.TextBox();
            this.HostFileTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.execTimeTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.sendIterationsTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.sendCountTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(403, 162);
            this.sendButton.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(56, 27);
            this.sendButton.TabIndex = 0;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // comPortTextBox
            // 
            this.comPortTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comPortTextBox.Location = new System.Drawing.Point(4, 26);
            this.comPortTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.comPortTextBox.Name = "comPortTextBox";
            this.comPortTextBox.Size = new System.Drawing.Size(67, 21);
            this.comPortTextBox.TabIndex = 1;
            this.comPortTextBox.Text = "Com9";
            this.comPortTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.comPortTextBox.TextChanged += new System.EventHandler(this.comPortTextBox_TextChanged);
            // 
            // commandListBox
            // 
            this.commandListBox.AllowDrop = true;
            this.commandListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.commandListBox.FormattingEnabled = true;
            this.commandListBox.ItemHeight = 15;
            this.commandListBox.Location = new System.Drawing.Point(88, 10);
            this.commandListBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.commandListBox.Name = "commandListBox";
            this.commandListBox.ScrollAlwaysVisible = true;
            this.commandListBox.Size = new System.Drawing.Size(222, 184);
            this.commandListBox.Sorted = true;
            this.commandListBox.TabIndex = 2;
            this.commandListBox.SelectedIndexChanged += new System.EventHandler(this.commandListBox_SelectedIndexChanged);
            // 
            // sendRecvTextBox
            // 
            this.sendRecvTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sendRecvTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendRecvTextBox.Location = new System.Drawing.Point(10, 511);
            this.sendRecvTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.sendRecvTextBox.Multiline = true;
            this.sendRecvTextBox.Name = "sendRecvTextBox";
            this.sendRecvTextBox.ReadOnly = true;
            this.sendRecvTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.sendRecvTextBox.Size = new System.Drawing.Size(603, 126);
            this.sendRecvTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Com Port";
            // 
            // userDataTextBox
            // 
            this.userDataTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userDataTextBox.Location = new System.Drawing.Point(14, 166);
            this.userDataTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.userDataTextBox.Name = "userDataTextBox";
            this.userDataTextBox.Size = new System.Drawing.Size(65, 21);
            this.userDataTextBox.TabIndex = 7;
            this.userDataTextBox.Text = "0";
            this.userDataTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 148);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "User Data";
            // 
            // statusTextBox
            // 
            this.statusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusTextBox.Location = new System.Drawing.Point(14, 291);
            this.statusTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.statusTextBox.Multiline = true;
            this.statusTextBox.Name = "statusTextBox";
            this.statusTextBox.ReadOnly = true;
            this.statusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.statusTextBox.Size = new System.Drawing.Size(599, 173);
            this.statusTextBox.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 273);
            this.label4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Status";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 479);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Send/Received";
            // 
            // cmdSelTextBox
            // 
            this.cmdSelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSelTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdSelTextBox.Location = new System.Drawing.Point(403, 10);
            this.cmdSelTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.cmdSelTextBox.Name = "cmdSelTextBox";
            this.cmdSelTextBox.ReadOnly = true;
            this.cmdSelTextBox.Size = new System.Drawing.Size(210, 21);
            this.cmdSelTextBox.TabIndex = 9;
            this.cmdSelTextBox.Text = "Cmd Description";
            // 
            // cmdInfoTextBox
            // 
            this.cmdInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdInfoTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdInfoTextBox.Location = new System.Drawing.Point(403, 34);
            this.cmdInfoTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.cmdInfoTextBox.Multiline = true;
            this.cmdInfoTextBox.Name = "cmdInfoTextBox";
            this.cmdInfoTextBox.ReadOnly = true;
            this.cmdInfoTextBox.Size = new System.Drawing.Size(210, 115);
            this.cmdInfoTextBox.TabIndex = 10;
            // 
            // cmdValueTextBox
            // 
            this.cmdValueTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdValueTextBox.Location = new System.Drawing.Point(315, 10);
            this.cmdValueTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.cmdValueTextBox.Name = "cmdValueTextBox";
            this.cmdValueTextBox.ReadOnly = true;
            this.cmdValueTextBox.Size = new System.Drawing.Size(78, 21);
            this.cmdValueTextBox.TabIndex = 11;
            this.cmdValueTextBox.Text = "Cmd Value";
            this.cmdValueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textForMsgTextBox
            // 
            this.textForMsgTextBox.AllowDrop = true;
            this.textForMsgTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textForMsgTextBox.Location = new System.Drawing.Point(12, 252);
            this.textForMsgTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.textForMsgTextBox.MaxLength = 512;
            this.textForMsgTextBox.Name = "textForMsgTextBox";
            this.textForMsgTextBox.Size = new System.Drawing.Size(601, 21);
            this.textForMsgTextBox.TabIndex = 12;
            this.textForMsgTextBox.Text = "meadow.config.yaml";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 235);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(278, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Text, Data or Meadow File Name (e.g. \'ping google.com\') ";
            // 
            // showRecvHexCheckBox
            // 
            this.showRecvHexCheckBox.AutoSize = true;
            this.showRecvHexCheckBox.Location = new System.Drawing.Point(114, 478);
            this.showRecvHexCheckBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.showRecvHexCheckBox.Name = "showRecvHexCheckBox";
            this.showRecvHexCheckBox.Size = new System.Drawing.Size(45, 17);
            this.showRecvHexCheckBox.TabIndex = 13;
            this.showRecvHexCheckBox.Text = "Hex";
            this.showRecvHexCheckBox.UseVisualStyleBackColor = true;
            this.showRecvHexCheckBox.CheckedChanged += new System.EventHandler(this.showRecvHexCheckBox_CheckedChanged);
            // 
            // msgClassTextBox
            // 
            this.msgClassTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.msgClassTextBox.Location = new System.Drawing.Point(315, 45);
            this.msgClassTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.msgClassTextBox.Name = "msgClassTextBox";
            this.msgClassTextBox.ReadOnly = true;
            this.msgClassTextBox.Size = new System.Drawing.Size(78, 21);
            this.msgClassTextBox.TabIndex = 11;
            this.msgClassTextBox.Text = "Cmd Type";
            this.msgClassTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // HostFileTextBox
            // 
            this.HostFileTextBox.AllowDrop = true;
            this.HostFileTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HostFileTextBox.Location = new System.Drawing.Point(14, 206);
            this.HostFileTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.HostFileTextBox.MaxLength = 512;
            this.HostFileTextBox.Name = "HostFileTextBox";
            this.HostFileTextBox.Size = new System.Drawing.Size(600, 21);
            this.HostFileTextBox.TabIndex = 12;
            this.HostFileTextBox.Text = "C:\\DnldHcom\\Upld\\meadow.config.yaml";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 189);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Host File";
            // 
            // execTimeTextBox
            // 
            this.execTimeTextBox.Location = new System.Drawing.Point(493, 166);
            this.execTimeTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.execTimeTextBox.Name = "execTimeTextBox";
            this.execTimeTextBox.ReadOnly = true;
            this.execTimeTextBox.Size = new System.Drawing.Size(120, 20);
            this.execTimeTextBox.TabIndex = 14;
            this.execTimeTextBox.Text = "Execution Time";
            this.execTimeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(326, 86);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(50, 29);
            this.cancelButton.TabIndex = 15;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // sendIterationsTextBox
            // 
            this.sendIterationsTextBox.Location = new System.Drawing.Point(334, 155);
            this.sendIterationsTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.sendIterationsTextBox.Name = "sendIterationsTextBox";
            this.sendIterationsTextBox.Size = new System.Drawing.Size(41, 20);
            this.sendIterationsTextBox.TabIndex = 16;
            this.sendIterationsTextBox.Text = "1";
            this.sendIterationsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(327, 140);
            this.label6.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Iterations";
            // 
            // sendCountTextBox
            // 
            this.sendCountTextBox.Location = new System.Drawing.Point(334, 177);
            this.sendCountTextBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.sendCountTextBox.Name = "sendCountTextBox";
            this.sendCountTextBox.ReadOnly = true;
            this.sendCountTextBox.Size = new System.Drawing.Size(41, 20);
            this.sendCountTextBox.TabIndex = 18;
            this.sendCountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // HcomDiagUiForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 647);
            this.Controls.Add(this.sendCountTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.sendIterationsTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.execTimeTextBox);
            this.Controls.Add(this.showRecvHexCheckBox);
            this.Controls.Add(this.HostFileTextBox);
            this.Controls.Add(this.textForMsgTextBox);
            this.Controls.Add(this.msgClassTextBox);
            this.Controls.Add(this.cmdValueTextBox);
            this.Controls.Add(this.cmdInfoTextBox);
            this.Controls.Add(this.cmdSelTextBox);
            this.Controls.Add(this.statusTextBox);
            this.Controls.Add(this.userDataTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sendRecvTextBox);
            this.Controls.Add(this.commandListBox);
            this.Controls.Add(this.comPortTextBox);
            this.Controls.Add(this.sendButton);
            this.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.Name = "HcomDiagUiForm";
            this.Text = "HcomDiagUi";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.TextBox comPortTextBox;
        internal System.Windows.Forms.ListBox commandListBox;
        private System.Windows.Forms.TextBox sendRecvTextBox;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox userDataTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox statusTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox cmdSelTextBox;
        private System.Windows.Forms.TextBox cmdInfoTextBox;
        private System.Windows.Forms.TextBox cmdValueTextBox;
        private System.Windows.Forms.TextBox textForMsgTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox showRecvHexCheckBox;
        private System.Windows.Forms.TextBox msgClassTextBox;
        private System.Windows.Forms.TextBox HostFileTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox execTimeTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox sendIterationsTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox sendCountTextBox;
    }
}


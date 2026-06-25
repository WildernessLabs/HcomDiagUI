
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
            this.showRecvHexCheckBox = new System.Windows.Forms.CheckBox();
            this.msgClassTextBox = new System.Windows.Forms.TextBox();
            this.HostFileTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.execTimeTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.sendIterationsTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.sendCountTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.extraDataTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(1075, 386);
            this.sendButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(149, 64);
            this.sendButton.TabIndex = 0;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // comPortTextBox
            // 
            this.comPortTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comPortTextBox.Location = new System.Drawing.Point(11, 62);
            this.comPortTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comPortTextBox.Name = "comPortTextBox";
            this.comPortTextBox.Size = new System.Drawing.Size(172, 41);
            this.comPortTextBox.TabIndex = 1;
            this.comPortTextBox.Text = "Com10";
            this.comPortTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.comPortTextBox.TextChanged += new System.EventHandler(this.comPortTextBox_TextChanged);
            // 
            // commandListBox
            // 
            this.commandListBox.AllowDrop = true;
            this.commandListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.commandListBox.FormattingEnabled = true;
            this.commandListBox.ItemHeight = 36;
            this.commandListBox.Location = new System.Drawing.Point(235, 24);
            this.commandListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.commandListBox.Name = "commandListBox";
            this.commandListBox.ScrollAlwaysVisible = true;
            this.commandListBox.Size = new System.Drawing.Size(542, 400);
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
            this.sendRecvTextBox.Location = new System.Drawing.Point(30, 1199);
            this.sendRecvTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sendRecvTextBox.Multiline = true;
            this.sendRecvTextBox.Name = "sendRecvTextBox";
            this.sendRecvTextBox.ReadOnly = true;
            this.sendRecvTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.sendRecvTextBox.Size = new System.Drawing.Size(1963, 373);
            this.sendRecvTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 32);
            this.label1.TabIndex = 5;
            this.label1.Text = "Com Port";
            // 
            // userDataTextBox
            // 
            this.userDataTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userDataTextBox.Location = new System.Drawing.Point(10, 374);
            this.userDataTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.userDataTextBox.Name = "userDataTextBox";
            this.userDataTextBox.Size = new System.Drawing.Size(167, 41);
            this.userDataTextBox.TabIndex = 7;
            this.userDataTextBox.Text = "0";
            this.userDataTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 326);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 32);
            this.label3.TabIndex = 5;
            this.label3.Text = "User Data";
            // 
            // statusTextBox
            // 
            this.statusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusTextBox.Location = new System.Drawing.Point(10, 685);
            this.statusTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.statusTextBox.Multiline = true;
            this.statusTextBox.Name = "statusTextBox";
            this.statusTextBox.ReadOnly = true;
            this.statusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.statusTextBox.Size = new System.Drawing.Size(1973, 407);
            this.statusTextBox.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 651);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 32);
            this.label4.TabIndex = 5;
            this.label4.Text = "Status";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 1142);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(208, 32);
            this.label5.TabIndex = 5;
            this.label5.Text = "Send/Received";
            // 
            // cmdSelTextBox
            // 
            this.cmdSelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSelTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdSelTextBox.Location = new System.Drawing.Point(1075, 24);
            this.cmdSelTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmdSelTextBox.Name = "cmdSelTextBox";
            this.cmdSelTextBox.ReadOnly = true;
            this.cmdSelTextBox.Size = new System.Drawing.Size(1943, 41);
            this.cmdSelTextBox.TabIndex = 9;
            this.cmdSelTextBox.Text = "Cmd Description";
            // 
            // cmdInfoTextBox
            // 
            this.cmdInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdInfoTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdInfoTextBox.Location = new System.Drawing.Point(1075, 81);
            this.cmdInfoTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmdInfoTextBox.Multiline = true;
            this.cmdInfoTextBox.Name = "cmdInfoTextBox";
            this.cmdInfoTextBox.ReadOnly = true;
            this.cmdInfoTextBox.Size = new System.Drawing.Size(918, 269);
            this.cmdInfoTextBox.TabIndex = 10;
            // 
            // cmdValueTextBox
            // 
            this.cmdValueTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdValueTextBox.Location = new System.Drawing.Point(840, 24);
            this.cmdValueTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmdValueTextBox.Name = "cmdValueTextBox";
            this.cmdValueTextBox.ReadOnly = true;
            this.cmdValueTextBox.Size = new System.Drawing.Size(201, 41);
            this.cmdValueTextBox.TabIndex = 11;
            this.cmdValueTextBox.Text = "Cmd Value";
            this.cmdValueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textForMsgTextBox
            // 
            this.textForMsgTextBox.AllowDrop = true;
            this.textForMsgTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textForMsgTextBox.Location = new System.Drawing.Point(32, 601);
            this.textForMsgTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textForMsgTextBox.MaxLength = 512;
            this.textForMsgTextBox.Name = "textForMsgTextBox";
            this.textForMsgTextBox.Size = new System.Drawing.Size(1596, 41);
            this.textForMsgTextBox.TabIndex = 12;
            // 
            // showRecvHexCheckBox
            // 
            this.showRecvHexCheckBox.AutoSize = true;
            this.showRecvHexCheckBox.Location = new System.Drawing.Point(304, 1140);
            this.showRecvHexCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.showRecvHexCheckBox.Name = "showRecvHexCheckBox";
            this.showRecvHexCheckBox.Size = new System.Drawing.Size(102, 36);
            this.showRecvHexCheckBox.TabIndex = 13;
            this.showRecvHexCheckBox.Text = "Hex";
            this.showRecvHexCheckBox.UseVisualStyleBackColor = true;
            this.showRecvHexCheckBox.CheckedChanged += new System.EventHandler(this.showRecvHexCheckBox_CheckedChanged);
            // 
            // msgClassTextBox
            // 
            this.msgClassTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.msgClassTextBox.Location = new System.Drawing.Point(840, 107);
            this.msgClassTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.msgClassTextBox.Name = "msgClassTextBox";
            this.msgClassTextBox.ReadOnly = true;
            this.msgClassTextBox.Size = new System.Drawing.Size(201, 41);
            this.msgClassTextBox.TabIndex = 11;
            this.msgClassTextBox.Text = "Cmd Type";
            this.msgClassTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // HostFileTextBox
            // 
            this.HostFileTextBox.AllowDrop = true;
            this.HostFileTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HostFileTextBox.Location = new System.Drawing.Point(37, 491);
            this.HostFileTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.HostFileTextBox.MaxLength = 512;
            this.HostFileTextBox.Name = "HostFileTextBox";
            this.HostFileTextBox.Size = new System.Drawing.Size(1593, 41);
            this.HostFileTextBox.TabIndex = 12;
            this.HostFileTextBox.Text = "C:\\HcomFiles\\Write\\Text\\TestFileGen1-26.txt";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(24, 451);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(126, 32);
            this.label7.TabIndex = 5;
            this.label7.Text = "Host File";
            // 
            // execTimeTextBox
            // 
            this.execTimeTextBox.Location = new System.Drawing.Point(563, 427);
            this.execTimeTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.execTimeTextBox.Name = "execTimeTextBox";
            this.execTimeTextBox.ReadOnly = true;
            this.execTimeTextBox.Size = new System.Drawing.Size(273, 38);
            this.execTimeTextBox.TabIndex = 14;
            this.execTimeTextBox.Text = "Execution Time";
            this.execTimeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(1367, 381);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(1);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(133, 69);
            this.cancelButton.TabIndex = 15;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // sendIterationsTextBox
            // 
            this.sendIterationsTextBox.Location = new System.Drawing.Point(869, 370);
            this.sendIterationsTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sendIterationsTextBox.Name = "sendIterationsTextBox";
            this.sendIterationsTextBox.Size = new System.Drawing.Size(125, 38);
            this.sendIterationsTextBox.TabIndex = 16;
            this.sendIterationsTextBox.Text = "1";
            this.sendIterationsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(872, 334);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(131, 32);
            this.label6.TabIndex = 17;
            this.label6.Text = "Iterations";
            // 
            // sendCountTextBox
            // 
            this.sendCountTextBox.Location = new System.Drawing.Point(869, 427);
            this.sendCountTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sendCountTextBox.Name = "sendCountTextBox";
            this.sendCountTextBox.ReadOnly = true;
            this.sendCountTextBox.Size = new System.Drawing.Size(125, 38);
            this.sendCountTextBox.TabIndex = 18;
            this.sendCountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 164);
            this.label8.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(147, 32);
            this.label8.TabIndex = 19;
            this.label8.Text = "Extra Data";
            // 
            // extraDataTextBox
            // 
            this.extraDataTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.extraDataTextBox.Location = new System.Drawing.Point(10, 213);
            this.extraDataTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.extraDataTextBox.Name = "extraDataTextBox";
            this.extraDataTextBox.Size = new System.Drawing.Size(165, 41);
            this.extraDataTextBox.TabIndex = 20;
            this.extraDataTextBox.Text = "0";
            this.extraDataTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 551);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(886, 32);
            this.label2.TabIndex = 5;
            this.label2.Text = "Meadow:Text, Data, Path Name or CMD Line (e.g. \'ping google.com\') ";
            // 
            // HcomDiagUiForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2022, 1624);
            this.Controls.Add(this.extraDataTextBox);
            this.Controls.Add(this.label8);
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
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
        private System.Windows.Forms.CheckBox showRecvHexCheckBox;
        private System.Windows.Forms.TextBox msgClassTextBox;
        private System.Windows.Forms.TextBox HostFileTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox execTimeTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox sendIterationsTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox sendCountTextBox;
        private System.Windows.Forms.Label label8;
        internal System.Windows.Forms.TextBox extraDataTextBox;
        private System.Windows.Forms.Label label2;
    }
}


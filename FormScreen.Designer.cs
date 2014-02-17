namespace GodaiQuest
{
    partial class FormScreen
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormScreen));
            this.picScreen = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnEdit = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnEnd = new System.Windows.Forms.Button();
            this.btnChangeAccount = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.btnGetout = new System.Windows.Forms.Button();
            this.txtTalk = new System.Windows.Forms.TextBox();
            this.btnTalk = new System.Windows.Forms.Button();
            this.picHeader = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labExpTotal = new System.Windows.Forms.Label();
            this.labExp = new System.Windows.Forms.Label();
            this.btnAshiatoLog = new System.Windows.Forms.Button();
            this.btnWarp = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.btnClear = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picScreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picScreen
            // 
            this.picScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picScreen.Location = new System.Drawing.Point(241, 19);
            this.picScreen.Name = "picScreen";
            this.picScreen.Size = new System.Drawing.Size(597, 612);
            this.picScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picScreen.TabIndex = 0;
            this.picScreen.TabStop = false;
            this.picScreen.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picScreen_MouseClick);
            this.picScreen.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.picScreen_MouseDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(145, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 12);
            this.label1.TabIndex = 1;
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.Location = new System.Drawing.Point(852, 510);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(129, 37);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "自分のダンジョンを編集";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "レンガ.png");
            this.imageList1.Images.SetKeyName(1, "草原.png");
            this.imageList1.Images.SetKeyName(2, "道.png");
            // 
            // btnEnd
            // 
            this.btnEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnd.Location = new System.Drawing.Point(853, 647);
            this.btnEnd.Name = "btnEnd";
            this.btnEnd.Size = new System.Drawing.Size(129, 37);
            this.btnEnd.TabIndex = 3;
            this.btnEnd.Text = "終了";
            this.btnEnd.UseVisualStyleBackColor = true;
            this.btnEnd.Click += new System.EventHandler(this.btnEnd_Click);
            // 
            // btnChangeAccount
            // 
            this.btnChangeAccount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangeAccount.Location = new System.Drawing.Point(852, 564);
            this.btnChangeAccount.Name = "btnChangeAccount";
            this.btnChangeAccount.Size = new System.Drawing.Size(129, 37);
            this.btnChangeAccount.TabIndex = 4;
            this.btnChangeAccount.Text = "アカウントの設定";
            this.btnChangeAccount.UseVisualStyleBackColor = true;
            this.btnChangeAccount.Click += new System.EventHandler(this.btnChangeAccountInfo_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBox1.BackColor = System.Drawing.Color.Silver;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.richTextBox1.ForeColor = System.Drawing.Color.Black;
            this.richTextBox1.Location = new System.Drawing.Point(12, 21);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(223, 514);
            this.richTextBox1.TabIndex = 6;
            this.richTextBox1.Text = "";
            this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
            this.richTextBox1.SelectionChanged += new System.EventHandler(this.richTextBox1_SelectionChanged);
            // 
            // timer2
            // 
            this.timer2.Interval = 500;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // btnGetout
            // 
            this.btnGetout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetout.Location = new System.Drawing.Point(852, 21);
            this.btnGetout.Name = "btnGetout";
            this.btnGetout.Size = new System.Drawing.Size(123, 38);
            this.btnGetout.TabIndex = 7;
            this.btnGetout.Text = "ダンジョン緊急脱出";
            this.btnGetout.UseVisualStyleBackColor = true;
            this.btnGetout.Click += new System.EventHandler(this.btnGetout_Click);
            // 
            // txtTalk
            // 
            this.txtTalk.AcceptsReturn = true;
            this.txtTalk.AcceptsTab = true;
            this.txtTalk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtTalk.Location = new System.Drawing.Point(12, 541);
            this.txtTalk.Multiline = true;
            this.txtTalk.Name = "txtTalk";
            this.txtTalk.Size = new System.Drawing.Size(223, 103);
            this.txtTalk.TabIndex = 8;
            // 
            // btnTalk
            // 
            this.btnTalk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTalk.Location = new System.Drawing.Point(12, 650);
            this.btnTalk.Name = "btnTalk";
            this.btnTalk.Size = new System.Drawing.Size(140, 34);
            this.btnTalk.TabIndex = 9;
            this.btnTalk.Text = "発言";
            this.btnTalk.UseVisualStyleBackColor = true;
            this.btnTalk.Click += new System.EventHandler(this.btnTalk_Click);
            // 
            // picHeader
            // 
            this.picHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.picHeader.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picHeader.Location = new System.Drawing.Point(852, 204);
            this.picHeader.Name = "picHeader";
            this.picHeader.Size = new System.Drawing.Size(129, 128);
            this.picHeader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHeader.TabIndex = 11;
            this.picHeader.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labExpTotal);
            this.groupBox1.Controls.Add(this.labExp);
            this.groupBox1.Location = new System.Drawing.Point(852, 338);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(129, 166);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            // 
            // labExpTotal
            // 
            this.labExpTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labExpTotal.AutoSize = true;
            this.labExpTotal.Location = new System.Drawing.Point(6, 15);
            this.labExpTotal.Name = "labExpTotal";
            this.labExpTotal.Size = new System.Drawing.Size(47, 12);
            this.labExpTotal.TabIndex = 1;
            this.labExpTotal.Text = "全経験：";
            // 
            // labExp
            // 
            this.labExp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labExp.AutoSize = true;
            this.labExp.Location = new System.Drawing.Point(6, 35);
            this.labExp.Name = "labExp";
            this.labExp.Size = new System.Drawing.Size(47, 12);
            this.labExp.TabIndex = 0;
            this.labExp.Text = "経験値：";
            // 
            // btnAshiatoLog
            // 
            this.btnAshiatoLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAshiatoLog.Location = new System.Drawing.Point(853, 119);
            this.btnAshiatoLog.Name = "btnAshiatoLog";
            this.btnAshiatoLog.Size = new System.Drawing.Size(122, 37);
            this.btnAshiatoLog.TabIndex = 13;
            this.btnAshiatoLog.Text = "足あとログを見る";
            this.btnAshiatoLog.UseVisualStyleBackColor = true;
            this.btnAshiatoLog.Click += new System.EventHandler(this.btnAshiatoLog_Click);
            // 
            // btnWarp
            // 
            this.btnWarp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWarp.Location = new System.Drawing.Point(852, 75);
            this.btnWarp.Name = "btnWarp";
            this.btnWarp.Size = new System.Drawing.Size(123, 38);
            this.btnWarp.TabIndex = 14;
            this.btnWarp.Text = "ワープ";
            this.btnWarp.UseVisualStyleBackColor = true;
            this.btnWarp.Click += new System.EventHandler(this.btnWarp_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Visible = true;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(158, 650);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(77, 34);
            this.btnClear.TabIndex = 15;
            this.btnClear.Text = "クリア";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnTalkCLear_Click);
            // 
            // FormScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 699);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnWarp);
            this.Controls.Add(this.btnAshiatoLog);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.picHeader);
            this.Controls.Add(this.btnTalk);
            this.Controls.Add(this.txtTalk);
            this.Controls.Add(this.btnGetout);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnChangeAccount);
            this.Controls.Add(this.btnEnd);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picScreen);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormScreen";
            this.Text = "Godai Quest";
            this.Load += new System.EventHandler(this.FormScreen_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormScreen_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.picScreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picScreen;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btnEnd;
        private System.Windows.Forms.Button btnChangeAccount;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Button btnGetout;
        private System.Windows.Forms.TextBox txtTalk;
        private System.Windows.Forms.Button btnTalk;
        private System.Windows.Forms.PictureBox picHeader;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labExp;
        private System.Windows.Forms.Label labExpTotal;
        private System.Windows.Forms.Button btnAshiatoLog;
        private System.Windows.Forms.Button btnWarp;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button btnClear;
    }
}
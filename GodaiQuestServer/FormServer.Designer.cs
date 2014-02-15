namespace GodaiQuestServer
{
    partial class FormServer
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormServer));
            this.tabContorler = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnSendServerMes = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtServerMes = new System.Windows.Forms.TextBox();
            this.tabUser = new System.Windows.Forms.TabPage();
            this.btnDelete = new System.Windows.Forms.Button();
            this.txtUserInfo = new System.Windows.Forms.TextBox();
            this.btnChangeEMail = new System.Windows.Forms.Button();
            this.btnChangePassword = new System.Windows.Forms.Button();
            this.cmbUserName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabContorler.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabUser.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabContorler
            // 
            this.tabContorler.Controls.Add(this.tabPage1);
            this.tabContorler.Controls.Add(this.tabPage2);
            this.tabContorler.Controls.Add(this.tabUser);
            this.tabContorler.Location = new System.Drawing.Point(12, 12);
            this.tabContorler.Name = "tabContorler";
            this.tabContorler.SelectedIndex = 0;
            this.tabContorler.Size = new System.Drawing.Size(821, 519);
            this.tabContorler.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtLog);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(813, 493);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "ログ";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(6, 6);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(801, 481);
            this.txtLog.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnSendServerMes);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.txtServerMes);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(813, 493);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "全体メッセージ";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnSendServerMes
            // 
            this.btnSendServerMes.Location = new System.Drawing.Point(23, 337);
            this.btnSendServerMes.Name = "btnSendServerMes";
            this.btnSendServerMes.Size = new System.Drawing.Size(224, 35);
            this.btnSendServerMes.TabIndex = 2;
            this.btnSendServerMes.Text = "送信";
            this.btnSendServerMes.UseVisualStyleBackColor = true;
            this.btnSendServerMes.Click += new System.EventHandler(this.btnSendServerMes_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "サーバーメッセージ";
            // 
            // txtServerMes
            // 
            this.txtServerMes.AcceptsReturn = true;
            this.txtServerMes.AcceptsTab = true;
            this.txtServerMes.Location = new System.Drawing.Point(16, 45);
            this.txtServerMes.Multiline = true;
            this.txtServerMes.Name = "txtServerMes";
            this.txtServerMes.Size = new System.Drawing.Size(492, 265);
            this.txtServerMes.TabIndex = 0;
            // 
            // tabUser
            // 
            this.tabUser.Controls.Add(this.btnDelete);
            this.tabUser.Controls.Add(this.txtUserInfo);
            this.tabUser.Controls.Add(this.btnChangeEMail);
            this.tabUser.Controls.Add(this.btnChangePassword);
            this.tabUser.Controls.Add(this.cmbUserName);
            this.tabUser.Controls.Add(this.label2);
            this.tabUser.Location = new System.Drawing.Point(4, 22);
            this.tabUser.Name = "tabUser";
            this.tabUser.Padding = new System.Windows.Forms.Padding(3);
            this.tabUser.Size = new System.Drawing.Size(813, 493);
            this.tabUser.TabIndex = 2;
            this.tabUser.Text = "ユーザ管理";
            this.tabUser.UseVisualStyleBackColor = true;
            this.tabUser.Click += new System.EventHandler(this.tabUser_Click);
            this.tabUser.Enter += new System.EventHandler(this.tabUser_Enter);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(11, 187);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(113, 29);
            this.btnDelete.TabIndex = 5;
            this.btnDelete.Text = "削除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // txtUserInfo
            // 
            this.txtUserInfo.Location = new System.Drawing.Point(11, 48);
            this.txtUserInfo.Multiline = true;
            this.txtUserInfo.Name = "txtUserInfo";
            this.txtUserInfo.ReadOnly = true;
            this.txtUserInfo.Size = new System.Drawing.Size(243, 76);
            this.txtUserInfo.TabIndex = 4;
            // 
            // btnChangeEMail
            // 
            this.btnChangeEMail.Location = new System.Drawing.Point(142, 141);
            this.btnChangeEMail.Name = "btnChangeEMail";
            this.btnChangeEMail.Size = new System.Drawing.Size(113, 29);
            this.btnChangeEMail.TabIndex = 3;
            this.btnChangeEMail.Text = "メールアドレスの変更";
            this.btnChangeEMail.UseVisualStyleBackColor = true;
            this.btnChangeEMail.Click += new System.EventHandler(this.btnChangeEMail_Click);
            // 
            // btnChangePassword
            // 
            this.btnChangePassword.Location = new System.Drawing.Point(13, 141);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new System.Drawing.Size(113, 29);
            this.btnChangePassword.TabIndex = 2;
            this.btnChangePassword.Text = "パスワードの変更";
            this.btnChangePassword.UseVisualStyleBackColor = true;
            this.btnChangePassword.Click += new System.EventHandler(this.btnChangePassword_Click);
            // 
            // cmbUserName
            // 
            this.cmbUserName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUserName.FormattingEnabled = true;
            this.cmbUserName.Location = new System.Drawing.Point(59, 12);
            this.cmbUserName.Name = "cmbUserName";
            this.cmbUserName.Size = new System.Drawing.Size(196, 20);
            this.cmbUserName.TabIndex = 1;
            this.cmbUserName.SelectedIndexChanged += new System.EventHandler(this.cmbUserName_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "ユーザ名";
            // 
            // FormServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(845, 543);
            this.Controls.Add(this.tabContorler);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormServer";
            this.Text = "Godai Quest サーバー";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormServer_FormClosed);
            this.tabContorler.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabUser.ResumeLayout(false);
            this.tabUser.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabContorler;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnSendServerMes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtServerMes;
        private System.Windows.Forms.TabPage tabUser;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.TextBox txtUserInfo;
        private System.Windows.Forms.Button btnChangeEMail;
        private System.Windows.Forms.Button btnChangePassword;
        private System.Windows.Forms.ComboBox cmbUserName;
        private System.Windows.Forms.Label label2;
    }
}


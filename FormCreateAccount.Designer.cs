namespace GodaiQuest
{
    partial class FormCreateAccount
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCreateAccount));
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.picPlayer = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnPic = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDownloadFolder = new System.Windows.Forms.TextBox();
            this.btnDownloadFolder = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picPlayer)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "キャラクタ名";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(71, 10);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(121, 19);
            this.txtName.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(85, 251);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(84, 27);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(218, 251);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 27);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // picPlayer
            // 
            this.picPlayer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPlayer.Location = new System.Drawing.Point(86, 45);
            this.picPlayer.Name = "picPlayer";
            this.picPlayer.Size = new System.Drawing.Size(64, 64);
            this.picPlayer.TabIndex = 4;
            this.picPlayer.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "キャラクタ画像";
            // 
            // btnPic
            // 
            this.btnPic.Location = new System.Drawing.Point(156, 45);
            this.btnPic.Name = "btnPic";
            this.btnPic.Size = new System.Drawing.Size(36, 64);
            this.btnPic.TabIndex = 6;
            this.btnPic.Text = "参照";
            this.btnPic.UseVisualStyleBackColor = true;
            this.btnPic.Click += new System.EventHandler(this.btnPic_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 226);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(261, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "※どれもあとで変更できますからお気軽に決めてください";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 133);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "資料ダウンロード先";
            // 
            // txtDownloadFolder
            // 
            this.txtDownloadFolder.Location = new System.Drawing.Point(111, 130);
            this.txtDownloadFolder.Name = "txtDownloadFolder";
            this.txtDownloadFolder.Size = new System.Drawing.Size(207, 19);
            this.txtDownloadFolder.TabIndex = 9;
            this.txtDownloadFolder.Text = "c:\\tmp\\GodaiQuest";
            // 
            // btnDownloadFolder
            // 
            this.btnDownloadFolder.Location = new System.Drawing.Point(320, 130);
            this.btnDownloadFolder.Name = "btnDownloadFolder";
            this.btnDownloadFolder.Size = new System.Drawing.Size(48, 19);
            this.btnDownloadFolder.TabIndex = 10;
            this.btnDownloadFolder.Text = "参照";
            this.btnDownloadFolder.UseVisualStyleBackColor = true;
            this.btnDownloadFolder.Click += new System.EventHandler(this.btnDownloadFolder_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(111, 189);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(207, 19);
            this.txtPassword.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 192);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "パスワード";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 161);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(310, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "※Godai Questでダウンロードできる資料の，ダウンロード先です．";
            // 
            // FormCreateAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 296);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnDownloadFolder);
            this.Controls.Add(this.txtDownloadFolder);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnPic);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.picPlayer);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCreateAccount";
            this.Text = "アカウント";
            this.Load += new System.EventHandler(this.FormCreateAccount_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picPlayer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox picPlayer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnPic;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDownloadFolder;
        private System.Windows.Forms.Button btnDownloadFolder;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}
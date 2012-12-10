namespace GodaiQuest
{
    partial class FormOpenItem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOpenItem));
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnUpload = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.btnWriteArticle = new System.Windows.Forms.Button();
            this.btnDeleteArticle = new System.Windows.Forms.Button();
            this.btnChangeState = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Location = new System.Drawing.Point(11, 33);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(174, 33);
            this.btnOpenFolder.TabIndex = 0;
            this.btnOpenFolder.Text = "エクスプローラーでフォルダを開く";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Visible = false;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(297, 455);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 36);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "閉じる";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(197, 34);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(175, 32);
            this.btnUpload.TabIndex = 3;
            this.btnUpload.Text = "フォルダの中身をアップロードする";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Visible = false;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.AcceptsTab = true;
            this.richTextBox1.Location = new System.Drawing.Point(12, 72);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(361, 286);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
            // 
            // richTextBox2
            // 
            this.richTextBox2.Location = new System.Drawing.Point(12, 364);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.Size = new System.Drawing.Size(360, 76);
            this.richTextBox2.TabIndex = 5;
            this.richTextBox2.Text = "";
            // 
            // btnWriteArticle
            // 
            this.btnWriteArticle.Location = new System.Drawing.Point(15, 454);
            this.btnWriteArticle.Name = "btnWriteArticle";
            this.btnWriteArticle.Size = new System.Drawing.Size(119, 37);
            this.btnWriteArticle.TabIndex = 6;
            this.btnWriteArticle.Text = "メッセージ書き込み";
            this.btnWriteArticle.UseVisualStyleBackColor = true;
            this.btnWriteArticle.Click += new System.EventHandler(this.btnWriteArticle_Click);
            // 
            // btnDeleteArticle
            // 
            this.btnDeleteArticle.Location = new System.Drawing.Point(161, 454);
            this.btnDeleteArticle.Name = "btnDeleteArticle";
            this.btnDeleteArticle.Size = new System.Drawing.Size(90, 39);
            this.btnDeleteArticle.TabIndex = 7;
            this.btnDeleteArticle.Text = "メッセージ削除";
            this.btnDeleteArticle.UseVisualStyleBackColor = true;
            this.btnDeleteArticle.Click += new System.EventHandler(this.btnDeleteArticle_Click);
            // 
            // btnChangeState
            // 
            this.btnChangeState.Location = new System.Drawing.Point(12, 3);
            this.btnChangeState.Name = "btnChangeState";
            this.btnChangeState.Size = new System.Drawing.Size(360, 25);
            this.btnChangeState.TabIndex = 8;
            this.btnChangeState.Text = "button1";
            this.btnChangeState.UseVisualStyleBackColor = true;
            this.btnChangeState.Visible = false;
            this.btnChangeState.Click += new System.EventHandler(this.btnChangeState_Click);
            // 
            // FormOpenItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 500);
            this.Controls.Add(this.btnChangeState);
            this.Controls.Add(this.btnDeleteArticle);
            this.Controls.Add(this.btnWriteArticle);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOpenFolder);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOpenItem";
            this.Text = "アイテム内容";
            this.Load += new System.EventHandler(this.FormOpenItem_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.Button btnWriteArticle;
        private System.Windows.Forms.Button btnDeleteArticle;
        private System.Windows.Forms.Button btnChangeState;
    }
}
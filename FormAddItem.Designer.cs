namespace GodaiQuest
{
    partial class FormAddItem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddItem));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtHeader = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.picHeader = new System.Windows.Forms.PictureBox();
            this.btnHeaderPic = new System.Windows.Forms.Button();
            this.picItem = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnItemOld = new System.Windows.Forms.Button();
            this.btnItemNew = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkProblem = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnFolder = new System.Windows.Forms.Button();
            this.btnClearList = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnRemoveUnusedFile = new System.Windows.Forms.Button();
            this.listFiles = new System.Windows.Forms.CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picItem)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(307, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "※技術情報はフォルダにまとめるか、見出しとして登録してください";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "見出し";
            // 
            // txtHeader
            // 
            this.txtHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeader.Location = new System.Drawing.Point(12, 60);
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.Size = new System.Drawing.Size(447, 120);
            this.txtHeader.TabIndex = 3;
            this.txtHeader.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(203, 212);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "見出し画像";
            this.label3.Visible = false;
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // picHeader
            // 
            this.picHeader.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picHeader.Location = new System.Drawing.Point(205, 236);
            this.picHeader.Name = "picHeader";
            this.picHeader.Size = new System.Drawing.Size(98, 91);
            this.picHeader.TabIndex = 5;
            this.picHeader.TabStop = false;
            this.picHeader.Visible = false;
            this.picHeader.Click += new System.EventHandler(this.picHeader_Click);
            // 
            // btnHeaderPic
            // 
            this.btnHeaderPic.Location = new System.Drawing.Point(309, 240);
            this.btnHeaderPic.Name = "btnHeaderPic";
            this.btnHeaderPic.Size = new System.Drawing.Size(76, 42);
            this.btnHeaderPic.TabIndex = 6;
            this.btnHeaderPic.Text = "画像登録";
            this.btnHeaderPic.UseVisualStyleBackColor = true;
            this.btnHeaderPic.Visible = false;
            this.btnHeaderPic.Click += new System.EventHandler(this.btnHeaderPic_Click);
            // 
            // picItem
            // 
            this.picItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picItem.Location = new System.Drawing.Point(15, 240);
            this.picItem.Name = "picItem";
            this.picItem.Size = new System.Drawing.Size(64, 64);
            this.picItem.TabIndex = 7;
            this.picItem.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 212);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "アイテムアイコン";
            // 
            // btnItemOld
            // 
            this.btnItemOld.Location = new System.Drawing.Point(85, 240);
            this.btnItemOld.Name = "btnItemOld";
            this.btnItemOld.Size = new System.Drawing.Size(95, 29);
            this.btnItemOld.TabIndex = 9;
            this.btnItemOld.Text = "既存画像を使う";
            this.btnItemOld.UseVisualStyleBackColor = true;
            this.btnItemOld.Click += new System.EventHandler(this.btnItemOld_Click);
            // 
            // btnItemNew
            // 
            this.btnItemNew.Location = new System.Drawing.Point(85, 275);
            this.btnItemNew.Name = "btnItemNew";
            this.btnItemNew.Size = new System.Drawing.Size(95, 29);
            this.btnItemNew.TabIndex = 10;
            this.btnItemNew.Text = "新規画像登録";
            this.btnItemNew.UseVisualStyleBackColor = true;
            this.btnItemNew.Click += new System.EventHandler(this.btnItemNew_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(9, 496);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(208, 42);
            this.btnOK.TabIndex = 16;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(223, 496);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(151, 44);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkProblem
            // 
            this.chkProblem.AutoSize = true;
            this.chkProblem.Location = new System.Drawing.Point(48, 32);
            this.chkProblem.Name = "chkProblem";
            this.chkProblem.Size = new System.Drawing.Size(304, 16);
            this.chkProblem.TabIndex = 23;
            this.chkProblem.Text = "解決すべき問題に関する情報ですか？(モンスターですか？）";
            this.chkProblem.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 185);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(276, 12);
            this.label7.TabIndex = 24;
            this.label7.Text = "※RSS://http://XXXXという書き方でRSSを購読できます";
            // 
            // btnFolder
            // 
            this.btnFolder.Location = new System.Drawing.Point(103, 443);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(90, 45);
            this.btnFolder.TabIndex = 26;
            this.btnFolder.Text = "フォルダ追加";
            this.btnFolder.UseVisualStyleBackColor = true;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // btnClearList
            // 
            this.btnClearList.Location = new System.Drawing.Point(199, 443);
            this.btnClearList.Name = "btnClearList";
            this.btnClearList.Size = new System.Drawing.Size(90, 45);
            this.btnClearList.TabIndex = 27;
            this.btnClearList.Text = "リストクリア";
            this.btnClearList.UseVisualStyleBackColor = true;
            this.btnClearList.Click += new System.EventHandler(this.btnCleraList_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 443);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 45);
            this.button1.TabIndex = 28;
            this.button1.Text = "ファイル追加";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnRemoveUnusedFile
            // 
            this.btnRemoveUnusedFile.Location = new System.Drawing.Point(295, 443);
            this.btnRemoveUnusedFile.Name = "btnRemoveUnusedFile";
            this.btnRemoveUnusedFile.Size = new System.Drawing.Size(90, 45);
            this.btnRemoveUnusedFile.TabIndex = 29;
            this.btnRemoveUnusedFile.Text = "未指定ファイルのクリア";
            this.btnRemoveUnusedFile.UseVisualStyleBackColor = true;
            this.btnRemoveUnusedFile.Click += new System.EventHandler(this.btnRemoveUnusedFiles_Click);
            // 
            // listFiles
            // 
            this.listFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listFiles.FormattingEnabled = true;
            this.listFiles.Location = new System.Drawing.Point(15, 338);
            this.listFiles.Name = "listFiles";
            this.listFiles.Size = new System.Drawing.Size(446, 88);
            this.listFiles.TabIndex = 30;
            // 
            // FormAddItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 551);
            this.Controls.Add(this.listFiles);
            this.Controls.Add(this.btnRemoveUnusedFile);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnClearList);
            this.Controls.Add(this.btnFolder);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.chkProblem);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnItemNew);
            this.Controls.Add(this.btnItemOld);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.picItem);
            this.Controls.Add(this.btnHeaderPic);
            this.Controls.Add(this.picHeader);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtHeader);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "FormAddItem";
            this.Text = "技術情報の追加";
            this.Load += new System.EventHandler(this.FormAddItem_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picItem)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox txtHeader;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox picHeader;
        private System.Windows.Forms.Button btnHeaderPic;
        private System.Windows.Forms.PictureBox picItem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnItemOld;
        private System.Windows.Forms.Button btnItemNew;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkProblem;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.Button btnClearList;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnRemoveUnusedFile;
        private System.Windows.Forms.CheckedListBox listFiles;
    }
}
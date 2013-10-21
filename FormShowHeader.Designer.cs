namespace GodaiQuest
{
    partial class FormShowHeader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormShowHeader));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.picHeader = new System.Windows.Forms.PictureBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRefPic = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtAshiato = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.picItemImage = new System.Windows.Forms.PictureBox();
            this.btnChangeItemImage = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picItemImage)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 171);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(363, 191);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // picHeader
            // 
            this.picHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picHeader.Location = new System.Drawing.Point(12, 12);
            this.picHeader.Name = "picHeader";
            this.picHeader.Size = new System.Drawing.Size(128, 128);
            this.picHeader.TabIndex = 1;
            this.picHeader.TabStop = false;
            this.picHeader.Visible = false;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 93);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(174, 72);
            this.btnOpen.TabIndex = 2;
            this.btnOpen.Text = "ダウンロードとメッセージ書き込み";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(301, 386);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 38);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "閉じる";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnRefPic
            // 
            this.btnRefPic.Location = new System.Drawing.Point(146, 12);
            this.btnRefPic.Name = "btnRefPic";
            this.btnRefPic.Size = new System.Drawing.Size(32, 128);
            this.btnRefPic.TabIndex = 4;
            this.btnRefPic.Text = "参照";
            this.btnRefPic.UseVisualStyleBackColor = true;
            this.btnRefPic.Visible = false;
            this.btnRefPic.Click += new System.EventHandler(this.btnRefPic_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(161, 386);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(126, 38);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "変更を保存する";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtAshiato
            // 
            this.txtAshiato.AcceptsReturn = true;
            this.txtAshiato.AcceptsTab = true;
            this.txtAshiato.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAshiato.Location = new System.Drawing.Point(192, 31);
            this.txtAshiato.Multiline = true;
            this.txtAshiato.Name = "txtAshiato";
            this.txtAshiato.ReadOnly = true;
            this.txtAshiato.Size = new System.Drawing.Size(182, 134);
            this.txtAshiato.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(193, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "足あと";
            // 
            // picItemImage
            // 
            this.picItemImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picItemImage.Location = new System.Drawing.Point(12, 12);
            this.picItemImage.Name = "picItemImage";
            this.picItemImage.Size = new System.Drawing.Size(64, 64);
            this.picItemImage.TabIndex = 8;
            this.picItemImage.TabStop = false;
            // 
            // btnChangeItemImage
            // 
            this.btnChangeItemImage.Location = new System.Drawing.Point(82, 12);
            this.btnChangeItemImage.Name = "btnChangeItemImage";
            this.btnChangeItemImage.Size = new System.Drawing.Size(105, 64);
            this.btnChangeItemImage.TabIndex = 9;
            this.btnChangeItemImage.Text = "アイテム画像変更";
            this.btnChangeItemImage.UseVisualStyleBackColor = true;
            this.btnChangeItemImage.Click += new System.EventHandler(this.btnChangeItemImage_Click);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 371);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(276, 12);
            this.label7.TabIndex = 25;
            this.label7.Text = "※RSS://http://XXXXという書き方でRSSを購読できます";
            // 
            // FormShowHeader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(387, 431);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnChangeItemImage);
            this.Controls.Add(this.picItemImage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtAshiato);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnRefPic);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.picHeader);
            this.Controls.Add(this.richTextBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormShowHeader";
            this.Text = "アイテムヘッダー";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormShowHeader_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picItemImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.PictureBox picHeader;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRefPic;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtAshiato;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox picItemImage;
        private System.Windows.Forms.Button btnChangeItemImage;
        private System.Windows.Forms.Label label7;
    }
}
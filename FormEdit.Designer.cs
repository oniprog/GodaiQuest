namespace GodaiQuest
{
    partial class FormEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEdit));
            this.picScreen = new System.Windows.Forms.PictureBox();
            this.btnAddBlockImage = new System.Windows.Forms.Button();
            this.btnEnlargeDungeon = new System.Windows.Forms.Button();
            this.btnAddTechInfo = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.cmbEditArea = new System.Windows.Forms.ComboBox();
            this.scrEditV = new System.Windows.Forms.VScrollBar();
            this.scrEditH = new System.Windows.Forms.HScrollBar();
            this.listView1 = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.chkLock = new System.Windows.Forms.CheckBox();
            this.btnEditBlockList = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picScreen)).BeginInit();
            this.SuspendLayout();
            // 
            // picScreen
            // 
            this.picScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picScreen.Location = new System.Drawing.Point(141, 12);
            this.picScreen.Name = "picScreen";
            this.picScreen.Size = new System.Drawing.Size(640, 640);
            this.picScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picScreen.TabIndex = 1;
            this.picScreen.TabStop = false;
            this.picScreen.DoubleClick += new System.EventHandler(this.picScreen_DoubleClick);
            this.picScreen.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.picScreen_MouseDoubleClick);
            this.picScreen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picScreen_MouseDown);
            this.picScreen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picScreen_MouseMove);
            this.picScreen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picScreen_MouseUp);
            // 
            // btnAddBlockImage
            // 
            this.btnAddBlockImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddBlockImage.Location = new System.Drawing.Point(812, 458);
            this.btnAddBlockImage.Name = "btnAddBlockImage";
            this.btnAddBlockImage.Size = new System.Drawing.Size(104, 42);
            this.btnAddBlockImage.TabIndex = 5;
            this.btnAddBlockImage.Text = "新規タイルの作成";
            this.btnAddBlockImage.UseVisualStyleBackColor = true;
            this.btnAddBlockImage.Click += new System.EventHandler(this.btnAddBlockImage_Click);
            // 
            // btnEnlargeDungeon
            // 
            this.btnEnlargeDungeon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnlargeDungeon.Location = new System.Drawing.Point(812, 407);
            this.btnEnlargeDungeon.Name = "btnEnlargeDungeon";
            this.btnEnlargeDungeon.Size = new System.Drawing.Size(104, 36);
            this.btnEnlargeDungeon.TabIndex = 6;
            this.btnEnlargeDungeon.Text = "空間を広げる";
            this.btnEnlargeDungeon.UseVisualStyleBackColor = true;
            this.btnEnlargeDungeon.Click += new System.EventHandler(this.btnEnlargeDungeon_Click);
            // 
            // btnAddTechInfo
            // 
            this.btnAddTechInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddTechInfo.Location = new System.Drawing.Point(816, 106);
            this.btnAddTechInfo.Name = "btnAddTechInfo";
            this.btnAddTechInfo.Size = new System.Drawing.Size(104, 36);
            this.btnAddTechInfo.TabIndex = 7;
            this.btnAddTechInfo.Text = "技術情報を配置";
            this.btnAddTechInfo.UseVisualStyleBackColor = true;
            this.btnAddTechInfo.Click += new System.EventHandler(this.btnAddTechInfo_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(812, 549);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(104, 42);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "保存する";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(812, 611);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(104, 42);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "閉じる";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // cmbEditArea
            // 
            this.cmbEditArea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbEditArea.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEditArea.FormattingEnabled = true;
            this.cmbEditArea.Location = new System.Drawing.Point(814, 44);
            this.cmbEditArea.Name = "cmbEditArea";
            this.cmbEditArea.Size = new System.Drawing.Size(97, 20);
            this.cmbEditArea.TabIndex = 10;
            this.cmbEditArea.SelectedIndexChanged += new System.EventHandler(this.cmbEditArea_SelectedIndexChanged);
            this.cmbEditArea.SelectedValueChanged += new System.EventHandler(this.cmbEditArea_SelectedValueChanged);
            // 
            // scrEditV
            // 
            this.scrEditV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrEditV.Location = new System.Drawing.Point(784, 12);
            this.scrEditV.Name = "scrEditV";
            this.scrEditV.Size = new System.Drawing.Size(19, 643);
            this.scrEditV.TabIndex = 11;
            this.scrEditV.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrEditV_Scroll);
            // 
            // scrEditH
            // 
            this.scrEditH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrEditH.Location = new System.Drawing.Point(141, 655);
            this.scrEditH.Name = "scrEditH";
            this.scrEditH.Size = new System.Drawing.Size(639, 19);
            this.scrEditH.TabIndex = 12;
            this.scrEditH.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrEditH_Scroll);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(9, 12);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(119, 579);
            this.listView1.TabIndex = 13;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(64, 64);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // chkLock
            // 
            this.chkLock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLock.AutoSize = true;
            this.chkLock.Location = new System.Drawing.Point(816, 12);
            this.chkLock.Name = "chkLock";
            this.chkLock.Size = new System.Drawing.Size(95, 16);
            this.chkLock.TabIndex = 14;
            this.chkLock.Text = "編集からガード";
            this.chkLock.UseVisualStyleBackColor = true;
            // 
            // btnEditBlockList
            // 
            this.btnEditBlockList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditBlockList.Location = new System.Drawing.Point(12, 611);
            this.btnEditBlockList.Name = "btnEditBlockList";
            this.btnEditBlockList.Size = new System.Drawing.Size(116, 36);
            this.btnEditBlockList.TabIndex = 15;
            this.btnEditBlockList.Text = "使用タイルの選択";
            this.btnEditBlockList.UseVisualStyleBackColor = true;
            this.btnEditBlockList.Click += new System.EventHandler(this.btnEditBlockList_Click);
            // 
            // FormEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(932, 680);
            this.Controls.Add(this.btnEditBlockList);
            this.Controls.Add(this.chkLock);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.scrEditH);
            this.Controls.Add(this.scrEditV);
            this.Controls.Add(this.cmbEditArea);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnAddTechInfo);
            this.Controls.Add(this.btnEnlargeDungeon);
            this.Controls.Add(this.btnAddBlockImage);
            this.Controls.Add(this.picScreen);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormEdit";
            this.Text = "ダンジョン編集画面";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEdit_FormClosing);
            this.Load += new System.EventHandler(this.FormEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picScreen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picScreen;
        private System.Windows.Forms.Button btnAddBlockImage;
        private System.Windows.Forms.Button btnEnlargeDungeon;
        private System.Windows.Forms.Button btnAddTechInfo;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ComboBox cmbEditArea;
        private System.Windows.Forms.VScrollBar scrEditV;
        private System.Windows.Forms.HScrollBar scrEditH;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.CheckBox chkLock;
        private System.Windows.Forms.Button btnEditBlockList;
    }
}
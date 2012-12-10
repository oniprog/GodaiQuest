namespace GodaiQuest
{
    partial class FormBlockImageList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBlockImageList));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.listView1 = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGoRight = new System.Windows.Forms.Button();
            this.btnGoLeft = new System.Windows.Forms.Button();
            this.listView2 = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.picSelect = new System.Windows.Forms.PictureBox();
            this.labCreateDate = new System.Windows.Forms.Label();
            this.labCreator = new System.Windows.Forms.Label();
            this.labName = new System.Windows.Forms.Label();
            this.btnModify = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btmCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSelect)).BeginInit();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(64, 64);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // listView1
            // 
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(14, 45);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(129, 458);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.Click += new System.EventHandler(this.listView1_Click);
            this.listView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "全部";
            // 
            // btnGoRight
            // 
            this.btnGoRight.Font = new System.Drawing.Font("MS UI Gothic", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnGoRight.Location = new System.Drawing.Point(169, 66);
            this.btnGoRight.Name = "btnGoRight";
            this.btnGoRight.Size = new System.Drawing.Size(141, 54);
            this.btnGoRight.TabIndex = 2;
            this.btnGoRight.Text = "→";
            this.btnGoRight.UseVisualStyleBackColor = true;
            this.btnGoRight.Click += new System.EventHandler(this.btnGoRight_Click);
            // 
            // btnGoLeft
            // 
            this.btnGoLeft.Font = new System.Drawing.Font("MS UI Gothic", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnGoLeft.Location = new System.Drawing.Point(169, 143);
            this.btnGoLeft.Name = "btnGoLeft";
            this.btnGoLeft.Size = new System.Drawing.Size(141, 54);
            this.btnGoLeft.TabIndex = 3;
            this.btnGoLeft.Text = "←";
            this.btnGoLeft.UseVisualStyleBackColor = true;
            this.btnGoLeft.Click += new System.EventHandler(this.btnGoLeft_Click);
            // 
            // listView2
            // 
            this.listView2.LargeImageList = this.imageList1;
            this.listView2.Location = new System.Drawing.Point(333, 43);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(125, 458);
            this.listView2.TabIndex = 4;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.Click += new System.EventHandler(this.listView2_Click);
            this.listView2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView2_MouseClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.picSelect);
            this.groupBox1.Controls.Add(this.labCreateDate);
            this.groupBox1.Controls.Add(this.labCreator);
            this.groupBox1.Controls.Add(this.labName);
            this.groupBox1.Location = new System.Drawing.Point(169, 253);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(141, 206);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // picSelect
            // 
            this.picSelect.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picSelect.Location = new System.Drawing.Point(36, 38);
            this.picSelect.Name = "picSelect";
            this.picSelect.Size = new System.Drawing.Size(64, 64);
            this.picSelect.TabIndex = 3;
            this.picSelect.TabStop = false;
            // 
            // labCreateDate
            // 
            this.labCreateDate.AutoSize = true;
            this.labCreateDate.Location = new System.Drawing.Point(6, 174);
            this.labCreateDate.Name = "labCreateDate";
            this.labCreateDate.Size = new System.Drawing.Size(87, 12);
            this.labCreateDate.TabIndex = 2;
            this.labCreateDate.Text = "作成日付：XXXX";
            // 
            // labCreator
            // 
            this.labCreator.AutoSize = true;
            this.labCreator.Location = new System.Drawing.Point(25, 147);
            this.labCreator.Name = "labCreator";
            this.labCreator.Size = new System.Drawing.Size(75, 12);
            this.labCreator.TabIndex = 1;
            this.labCreator.Text = "作成者：XXXX";
            // 
            // labName
            // 
            this.labName.AutoSize = true;
            this.labName.Location = new System.Drawing.Point(25, 122);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(63, 12);
            this.labName.TabIndex = 0;
            this.labName.Text = "名前：XXXX";
            // 
            // btnModify
            // 
            this.btnModify.Location = new System.Drawing.Point(35, 526);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(91, 34);
            this.btnModify.TabIndex = 6;
            this.btnModify.Text = "修正";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btnModify_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(281, 526);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(91, 34);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btmCancel
            // 
            this.btmCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btmCancel.Location = new System.Drawing.Point(378, 526);
            this.btmCancel.Name = "btmCancel";
            this.btmCancel.Size = new System.Drawing.Size(91, 34);
            this.btmCancel.TabIndex = 8;
            this.btmCancel.Text = "Cancel";
            this.btmCancel.UseVisualStyleBackColor = true;
            this.btmCancel.Click += new System.EventHandler(this.btmCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(331, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "あなたのパレット内";
            // 
            // FormBlockImageList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 578);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btmCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnModify);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.btnGoLeft);
            this.Controls.Add(this.btnGoRight);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBlockImageList";
            this.Text = "タイルの選択";
            this.Load += new System.EventHandler(this.FormBlockImageList_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSelect)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnGoRight;
        private System.Windows.Forms.Button btnGoLeft;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox picSelect;
        private System.Windows.Forms.Label labCreateDate;
        private System.Windows.Forms.Label labCreator;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btmCancel;
        private System.Windows.Forms.Label label2;
    }
}
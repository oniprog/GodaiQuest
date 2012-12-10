namespace GodaiQuest
{
    partial class FormUseExperience
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUseExperience));
            this.radExpandX = new System.Windows.Forms.RadioButton();
            this.radExpandY = new System.Windows.Forms.RadioButton();
            this.radMakeNewDungeon = new System.Windows.Forms.RadioButton();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.labExpandX = new System.Windows.Forms.Label();
            this.labExpandY = new System.Windows.Forms.Label();
            this.labCreateNewFloor = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // radExpandX
            // 
            this.radExpandX.AutoSize = true;
            this.radExpandX.Location = new System.Drawing.Point(15, 19);
            this.radExpandX.Name = "radExpandX";
            this.radExpandX.Size = new System.Drawing.Size(127, 16);
            this.radExpandX.TabIndex = 0;
            this.radExpandX.TabStop = true;
            this.radExpandX.Text = "横にダンジョンを広げる";
            this.radExpandX.UseVisualStyleBackColor = true;
            // 
            // radExpandY
            // 
            this.radExpandY.AutoSize = true;
            this.radExpandY.Location = new System.Drawing.Point(15, 41);
            this.radExpandY.Name = "radExpandY";
            this.radExpandY.Size = new System.Drawing.Size(127, 16);
            this.radExpandY.TabIndex = 1;
            this.radExpandY.TabStop = true;
            this.radExpandY.Text = "縦にダンジョンを広げる";
            this.radExpandY.UseVisualStyleBackColor = true;
            // 
            // radMakeNewDungeon
            // 
            this.radMakeNewDungeon.AutoSize = true;
            this.radMakeNewDungeon.Location = new System.Drawing.Point(15, 63);
            this.radMakeNewDungeon.Name = "radMakeNewDungeon";
            this.radMakeNewDungeon.Size = new System.Drawing.Size(129, 16);
            this.radMakeNewDungeon.TabIndex = 2;
            this.radMakeNewDungeon.TabStop = true;
            this.radMakeNewDungeon.Text = "地下に新しい階を作る";
            this.radMakeNewDungeon.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 98);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 31);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(192, 98);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 31);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // labExpandX
            // 
            this.labExpandX.AutoSize = true;
            this.labExpandX.Location = new System.Drawing.Point(148, 21);
            this.labExpandX.Name = "labExpandX";
            this.labExpandX.Size = new System.Drawing.Size(35, 12);
            this.labExpandX.TabIndex = 5;
            this.labExpandX.Text = "label1";
            // 
            // labExpandY
            // 
            this.labExpandY.AutoSize = true;
            this.labExpandY.Location = new System.Drawing.Point(148, 43);
            this.labExpandY.Name = "labExpandY";
            this.labExpandY.Size = new System.Drawing.Size(35, 12);
            this.labExpandY.TabIndex = 6;
            this.labExpandY.Text = "label1";
            // 
            // labCreateNewFloor
            // 
            this.labCreateNewFloor.AutoSize = true;
            this.labCreateNewFloor.Location = new System.Drawing.Point(148, 65);
            this.labCreateNewFloor.Name = "labCreateNewFloor";
            this.labCreateNewFloor.Size = new System.Drawing.Size(35, 12);
            this.labCreateNewFloor.TabIndex = 7;
            this.labCreateNewFloor.Text = "label1";
            // 
            // FormUseExperience
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 141);
            this.Controls.Add(this.labCreateNewFloor);
            this.Controls.Add(this.labExpandY);
            this.Controls.Add(this.labExpandX);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.radMakeNewDungeon);
            this.Controls.Add(this.radExpandY);
            this.Controls.Add(this.radExpandX);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUseExperience";
            this.ShowInTaskbar = false;
            this.Text = "経験値を使用する";
            this.Load += new System.EventHandler(this.FormUseExperience_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radExpandX;
        private System.Windows.Forms.RadioButton radExpandY;
        private System.Windows.Forms.RadioButton radMakeNewDungeon;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label labExpandX;
        private System.Windows.Forms.Label labExpandY;
        private System.Windows.Forms.Label labCreateNewFloor;
    }
}
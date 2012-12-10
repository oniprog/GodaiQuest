using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GodaiQuestServer
{
    public partial class FormInputLine : Form
    {
        private String mHeader;
        private String mDefaultValue;
        private String mResult;

        public FormInputLine(String strHeader_, String strDefaultValue_ )
        {
            InitializeComponent();

            this.mHeader = strHeader_;
            this.mDefaultValue = strDefaultValue_;
        }

        private void FormInputLine_Load(object sender, EventArgs e)
        {
            this.label1.Text = this.mHeader;
            this.textBox1.Text = this.mDefaultValue;
        }

        public String getResult()
        {
            return this.mResult;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.mResult = this.textBox1.Text;
        }
    }
}

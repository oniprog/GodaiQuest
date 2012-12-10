using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GodaiQuest
{
    public partial class FormMessage : Form
    {
        private String mHeader;
        private String mMessage;

        public FormMessage(String strHeader, String strMes)
        {
            InitializeComponent();
            this.mHeader = strHeader;
            this.mMessage = strMes;
        }

        private void FormMessage_Load(object sender, EventArgs e)
        {
            this.Text = this.mHeader;
            this.txtSystemMessage.Text = this.mMessage;
        }
    }
}

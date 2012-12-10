using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace GodaiQuest
{
    public partial class FormLogOn : Form
    {
        private GQCommandMaster mGQCom;

        public FormLogOn()
        {
            InitializeComponent();
        }

        private void processRecord()
        {
            String strMail = "", strPassword = "";
            if (this.chkRecord.Checked)
            {
                strMail = textMail.Text;
                strPassword = textPassword.Text;
            }
            Settings.Instance.Mail = strMail;
            Settings.Instance.Password = strPassword;
            Settings.Instance.Memory = this.chkRecord.Checked;
            Settings.SaveToXmlFile();
        }


        private void FormLogOn_Load(object sender, EventArgs e)
        {
            try
            {
                Settings.LoadFromXmlFile();
            }
            catch (Exception) {}

            this.textMail.Text = Settings.Instance.Mail;
            this.textPassword.Text = Settings.Instance.Password;
            this.chkRecord.Checked = Settings.Instance.Memory;
        }


        private void btnEnd_Click(object sender, EventArgs e)
        {
            processRecord();
            DialogResult = DialogResult.Cancel;
        }

        private bool checkInput()
        {
            String strMail = textMail.Text;
            String strPassword = textPassword.Text;
            if (strMail.Length == 0)
            {
                MessageBox.Show("メールアドレスを入力してください");
                return false;
            }
            if (strPassword.Length == 0)
            {
                MessageBox.Show("パスワードを入力してください");
                return false;
            }
            return true;
        }

        public GQCommandMaster getConnect()
        {
            return this.mGQCom;
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            processRecord();
            if (!checkInput())
                return;

            String strMail = textMail.Text;
            String strPassword = textPassword.Text;
            try
            {
                this.mGQCom = new GQCommandMaster();
                if (this.mGQCom.tryLogon(strMail, strPassword))
                {
                    // ログイン成功
                    this.DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show(this.mGQCom.getErrorReasonString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnNew_Click(object sender, EventArgs e)
        {
            processRecord();

            if (!checkInput())
                return;

            String strMail = textMail.Text;
            String strPassword = textPassword.Text;

            if (MessageBox.Show("入力されたメールアドレスとパスワードで作成します。よろしいですか？", "Notify", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                return;

            FormCreateAccount formAccount = new FormCreateAccount(true);
            formAccount.setInit(strMail, strPassword);
            formAccount.ShowDialog();
        }

        private void FormLogOn_FormClosed(object sender, FormClosedEventArgs e)
        {
            processRecord();
        }
    }
}

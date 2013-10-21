using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuest
{
    public partial class FormCreateAccount : Form
    {
        private String mMail;
        private String mPassword;
        private bool mCreateAccount;
        private AUser mUser;
        private GQCommandMaster mGQCom;

        public FormCreateAccount(bool bCreateAccount)
        {
            InitializeComponent();

            this.mCreateAccount = bCreateAccount;
        }

        private void FormCreateAccount_Load(object sender, EventArgs e)
        {
            if (!this.mCreateAccount)
            {
                this.txtName.Text = this.mUser.getName();
                this.picPlayer.Image = this.mUser.getCharacterImage();
                this.txtDownloadFolder.Text = ""; // 最初は空
            }
            else
            {
                this.txtPassword.Text = this.mPassword;
            }
        }

        public void setInitForChangeUserInfo(AUser user, GQCommandMaster gqcom)
        {
            this.mUser = user;
            this.mGQCom = gqcom;
        }

        public void setInit(String strMail, String strPassword)
        {
            this.mMail = strMail;
            this.mPassword = strPassword;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.txtName.Text.Length == 0)
            {
                MessageBox.Show("名前を指定してください");
                return;
            }
            if (this.picPlayer.Image == null)
            {
                MessageBox.Show("キャラクターの絵を指定してください");
                return;
            }
            if (this.mCreateAccount && this.txtDownloadFolder.Text.Length == 0)
            {
                MessageBox.Show("ダウンロード先フォルダを指定してください");
                return;
            }
            if (this.mCreateAccount && this.txtPassword.Text.Length == 0)
            {
                MessageBox.Show("パスワードを指定してください");
                return;
            }

            //
            if (this.txtDownloadFolder.Text.Length > 0)
            {
                Directory.CreateDirectory(this.txtDownloadFolder.Text);
                if (!Directory.Exists(this.txtDownloadFolder.Text))
                {
                    MessageBox.Show("ダウンロードフォルダが作れませんでした");
                    return;
                }
            }
			
            try
            {
                if (this.mCreateAccount)
                {
                    //　保存
                    GQCommandMaster gqcom = new GQCommandMaster();
                    if (gqcom.addUser(this.mMail, this.mPassword, this.txtName.Text, this.picPlayer.Image, this.txtDownloadFolder.Text))
                    {
                        MessageBox.Show("登録が終了しました");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(gqcom.getErrorReasonString());
                        Close();
                    }
                }
                else
                {
                    // 変更
                    this.mUser.setName(this.txtName.Text);
                    this.mUser.setCharacterImage(this.picPlayer.Image);
                    if (this.mGQCom.setAUser(this.mUser))
                    {
                        if (txtDownloadFolder.Text.Length > 0 && !this.mGQCom.setUserFolder(this.mUser.getUserID(), txtDownloadFolder.Text))
                        {
                            MessageBox.Show("ユーザフォルダの設定変更に失敗しました");
                        }

                        if (this.txtPassword.Text.Length != 0)
                        {
                            Settings.Instance.Password = this.txtPassword.Text;

                            if (!this.mGQCom.changePassword(GodaiLibrary.Crypto.calcPasswordHash(this.txtPassword.Text)))
                            {
                                MessageBox.Show(this.mGQCom.getErrorReasonString());
                                // でも終了する…
                            }
                            else
                            {
                                MessageBox.Show("パスワードを変更しました");
                            }
                        }
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Settings.Instance.DownloadFolder = txtDownloadFolder.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void btnPic_Click(object sender, EventArgs e)
        {
            this.picPlayer.Image = GodaiLibrary.KLib.loadAndResizeImage(64,64);
        }

        private void btnDownloadFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            txtDownloadFolder.Text = dlg.SelectedPath;
        }
    }
}

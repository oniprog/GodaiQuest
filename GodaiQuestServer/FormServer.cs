using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Linq.Expressions;

using GodaiLibrary.GodaiQuest;

namespace GodaiQuestServer
{

    //
    public partial class FormServer : Form
    {
        private ServerWorker mServerWorker;

        public FormServer()
        {
            InitializeComponent();
        }

        private List<String> mLogList = new List<string>();

        // ログに文字列を追加する
		// 別スレッドから呼ばれる想定
        public void addLog(String strLog)
        {
            DateTime now = DateTime.Now.ToLocalTime();
            strLog = "["+now.ToShortDateString()+" "+now.ToShortTimeString() + "]:" + strLog;

            this.mLogList.Add(strLog);
            if (this.mLogList.Count > 10000)
                this.mLogList.RemoveAt(0);
            StringBuilder strL = new StringBuilder();
            for (int it = this.mLogList.Count - 1; it >= 0; --it)
            {
                strL.AppendLine(this.mLogList[it]);
            }
            txtLog.SetPropertyThreadSafe(() => txtLog.Text, strL.ToString());
        }

        public void setWorker(ServerWorker worker_)
        {
            this.mServerWorker = worker_;
        }

        private void btnSendServerMes_Click(object sender, EventArgs e)
        {
            this.mServerWorker.setSystemMessage(this.txtServerMes.Text);
        }

        private List<AUser> mUserList = new List<AUser>();
        private UserInfo mUserInfo;

        private void tabUser_Click(object sender, EventArgs e)
        {

        }

        private void cmbUserName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = this.cmbUserName.SelectedIndex;
            if (nIndex < 0)
                return;
            
            var auser = this.mUserList[nIndex];
            this.txtUserInfo.Text =
                "名前   : " + auser.getName() + "\r\n" +
                "メール : " + auser.getMail() + "\r\n"; 
                

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            MessageBox.Show("まだ実装していません");
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            int nIndex = this.cmbUserName.SelectedIndex;
            if (nIndex < 0)
                return;

            FormInputLine formInput = new FormInputLine("新パスワード", "");
            if ( formInput.ShowDialog() != DialogResult.OK )
                return;

            if (formInput.getResult().Length == 0)
            {
                MessageBox.Show("空のパスワードは許可されません");
                return;
            }

            var user = this.mUserList[nIndex];
            String strPasswordHash = GodaiLibrary.Crypto.CalcPasswordHash(formInput.getResult());
            this.mServerWorker.changePassword( user.getUserID(), strPasswordHash);
            MessageBox.Show("パスワードを変更しました");
        }

        private void btnChangeEMail_Click(object sender, EventArgs e)
        {
            MessageBox.Show("まだ実装していません");

        }

        private void FormServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.mServerWorker.terminate();
        }

        private void tabUser_Enter(object sender, EventArgs e)
        {
            this.mServerWorker.getUserInfo(out this.mUserInfo);

            this.mUserList.Clear();
            this.cmbUserName.Items.Clear();

            foreach (var auser in this.mUserInfo)
            {
                this.cmbUserName.Items.Add(auser.getName());
                this.mUserList.Add(auser);
            }
        }

        private void btnInitDefault_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("完全に初期化します．よろしいですか？", "警告", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            ItemInfo iteminfo;
            mServerWorker.getItemInfo(out iteminfo);
            if (iteminfo.Count() != 0)
            {
                MessageBox.Show("mongodbを停止したあと，c:\\data\\dbフォルダの中身を空にしてください．その後，再びこのボタンを押してください");
                return;
            }

            //var formMail = new FormInputLine("管理ユーザのメールアドレス(ログイン用)", "");
            //if (formMail.ShowDialog() != DialogResult.OK)
             //   return;

            mServerWorker.ForceInitializeMongoDB();
        }
    }

    // 拡張メソッド用のクラス
    static class Extention
    {
        // Webより
        private delegate void SetPropertyThreadSafeDelegate<TResult>(Control @this, Expression<Func<TResult>> property, TResult value);

        // 拡張メソッド
        public static void SetPropertyThreadSafe<TResult>(this Control @this, Expression<Func<TResult>> property, TResult value)
        {
            var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;

            if (propertyInfo == null ||
                !@this.GetType().IsSubclassOf(propertyInfo.ReflectedType) ||
                @this.GetType().GetProperty(propertyInfo.Name, propertyInfo.PropertyType) == null)
            {
                throw new ArgumentException("The lambda expression 'property' must reference a valid property on this Control.");
            }

            if (@this.InvokeRequired)
            {
                @this.Invoke(new SetPropertyThreadSafeDelegate<TResult>(SetPropertyThreadSafe), new object[] { @this, property, value });
            }
            else
            {
                @this.GetType().InvokeMember(propertyInfo.Name, BindingFlags.SetProperty, null, @this, new object[] { value });
            }
        }

    }
}

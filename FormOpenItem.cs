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
    public partial class FormOpenItem : Form
    {
        private String mFolder;
        private GQCommandMaster mGQCom;
        private int mItemID;
        private bool mOwner;
        private MonsterInfo mMonsterInfo;
        private int mDungeonID;
        private AItem mItem;
        private ALocation mLocation;
        private DungeonInfo mDungeonInfo;
        private bool mChangeDungeon;

        public FormOpenItem(bool bOwner_, bool bShowChangeItemBtn, String strFolder, GQCommandMaster gqcom_, int nItemID_, int nDungeonID_, ALocation location_, DungeonInfo dungeonInfo_)
        {
            InitializeComponent();
            if (!bOwner_ || !bShowChangeItemBtn)
                btnChangeAItem.Visible = false;

            mChangeDungeon = false;
            this.mFolder = strFolder;
            this.mGQCom = gqcom_;
            this.mItemID = nItemID_;
            this.mOwner = bOwner_;
            this.mDungeonID = nDungeonID_;
            this.mLocation = location_;
            this.mDungeonInfo = dungeonInfo_;
        }

        private void FormOpenItem_Load(object sender, EventArgs e)
        {
            makeListView();

            // アップロードボタンはオーナーのみの特権
            this.btnUpload.Visible = this.mOwner;

            // 退治、状態チェンジもオーナーの特権
            this.btnChangeState.Visible = this.mOwner;

            this.setTitle();

            this.loadArticle();
        }

        private void loadArticle()
        {
			// アイテム概要を得る
            ItemInfo itemInfo = new ItemInfo();
            this.mGQCom.getItemInfo(out itemInfo);
            var item = itemInfo.getAItem(this.mItemID);
            this.richTextBox3.Text = item.getHeaderString();
            this.mItem = item;
            
            // アーティクルをセットする
            richTextBox1.Text = this.mGQCom.getAritcleString(this.mItemID);

            // 未読を消化
            this.mGQCom.readArticle(this.mItemID, this.mGQCom.getUserID());
        }

        private void setTitle()
        {
            this.mGQCom.getMonsterInfo(out this.mMonsterInfo, this.mDungeonID );
            if (this.mMonsterInfo.isMonster(this.mItemID))
            {
                this.Text = "モンスターの情報";
                this.btnChangeState.Text = "モンスタ（問題）を退治した！";
            }
            else
            {
                this.Text = "アイテムの内容";
                this.btnChangeState.Text = "モンスタ(問題）になった";
            }


        }

#if true
        private void makeListView()
        {
            this.listView1.Items.Clear();
            this.makeListView(Path.GetFullPath(this.mFolder), "");
        }
        private void makeListView( String strSearchPath, string strAddPath ) {
            DirectoryInfo dirinfo = new DirectoryInfo(strSearchPath);
            foreach (var file in dirinfo.GetFiles())
            {
                this.listView1.Items.Add(Path.Combine(strAddPath, file.Name));
            }
            foreach (var dir in dirinfo.GetDirectories())
            {
                makeListView(dir.FullName, Path.Combine(strAddPath, Path.GetFileName(dir.Name)));
            }
        }
#endif

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("EXPLORER.exe", this.mFolder);
        }

#if true
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);
            if (item == null)
                return;
            System.Diagnostics.Process.Start(Path.Combine(this.mFolder, item.Text), "");
        }
#endif
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void makeUploadList(HashSet<FilePair> setFiles, String strFullPath, String strSubPath)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(strFullPath);
            foreach ( var file in dirinfo.GetFiles())
            {
                setFiles.Add(new FilePair(file.FullName, Path.Combine(strSubPath, Path.GetFileName(file.Name))));
            }
            foreach (var dir in dirinfo.GetDirectories())
            {
                makeUploadList(setFiles, Path.GetFullPath(Path.Combine(strFullPath, dir.Name)), Path.Combine(strSubPath, Path.GetFileName(dir.Name)));
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            HashSet<FilePair> setFiles = new HashSet<FilePair>();

            makeUploadList(setFiles, this.mFolder, "");

            if ( !this.mGQCom.uploadAItemFiles(this.mItemID, setFiles, true) ) {

                MessageBox.Show(this.mGQCom.getErrorReasonString());
            }
            Close();
        }

        /// メッセージ書き込み
        private void btnWriteArticle_Click(object sender, EventArgs e)
        {
            String strWriteMessage = richTextBox2.Text;
            if (strWriteMessage.Length == 0)
            {
                MessageBox.Show("メッセージが空です");
                return;
            }

            ItemArticle article = new ItemArticle(this.mItemID, 0, this.mGQCom.getUserID(), strWriteMessage, DateTime.Now);
            if (!this.mGQCom.setItemArticle(article))
            {
                MessageBox.Show(this.mGQCom.getErrorReasonString());
            }
            else
            {
                loadArticle();
            }
        }

        private void btnDeleteArticle_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("あなたの最後の書き込みを消してよろしいですか？",  "Notice", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                return;

            if (!this.mGQCom.deleteLastItemArticle(this.mItemID))
            {
                MessageBox.Show(this.mGQCom.getErrorReasonString());

            }
            else
            {
                loadArticle();
            }
        }

        private void btnChangeState_Click(object sender, EventArgs e)
        {
            bool bCurState = this.mMonsterInfo.isMonster(this.mItemID);
            this.mGQCom.setMonster(this.mItemID, !bCurState);
            this.setTitle();
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void richTextBox3_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void btnChangeAItem_Click(object sender, EventArgs e)
        {
            var form = new FormShowHeader(this.mItem, this.mGQCom, this.mLocation, this.mDungeonInfo, this.mDungeonID,
                this.mFolder);
            form.ShowDialog();
            this.loadArticle();
            mChangeDungeon = true;
        }

        public bool isChangeDungeon()
        {
            return mChangeDungeon;
        }
    }
}

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
    public partial class FormShowHeader : Form
    {
        private String mUserFolder;
        private String mItemHeader;
        private Image mItemHeaderImage;
        private int mItemID;
        private GQCommandMaster mGQCom;
        private AItem mItem;
        private FormEdit mParent;
        private int mItemImageID;
        private ALocation mLocation;
        private DungeonInfo mDungeon;
        private int mDungeonID;

        public FormShowHeader(AItem item_, GQCommandMaster gqcom_, ALocation location_, DungeonInfo dungeon_, int nDungeonID_, String strUserFolder_)
        {
            InitializeComponent();

            btnChangeItemImage.Visible = false;
            btnOpen.Visible = false;
                
            this.mDungeonID = nDungeonID_;

            this.mItem = item_;
            this.mItemHeader = item_.getHeaderString();
            this.mItemHeaderImage = item_.getHeaderImage();
            this.mItemID = item_.getItemID();
            this.mGQCom = gqcom_;
            this.mItemImageID = this.mItem.getItemImageID();

            // イメージを得る
            DungeonBlockImageInfo images;
            this.mGQCom.getDungeonBlockImage(out images);
            this.picItemImage.Image = images.getImageAt((uint)this.mItemImageID);
            this.mLocation = location_;

            this.mUserFolder = strUserFolder_;
            this.mDungeon = dungeon_;
            
        } 

        public FormShowHeader(FormEdit parent, AItem item_, GQCommandMaster gqcom_, ALocation location_, DungeonInfo dungeon_, int nDungeonID_, String strUserFolder_ ) 
        {
            InitializeComponent();

            this.mDungeonID = nDungeonID_;

            this.mParent = parent;
            this.mItem = item_;
            this.mItemHeader = item_.getHeaderString();
            this.mItemHeaderImage = item_.getHeaderImage();
            this.mItemID = item_.getItemID();
            this.mGQCom = gqcom_;
            this.mItemImageID = this.mItem.getItemImageID();

            // イメージを得る
            DungeonBlockImageInfo images;
            this.mGQCom.getDungeonBlockImage(out images);
            this.picItemImage.Image = images.getImageAt((uint)this.mItemImageID);
            this.mLocation = location_;
            this.mDungeon = dungeon_;


            this.mUserFolder = strUserFolder_;
        }

        private void FormShowHeader_Load(object sender, EventArgs e)
        {
            if (this.mItemHeaderImage == null || this.mItemHeaderImage.Size.Width == 1)
            {
#if false
                // ヘッダーイメージが無いときは、ヘッダー文字列領域を広げる
                this.richTextBox1.Top = this.picHeader.Top;
                this.richTextBox1.Height += this.picHeader.Size.Height;
                this.picHeader.Visible = false;
                this.btnRefPic.Visible = false;
#endif
            }
            else
            {
                this.picHeader.Image = this.mItemHeaderImage;
            }
            this.richTextBox1.Text = this.mItemHeader;

            // 足あとのダウンロード
            makeAshiatoList();
        }

        private void makeAshiatoList()
        {

            PickupedInfo ashiato;
            this.mGQCom.getAshiato(out ashiato, this.mItemID);

            SortedList<DateTime, APickuped> listPrint = new SortedList<DateTime, APickuped>();
            foreach (var apickup in ashiato)
            {
                listPrint.Add(apickup.getDateTime(), apickup);
            }

            List<APickuped> listPrint2 = new List<APickuped>();
            foreach (var apickup in listPrint.Values) 
            {
                listPrint2.Add(apickup);
            }
            listPrint2.Reverse();

            UserInfo userinfo;
            if ( !this.mGQCom.getUserInfo(out userinfo) )
                return;

            StringBuilder strOutput = new StringBuilder();

            foreach (var apickup in listPrint2)
            {
                var user = userinfo.getAUser(apickup.getUserID());
                var datetime = apickup.getDateTime();
                strOutput.AppendLine(datetime.ToLocalTime().ToShortDateString() + " " + datetime.ToLocalTime().ToShortTimeString() + "  : " + user.getName());
            }
            txtAshiato.Text = strOutput.ToString();
        }

        // ダウンロード
        private void btnOpen_Click(object sender, EventArgs e)
        {
            String strFolder = Path.Combine(this.mUserFolder, "" + this.mItemID);
            Hide();
            this.mGQCom.getAItem(this.mItemID, strFolder);
            FormOpenItem dlg = new FormOpenItem(true, false, strFolder, this.mGQCom, this.mItemID, this.mDungeonID, this.mLocation, this.mDungeon);
            dlg.ShowDialog();
            Show();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRefPic_Click(object sender, EventArgs e)
        {
            this.picHeader.Image = GodaiLibrary.KLib.loadAndResizeImage(128, 128);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.mItem = new AItem(this.mItem.getItemID(), this.mItemImageID, this.richTextBox1.Text, this.picHeader.Image, false);
            if (!this.mGQCom.changeAItem(this.mItem))
            {
                MessageBox.Show(this.mGQCom.getErrorReasonString());
            }
            else
            {
                // ダンジョンの可視化情報も変える
                this.mDungeon.setDungeonImageAt(this.mLocation.getIX(), this.mLocation.getIY(),
                    (uint) this.mItemImageID);

                if (this.mParent != null)
                {
                    //
                    this.mParent.loadItemInfo();
                }
                MessageBox.Show("変更を保存しました");
                Close();
            }
        }

        private void btnChangeItemImage_Click(object sender, EventArgs e)
        {
            Hide();
            FormSelectItemImage formSelectItemImage = new FormSelectItemImage(this.mGQCom);
            if (formSelectItemImage.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                Show();
                return;
            }
            Show();

            // 新イメージを設定する
            this.picItemImage.Image = formSelectItemImage.getSelectedImagePair().getImage();
            this.mItemImageID = formSelectItemImage.getSelectedImagePair().getNumber();
        }
    }
}

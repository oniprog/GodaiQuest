using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GodaiLibrary.GodaiQuest;

namespace GodaiQuest
{
    public partial class FormBlockImageList : Form
    {
        private GQCommandMaster mGQCom;
        private TilePalette mPalette;
        private DungeonBlockImageInfo mImageInfo;
        private TileInfo mTileInfo;
        private UserInfo mUserInfo;
        private ObjectAttrInfo mObjectAttrInfo;

        private List<ulong> mImageIDList = new List<ulong>();
        private List<ulong> mImageIDList1 = new List<ulong>();
        private List<ulong> mImageIDList2 = new List<ulong>();

        private bool mModify = false;

        public FormBlockImageList(GQCommandMaster gqcom, DungeonBlockImageInfo imageInfo, TileInfo tileinfo, TilePalette palette, UserInfo userinfo, ObjectAttrInfo objectinfo )
        {
            InitializeComponent();

            this.mGQCom = gqcom;
            this.mImageInfo = imageInfo;
            this.mTileInfo = tileinfo;
            this.mUserInfo = userinfo;
            this.mObjectAttrInfo = objectinfo;

            this.mPalette = palette.copy(); // コピーを保持する
//            loadBlockImagePalette();
        }

        public TilePalette getImagePalette()
        {
            return this.mPalette;
        }

#if false   // 読みだしてはダメ
        private bool loadBlockImagePalette()
        {
            return this.mGQCom.getBlockImagePallette(out this.mPalette, this.mGQCom.getUserID());
        }
#endif

        private void makeImageView1()
        {
            listView1.Items.Clear();
            listView2.Items.Clear();
            imageList1.Images.Clear();
            this.mImageIDList1.Clear();
            this.mImageIDList2.Clear();

            int it=0;
            foreach (var tile in this.mTileInfo)
            {
                uint nImageID = tile.getImageID();
                var imagepair = this.mImageInfo.getImagePairAt(nImageID);

                if (imagepair.canItemImage())
                    continue;

                imageList1.Images.Add(imagepair.getImage());

                ListViewItem item = new ListViewItem();
                item.Text = imagepair.getName();

                item.ImageIndex = it++;

                this.mImageIDList.Add(tile.getTileID());

                if (this.mPalette.containsTileID(tile.getTileID()))
                {
                    listView2.Items.Add(item);
                    this.mImageIDList2.Add(tile.getTileID());
                }
                else
                {
                    listView1.Items.Add(item);
                    this.mImageIDList1.Add(tile.getTileID());
                }
            }
        }

        private void FormBlockImageList_Load(object sender, EventArgs e)
        {
            hideDetail();
            makeImageView1();
        }

        private void btnGoRight_Click(object sender, EventArgs e)
        {
            var indexset = this.listView1.SelectedIndices;
            foreach (var ii in indexset) 
            {
                this.mPalette.addTileID(this.mImageIDList1[(int)ii]);
            }

            makeImageView1();

            this.mModify = true;
        }

        private void btnGoLeft_Click(object sender, EventArgs e)
        {

            var indexset = this.listView2.SelectedIndices;
            foreach (var ii in indexset)
            {
                this.mPalette.removeTileID(this.mImageIDList2[(int)ii]);
            }

            makeImageView1();

            this.mModify = true;
        }

        private void showDetail(ulong nTileID)
        {
            Tile tile = new Tile(nTileID);
            uint nImageID = tile.getImageID();

            var imagepair = this.mImageInfo.getImagePairAt(nImageID);
            var nOwnerID = imagepair.getOwner();
            var auser = this.mUserInfo.getAUser(nOwnerID);
            var strOwnerName = auser.getName(); 

            this.labCreateDate.Text = "作成日: " + imagepair.getCreateTime().ToLocalTime().ToShortDateString() + " " + imagepair.getCreateTime().ToLocalTime().ToShortTimeString();
            this.labCreator.Text = "作成者: " + strOwnerName;
            this.labName.Text = "名前: " + imagepair.getName();

            this.picSelect.Image = auser.getCharacterImage();
        }
        private void hideDetail()
        {
            this.labCreateDate.Text = "";
            this.labName.Text = "";
            this.labCreator.Text = "";
            this.picSelect.Image = null;
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            
        }

        private void listView2_Click(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btmCancel_Click(object sender, EventArgs e)
        {
            if (this.mModify)
            {
                if (MessageBox.Show("閉じてよろしいですか？", "Notice", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                    return;
            }
            Close();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            var item = this.listView1.GetItemAt(e.X, e.Y);
            var nTileID = this.mImageIDList[item.ImageIndex];

            showDetail(nTileID);
        }

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {

            var item = this.listView2.GetItemAt(e.X, e.Y);
            var nTileID = this.mImageIDList[item.ImageIndex];

            showDetail(nTileID);
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count == 0 ) {
                MessageBox.Show("画像が選択されていません");
                return;
            }
            else if (this.listView1.SelectedIndices.Count > 1 ) {
                MessageBox.Show("複数の画像が選択されています");
                return;
            }

            int nSelIndex = this.listView1.SelectedIndices[0];
            var nTileID = this.mImageIDList1[nSelIndex];
            Tile tile = new Tile(nTileID);

            var image = this.mImageInfo.getImagePairAt(tile.getImageID());
            if (image.getOwner() != this.mGQCom.getUserID())
            {
                MessageBox.Show("自分の画像しか編集できません");
                return;
            }

            Hide();
            FormAddBlockImage form = new FormAddBlockImage(this.mGQCom, (int)tile.getObjectID(), (int)tile.getImageID(), this.mImageInfo, this.mObjectAttrInfo, this.mGQCom.getUserID());
            form.ShowDialog();
            Show();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

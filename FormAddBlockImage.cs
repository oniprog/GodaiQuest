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
    public partial class FormAddBlockImage : Form
    {
        private int mUserID;
        private DungeonBlockImageInfo mImages;
        private TileInfo mTileInfo;
        private ObjectAttrInfo mObjectInfo;
        
        private bool mModifyMode;

        private int mObjectID;
        private int mImageID;
        private GQCommandMaster mGQCom;

        // 新規追加モード
        public FormAddBlockImage( DungeonBlockImageInfo images_, ObjectAttrInfo objectinfo_, TileInfo tileinfo, int nUserID_)
        {
            InitializeComponent();

            this.mImages = images_;
            this.mObjectInfo = objectinfo_;
            this.mUserID = nUserID_;
            this.mTileInfo = tileinfo;
            this.mModifyMode = false;
        }

        // 変更モード
        public FormAddBlockImage( GQCommandMaster gqcom, int nObjectID, int nImageID, DungeonBlockImageInfo images_, ObjectAttrInfo objectinfo_, int nUserID_)
        {
            InitializeComponent();

            this.mGQCom = gqcom;
            this.mObjectID = nObjectID;
            this.mImageID = nImageID;
            this.mImages = images_;
            this.mObjectInfo = objectinfo_;
            this.mUserID = nUserID_;
            this.mModifyMode = true;
        }

        private void btnImageRef_Click(object sender, EventArgs e)
        {
            this.picBlockImage.Image = GodaiLibrary.KLib.loadAndResizeImage(64,64);
            if (this.txtName.Text.Length == 0)
            {
                this.txtName.Text = Path.GetFileNameWithoutExtension(GodaiLibrary.KLib.getFileLoadedFilePath());
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.picBlockImage.Image == null)
            {
                MessageBox.Show("イメージを指定してください");
                return;
            }

            if (this.mModifyMode)
            {
                // 修正モード
                var obj = this.mObjectInfo.getObject(this.mObjectID);
                var newobj = new ObjectAttr( obj.getObjectID(), this.chkCanWalk.Checked, obj.getItemID(),(EObjectCommand) this.cmbCommand.SelectedIndex, obj.getObjectCommandSub(), false );
                this.mObjectInfo.removeObject(obj);
                this.mObjectInfo.addObject(newobj);
                this.mGQCom.changeObjectAttr(newobj);

                var imagepair = this.mImages.getImagePairAt((uint)this.mImageID);
                imagepair = new ImagePair(imagepair.getNumber(), imagepair.canItemImage(), this.picBlockImage.Image, this.txtName.Text, imagepair.getOwner(), imagepair.getCreateTime(), false);
                this.mGQCom.changeDungeonBlockImagePair(imagepair);
            }
            else
            {
                // 新規作成モード
                uint nObjectID = (uint)this.mObjectInfo.newObjectID();
                uint nImageID = (uint)this.mImages.newImageNum();

                ObjectAttr objectattr = new ObjectAttr((int)nObjectID, this.chkCanWalk.Checked, 0, (EObjectCommand)this.cmbCommand.SelectedIndex, 0, true);
                this.mObjectInfo.addObject(objectattr);

                this.mImages.addImage(nImageID, false, this.picBlockImage.Image, this.txtName.Text, this.mUserID, DateTime.Now, true);

                Tile tile = new Tile(nObjectID, nImageID);
                this.mTileInfo.addTile(tile);
            }
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormAddBlockImage_Load(object sender, EventArgs e)
        {
//            if (this.mUserID == 1)
                this.cmbCommand.Visible = true;
            this.cmbCommand.SelectedIndex = 0;

            if (this.mModifyMode)
            {
                var imagepair = this.mImages.getImagePairAt((uint)this.mImageID);
                var obj = this.mObjectInfo.getObject(this.mObjectID);
                this.picBlockImage.Image = imagepair.getImage();
                this.txtName.Text = imagepair.getName();
                this.chkCanWalk.Checked = obj.canWalk();
                this.cmbCommand.SelectedIndex = (int)obj.getObjectCommand();
            }
        }
    }
}

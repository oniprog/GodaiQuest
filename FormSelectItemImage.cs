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
    public partial class FormSelectItemImage : Form
    {
        private GQCommandMaster mGQCom;
        private DungeonBlockImageInfo mBlockImages;
        private List<ImagePair> mImageList = new List<ImagePair>();
        private ImagePair mSelectedImagePair;

        public FormSelectItemImage(GQCommandMaster com)
        {
            InitializeComponent();
            this.mGQCom = com;
        }

        private void FormSelectItemImage_Load(object sender, EventArgs e)
        {
            this.mGQCom.getDungeonBlockImage(out this.mBlockImages);

            foreach (var image in this.mBlockImages)
            {
                if ( !image.canItemImage() )
                    continue;

                this.imageList1.Images.Add(image.getImage());
                ListViewItem item = new ListViewItem();
                item.ImageIndex = this.imageList1.Images.Count-1;
                item.Name = image.getName();
                this.listView1.Items.Add(item);
                this.mImageList.Add(image);
            }
        }

        public ImagePair getSelectedImagePair()
        {
            return this.mSelectedImagePair;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count == 0)
            {
                MessageBox.Show("アイテム画像を選択してください");
                return;
            }
            var imagepair = this.mImageList[this.listView1.SelectedIndices[0]];
            this.mSelectedImagePair = imagepair;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

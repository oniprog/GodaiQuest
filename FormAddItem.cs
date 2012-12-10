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
    public partial class FormAddItem : Form
    {
        private HashSet<FilePair> mFileSet = new HashSet<FilePair>();
        private GQCommandMaster mGQCom;
        private int mImageID;
        private bool mNewItemImage;
        private AItem mItem;

        public FormAddItem(GQCommandMaster gqcom_)
        {
            InitializeComponent();

            this.mGQCom = gqcom_;
        }

        public AItem getItem()
        {
            return this.mItem;
        }

        private void FormAddItem_Load(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // アイテムの登録
            if (this.picItem.Image == null)
            {
                MessageBox.Show("アイテム画像を指定してください");
                return;
            }
            if ( this.picHeader.Image == null ) {
                this.picHeader.Image = new Bitmap(1,1);
            }

            ImagePair imagepair = new ImagePair(this.mImageID, true, this.picItem.Image, "", mGQCom.getUserID(), DateTime.Now, this.mNewItemImage);
            AItem item = new AItem(0, this.mImageID, this.txtHeader.Text, this.picHeader.Image, true);

            this.mGQCom.setAItem(ref item, imagepair, this.mFileSet);

            // 場合によってはモンスタ化する
            this.mGQCom.setMonster(item.getItemID(), this.chkProblem.Checked);

            this.mItem = item;
            this.DialogResult = DialogResult.OK;

            Close();
        }

        private void btnHeaderPic_Click(object sender, EventArgs e)
        {
            Image image = GodaiLibrary.KLib.loadAndResizeImage(128, 128);
            this.picHeader.Image = image;
        }

        private void btnItemNew_Click(object sender, EventArgs e)
        {
            Image image = GodaiLibrary.KLib.loadAndResizeImage(64, 64);
            this.picItem.Image = image;
            this.mImageID = 0;
            this.mNewItemImage = true;
        }

        private void btnItemOld_Click(object sender, EventArgs e)
        {
            Hide();
            FormSelectItemImage formSelect = new FormSelectItemImage(this.mGQCom);
            if (formSelect.ShowDialog() == DialogResult.OK)
            {
                ImagePair imagepair = formSelect.getSelectedImagePair();
                this.picItem.Image = imagepair.getImage();
                this.mImageID = imagepair.getNumber();
                this.mNewItemImage = false;
            }
            Show();
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            addFolder(dlg.SelectedPath, "");
        }

        private void addFolder(String strDirectory, String strAddPath)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(strDirectory);
            foreach (var fileinfo in dirinfo.GetFiles())
            {
                String strHalfPath = Path.Combine(strAddPath, fileinfo.Name);
                this.mFileSet.Add(new FilePair(fileinfo.FullName, strHalfPath));
//                this.listFiles.Items.Add(strHalfPath);
//                this.listFiles.SetItemChecked(this.listFiles.Items.Count - 1, true);
            }
            foreach (var dir in dirinfo.GetDirectories() ) {
                addFolder(dir.FullName, Path.Combine(strAddPath, dir.Name));
            }
        }

        private void btnCleraList_Click(object sender, EventArgs e)
        {
//            this.listFiles.Items.Clear();
            this.mFileSet.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            foreach (var filename in dlg.FileNames)
            {
                this.mFileSet.Add(new FilePair(Path.GetFullPath(filename), Path.GetFileName(filename)));
//                this.listFiles.Items.Add(Path.GetFileName(filename));
//                this.listFiles.SetItemChecked(this.listFiles.Items.Count - 1, true);
            }
        }

        private void listFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
//                    this.listFiles.Items.Add(Path.GetFileName(file));
                    this.mFileSet.Add(new FilePair(Path.GetFullPath(file), Path.GetFileName(file)));
//                    this.listFiles.SetItemChecked(this.listFiles.Items.Count - 1, true);
                }
            }
        }

        private void listFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void btnRemoveUnusedFiles_Click(object sender, EventArgs e)
        {
#if false
            for (int it = 0; it < this.listFiles.Items.Count; ++it)
            {
                bool bChecked = this.listFiles.GetItemChecked(it);
                if (!bChecked)
                {
                    var item = this.listFiles.Items[it];
                    for (int im = 0; im < this.mFileSet.Count; ++im)
                    {
                        var filepair = this.mFileSet.ElementAt(im);
                        if (filepair.HalfPath == this.listFiles.GetItemText(item))
                        {
                            this.mFileSet.Remove(filepair);
                        }
                    }
                     
                    this.listFiles.Items.RemoveAt(it--);
                }
            }
#endif
        }
    }

    public class FilePair
    {
        public String FullPath { get; set; }
        public String HalfPath { get; set; }

        public FilePair(String fullpath_, String halfpath_)
        {
            this.FullPath = fullpath_;
            this.HalfPath = halfpath_;
        }
    }
}

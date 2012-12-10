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
    public partial class FormEdit : Form
    {
        static String TMP_FOLDER = "c:\\tmp";

        public FormEdit(GQCommandMaster gqCom, UserInfo userinfo, String strUserFolder )
        {
            InitializeComponent();

            this.mGQCom = gqCom;
            this.mUserInfo = userinfo;
            this.mCx = this.mCy = 4;
            this.mUserFolder = strUserFolder;
        }

        private String mUserFolder;
        private TileInfo mTileList = new TileInfo();
        private DungeonInfo mDungeon = new DungeonInfo();
        private DungeonBlockImageInfo mDungeonBlockImage = new DungeonBlockImageInfo();
        private TilePalette mBlockImagePalette = new TilePalette(0);
        private ObjectAttrInfo mObjectAttrInfo = new ObjectAttrInfo();
        private ItemInfo mItemInfo = new ItemInfo();
        private UserInfo mUserInfo;
        private IslandGroundInfo mIslandGroundInfo;
 
        private bool mModify = false;
        private int mCx, mCy;
        private Point mSelectBegin = new Point(-1, -1);
        private Point mSelectEnd = new Point(-1, -1);
        private Point mSelectedCell = new Point(-1, -1);

        private FormShowHeader mFormShowHeader;

        private GQCommandMaster mGQCom;
        private List<Tile> mImageTileList = new List<Tile>();

        private int mDungeonUserID;

        private bool mStartup;
        private bool mMoveItem = false;

        private bool loadDungeon(int nUserID, int nDugeonNumber)
        {
            if (!this.mGQCom.getDungeon(out this.mDungeon, nUserID, nDugeonNumber) )  {
             
                this.scrEditH.Enabled = false;
                this.scrEditV.Enabled = false;
                return false;
            }

            // 記録しておく(保存時用)
            this.mDungeonUserID = nUserID;

            this.scrEditH.Enabled = true;
            this.scrEditV.Enabled = true;

            this.scrEditH.Minimum = 4; this.scrEditH.Maximum = this.mDungeon.getSizeX() + this.scrEditH.LargeChange - 1 - 6;
            this.scrEditV.Minimum = 4; this.scrEditV.Maximum = this.mDungeon.getSizeY() + this.scrEditV.LargeChange - 1 - 6;

            // 位置設定
            if (nUserID == 0)
            {
                // 自分のダンジョンの周辺に
                // 外に出た
                IslandGroundInfo groundinfo;
                if (!this.mGQCom.getIslandGroundInfo(out groundinfo))
                {
                    MessageBox.Show(this.mGQCom.getErrorReasonString());
                    return false;
                }
                var listGround = groundinfo.getIslandGroundByUserID(nUserID);

                int ix1 = 0, iy1 = 0;
                int ix2 = this.mDungeon.getSizeX() - 1, iy2 = this.mDungeon.getSizeY() - 1;
                for (int iy = iy1; iy <= iy2; ++iy)
                {
                    bool bFind = false;
                    for (int ix = ix1; ix <= ix2; ++ix)
                    {
                        bool bIn = false;
                        foreach (var ground in listGround)
                        {
                            if (ground.contains(ix, iy))
                            {
                                bIn = true;
                                break;
                            }
                        }
                        if (!bIn)
                            continue;

                        this.scrEditH.Value = ix;
                        this.scrEditV.Value = iy;
                        bFind = true;
                        break;
                    }
                    if (bFind)
                        break;
                }
            }
            else
            {
                if (this.scrEditH.Value > this.scrEditH.Maximum-this.scrEditH.LargeChange-1-6) this.scrEditH.Value = 4;
                if (this.scrEditV.Value > this.scrEditV.Maximum-this.scrEditV.LargeChange-1-6) this.scrEditV.Value = 4;
            }

            this.mCx = this.scrEditH.Value;
            this.mCy = this.scrEditV.Value;
            drawDungeon();
            this.picScreen.Refresh();

            return true;
        }

        private bool loadDungeonImages()
        {
            return this.mGQCom.getDungeonBlockImage(out this.mDungeonBlockImage);
        }

        private bool loadBlockImagePalette()
        {
            return this.mGQCom.getBlockImagePallette(out this.mBlockImagePalette, this.mGQCom.getUserID());
        }

        private bool loadObjectAttrInfo()
        {
            return this.mGQCom.getObjectAttrInfo(out this.mObjectAttrInfo);
        }

        private bool loadTileList()
        {
            return this.mGQCom.getTitleList(out this.mTileList);
        }

        internal bool loadItemInfo()
        {
            return this.mGQCom.getItemInfo(out this.mItemInfo);
        }

        private bool loadIslandGroundInfo()
        {
            return this.mGQCom.getIslandGroundInfo(out this.mIslandGroundInfo);
        }

        public static bool isSelected(Point selBegin, Point selEnd, int ix, int iy)
        {
            return
                Math.Min(selBegin.X, selEnd.X) <= ix && ix <= Math.Max(selBegin.X, selEnd.X) &&
                Math.Min(selBegin.Y, selEnd.Y) <= iy && iy <= Math.Max(selBegin.Y, selEnd.Y);
        }

        public void drawDungeon(Graphics g, DungeonInfo dungeon, DungeonBlockImageInfo images, int cx, int cy, Point selBegin, Point selEnd, Point selectedCell )
        {
            Rectangle rect = new Rectangle(0, 0, 640, 640);
            g.FillRectangle(Brushes.DarkGray, rect);

            var listGround = this.mIslandGroundInfo.getIslandGroundByUserID(this.mGQCom.getUserID());

            for (int iy = cy - 5; iy <= cy + 5; ++iy)
            {
                if (iy < 0 || iy >= dungeon.getSizeY()) continue;

                for (int ix = cx - 5; ix <= cx + 5; ++ix)
                {
                    if (ix < 0 || ix >= dungeon.getSizeX()) continue;

                    bool bSelected = isSelected(selBegin, selEnd, ix, iy);

                    uint nImage = dungeon.getDungeonImageAt(ix, iy);
                    var image = images.getImageAt(nImage);

                    int idrawx = (ix-cx+4) * 64 + rect.Left;
                    int idrawy = (iy-cy+4) * 64 + rect.Top;

                    if (image != null)
                    {
                        g.DrawImage(image, idrawx, idrawy);
                    }

                    if (bSelected)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(64, 0x20, 0x20, 0x80)), idrawx, idrawy, 64, 64);
                    }

                    if (ix == selectedCell.X && iy == selectedCell.Y)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(64, 0x80, 0x20, 0x20)), idrawx, idrawy, 64, 64);
                    }

                    if (this.mDungeonUserID == 0) {
                        bool bIn = false;
                        foreach (var ground in listGround)
                        {
                            if (ground.contains(ix, iy))
                            {
                                bIn = true;
                                break;
                            }
                        }
                        if( !bIn )
                            g.FillRectangle(new SolidBrush(Color.FromArgb(128, 0x0, 020, 0x0)), idrawx, idrawy, 64, 64);
                    }
                }
            }
        }

        //
        private void paintMapData(Point selBegin, Point selEnd, int nSelect)
        {
            Tile tile = this.mImageTileList[nSelect];

            int ix1 = Math.Min(selBegin.X, selEnd.X);
            int ix2 = Math.Max(selBegin.X, selEnd.X);
            int iy1 = Math.Min(selBegin.Y, selEnd.Y);
            int iy2 = Math.Max(selBegin.Y, selEnd.Y);

            var listGround = this.mIslandGroundInfo.getIslandGroundByUserID(this.mGQCom.getUserID());

            for (int iy = iy1; iy <= iy2; ++iy)
            {
                for (int ix = ix1; ix <= ix2; ++ix)
                {
                    if (this.mDungeonUserID == 0)
                    {
                        bool bIn = false;
                        foreach (var ground in listGround)
                        {
                            if (ground.contains(ix, iy))
                            {
                                bIn = true;
                                break;
                            }
                        }
                        if (!bIn)
                            continue;
                    }

                    // アイテムが有るかの判定
                    var nObjectID = this.mDungeon.getDungeonContentAt(ix, iy);
                    var obj = this.mObjectAttrInfo.getObject((int)nObjectID);
                    if (obj.getItemID() >0 )
                        continue;

                    this.mDungeon.setDungeonTileAt(ix,iy,tile.getTileID());
                }
            }
        }

        // 選択用イメージリストを作成する
        private void makeBlockImageList()
        {
            this.imageList1.Images.Clear();
            this.mImageTileList.Clear();
            this.listView1.Items.Clear();

            int it=0;
            foreach (var tile in this.mTileList)
            {
                ulong nTileID = tile.getTileID();
                if (!this.mBlockImagePalette.containsTileID(nTileID) )
                    continue;

                this.mImageTileList.Add(tile);

                uint nImageID = tile.getImageID();

                var imagepair = this.mDungeonBlockImage.getImagePairAt(nImageID);
                this.imageList1.Images.Add(imagepair.getImage());

                ListViewItem item = new ListViewItem();
                item.ImageIndex = it++;
                item.Text = imagepair.getName();
                this.listView1.Items.Add(item);
            }
        }

        private void makeDungeonList()
        {
            this.cmbEditArea.Items.Clear();
            this.cmbEditArea.Items.Add("大陸");

            int nDungeonDepth = this.mGQCom.getDungeonDepth(this.mGQCom.getUserID());
            for (int idd = 1; idd <= nDungeonDepth; ++idd)
            {
                this.cmbEditArea.Items.Add("ダンジョン" + ("" + idd));
            }
            this.cmbEditArea.SelectedIndex = 1;
        }

        private void FormEdit_Load(object sender, EventArgs e)
        {
            this.mStartup = true;
            this.makeDungeonList();

            this.picScreen.Image = new Bitmap(640, 640);

            try {
                loadIslandGroundInfo(); // 一度きり
                loadDungeon(this.mGQCom.getUserID(), 0);
                loadItemInfo();
                loadTileList();
                loadObjectAttrInfo();
                loadBlockImagePalette();
                loadDungeonImages();
                drawDungeon();
                makeBlockImageList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.mStartup = false;
        }

        private void drawDungeon()
        {
            Graphics g = Graphics.FromImage(this.picScreen.Image);
            drawDungeon(g, this.mDungeon, this.mDungeonBlockImage, this.mCx, this.mCy, this.mSelectBegin, this.mSelectEnd, this.mSelectedCell);
            this.picScreen.Refresh();
            g.Dispose();
        }

        private bool checkClose()
        {

            if (this.mModify)
            {
                var result = MessageBox.Show("保存してよろしいですか？", "notice", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    this.saveDungeon();
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        private void FormEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.checkClose())
                e.Cancel = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (this.checkClose()) 
                Close();
        }

        /// 作ったダンジョンを保存する
        private void btnSave_Click(object sender, EventArgs e)
        {
            //
            saveDungeon();
            MessageBox.Show("ダンジョンを保存しました");
        }

        private void saveDungeon() {

            this.mGQCom.setDungeon(this.mDungeonUserID, this.mDungeon, this.mDungeonBlockImage, this.mObjectAttrInfo, this.mTileList);
            this.mModify = false;

            // 再読込してダンジョンをリセットする
            // 新規作成フラグをリセットするために
            loadTileList();
            loadDungeon(this.mDungeonUserID, this.mDungeon.getDungeonNumber());
            loadDungeonImages();
            loadObjectAttrInfo();

        }

        /// ブロックのイメージを追加する
        private void btnAddBlockImage_Click(object sender, EventArgs e)
        {
            Hide();
            FormAddBlockImage formAddBlockImage = new FormAddBlockImage(this.mDungeonBlockImage, this.mObjectAttrInfo, this.mTileList, this.mGQCom.getUserID() );
            if (formAddBlockImage.ShowDialog() == DialogResult.OK)
                this.mModify = true;
            Show();
            drawDungeon();
            makeBlockImageList();
        }

        private void scrEditV_Scroll(object sender, ScrollEventArgs e)
        {
            this.mCy = this.scrEditV.Value;
            drawDungeon();
        }

        private void scrEditH_Scroll(object sender, ScrollEventArgs e)
        {
            this.mCx = this.scrEditH.Value;
            drawDungeon();
        }

        private Point getMapXY(Point cp)
        {
            int nSize = Math.Min(this.picScreen.Width, this.picScreen.Height);
            int ix = (cp.X-(this.picScreen.Width-nSize)/2) / (nSize/10) + this.mCx - 4;
            int iy = (cp.Y - (this.picScreen.Height- nSize)/2) / (nSize/10) + this.mCy - 4;
            return new Point(ix, iy);
        }

        private bool mPaintClicked = false;

        private void picScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.mPaintClicked)
            {
                this.mSelectEnd = this.getMapXY(e.Location);
                this.drawDungeon();
                this.picScreen.Refresh();
            }
        }

        private void picScreen_MouseDown(object sender, MouseEventArgs e)
        {
            if ( this.mFormShowHeader != null ) {
                mFormShowHeader.Dispose();
                mFormShowHeader = null;
            }

            this.mSelectedCell = this.getMapXY(e.Location);

            if (e.Button == MouseButtons.Left && this.chkLock.Checked == false)
            {
                var objid = this.mDungeon.getDungeonContentAt(this.mSelectedCell.X, this.mSelectedCell.Y);
                var obj = this.mObjectAttrInfo.getObject((int)objid);
                if (obj.getItemID() > 0)
                {
                    if (this.mDungeonUserID != 0)
                    {
                        this.mMoveItem = true;
                        this.Cursor = System.Windows.Forms.Cursors.Hand;
                    }
                }
                else
                {
                    this.mPaintClicked = true;
                    this.mSelectEnd = this.mSelectBegin = this.getMapXY(e.Location);
                    this.picScreen.Capture = true;
                }
            }
            this.drawDungeon();
            this.picScreen.Refresh();
        }

        private Point forceWithinDungeon( Point poi ) {
            return new Point(
               poi.X < 0 ? 0 : poi.X >= this.mDungeon.getSizeX() ? this.mDungeon.getSizeX()-1 : poi.X, 
               poi.Y < 0 ? 0 : poi.Y >= this.mDungeon.getSizeY() ? this.mDungeon.getSizeY()-1 : poi.Y
               );
        }

        private void picScreen_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.mPaintClicked)
            {
                this.mSelectEnd = forceWithinDungeon( this.getMapXY(e.Location));
                this.picScreen.Capture = false;

                if (this.listView1.SelectedIndices.Count > 0)
                {
                    paintMapData(this.mSelectBegin, this.mSelectEnd, this.listView1.SelectedIndices[0]);
                    this.mModify = true;
                }

                this.mSelectBegin = this.mSelectEnd = new Point(-1, -1);
                this.drawDungeon();
                this.picScreen.Refresh();
                this.mPaintClicked = false;
            }
            else if (this.mMoveItem)
            {
                // アイテムの移動
                var mototile = this.mDungeon.getDungeonTileAt(this.mSelectedCell.X, this.mSelectedCell.Y);
                var cur = this.getMapXY(e.Location);
                if (cur.X >= 0 && cur.X < this.mDungeon.getSizeX() && cur.Y >= 0 && cur.Y < this.mDungeon.getSizeY()) { 
                    var targettile = this.mDungeon.getDungeonTileAt(cur.X, cur.Y);

                    this.mDungeon.setDungeonTileAt(cur.X, cur.Y, mototile);
                    this.mDungeon.setDungeonTileAt(this.mSelectedCell.X, this.mSelectedCell.Y, targettile);
                    this.drawDungeon();
                    this.picScreen.Refresh();
                    this.mMoveItem = false;
                    this.mModify = true;
                }
                this.Cursor = null;
            }
        }

        private void picScreen_DoubleClick(object sender, EventArgs e)
        {
        }

        private void picScreen_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            var xy = this.getMapXY(e.Location);
            var objid = this.mDungeon.getDungeonContentAt(this.mSelectedCell.X, this.mSelectedCell.Y);
            var obj = this.mObjectAttrInfo.getObject((int)objid);
            if (obj.getItemID() > 0)
            {
                // アイテム取得
                AItem aitem = this.mItemInfo.getAItem( obj.getItemID() );
                this.mFormShowHeader = new FormShowHeader(this, aitem, this.mGQCom, new ALocation(this.mGQCom.getUserID(), this.mSelectedCell.X, this.mSelectedCell.Y, this.mDungeonUserID, this.mDungeon.getDungeonNumber()), this.mDungeon, this.mDungeonUserID, this.mUserFolder);
                this.mFormShowHeader.Show();
            }
        }

        /// 使用ブロックリストの編集
        private void btnEditBlockList_Click(object sender, EventArgs e)
        {
            if (this.mModify)
            {
                if (MessageBox.Show("保存する必要があります。保存してよろしいですか？", "notice", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
                this.saveDungeon();
            }

            FormBlockImageList form = new FormBlockImageList(this.mGQCom, this.mDungeonBlockImage, this.mTileList, this.mBlockImagePalette, this.mUserInfo, this.mObjectAttrInfo);
            if (form.ShowDialog() == DialogResult.OK)
            {
                this.mBlockImagePalette = form.getImagePalette();
                makeBlockImageList();
                this.mGQCom.setBlockImagePalette(this.mBlockImagePalette, this.mGQCom.getUserID());
            }

            // 修正した場合、読み込みが必要
            this.loadDungeonImages();
            this.loadObjectAttrInfo();
        }

        private void cmbEditArea_SelectedValueChanged(object sender, EventArgs e)
        {
        }

        private void cmbEditArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.mStartup)
                return;

            if (this.mModify)
            {
                if (MessageBox.Show("保存する必要があります。保存してよろしいですか？", "notice", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
                saveDungeon();
            }

            if (this.cmbEditArea.SelectedIndex == 0)
            {
                // 大陸
                loadDungeon(0, 0);
            }
            else
            {
                // 個々のダンジョン
                loadDungeon(this.mGQCom.getUserID(), this.cmbEditArea.SelectedIndex-1);
            }
            drawDungeon();
            this.picScreen.Refresh();
        }

        // アイテム配置位置を得る
        private bool getPlaceItemPoint(out Point pointItem)
        {
            pointItem = new Point(-1,0);
            int nDist = int.MaxValue;
            for (int iy = 0; iy < this.mDungeon.getSizeY(); ++iy)
            {
                for( int ix=0; ix<this.mDungeon.getSizeX(); ++ix ) {
                    var objectid = this.mDungeon.getDungeonContentAt(ix,iy);
                    var obj = this.mObjectAttrInfo.getObject((int)objectid);
                    if (obj != null ) {
                        if ( obj.getItemID() > 0 )
                            continue;
                    }
                    int nDistTmp = Math.Abs(ix - this.mSelectedCell.X) + Math.Abs(iy - this.mSelectedCell.Y);
                    if (nDistTmp < nDist)
                    {
                        nDist = nDistTmp;
                        pointItem = new Point(ix, iy);
                    }
                }
            }
            return pointItem.X != -1;
        }

        // 技術情報を追加する
        private void btnAddTechInfo_Click(object sender, EventArgs e)
        {
            if (this.mDungeonUserID == 0)
            {
                MessageBox.Show("大陸には技術情報を配置できません");
                return;
            }

            Point pointItem;
            if (!this.getPlaceItemPoint(out pointItem))
            {
                MessageBox.Show("アイテムを配置するスペースがありません");
                return;
            }

            if (this.mModify)
            {
                if (MessageBox.Show("保存する必要があります。保存してよろしいですか？", "notice", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
                saveDungeon();
            }

            Hide();
            FormAddItem formAddItem = new FormAddItem(this.mGQCom);
            if (formAddItem.ShowDialog() == DialogResult.OK)
            {
                AItem item = formAddItem.getItem();

                ObjectAttr obj = new ObjectAttr(this.mObjectAttrInfo.newObjectID(), true, item.getItemID(), EObjectCommand.Nothing, 0, true);
                this.mObjectAttrInfo.addObject(obj);
                this.saveDungeon(); // 保存する必要がある 新規オブジェクトID割り振り

                // 上記で保存したオブジェクトIDを得る
                this.mGQCom.getObjectAttrInfo(out this.mObjectAttrInfo);

                // マッチするオブジェクトIDの検索
                int nObjectID = this.mObjectAttrInfo.findObjectByItemID(obj.getItemID());

                // ダンジョン書き換え
                this.mDungeon.setDungeonImageAt(pointItem.X, pointItem.Y, (uint)item.getItemImageID());
                this.mDungeon.setDungeonContentAt(pointItem.X, pointItem.Y, (uint)nObjectID);
                this.saveDungeon();

                this.mGQCom.getDungeonBlockImage(out this.mDungeonBlockImage);

                this.drawDungeon();
                this.picScreen.Refresh();

                loadItemInfo();
            }
            Show();
        }

        private void btnEnlargeDungeon_Click(object sender, EventArgs e)
        {
            if (this.mDungeonUserID == 0)
            {
                MessageBox.Show("大陸では何も出来ません");
                return;
            }

            if (this.mModify)
            {
                if (MessageBox.Show("保存する必要があります。保存してよろしいですか？", "notice", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
                this.saveDungeon();
            }

            Hide();
            FormUseExperience form = new FormUseExperience(this.mGQCom, this.mDungeon.getDungeonNumber());
            form.ShowDialog();
            Show();

            this.loadDungeon(this.mGQCom.getUserID(), this.mDungeon.getDungeonNumber());
            makeDungeonList();
        }


    }
}

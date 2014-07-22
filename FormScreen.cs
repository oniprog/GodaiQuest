using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Xml.XPath;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuest
{
    public partial class FormScreen : Form
    {
        public FormScreen()
        {
            InitializeComponent();
        }

        private String mUserFolder;
        private UserInfo mUserInfo;
        private GQCommandMaster mGQCom;

        private LocationInfo mLocationInfo = new LocationInfo();
        private ALocation mLocation;

        private Dictionary<int, int> mLoginUserList = new Dictionary<int, int>();

        private DungeonInfo mDungeon;
        private DungeonBlockImageInfo mDungeonBlockImage;
        private ObjectAttrInfo mObjectAttrInfo;
        private ItemInfo mItemInfo;
        private MessageInfo mMessageInfo;
        private UnpickedupInfo mUnpickedupInfo;

        private MonsterInfo mMonsterInfo;
        private Dictionary<int, MonsterLocation> mMonsterLocation = new Dictionary<int, MonsterLocation>();

        private Point mSelectedCell;

        private List<int> mWalkPath = new List<int>();

        private RDReadItemInfo mRDReadItemInfo; // ランダムダンジョンの捕まえ済み情報

        private RealMonsterInfo _realMonsterSrcInfo;	// 外部モンスターの元情報
        private RealMonsterLocationInfo _realMonsterLocationInfo;	// 外部モンスターの位置

        private Random rand = new Random();

        private List<BombAnim> _listBombAnim = new List<BombAnim>();

        private bool _bEnableCopyRichTextBox = true;	// コピー＆ペーストの禁止（モンスターの呪文のコピーを防ぐため）

        private void FormScreen_Load(object sender, EventArgs e)
        {
			// 爆破アニメーションの読み込み
			BombAnim.InitBombImage();
            
            // ログイン処理
            Hide();
            try
            {
                FormLogOn formlogon = new FormLogOn();
                if (formlogon.ShowDialog() != DialogResult.OK)
                {
                    Application.Exit();
                    return;
                }

                // 接続済みオブジェクトを得る
                Show();
                this.mGQCom = formlogon.getConnect();
                this.loadUserInfo();

                // フォルダを得る
                while (true)
                {
                    this.mUserFolder = this.mGQCom.getUserFolder(this.mGQCom.getUserID());
                    if (this.mUserFolder.Length > 0)
                        break;
                    MessageBox.Show("情報のダウンロード先フォルダを指定してください");
                    Hide();
                    FormCreateAccount form = new FormCreateAccount(false);
                    form.setInitForChangeUserInfo(this.mUserInfo.getAUser(this.mGQCom.getUserID()), this.mGQCom);
                    form.ShowDialog();
                    Show();
                }
                // 現在位置をサーバーから得る
                this.mGQCom.getLocationInfo(out this.mLocationInfo);
                this.mLocation = this.mLocationInfo.getLocationByUserID(this.mGQCom.getUserID());
                if (this.mLocation == null)
                    this.mLocation = new ALocation(this.mGQCom.getUserID());

                // メッセージを読む
                this.loadMessageInfo();

                // 経験値を読む
                this.loadExpValue();

                // ピックアップしてない情報を読む
                this.loadUnpickedupInfo();

				// 外部モンスター一覧表を読む
                loadRealMonsterSrcInfo();

                this.picScreen.Image = new Bitmap(BLOCK_SIZE * HALF_WIDTH*2, BLOCK_SIZE * HALF_WIDTH*2);

                loadDungeon(this.mLocation, true);

                // タイマー２始動
                timer2.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
                return;
            }
        }

        // ダンジョン読み込み
        private bool loadDungeon(ALocation location, bool refresh)
        {
            if (!this.mGQCom.getDungeon(out this.mDungeon, location.getDungeonUserID(), location.getDungeonNumber() ) )
            {
                MessageBox.Show("Error : " + this.mGQCom.getErrorReasonString());
                Application.Exit();

            }

            this.loadObjectAttrInfo();
            this.loadItemInfo();
            this.loadDugeonImages();

            // モンスタの一覧を読む
            this.loadMonsterInfo();
            this.initMonsterPosition();
            // 特殊ダンジョンのアイテム読み済みリスト
            this.loadRDReadItemInfo();

            if (refresh)
            { 
                this.drawScreen();
                this.picScreen.Refresh();
            }
            
            return true;
        }
            
        private bool loadDugeonImages()
        {
            return this.mGQCom.getDungeonBlockImage(out this.mDungeonBlockImage);
        }

        private bool loadObjectAttrInfo()
        {
            return this.mGQCom.getObjectAttrInfo(out this.mObjectAttrInfo);
        }

        private bool loadMessageInfo()
        {
            return this.mGQCom.getMessageInfo(out this.mMessageInfo);
        }

        private bool loadItemInfo()
        {
            return this.mGQCom.getItemInfo(out this.mItemInfo);
        }

        private void loadExpValue()
        {
            var expvalue = this.mGQCom.getExpValue();
            this.labExp.Text = "経験値: " + ("" + expvalue.getExpValue());
            this.labExpTotal.Text = "全経験: " + ("" + expvalue.getTotalValue());
        }

        private bool loadUnpickedupInfo()
        {
            return this.mGQCom.getUnpickedupItemInfo(out this.mUnpickedupInfo, (int)this.mLocation.getDungeonUserID());
        }

        private bool loadUserInfo()
        {
            return this.mGQCom.getUserInfo(out this.mUserInfo);
        }

        private bool loadMonsterInfo()
        {
            return this.mGQCom.getMonsterInfo(out this.mMonsterInfo, this.mLocation.getDungeonUserID());
        }

        // 特殊ダンジョンのアイテム読み済みリスト
        private bool loadRDReadItemInfo()
        {
            if (this.inRandomDungeon())
            {
                return this.mGQCom.getRDReadItemInfo(out this.mRDReadItemInfo);
            }
            else
            {
                return true;
            }
        }

		// 外部モンスターの元情報の取得
        private bool loadRealMonsterSrcInfo()
        {
            return this.mGQCom.getRealMonsterSrcInfo(out _realMonsterSrcInfo);
        }

        // 特殊ダンジョンにいるかの判定
        private bool inRandomDungeon()
        {
            return this.mLocation.getDungeonUserID() == 0x40000000; 
        }

		// 大陸にいるかの判定
        private bool inIsland()
        {
            return this.mLocation.getDungeonUserID() == 0;
        }

        // モンスタ位置の初期化
        private void initMonsterPosition()
        {
            Dictionary<int, MonsterLocation> tmp = new Dictionary<int, MonsterLocation>(); ;
            this.mMonsterLocation.Clear();

            for (int iy = 0; iy < this.mDungeon.getSizeY(); ++iy)
            {
                for (int ix = 0; ix < this.mDungeon.getSizeX(); ++ix)
                {
                    int nObjectID = (int)this.mDungeon.getDungeonContentAt(ix, iy);
                    var obj = this.mObjectAttrInfo.getObject(nObjectID);
                    if (obj != null && obj.getItemID() > 0 && this.mMonsterInfo.isMonster(obj.getItemID()))
                    {
                        if (!tmp.ContainsKey(obj.getObjectID()))
                        {
                            // モンスタ
                            MonsterLocation monsterloc = new MonsterLocation();
                            monsterloc.Location = new Point(ix, iy);
                            tmp.Add((int)obj.getObjectID(), monsterloc);
                        }
                        else
                        {
                            // そのまま移す
                            tmp.Add(obj.getObjectID(), tmp[obj.getObjectID()]);
                        }
                    }
                }
            }
            this.mMonsterLocation = tmp;
        }

        // モンスタを動かす
        private void moveMonster()
        {
            Dictionary<int, MonsterLocation> dicNew = new Dictionary<int, MonsterLocation>();
            foreach (var monster in this.mMonsterLocation)
            {
                dicNew.Add(monster.Key, monster.Value);

                var posinfo = monster.Value;
                int iix = 0, iiy = 0;
                int nDir = this.rand.Next() % 10;
                switch (nDir)
                {
                    case 0: iix = -1; break;
                    case 1: iix = 1; break;
                    case 2: iiy = -1; break;
                    case 3: iiy = 1; break;
                    default:
                        if (nDir < 6)
                        {
                            iix = posinfo.MoveDir.X;
                            iiy = posinfo.MoveDir.Y;
                        }
                        else
                        {
                            iix = iiy = 0;
                        }
                        break;
                }
                int ix = posinfo.Location.X + iix;
                int iy = posinfo.Location.Y + iiy;
                if ( ix < 0 || ix >= this.mDungeon.getSizeX() )
                    continue;
                if ( iy < 0 || iy >= this.mDungeon.getSizeY() )
                    continue;

                int nObjectID = (int)this.mDungeon.getDungeonContentAt(ix, iy);
                var obj = this.mObjectAttrInfo.getObject((int)nObjectID);
                if (!obj.canWalk())
                    continue;

                posinfo.Location = new Point(ix, iy);
                posinfo.MoveDir = new Point(iix, iiy);
                dicNew[monster.Key] = posinfo;
            }
            this.mMonsterLocation = dicNew;
        }

        private static int HALF_WIDTH = 5;
        private static int BLOCK_SIZE = 64;

        private void drawScreen()
        {
            int cx = this.mLocation.getIX();
            int cy = this.mLocation.getIY();

            // ユーザ一覧を得る
            Dictionary<int, String> dicMessage = new Dictionary<int, string>();
            List<ALocation> listLocation = new List<ALocation>();
            foreach (var loc in this.mLocationInfo)
            {
                if (loc.getUserID() == this.mLocation.getUserID())
                {
                    var mes = this.mMessageInfo.getAMessage(this.mLocation.getUserID());
                    if (mes == null)
                        mes = new AMessage(this.mLocation.getUserID(), "");
                    dicMessage.Add(mes.getUserID(), mes.getMessage());
                    
                    continue; // 自分は表示しない
                }
                   
                if (this.mLoginUserList.ContainsKey(loc.getUserID()))
                {
                    if (loc.getDungeonUserID() == this.mLocation.getDungeonUserID() &&
                        loc.getDungeonNumber() == this.mLocation.getDungeonNumber()
                    )
                    {
                        // 位置を記録
                        listLocation.Add(loc);

                        // メッセージを記録
                        var mes = this.mMessageInfo.getAMessage(loc.getUserID());
                        if (mes == null)
                            mes = new AMessage(loc.getUserID(), "");
                        dicMessage.Add(mes.getUserID(), mes.getMessage());
                    }
                }
            }

            //
            using ( Font font = new Font("MS-MINCHO", 8))
            using( Graphics gra = Graphics.FromImage(this.picScreen.Image) ) {

                Rectangle rect = new Rectangle(0, 0, 640, 640);
                if ( this.mLocation.getDungeonUserID() == 0 )
                    gra.FillRectangle(Brushes.DarkBlue, rect);
                else
                    gra.FillRectangle(Brushes.DarkGray, rect);

                for (int iy = cy - HALF_WIDTH; iy <= cy + HALF_WIDTH; ++iy)
                {
                    if (iy < 0 || iy >= this.mDungeon.getSizeY()) continue;

                    for (int ix = cx - HALF_WIDTH; ix <= cx + HALF_WIDTH; ++ix)
                    {
                        if (ix < 0 || ix >= this.mDungeon.getSizeX()) continue;

                        uint nImage = this.mDungeon.getDungeonImageAt(ix, iy);

                        var nObjectID = this.mDungeon.getDungeonContentAt(ix, iy);
                        var obj = this.mObjectAttrInfo.getObject((int)nObjectID);
                        if (obj != null && this.mMonsterInfo.isMonster(obj.getItemID()))
                        {
                            // モンスタの元位置の描画
                            nImage = 1; // 2番めのイメージ
                            obj = null; // いったん無効にする
                        }
                        else if (this.inRandomDungeon() && obj != null && obj.getItemID() > 0 && this.mRDReadItemInfo.isReadItem(obj.getItemID()))
                        {
                            // 特殊ダンジョン内で退治済みアイテムの描画を変える
                            if (this.mMonsterInfo.isMonster(obj.getItemID()))
                            {
                                nImage = 1; // 壁が望ましい
                            }
                            else
                            {
                                nImage = 0; // 1番目のイメージ
                            }
                            obj = null; // 無効化する
                        }

                        var image = this.mDungeonBlockImage.getImageAt(nImage);

                        int idrawx = (ix - cx + HALF_WIDTH) * BLOCK_SIZE + rect.Left;
                        int idrawy = (iy - cy + HALF_WIDTH) * BLOCK_SIZE + rect.Top;

                        // 地形イメージを表示
                        if (image != null)
                        {
                            gra.DrawImage(image, idrawx, idrawy, BLOCK_SIZE, BLOCK_SIZE);
                        }

                        // ユーザのイメージを表示
                        if (ix == this.mLocation.getIX() && iy == this.mLocation.getIY())
                        {
                            var user = this.mUserInfo.getAUser(this.mGQCom.getUserID());
                            gra.DrawImage(user.getCharacterImage(), idrawx, idrawy, BLOCK_SIZE, BLOCK_SIZE);
                        }

                        foreach (var loc in listLocation)
                        {
                            if (loc.getIX() == ix && loc.getIY() == iy)
                            {
                                // ユーザのイメージを表示
                                var user = this.mUserInfo.getAUser(loc.getUserID());
                                Image imageOther = user.getCharacterImage();
                                gra.DrawImage(imageOther, idrawx, idrawy, BLOCK_SIZE, BLOCK_SIZE);

                            }
                        }

                        // モンスター位置のチェック
                        foreach (var posinfo in this.mMonsterLocation)
                        {
                            var pos = posinfo.Value.Location;
                            if (pos.X == ix && pos.Y == iy)
                            {
                                // モンスタの描画
                                var nMonsterObjectID = posinfo.Key;
                                obj = this.mObjectAttrInfo.getObject(nMonsterObjectID);
                                var item = this.mItemInfo.getAItem(obj.getItemID());

                                // 特殊ダンジョンで読み込み済みアイテムは無視する
                                if ( inRandomDungeon() && this.mRDReadItemInfo.isReadItem(item.getItemID()) )
                                    continue;

                                var imageMonster = this.mDungeonBlockImage.getImageAt((uint)item.getItemImageID());
                                gra.DrawImage(imageMonster, idrawx, idrawy, BLOCK_SIZE, BLOCK_SIZE);
                            }
                        }

						// 外部モンスターの位置チェック
                        if (_realMonsterLocationInfo != null)
                        {
                            foreach (var posinfo in _realMonsterLocationInfo)
                            {
                                if (ix == posinfo.MonsterIx && iy == posinfo.MonsterIy && inIsland())
                                {
                                    // 外部モンスターの描画
                                    var nSrcMonsterId = posinfo.MonsterSrcId;
                                    var imageRealMonster = _realMonsterSrcInfo[nSrcMonsterId].MonsterImage;
                                    gra.DrawImage(imageRealMonster, idrawx, idrawy, BLOCK_SIZE, BLOCK_SIZE);
                                }
                            }
                        }

                        // 選択色
                        if (ix == this.mSelectedCell.X && iy == this.mSelectedCell.Y)
                            gra.FillRectangle(new SolidBrush(Color.FromArgb(64, 0x20, 0x20, 0x80)), idrawx, idrawy, BLOCK_SIZE, BLOCK_SIZE);


                        // 未読の枠を書く
                        if (obj != null && obj.getItemID() > 0)
                        {
                            if (this.inRandomDungeon())
                            {
                                if (!this.mRDReadItemInfo.isReadItem(obj.getItemID()))
                                {
                                    gra.DrawRectangle(Pens.Red, idrawx, idrawy, BLOCK_SIZE-1, BLOCK_SIZE-1);
                                    gra.DrawRectangle(Pens.Red, idrawx + 2, idrawy + 2, BLOCK_SIZE-3, BLOCK_SIZE-3);
                                }
                            }
                            else
                            {
                                if (this.mUnpickedupInfo.containItemID(obj.getItemID()))
                                {
                                    gra.DrawRectangle(Pens.Red, idrawx, idrawy, BLOCK_SIZE-1, BLOCK_SIZE-1);
                                    gra.DrawRectangle(Pens.Red, idrawx + 2, idrawy + 2, BLOCK_SIZE-3, BLOCK_SIZE-3);
                                }
                            }
                        }

						// モンスターの爆発アニメーション
                        lock (_listBombAnim)
                        {
                            var listDestroy = new List<BombAnim>();
                            foreach (var bombanim in _listBombAnim)
                            {
                                if (bombanim.Ix == ix && bombanim.Iy == iy)
                                {
                                    Image img = bombanim.GetBombImage();

                                    bombanim.IncTurn();
                                    if (bombanim.Expired())
                                    {
                                        listDestroy.Add(bombanim);
                                    }

                                    if (img != null)
                                    {
                                        gra.DrawImage(img, idrawx, idrawy, BLOCK_SIZE, BLOCK_SIZE);
                                    }
                                }
                            }
							// 削除
                            foreach (var bombanim in listDestroy)
                            {
                                _listBombAnim.Remove(bombanim);
                            }
                        }
                    }
                }

                for (int iy = cy - HALF_WIDTH; iy <= cy + HALF_WIDTH; ++iy)
                {
                    if (iy < 0 || iy >= this.mDungeon.getSizeY()) continue;

                    for (int ix = cx - HALF_WIDTH; ix <= cx + HALF_WIDTH; ++ix)
                    {
                        if (ix < 0 || ix >= this.mDungeon.getSizeX()) continue;

                        int idrawx = (ix - cx + HALF_WIDTH) * BLOCK_SIZE + rect.Left;
                        int idrawy = (iy - cy + HALF_WIDTH) * BLOCK_SIZE + rect.Top;

                        if (ix == this.mLocation.getIX() && iy == this.mLocation.getIY())
                        {
                            var user = this.mUserInfo.getAUser(this.mGQCom.getUserID());
                            gra.DrawImage(user.getCharacterImage(), idrawx, idrawy, BLOCK_SIZE, BLOCK_SIZE);

                            // メッセージを表示
                            if (dicMessage.ContainsKey(user.getUserID()))
                            {
                                var mes = dicMessage[user.getUserID()];
                                String[] listMes = mes.Split('\n');
                                int idy = idrawy;
                                foreach (var mesline in listMes)
                                {
                                    gra.DrawString(mesline, font, Brushes.Black, idrawx + BLOCK_SIZE+4, idy);
                                    idy += (int)gra.MeasureString(mesline, font).Height;
                                }
                            }
                        }

                        foreach (var loc in listLocation)
                        {
                            if (loc.getIX() == ix && loc.getIY() == iy)
                            {
                                // メッセージを表示
                                if (dicMessage.ContainsKey(loc.getUserID()))
                                {
                                    var mes = dicMessage[loc.getUserID()];
                                    String[] listMes = mes.Split('\n');
                                    int idy = idrawy;
                                    foreach (var mesline in listMes)
                                    {
                                        gra.DrawString(mesline, font, Brushes.Black, idrawx + BLOCK_SIZE+4, idy);
                                        idy += (int)gra.MeasureString(mesline, font).Height;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            Hide();

            FormEdit formEdit = new FormEdit(this.mGQCom, this.mUserInfo, this.mUserFolder);
            formEdit.ShowDialog();

            Show();
            this.loadDungeon(this.mLocation, true);
        }

        // 終了をクリックする
        private void btnEnd_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// クリックされたダンジョン座標を得る
        private Point getDungeonCoordInPicture(int x, int y)
        {
            int nMinSize = Math.Min(this.picScreen.Width, this.picScreen.Height);
            int nSize = nMinSize / (HALF_WIDTH*2);
            int newix = ((x-(this.picScreen.Width-nMinSize)/2) / nSize) + this.mLocation.getIX() - HALF_WIDTH;
            int newiy = ((y-(this.picScreen.Height-nMinSize)/2) / nSize) + this.mLocation.getIY() - HALF_WIDTH;
            return new Point(newix, newiy);
        }

        // マウス一回クリック
        private void picScreen_MouseClick(object sender, MouseEventArgs e)
        {
            Point poiClick = getDungeonCoordInPicture(e.X, e.Y);
            if (poiClick.X < 0 || poiClick.X >= this.mDungeon.getSizeX() || poiClick.Y < 0 || poiClick.Y >= this.mDungeon.getSizeY())
                return;

			/// 外部モンスターの参照をまずチェック
            bool bDone = false;
			foreach (var posinfo in _realMonsterLocationInfo)
			{
				if( poiClick.X == posinfo.MonsterIx && poiClick.Y == posinfo.MonsterIy && inIsland() )
				{
					// 外部モンスターをピックアップした
				    var srcAmon = _realMonsterSrcInfo[posinfo.MonsterSrcId];
				    picHeader.Image = srcAmon.MonsterImage;
				    richTextBox1.Text = srcAmon.Name + "\r\n=========\r\n" + srcAmon.Spell + "\r\n=========\r\nが苦手";
				    bDone = true;
				    _bEnableCopyRichTextBox = false;
				    break;
				}
			}
            if (!bDone)
            {
                var objectid = this.mDungeon.getDungeonContentAt(poiClick.X, poiClick.Y);
                var obj = this.mObjectAttrInfo.getObject((int) objectid);
                int nItemID = 0;
                if (obj.getItemID() > 0 && !this.mMonsterInfo.isMonster(obj.getItemID()) &&
                    (!this.inRandomDungeon() || !this.mRDReadItemInfo.isReadItem(obj.getItemID())))
                {
                    // アイテムピックアップ
                    nItemID = obj.getItemID();
                }
                {
                    // しかし、モンスタ位置が優先。チェックする
                    foreach (var monster in this.mMonsterLocation)
                    {
                        if (monster.Value.Location.X == poiClick.X && monster.Value.Location.Y == poiClick.Y)
                        {
                            // モンスターをピックアップした
                            obj = this.mObjectAttrInfo.getObject(monster.Key);
                            nItemID = obj.getItemID();
                            break;
                        }
                    }
                }

                if (nItemID > 0)
                {
                    // アイテム有り
                    // 情報表示
                    var item = this.mItemInfo.getAItem(nItemID);
                    this.richTextBox1.Text = item.getHeaderString();
                    _bEnableCopyRichTextBox = true;
                    if (item.getHeaderImage() != null && item.getHeaderImage().Size.Width > 1)
                    {
                        this.picHeader.Image = item.getHeaderImage();
                    }
                    else
                    {
                        this.picHeader.Image = this.mDungeonBlockImage.getImageAt((uint) item.getItemImageID());
                    }
                }
                else if (obj.getObjectCommand() == EObjectCommand.IntoDungeon && this.mLocation.getDungeonUserID() == 0)
                {
                    IslandGroundInfo groundinfo;
                    if (!this.mGQCom.getIslandGroundInfo(out groundinfo))
                    {
                        MessageBox.Show(this.mGQCom.getErrorReasonString());
                        return;
                    }

                    // ダンジョンの入口
                    this.picHeader.Image =
                        this.mDungeonBlockImage.getImageAt(this.mDungeon.getDungeonImageAt(poiClick.X, poiClick.Y));
                    int nDungeonID = groundinfo.getUserIDByCoord(poiClick.X, poiClick.Y);
                    if (nDungeonID == 0)
                    {

                        this.richTextBox1.Text = "謎のダンジョンです\r\n" + "挑戦者を待ち受けています";
                        _bEnableCopyRichTextBox = true;
                    }
                    else
                    {
                        var auser = this.mUserInfo.getAUser(nDungeonID);
                        UnpickedupInfo unpickinfo;
                        this.mGQCom.getUnpickedupItemInfo(out unpickinfo, nDungeonID);

                        this.richTextBox1.Text = auser.getName() + "さんのダンジョンです\r\n" + ("" + unpickinfo.count()) +
                                                 "個の未読情報があります\r\n";
                        _bEnableCopyRichTextBox = true;
                    }
                }
            }
            this.mSelectedCell = poiClick;
            this.drawScreen();
            this.picScreen.Refresh();
        }

        // 出現する
        private void goOtherGround(int nDungeonUesrID, int nDungeonNumber, int nBeforeDungeonUserID, int nBeforeDungeonNumber)
        {
			// 爆発情報をクリアする
            lock (_listBombAnim)
            {
                _listBombAnim.Clear();
            }

            // ダンジョンデータロード
            this.mLocation = new ALocation(this.mGQCom.getUserID(), 0, 0, nDungeonUesrID, nDungeonNumber);
            this.loadDungeon(this.mLocation, false);

            int nNewX = 0;
            int nNewY = 0;
            if (nDungeonUesrID == 0)
            {
                if (nBeforeDungeonUserID == 0 ) {
                    //　謎
                }
                else {

                    // 外に出た
                    bool bBefIsland = nBeforeDungeonUserID == 0;

                    IslandGroundInfo groundinfo;
                    if ( !this.mGQCom.getIslandGroundInfo(out groundinfo) ) {
                        MessageBox.Show(this.mGQCom.getErrorReasonString());
                        return;
                    }
                    var listGround = groundinfo.getIslandGroundByUserID(nBeforeDungeonUserID);

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

                            nNewX = ix; nNewY = iy; //とりあえず更新

                            var nObjectID = this.mDungeon.getDungeonContentAt(ix, iy);
                            var obj = this.mObjectAttrInfo.getObject((int)nObjectID);
                            {
                                if (obj.getObjectCommand() == EObjectCommand.GoDown || obj.getObjectCommand() == EObjectCommand.IntoDungeon)
                                {
                                    nNewX = ix; nNewY = iy; bFind = true;
                                    break;
                                }
                            }
                        }
                        if ( bFind )
                            break;
                    }
                }
            }
            else 
            {
                bool bBefIsland = nBeforeDungeonUserID == 0;
                bool bGoDown = nBeforeDungeonNumber <= nDungeonNumber;

                // ダンジョン内
                int nBefX = 0, nBefY  = 0;

                int nDistMin = int.MaxValue;
                if (!bBefIsland)
                {
                    nBefX = this.mLocation.getIX();
                    nBefY = this.mLocation.getIY();
                }

                int ix1 = 0, iy1 = 0;
                int ix2 = this.mDungeon.getSizeX() - 1, iy2 = this.mDungeon.getSizeY() - 1;
                for (int iy = iy1; iy <= iy2; ++iy)
                {
                    for (int ix = ix1; ix <= ix2; ++ix)
                    {
                        int nDist = Math.Abs(nBefX - ix) + Math.Abs(nBefY - iy);
                        if ( nDist >= nDistMin )
                            continue;

                        var nObjectID = this.mDungeon.getDungeonContentAt(ix, iy);
                        var obj = this.mObjectAttrInfo.getObject((int)nObjectID);
                        bool bFind = false;
                        if (bBefIsland)
                        {
                            if (bGoDown)
                            {
                                if ( nDungeonNumber == 0 ) {

                                    if (obj.getObjectCommand() == EObjectCommand.GoUp || obj.getObjectCommand() == EObjectCommand.GoOutDungeon)
                                    {
                                        bFind = true;
                                    }
                                }
                                else {
                                    if (obj.getObjectCommand() == EObjectCommand.GoUp )
                                    {
                                        bFind = true;
                                    }
                                }
                            }
                            else
                            {
                                if (obj.getObjectCommand() == EObjectCommand.GoDown)
                                {
                                    bFind = true;
                                }
                            }
                        }
                        else
                        {
                            if (bGoDown)
                            {
                                if (obj.getObjectCommand() == EObjectCommand.GoUp) 
                                {
                                    bFind = true;
                                }
                            }
                            else
                            {
                                if (obj.getObjectCommand() == EObjectCommand.GoDown)
                                {
                                    bFind = true;
                                }
                            }
                        }
                        if (bFind)
                        {
                            nNewX = ix; nNewY = iy;
                            nDistMin = nDist;
                        }
                    }
                }
            }

            this.mLocation.setXY(nNewX, nNewY);
            this.loadUnpickedupInfo();
            this.drawScreen();
            this.picScreen.Refresh();
        }

        // コマンド実行
        private void runCommand(Point poiClick, ObjectAttr obj)
        {
            bool bInIsland = this.mLocation.getDungeonUserID() == 0;

            if (obj.getObjectCommand() == EObjectCommand.IntoDungeon)
            {
                // ダンジョンの中に入る
                if (bInIsland)
                {
                    // 誰のダンジョンかを突き止める
                    IslandGroundInfo groundinfo;
                    if (!this.mGQCom.getIslandGroundInfo(out groundinfo))
                    {
                        MessageBox.Show(this.mGQCom.getErrorReasonString());
                        return;
                    }
                    int nNewDugeonID = groundinfo.getUserIDByCoord(poiClick.X, poiClick.Y);

                    if (nNewDugeonID == 0)
                    {
                        // 誰にも所属していないので、ランダムダンジョンらしい
                        nNewDugeonID = 0x40000000;  
                    }

                    this.goOtherGround(nNewDugeonID, 0, 0, 0);
                }
                else
                {
                    MessageBox.Show("入り口のように見えたが行き止まりのようだ");
                }
            }
            else if (obj.getObjectCommand() == EObjectCommand.GoDown)
            {
                int nDungeonDepth = this.mGQCom.getDungeonDepth(this.mLocation.getDungeonUserID());
                if (!bInIsland && nDungeonDepth <= this.mLocation.getDungeonNumber()+1)
                {
                    MessageBox.Show("降りる階段に見えたが行き止まりだった");
                }
                else
                {
                    if ( bInIsland )
                        MessageBox.Show("降りる階段に見えたが行き止まりだった");
                    else
                        this.goOtherGround(this.mLocation.getDungeonUserID(), this.mLocation.getDungeonNumber() + 1, this.mLocation.getDungeonUserID(), this.mLocation.getDungeonNumber());
                }
            }
            else if (obj.getObjectCommand() == EObjectCommand.GoUp)
            {
                if ( bInIsland ) {
                    MessageBox.Show("登ってみたが何も無かった");
                }
                else if ( this.mLocation.getDungeonNumber() == 0 )
                {
                    MessageBox.Show("登ってみたが天井で行き止まりだった");
                }
                else
                {
                    this.goOtherGround(this.mLocation.getDungeonUserID(), this.mLocation.getDungeonNumber()-1, this.mLocation.getDungeonUserID(), this.mLocation.getDungeonNumber());
                }
            }
            else if (obj.getObjectCommand() == EObjectCommand.GoOutDungeon)
            {
                if (bInIsland)
                {
                    MessageBox.Show("どこかにつながっているようでつながっていない");
                }
                else
                {
                    // 外に出る
                    this.goOtherGround(0, 0, this.mLocation.getDungeonUserID(), this.mLocation.getDungeonNumber() );
                }
            }
        }

        // マウス二回クリック
        private void picScreen_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point poiClick = getDungeonCoordInPicture(e.X, e.Y);
            if (poiClick.X < 0 || poiClick.X >= this.mDungeon.getSizeX() || poiClick.Y < 0 || poiClick.Y >= this.mDungeon.getSizeY())
                return;

            int nItemID = 0;
            var objectid = this.mDungeon.getDungeonContentAt(poiClick.X, poiClick.Y);
            var obj = this.mObjectAttrInfo.getObject((int)objectid);
            if (obj.getItemID() > 0 && !this.mMonsterInfo.isMonster(obj.getItemID()) && (!this.inRandomDungeon() || !this.mRDReadItemInfo.isReadItem(obj.getItemID())))
            {   // モンスタでなければ見れる
                nItemID = obj.getItemID();
            }
            {
                int nItemIDBef = nItemID;
                // モンスタ位置をチェックする。モンスター優先だから。
                foreach (var monster in this.mMonsterLocation)
                {
                    if (monster.Value.Location.X == poiClick.X && monster.Value.Location.Y == poiClick.Y)
                    {
                        // モンスターをピックアップした
                        obj = this.mObjectAttrInfo.getObject(monster.Key);
                        nItemID = obj.getItemID();
                        if (this.inRandomDungeon() && this.mRDReadItemInfo.isReadItem(nItemID))
                        {
                            nItemID = nItemIDBef;
                            continue;
                        }
                        break;
                    }
                }
            }
            if ( nItemID > 0 ) {
                // パスがあるときのみファイル処理
                if ( findAndGoPath(poiClick.X, poiClick.Y) ) {
                    nItemID = obj.getItemID();
                    String strFolder = Path.Combine(this.mUserFolder, "" + nItemID);
                    this.mGQCom.getAItem(nItemID, strFolder);

                    // 特殊ダンジョンではモンスターかアイテムを捕まえた
                    if (this.inRandomDungeon())
                    {
                        this.mGQCom.setRDReadItem(nItemID);
                        loadRDReadItemInfo();
//                        loadDungeon(this.mLocation, true);
                    }

                    // アイテムの処理
                    FormOpenItem form = new FormOpenItem(false, strFolder, this.mGQCom, nItemID, this.mLocation.getDungeonUserID());
                    form.ShowDialog();

                    // ピックアップ情報を再読み込み
                    this.loadUnpickedupInfo();

                    // モンスタ情報を再読み込み
                    this.loadMonsterInfo();
                    this.initMonsterPosition();
                }
            }
            else if (poiClick.X == this.mLocation.getIX() && poiClick.Y == this.mLocation.getIY())
            {
                // 何らかのクリック。階段移動など。
//                var nObjectid = this.mDungeon.getDungeonContentAt(poiClick.X, poiClick.Y);
//                var obj = this.mObjectAttrInfo.getObject((int)nObjectid);
                if (obj.getObjectCommand() != EObjectCommand.Nothing)
                    runCommand(poiClick, obj);
            }
            else
            {
                findAndGoPath(poiClick.X, poiClick.Y);
            } 
        }

        /// 目的地までのパスを算出して移動をセットする
        private bool findAndGoPath(int itx, int ity)
        {
            int nSizeX = this.mDungeon.getSizeX();
            int nSizeY = this.mDungeon.getSizeY();
            
            if ( itx < 0 || ity < 0 || itx >= nSizeX || ity >= nSizeY )
                return false;
            
            int nLine = nSizeX+2;
            int[] findbuf = new int[(nSizeX + 2) * (nSizeY + 2)];

            for (int iiy = 0; iiy < nSizeY; ++iiy)
            {
                for (int iix = 0; iix < nSizeX; ++iix)
                {
                    uint nObjectID = this.mDungeon.getDungeonContentAt(iix, iiy);
                    var obj = this.mObjectAttrInfo.getObject((int)nObjectID);
                    if (obj.canWalk() && !this.mMonsterInfo.isMonster(obj.getItemID())) // モンスタの元でないとき移動できる
                        findbuf[(iix+1) + (iiy+1) * nLine] = int.MaxValue - 10;
                    else
                        findbuf[(iix+1) + (iiy+1) * nLine] = -1;
                }
            }

            for (int iix = 0; iix < nSizeX + 2; ++iix)
            {
                findbuf[iix] = findbuf[iix + (nSizeY + 1) * nLine] = -1;
            }
            for (int iiy = 0; iiy < nSizeY + 2; ++iiy)
            {
                findbuf[iiy * nLine] = findbuf[nSizeX + 1 + iiy * nLine] = -1;
            }

            int ix = mLocation.getIX();
            int iy = mLocation.getIY();

            Queue<int> posstack = new Queue<int>();
            int nBeginPos = ix + 1 + (iy + 1) * nLine;
            posstack.Enqueue(nBeginPos);

            bool bFind = false;

            findbuf[nBeginPos] = 0;

            while (posstack.Count>0)
            {
                int nCurPos = posstack.Dequeue();

                ix = nCurPos % nLine;
                iy = nCurPos / nLine;

                if (ix == itx+1 && iy == ity+1)
                {
                    // 発見!
                    bFind = true;
                    break;
                }

                int nCurScore = findbuf[ix + iy * nLine] + 1;
                if (findbuf[nCurPos - nLine] > nCurScore)
                {
                    posstack.Enqueue(nCurPos - nLine);
                    findbuf[nCurPos - nLine] = nCurScore;
                }
                if (findbuf[nCurPos + nLine] > nCurScore)
                {
                    posstack.Enqueue(nCurPos + nLine);
                    findbuf[nCurPos + nLine] = nCurScore;
                }
                if (findbuf[nCurPos - 1] > nCurScore)
                {
                    posstack.Enqueue(nCurPos - 1);
                    findbuf[nCurPos - 1] = nCurScore;
                }
                if (findbuf[nCurPos + 1] > nCurScore)
                {
                    posstack.Enqueue(nCurPos + 1);
                    findbuf[nCurPos + 1] = nCurScore;
                }
            }

            if (!bFind)
                return false;

            // 道筋を得る
            List<int> listPath = new List<int>();
            int nCurPos2 = (ity + 1) * nLine + itx + 1;
            int nCurScore2 = findbuf[nCurPos2];
            int nBeginPos2 = (mLocation.getIY()+1)*nLine+mLocation.getIX()+1;
            while (true)
            {
                if (nCurPos2 == nBeginPos2)
                    break;

                ix = nCurPos2 % nLine;
                iy = nCurPos2 / nLine;

                listPath.Add(ix-1+(iy-1)*(nLine-2));

                --nCurScore2;

                if (findbuf[nCurPos2 - nLine] == nCurScore2 )
                {
                    nCurPos2 -= nLine;
                }
                else if (findbuf[nCurPos2 + nLine] == nCurScore2)
                {
                    nCurPos2 += nLine;
                }
                else if (findbuf[nCurPos2 + 1] == nCurScore2)
                {
                    nCurPos2 += 1;
                }
                else
                {
                    nCurPos2 -= 1;
                }
            }

            listPath.Reverse();

            this.mWalkPath = listPath;
            return true;
        }

        private void FormScreen_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // モンスターの動き
            this.moveMonster();

            // プレイヤーの動き
            if (this.mWalkPath.Count > 0)
            {
                int nNextPos = this.mWalkPath[0];
                this.mWalkPath.RemoveAt(0);

                int ix = nNextPos % this.mDungeon.getSizeX();
                int iy = nNextPos / this.mDungeon.getSizeX();

                this.mLocation.setXY(ix, iy);
                drawScreen();
                this.picScreen.Refresh();
            }
        }

        private void btnChangeAccountInfo_Click(object sender, EventArgs e)
        {
            FormCreateAccount form = new FormCreateAccount(false);
            AUser user = this.mUserInfo.getAUser(this.mGQCom.getUserID());
            form.setInitForChangeUserInfo(user, this.mGQCom);
            form.ShowDialog();

            this.mGQCom.getUserInfo(out this.mUserInfo);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            {
                if (!this.mStartPooling)
                {
                    this.mStartPooling = true;
                    this.mFinishPooling = false;
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            // ポーリング
                            this.mGQCom.polling(out this.mSignalqueue, out this.mLocationInfo, out this.mLoginUserList, out this._realMonsterLocationInfo, this.mLocation);
                            this.mFinishPooling = true;
                        }
                        catch (Exception)
                        {
                            timer2.Enabled = false;
                            MessageBox.Show("サーバに異常が発生しました。ご迷惑をおかけします");
                            Application.Exit();
                        }
                    });
                }
                else
                {
                    if (this.mFinishPooling)
                    {
                        // シグナルの処理
                        foreach (var signal in this.mSignalqueue)
                        {
                            switch (signal.SigType)
                            {
                                case SignalType.RefreshDungeon: 
                                    this.loadDungeon(this.mLocation, true); 
                                    notifyIcon1.BalloonTipText = "ダンジョンが更新されました";
                                    notifyIcon1.ShowBalloonTip(800);
                                    break;
                                case SignalType.RefreshMessage: 
                                    this.loadMessageInfo();
                                    notifyIcon1.BalloonTipText = "メッセージが更新されました";
                                    notifyIcon1.ShowBalloonTip(800);
                                    break;
                                case SignalType.RefreshExpValue: this.loadExpValue(); break;
                                case SignalType.SystemMessage: var form = new FormMessage("システムメッセージ", this.mSignalqueue.getSystemMessage()); form.Show(); break;
                                case SignalType.RefreshUser: this.loadUserInfo(); break;
                                case SignalType.DestroyMonster:
                                    lock (_listBombAnim)
                                    {
                                        _listBombAnim.Add(new BombAnim(signal.Ix, signal.Iy));
                                    }
                                    break;
                            }
                        }

                        this.drawScreen();
                        this.picScreen.Refresh();

                        this.mStartPooling = false;
                        this.mFinishPooling = false;
                    }
                }
            }
        }

        private volatile bool mStartPooling = false;
        private volatile bool mFinishPooling = false;
        private volatile SignalQueue mSignalqueue = null;

        private void btnGetout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("緊急脱出してよろしいですか？", "notice", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            this.mLocation = new ALocation(this.mGQCom.getUserID(), 0, 0, 0, 0);
            this.loadDungeon(this.mLocation, true);
        }

        // 発言をする
        private void btnTalk_Click(object sender, EventArgs e)
        {
            AMessage mes = new AMessage(this.mGQCom.getUserID(), this.txtTalk.Text);
            this.mGQCom.setAMessage(mes);
            txtTalk.SelectAll();
        }

        // 発言を消す
        private void btnTalkCLear_Click(object sender, EventArgs e)
        {
            AMessage mes = new AMessage(this.mGQCom.getUserID(), "");
            this.mGQCom.setAMessage(mes);
            txtTalk.Clear();
        }

        // リッチテキストのリンククリック
        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        // 足あとログ機能
        private void btnAshiatoLog_Click(object sender, EventArgs e)
        {
            List<string> listLog;
            this.mGQCom.getAshiatolog(out listLog);

            StringBuilder log = new StringBuilder();
            foreach (var line in listLog)
            {
                log.AppendLine(line);
            }

            FormMessage form = new FormMessage("足あとログ", log.ToString());
            form.Show();
        }

        private void btnWarp_Click(object sender, EventArgs e)
        {
            Hide();
            FormWarp form = new FormWarp(this.mGQCom);
            var result = form.ShowDialog();
            Show();

            if (result == DialogResult.OK)
            {
                var newlocation = form.getNewLocation();
                if (newlocation != null)
                {
                    this.mLocation = newlocation;
                    this.mLocation.setUserID(this.mGQCom.getUserID());
                    this.loadDungeon(this.mLocation, true);
                    this.mWalkPath.Clear();
                }
            }
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
			if ( !_bEnableCopyRichTextBox)
				richTextBox1.Select(0, 0);
        }
    }

    public class MonsterLocation
    {
        public Point Location;
        public Point MoveDir;
    }

	// 爆発アニメーション用
    public class BombAnim
    {
        private int _timeBegin;
        private int _turn;
		
		public int Ix { get; set; }
        public int Iy { get; set; }

        public BombAnim(int ix, int iy)
        {
            _timeBegin = System.Environment.TickCount;
            _turn = 0;
            Ix = ix;
            Iy = iy;
        }

        public bool Expired()
        {
            return System.Environment.TickCount - _timeBegin > 5000;
        }
        public int IncTurn()
        {
            return _turn++;
        }

        public Image GetBombImage()
        {
            if (_turn%2 == 0)
                return _img1;
            else
                return _img2;
        }

        public static Image _img1, _img2;

        public static void InitBombImage()
        {
            string strDir = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            try
            {
                _img1 = Image.FromFile(Path.Combine(strDir, "bomb1.png"));
                _img2 = Image.FromFile(Path.Combine(strDir, "bomb2.png"));
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("bomb1.pngとbomb2.pngが読めませんでした．モンスターが爆発しません");
            }
        }
    }
}


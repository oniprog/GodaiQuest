using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Drawing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuestServer
{
    public class MongoMaster
    {
        private const int MAX_ISLAND_SIZE = 512;

		// 大陸サイズを返す
        public static int GetIslandSize()
        {
            return MAX_ISLAND_SIZE;
        }

		private MongoServer mMongo;
        private MongoDatabase mDB;
        private MongoCollection<DBUser> mUserCollection;
        private MongoCollection<DBGodaiSystem> mGodaiSystemCollection;
        private MongoCollection<DBDungeon> mDungeonCollection;
        private MongoCollection<DBDungeonBlockImage> mDungeonBlockImageCollection;
        private MongoCollection<DBTilePalette> mBlockImagePaletteCollection;
        private MongoCollection<DBObjectAttr> mObjectAttrCollection;
        private MongoCollection<DBTile> mTileCollection;
        private MongoCollection<DBItem> mItemCollection;
        private MongoCollection<DBLocation> mLocationCollection;
        private MongoCollection<DBIslandGround> mIslandGroundCollection;
        private MongoCollection<DBMessage> mMessageCollection;
        private MongoCollection<DBExpValue> mExpValueCollection;
        private MongoCollection<DBPickedup> mPickupedCollection;
        private MongoCollection<DBItemOwner> mItemOwnerCollection;
        private MongoCollection<DBAshiatoLog> mAshiatoLogCollection;
        private MongoCollection<DBItemArticle> mItemArticleCollection;
        private MongoCollection<DBUnreadArticle> mUnreadArticleCollection;
        private MongoCollection<DBDungeonDepth> mDungeonDepthCollection;
        private MongoCollection<DBMonster> mMonsterCollection;
        private MongoCollection<DBMonster> mRDMonsterCollection;    // ランダムダンジョンモンスターコレクション
        private MongoCollection<DBRDReadItem> mRDReadItemCollection;
        private MongoCollection<DBRSSLastUpdateTime> mRSSLastUpdateTimeCollection;
        private MongoCollection<DBUserFolder> mUserFolderCollection;
        private MongoCollection<DBKeyword> mKeywordCollection;
        private MongoCollection<DBKeywordItem> mKeywordItemCollection;
        private MongoCollection<DBItemTime> mItemTime;

        public MongoMaster()
        {
            this.mMongo = MongoServer.Create("mongodb://localhost");
            this.mDB = this.mMongo["5dai_quest"];
            this.mUserCollection = this.mDB.GetCollection<DBUser>("user");
            this.mGodaiSystemCollection = this.mDB.GetCollection<DBGodaiSystem>("godaisystem");
            this.mDungeonCollection = this.mDB.GetCollection<DBDungeon>("dungeon");
            this.mDungeonBlockImageCollection = this.mDB.GetCollection<DBDungeonBlockImage>("dungeon_block_image");
            this.mBlockImagePaletteCollection = this.mDB.GetCollection<DBTilePalette>("block_image_palette");
            this.mObjectAttrCollection = this.mDB.GetCollection<DBObjectAttr>("object_attr");
            this.mTileCollection = this.mDB.GetCollection<DBTile>("tile");
            this.mItemCollection = this.mDB.GetCollection<DBItem>("item");
            this.mLocationCollection = this.mDB.GetCollection<DBLocation>("location");
            this.mIslandGroundCollection = this.mDB.GetCollection<DBIslandGround>("island_ground");
            this.mMessageCollection = this.mDB.GetCollection<DBMessage>("message");
            this.mExpValueCollection = this.mDB.GetCollection<DBExpValue>("expvalue");
            this.mPickupedCollection = this.mDB.GetCollection<DBPickedup>("pickedup");
            this.mItemOwnerCollection = this.mDB.GetCollection<DBItemOwner>("item_owner");
            this.mAshiatoLogCollection = this.mDB.GetCollection<DBAshiatoLog>("ashiato_log");
            this.mItemArticleCollection = this.mDB.GetCollection<DBItemArticle>("item_artcile");
            this.mUnreadArticleCollection = this.mDB.GetCollection<DBUnreadArticle>("article_unread");
            this.mDungeonDepthCollection = this.mDB.GetCollection<DBDungeonDepth>("dungeon_depth");
            this.mMonsterCollection = this.mDB.GetCollection<DBMonster>("monster");
            this.mRDMonsterCollection = this.mDB.GetCollection<DBMonster>("rd_monster");
            this.mRDReadItemCollection = this.mDB.GetCollection<DBRDReadItem>("rd_readitem");
            this.mRSSLastUpdateTimeCollection = this.mDB.GetCollection<DBRSSLastUpdateTime>("rss_lastupate");
            this.mUserFolderCollection = this.mDB.GetCollection<DBUserFolder>("user_fodler");
            this.mKeywordCollection= this.mDB.GetCollection<DBKeyword>("keyword");
            this.mKeywordItemCollection= this.mDB.GetCollection<DBKeywordItem>("keyword_item");
            this.mItemTime = this.mDB.GetCollection<DBItemTime>("item_time");
        }

		// データベースが有効かを判定する
        public bool IsAvailableMongoDB()
        {
            try
            {
                foreach (var user in mUserCollection.FindAll())
                {
                    System.Console.WriteLine("Cheking... : " + user.Name);
                    break;	// コンパイラの最適化で消えてしまわないようにの処置
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        // ユーザーが有効かどうかをチェックする
        public GodaiLibrary.GodaiQuest.EServerResult tryLogon(out int nID, String strMail, String strPasswordHash)
        {
            nID = 0;
            foreach (DBUser user in this.mUserCollection.FindAll() )
            {
                if (user.Mail.ToLower() == strMail.ToLower() && user.PasswordHash == strPasswordHash)
                {
                    nID = user.UserID;
                    return GodaiLibrary.GodaiQuest.EServerResult.SUCCESS;
                }
                if (user.Mail.ToLower() == strMail.ToLower() )
                    return GodaiLibrary.GodaiQuest.EServerResult.PasswordWrong;
            }
            return GodaiLibrary.GodaiQuest.EServerResult.MissingUser;
        }

        // ユーザを追加する
        public GodaiLibrary.GodaiQuest.EServerResult addUser(out int nUserID, DBUser user)
        {
            var userAlready = this.mUserCollection.FindOne(Query.EQ("Mail", user.Mail));
            if (userAlready != null)
            {
                nUserID = 0;
                return GodaiLibrary.GodaiQuest.EServerResult.UserAlreadyExist;
            }
            nUserID = user.UserID = newUserID();
            this.mUserCollection.Save(user);

            // 土地情報を設定する
            IslandGroundInfo groundinfo = new IslandGroundInfo();
            this.getIslandGroundInfo(out groundinfo);

            int nSize = 8;  // 土地のサイズ

            int nIx = groundinfo.count() % (MAX_ISLAND_SIZE / nSize);
            int nIy = groundinfo.count() / (MAX_ISLAND_SIZE / nSize);
            nIx *= nSize;
            nIy *= nSize;
            IslandGround ground = new IslandGround(user.UserID, nIx, nIy, nIx+nSize-1, nIy+nSize-1);
            DBIslandGround grounddb = new DBIslandGround();
            grounddb.setInit(ground);
            this.mIslandGroundCollection.Save(grounddb);

            //
            return GodaiLibrary.GodaiQuest.EServerResult.SUCCESS;
        }

        // ダンジョンを得る
        public GodaiLibrary.GodaiQuest.EServerResult getDungeon(out GodaiLibrary.GodaiQuest.DungeonInfo dungeon, int nUserID, int nDungeonNumber)
        {
            //
            dungeon = null;

            // 深さを調べる
            var depthdb = this.mDungeonDepthCollection.FindOne(Query.EQ("UserID", nUserID));
            if (depthdb == null)
            {
                if (nDungeonNumber > 0)
                {
                    return EServerResult.NotExistDungeon;
                }
            }
            else
            {
                if (nDungeonNumber >= depthdb.Depth)
                    return EServerResult.NotExistDungeon;
            }

            var dungeonInside = this.mDungeonCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("DungeonNumber", nDungeonNumber)));
            if (dungeonInside == null)
            {
//                return GodaiLibrary.GodaiQuest.EServerResult.MissingDungeon;
                dungeon = makeDefaultDungeon(nUserID, nDungeonNumber);
                return EServerResult.SUCCESS;
            }

            dungeon = new GodaiLibrary.GodaiQuest.DungeonInfo();
            dungeon.setInit(dungeonInside.DungeonNumber, dungeonInside.Dungeon, dungeonInside.SizeX, dungeonInside.SizeY);
            return GodaiLibrary.GodaiQuest.EServerResult.SUCCESS;
        }

        // ダンジョンを設定する(大陸用）
        public EServerResult setDungeonForIsland(int nRealUserID, DungeonInfo dungeon, DungeonBlockImageInfo images, ObjectAttrInfo objectinfo, TileInfo tileinfo)
        {
            // 自分がいじれるところ以外は、上書きする
//            if (nRealUserID != 1)
            {
                DungeonInfo dungeonMoto;
                getDungeon(out dungeonMoto, 0, 0);

                //
                IslandGroundInfo groundinfo;
                getIslandGroundInfo(out groundinfo);

                var listGround = groundinfo.getIslandGroundByUserID(nRealUserID);

                for (int iy = 0; iy < dungeon.getSizeY(); ++iy)
                {
                    for (int ix = 0; ix < dungeon.getSizeX(); ++ix)
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
                        if (bIn)
                            continue;

                        dungeon.setDungeonTileAt(ix, iy, dungeonMoto.getDungeonTileAt(ix, iy));
                    }
                }
            }
             
            return setDungeon(dungeon, 0, 0, images, objectinfo, tileinfo);
        }

        // ダンジョンを設定する
        public EServerResult setDungeon( DungeonInfo dungeon, int nUserID, int nDungeonNumber, DungeonBlockImageInfo images, ObjectAttrInfo objectinfo, TileInfo tileinfo ) {

            // 新しい画像に、番号を割り振る
            var dicImage = new Dictionary<uint, uint>();
            foreach (var imagepair in images)
            {
                if (!imagepair.isNewImage())
                {
                    dicImage.Add((uint)imagepair.getNumber(), (uint)imagepair.getNumber()); // 自身に変換
                }
                else
                {
                    int nNewID = newDungeonBlockImageID();
                    dicImage.Add((uint)imagepair.getNumber(), (uint)nNewID);

                    // 登録
                    var imagedb = this.mDungeonBlockImageCollection.FindOne(Query.EQ("BlockID", imagepair.getNumber()));
                    if (imagedb == null)
                    {
                        imagedb = new DBDungeonBlockImage();
                    }
                    imagedb.setInit(imagepair);
                    this.mDungeonBlockImageCollection.Save(imagedb);
                }
            }

            // オブジェクトIDに新しい番号を割り当てる
            var dicObject = new Dictionary<uint, uint>();
            foreach (var obj in objectinfo)
            {
                if (!obj.isNew())
                {
                    dicObject.Add((uint)obj.getObjectID(), (uint)obj.getObjectID());
                }
                else
                {
                    int nNewID = newObjectID();
                    dicObject.Add((uint)obj.getObjectID(), (uint)nNewID);

                    // 登録
                    var objdb = this.mObjectAttrCollection.FindOne(Query.EQ("ObjectID", obj.getObjectID()));
                    if (objdb == null) { 
                        objdb = new DBObjectAttr();
                    }
                    objdb.setInit(obj);

                    this.mObjectAttrCollection.Save(objdb);
                }
            }

            // ダンジョン内の番号を置換する
            for (int iy = 0; iy < dungeon.getSizeY(); ++iy)
            {
                for (int ix = 0; ix < dungeon.getSizeX(); ++ix)
                {
                    uint nNewID = dicImage[dungeon.getDungeonImageAt(ix, iy)];
                    dungeon.setDungeonImageAt(ix, iy, nNewID);

                    uint nNewObjectID = dicObject[dungeon.getDungeonContentAt(ix, iy)];
                    dungeon.setDungeonContentAt(ix, iy, nNewObjectID);
                }
            }

            // タイルIDを更新する
            HashSet<ulong> setTileiD = new HashSet<ulong>();
            for (int iy = 0; iy < dungeon.getSizeY(); ++iy)
            {
                for (int ix = 0; ix < dungeon.getSizeX(); ++ix)
                {
                    ulong nTileID = dungeon.getDungeonTileAt(ix, iy);
                    setTileiD.Add(nTileID);
                }
            }

            // タイルIDを追加する
            foreach (var tile in tileinfo) 
            {
                uint nImageID = dicImage[tile.getImageID()];
                uint nObjectID = dicObject[tile.getObjectID()];

                ulong nTileID = new Tile(nObjectID, nImageID).getTileID();
                setTileiD.Add(nTileID);
            }

            // タイルIDを登録する
            foreach (var nTileID in setTileiD)
            {
                var tiledb = this.mTileCollection.FindOne(Query.EQ("TileID", nTileID));
                if (tiledb == null)
                {
                    tiledb = new DBTile();
                    tiledb.TileID = nTileID;
                    this.mTileCollection.Save(tiledb);
                }
            }

            // ダンジョンを保存する
            var dungeonDB = this.mDungeonCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("DungeonNumber", nDungeonNumber)));
            if (dungeonDB == null)
            {
                dungeonDB = new DBDungeon(nUserID, nDungeonNumber, dungeon.getDungeon(), dungeon.getSizeX(), dungeon.getSizeY());
            }
            else {
                dungeonDB.Dungeon = dungeon.getDungeon();
                dungeonDB.SizeX = dungeon.getSizeX();
                dungeonDB.SizeY = dungeon.getSizeY();
            }
            this.mDungeonCollection.Save(dungeonDB);
            return EServerResult.SUCCESS;
        }

        // ダンジョンを設定する
        public EServerResult setDungeonInside(DungeonInfo dungeon, int nUserID, int nDungeonNumber)
        {
            // ダンジョンを保存する
            var dungeonDB = this.mDungeonCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("DungeonNumber", nDungeonNumber)));
            if (dungeonDB == null)
            {
                dungeonDB = new DBDungeon(nUserID, nDungeonNumber, dungeon.getDungeon(), dungeon.getSizeX(), dungeon.getSizeY());
            }
            else
            {
                dungeonDB.Dungeon = dungeon.getDungeon();
                dungeonDB.SizeX = dungeon.getSizeX();
                dungeonDB.SizeY = dungeon.getSizeY();
            }
            this.mDungeonCollection.Save(dungeonDB);
            return EServerResult.SUCCESS;
        }

        // 画像を得る
        public EServerResult getDungeonBlockImage( out DungeonBlockImageInfo images ) {

            images = null;

            var listImage = this.mDungeonBlockImageCollection.FindAll();
            if ( listImage == null )
                return EServerResult.SUCCESS;

            images = new DungeonBlockImageInfo();

            foreach(var imagedb in listImage) {
                images.addImage((uint)imagedb.ImageID, imagedb.CanItemImage, imagedb.Image, imagedb.Name, imagedb.OwnerID, imagedb.CreatedTime, false);
            }
                
            return EServerResult.SUCCESS;
        }

        // 画像を設定する
        public EServerResult setDungeonBlockImage( DungeonBlockImageInfo images ) {

            foreach (var imagepair in images)
            {
                DBDungeonBlockImage imagedb;
                if (imagepair.isNewImage())
                {
                    imagedb = new DBDungeonBlockImage();
                    imagedb.setInit(imagepair);
                    imagedb.ImageID = newDungeonBlockImageID();
                }
                else
                {
                    imagedb = this.mDungeonBlockImageCollection.FindOne(Query.EQ("ImageID", imagepair.getNumber()));
                    if (imagedb == null)
                    {
                        imagedb = new DBDungeonBlockImage();
                        imagedb.ImageID = imagepair.getNumber();
                    }
                    imagedb.setInit(imagepair);
                }
//                imagedb.Image = imagepair.getImage();

                this.mDungeonBlockImageCollection.Save(imagedb);
            }
            return EServerResult.SUCCESS;
        }

        /// ブロックイメージパレットの取得
        public EServerResult getBlockImagePalette(out TilePalette palette, int nUserID )
        {
            palette = new TilePalette(nUserID);

            var palettedb = this.mBlockImagePaletteCollection.FindOne(Query.EQ("UserID", nUserID));
            if( palettedb != null )
            {
                foreach (var nID in palettedb.TileIDSet)
                {
                    palette.addTileID(nID);
                }
            }

            return EServerResult.SUCCESS;
        }

        // ブロックイメージパレットを設定する
        public EServerResult setBlockImagePalette(TilePalette palette, int nUserID)
        {
            var palettedb = this.mBlockImagePaletteCollection.FindOne(Query.EQ("UserID", nUserID));
            if (palettedb == null)
            {
                palettedb = new DBTilePalette();
            }
            palettedb.setInit(palette);
			palettedb.UserID = nUserID;

            this.mBlockImagePaletteCollection.Save(palettedb);

            return EServerResult.SUCCESS;
        }

        // オブジェクト情報を得る
        public EServerResult getObjectAttrInfo(out ObjectAttrInfo info)
        {
            info = new ObjectAttrInfo();
            var listObjectAttr = this.mObjectAttrCollection.FindAll();
            if (listObjectAttr == null)
                return EServerResult.SUCCESS;
            foreach (var db in listObjectAttr)
            {
                ObjectAttr obj = new ObjectAttr(db.ObjectID, db.CanWalk, db.ItemID, db.Command, db.CommandSub, false);
                info.addObject(obj);
            }
            return EServerResult.SUCCESS;
        }

        // オブジェクト情報を設定する
        public EServerResult setObjectAttrInfo(ObjectAttrInfo info)
        {
            foreach (var obj in info)
            {
                DBObjectAttr db = this.mObjectAttrCollection.FindOne(Query.EQ("ObjectID", obj.getObjectID()));
                if (db == null)
                {
                    db = new DBObjectAttr();
                }
                db.setInit(obj);
                this.mObjectAttrCollection.Save(db);
            }
            return EServerResult.SUCCESS;
        }

        // タイル情報を得る
        public EServerResult getTileList(out TileInfo tileinfo)
        {
            tileinfo = new TileInfo();
            var listTileID = this.mTileCollection.FindAll();
            if (listTileID == null)
            {
                return EServerResult.SUCCESS;
            }

            foreach (var tiledb in listTileID)
            {
                Tile tile = new Tile(tiledb.TileID);
                tileinfo.addTile(tile);
            }

            return EServerResult.SUCCESS;
        }

        // ユーザ情報を得る
        public EServerResult getUserInfo(out UserInfo userinfo)
        {
            userinfo = new UserInfo();
            var listUser = this.mUserCollection.FindAll();
            if (listUser == null)
            {
                return EServerResult.SUCCESS;
            }

            foreach (var userdb in listUser) 
            {
                AUser user = new AUser(userdb.UserID, userdb.Mail, userdb.Name, userdb.ImageCharacter);
                userinfo.addUesr(user);
            }
            return EServerResult.SUCCESS;
        }

        // アイテムを登録する。登録したIDを返す目的でオブジェクトを返す。
        public EServerResult setAItem(ref AItem item, ImagePair imagepair, int nUserID)
        {
            // アイテムイメージを登録する
            var imagedb = this.mDungeonBlockImageCollection.FindOne(Query.EQ("ImageID", imagepair.getNumber()));
            if (imagedb == null || imagepair.isNewImage()/*前の検索が無駄になるが…*/ ) { 
                imagedb = new DBDungeonBlockImage();
                imagedb.setInit(imagepair);
                if (imagepair.isNewImage())
                {
                    imagedb.ImageID = newDungeonBlockImageID();
                    item.setItemImageID(imagedb.ImageID);
                }
                else
                    imagedb.ImageID = imagepair.getNumber();
            }
            this.mDungeonBlockImageCollection.Save(imagedb);

            var itemdb = this.mItemCollection.FindOne(Query.EQ("ItemID", item.getItemID()));
            if (itemdb == null)
            {
                itemdb = new DBItem();
                itemdb.ItemID = newItemID();
                item.setItemID(itemdb.ItemID);
                item.resetNew();
            }
            itemdb.setInit(item, imagedb.ImageID );
            this.mItemCollection.Save(itemdb);

            // Item owner登録をする
            this.setItemOwner(item.getItemID(), nUserID);

            return EServerResult.SUCCESS;
        }

        // アイテム情報を変更する
        public EServerResult changeAtItem(AItem item)
        {
            var itemdb = this.mItemCollection.FindOne(Query.EQ("ItemID", item.getItemID()));
            if (itemdb == null)
            {
                return EServerResult.MissingItem;
            }
            itemdb.setInit(item, item.getItemImageID());
            this.mItemCollection.Save(itemdb);

            // アイテムの拾い上げ履歴を消す
            this.mPickupedCollection.Remove(Query.EQ("ItemID", item.getItemID()));

            return EServerResult.SUCCESS;
        }

        // アイテム一覧を得る(特定のユーザの)
        public EServerResult getItemInfoByUserId (out ItemInfo iteminfo, int nUserId)
		{
			var itemowner = mItemOwnerCollection.Find (Query.EQ ("UserID", nUserId));
			var listItem = new List<int> ();

			foreach (var itemownerdb in itemowner) {
				listItem.Add (itemownerdb.ItemID);
			}

            iteminfo = new ItemInfo();
			foreach( var nItemId in listItem ) {
				var itemdb_All = this.mItemCollection.Find(Query.EQ ("ItemID", nItemId));
				if (itemdb_All != null ) {
					foreach(var itemdb in itemdb_All ) {
		                AItem item = new AItem(itemdb.ItemID, itemdb.ItemImageID, itemdb.HeaderString, itemdb.HeaderImage, false);
		                iteminfo.addItem(item);
					}
				}
            }
            return EServerResult.SUCCESS;
        }

        // アイテム一覧を得る
        public EServerResult getItemInfo(out ItemInfo iteminfo)
        {
            iteminfo = new ItemInfo();
            var listItem = this.mItemCollection.FindAll();
            foreach (var itemdb in listItem)
            {
                AItem item = new AItem(itemdb.ItemID, itemdb.ItemImageID, itemdb.HeaderString, itemdb.HeaderImage, false);
                iteminfo.addItem(item);
            }
            return EServerResult.SUCCESS;
        }

        // ユーザ情報を設定する
        public EServerResult setAUser(AUser user)
        {
            var userdb = this.mUserCollection.FindOne(Query.EQ("UserID", user.getUserID()));
            if (userdb == null)
            {
                return EServerResult.SUCCESS; // いいのかな？
            }

            userdb.ImageCharacter = user.getCharacterImage() == null ? null : user.getCharacterImage() is Bitmap
                ? user.getCharacterImage() as Bitmap
                : new Bitmap(user.getCharacterImage());
            userdb.Name = user.getName();
            this.mUserCollection.Save(userdb);

            return EServerResult.SUCCESS;
        }

        // 位置情報を設定する
        public EServerResult setALocation(int nUserID, ALocation loc)
        {
            var locdb = this.mLocationCollection.FindOne(Query.EQ("UserID", nUserID));
            if (locdb == null)
            {
                locdb = new DBLocation();
            }
            locdb.setInit(nUserID, loc);
            this.mLocationCollection.Save(locdb);

            return EServerResult.SUCCESS;
        }

        // 位置情報一覧を得る
        public EServerResult getLocationInfo(out LocationInfo locinfo)
        {
            var loclist = this.mLocationCollection.FindAll();
            locinfo = new LocationInfo();
            foreach (var locdb in loclist)
            {
                ALocation loc = new ALocation(locdb.UserID, locdb.Ix, locdb.iy, locdb.DugeonUserID, locdb.DungeonNumber);
                locinfo.addLocation(locdb.UserID, loc); 
            }

            return EServerResult.SUCCESS;
        }

        // 大陸土地情報を得る
        public EServerResult getIslandGroundInfo(out IslandGroundInfo islandgroundinfo)
        {
            islandgroundinfo = new IslandGroundInfo();

            var listIslandGround = this.mIslandGroundCollection.FindAll();
            foreach (var islandgrounddb in listIslandGround)
            {
                IslandGround islandground = new IslandGround(islandgrounddb.UserID, islandgrounddb.Ix1, islandgrounddb.Iy1, islandgrounddb.Ix2, islandgrounddb.Iy2);
                islandgroundinfo.addIslandGround(islandground);
            }
            return EServerResult.SUCCESS;
        }

        // オブジェクト情報を変える
        public EServerResult changeObjectAttr(ObjectAttr obj)
        {
            var objectdb = this.mObjectAttrCollection.FindOne(Query.EQ("ObjectID", obj.getObjectID()));
            if (objectdb == null)
            {
                return EServerResult.MissingObject;
            }
            objectdb.setInit(obj);
            this.mObjectAttrCollection.Save(objectdb);

            return EServerResult.SUCCESS;
        }

        // 画像情報を差し替える
        public EServerResult changeDungeonBlockImagePair(ImagePair imagepair)
        {
            var imagedb = this.mDungeonBlockImageCollection.FindOne(Query.EQ("ImageID", imagepair.getNumber()));
            if (imagedb == null)
            {
                return EServerResult.MissingDugeonBlockImage;
            }
            imagedb.setInit(imagepair);
            this.mDungeonBlockImageCollection.Save(imagedb);

            return EServerResult.SUCCESS;
        }

        // メッセージを得る
        public EServerResult getMessageInfo(out MessageInfo mesinfo)
        {
            mesinfo = new MessageInfo();
            var listMessage = this.mMessageCollection.FindAll();
            foreach (var mesdb in listMessage)
            {
                AMessage mes = new AMessage(mesdb.UserID, mesdb.Message);
                mesinfo.addAMessage(mes);
            }

            return EServerResult.SUCCESS;
        }

        // メッセージを設定する
        public EServerResult setAMessage( AMessage mes ) {

            var mesdb = this.mMessageCollection.FindOne(Query.EQ("UserID", mes.getUserID()));
            if (mesdb == null)
            {
                mesdb = new DBMessage();
            }
            mesdb.setInit(mes);
            this.mMessageCollection.Save(mesdb);

            return EServerResult.SUCCESS;
        }


        // 経験値を得る
        public EServerResult getExpValue(out ExpValue expvalue, int nUserID)
        {
            var expvaluedb = this.mExpValueCollection.FindOne(Query.EQ("UserID", nUserID));
            if (expvaluedb == null)
            {
#if false
                expvalue = new ExpValue(nUserID, 10000, 1000); 
#else
                expvalue = new ExpValue(nUserID, 0, 0); 
#endif
            }
            else
            {
                expvalue = new ExpValue(nUserID, expvaluedb.Value, expvaluedb.Total);
            }
            return EServerResult.SUCCESS;
        }

        // 経験値を設定する
        public EServerResult setExpValue(int nUserID, ExpValue expvalue)
        {
            var expvaluedb = this.mExpValueCollection.FindOne(Query.EQ("UserID", nUserID));
            if (expvaluedb == null)
                expvaluedb = new DBExpValue();
            expvaluedb.setInit(expvalue);
            this.mExpValueCollection.Save(expvaluedb);

            return EServerResult.SUCCESS;
        }

        // 記事をすでに見ているかを判定する
        public bool isAlreadyPickedupItem(int nUserID, int nItemID)
        {
            var pickupeddb = this.mPickupedCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("ItemID", nItemID)));
            return pickupeddb != null; // 存在すれば見ている
        }

        // 記事を見たことを記録する
        public void setPickedupItem(int nUserID, int nItemID)
        {
            var pickupeddb = this.mPickupedCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("ItemID", nItemID)));
            if (pickupeddb == null)
            {
                pickupeddb = new DBPickedup();
            }
            pickupeddb.UserID = nUserID;
            pickupeddb.ItemID = nItemID;
            pickupeddb.PickupedTime = DateTime.Now;
            this.mPickupedCollection.Save(pickupeddb);
        }

        // 見たことのあるアイテム番号を消す
        public void removeAlreadyPickedupItem(ref HashSet<int> setItemID, int nDungeonID, int nUserID)
        {
            // 拾い済みのアイテムリスト
            var listPickedup = this.mPickupedCollection.Find(Query.EQ("UserID", nUserID));
            if (listPickedup == null)
                return;

            if (nUserID == nDungeonID)
            {
                // ダンジョンのオーナ自身のとき
                HashSet<int> setNew = new HashSet<int>();
                foreach (var nItemID in setItemID)
                {
                    var unreaddb = this.mUnreadArticleCollection.Find(Query.And(Query.EQ("UserID", nUserID), Query.EQ("ItemID", nItemID)));
                    if (unreaddb == null || unreaddb.Count() == 0)
                        continue;   // null 読み済みのとき 
                    setNew.Add(nItemID);
                }
                setItemID = setNew;
            }
            else
            {
                foreach (var pickupdb in listPickedup)
                {
                    // かつ、未読リストにアイテム番号がないとき
                    var unreaddb = this.mUnreadArticleCollection.Find(Query.And(Query.EQ("UserID", nUserID), Query.EQ("ItemID", pickupdb.ItemID)));
                    if (unreaddb != null && unreaddb.Count() > 0 )
                        continue;   // 未読リストにある＝読んでない
                    setItemID.Remove(pickupdb.ItemID);
                }
            }
        }

        // Itemのオーナーを得る
        public int getItemOwner(int nItemID)
        {
            var itemownerdb = this.mItemOwnerCollection.FindOne(Query.EQ("ItemID", nItemID));
            if (itemownerdb == null) {
                return 0;   // Owner無し？
            }
            return itemownerdb.UserID;
        }

        // Itemのオーナーを設定する
        public void setItemOwner(int nItemID, int nOwnerID)
        {
            var itemownerdb = this.mItemOwnerCollection.FindOne(Query.EQ("ItemID", nItemID));
            if (itemownerdb == null)
            {
                itemownerdb = new DBItemOwner();
                itemownerdb.ItemID = nItemID;
            }
            itemownerdb.UserID = nOwnerID;
            this.mItemOwnerCollection.Save(itemownerdb);
        }

        // Itemのオーナーが指定されたユーザのものだけを残す
        public HashSet<int> getItemIDListBelongUser(int nUserID)
        {
            HashSet<int> setRet = new HashSet<int>();

            var listItemOwner = this.mItemOwnerCollection.Find(Query.EQ("UserID", nUserID));
            if (listItemOwner == null)
                return setRet;

            foreach (var itemownerdb in listItemOwner)
            {
                setRet.Add(itemownerdb.ItemID);
            }
            return setRet;
        }

        // 足跡を得る
        public EServerResult getAshiato(out PickupedInfo pickupinfo, int nItemID)
        {
            pickupinfo = new PickupedInfo();

            var listReader = this.mPickupedCollection.Find(Query.EQ("ItemID", nItemID));
            foreach (var reader in listReader)
            {
                APickuped info = new APickuped(reader.UserID, reader.ItemID, reader.PickupedTime);
                pickupinfo.addPickupedInfo(info);
            }

            return EServerResult.SUCCESS;
        }

        // 足あとログを記録する
        public EServerResult addAshiatoLog(int nUserID, String strLogLine)
        {
            var logdb = this.mAshiatoLogCollection.FindOne(Query.EQ("UserID", nUserID));
            if (logdb == null)
            {
                logdb = new DBAshiatoLog();
                logdb.Log = "";
                logdb.UserID = nUserID;
            }
            String[] lines = logdb.Log.Split('\n');
            StringBuilder newline = new StringBuilder();
            var datetime = DateTime.Now.ToLocalTime();
            newline.Append( "["+datetime.ToShortDateString() + " "+ datetime.ToShortTimeString() + "]: " +  strLogLine + "\n");
            for (int it = 0; it < 100; ++it)
            {
                if (lines.Length <= it )
                    break;
                newline.Append(lines[it] + "\n");
            }
            logdb.Log = newline.ToString();

            this.mAshiatoLogCollection.Save(logdb);

            return EServerResult.SUCCESS;
        }

        // 足あとログを得る
        public EServerResult getAshiatoLog(out List<String> listLog, int nUserID)
        {

            listLog = new List<String>();
            var logdb = this.mAshiatoLogCollection.FindOne(Query.EQ("UserID", nUserID));
            if (logdb == null)
            {
                return EServerResult.SUCCESS;
            }
            String[] lines = logdb.Log.Split('\n');
            foreach (var line in lines) 
            {
                listLog.Add(line);
            }

            return EServerResult.SUCCESS;
        }

        // アーティクルを得る
        public bool getItemArticle(out ItemArticle article, int nItemID, int nArticleID)
        {
            var articledb = this.mItemArticleCollection.FindOne(Query.And(Query.EQ("ArticleID", nArticleID), Query.EQ("ItemID", nItemID)));
            if (articledb == null)
            {
                article = null;
                return false;
            }

            article = new ItemArticle(nItemID, nArticleID, articledb.UserID, articledb.Contents, articledb.CreateTime);
            return true;
        }

        // 存在するアーティクルの数を数える
        public int countItemArticle(int nItemID)
        {
            int nCnt = (int)this.mItemArticleCollection.Count(Query.EQ("ItemID", nItemID));
            return nCnt;
        }

        // アーティクルをセットする
        // countを使ってセットする場所は調べ済み、のはず。
        public void setItemArticle(ItemArticle article)
        {
            var articledb = this.mItemArticleCollection.FindOne(Query.And(Query.EQ("ItemID", article.getItemID()), Query.EQ("ArticleID", article.getArticleID())));
            if (articledb == null)
            {
                articledb = new DBItemArticle();
            }
            articledb.setInit(article);

            this.mItemArticleCollection.Save(articledb);
        }

        // 未読を書き込む
        public void setUnreadArticle(ItemArticle article, UserInfo userinfo)
        {
            foreach (var user in userinfo)
            {
                // 書き込み者には未読にならない
                if ( user.getUserID() == article.getUserID() )
                    continue;
                // 未読が無ければ書き込む
                var unreaddb = this.mUnreadArticleCollection.FindOne(Query.And(Query.EQ("ItemID", article.getItemID()), Query.EQ("ArticleID", article.getArticleID()), Query.EQ("UserID", user.getUserID())));
                if ( unreaddb != null )
                    continue;
                unreaddb = new DBUnreadArticle();
                unreaddb.ArticleID = article.getArticleID();
                unreaddb.ItemID = article.getItemID();
                unreaddb.UserID = user.getUserID();
                this.mUnreadArticleCollection.Save(unreaddb);
            }
        }

        // 未読を得る
        public EServerResult getUnreadArticleForAUser(out UnreadArticleInfo unreadinfo, int nItemID, int nUserID)
        {
            unreadinfo = new UnreadArticleInfo();
            var listUnread = this.mUnreadArticleCollection.Find(Query.And(Query.EQ("ItemID", nItemID), Query.EQ("UserID", nUserID)));
            if (listUnread == null)
            {
                return EServerResult.SUCCESS;
            }

            foreach (var unreaddb in listUnread) 
            {
                UnreadArticle unread = new UnreadArticle(unreaddb.ItemID, unreaddb.ArticleID, unreaddb.UserID);
                unreadinfo.addUnreadArticle(unread);
            }

            return EServerResult.SUCCESS;
        }

        // 未読の取り消し
        public EServerResult removeUnreadArticle(int nItemID, int nUserID)
        {
            var unreaddb = this.mUnreadArticleCollection.Find(Query.And(Query.EQ("ItemID", nItemID), Query.EQ("UserID", nUserID)));
            if (unreaddb == null)
            {
                return EServerResult.AlreadyRead;
            }
            this.mUnreadArticleCollection.Remove(Query.And(Query.EQ("ItemID", nItemID), Query.EQ("UserID", nUserID)));
            return EServerResult.SUCCESS;
        }

        // 未読を読んだ
        public List<int> getUnreadArticleOwnerList(int nItemID, int nUserID)
        {
            List<int> listArtcileOwner = new List<int>();
            var listUnread = this.mUnreadArticleCollection.Find(Query.And(Query.EQ("ItemID", nItemID), Query.EQ("UserID", nUserID)));
            foreach (var unreaddb in listUnread)
            {
                var articledb = this.mItemArticleCollection.FindOne(Query.And(Query.EQ("ItemID", nItemID), Query.EQ("ArticleID", unreaddb.ArticleID)));
                listArtcileOwner.Add(articledb.UserID);
            }

            return listArtcileOwner;
        }

        // アーティクルを削除する
        public EServerResult deleteLastItemArticle(int nItemID, int nUserID)
        {
            int nCnt = this.countItemArticle(nItemID);
            for (int it = nCnt - 1; it >= 0; --it)
            {
                var articledb = this.mItemArticleCollection.FindOne(Query.And(Query.EQ("ItemID", nItemID), Query.EQ("ArticleID", it)));
                if (articledb == null)
                    continue;
                if ( articledb.UserID != nUserID )
                    continue;
                if ( articledb.Contents.Length == 0 )
                    continue;
                articledb.Contents = "";
                this.mItemArticleCollection.Save(articledb);
                return EServerResult.SUCCESS;
            }

            return EServerResult.NotYourArticle;
        }

        // ダンジョンの深さを得る
        public int getDungeonDepth(int nUserID)
        {
            var depthdb = this.mDungeonDepthCollection.FindOne(Query.EQ("UserID", nUserID));
            if (depthdb == null)
            {
                depthdb = new DBDungeonDepth();
                depthdb.Depth = 1;
            }
            return depthdb.Depth;
        }

        // ダンジョンの深さを設定する
        public EServerResult setDungeonDepth(int nUserID, int nNewDepth)
        {
            var depthdb = this.mDungeonDepthCollection.FindOne(Query.EQ("UserID", nUserID));
            if (depthdb == null)
            {
                depthdb = new DBDungeonDepth();
                depthdb.UserID = nUserID;
            }
            depthdb.Depth = nNewDepth;
            this.mDungeonDepthCollection.Save(depthdb);
            return EServerResult.SUCCESS;
        }

        // モンスタかどうかを得る
        public EServerResult getMonsterInfo(out MonsterInfo monsterinfo) {

            monsterinfo = new MonsterInfo();
            var listmonster = this.mMonsterCollection.FindAll();
            foreach (var monsterdb in listmonster)
            {
                monsterinfo.addMonster(monsterdb.ItemID);
            }
            return EServerResult.SUCCESS;
        }

        // モンスタの設定をする
        public void setMonster(int nItemID, bool bMonster)
        {
            var monsterdb = this.mMonsterCollection.FindOne(Query.EQ("ItemID", nItemID));
            if (bMonster)
            {
                if (monsterdb != null)
                    return;
                monsterdb = new DBMonster();
                monsterdb.ItemID = nItemID;
                this.mMonsterCollection.Save(monsterdb);
            }
            else
            {
                if (monsterdb == null)
                    return;
                this.mMonsterCollection.Remove(Query.EQ("ItemID", nItemID));
            }
        }

        // NPC用ランダムダンジョンを作成する
        public void setDungeonByNPC(ulong[] dungeon, int nSizeX, int nSizeY, int nDungeonID, int nDungeonNumber)
        {
            var dungeondb = this.mDungeonCollection.FindOne(Query.And(Query.EQ("UserID", nDungeonID), Query.EQ("DungeonNumber", nDungeonNumber)));
            if (dungeondb == null)
            {
                dungeondb = new DBDungeon(nDungeonID, nDungeonNumber, dungeon, nSizeX, nSizeY);
                this.mDungeonCollection.Save(dungeondb);
            }
            else
            {
                DBDungeon dungeonNew = new DBDungeon(nDungeonID, nDungeonNumber, dungeon, nSizeX, nSizeY);
                dungeonNew._id = dungeondb._id;
                this.mDungeonCollection.Save(dungeonNew);
            }
        }


        // モンスタかどうかの情報を得る(ランダムダンジョン)
        public EServerResult getMonsterInfoForRandomDungeon(out MonsterInfo monsterinfo)
        {
            monsterinfo = new MonsterInfo();
            var listmonster = this.mRDMonsterCollection.FindAll();
            foreach (var monsterdb in listmonster)
            {
                monsterinfo.addMonster(monsterdb.ItemID);
            }
            return EServerResult.SUCCESS;
        }
        // モンスタの設定をする(ランダムダンジョン)
        public void setMonsterForRandomDungeon(int nItemID, bool bMonster)
        {
            var monsterdb = this.mRDMonsterCollection.FindOne(Query.EQ("ItemID", nItemID));
            if (bMonster)
            {
                if (monsterdb != null)
                    return;
                monsterdb = new DBMonster();
                monsterdb.ItemID = nItemID;
                this.mRDMonsterCollection.Save(monsterdb);
            }
            else
            {
                if (monsterdb == null)
                    return;
                this.mRDMonsterCollection.Remove(Query.EQ("ItemID", nItemID));
            }
        }

        // ランダムダンジョンの読み込み済みアイテムリストを返す
        public EServerResult getRDReadItemInfo(out RDReadItemInfo readiteminfo, int nUserID)
        {
            readiteminfo = new RDReadItemInfo(nUserID);
            var listreaditem = this.mRDReadItemCollection.Find(Query.EQ("UserID", nUserID));
            if (listreaditem == null)
            {
                return EServerResult.SUCCESS;
            }
            foreach (var readitemdb in listreaditem)
            {
                readiteminfo.readItem(readitemdb.ItemID);
            }
            return EServerResult.SUCCESS;
        }

        // ランダムダンジョンのアイテムを読み込み済みにする
        public bool setRDReadItem(int nUserID, int nItemID)
        {
            var readitemdb = this.mRDReadItemCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("ItemID", nItemID)));
            if (readitemdb != null)
            {
                return false;
            }
            readitemdb = new DBRDReadItem();
            readitemdb.ItemID = nItemID;
            readitemdb.UserID = nUserID;
            this.mRDReadItemCollection.Save(readitemdb);

            return true;
        }

        // ランダムダンジョンのアイテム読み込み済みリストをクリアする
        public bool clearRDReadItemInfo()
        {
            try
            {
                this.mRDReadItemCollection.RemoveAll();
            }
            catch(Exception){}
            return true;
        }

        // パスワードを変更する
        public EServerResult changePassword(int nUserID, string strNewPasswordHash)
        {
            var auser = this.mUserCollection.FindOne(Query.EQ("UserID", nUserID));
            if (auser == null)
            {
                return EServerResult.MissingUser;
            }

            auser.PasswordHash = strNewPasswordHash;
            this.mUserCollection.Save(auser);

            return EServerResult.SUCCESS;
        }

        // 最終更新日付を得る
        public DateTime getLastUpdateTimeOfRSS(int nItemID, DateTime defaulttime)
        {
            var lastupdatedb = this.mRSSLastUpdateTimeCollection.FindOne(Query.EQ("ItemID", nItemID));
            if (lastupdatedb == null)
            {
                return defaulttime;
            }
            return lastupdatedb.LastUpdateTime;
        }

        // 最終更新日付を設定する
        public void setLastUpdateTimeOfRSS(int nItemID, DateTime lastupdate)
        {
            var lastupdatedb = this.mRSSLastUpdateTimeCollection.FindOne(Query.EQ("ItemID", nItemID));
            if (lastupdatedb == null)
            {
                lastupdatedb = new DBRSSLastUpdateTime();
                lastupdatedb.ItemID = nItemID;
            }
            lastupdatedb.LastUpdateTime = lastupdate;
            this.mRSSLastUpdateTimeCollection.Save(lastupdatedb);
        }

        // ユーザのフォルダを設定する
        public EServerResult setUserFolder(int nUserID, String strComputerName, String strFolder)
        {
            var folderdb = this.mUserFolderCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("ComputerName", strComputerName)));
            if (folderdb == null)
            {
                folderdb = new DBUserFolder();
                folderdb.UserID = nUserID;
                folderdb.ComputerName = strComputerName;
            }
            folderdb.Folder = strFolder;
            this.mUserFolderCollection.Save(folderdb);
            return EServerResult.SUCCESS;
        }

        // ユーザのフォルダを得る
        public String getUserFolder(int nUserID, String strComputerName)
        {
            var folderdb = this.mUserFolderCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("ComputerName", strComputerName)));
            if (folderdb == null)
            {
                return null;
            }

            return folderdb.Folder;
        }


        // UserIDを得る
        private int newUserID()
        {
            while (true)
            {
                var findone = this.mGodaiSystemCollection.FindOne();
                if ( findone == null ) {
                    this.mGodaiSystemCollection.Save(new DBGodaiSystem());
                }
                else {
                    int nMaxID = findone.MaxUserID;
                    var query = Query.EQ("MaxUserID", nMaxID);
                    var sortBy = SortBy.Null;
                    var update = Update.Inc("MaxUserID", 1);
                    if (this.mGodaiSystemCollection.FindAndModify(query, sortBy, update, false) != null)
                        return nMaxID;
                }
            }
        }

        // DungeonBlockImageIDを得る
        private int newDungeonBlockImageID()
        {
            while (true)
            {
                var findone = this.mGodaiSystemCollection.FindOne();
                if (findone == null)
                {
                    this.mGodaiSystemCollection.Save(new DBGodaiSystem());
                }
                else
                {
                    int nMaxID = this.mGodaiSystemCollection.FindOne().MaxDungeonBlockImageID;
                    var query = Query.EQ("MaxDungeonBlockImageID", nMaxID);
                    var sortBy = SortBy.Null;
                    var update = Update.Inc("MaxDungeonBlockImageID", 1);
                    if (this.mGodaiSystemCollection.FindAndModify(query, sortBy, update, false) != null)
                    {
                        return nMaxID;
                    }
                }
            }
        }

        // ObjectIDを得る
        private int newObjectID()
        {
            while (true)
            {
                var findone = this.mGodaiSystemCollection.FindOne();
                if (findone == null)
                {
                    this.mGodaiSystemCollection.Save(new DBGodaiSystem());
                }
                else
                {
                    int nMaxID = findone.MaxObjectID;
                    var query = Query.EQ("MaxObjectID", nMaxID);
                    var sortBy = SortBy.Null;
                    var update = Update.Inc("MaxObjectID", 1);
                    if (this.mGodaiSystemCollection.FindAndModify(query, sortBy, update, false) != null)
                        return nMaxID;
                }
            }
        }

        // ItemIDを得る
        private int newItemID()
        {
            while (true)
            {
                var findone = this.mGodaiSystemCollection.FindOne();
                if (findone == null)
                {
                    this.mGodaiSystemCollection.Save(new DBGodaiSystem());
                }
                else
                {
                    int nMaxID = findone.MaxItemID;
                    var query = Query.EQ("MaxItemID", nMaxID);
                    var sortBy = SortBy.Null;
                    var update = Update.Inc("MaxItemID", 1);
                    if (this.mGodaiSystemCollection.FindAndModify(query, sortBy, update, false) != null)
                    {
                        if (nMaxID >= 1)
                            return nMaxID;
                    }
                }
            }
        }

        // KeywordIDを得る 
        private int newKeywordID()
        {
            while (true)
            {
                var findone = this.mGodaiSystemCollection.FindOne();
                if (findone == null)
                {
                    this.mGodaiSystemCollection.Save(new DBGodaiSystem());
                }
                else
                {
					if (findone.MaxKeywordID <= 1 ) {
						mGodaiSystemCollection.Save(findone);
					}
                    int nMaxID = findone.MaxKeywordID;
                    var query = Query.EQ("MaxKeywordID", nMaxID);
                    var sortBy = SortBy.Null;
                    var update = Update.Inc("MaxKeywordID", 1);
                    if (this.mGodaiSystemCollection.FindAndModify(query, sortBy, update, false) != null)
                    {
						return nMaxID;
                    }
                }
            }
        }



#if false
        // ItemImageIDを得る
        private int newItemImageID()
        {
            while (true)
            {
                var findone = this.mGodaiSystemCollection.FindOne();
                if (findone == null)
                {
                    this.mGodaiSystemCollection.Save(new DBGodaiSystem());
                }
                else
                {
                    int nMaxID = findone.MaxItemImageID;
                    var query = Query.EQ("MaxItemImageID", nMaxID);
                    var sortBy = SortBy.Null;
                    var update = Update.Inc("MaxItemImageID", 1);
                    if (this.mGodaiSystemCollection.FindAndModify(query, sortBy, update, false) != null)
                    {
                        if (nMaxID >= 1)
                            return nMaxID;
                    }
                }
            }
        }
#endif
        // ディフォルトダンジョンを作成する
        private DungeonInfo makeDefaultDungeon(int nUserID, int nDungeonNumber)
        {
            if (nUserID == 0)
            {
                ulong[] dungeonmap = new ulong[MAX_ISLAND_SIZE * MAX_ISLAND_SIZE];

                DungeonInfo dungeon = new DungeonInfo();
                dungeon.setInit(0, dungeonmap, MAX_ISLAND_SIZE, MAX_ISLAND_SIZE);

                return dungeon;
            }
            else
            {
                ulong[] dungeonmap = new ulong[11*11];

                DungeonInfo dungeon = new DungeonInfo();
                dungeon.setInit(nDungeonNumber, dungeonmap, 11, 11);

                return dungeon;
            }
        }

		// キーワードを登録する(同一キーワードがあったら弾く)
        public EServerResult registerKeyword(out int nKeywordID, int nUserId, string keyword, int nPriority)
        {
            nKeywordID = 0;
            DBKeyword keywordDB;
			var findone = this.mKeywordCollection.FindOne(Query.And(Query.EQ("UserID", nUserId), Query.EQ("Keyword", keyword) ) );
            if (findone != null)
                return EServerResult.SameKeyword;

            keywordDB = new DBKeyword();
            keywordDB.UserID = nUserId;
            keywordDB.Keyword = keyword;
            keywordDB.KeywordID = nKeywordID = newKeywordID();
            keywordDB.KeywordPriority = nPriority;
            mKeywordCollection.Save(keywordDB);
            return EServerResult.SUCCESS;
        }

		// キーワードを変更する
        public EServerResult modifyKeyword(int nUserId, int nKeywordId, string newKeyword)
        {
			var findone = this.mKeywordCollection.FindOne(Query.And(Query.EQ("UserID", nUserId), Query.EQ("Keyword", newKeyword)) );
            if (findone != null)
                return EServerResult.SameKeyword;
			
			findone = this.mKeywordCollection.FindOne(Query.And(Query.EQ("UserID", nUserId), Query.EQ("KeywordID", nKeywordId)) );
            if (findone == null)
                return EServerResult.MissingKeyword;

            var update = Update.Rename(findone.Keyword, newKeyword);
            mKeywordCollection.Update(Query.And(Query.EQ("UserID", nUserId), Query.EQ("KeywordID", nKeywordId)), update);
            return EServerResult.SUCCESS;
        }

		// キーワードの優先順位を変更する
        public EServerResult modifyKeywordPriority(int nUserId, int nKeywordId, int newPriority)  
        {
			var findone = this.mKeywordCollection.FindOne(Query.And(Query.EQ("UserID", nUserId), Query.EQ("KeywordID", nKeywordId)) );
            if (findone == null)
                return EServerResult.MissingKeyword;

            var update = Update.Set("KeywordPriority", BsonValue.Create(newPriority));
            mKeywordCollection.Update(Query.And(Query.EQ("UserID", nUserId), Query.EQ("KeywordID", nKeywordId)), update);
            return EServerResult.SUCCESS;
        }

		// キーワードをアイテムに関連付ける
        public EServerResult attachKeyword(int keywordId, int nItemID, int nItemPriority) 
        {
            var findone =
                this.mKeywordItemCollection.FindOne(Query.And( Query.EQ("KeywordID", keywordId), Query.EQ("ItemID", nItemID)));
            if (findone != null)
                return EServerResult.SUCCESS;

            var keywordItemDB = new DBKeywordItem();
            keywordItemDB.ItemPriority = nItemPriority;
            keywordItemDB.KeywordID = keywordId;
            keywordItemDB.ItemID = nItemID;
			mKeywordItemCollection.Save(keywordItemDB);
            return EServerResult.SUCCESS;
        }

		// キーワードに関連づけたアイテムを除く
        public void detachKeyword(int nKeywordId, int nItemID)
        {
            this.mKeywordItemCollection.Remove(Query.And(Query.EQ("KeywordID", nKeywordId), Query.EQ("ItemID", nItemID)));
        }

		// キーワード一覧を得る
        public KeywordUserInfo listKeyword(int nUserID)
        {
            var ret = new KeywordUserInfo();
            var find = mKeywordCollection.Find(Query.And(Query.EQ("UserID", nUserID)));
            if (find == null)
            {
                return ret;
            }

            foreach (var akeywordDB in find)
            {
                var akeyword = new AKeyword(akeywordDB.KeywordID, akeywordDB.Keyword, akeywordDB.KeywordPriority);
                ret.addKeyword(akeyword);
            }
            return ret;
        }

		// キーワードに関連付けたアイテム一覧を得る
        public AKeyword getKeywordDetail(int nKeywordId)
        {
            var ret = new AKeyword(nKeywordId, "", 0);
            var find = mKeywordItemCollection.Find(Query.And(Query.EQ("KeywordID", nKeywordId)));
            if (find == null)
            {
                return ret;
            }

            foreach (var keywordUserDB in find) 
            {
                ret.addKeywordItem(new AKeywordItem(keywordUserDB.ItemID, keywordUserDB.ItemPriority));
            }
			return ret;
		}

		// アイテムの優先順位を変える
        public EServerResult modifyKeywordItemPriority(int nKeywordID, int nItemID, int nNewPriority)
        {
            var update = Update.Set("ItemPriority", BsonValue.Create(nNewPriority));
            mKeywordItemCollection.Update(Query.And(Query.EQ("KeywordID", nKeywordID), Query.EQ("ItemID", nItemID)),
                update);
            return EServerResult.SUCCESS;
        }

		// キーワードを削除する
        public EServerResult deleteKeyword(int nUserID, int nKeywordID)
        {
            var findone =
                mKeywordCollection.FindOne(Query.And(Query.EQ("UserID", nUserID), Query.EQ("KeywordID", nKeywordID)));
            if (findone == null)
            {
                return EServerResult.MissingKeyword;
            }

            mKeywordCollection.Remove(Query.EQ("KeywordID", nKeywordID));
            mKeywordItemCollection.Remove(Query.EQ("KeywordID", nKeywordID));
            return EServerResult.SUCCESS;
        }

		// 時間を設定する
        public EServerResult setItemTimeCreated(int nItemID, DateTime created)
        {
            var findone =
                mItemTime.FindOne(Query.EQ("ItemID", nItemID));
            if (findone == null)
            {
                var itemtime = new DBItemTime();
                itemtime.setInit(nItemID, created, created);
                mItemTime.Save(itemtime);
            }
            return EServerResult.SUCCESS;
        }
		// 時間を設定する
        public EServerResult setItemTimeModified(int nItemID, DateTime modified)
        {
            var findone =
                mItemTime.FindOne(Query.EQ("ItemID", nItemID));
            if (findone == null)
            {
                var itemtime = new DBItemTime();
                itemtime.setInit(nItemID, modified, modified);
                mItemTime.Save(itemtime);
            }
			else {
				var update = Update.Set("LastModified", BsonValue.Create(modified));
				mItemTime.Update(Query.EQ("ItemID", nItemID), update);
            }
            return EServerResult.SUCCESS;
        }
		// 時間を得る
        public AItemTime getItemTime(int nItemID)
        {
            var findone = mItemTime.FindOne(Query.EQ("ItemID", nItemID));
            var ret = new AItemTime(findone.ItemID, findone.Created, findone.LastModified);
            return ret;
        }
    }

    public class DBGodaiSystem
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int MaxUserID { get; set; }
        public int MaxDungeonBlockImageID { get; set; }
        public int MaxObjectID { get; set; }
        public int MaxItemID { get; set; }
        public int MaxItemImageID { get; set; }
		public int MaxKeywordID { get; set; }

        public DBGodaiSystem()
        {
            this.MaxUserID = 1; // 0 はシステム
            this.MaxDungeonBlockImageID = 0;
            this.MaxObjectID = 0;
            this.MaxItemID = 1; // 0 はアイテム無し
            this.MaxItemImageID = 1; // 0はアイテム無し
            this.MaxKeywordID = 1;
        }
    }

    public class DBUser
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public String Mail { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }

        public Bitmap ImageCharacter { get; set; }
    }
    public class DBLogon
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ID { get; set; }
        public DateTime LastLogon { get; set; }
    }
    public class DBDungeon
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public int DungeonNumber { get; set; }
        public ulong[] Dungeon { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public DBDungeon(int nUserID, int nDungeonNumber, ulong[] dungeon, int nSizeX, int nSizeY ) 
        {
            this.UserID = nUserID;
            this.DungeonNumber = nDungeonNumber;
            this.Dungeon = dungeon;
            this.SizeX = nSizeX;
            this.SizeY = nSizeY;
        }
    }
    public class DBDungeonBlockImage
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ImageID { get; set; }

        public Bitmap Image { get; set;  }

        public DateTime CreatedTime { get; set; }
        public int OwnerID { get; set; }
        public bool CanItemImage { get; set;  }
        public String Name { get; set; }

        public void setInit(ImagePair pair)
        {
            this.ImageID = pair.getNumber();
            this.Image = pair.getImage () == null ? null : pair.getImage() is Bitmap ? pair.getImage() as Bitmap : new Bitmap(pair.getImage());
            this.CreatedTime = pair.getCreateTime();
            this.OwnerID = pair.getOwner();
            this.Name = pair.getName();
            this.CanItemImage = pair.canItemImage();
        }
    }

    public class DBTilePalette
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public HashSet<ulong> TileIDSet { get; set; }

        public void setInit(TilePalette palette)
        {
            this.UserID = palette.getUesrID();

            this.TileIDSet.Clear();
            foreach (var nImageID in palette)
            {
                this.TileIDSet.Add(nImageID);
            }
        }

        public DBTilePalette()
        {
            this.TileIDSet = new HashSet<ulong>();
        }
    }

    public class DBObjectAttr
    {
        public MongoDB.Bson.ObjectId _id { get; set; }

        public int ObjectID { get; set; }
        public bool CanWalk { get; set; }
        public int ItemID { get; set; }
        public EObjectCommand Command { get; set; }
        public int CommandSub { get; set; }

        public void setInit(ObjectAttr obj)
        {
            this.ObjectID = obj.getObjectID();
            this.CanWalk = obj.canWalk();
            this.ItemID = obj.getItemID();
            this.Command = obj.getObjectCommand();
            this.CommandSub = obj.getObjectCommandSub();
        }
    }

    public class DBTile
    {
        public MongoDB.Bson.ObjectId _id { get; set; }

        public ulong TileID { get; set; }

        public void setInit(Tile tile_)
        {
            this.TileID = tile_.getTileID();
        }
    }

    public class DBItem
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ItemID { get; set; }
        public String HeaderString { get; set; }

        public Bitmap HeaderImage { get; set; }

        public int ItemImageID { get; set; } // BlockImageIDと共通

        public void setInit(AItem aitem, int nItemImageID) {
            this.HeaderString = aitem.getHeaderString();
            this.HeaderImage = aitem.getHeaderImage() == null ? null :
				aitem.getHeaderImage() is Bitmap
                ? aitem.getHeaderImage() as Bitmap
                : new Bitmap(aitem.getHeaderImage());
            this.ItemImageID = nItemImageID;
        }
    }

	// アイテムの日付関連を記録する
    public class DBItemTime
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ItemID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public void setInit(int nItemID, DateTime created, DateTime lastModified)
        {
            ItemID = nItemID;
            Created = created;
            LastModified = lastModified;
        }
    }

    public class DBLocation
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public int DugeonUserID { get; set; }
        public int Ix { get; set; }
        public int iy { get; set; }
        public int DungeonNumber { get; set; }

        public void setInit(int nUserID, ALocation loc)
        {
            this.UserID = nUserID;
            this.DugeonUserID = loc.getDungeonUserID();
            this.DungeonNumber = loc.getDungeonNumber();
            this.Ix = loc.getIX();
            this.iy = loc.getIY();
        }
    }

    public class DBIslandGround
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public int Ix1 { get; set; }
        public int Iy1 { get; set; }
        public int Ix2 { get; set; }
        public int Iy2 { get; set; }

        public void setInit(IslandGround ground)
        {
            this.UserID = ground.getUserID();
            this.Ix1 = ground.getIx1();
            this.Iy1 = ground.getIy1();
            this.Ix2 = ground.getIx2();
            this.Iy2 = ground.getIy2();
        }
    }

    public class DBMessage
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public String Message { get; set; }

        public void setInit(AMessage mes)
        {
            this.UserID = mes.getUserID();
            this.Message = mes.getMessage();
        }
    }

    public class DBExpValue
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public int Value { get; set; }
        public int Total { get; set; }
        public void setInit(ExpValue ev)
        {
            this.UserID = ev.getUserID();
            this.Value = ev.getExpValue();
            this.Total = ev.getTotalValue();
        }
    }

    public class DBPickedup
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public int ItemID { get; set; }
        public DateTime PickupedTime { get; set; }
    }

    public class DBItemOwner
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ItemID { get; set; }
        public int UserID { get; set; } // オーナー
    }


    public class DBAshiatoLog
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public String Log { get; set; }
    }

    public class DBItemArticle
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ItemID { get; set; }
        public int ArticleID { get; set; }
        public int UserID { get; set; }
        public String Contents { get; set; }
        public DateTime CreateTime { get; set; }

        public void setInit(ItemArticle article)
        {
            this.ItemID = article.getItemID();
            this.ArticleID = article.getArticleID();
            this.UserID = article.getUserID();
            this.Contents = article.getContents();
            this.CreateTime = article.getCreateTime();
        }
    }

    public class DBUnreadArticle
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ItemID { get; set; }
        public int ArticleID { get; set; }
        public int UserID { get; set; }
    }

    public class DBDungeonDepth
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public int Depth { get; set; }
    }

    public class DBMonster
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ItemID { get; set;  }
    }

    public class DBRDReadItem
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ItemID { get; set; }
        public int UserID { get; set; }
    }

    public class DBRSSLastUpdateTime
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int ItemID { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public class DBUserFolder
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        public int UserID { get; set; }
        public String ComputerName { get; set; }
        public String Folder { get; set; }
    }

    public class DBKeywordItem
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
		public int KeywordID { get; set; }
		public int ItemPriority { get; set; }
		public int ItemID { get; set; }
    }

    public class DBKeyword
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
		public int UserID { get; set; }
		public int KeywordID { get; set; }
		public string Keyword { get; set; }
		public int KeywordPriority { get; set; }
    }


#if false
    public class DBItemImage
    {
        public MongoDB.Bson.ObjectId _id { get; set;  }
        public int ItemImageID;
        public Image ItemImage;

        public void setInit(AItemImage itemimage)
        {
            this.ItemImageID = itemimage.getItemImageID();
            this.ItemImage = itemimage.getItemImage();
        }
    }
#endif
}

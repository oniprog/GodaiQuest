using System;
using System.Collections.Generic;
using GodaiLibrary;
using System.Net.Sockets;
using System.Drawing;
using System.IO;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuest
{
    public class GQCommandMaster
    {
        //public static int CLIENT_VERSION = 2012120216;
        public static int CLIENT_VERSION = 2014021618;

        public static int SERVER_PORT = 21014;  // サーバ用のポート
        //public static int SERVER_PORT = 21015;  // サーバ用のポート
        private GodaiLibrary.Network mNetwork;
        private GodaiLibrary.GodaiQuest.EServerResult   mError;

        private int mUserID;
        private object mLock = new object();

        public GQCommandMaster()
        {
            TcpClient client = new TcpClient();
#if DEBUG
            client.Connect("localhost", SERVER_PORT);
#else

//            client.Connect("192.168.48.111", SERVER_PORT);
            client.Connect(System.Configuration.ConfigurationManager.AppSettings["SERVER_IP"], SERVER_PORT);
#endif
            mNetwork = new GodaiLibrary.Network(client.GetStream());

            mNetwork.sendDWORD(1); // Version Number
            mNetwork.flush();

            int nOK = mNetwork.receiveDWORD();
            if (nOK != 1)
            {
                throw new Exception("正常に初期化できませんでした");
            }
        }

        public int getUserID()
        {
            return mUserID;
        }
		
        ///  ユーザーの追加
        public bool addUser(String strMail, String strPassword, String strName, Image imageCharacter, String strUserFolder)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)GodaiLibrary.GodaiQuest.EServerCommand.AddUser);
                mNetwork.sendDWORD(1); // Version

#if true
				godaiquest.AddUser user = new godaiquest.AddUser();
				user.mail_address = strMail;
                user.password = GodaiLibrary.Crypto.CalcPasswordHash(strPassword);
                user.user_name = strName;
                user.user_folder = strUserFolder;
                user.computer_name = getComputerName();
                user.user_image = GodaiLibrary.Network.ImageToByteArray(imageCharacter);
				mNetwork.Serialize( user );
#else
                mNetwork.sendString(strMail);
                mNetwork.sendString(strName);
                mNetwork.sendString(GodaiLibrary.Crypto.calcPasswordHash(strPassword));
                mNetwork.sendString(strUserFolder);
                mNetwork.sendString(getComputerName());

                mNetwork.sendImage(imageCharacter);
#endif
                mNetwork.flush();

                GodaiLibrary.GodaiQuest.EServerResult eResult = (GodaiLibrary.GodaiQuest.EServerResult)mNetwork.receiveDWORD();
                if (eResult == GodaiLibrary.GodaiQuest.EServerResult.SUCCESS)
                    return true;

                mError = eResult;
                return false;
            }
        }


        /// ログオン
        public bool tryLogon(String strMail, String strPassword)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)GodaiLibrary.GodaiQuest.EServerCommand.TryLogon);
                mNetwork.sendDWORD(1); // Version

#if true
                godaiquest.Login login = new godaiquest.Login();
                login.mail_address = strMail;
				login.password = GodaiLibrary.Crypto.CalcPasswordHash(strPassword); 
				login.client_version = (uint)CLIENT_VERSION;
                mNetwork.Serialize(login);
#else
                mNetwork.sendString(strMail);
                mNetwork.sendString(GodaiLibrary.Crypto.calcPasswordHash(strPassword));
                mNetwork.sendDWORD(CLIENT_VERSION);    // クライアントバージョン
#endif

                mNetwork.flush();

                var eResult = (GodaiLibrary.GodaiQuest.EServerResult)mNetwork.receiveDWORD();
                if (eResult != GodaiLibrary.GodaiQuest.EServerResult.SUCCESS)
                {
                    mError = eResult;
                    return false;
                }
                mUserID = mNetwork.receiveDWORD();
                return true;
            }
        }

        /// ダンジョン情報を受け取る
        public bool getDungeon(out GodaiLibrary.GodaiQuest.DungeonInfo dungeon, int nID, int nDungeonNumber)
        {
            lock (mLock)
            {
                dungeon = null;

                mNetwork.sendDWORD((int)GodaiLibrary.GodaiQuest.EServerCommand.GetDungeon);
                mNetwork.sendDWORD(1); // Version
#if true
				var get_dungeon = new godaiquest.GetDungeon();
				get_dungeon.id = nID;
				get_dungeon.dungeon_number = nDungeonNumber;
                mNetwork.Serialize(get_dungeon);
#else
                mNetwork.sendDWORD(nID);
                mNetwork.sendDWORD(nDungeonNumber);
#endif

                mNetwork.flush();

                var eResult = (GodaiLibrary.GodaiQuest.EServerResult)mNetwork.receiveDWORD();
                if (eResult != GodaiLibrary.GodaiQuest.EServerResult.SUCCESS)
                {
                    mError = eResult;
                    return false;
                }

#if true
                dungeon = new DungeonInfo(mNetwork.Deserialize<godaiquest.DungeonInfo>());
#else
                byte[] data = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                dungeon = (GodaiLibrary.GodaiQuest.DungeonInfo)formatter.Deserialize(new MemoryStream(data));
#endif
                return true;
            }
        }

        /// ダンジョン情報をセットする
        public bool setDungeon(int nUserID, DungeonInfo dungeon, DungeonBlockImageInfo images, ObjectAttrInfo objectinfo, TileInfo tileinfo)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetDungeon);

                mNetwork.sendDWORD(1); // Version
#if true

                var set_dungeon = new godaiquest.SetDungeon();

                set_dungeon.user_id = nUserID;
                set_dungeon.dungeon_number = dungeon.getDungeonNumber();
				
                set_dungeon.dungeon_info = dungeon.getSerialize();
                set_dungeon.images = images.getSerialize();
                set_dungeon.object_info = objectinfo.getSerialize();
                set_dungeon.tile_info = tileinfo.getSerialize();
                mNetwork.Serialize(set_dungeon);
#else
                mNetwork.sendDWORD(nUserID);
                mNetwork.sendDWORD(dungeon.getDungeonNumber());

                MemoryStream memoryDungeon = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryDungeon, dungeon);

                mNetwork.sendBinary(memoryDungeon.ToArray());

                MemoryStream memoryImages = new MemoryStream();
                formatter.Serialize(memoryImages, images);
                mNetwork.sendBinary(memoryImages.ToArray());

                MemoryStream memoryObject = new MemoryStream();
                formatter.Serialize(memoryObject, objectinfo);
                mNetwork.sendBinary(memoryObject.ToArray());

                MemoryStream memoryTile = new MemoryStream();
                formatter.Serialize(memoryTile, tileinfo);
                mNetwork.sendBinary(memoryTile.ToArray());
#endif

                mNetwork.flush();

                var eResult = (EServerResult)mNetwork.receiveDWORD();
                if (eResult == EServerResult.SUCCESS)
                    return true;

                mError = eResult;
                return true;
            }
        }

        /// ダンジョンのブロック図を得る
        public bool getDungeonBlockImage(out DungeonBlockImageInfo images)
        {
            lock (mLock)
            {
                images = null;

                mNetwork.sendDWORD((int)EServerCommand.GetDungeonBlockImage);
                mNetwork.sendDWORD(1); // Version
                mNetwork.flush();

                var eResult = (EServerResult)mNetwork.receiveDWORD();
                if (eResult != EServerResult.SUCCESS)
                {
                    mError = eResult;
                    return false;
                }

#if true
                images = new DungeonBlockImageInfo(mNetwork.Deserialize<godaiquest.DungeonBlockImageInfo>());
#else
                byte[] data = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                images = (DungeonBlockImageInfo)formatter.Deserialize(new MemoryStream(data));
#endif
                return true;
            }
        }

        /// ダンジョンのブロック図を設定する
        public bool setDungeonBlockImage(DungeonBlockImageInfo images)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetDungeonBlockImage);

                mNetwork.sendDWORD(1); // Version

#if true
                mNetwork.Serialize(images.getSerialize());
#else
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, images);
                mNetwork.sendBinary(memory.ToArray());
#endif
                mNetwork.flush();

                var eResult = (EServerResult)mNetwork.receiveDWORD();
                if (eResult != EServerResult.SUCCESS)
                {
                    mError = eResult;
                    return false;
                }

                return true;
            }
        }

        // ブロックパレットの読み出し
        public bool getBlockImagePallette(out TilePalette palette, int nUserID)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetTilePalette);

                mNetwork.sendDWORD(1); // Version
                mNetwork.sendDWORD(nUserID);
                mNetwork.flush();

                palette = null;

                EServerResult result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

#if true
                palette = new TilePalette(mNetwork.Deserialize<godaiquest.TilePalette>());
#else
                byte[] data = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                palette = (TilePalette)formatter.Deserialize(new MemoryStream(data));
#endif

                return true;
            }
        }

        // ブロックパレットの設定
        public bool setBlockImagePalette(TilePalette palette, int nUserID)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetTilePalette);
                mNetwork.sendDWORD(1);
#if true
                var set_block_image_palette = new godaiquest.SetBlockImagePalette();
                set_block_image_palette.user_id = nUserID;
                set_block_image_palette.tile_palette = palette.getSerialize();
                mNetwork.Serialize(set_block_image_palette);
#else
                mNetwork.sendDWORD(nUserID);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, palette);
                byte[] data = memory.ToArray();

                mNetwork.sendBinary(data);
#endif
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result == EServerResult.SUCCESS)
                    return true;

                mError = result;
                return false;
            }
        }

        // オブジェクト情報を得る
        public bool getObjectAttrInfo(out ObjectAttrInfo info)
        {
            lock (mLock)
            {
                info = null;

                mNetwork.sendDWORD((int)EServerCommand.GetObjectAttrInfo);
                mNetwork.sendDWORD(1);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

#if true
                info = new ObjectAttrInfo(mNetwork.Deserialize<godaiquest.ObjectAttrInfo>());
#else
                byte[] data = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                info = (ObjectAttrInfo)formatter.Deserialize(new MemoryStream(data));
#endif
                return true;
            }
        }

        // オブジェクトの情報を設定する
        public bool setObjectAttrInfo(ObjectAttrInfo info)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetObjectAttrinfo);

                mNetwork.sendDWORD(1);

#if true
                mNetwork.Serialize(info.getSerialize());
#else
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, info);
                mNetwork.sendBinary(memory.ToArray());
#endif

                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

                return true;
            }
        }

        // タイル情報を得る
        public bool getTitleList(out TileInfo tilelist)
        {
            tilelist = null;
            lock (mLock)
            {

                mNetwork.sendDWORD((int)EServerCommand.GetTileList);
                mNetwork.sendDWORD(1);

                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

#if true
                tilelist = new TileInfo(mNetwork.Deserialize<godaiquest.TileInfo>());
#else
                byte[] data = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                tilelist = (TileInfo)formatter.Deserialize(new MemoryStream(data));
#endif

                return true;
            }
        }

        // ユーザー情報を得る
        public bool getUserInfo(out UserInfo userinfo)
        {
            lock (mLock)
            {
                userinfo = null;

                mNetwork.sendDWORD((int)EServerCommand.GetUserInfo);
                mNetwork.sendDWORD(1);

                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

#if true
                userinfo = new UserInfo(mNetwork.Deserialize<godaiquest.UserInfo>());
#else
                byte[] data = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                userinfo = (UserInfo)formatter.Deserialize(new MemoryStream(data));
#endif

                return true;
            }
        }

        // アイテム情報をセットする
//        public bool setAItem(ref AItem item, ImagePair itemimage, String strDataFolder )
        public bool setAItem(ref AItem item, ImagePair itemimage, HashSet<FilePair> setFile )
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetAItem);
                mNetwork.sendDWORD(1);

                mNetwork.flush();

#if true
                mNetwork.Serialize(item.getSerialize());
#else
                MemoryStream memoryItem = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryItem, item);
                mNetwork.sendBinary(memoryItem.ToArray());
#endif

#if true
                mNetwork.Serialize(itemimage.getSerialize());
#else
                MemoryStream memoryItemImage = new MemoryStream();
                formatter.Serialize(memoryItemImage, itemimage);
                mNetwork.sendBinary(memoryItemImage.ToArray());
#endif

                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

                // ファイルを全部転送する
                foreach (var fileinfo in setFile)
                {
                    Network.sendFile(mNetwork, Path.GetFileName(fileinfo.FullPath), fileinfo.FullPath, Path.GetDirectoryName(fileinfo.HalfPath));
                }
                mNetwork.sendByte(0);

#if true
                item = new AItem(mNetwork.Deserialize<godaiquest.AItem>());
#else
                byte[] dataItem = mNetwork.receiveBinary();
                item = (AItem)formatter.Deserialize(new MemoryStream(dataItem));
#endif

                return true;
            }
        }

        // アイテム情報ファイルを得る
        public bool getAItem(int nItemID, String strDirectory)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetAItem);
                mNetwork.sendDWORD(1);

                mNetwork.sendDWORD(nItemID);

                mNetwork.flush();

                Network.receiveFiles(mNetwork, strDirectory);

                return true;
            }
        }

        // アイテム情報一覧を得る
        public bool getItemInfo(out ItemInfo iteminfo)
        {
            lock (mLock)
            {
                iteminfo = new ItemInfo();

                mNetwork.sendDWORD((int)EServerCommand.GetItemInfo);
                mNetwork.sendDWORD(1);

                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

#if true
                iteminfo = new ItemInfo(mNetwork.Deserialize<godaiquest.ItemInfo>());
#else
                byte[] dataItemInfo = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                iteminfo = (ItemInfo)formatter.Deserialize(new MemoryStream(dataItemInfo));
#endif
                return true;
            }
        }

        // ユーザ情報を設定する
        public bool setAUser(AUser user)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetAUser);
                mNetwork.sendDWORD(1);
#if true
                mNetwork.Serialize(user.getSerialize());		
#else
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, user);
                mNetwork.sendBinary(memory.ToArray());
#endif

                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // アイテム情報を変更する
        public bool changeAItem(AItem item)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.ChangeAItem);
                mNetwork.sendDWORD(1);

#if true
                mNetwork.Serialize(item.getSerialize());
#else
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, item);
                mNetwork.sendBinary(memory.ToArray());
#endif
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // アイテムファイル情報を変更する
        public bool uploadAItemFiles(int nItemID, HashSet<FilePair> setFiles, bool bDeleteMoto)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.UploadItemFiles);
                mNetwork.sendDWORD(1);

                mNetwork.sendDWORD(nItemID);
                mNetwork.sendDWORD(bDeleteMoto ? 1 : 0);

                // ファイルを全部転送する
                foreach (var fileinfo in setFiles)
                {
                    Network.sendFile(mNetwork, Path.GetFileName(fileinfo.FullPath), fileinfo.FullPath, Path.GetDirectoryName(fileinfo.HalfPath));
                }
                mNetwork.sendByte(0);

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

                return true;
            }
        }

        // ポーリング
        public bool polling(out SignalQueue signalqueue, out LocationInfo locInfo, out Dictionary<int,int> listLoginUser, out RealMonsterLocationInfo realMonsterLocationInfo, ALocation locSelf)
        {
            lock (mLock)
            {
                listLoginUser = new Dictionary<int, int>();

                mNetwork.sendDWORD((int)EServerCommand.Polling);
                mNetwork.sendDWORD(1);

                mNetwork.Serialize(locSelf.getSerialize());
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    locInfo = new LocationInfo();
                    signalqueue = new SignalQueue();
					realMonsterLocationInfo = new RealMonsterLocationInfo();

                    mError = result;
                    return false;
                }

                // ユーザ位置情報を得る
				locInfo = new LocationInfo( mNetwork.Deserialize<godaiquest.LocationInfo>());

                // ログインユーザ情報を得る
                {
                    int nLogonUserCount = (int)mNetwork.receiveLength();
                    for (int iu = 0; iu < nLogonUserCount; ++iu)
                    {
                        int nUserID = mNetwork.receiveDWORD();
                        listLoginUser.Add(nUserID, nUserID);
                    }
                }

				// 外部モンスターの位置を得る
                realMonsterLocationInfo = new RealMonsterLocationInfo(mNetwork.Deserialize<godaiquest.RealMonsterLocationInfo>());

                // シグナルを得る
				signalqueue = new SignalQueue( mNetwork.Deserialize<godaiquest.SignalQueue>());

                return true;
            }
        }

        // 大陸の土地専有情報を得る
        public bool getIslandGroundInfo(out IslandGroundInfo groundinfo)
        {

            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetIslandGroundInfo);
                mNetwork.sendDWORD(1);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    groundinfo = new IslandGroundInfo();
                    mError = result;
                    return false;
                }

#if true
                groundinfo = new IslandGroundInfo(mNetwork.Deserialize<godaiquest.IslandGroundInfo>());
#else
                byte[] dataGroundInfo = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                groundinfo = (IslandGroundInfo)formatter.Deserialize(new MemoryStream(dataGroundInfo));
#endif

                return true;
            }
        }

        // オブジェクトの内容変更をする
        public bool changeObjectAttr(ObjectAttr obj)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.ChangeObjectAttr);
                mNetwork.sendDWORD(1);

#if true
                mNetwork.Serialize(obj.getSerialize());
#else
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, obj);
                mNetwork.sendBinary(memory.ToArray());
#endif
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // イメージの変更をする
        public bool changeDungeonBlockImagePair(ImagePair imagepair)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.ChangeDungeonBlockImagePair);
                mNetwork.sendDWORD(1);

#if true
				mNetwork.Serialize(imagepair.getSerialize());
#else
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, imagepair);
                mNetwork.sendBinary(memory.ToArray());
#endif
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // 位置情報を得る
        public bool getLocationInfo(out LocationInfo locinfo)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetLocationInfo);
                mNetwork.sendDWORD(1);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    locinfo = new LocationInfo();
                    mError = result;
                    return false;
                }

#if true
                locinfo = new LocationInfo(mNetwork.Deserialize<godaiquest.LocationInfo>());
#else
                byte[] dataLocationInfo = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                locinfo = (LocationInfo)formatter.Deserialize(new MemoryStream(dataLocationInfo));
#endif

                return true;
            }
        }

        // メッセージ情報を得る
        public bool getMessageInfo(out MessageInfo mesinfo)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetMessageInfo);
                mNetwork.sendDWORD(1);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mesinfo = new MessageInfo();
                    mError = result;
                    return false;
                }

#if true
				mesinfo = new MessageInfo( mNetwork.Deserialize<godaiquest.MessageInfo>());
#else
                byte[] dataMesInfo = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                mesinfo = (MessageInfo)formatter.Deserialize(new MemoryStream(dataMesInfo));
#endif

                return true;
            }
        }

        // メッセージ情報を設定する
        public bool setAMessage(AMessage mes)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetAMessage);
                mNetwork.sendDWORD(1);

#if true
                mNetwork.Serialize(mes.getSerialize());
#else
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, mes);

                mNetwork.sendBinary(memory.ToArray());
#endif

                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

                return true;
            }
        }

        // 経験値を得る
        public ExpValue getExpValue()
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetExpValue);
                mNetwork.sendDWORD(1);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return new ExpValue(mUserID, 0, 0);
                }

                int nExpValue = mNetwork.receiveDWORD();
                int nTotalValue = mNetwork.receiveDWORD();
                return new ExpValue(mUserID, nExpValue, nTotalValue);
            }
        }

        /// まだピックアップしてないアイテムの情報を得る
        public bool getUnpickedupItemInfo(out UnpickedupInfo info, int nDungeonID )
        {
            lock (mLock)
            {
                info = new UnpickedupInfo();
                mNetwork.sendDWORD((int)EServerCommand.GetUnpickedupItemInfo);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(mUserID);
                mNetwork.sendDWORD(nDungeonID);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

#if true
                int nLen = (int)mNetwork.receiveLength();
                HashSet<int> setItemID = new HashSet<int>();
                for (int it = 0; it < nLen; ++it)
                {
                    setItemID.Add(mNetwork.receiveDWORD());
                }
#else
                byte[] dataItemID = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                HashSet<int> setItemID = (HashSet<int>)formatter.Deserialize(new MemoryStream(dataItemID));
#endif

                    foreach (var nItemID in setItemID)
                    {
                        info.addItemID(nItemID);
                    }

                return true;
            }
        }

        /// 足あとを得る
        public bool getAshiato(out PickupedInfo pickinfo, int nItemID) {

            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetAshiato);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nItemID);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    pickinfo = new PickupedInfo();
                    mError = result;
                    return false;
                }

#if true
                pickinfo = new PickupedInfo(mNetwork.Deserialize<godaiquest.PickupedInfo>());
#else
                byte[] dataAshiato = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                pickinfo = (PickupedInfo)formatter.Deserialize(new MemoryStream(dataAshiato));
#endif

                return true;
            }
        }

        /// 足あとログを得る
        public bool getAshiatolog(out List<string> listLog)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetAshiatoLog);
                mNetwork.sendDWORD(1);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    listLog = new List<string>();
                    mError = result;
                    return false;
                }

#if true
                listLog = new List<string>();
                var logtmp = mNetwork.Deserialize<godaiquest.Ashiatolog>();
				foreach (var alog in logtmp.alog)
				{
                    listLog.Add(alog);
				}
#else
                byte[] dataLog = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                listLog = (List<String>)formatter.Deserialize(new MemoryStream(dataLog));
#endif

                return true;
            }
        }

        /// アイテムに付随するメッセージを得る
        public String getAritcleString(int nItemID)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetArticleString);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nItemID);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return "";
                }

                String strRet = mNetwork.receiveString();
                return strRet;
            }
        }

        /// アーティクルを書き込む
        public bool setItemArticle(ItemArticle article)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetItemArticle);
                mNetwork.sendDWORD(1);

#if true
                mNetwork.Serialize(article.getSerialize());
#else
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, article);

                mNetwork.sendBinary(memory.ToArray());
#endif
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // アーティクルを読み込む
        public bool readArticle(int nItemID, int nUserID)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.ReadArticle);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nItemID);
                mNetwork.sendDWORD(nUserID);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }

                return true;
            }
        }

        // アーティクルを削除する
        public bool deleteLastItemArticle(int nItemID)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.DeleteLastItemArticle);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nItemID);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // ダンジョンの深さを得る
        public int getDungeonDepth(int nDungeonID)
        {
            mNetwork.sendDWORD((int)EServerCommand.GetDungeonDepth);
            lock (mLock)
            {
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nDungeonID);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return 0;
                }
                return mNetwork.receiveDWORD();
            }
        }

        // 経験値を使う
        public bool useExperience(EUseExp eUseExpType, int nDungeonNumber)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.UseExperience);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD((int)eUseExpType);
                mNetwork.sendDWORD(nDungeonNumber);
                mNetwork.flush();

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // モンスタかの一覧を得る
        public bool getMonsterInfo(out MonsterInfo monsterinfo, int nDungeonID)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetMonsterInfo);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nDungeonID);

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    monsterinfo = new MonsterInfo();
                    mError = result;
                    return false;
                }

#if true
                monsterinfo = new MonsterInfo(mNetwork.Deserialize<godaiquest.MonsterInfo>());
#else
                byte[] dataMonsterInfo = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                monsterinfo = (MonsterInfo)formatter.Deserialize(new MemoryStream(dataMonsterInfo));
#endif

                return true;
            }
        }

        // モンスタかを設定する
        public bool setMonster(int nItemID, bool bMonster)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetMonster);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nItemID);
                mNetwork.sendDWORD(bMonster ? 1 : 0);

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // ランダムダンジョンの読み込み済み一覧を得る
        public bool getRDReadItemInfo(out RDReadItemInfo readiteminfo)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetRDReadItemInfo);
                mNetwork.sendDWORD(1);

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    readiteminfo = new RDReadItemInfo(mUserID);
                    mError = result;
                    return false;
                }

#if true
                readiteminfo = new RDReadItemInfo(mNetwork.Deserialize<godaiquest.RDReadItemInfo>());
#else
                byte[] dataReadItemInfo = mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                readiteminfo = (RDReadItemInfo)formatter.Deserialize(new MemoryStream(dataReadItemInfo));
#endif

                return true;
            }
        }

        // ランダムダンジョンのアイテムを捕まえた
        public bool setRDReadItem(int nItemID)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetRDReadItem);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nItemID);
                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // パスワード変更
        public bool changePassword(String strNewPasswordHash)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.ChangePassword);
                mNetwork.sendDWORD(1);
                mNetwork.sendString(strNewPasswordHash);
                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        //
        private String getComputerName()
        {
            return System.Environment.MachineName;
        }

        // ユーザフォルダを設定する
        public bool setUserFolder(int nUserID, String strFolder)
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.SetUserFolder);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nUserID);
                mNetwork.sendString(getComputerName());
                mNetwork.sendString(strFolder);

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return false;
                }
                return true;
            }
        }

        // ユーザフォルダを得る
        public String getUserFolder(int nUserID )
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetUserFolder);
                mNetwork.sendDWORD(1);
                mNetwork.sendDWORD(nUserID);
                mNetwork.sendString(getComputerName());

                var result = (EServerResult)mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mError = result;
                    return null;
                }

                String strFolder = mNetwork.receiveString();
                return strFolder;
            }
        }

		// モンスターの元情報を得る
        public bool getRealMonsterSrcInfo(out RealMonsterInfo info ) 
        {
            lock (mLock)
            {
                mNetwork.sendDWORD((int)EServerCommand.GetRealMonsterSrcInfo);
                mNetwork.sendDWORD(1);

                var result = (EServerResult) mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
					info = new RealMonsterInfo();
                    mError = result;
                    return false;
                }

                info = new RealMonsterInfo(mNetwork.Deserialize<godaiquest.RealMonsterInfo>());
                return true;
            }
        }

        ///  エラーメッセージに直す
        public String getErrorReasonString()
        {
            lock (mLock)
            {
                if (mError == EServerResult.UnknownError)
                {
                    return "何らかのエラーが発生しました";
                }
                else if (mError == GodaiLibrary.GodaiQuest.EServerResult.UserAlreadyExist)
                {
                    return "同一メールアドレスのユーザが既に存在します";
                }
                else if (mError == GodaiLibrary.GodaiQuest.EServerResult.PasswordWrong)
                {
                    return "パスワードが間違っています";
                }
                else if (mError == GodaiLibrary.GodaiQuest.EServerResult.MissingUser)
                {
                    return "ユーザが存在しません";
                }
                else if (mError == EServerResult.RequireLogon)
                {
                    return "ログインしていません。ログインする必要があります。";
                }
                else if (mError == EServerResult.ClientObsolete)
                {
                    return "新しいバージョンのソフトをインストールしてください。";
                }
                else if (mError == EServerResult.MissingItem)
                {
                    return "対応するアイテムが存在しません(内部エラー)";
                }
                else if (mError == EServerResult.AlreadyLogin)
                {
                    return "すでにログイン済みです";
                }
                else if (mError == EServerResult.MissingObject)
                {
                    return "オブジェクトが見つかりません(内部エラー)";
                }
                else if (mError == EServerResult.MissingDugeonBlockImage)
                {
                    return "タイルイメージがありません";
                }
                else if (mError == EServerResult.NotYourArticle)
                {
                    return "あなたの書き込みがありません";
                }
                else if (mError == EServerResult.NotExistDungeon)
                {
                    return "作成していないダンジョンです";
                }
                else if (mError == EServerResult.NotEnoughExp)
                {
                    return "経験値が足りません";
                }
                else if (mError == EServerResult.ClientNewer)
                {
                    return "クライアントが新しすぎます";
                }
                else
                    return "";
            }
        }
    }
}

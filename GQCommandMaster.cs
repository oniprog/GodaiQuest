using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GodaiLibrary;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuest
{
    public class GQCommandMaster
    {
        public static int CLIENT_VERSION = 2012120216;

        public static int SERVER_PORT = 21014;  // サーバ用のポート
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
            this.mNetwork = new GodaiLibrary.Network(client.GetStream());

            this.mNetwork.sendDWORD(1); // Version Number
            this.mNetwork.flush();

            int nOK = this.mNetwork.receiveDWORD();
            if (nOK != 1)
            {
                throw new Exception("正常に初期化できませんでした");
            }
        }

        public int getUserID()
        {
            return this.mUserID;
        }

        ///  ユーザーの追加
        public bool addUser(String strMail, String strPassword, String strName, Image imageCharacter, String strUserFolder)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)GodaiLibrary.GodaiQuest.EServerCommand.AddUser);

                this.mNetwork.sendDWORD(1); // Version
                this.mNetwork.sendString(strMail);
                this.mNetwork.sendString(strName);
                this.mNetwork.sendString(GodaiLibrary.Crypto.calcPasswordHash(strPassword));
                this.mNetwork.sendString(strUserFolder);
                this.mNetwork.sendString(getComputerName());

                this.mNetwork.sendImage(imageCharacter);

                this.mNetwork.flush();

                GodaiLibrary.GodaiQuest.EServerResult eResult = (GodaiLibrary.GodaiQuest.EServerResult)this.mNetwork.receiveDWORD();
                if (eResult == GodaiLibrary.GodaiQuest.EServerResult.SUCCESS)
                    return true;

                this.mError = eResult;
                return false;
            }
        }


        /// ログオン
        public bool tryLogon(String strMail, String strPassword)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)GodaiLibrary.GodaiQuest.EServerCommand.TryLogon);

                this.mNetwork.sendDWORD(1); // Version
                this.mNetwork.sendString(strMail);
                this.mNetwork.sendString(GodaiLibrary.Crypto.calcPasswordHash(strPassword));
                this.mNetwork.sendDWORD(CLIENT_VERSION);    // クライアントバージョン

                this.mNetwork.flush();

                var eResult = (GodaiLibrary.GodaiQuest.EServerResult)this.mNetwork.receiveDWORD();
                if (eResult != GodaiLibrary.GodaiQuest.EServerResult.SUCCESS)
                {
                    this.mError = eResult;
                    return false;
                }
                this.mUserID = this.mNetwork.receiveDWORD();
                return true;
            }
        }

        /// ダンジョン情報を受け取る
        public bool getDungeon(out GodaiLibrary.GodaiQuest.DungeonInfo dungeon, int nID, int nDungeonNumber)
        {
            lock (this.mLock)
            {
                dungeon = null;

                this.mNetwork.sendDWORD((int)GodaiLibrary.GodaiQuest.EServerCommand.GetDungeon);

                this.mNetwork.sendDWORD(1); // Version
                this.mNetwork.sendDWORD(nID);
                this.mNetwork.sendDWORD(nDungeonNumber);

                this.mNetwork.flush();

                var eResult = (GodaiLibrary.GodaiQuest.EServerResult)this.mNetwork.receiveDWORD();
                if (eResult != GodaiLibrary.GodaiQuest.EServerResult.SUCCESS)
                {
                    this.mError = eResult;
                    return false;
                }

                byte[] data = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                dungeon = (GodaiLibrary.GodaiQuest.DungeonInfo)formatter.Deserialize(new MemoryStream(data));

                return true;
            }
        }

        /// ダンジョン情報をセットする
        public bool setDungeon(int nUserID, DungeonInfo dungeon, DungeonBlockImageInfo images, ObjectAttrInfo objectinfo, TileInfo tileinfo)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetDungeon);

                this.mNetwork.sendDWORD(1); // Version
                this.mNetwork.sendDWORD(nUserID);
                this.mNetwork.sendDWORD(dungeon.getDungeonNumber());

                MemoryStream memoryDungeon = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryDungeon, dungeon);

                this.mNetwork.sendBinary(memoryDungeon.ToArray());

                MemoryStream memoryImages = new MemoryStream();
                formatter.Serialize(memoryImages, images);
                this.mNetwork.sendBinary(memoryImages.ToArray());

                MemoryStream memoryObject = new MemoryStream();
                formatter.Serialize(memoryObject, objectinfo);
                this.mNetwork.sendBinary(memoryObject.ToArray());

                MemoryStream memoryTile = new MemoryStream();
                formatter.Serialize(memoryTile, tileinfo);
                this.mNetwork.sendBinary(memoryTile.ToArray());

                this.mNetwork.flush();

                var eResult = (EServerResult)this.mNetwork.receiveDWORD();
                if (eResult == EServerResult.SUCCESS)
                    return true;

                this.mError = eResult;
                return true;
            }
        }

        /// ダンジョンのブロック図を得る
        public bool getDungeonBlockImage(out DungeonBlockImageInfo images)
        {
            lock (this.mLock)
            {
                images = null;

                this.mNetwork.sendDWORD((int)EServerCommand.GetDungeonBlockImage);
                this.mNetwork.sendDWORD(1); // Version
                this.mNetwork.flush();

                var eResult = (EServerResult)this.mNetwork.receiveDWORD();
                if (eResult != EServerResult.SUCCESS)
                {
                    this.mError = eResult;
                    return false;
                }

                byte[] data = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                images = (DungeonBlockImageInfo)formatter.Deserialize(new MemoryStream(data));

                return true;
            }
        }

        /// ダンジョンのブロック図を設定する
        public bool setDungeonBlockImage(DungeonBlockImageInfo images)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetDungeonBlockImage);

                this.mNetwork.sendDWORD(1); // Version

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, images);
                this.mNetwork.sendBinary(memory.ToArray());
                this.mNetwork.flush();

                var eResult = (EServerResult)this.mNetwork.receiveDWORD();
                if (eResult != EServerResult.SUCCESS)
                {
                    this.mError = eResult;
                    return false;
                }

                return true;
            }
        }

        // ブロックパレットの読み出し
        public bool getBlockImagePallette(out TilePalette palette, int nUserID)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetTilePalette);

                this.mNetwork.sendDWORD(1); // Version
                this.mNetwork.sendDWORD(nUserID);

                this.mNetwork.flush();

                palette = null;

                EServerResult result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                byte[] data = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                palette = (TilePalette)formatter.Deserialize(new MemoryStream(data));

                return true;
            }
        }

        // ブロックパレットの設定
        public bool setBlockImagePalette(TilePalette palette, int nUserID)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetTilePalette);

                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nUserID);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, palette);
                byte[] data = memory.ToArray();

                this.mNetwork.sendBinary(data);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result == EServerResult.SUCCESS)
                    return true;

                this.mError = result;
                return false;
            }
        }

        // オブジェクト情報を得る
        public bool getObjectAttrInfo(out ObjectAttrInfo info)
        {
            lock (this.mLock)
            {
                info = null;

                this.mNetwork.sendDWORD((int)EServerCommand.GetObjectAttrInfo);

                this.mNetwork.sendDWORD(1);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                byte[] data = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                info = (ObjectAttrInfo)formatter.Deserialize(new MemoryStream(data));

                return true;
            }
        }

        // オブジェクトの情報を設定する
        public bool setObjectAttrInfo(ObjectAttrInfo info)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetObjectAttrinfo);

                this.mNetwork.sendDWORD(1);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, info);
                this.mNetwork.sendBinary(memory.ToArray());

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                return true;
            }
        }

        // タイル情報を得る
        public bool getTitleList(out TileInfo tilelist)
        {
            tilelist = null;
            lock (this.mLock)
            {

                this.mNetwork.sendDWORD((int)EServerCommand.GetTileList);
                this.mNetwork.sendDWORD(1);

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                byte[] data = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                tilelist = (TileInfo)formatter.Deserialize(new MemoryStream(data));

                return true;
            }
        }

        // ユーザー情報を得る
        public bool getUserInfo(out UserInfo userinfo)
        {
            lock (this.mLock)
            {
                userinfo = null;

                this.mNetwork.sendDWORD((int)EServerCommand.GetUserInfo);
                this.mNetwork.sendDWORD(1);

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                byte[] data = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                userinfo = (UserInfo)formatter.Deserialize(new MemoryStream(data));

                return true;
            }
        }

        // アイテム情報をセットする
//        public bool setAItem(ref AItem item, ImagePair itemimage, String strDataFolder )
        public bool setAItem(ref AItem item, ImagePair itemimage, HashSet<FilePair> setFile )
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetAItem);
                this.mNetwork.sendDWORD(1);

                this.mNetwork.flush();

                MemoryStream memoryItem = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryItem, item);
                this.mNetwork.sendBinary(memoryItem.ToArray());

                MemoryStream memoryItemImage = new MemoryStream();
                formatter.Serialize(memoryItemImage, itemimage);
                this.mNetwork.sendBinary(memoryItemImage.ToArray());

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

#if true
                //            Network.sendFiles(this.mNetwork, strDataFolder);
                // ファイルを全部転送する
                foreach (var fileinfo in setFile)
                {
                    Network.sendFile(this.mNetwork, Path.GetFileName(fileinfo.FullPath), fileinfo.FullPath, Path.GetDirectoryName(fileinfo.HalfPath));
                }
                this.mNetwork.sendByte(0);
#endif

                byte[] dataItem = this.mNetwork.receiveBinary();
                item = (AItem)formatter.Deserialize(new MemoryStream(dataItem));

                return true;
            }
        }

        // アイテム情報ファイルを得る
        public bool getAItem(int nItemID, String strDirectory)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetAItem);
                this.mNetwork.sendDWORD(1);

                this.mNetwork.sendDWORD(nItemID);

                this.mNetwork.flush();

                Network.receiveFiles(this.mNetwork, strDirectory);

                return true;
            }
        }

        // アイテム情報一覧を得る
        public bool getItemInfo(out ItemInfo iteminfo)
        {
            lock (this.mLock)
            {
                iteminfo = new ItemInfo();

                this.mNetwork.sendDWORD((int)EServerCommand.GetItemInfo);
                this.mNetwork.sendDWORD(1);

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                byte[] dataItemInfo = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                iteminfo = (ItemInfo)formatter.Deserialize(new MemoryStream(dataItemInfo));
                return true;
            }
        }

        // ユーザ情報を設定する
        public bool setAUser(AUser user)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetAUser);
                this.mNetwork.sendDWORD(1);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, user);
                this.mNetwork.sendBinary(memory.ToArray());

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // アイテム情報を変更する
        public bool changeAItem(AItem item)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.ChangeAItem);
                this.mNetwork.sendDWORD(1);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, item);

                this.mNetwork.sendBinary(memory.ToArray());

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // アイテムファイル情報を変更する
        public bool uploadAItemFiles(int nItemID, HashSet<FilePair> setFiles, bool bDeleteMoto)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.UploadItemFiles);
                this.mNetwork.sendDWORD(1);

                this.mNetwork.sendDWORD(nItemID);
                this.mNetwork.sendDWORD(bDeleteMoto ? 1 : 0);

                // ファイルを全部転送する
                foreach (var fileinfo in setFiles)
                {
                    Network.sendFile(this.mNetwork, Path.GetFileName(fileinfo.FullPath), fileinfo.FullPath, Path.GetDirectoryName(fileinfo.HalfPath));
                }
                this.mNetwork.sendByte(0);

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                return true;
            }
        }

        // ポーリング
        public bool polling(out SignalQueue signalqueue, out LocationInfo locInfo, out Dictionary<int,int> listLoginUser, ALocation locSelf)
        {
            lock (this.mLock)
            {
                listLoginUser = new Dictionary<int, int>();

                this.mNetwork.sendDWORD((int)EServerCommand.Polling);
                this.mNetwork.sendDWORD(1);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, locSelf);
                this.mNetwork.sendBinary(memory.ToArray());

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    locInfo = new LocationInfo();
                    signalqueue = new SignalQueue();

                    this.mError = result;
                    return false;
                }

                // ユーザ位置情報を得る
                byte[] dataLocationInfo = this.mNetwork.receiveBinary();
                locInfo = (LocationInfo)formatter.Deserialize(new MemoryStream(dataLocationInfo));

                // ログインユーザ情報を得る
                {
                    int nLogonUserCount = (int)this.mNetwork.receiveLength();
                    for (int iu = 0; iu < nLogonUserCount; ++iu)
                    {
                        int nUserID = this.mNetwork.receiveDWORD();
                        listLoginUser.Add(nUserID, nUserID);
                    }
                }

                // シグナルを得る
                byte[] dataSignal = this.mNetwork.receiveBinary();
                signalqueue = (SignalQueue)formatter.Deserialize(new MemoryStream(dataSignal));

                return true;
            }
        }

        // 大陸の土地専有情報を得る
        public bool getIslandGroundInfo(out IslandGroundInfo groundinfo)
        {

            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetIslandGroundInfo);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    groundinfo = new IslandGroundInfo();
                    this.mError = result;
                    return false;
                }

                byte[] dataGroundInfo = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                groundinfo = (IslandGroundInfo)formatter.Deserialize(new MemoryStream(dataGroundInfo));

                return true;
            }
        }

        // オブジェクトの内容変更をする
        public bool changeObjectAttr(ObjectAttr obj)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.ChangeObjectAttr);
                this.mNetwork.sendDWORD(1);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, obj);
                this.mNetwork.sendBinary(memory.ToArray());
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // イメージの変更をする
        public bool changeDungeonBlockImagePair(ImagePair imagepair)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.ChangeDungeonBlockImagePair);
                this.mNetwork.sendDWORD(1);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, imagepair);
                this.mNetwork.sendBinary(memory.ToArray());
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // 位置情報を得る
        public bool getLocationInfo(out LocationInfo locinfo)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetLocationInfo);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    locinfo = new LocationInfo();
                    this.mError = result;
                    return false;
                }

                byte[] dataLocationInfo = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                locinfo = (LocationInfo)formatter.Deserialize(new MemoryStream(dataLocationInfo));

                return true;
            }
        }

        // メッセージ情報を得る
        public bool getMessageInfo(out MessageInfo mesinfo)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetMessageInfo);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    mesinfo = new MessageInfo();
                    this.mError = result;
                    return false;
                }

                byte[] dataMesInfo = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                mesinfo = (MessageInfo)formatter.Deserialize(new MemoryStream(dataMesInfo));

                return true;
            }
        }

        // メッセージ情報を設定する
        public bool setAMessage(AMessage mes)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetAMessage);
                this.mNetwork.sendDWORD(1);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, mes);

                this.mNetwork.sendBinary(memory.ToArray());

                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                return true;
            }
        }

        // 経験値を得る
        public ExpValue getExpValue()
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetExpValue);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return new ExpValue(this.mUserID, 0, 0);
                }

                int nExpValue = this.mNetwork.receiveDWORD();
                int nTotalValue = this.mNetwork.receiveDWORD();
                return new ExpValue(this.mUserID, nExpValue, nTotalValue);
            }
        }

        /// まだピックアップしてないアイテムの情報を得る
        public bool getUnpickedupItemInfo(out UnpickedupInfo info, int nDungeonID )
        {
            lock (this.mLock)
            {
                info = new UnpickedupInfo();
                this.mNetwork.sendDWORD((int)EServerCommand.GetUnpickedupItemInfo);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(this.mUserID);
                this.mNetwork.sendDWORD(nDungeonID);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                byte[] dataItemID = this.mNetwork.receiveBinary();

                BinaryFormatter formatter = new BinaryFormatter();
                HashSet<int> setItemID = (HashSet<int>)formatter.Deserialize(new MemoryStream(dataItemID));

                foreach (var nItemID in setItemID)
                {
                    info.addItemID(nItemID);
                }

                return true;
            }
        }

        /// 足あとを得る
        public bool getAshiato(out PickupedInfo pickinfo, int nItemID) {

            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetAshiato);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nItemID);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    pickinfo = new PickupedInfo();
                    this.mError = result;
                    return false;
                }

                byte[] dataAshiato = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                pickinfo = (PickupedInfo)formatter.Deserialize(new MemoryStream(dataAshiato));

                return true;
            }
        }

        /// 足あとログを得る
        public bool getAshiatolog(out List<string> listLog)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetAshiatoLog);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    listLog = new List<string>();
                    this.mError = result;
                    return false;
                }

                byte[] dataLog = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                listLog = (List<String>)formatter.Deserialize(new MemoryStream(dataLog));

                return true;
            }
        }

        /// アイテムに付随するメッセージを得る
        public String getAritcleString(int nItemID)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetArticleString);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nItemID);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return "";
                }

                String strRet = this.mNetwork.receiveString();
                return strRet;
            }
        }

        /// アーティクルを書き込む
        public bool setItemArticle(ItemArticle article)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetItemArticle);
                this.mNetwork.sendDWORD(1);

                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory, article);

                this.mNetwork.sendBinary(memory.ToArray());
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // アーティクルを読み込む
        public bool readArticle(int nItemID, int nUserID)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.ReadArticle);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nItemID);
                this.mNetwork.sendDWORD(nUserID);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }

                return true;
            }
        }

        // アーティクルを削除する
        public bool deleteLastItemArticle(int nItemID)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.DeleteLastItemArticle);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nItemID);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // ダンジョンの深さを得る
        public int getDungeonDepth(int nDungeonID)
        {
            this.mNetwork.sendDWORD((int)EServerCommand.GetDungeonDepth);
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nDungeonID);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return 0;
                }
                return this.mNetwork.receiveDWORD();
            }
        }

        // 経験値を使う
        public bool useExperience(EUseExp eUseExpType, int nDungeonNumber)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.UseExperience);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD((int)eUseExpType);
                this.mNetwork.sendDWORD(nDungeonNumber);
                this.mNetwork.flush();

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // モンスタかの一覧を得る
        public bool getMonsterInfo(out MonsterInfo monsterinfo, int nDungeonID)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetMonsterInfo);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nDungeonID);

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    monsterinfo = new MonsterInfo();
                    this.mError = result;
                    return false;
                }

                byte[] dataMonsterInfo = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                monsterinfo = (MonsterInfo)formatter.Deserialize(new MemoryStream(dataMonsterInfo));

                return true;
            }
        }

        // モンスタかを設定する
        public bool setMonster(int nItemID, bool bMonster)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetMonster);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nItemID);
                this.mNetwork.sendDWORD(bMonster ? 1 : 0);

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // ランダムダンジョンの読み込み済み一覧を得る
        public bool getRDReadItemInfo(out RDReadItemInfo readiteminfo)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetRDReadItemInfo);
                this.mNetwork.sendDWORD(1);

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    readiteminfo = new RDReadItemInfo(this.mUserID);
                    this.mError = result;
                    return false;
                }

                byte[] dataReadItemInfo = this.mNetwork.receiveBinary();
                BinaryFormatter formatter = new BinaryFormatter();
                readiteminfo = (RDReadItemInfo)formatter.Deserialize(new MemoryStream(dataReadItemInfo));

                return true;
            }
        }

        // ランダムダンジョンのアイテムを捕まえた
        public bool setRDReadItem(int nItemID)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetRDReadItem);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nItemID);
                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // パスワード変更
        public bool changePassword(String strNewPasswordHash)
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.ChangePassword);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendString(strNewPasswordHash);
                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
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
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.SetUserFolder);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nUserID);
                this.mNetwork.sendString(getComputerName());
                this.mNetwork.sendString(strFolder);

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return false;
                }
                return true;
            }
        }

        // ユーザフォルダを得る
        public String getUserFolder(int nUserID )
        {
            lock (this.mLock)
            {
                this.mNetwork.sendDWORD((int)EServerCommand.GetUserFolder);
                this.mNetwork.sendDWORD(1);
                this.mNetwork.sendDWORD(nUserID);
                this.mNetwork.sendString(getComputerName());

                var result = (EServerResult)this.mNetwork.receiveDWORD();
                if (result != EServerResult.SUCCESS)
                {
                    this.mError = result;
                    return null;
                }

                String strFolder = this.mNetwork.receiveString();
                return strFolder;
            }
        }

        ///  エラーメッセージに直す
        public String getErrorReasonString()
        {
            lock (this.mLock)
            {
                if (this.mError == EServerResult.UnknownError)
                {
                    return "何らかのエラーが発生しました";
                }
                else if (this.mError == GodaiLibrary.GodaiQuest.EServerResult.UserAlreadyExist)
                {
                    return "同一メールアドレスのユーザが既に存在します";
                }
                else if (this.mError == GodaiLibrary.GodaiQuest.EServerResult.PasswordWrong)
                {
                    return "パスワードが間違っています";
                }
                else if (this.mError == GodaiLibrary.GodaiQuest.EServerResult.MissingUser)
                {
                    return "ユーザが存在しません";
                }
                else if (this.mError == EServerResult.RequireLogon)
                {
                    return "ログインしていません。ログインする必要があります。";
                }
                else if (this.mError == EServerResult.ClientObsolete)
                {
                    return "新しいバージョンのソフトをインストールしてください。";
                }
                else if (this.mError == EServerResult.MissingItem)
                {
                    return "対応するアイテムが存在しません(内部エラー)";
                }
                else if (this.mError == EServerResult.AlreadyLogin)
                {
                    return "すでにログイン済みです";
                }
                else if (this.mError == EServerResult.MissingObject)
                {
                    return "オブジェクトが見つかりません(内部エラー)";
                }
                else if (this.mError == EServerResult.MissingDugeonBlockImage)
                {
                    return "タイルイメージがありません";
                }
                else if (this.mError == EServerResult.NotYourArticle)
                {
                    return "あなたの書き込みがありません";
                }
                else if (this.mError == EServerResult.NotExistDungeon)
                {
                    return "作成していないダンジョンです";
                }
                else if (this.mError == EServerResult.NotEnoughExp)
                {
                    return "経験値が足りません";
                }
                else if (this.mError == EServerResult.ClientNewer)
                {
                    return "クライアントが新しすぎます";
                }
                else
                    return "";
            }
        }
    }
}

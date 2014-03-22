using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuestServer
{
    /*
     * 全体を統括するクラス
     */
    public class ServerWorker
    {
        public static int SERVER_PORT = 21014;  // サーバ用のポート
        //public static int SERVER_PORT = 21015;  // サーバ用のポート

#if __MonoCS__
		private FormServerMono mForm;
#else
		private FormServer mForm;
#endif
		private bool mStopFlag = false;
        private MongoMaster mMongo;
        private object mSync = new object();
        private Dictionary<int, int> mDicLogonUser = new Dictionary<int, int>();
        private Dictionary<int, UserWorker> mDicUserWorker = new Dictionary<int, UserWorker>();
        private NPCWorker mNPCWorker;
        private RSSReaderWorker mRSSReaderWorker;
        private MonsterMaster _monster;
		public System.Threading.ManualResetEvent EventWeakUp = new ManualResetEvent(false);
		public System.Threading.ManualResetEvent EventServerWeakUp = new ManualResetEvent(false);
        public bool WakeUpFailed = false;

#if __MonoCS__
		public void startThread(FormServerMono form)
#else
		public void startThread(FormServer form)
#endif
		{
            this.mForm = form;

            //
            try
            {
                bool bMongoDB_OK = true;
                try
                {
                    this.mMongo = new MongoMaster();
                }
                catch (Exception ex)
                {
                    GodaiLibrary.MessageBox2.Show("MongoDBとの接続で例外が発生しました:" + ex.Message);
                    bMongoDB_OK = false;
                }

                if (!bMongoDB_OK || !mMongo.IsAvailableMongoDB())
                {
                    GodaiLibrary.MessageBox2.Show("MongoDBとの接続に失敗しました．起動していますか？");
                    WakeUpFailed = true;
                    return;
                }
                // サーバ用のスレッドを作り実行する
                Thread thread = new Thread(new ThreadStart(this.run));
                thread.Start();
                EventServerWeakUp.WaitOne();

                // モンスターリストを生成する
                _monster = new MonsterMaster(this);
                Thread thread4 = new Thread(_monster.Run);
                thread4.IsBackground = true;
                thread4.Start();

                // NPC用のスレッドを作り実行する
                this.mNPCWorker = new NPCWorker(this);
                Thread thread2 = new Thread(new ThreadStart(this.mNPCWorker.Run));
                thread2.IsBackground = true; // 終了を待たない
                thread2.Start();

                // Reader用スレッドを作成し実行する
                this.mRSSReaderWorker = new RSSReaderWorker(this);
                Thread thread3 = new Thread(new ThreadStart(this.mRSSReaderWorker.Run));
                thread3.IsBackground = true;
                thread3.Start();

            }
            finally
            {
                // 起動には成功したことを返す
                EventWeakUp.Set();
            }
        }

        // サーバ用スレッド
        public void run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, SERVER_PORT );
            try
            {
                listener.Start();
            }
            catch (Exception )
            {
                GodaiLibrary.MessageBox2.Show("同時にサーバーを起動することはできません．もしくはポートが使用済みです");
				System.Environment.Exit(0);
            }

            EventServerWeakUp.Set();

            while (!this.mStopFlag)
            {
                if (listener.Pending())
                {
                    // ユーザ用ワーカを立ち上げる
                    TcpClient client = listener.AcceptTcpClient();
                    UserWorker worker = new UserWorker(client, this, _monster);
                    Thread thread = new Thread(new ThreadStart(worker.run));
                    thread.Start();
                }
                else
                {
                    Thread.Sleep(1);
                }
            }

            listener.Stop();
        }

        public void terminate()
        {
            this.mRSSReaderWorker.terminate();
        }

        public void logon(int nUserID, UserWorker worker)
        {
            this.mDicLogonUser.Add(nUserID, nUserID);
            lock (this.mDicUserWorker)
            {
                this.mDicUserWorker.Add(nUserID, worker);
            }
        }
        public void logout(int nUserID, UserWorker worker)
        {
            this.mDicLogonUser.Remove(nUserID);

            lock (this.mDicUserWorker)
            {
                this.mDicUserWorker.Remove(nUserID);
            }
        }

        // シグナルを伝達する
        public void makeSignalAllUser(Signal sig_)
        {
            lock (this.mDicUserWorker)
            {
                foreach (var worker in this.mDicUserWorker.Values)
                {
                    worker.addSignal(sig_);
                }
            }
        }

        // シグナルを伝達する
        public void makeSignal(Signal sig_, int nUserID)
        {
            lock (this.mDicUserWorker)
            {
                UserWorker worker;
                this.mDicUserWorker.TryGetValue(nUserID, out worker);
                if ( worker == null )
                    return;
                worker.addSignal(sig_);
            }
        }

        //
        public void setSystemMessage(String strMessage)
        {
            lock (this.mDicUserWorker)
            {
                foreach (var worker in this.mDicUserWorker.Values)
                {
                    worker.setSystemMessage(strMessage);
                }
            }

        }

        public Dictionary<int, int> getLoginUserList()
        {
            return this.mDicLogonUser;
        }

        public void setStopThread()
        {
            this.mStopFlag = true;
        }

        // ログ出力用
        public void addLog(String strLog)
        {
            this.mForm.addLog(strLog);
        }

        // ユーザ認証
        public GodaiLibrary.GodaiQuest.EServerResult tryLogon(out int nID, String strUser, String strPasswordHash)
        {
            lock (this.mSync)
            {
                return this.mMongo.tryLogon(out nID, strUser, strPasswordHash);
            }
        }

        // ユーザを追加する
        public GodaiLibrary.GodaiQuest.EServerResult addUser(out int nUserID, UserInfoInside infoUser)
        {
            lock (this.mSync)
            {
                //
                var result = this.mMongo.addUser(out nUserID, infoUser.asDBUser());
                if (result != EServerResult.SUCCESS)
                    return result;

                return result;
            }
        }

        // ダンジョンを得る
        public GodaiLibrary.GodaiQuest.EServerResult getDungeon(out GodaiLibrary.GodaiQuest.DungeonInfo dungeon, int nID, int nDungeonNumber)
        {
            lock (this.mSync)
            {
                return this.mMongo.getDungeon(out dungeon, nID, nDungeonNumber);
            }
        }

        // ダンジョンの大陸を設定する
        public GodaiLibrary.GodaiQuest.EServerResult setDungeonForIsland(int nRealID, DungeonInfo dungeon, DungeonBlockImageInfo images, ObjectAttrInfo objectattrinfo, TileInfo tileinfo)
        {
            lock (this.mSync)
            {
                return this.mMongo.setDungeonForIsland(nRealID, dungeon, images, objectattrinfo, tileinfo);
            }
        }

        // ダンジョンを設定する
        public GodaiLibrary.GodaiQuest.EServerResult setDungeon(GodaiLibrary.GodaiQuest.DungeonInfo dungeon, int nID, int nDungeonNumber, DungeonBlockImageInfo images, ObjectAttrInfo objectinfo, TileInfo tileinfo)
        {
            lock (this.mSync)
            {
                return this.mMongo.setDungeon(dungeon, nID, nDungeonNumber, images, objectinfo, tileinfo);
            }
        }

        // ダンジョンのブロック絵を得る
        public GodaiLibrary.GodaiQuest.EServerResult getDungeonBlockImage(out DungeonBlockImageInfo images)
        {
            lock (this.mSync)
            {
                return this.mMongo.getDungeonBlockImage(out images);
            }
        }

        // ダンジョンのブロック絵を設定する
        public EServerResult setDungeonBlockImage(DungeonBlockImageInfo images)
        {
            lock (this.mSync)
            {
                return this.mMongo.setDungeonBlockImage(images);
            }
        }

        // ブロックイメージを設定する
        public EServerResult setBlockImagePalette(TilePalette palette, int nUserID)
        {
            lock (this.mSync)
            {
                return this.mMongo.setBlockImagePalette(palette, nUserID);
            }
        }

        // ブロックイメージを得る
        public EServerResult getBlockImagePalette(out TilePalette palette, int nUserID)
        {
            lock (this.mSync)
            {
                return this.mMongo.getBlockImagePalette(out palette, nUserID );
            }
        }

        // オブジェクト情報を設定する
        public EServerResult setObjectAttrInfo(ObjectAttrInfo info)
        {
            lock (this.mSync)
            {
                return this.mMongo.setObjectAttrInfo(info);
            }
        }

        // オブジェクト情報を取得する
        public EServerResult getObjectAttrInfo(out ObjectAttrInfo info)
        {
            lock (this.mSync)
            {
                return this.mMongo.getObjectAttrInfo(out info);
            }
        }

        // タイルIDリストを得る
        public EServerResult getTileList(out TileInfo tileinfo)
        {
            lock (this.mSync)
            {
                return this.mMongo.getTileList(out tileinfo);
            }
        }

        // ユーザ情報を得る
        public EServerResult getUserInfo(out UserInfo userinfo)
        {
            lock (this.mSync)
            {
                return this.mMongo.getUserInfo(out userinfo);
            }
        }

        // アイテムを登録する
        public EServerResult setAItem(ref AItem item, ImagePair itemimage, int nUserID )
        {
            lock (this.mSync)
            {
                return this.mMongo.setAItem(ref item, itemimage, nUserID);
            }
        }

        // アイテム一覧を得る
        public EServerResult getItemInfoByUserId(out ItemInfo iteminfo, int nUserId)
        {
            lock (this.mSync)
            {
                return this.mMongo.getItemInfoByUserId(out iteminfo, nUserId);
            }
        }

		// アイテム一覧を得る
        public EServerResult getItemInfo(out ItemInfo iteminfo)
        {
            lock (this.mSync)
            {
                return this.mMongo.getItemInfo(out iteminfo);
            }
        }

        // ユーザ情報を変更する
        public EServerResult setAUser(AUser user)
        {
            lock (this.mSync)
            {
                return this.mMongo.setAUser(user);
            }
        }

        // アイテム情報を変更する
        public EServerResult changeAItem(AItem item)
        {
            lock (this.mSync)
            {
                return this.mMongo.changeAtItem(item);
            }
        }

        // 位置を設定する
        public EServerResult setALocation(int nUserID, ALocation loc)
        {
            lock (this.mSync)
            {
                return this.mMongo.setALocation(nUserID, loc);
            }
        }

        // 位置一覧を取得する
        public EServerResult getLocationInfo(out LocationInfo locInfo)
        {
            lock (this.mSync)
            {
                return this.mMongo.getLocationInfo(out locInfo);
            }
        }

        // 大陸の領土情報を得る
        public EServerResult getIslandGroundInfo(out IslandGroundInfo groundinfo)
        {
            lock (this.mSync)
            {
                return this.mMongo.getIslandGroundInfo(out groundinfo);
            }
        }

        // オブジェクトを変える
        public EServerResult changeObjectAttr(ObjectAttr obj) {
            lock(this.mSync) 
            {
                return this.mMongo.changeObjectAttr(obj);
            }
        }

        // イメージを変える
        public EServerResult changeDugeonBlockImagePair(ImagePair imagepair)
        {
            lock (this.mSync)
            {
                return this.mMongo.changeDungeonBlockImagePair(imagepair);
            }
        }

        // メッセージを得る
        public EServerResult getMessageInfo( out MessageInfo mesinfo ) {
            lock(this.mSync) 
            {
                return  this.mMongo.getMessageInfo(out mesinfo);
            }
        }

        // メッセージを設定する
        public EServerResult setAMessage(AMessage mes)
        {
            lock (this.mSync)
            {
                return this.mMongo.setAMessage(mes);
            }
        }

        // アイテムの所有者を返す
        public int getItemOwner(int nItemID)
        {
            lock (this.mSync)
            {
                return this.mMongo.getItemOwner(nItemID);
            }
        }

        // アイテムをすでに見ているかを調べる
        public bool isAlreadyPickedupItem(int nItemID, int nUserID)
        {
            lock (this.mSync)
            {
                return this.mMongo.isAlreadyPickedupItem(nUserID, nItemID);
            }
        }

        // アイテムを見たことを記録する
        public void setPickedupItem(int nUserID, int nItemID)
        {
            lock (this.mSync)
            {
                this.mMongo.setPickedupItem(nUserID, nItemID);
            }
        }

        // 経験値を増やす
        public void incrementExpValue(int nUserID, int nValue)
        {
            lock (this.mSync)
            {
                ExpValue expvalue;
                this.mMongo.getExpValue(out expvalue, nUserID);
                expvalue.incValue(nValue);
                this.mMongo.setExpValue(nUserID, expvalue);

                this.makeSignal(new Signal(SignalType.RefreshExpValue), nUserID );
            }
        }

        // 現在の経験値を得る
        public ExpValue getExpValue(int nUserID)
        {
            lock (this.mSync)
            {
                ExpValue expvalue;
                this.mMongo.getExpValue(out expvalue, nUserID);
                return expvalue;
            }
        }

        // 見てないアイテム一覧を得る
        public EServerResult getUnpickedupItemInfo(out HashSet<int> setItemID, int nUserID, int nDungeonID)
        {
            lock (this.mSync)
            {
                // オーナーがnDungeonIDのものだけを残す
                setItemID = this.mMongo.getItemIDListBelongUser(nDungeonID);
                this.mMongo.removeAlreadyPickedupItem(ref setItemID, nDungeonID, nUserID);
            }
            return EServerResult.SUCCESS;
        }

        // 足あと一覧を得る
        public EServerResult getAshiato(out PickupedInfo pickinfo, int nItemID)
        {
            lock (this.mSync)
            {
                return this.mMongo.getAshiato(out pickinfo, nItemID);
            }
        }

        // 足あとログを追加する
        public EServerResult addAshiatoLog(int nUserID, String strLog)
        {
            lock (this.mSync)
            {
                return this.mMongo.addAshiatoLog(nUserID, strLog);
            }
        }

        // 足あとログを得る
        public EServerResult getAshiatoLog(out List<String> listLog, int nUserID)
        {
            lock (this.mSync)
            {
                return this.mMongo.getAshiatoLog(out listLog, nUserID);
            }
        }

        // アーティクルを得る
        // 整形して返す
        public EServerResult getArticleString(out String strArticle, int nItemID)
        {
            lock (this.mSync)
            {
                UserInfo userinfo;
                getUserInfo(out userinfo);
                userinfo.addUesr(new AUser(0, "", "System", null) );

                StringBuilder ret = new StringBuilder();

                int nCnt = this.mMongo.countItemArticle(nItemID);
                for (int it = nCnt-1; it >= 0; --it)
                {
                    ItemArticle article;
                    if (this.mMongo.getItemArticle(out article, nItemID, it))
                    {
                        if (article.getContents().Length == 0)
                        {
//                            ret.AppendLine("Deleted");
                        }
                        else
                        {
                            ret.AppendLine("---------------------------");

                            AUser user = userinfo.getAUser(article.getUserID());
                            var createtime = article.getCreateTime();
                            ret.AppendLine((""+it)+" [" + user.getName() + "] " + createtime.ToLocalTime().ToShortDateString() + " " + createtime.ToLocalTime().ToShortTimeString());
                            ret.AppendLine(article.getContents());
                        }
                    }
                }

                strArticle = ret.ToString();
                return EServerResult.SUCCESS;
            }
        }

        // アーティクルを書き込む
        public EServerResult setItemAritcle(ItemArticle article)
        {
            lock (this.mSync)
            {
                UserInfo userinfo;
                this.getUserInfo(out userinfo);

                userinfo.addUesr(new AUser(0, "", "System", null));

                // アーティクルを書き込む
                int nCnt = this.mMongo.countItemArticle(article.getItemID());
                article.setArticleID(nCnt);
                this.mMongo.setItemArticle(article);

                // 未読を設定する
                this.mMongo.setUnreadArticle(article, userinfo);

                // 
                var writer = userinfo.getAUser(article.getUserID());
                foreach (var userdb in userinfo ) {

                    var reader = userinfo.getAUser(userdb.getUserID());
                    if (writer.getUserID() == article.getUserID())
                    {
                        if (reader.getUserID() != writer.getUserID())
                        {   // 技術情報を書き込んだ人と、メッセージを書いた人が違うならOK
                            String strLog1 = writer.getName() + "があなたのメッセージにレスポンスを書きました";
                            this.addAshiatoLog(reader.getUserID(), strLog1);
                        }
                    }
                    else
                    {
                        String strLog1 = writer.getName() + "がメッセージを書きました";
                        this.addAshiatoLog(reader.getUserID(), strLog1);
                    }
                }

                return EServerResult.SUCCESS;
            }
        }

        // アーティクルを読んだ情報
        public EServerResult readArticle(int nItemID, int nUserID)
        {
            lock (this.mSync)
            {
                UserInfo userinfo;
                this.getUserInfo(out userinfo);

                // 読んだアーティクルの持ち主一覧を受け取る
                var listUnreadOwner = this.mMongo.getUnreadArticleOwnerList(nItemID, nUserID);
                if (listUnreadOwner.Count == 0)
                    return EServerResult.SUCCESS;

                // 読み込み情報を消す
                var result = this.mMongo.removeUnreadArticle(nItemID, nUserID);
                if (result != EServerResult.SUCCESS)
                    return result;  // おそらく読み済み

                // 経験を増やす
                this.incrementExpValue(nUserID, 5); // 5ポイント増加

                var user = userinfo.getAUser(nUserID);

                foreach (var ownerid in listUnreadOwner)
                {
                    this.incrementExpValue(ownerid, 1); // 持ち主は１ポイントもらえる

                    String strLog1 = user.getName() + "があなたの書き込みを見ました";
                    this.addAshiatoLog(ownerid, strLog1);

                    this.makeSignal(new Signal(SignalType.RefreshExpValue), ownerid);
                }

                // 通知をする
                this.makeSignal(new Signal(SignalType.RefreshExpValue), nUserID);
            }

            return EServerResult.SUCCESS;
        }

        // アーティクルを削除する
        public EServerResult deleteLastItemArticle(int nItemID, int nUserID)
        {
            lock (this.mSync)
            {
                return this.mMongo.deleteLastItemArticle(nItemID, nUserID);
            }
        }

        // 経験値を使う
        // コスト計算のロジックがある
        public EServerResult useExperience(int nUserID, int nDungeonNumber, EUseExp eUseExpType)
        {
            lock (this.mSync)
            {
                ExpValue expvalue = this.getExpValue(nUserID);

                if (eUseExpType == EUseExp.ExpandX)
                {
                    DungeonInfo dungeon;
                    var result = this.getDungeon(out dungeon, nUserID, nDungeonNumber);
                    if ( result != EServerResult.SUCCESS )
                        return result;
                    int nCost = (dungeon.getSizeY()) * (nDungeonNumber + 1) * 10;
                    if (expvalue.getExpValue() < nCost)
                    {
                        return EServerResult.NotEnoughExp;
                    }
                    this.incrementExpValue(nUserID, -nCost);
                    dungeon.resize(dungeon.getSizeX()+1, dungeon.getSizeY() );
                    this.mMongo.setDungeonInside(dungeon, nUserID, nDungeonNumber);
                }
                else if (eUseExpType == EUseExp.ExpandY)
                {
                    DungeonInfo dungeon;
                    var result = this.getDungeon(out dungeon, nUserID, nDungeonNumber);
                    if (result != EServerResult.SUCCESS)
                        return result;
                    int nCost = (dungeon.getSizeX()) * (nDungeonNumber + 1) * 10;
                    if (expvalue.getExpValue() < nCost)
                    {
                        return EServerResult.NotEnoughExp;
                    }
                    this.incrementExpValue(nUserID, -nCost);
                    dungeon.resize(dungeon.getSizeX(), dungeon.getSizeY()+1);
                    this.mMongo.setDungeonInside(dungeon, nUserID, nDungeonNumber);
                }
                else if (eUseExpType == EUseExp.CreateNewFloor)
                {
                    int nDepth = this.mMongo.getDungeonDepth(nUserID);
                    int nCost = (nDepth+1) * 10 * 11 * 11;

                    if (expvalue.getExpValue() < nCost)
                    {
                        return EServerResult.NotEnoughExp;
                    }
                    this.incrementExpValue(nUserID, -nCost);
                    this.mMongo.setDungeonDepth(nUserID, nDepth + 1);
                }
            }

            return EServerResult.SUCCESS;
        }

        // 深さを得る
        public int getDungeonDepth(int nDungeonID)
        {
            lock (this.mSync)
            {
                return this.mMongo.getDungeonDepth(nDungeonID);
            }
        }

        // モンスタかを得る
        public EServerResult getMonsterInfo(out MonsterInfo monsterinfo, int nDungeonID )
        {
            lock (this.mSync)
            {
                if (nDungeonID == 0x40000000)
                {
                    // Sepcial Dungeon
                    return this.mMongo.getMonsterInfoForRandomDungeon(out monsterinfo);
                }
                else
                {
                    return this.mMongo.getMonsterInfo(out monsterinfo);
                }
            }
        }

        // モンスタかを設定する
        public void setMonster(int nItemID, bool bMonster)
        {
            lock (this.mSync)
            {
                this.mMongo.setMonster(nItemID, bMonster);
            }
        }

        // モンスタかを設定する（ランダムダンジョン用）
        public void setMonsterForRandomDungeon(int nItemID, bool bMonster)
        {
            lock (this.mSync)
            {
                this.mMongo.setMonsterForRandomDungeon(nItemID, bMonster);
            }
        }

        // NPCランダムダンジョンの作成
        public void setDungeonByNPC(ulong[] dungeon, int nSizeX, int nSizeY, int nDungeonID, int nDungeonNumber)
        {
            lock (this.mSync)
            {
                this.mMongo.setDungeonByNPC(dungeon, nSizeX, nSizeY, nDungeonID, nDungeonNumber);
            }
        }

        // ランダムダンジョンの読み込み済みアイテムリストを得る
        public EServerResult getRDReadItemInfo(out RDReadItemInfo readiteminfo, int nUserID)
        {
            lock (this.mSync)
            {
                return this.mMongo.getRDReadItemInfo(out readiteminfo, nUserID);
            }
        }

        // ランダムダンジョンのアイテムを読み込み済みにする
        public EServerResult setRDReadItem(int nUserID, int nItemID)
        {
            lock (this.mSync)
            {
                if (this.mMongo.setRDReadItem(nUserID, nItemID))
                {
                    // 経験値も増やす
                    int nItemOwner = this.mMongo.getItemOwner(nItemID);

                    // アイテムのオーナーに+1する
                    this.incrementExpValue(nItemOwner, 1);

                    // 読んだ人に+10する
                    this.incrementExpValue(nUserID, 10);
                }

                return EServerResult.SUCCESS;
            }
        }

        // ランダムダンジョンのアイテム読み込み済みをクリアする
        public void clearRDReadItemInfo()
        {
            lock (this.mSync)
            {
                this.mMongo.clearRDReadItemInfo();
            }
        }

        // パスワードを変更する
        public EServerResult changePassword(int nUserID, string strNewPasswordHash)
        {
            lock (this.mSync)
            {
                return this.mMongo.changePassword(nUserID, strNewPasswordHash);
            }
        }

        // 最終更新日付を得る
        public DateTime getLastUpdateTimeOfRSS(int nItemID, DateTime defaulttime )
        {
            lock (this.mSync)
            {
                return this.mMongo.getLastUpdateTimeOfRSS(nItemID, defaulttime);
            }
        }

        // 最終更新日付を設定する
        public void setLastUpdateTimeOfRSS(int nItemID, DateTime time)
        {
            lock (this.mSync)
            {
                this.mMongo.setLastUpdateTimeOfRSS(nItemID, time);
            }
        }

        // ユーザのフォルダを設定する
        public EServerResult setUserFolder(int nUserID, String strComputerName, String strFolder)
        {
            lock (this.mSync)
            {
                return this.mMongo.setUserFolder(nUserID, strComputerName, strFolder);
            }
        }

        // ユーザのファルダを得る
        public String getUserFolder(int nUserID, String strComputerName)
        {
            lock (this.mSync)
            {
                return this.mMongo.getUserFolder(nUserID, strComputerName);
            }
        }

		// キーワードを登録する
        public EServerResult registerKeyword(out int nKeywordID, int nUesrID, string keyword, int nKeywordPriority)
        {
            lock (mSync)
            {
                return mMongo.registerKeyword(out nKeywordID, nUesrID, keyword, nKeywordPriority);
            }
        }

		// キーワードを変更する
        public EServerResult modifyKeyword(int nUserID, int nKeywordID, string newKeyword)
        {
            lock (mSync)
            {
                return mMongo.modifyKeyword(nUserID, nKeywordID, newKeyword);
            }
        }

		// キーワードの優先順位を変える
        public EServerResult modifyKeywordPriority(int nUserID, int nKeywordID, int nNewPriority)
        {
            lock (mSync)
            {
                return mMongo.modifyKeywordPriority(nUserID, nKeywordID, nNewPriority);
            }
        }

		// キーワードをアイテムに関連づける
        public EServerResult attachKeyword( int nKeywordId, int nItemID, int nItemPriority)
        {
            lock (mSync)
            {
                return mMongo.attachKeyword( nKeywordId, nItemID, nItemPriority);
            }
        }

		// キーワードに関連づけたアイテムを除く
        public void detachKeyword(int nKeywordId, int nItemID)
        {
            lock (mSync)
            {
                mMongo.detachKeyword(nKeywordId, nItemID);
            }
        }

		// キーワード一覧を得る
        public KeywordUserInfo listKeyword(int nUserID)
        {
            lock (mSync)
            {
                return mMongo.listKeyword(nUserID);
            }
        }

		// キーワードに関連付けたアイテム一覧を得る
        public AKeyword getKeywordDetail(int nKeywordId)
        {
            lock (mSync)
            {
                return mMongo.getKeywordDetail(nKeywordId);
            }
        }

		// アイテムの優先順位を変える
        public EServerResult modifyKeywordItemPriority(int nKeywordID, int nItemID, int nNewPriority)
        {
            lock (mSync)
            {
                return mMongo.modifyKeywordItemPriority(nKeywordID, nItemID, nNewPriority);
            }
        }

		// キーワードを削除する
        public EServerResult deleteKeyword(int nUserID, int nKeywordID)
        {
            lock (mSync)
            {
                return mMongo.deleteKeyword(nUserID, nKeywordID);
            }
        }

#if false 

        // アイテム情報を得る
        public EServerResult getItemImageInfo(out ItemImageInfo itemimageinfo)
        {
            lock (this.mSync)
            {
                return this.mMongo.getItemImageInfo(out itemimageinfo);
            }
        }
#endif

		// 強制初期化する
		// ユーザは作らない
        public bool ForceInitializeMongoDB()
        {
            String strFolder = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath),
                "InitImages");
            const int UserID = 1;

			var objinfo = new ObjectAttrInfo();
            var tileinfo = new TileInfo();
			var images = new DungeonBlockImageInfo();

            DungeonInfo dungeon;
            if (getDungeon(out dungeon, 0, 0) != EServerResult.SUCCESS)
            {
                GodaiLibrary.MessageBox2.Show("Internal Error: can't get dungeon");
                return false;
            }

			using(var loader = new GodaiLibrary.CSVLoader())
            try
            {
                if (!loader.OpenFile(Path.Combine(strFolder, "_InitBlock.txt"), Encoding.UTF8))
                {
                    GodaiLibrary.MessageBox2.Show("_InitBlock.txtが開けませんでした");
                    return false;
                }

				int nObjectID = 0;
				int nImageID = 0;
				Image img;

                while (loader.ReadLine())
                {
                    String strName = loader.GetString();
                    String strFileName = loader.GetString();
                    bool bCanWalk = loader.GetInt() != 0 ? true : false;
					var nCommand = loader.GetInt();
					EObjectCommand eCommand;
					bool bCanItem = false;
					if ( nCommand < 5 ) { 
						eCommand = (EObjectCommand)nCommand;
					}
					else { 
						eCommand = EObjectCommand.Nothing;
						bCanItem = true;
						bCanWalk = true;
					}

                    try
                    {
                        img = GodaiLibrary.KLib.loadAndResizeImage(64, 64, Path.Combine(strFolder, strFileName));
                    }
                    catch (IOException e)
                    {
                        GodaiLibrary.MessageBox2.Show(strFileName + "の画像が読めませんでした");
                        throw e;
                    }
                    images.addImage((uint) nImageID, bCanItem, img, strName, UserID, DateTime.Now, true);
                    var itemattr1 = new ObjectAttr(nObjectID, bCanWalk, 0, eCommand, 0, true);
                    objinfo.addObject(itemattr1);

                    Tile tile1 = new Tile((uint) nObjectID, (uint) nImageID);
                    tileinfo.addTile(tile1);

                    ++nImageID;
                    ++nObjectID;
                }

				// 情報をセットする
                setDungeon(dungeon, 0, 0, images, objinfo, tileinfo );
            }
            catch (Exception e)
            {
                GodaiLibrary.MessageBox2.Show("何らかの例外が発生しました．ロールバックはしないので手動でデータベースを削除してやり直してください:" + e.Message);
                return false;
            }


            GodaiLibrary.MessageBox2.Show("ディフォルトの設定で初期化しました");
            return true;
        }
    }
}

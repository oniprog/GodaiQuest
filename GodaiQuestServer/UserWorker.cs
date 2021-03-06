using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting.Messaging;
using GodaiLibrary.GodaiQuest;
using GodaiLibrary;

namespace GodaiQuestServer
{
    /* 
     * 一人のユーザに付き、1つ作成される。ユーザがログアウトするまで面倒を見る
     */
    public class UserWorker
    {
        //public int CLIENT_VERSION = 2012120216; 
        public int CLIENT_VERSION = 2014021819;

        private TcpClient mTcpClient;
        private ServerWorker mParent;

        private String mMail;
        private String mName;
        private int mUserID;
        private SignalQueue mSignalQueue = new SignalQueue();

        private MonsterMaster _monster;		// 外部モンスター
        private ALocation _locationSelf;

        public UserWorker(TcpClient tcpclient, ServerWorker parent, MonsterMaster mon) 
        {
            this.mTcpClient = tcpclient;
            this.mParent = parent;
            _monster = mon;
        }

        private GodaiLibrary.Network mNetwork;

        public void run()
        {
            try
            {
                // まずは認証から行う
                this.mNetwork = new GodaiLibrary.Network(this.mTcpClient.GetStream());

                // バージョン番号
                int nVersion = this.mNetwork.receiveDWORD();
                if (nVersion > 0x1000)
                {
#if false
                    // Webサーバ用と判定する
                    char ch1 = (char) (nVersion & 0xff);
                    char ch2 = (char) ((nVersion >> 8) & 0xff);
                    char ch3 = (char) ((nVersion >> 16) & 0xff);
                    char ch4 = (char) ((nVersion >> 24) & 0xff);
                    var webServer = new WebServerWorker();
                    String str1 = (string)ch1 + ch2 + ch3 + ch4;
                    webServer.SetInitString(str1);
                    webServer.Run();
#endif
                    return;
                }

                // 接続OKを返す
                this.mNetwork.sendDWORD(1);
                this.mNetwork.flush();

                this.mMail = "Unknown";

                while (true)
                {

                    // コマンド
                    GodaiLibrary.GodaiQuest.EServerCommand eCommand = (GodaiLibrary.GodaiQuest.EServerCommand)this.mNetwork.receiveDWORD();
                    if (eCommand == GodaiLibrary.GodaiQuest.EServerCommand.TryLogon)
                    {
                        ComTryLogOn();
                    }
                    else if (eCommand == GodaiLibrary.GodaiQuest.EServerCommand.AddUser)
                    {
                        ComAddUser();
                    }
                    else {
                        if ( !this.isLogon() ) {
                            this.mNetwork.sendDWORD((int)EServerResult.RequireLogon );
                            this.mNetwork.flush();
                            throw new Exception("ログインが必要です");
                        }
                        if (eCommand == EServerCommand.SetDungeon)
                        {
                            ComSetDungeon();
                        }
                        else if (eCommand == EServerCommand.GetDungeon)
                        {
                            ComGetDungeon();
                        }
                        else if (eCommand == EServerCommand.SetDungeonBlockImage)
                        {
                            ComSetDungeonBlockImage();
                        }
                        else if (eCommand == EServerCommand.GetDungeonBlockImage)
                        {
                            ComGetDungeonBlockImage();
                        }
                        else if (eCommand == EServerCommand.GetTilePalette)
                        {
                            ComGetBlockImagePalette();
                        }
                        else if (eCommand == EServerCommand.SetTilePalette)
                        {
                            ComSetBlockImagePalette();
                        }
                        else if (eCommand == EServerCommand.GetObjectAttrInfo)
                        {
                            ComGetObjectAttrInfo();
                        }
                        else if (eCommand == EServerCommand.SetObjectAttrinfo)
                        {
                            ComSetObjectAttrInfo();
                        }
                        else if (eCommand == EServerCommand.GetTileList)
                        {
                            ComGetTileList();
                        }
                        else if (eCommand == EServerCommand.GetUserInfo)
                        {
                            ComGetUserInfo();
                        }
                        else if (eCommand == EServerCommand.SetAItem)
                        {
                            ComSetAItem();
                        }
                        else if (eCommand == EServerCommand.GetItemInfo)
                        {
                            ComGetItemInfo();
                        }
                        else if (eCommand == EServerCommand.GetAItem)
                        {
                            ComGetAItem();
                        }
                        else if (eCommand == EServerCommand.SetAUser)
                        {
                            ComSetAUser();
                        }
                        else if (eCommand == EServerCommand.ChangeAItem)
                        {
                            ComChangeAItem();
                        }
                        else if (eCommand == EServerCommand.UploadItemFiles)
                        {
                            ComUploadItemFiles();
                        }
                        else if (eCommand == EServerCommand.Polling)
                        {
                            ComPolling();
                        }
                        else if (eCommand == EServerCommand.GetIslandGroundInfo)
                        {
                            ComGetIslandGroundInfo();
                        }
                        else if (eCommand == EServerCommand.ChangeObjectAttr)
                        {
                            ComChangeObjectAttr();
                        }
                        else if (eCommand == EServerCommand.ChangeDungeonBlockImagePair)
                        {
                            ComChangeDungeonBlockImagePair();
                        }
                        else if (eCommand == EServerCommand.GetLocationInfo)
                        {
                            ComGetLocationInfo();
                        }
                        else if (eCommand == EServerCommand.GetMessageInfo)
                        {
                            ComGetMessageInfo();
                        }
                        else if (eCommand == EServerCommand.SetAMessage)
                        {
                            ComSetAMessage();
                        }
                        else if (eCommand == EServerCommand.GetExpValue)
                        {
                            ComGetExpValue();
                        }
                        else if (eCommand == EServerCommand.GetUnpickedupItemInfo)
                        {
                            ComGetUnpickedupItemInfo();
                        }
                        else if (eCommand == EServerCommand.GetAshiato)
                        {
                            ComGetAshiato();
                        }
                        else if (eCommand == EServerCommand.GetAshiatoLog)
                        {
                            ComGetAshiatoLog();
                        }
                        else if (eCommand == EServerCommand.GetArticleString)
                        {
                            ComGetArticleString();
                        }
                        else if (eCommand == EServerCommand.SetItemArticle)
                        {
                            ComSetItemArticle();
                        }
                        else if (eCommand == EServerCommand.ReadArticle)
                        {
                            ComReadArticle();
                        }
                        else if (eCommand == EServerCommand.DeleteLastItemArticle)
                        {
                            ComDeleteLastItemArticle();
                        }
                        else if (eCommand == EServerCommand.UseExperience)
                        {
                            ComUseExperience();
                        }
                        else if (eCommand == EServerCommand.GetDungeonDepth)
                        {
                            ComGetDungeonDepth();
                        }
                        else if (eCommand == EServerCommand.GetMonsterInfo)
                        {
                            ComGetMonsterInfo();
                        }
                        else if (eCommand == EServerCommand.SetMonster)
                        {
                            ComSetMonster();
                        }
                        else if (eCommand == EServerCommand.GetRDReadItemInfo)
                        {
                            ComGetRDReadItemInfo();
                        }
                        else if (eCommand == EServerCommand.SetRDReadItem)
                        {
                            ComSetRDReadItem();
                        }
                        else if (eCommand == EServerCommand.ChangePassword)
                        {
                            ComChangePassword();
                        }
                        else if (eCommand == EServerCommand.GetUserFolder)
                        {
                            ComGetUserFolder();
                        }
                        else if (eCommand == EServerCommand.SetUserFolder)
                        {
                            ComSetUserFolder();
                        }
                        else if (eCommand == EServerCommand.GetRealMonsterSrcInfo)
                        {
                            ComGetRealMonsterSrcInfo();
                        }
                        else if (eCommand == EServerCommand.GetItemInfoByUserId)
                        {
							ComGetItemInfoByUserId();
                        }
						else if (eCommand == EServerCommand.RegisterKeyword)
						{
						    ComRegisterKeyword();
						}
                        else if (eCommand == EServerCommand.ModifyKeyword)
						{
                            ComModifyKeyword();
						}
                        else if (eCommand == EServerCommand.ModifyKeywordPriority)
						{
                            ComModifyKeywordPriority();
						}
                        else if (eCommand == EServerCommand.AttachKeyword)
						{
                            ComAttachKeyword();
						}
                        else if (eCommand == EServerCommand.DetachKeyword)
						{
                            ComDetachKeyword();
						}
                        else if (eCommand == EServerCommand.ListKeyword)
						{
                            ComListKeyword();
						}
                        else if (eCommand == EServerCommand.GetKeywordDetail)
						{
                            ComGetKeywordDetail();
						}
                        else if (eCommand == EServerCommand.ModifyKeywordItemPriority)
						{
                            ComModifyKeywordItemPriority();
						}
                        else if (eCommand == EServerCommand.DeleteKeyword)
						{
                            ComDeleteKeyword();
						}
                        else if (eCommand == EServerCommand.GetItemInfo2ByUserId)
                        {
							ComGetItemInfo2ByUserId();
                        }
						else
						{
						    throw new Exception("Invalid Command : " + this.mMail);
						}
                    }
                    this.mNetwork.flush();
                }
            }
            catch(Exception ex)
            {
                this.mNetwork.disconnect();
                this.mParent.addLog("Connection close : " + this.mMail + " " + ex.Message );
            }
            finally {
                this.mParent.logout(this.mUserID, this);
            }
        }

        public void addSignal(Signal sig_)
        {
            lock (this.mSignalQueue)
            {
                this.mSignalQueue.addSignal(sig_);
            }
        }

        private void addLog(String strLog)
        {
            this.mParent.addLog(strLog);
        }

        public void setSystemMessage(String strMessage)
        {
            lock (this.mSignalQueue)
            {
                this.mSignalQueue.setSystemMessage(strMessage);
            }
        }

        private bool isLogon()
        {
            return this.mUserID != 0;
        }

        /// コマンド：ユーザの追加
        private void ComAddUser()
        {
            addLog("Command: add user");

            int nVersion = this.mNetwork.receiveDWORD();
            var adduser = this.mNetwork.Deserialize<godaiquest.AddUser>();
            String strMail = adduser.mail_address;
            String strName = adduser.user_name;
            String strPasswordHash = adduser.password;
            String strUserFolder = adduser.user_folder;
            String strComputerName = adduser.computer_name;
            Image imageCharacter = GodaiLibrary.Network.ByteArrayToImage(adduser.user_image);

            addLog("Mail:" + strMail);
            addLog("Name:" + strName);
            addLog("UserFolder:" + strUserFolder);
            addLog("ComputerName: " + strComputerName);

            UserInfoInside info = new UserInfoInside(strMail, strName, strPasswordHash, imageCharacter);

            int nUserID;
            GodaiLibrary.GodaiQuest.EServerResult result = this.mParent.addUser(out nUserID, info);

            if (result == EServerResult.SUCCESS) { 
                this.mParent.setUserFolder(nUserID, strComputerName, strUserFolder);
            }

            addLog("Result:" + result.ToString());
            this.mNetwork.sendDWORD((int)result);

            // 通知する
            this.mParent.makeSignalAllUser(new Signal(SignalType.RefreshUser));
        }

        /// コマンド：ログオンを試みる
        private void ComTryLogOn() {

            addLog("Command : try logon");

            int nVersion = this.mNetwork.receiveDWORD();

            var login = this.mNetwork.Deserialize<godaiquest.Login>();
            String strMail = login.mail_address;
            String strPasswordHash = login.password;
            uint nClientVersion = login.client_version;

            addLog("Mail:" + strMail);
            addLog("Client Version: " + nClientVersion);

            if (nClientVersion < CLIENT_VERSION)
            {
                this.mNetwork.sendDWORD((int)EServerResult.ClientObsolete);
                return;
            }
            else if (nClientVersion > CLIENT_VERSION)
            {
                this.mNetwork.sendDWORD((int)EServerResult.ClientNewer);
                return;
            }

            EServerResult result;
            if (this.mParent.getLoginUserList().ContainsKey(mUserID))
            {
                result = EServerResult.AlreadyLogin;
            }
            else
            {
                result = this.mParent.tryLogon(out this.mUserID, strMail, strPasswordHash);
            }

            addLog("Result : " + result.ToString());

            this.mMail = strMail;

            this.mNetwork.sendDWORD((int)result);

            if (result == EServerResult.SUCCESS)
            {
                this.mNetwork.sendDWORD(this.mUserID);
                this.mParent.logon(this.mUserID, this);
            }
        }

        /// コマンド：ダンジョン情報取得
        private void ComGetDungeon()
        {
            addLog("Command : get dungeon " +this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();
            var get_dungeon = this.mNetwork.Deserialize<godaiquest.GetDungeon>();
            int nID = get_dungeon.id;
            int nDungeonNumber = get_dungeon.dungeon_number;

            addLog("ID : " + ("" + nID));
            addLog("DunNum : " + (""+nDungeonNumber ) );

            GodaiLibrary.GodaiQuest.DungeonInfo dungeon;
            var result = this.mParent.getDungeon(out dungeon, nID, nDungeonNumber);
            addLog("Result:"+result.ToString());
            this.mNetwork.sendDWORD((int)result);

            if (result == GodaiLibrary.GodaiQuest.EServerResult.SUCCESS)
            {
            this.mNetwork.Serialize(dungeon.getSerialize());
            }
        }

        /// コマンド：ダンジョン情報のセット
        private void ComSetDungeon()
        {
            addLog("Command : set dungeon " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            var set_dungeon = this.mNetwork.Deserialize<godaiquest.SetDungeon>();
            int nUserID = set_dungeon.user_id;
            int nDungeonNumber = set_dungeon.dungeon_number;

            addLog("UserId : " + ("" + nUserID));
            addLog("DunNum : " + ("" + nDungeonNumber));

            DungeonInfo dungeon = new DungeonInfo(set_dungeon.dungeon_info);
            DungeonBlockImageInfo images = new DungeonBlockImageInfo(set_dungeon.images);
            ObjectAttrInfo objectinfo = new ObjectAttrInfo(set_dungeon.object_info);
            TileInfo tileinfo = new TileInfo(set_dungeon.tile_info);

			if (nUserID == 0)
			{	
				// 大陸の修正用
				var result = this.mParent.setDungeonForIsland(this.mUserID, dungeon, images, objectinfo, tileinfo);
				this.mNetwork.sendDWORD((int)result);
			}
			else {
				// 普通のダンジョンの修正用
                var result = this.mParent.setDungeon(dungeon, nUserID, nDungeonNumber, images, objectinfo, tileinfo);
				this.mNetwork.sendDWORD((int)result);
			}

            this.mParent.makeSignalAllUser(new Signal(SignalType.RefreshDungeon));
        }

        /// コマンド：ダンジョン壁イメージの取得
        private void ComGetDungeonBlockImage()
        {
            addLog("Command : get dungeon block image " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            DungeonBlockImageInfo info;
            var result = this.mParent.getDungeonBlockImage(out info);

            if ( info ==  null ) info = new DungeonBlockImageInfo();

            this.mNetwork.sendDWORD((int)result);

            this.mNetwork.Serialize(info.getSerialize());
        }

        /// コマンド：ダンジョン壁イメージの設定
        private void ComSetDungeonBlockImage()
        {
            addLog("Command : set dungeon block image " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            var images = new DungeonBlockImageInfo(this.mNetwork.Deserialize<godaiquest.DungeonBlockImageInfo>());

            this.mParent.setDungeonBlockImage(images);

            this.mNetwork.sendDWORD((int)EServerResult.SUCCESS);
        }

        /// コマンド：ブロックイメージパレットの設定
        private void ComSetBlockImagePalette()
        {
            addLog("Command : set block image palette " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();
            var set_block_image_palette = this.mNetwork.Deserialize<godaiquest.SetBlockImagePalette>();
            int nUserID = set_block_image_palette.user_id;
			set_block_image_palette.tile_palette.user_id = set_block_image_palette.user_id;
            var palette = new TilePalette(set_block_image_palette.tile_palette);
            var result = this.mParent.setBlockImagePalette(palette, nUserID);
            this.mNetwork.sendDWORD((int)result);
        }

        /// コマンド：ブロックイメージパレットの取得
        private void ComGetBlockImagePalette()
        {
            addLog("Command : get block image palette " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();
            int nUserID = this.mNetwork.receiveDWORD();

            TilePalette palette;

            var result = this.mParent.getBlockImagePalette(out palette, nUserID);
            this.mNetwork.sendDWORD((int)result);

            if (result == EServerResult.SUCCESS)
            {

                this.mNetwork.Serialize(palette.getSerialize());
            }
        }

        /// コマンド：オブジェクト情報を得る
        private void ComGetObjectAttrInfo()
        {

            addLog("Command : get object attr info " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            ObjectAttrInfo info;
            var result = this.mParent.getObjectAttrInfo(out info);

            this.mNetwork.sendDWORD((int)result);

            if (result != EServerResult.SUCCESS)
            {
                return;
            }

            this.mNetwork.Serialize(info.getSerialize());
        }

        /// コマンド：オブジェクト情報を設定する
        private void ComSetObjectAttrInfo()
        {
            addLog("Command : set object attr info " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            var info = new ObjectAttrInfo(this.mNetwork.Deserialize<godaiquest.ObjectAttrInfo>());
            var result = this.mParent.setObjectAttrInfo(info);
            this.mNetwork.sendDWORD((int)result);
        }

        /// コマンド：タイルリストを得る
        private void ComGetTileList()
        {
            addLog("Command : get tilelist " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            TileInfo tileinfo;
            var result = this.mParent.getTileList(out tileinfo);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS) return;

            this.mNetwork.Serialize(tileinfo.getSerialize());
        }

        /// コマンド：ユーザ一覧を得る
        private void ComGetUserInfo()
        {
            addLog("Command : get userinfo " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            UserInfo userinfo;
            var result = this.mParent.getUserInfo(out userinfo);
            this.mNetwork.sendDWORD((int)result);
            if ( result != EServerResult.SUCCESS)  return;

            this.mNetwork.Serialize(userinfo.getSerialize());

        }

        private String getItemFolder(int nItemID)
        {
            String strDirectory = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            return Path.Combine(Path.Combine(strDirectory, "important_data"), "" + nItemID);
        }

        /// コマンド：アイテム情報をセットする
        /// 何回もやり取りするプロトコルになっている
        private void ComSetAItem()
        {
            addLog("Command : set a item " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            var item = new AItem(this.mNetwork.Deserialize<godaiquest.AItem>());
            var itemimage = new ImagePair(this.mNetwork.Deserialize<godaiquest.ImagePair>());

            var result = this.mParent.setAItem(ref item, itemimage, this.mUserID);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
            {
                return;
            }

            this.mNetwork.flush();

			// アイテムの更新日付を更新する
            this.mParent.setItemTimeModified(item.getItemID(), DateTime.Now);

            // ファイル群を受信する
            String strFolder = getItemFolder(item.getItemID());
            if ( Directory.Exists(strFolder))
                Directory.Delete(strFolder, true);
            Directory.CreateDirectory(strFolder);
            Network.receiveFiles(this.mNetwork, strFolder);

            this.mNetwork.Serialize(item.getSerialize());
        }

		// コマンド：アイテム一覧（特定のユーザの）を得る
		private void ComGetItemInfoByUserId ()
		{
			addLog ("Command : get item info "+ mMail );
			int nVersion = mNetwork.receiveDWORD();
			int nUserId = mNetwork.receiveDWORD();

            ItemInfo iteminfo;
            var result = this.mParent.getItemInfoByUserId(out iteminfo, nUserId);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
            {
                return;
            }

            this.mNetwork.Serialize(iteminfo.getSerialize());
		}

        // コマンド：アイテム一覧(特定ユーザの），更新日付付き
        private void ComGetItemInfo2ByUserId()
        {
			addLog ("Command : get item info2 "+ mMail );
			int nVersion = mNetwork.receiveDWORD();
			int nUserId = mNetwork.receiveDWORD();

            ItemInfo iteminfo;
            var result = this.mParent.getItemInfoByUserId(out iteminfo, nUserId);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
            {
                return;
            }
            ItemInfo2 iteminfo2;
            this.mParent.convToItemInfo2(out iteminfo2, iteminfo);

            this.mNetwork.Serialize(iteminfo2.getSerialize());

        }

        /// コマンド：アイテム一覧を得る
        private void ComGetItemInfo()
        {
            addLog("Command : get item info  " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            ItemInfo iteminfo;
            var result = this.mParent.getItemInfo(out iteminfo);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
            {
                return;
            }

            this.mNetwork.Serialize(iteminfo.getSerialize());
        }

        /// コマンド：アイテム情報を得る
        private void ComGetAItem()
        {
            addLog("Command : get aitem " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            int nItemID = this.mNetwork.receiveDWORD();

            var listAlreadyFile = new List<Network.LocalFileInfo>();
			if ( nVersion >= 2 )
			{
			    listAlreadyFile = Network.receiveLocalFilesInfo(mNetwork, getItemFolder(nItemID));
			}

            Network.sendFiles(this.mNetwork, getItemFolder(nItemID), listAlreadyFile);

            // アイテムの所有者を探す
            int nItemOwner = this.mParent.getItemOwner(nItemID); 
            if (nItemOwner == this.mUserID)
                return; // 所有者が自分のときは何もしない

            // すでに見ているかを調べる
            if (this.mParent.isAlreadyPickedupItem(nItemID, this.mUserID))
                return; // すでに見ているので何もしない

            // 見たことを記録する
            this.mParent.setPickedupItem(this.mUserID, nItemID);

            // 経験値を増やす 他人のアイテムを見た
            this.mParent.incrementExpValue(this.mUserID, 10);

            // 経験値を増やす　オーナー　見てくれたことによる
            this.mParent.incrementExpValue(nItemOwner, 1);


            // ログを作る
            UserInfo userinfo;
            this.mParent.getUserInfo(out userinfo);

            var user = userinfo.getAUser(this.mUserID);
            String strLog1 = user.getName()+"があなたの新情報を見ました";
            this.mParent.addAshiatoLog(nItemOwner, strLog1);

            // 通知をする
            this.mParent.makeSignal(new Signal(SignalType.RefreshExpValue), this.mUserID);
            this.mParent.makeSignal(new Signal(SignalType.RefreshExpValue), nItemOwner);
        }

        /// コマンド：ユーザの情報を変更する
        private void ComSetAUser()
        {
            addLog("Command : set a user " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            var user = new AUser(this.mNetwork.Deserialize<godaiquest.AUser>());

            var result = this.mParent.setAUser(user);
            this.mNetwork.sendDWORD((int)result);
            return;                
        }

        // コマンド：アイテム情報を変更する
        public void ComChangeAItem()
        {
            addLog("Command : change a item " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            var item = new AItem(this.mNetwork.Deserialize<godaiquest.AItem>());

            // 中でアイテムの拾い上げログをけしている。これで経験値が再び入るようになる。
            var result = this.mParent.changeAItem(item);
            this.mNetwork.sendDWORD((int)result);

            // 更新日付を記録する
            this.mParent.setItemTimeModified(item.getItemID(), DateTime.Now);

            // ログを作る
            UserInfo userinfo;
            this.mParent.getUserInfo(out userinfo);
            var creater = userinfo.getAUser(this.mUserID);
            foreach (var user in userinfo)
            {
                if ( user.getUserID() == this.mUserID )
                    continue;

                String strLog1 = creater.getName() + "が情報を更新しました";
                this.mParent.addAshiatoLog(user.getUserID(), strLog1);
            }
            return;
        }

        // コマンド：アイテムファイルをアップロードする
        public void ComUploadItemFiles()
        {
            addLog("Command : upload item files " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            int nItemID = this.mNetwork.receiveDWORD();
            int nDeleteMoto = this.mNetwork.receiveDWORD();

            // TODO:ユーザに権限があるかのチェック

            String strItemFolder = this.getItemFolder(nItemID);

            if (nDeleteMoto != 0 )
            {
				if ( File.Exists(strItemFolder) )
	                Directory.Delete(strItemFolder, true);
            }

            Network.receiveFiles(this.mNetwork, strItemFolder);

            this.mNetwork.sendDWORD((int)EServerResult.SUCCESS);

            return;
        }

        // コマンド：ポーリング
        public void ComPolling()
        {
            int nVersion = this.mNetwork.receiveDWORD();

            // ユーザの位置情報を得る
            var locSelf = new ALocation(this.mNetwork.Deserialize<godaiquest.ALocation>());
            _locationSelf = locSelf;
            this.mParent.setALocation(this.mUserID, locSelf);
            this.mNetwork.sendDWORD((int)EServerResult.SUCCESS);

            // 位置情報を送信する
            LocationInfo locInfo;
            this.mParent.getLocationInfo(out locInfo);
            this.mNetwork.Serialize(locInfo.getSerialize());

            // ログインユーザを送る
            var listLogin = this.mParent.getLoginUserList();
            this.mNetwork.sendLength(listLogin.Count);
            foreach (var nUserID in listLogin.Keys )
            {
                this.mNetwork.sendDWORD(nUserID);
            }

			// モンスタ情報を送る
            var listMonsterPosition = _monster.GetRealMonsterLocationInfo();
			this.mNetwork.Serialize(listMonsterPosition.getSerialize());

            // シグナル情報を送る
            MemoryStream memorySignal = new MemoryStream();
            lock (this.mSignalQueue)
            {
                this.mNetwork.Serialize(mSignalQueue.getSerialize());
                this.mSignalQueue.clear();
            }
        }

        // コマンド：大陸領土情報の取得
        public void ComGetIslandGroundInfo()
        {

            int nVersion = this.mNetwork.receiveDWORD();

            IslandGroundInfo groundinfo;
            var result = this.mParent.getIslandGroundInfo(out groundinfo);
            this.mNetwork.sendDWORD((int)result);

            if (result != EServerResult.SUCCESS)
                return;

            this.mNetwork.Serialize(groundinfo.getSerialize());

            return;
        }

        // コマンド：オブジェクトを変更する
        public void ComChangeObjectAttr()
        {
            addLog("Command : change object attr " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            var obj = new ObjectAttr(this.mNetwork.Deserialize<godaiquest.ObjectAttr>());

            var result = this.mParent.changeObjectAttr(obj);
            this.mNetwork.sendDWORD((int)result);
        }

        /// コマンド：イメージを変更する
        public void ComChangeDungeonBlockImagePair()
        {
            addLog("Command : change a dungeon block image pair " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            var imagepair = new ImagePair(this.mNetwork.Deserialize<godaiquest.ImagePair>());

            var result = this.mParent.changeDugeonBlockImagePair(imagepair);
            this.mNetwork.sendDWORD((int)result);
        }

        /// コマンド：位置情報を得る
        public void ComGetLocationInfo()
        {
            addLog("Command : get location info " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            LocationInfo locinfo;
            var result = this.mParent.getLocationInfo(out locinfo);

            this.mNetwork.sendDWORD((int)result);

            if ( result != EServerResult.SUCCESS )
                return;

            this.mNetwork.Serialize(locinfo.getSerialize());

            return;
        }

        /// コマンド：メッセージを得る
        public void ComGetMessageInfo()
        {
            addLog("Command : get message info " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            MessageInfo mesinfo;
            var result = this.mParent.getMessageInfo(out mesinfo);

            this.mNetwork.sendDWORD((int)result);

            if (result != EServerResult.SUCCESS)
                return;

            this.mNetwork.Serialize(mesinfo.getSerialize());

            return;
        }

        // コマンド：メッセージを設定する
        public void ComSetAMessage()
        {
            addLog("Command : get message info " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();

            var mes = new AMessage(this.mNetwork.Deserialize<godaiquest.AMessage>());
            var result = this.mParent.setAMessage(mes);

            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
                return;

            if (_locationSelf != null)
            {
				// モンスターへの呪文攻撃の判定
                _monster.AttackMonster(mes.getMessage(), _locationSelf);
            }

            this.mParent.makeSignalAllUser(new Signal(SignalType.RefreshMessage));
        }

        // コマンド：経験値を得る
        public void ComGetExpValue()
        {
            addLog("Command : get expvalue " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            this.mNetwork.sendDWORD((int)EServerResult.SUCCESS);

            var expvalue = this.mParent.getExpValue(this.mUserID);
            this.mNetwork.sendDWORD(expvalue.getExpValue());
            this.mNetwork.sendDWORD(expvalue.getTotalValue());
        }

        // コマンド：指定ダンジョンのまだ見ていないアイテムID一覧を得る
        public void ComGetUnpickedupItemInfo()
        {

            addLog("Command : get unpickedup Item Info " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();
            int nUserID = this.mNetwork.receiveDWORD();
            int nDungeonID = this.mNetwork.receiveDWORD();

            this.mNetwork.sendDWORD((int)EServerResult.SUCCESS);

            HashSet<int> setItemID;
            this.mParent.getUnpickedupItemInfo(out setItemID, nUserID, nDungeonID);
			this.mNetwork.sendLength(setItemID.Count);
			foreach (var nItemID in setItemID)
			{
                this.mNetwork.sendDWORD(nItemID);
			}
        }

        // コマンド：足跡を得る
        public void ComGetAshiato()
        {
            addLog("Command : get ashiato " + this.mMail);

            int nVersion = this.mNetwork.receiveDWORD();
            int nItemID = this.mNetwork.receiveDWORD();

            PickupedInfo pickinfo;
            var result = this.mParent.getAshiato(out pickinfo, nItemID);
            this.mNetwork.sendDWORD((int)result);

            if (result != EServerResult.SUCCESS)
                return;

            this.mNetwork.Serialize(pickinfo.getSerialize());
        }

        // コマンド：足あとログを得る
        public void ComGetAshiatoLog()
        {
            addLog("Command : get ashiato log " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            List<String> listLog = new List<String>();
            var result = this.mParent.getAshiatoLog(out listLog, this.mUserID);
            this.mNetwork.sendDWORD((int)result);

            if (result != EServerResult.SUCCESS)
                return;

            var logtmp = new godaiquest.Ashiatolog();
			foreach (var alog in listLog)
			{
                logtmp.alog.Add(alog);
			}
            this.mNetwork.Serialize(logtmp);
        }

        // コマンド：Articleを得る
        public void ComGetArticleString()
        {
            addLog("Command : get article string " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            int nItemID = this.mNetwork.receiveDWORD();

            String strResult;
            var result = this.mParent.getArticleString(out strResult, nItemID);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
                return;

            this.mNetwork.sendString(strResult);
        }

        // コマンド：Articleを書き込む
        public void ComSetItemArticle()
        {
            addLog("Command : set item article " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            var article = new ItemArticle(this.mNetwork.Deserialize<godaiquest.ItemArticle>());

            var result = this.mParent.setItemAritcle(article);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
                return;

        }

        // コマンド：Articleを読んだ
        public void ComReadArticle()
        {
            addLog("Command : read article " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            int nItemID = this.mNetwork.receiveDWORD();
            int nUserID = this.mNetwork.receiveDWORD();
            var result = this.mParent.readArticle(nItemID, nUserID);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
                return;

        }

        // コマンド：Articleを削除する
        public void ComDeleteLastItemArticle()
        {
            addLog("Command : delete last item article " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            int nItemID = this.mNetwork.receiveDWORD();
            int nUserID = this.mUserID;

            var result = this.mParent.deleteLastItemArticle(nItemID, this.mUserID);
            this.mNetwork.sendDWORD((int)result);
        }

        // コマンド：経験値を使う
        public void ComUseExperience()
        {
            addLog("Command : use experience " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();

            EUseExp eUseExpType = (EUseExp)this.mNetwork.receiveDWORD();
            int nDungeonNumber = this.mNetwork.receiveDWORD();

            var result = this.mParent.useExperience(this.mUserID, nDungeonNumber, eUseExpType);
            this.mNetwork.sendDWORD((int)result);
        }

        // コマンド：ダンジョンの深さを得る
        public void ComGetDungeonDepth()
        {
            addLog("Command : get dungeon depth " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();
            int nDungeonID = this.mNetwork.receiveDWORD();

            int nDepth = this.mParent.getDungeonDepth(nDungeonID);
            this.mNetwork.sendDWORD((int)EServerResult.SUCCESS);
            this.mNetwork.sendDWORD(nDepth);
        }

        // コマンド：モンスタかの一覧を返す
        public void ComGetMonsterInfo()
        {
            addLog("Command : get monster info " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();
            int nDungeonID = this.mNetwork.receiveDWORD();

            MonsterInfo monsterinfo;
            var result = this.mParent.getMonsterInfo(out monsterinfo, nDungeonID);
            this.mNetwork.sendDWORD((int)result);

            this.mNetwork.Serialize(monsterinfo.getSerialize());
        }

        // コマンド：モンスタかを設定する
        public void ComSetMonster()
        {
            addLog("Command : set Monster " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();
            int nItemID = this.mNetwork.receiveDWORD();
            bool bMonster = this.mNetwork.receiveDWORD() != 0;

            this.mParent.setMonster(nItemID, bMonster);
            this.mNetwork.sendDWORD((int)EServerResult.SUCCESS);
        }

        // コマンド：ランダムダンジョンの読み込み済み一覧を得る
        public void ComGetRDReadItemInfo()
        {
            addLog("Command : get RD RedItemInfo " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();
//            int nUserID = this.mNetwork.receiveDWORD();
            int nUserID = this.mUserID;

            RDReadItemInfo readiteminfo;
            var result = this.mParent.getRDReadItemInfo(out readiteminfo, nUserID);
            this.mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
                return;

            this.mNetwork.Serialize(readiteminfo.getSerialize());
        }

        // コマンド：ランダムダンジョンの読み込み済みを設定する
        public void ComSetRDReadItem()
        {

            addLog("Command : set RD Read Item " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();
            int nUserID = this.mUserID;
//            int nUserID = this.mNetwork.receiveDWORD();
            int nItemID = this.mNetwork.receiveDWORD();

            var result = this.mParent.setRDReadItem(nUserID, nItemID);
            this.mNetwork.sendDWORD((int)result);
        }

        // コマンド：パスワードを変更する
        public void ComChangePassword()
        {
            addLog("Command : change password " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();
            String strNewPasswordHash = this.mNetwork.receiveString();

            var result = this.mParent.changePassword(this.mUserID, strNewPasswordHash);
            this.mNetwork.sendDWORD((int)result);
        }

        // コマンド：ユーザフォルダを設定する
        public void ComSetUserFolder()
        {
            addLog("Command : set User Folder " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();
            int nUserID = this.mNetwork.receiveDWORD();
            String strComputerName = this.mNetwork.receiveString();
            String strFolder = this.mNetwork.receiveString();

            var result = this.mParent.setUserFolder(nUserID, strComputerName, strFolder);
            this.mNetwork.sendDWORD((int)result);
        }

        // コマンド：ユーザファルダを得る
        public void ComGetUserFolder()
        {
            addLog("Command : Get User Folder " + this.mMail);
            int nVersion = this.mNetwork.receiveDWORD();
            int nUserID = this.mNetwork.receiveDWORD();
            String strComputerName = this.mNetwork.receiveString();

            var strFolder = this.mParent.getUserFolder(nUserID, strComputerName);
            this.mNetwork.sendDWORD((int)EServerResult.SUCCESS);

            this.mNetwork.sendString(strFolder == null ? "" : strFolder);
        }

		// コマンド：モンスターの元の情報を得る
        public void ComGetRealMonsterSrcInfo()
        {
            addLog("Command : Get RealMonsterSrcInfo");
            int nVersion = mNetwork.receiveDWORD();
			mNetwork.sendDWORD((int)EServerResult.SUCCESS);
			mNetwork.Serialize(_monster.GetRealMonsterSrcInfo().getSerialize());
        }

		// コマンド：キーワードを登録する
        public void ComRegisterKeyword()
        {
            addLog("Command : Register Keyword");
            int nVersion = mNetwork.receiveDWORD();
            int nUserID = mUserID;
            string keyword = mNetwork.receiveString();
            int nKeywordPriority = mNetwork.receiveDWORD();
            int nKeywordID = 0;
            var result = mParent.registerKeyword(out nKeywordID, nUserID, keyword, nKeywordPriority);
			mNetwork.sendDWORD((int)result);
            if (result != EServerResult.SUCCESS)
                return;
            mNetwork.sendDWORD(nKeywordID);
        }

		// コマンド：キーワードを変更する
        public void ComModifyKeyword()
        {
            addLog("Command : Modify Keyword");
            int nVersion = mNetwork.receiveDWORD();
            int nUserID = mUserID;
            int nKeywordID = mNetwork.receiveDWORD();
            string newKeyword = mNetwork.receiveString();
		
            var result = mParent.modifyKeyword(nUserID, nKeywordID, newKeyword);
			mNetwork.sendDWORD((int)result);
            
        }

		// コマンド：キーワードの優先順位を変更する
        public void ComModifyKeywordPriority()
        {
            addLog("Command : Modify Keyword");
            int nVersion = mNetwork.receiveDWORD();
            int nUserID = mUserID;
            int nKeywordID = mNetwork.receiveDWORD();
            int nNewPriority = mNetwork.receiveDWORD();
		
            var result = mParent.modifyKeywordPriority(nUserID, nKeywordID, nNewPriority);
			mNetwork.sendDWORD((int)result);
        }

		// コマンド：キーワードにアイテムを割り当てる
        public void ComAttachKeyword()
        {
            addLog("Command : Attach Keyword");
            int nVersion = mNetwork.receiveDWORD();
            int nKeywordID = mNetwork.receiveDWORD();
            int nItemID = mNetwork.receiveDWORD();
            int nItemPriority = mNetwork.receiveDWORD();
		
            var result = mParent.attachKeyword( nKeywordID, nItemID, nItemPriority);
			mNetwork.sendDWORD((int)result);
        }

		// コマンド：キーワードに割り当てたアイテムを外す
        public void ComDetachKeyword()
        {
            addLog("Command : Detach Keyword");
            int nVersion = mNetwork.receiveDWORD();
            int nKeywordID = mNetwork.receiveDWORD();
            int nItemID = mNetwork.receiveDWORD();
		
            mParent.detachKeyword(nKeywordID, nItemID);
			mNetwork.sendDWORD((int)EServerResult.SUCCESS);
        }

		// コマンド：キーワード一覧を得る
        public void ComListKeyword()
        {
            addLog("Command : List Keyword");
            int nVersion = mNetwork.receiveDWORD();
            int nUserID = mNetwork.receiveDWORD(); //自分以外も見たいので 
		
            var result = mParent.listKeyword(nUserID);
			mNetwork.sendDWORD((int)EServerResult.SUCCESS);

            var keywordInfo = new KeywordUserInfo();
            foreach (var akeyworddb in result)
            {
                keywordInfo.addKeyword(new AKeyword(akeyworddb.getKewordID(), akeyworddb.getKeyword(), akeyworddb.getKeywordPriority()));
            }
            mNetwork.Serialize(keywordInfo.getSerialize());
        }

		// コマンド：キーワードの詳細を得る
        public void ComGetKeywordDetail()
        {
            addLog("Command : Get Keyword Detail");
            int nVersion = mNetwork.receiveDWORD();
            int nKeywordID = mNetwork.receiveDWORD();

            var result = mParent.getKeywordDetail(nKeywordID);
			mNetwork.sendDWORD((int)EServerResult.SUCCESS);

            var keyword = new AKeyword();
            foreach (var akeyworddb in result) 
            {
                keyword.addKeywordItem(new AKeywordItem(akeyworddb.getItemID(), akeyworddb.getItemPriority()));
            }
			mNetwork.Serialize(keyword.getSerialize());
        }

		// コマンド：キーワードのアイテムの優先順位を変える
        public void ComModifyKeywordItemPriority()
        {
            addLog("Command : Modify Keyword Item Priority");
            int nVersion = mNetwork.receiveDWORD();
            int nKeywordID = mNetwork.receiveDWORD();
            int nItemID = mNetwork.receiveDWORD();
            int nNewPriority = mNetwork.receiveDWORD();

            var result = mParent.modifyKeywordItemPriority(nKeywordID, nItemID, nNewPriority);
			mNetwork.sendDWORD((int)result);
        }

		// コマンド：キーワードを削除する
        public void ComDeleteKeyword()
        {
            addLog("Command : Delete Keyword");
            int nVersion = mNetwork.receiveDWORD();
            int nKeywordID = mNetwork.receiveDWORD();

            var result = mParent.deleteKeyword(mUserID, nKeywordID);
			mNetwork.sendDWORD((int)result);
        }
    }
}

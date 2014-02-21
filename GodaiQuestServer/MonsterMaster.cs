using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GodaiLibrary;
using GodaiLibrary.GodaiQuest;
using godaiquest;
using ARealMonsterLocation = GodaiLibrary.GodaiQuest.ARealMonsterLocation;
using LocationInfo = GodaiLibrary.GodaiQuest.LocationInfo;
using RealMonsterInfo = GodaiLibrary.GodaiQuest.RealMonsterInfo;
using RealMonsterLocationInfo = GodaiLibrary.GodaiQuest.RealMonsterLocationInfo;

/*
 * モンスタ管理クラス
 */

namespace GodaiQuestServer
{
    public class MonsterMaster
    {
        private const int MAX_MONSTER = 100;	// モンスターの最大数

        private ServerWorker _parent;
		// 可能なモンスター一覧
		private RealMonsterInfo _listAvailableMonster = new RealMonsterInfo();

		// 現在生きているモンスタ
		//private RealMonsterInfo _listLiveMonster = new RealMonsterInfo();
        private RealMonsterLocationInfo _listLiveMonsterLocation = new RealMonsterLocationInfo();

        private int _monsterId = 1;	// Id 0はデータ内のモンスタ
        private int _targetUesr;	// モンスターがターゲットとするユーザID

        private Random _rand = new Random();

		// 排他処理用
        private Object _cs = new Object();

        public MonsterMaster(ServerWorker worker)
        {
            _parent = worker;
        }

        public void Run()
        {
            if (!ReadInitFile())
            {
                addLog("monster.txtが読めませんでした");
				return;
            }

            _targetUesr = 0;

            while (true)
            {
				Thread.Sleep(5000);      

				// モンスターの生成
				MakeMonster();
                
                // モンスターを動かす
                LocationInfo userLocation;
                if (_parent.getLocationInfo(out userLocation) == EServerResult.SUCCESS)
                {

					// たまに狙いを変える
                    if (_rand.NextDouble() < 0.01)
                    {
                        _targetUesr = _rand.Next(0, userLocation.Count());
                    }

                    MoveMonster(userLocation);
                }
            }    
        }

		// モンスターの位置情報を得る
        public RealMonsterLocationInfo GetRealMonsterLocationInfo()
        {
            lock (_cs)
            {
                return new RealMonsterLocationInfo(_listLiveMonsterLocation);
            }
        }

		// モンスターの情報を得る
        public RealMonsterInfo GetRealMonsterSrcInfo()
        {
            lock (_cs)
            {
                return new RealMonsterInfo(_listAvailableMonster);
            }
        }

		// モンスターへの攻撃検出
        public void AttackMonster(string strMes, GodaiLibrary.GodaiQuest.ALocation locSelf)
        {
            if (locSelf.getDungeonUserID() != 0)
                return;	// 大陸以外では未サポート

            var listDestroy = new List<ARealMonsterLocation>();

            lock (_cs)
            {
                foreach (var amon in _listLiveMonsterLocation)
                {
                    int dx = Math.Abs(amon.MonsterIx - locSelf.getIX());
                    int dy = Math.Abs(amon.MonsterIy - locSelf.getIY());
                    if (Math.Max(dx, dy) > 5)
                        continue;	// 遠すぎる

                    int nSrcId = amon.MonsterSrcId;
                    var monsrc = _listAvailableMonster[nSrcId];
                    if (strMes.Contains(monsrc.Spell))
                    {
                        // 退治
						listDestroy.Add(amon);
                    }
                }
                foreach (var amon in listDestroy)
                {
                    _listLiveMonsterLocation.Remove(amon);
                }
            }

			// モンスタの削除処理継続
            foreach (var amon in listDestroy)
            {
                // 経験値の増加
                var monsrc = _listAvailableMonster[amon.MonsterSrcId];
                int expvalue = monsrc.ExpValue;
				_parent.incrementExpValue(locSelf.getUserID(), expvalue);

				// モンスタ削除イベントの発生
                var sig = new GodaiLibrary.GodaiQuest.Signal(SignalType.DestroyMonster);
                sig.ID = monsrc.ID;
                sig.Ix = amon.MonsterIx;
                sig.Iy = amon.MonsterIy;
                _parent.makeSignalAllUser(sig);
            }
            if (listDestroy.Count > 0)
            {
                _parent.makeSignal(new GodaiLibrary.GodaiQuest.Signal(SignalType.RefreshExpValue), locSelf.getUserID());
            }
        }

		// モンスターの作成
        private void MakeMonster()
        {
            lock (_cs)
            {
                if (_listAvailableMonster.size() == 0)
                    return;

                int nLiveMonster = _listLiveMonsterLocation.size();
                int nMakeMonsterCnt = MAX_MONSTER - nLiveMonster;
                if (nMakeMonsterCnt <= 0)
                    return;

                int nMaxSize = MongoMaster.GetIslandSize();

                for (int it = 0; it < nMakeMonsterCnt; ++it)
                {
                    int ix = _rand.Next(0, nMaxSize/4);
                    int iy = _rand.Next(0, nMaxSize/4);
                    int nMonsterType = _rand.Next(0, _listAvailableMonster.size());

                    // 元となるモンスター情報
                    var monsterSrc = _listAvailableMonster[nMonsterType];

                    // 新規モンスタの作成
//                    var new_monster = new RealMonster(_monsterId, monsterSrc.ID, monsterSrc.Name,
//                        monsterSrc.MonsterImage, monsterSrc.ExpValue, monsterSrc.Spell);
//                    _listLiveMonster.addRealMonster(new_monster);

                    // モンスタの位置を設定
                    var location = new ARealMonsterLocation();
                    location.MonsterId = _monsterId++;
                    location.MonsterIx = ix;
                    location.MonsterIy = iy;
                    location.MonsterSrcId = nMonsterType;
                    _listLiveMonsterLocation.addRealMonsterLocation(location);
                }
            }
        }

		// モンスターを動かす
        private void MoveMonster(LocationInfo locinfo)
        {
            lock (_cs)
            {
                var listLocation = locinfo.ToArray();
                if (listLocation.Count() == 0)
                    return;

                var targetUesr = listLocation[_targetUesr];

                for (int it = 0; it < _listLiveMonsterLocation.size(); ++it)
                {
                    var amonloc = _listLiveMonsterLocation[it];

                    bool bRandom = _rand.NextDouble() < 0.3;
                    if (bRandom)
                    {
						// ランダムで移動
                        int dix = _rand.Next(-1, 1 + 1);
                        int diy = _rand.Next(-1, 1 + 1);
                        TryMonsterMove(ref amonloc, dix, diy);
                    }
                    else
                    {
                        // ユーザに向かって突撃
                        int dix = targetUesr.getIX() - amonloc.MonsterIx;
                        int diy = targetUesr.getIY() - amonloc.MonsterIy;
                        if (dix > diy)
                        {
                            diy = 0;
                        }
                        else
                        {
                            dix = 0;
                        }
                        dix = dix == 0 ? 0 : dix > 0 ? 1 : -1;
                        diy = diy == 0 ? 0 : diy > 0 ? 1 : -1;

                        TryMonsterMove(ref amonloc, dix, diy);
                    }
                }
            }           
        }

        /// モンスターが動けるかを判定しOKならば動かす
        private bool TryMonsterMove(ref ARealMonsterLocation amonloc, int dix, int diy)
        {
            int newix = dix + amonloc.MonsterIx;
            int newiy = diy + amonloc.MonsterIy;
            if (newix < 0 || newiy < 0 || newix >= MongoMaster.GetIslandSize() || newiy >= MongoMaster.GetIslandSize())
                return false;

            amonloc.MonsterIx = newix;
            amonloc.MonsterIy = newiy;
            return true;
        }

		// ログ出力
        private void addLog(String strLog)
        {
            _parent.addLog(strLog);
        }

		// 設定ファイルの読み込み
        private bool ReadInitFile()
        {
            string strDir = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string strPath = Path.Combine(strDir, "Monster.txt");

			// CSV形式とする
            int nCntMonster = 0;
            using (var loader = new CSVLoader())
            {
                if (!loader.OpenFile(strPath, Encoding.UTF8))
                    return false;

                while (loader.ReadLine())
                {
                    string strMonsterName = loader.GetString();
                    string strImagePath = loader.GetString();
                    int nExpValue = loader.GetInt();
                    string strSpell = loader.GetString();

					strImagePath = strImagePath.Replace("\\", ""+Path.DirectorySeparatorChar);

                    Image image;
                    try
                    {
                       image = Image.FromFile(Path.Combine(strDir, strImagePath));
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                    var mon = new RealMonster(nCntMonster, -1, strMonsterName, image, nExpValue, strSpell);
                    _listAvailableMonster.addRealMonster(mon);
                }
            }
			addLog("Number of real monster : " + (""+_listAvailableMonster.size()));
            return true;
        }
    }
}

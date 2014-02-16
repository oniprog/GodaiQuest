using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
                    int ix = _rand.Next(0, nMaxSize);
                    int iy = _rand.Next(0, nMaxSize);
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
            string strPath = Path.Combine(strDir, "monster.txt");

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuestServer
{

    public class NPCWorker
    {
        private ServerWorker mParent;
        private Random rand = new Random();

        public NPCWorker(ServerWorker parent)
        {
            this.mParent = parent;
        }

        public void Run()
        {
            Thread.Sleep(10 * 1000);
            bool bFirstFlag = true;
            while (true)
            {
                // 時間待ち
                if (bFirstFlag)
                    bFirstFlag = false;
                else
                    Thread.Sleep(3 * 60 * 60 * 1000);   // ３時間毎に活動する

                // ランダムダンジョンを作成する
                ulong[] dungeon;
                int nSizeX = 100;
                int nSizeY = 100;
                if (!this.makeDungeon(out dungeon, nSizeX, nSizeY))
                {
                    this.mParent.addLog("can' create dungeon by NPC");
                    continue;
                }
                this.mParent.setDungeonByNPC(dungeon, nSizeX, nSizeY, 0x40000000, 0 );

                // ダンジョンの位置を大陸に登録する
                if (!this.makeDungeonEnter())
                {
                    this.mParent.addLog("can't create dungeon enterance by NPC");
                    continue;
                }

                // 読み込み済みアイテム一覧表をクリアする
                this.mParent.clearRDReadItemInfo();

                // ダンジョンの変更を通知する
                this.mParent.makeSignal(Signal.RefreshDungeon);

                this.mParent.addLog("New Random dungeon is created.");
            }
        }

        /// ダンジョンを作成する
        public bool makeDungeon(out ulong[] dungeon, int nSizeX, int nSizeY)
        {
            dungeon = new ulong[nSizeX * nSizeY];

            // タイルを得る
            TileInfo tileinfo;
            this.mParent.getTileList(out tileinfo);
            ObjectAttrInfo objectinfo;
            this.mParent.getObjectAttrInfo(out objectinfo);

            Tile tileWall = null;
            Tile tileOut = null;
            Tile tileIn = null;

            foreach (var tile in tileinfo)
            {
                var obj = objectinfo.getObject((int)tile.getObjectID());
                if (!obj.canWalk() && tileWall == null )
                {
                    tileWall = tile;
                }
                else if (obj.getObjectCommand() == EObjectCommand.IntoDungeon && tileIn == null )
                {
                    tileIn = tile;
                }
                else if (obj.getObjectCommand() == EObjectCommand.GoOutDungeon && tileOut == null)
                {
                    tileOut = tile;
                }
            }

            if (tileWall == null || tileIn == null || tileOut == null)
                return false;

            for (int iy = 0; iy < nSizeY; ++iy)
            {
                for (int ix = 0; ix < nSizeX; ++ix)
                {
                    dungeon[ix + iy * nSizeX] = tileWall.getTileID();
                }
            }

            int iCurX = rand.Next() % nSizeX;
            int iCurY = rand.Next() % nSizeY;

            List<DungeonDir> listCur = new List<DungeonDir>();
            listCur.Add(new DungeonDir(new Point(iCurX, iCurY)));

            Point[] poiDir = { new Point(-2, 0), new Point(2, 0), new Point(0, 2), new Point(0, -2) };

            while (listCur.Count > 0)
            {
                DungeonDir duncur = listCur[listCur.Count - 1];
                listCur.RemoveAt(listCur.Count - 1);

                List<int> listNext = new List<int>();
                listNext.Add(0);
                listNext.Add(1);
                listNext.Add(2);
                listNext.Add(3);

//                while (listNext.Count > 0)
                for (int il = 0; il < 2 && listNext.Count > 0 ; ++il )
                {
                    int id = -1;
                    if (this.rand.Next() % 10 >= 1)
                    {
                        // 可能ならば前回と同じ方向に進む
                        for (int it = 0; it < listNext.Count; ++it)
                        {
                            if (listNext[it] == duncur.Dir)
                            {
                                id = duncur.Dir;
                                listNext.RemoveAt(it);
                                break;
                            }
                        }
                    }
                    // まだ見つかっていないとき
                    if (id < 0)
                    {
                        int nRand = this.rand.Next();
                        id = listNext[nRand % listNext.Count];
                        listNext.RemoveAt(nRand % listNext.Count);
                    }

                    Point poiCur = duncur.Pos;
                    Point poiNext = new Point(poiCur.X + poiDir[id].X, poiCur.Y + poiDir[id].Y);
                    if (poiNext.X < 0 || poiNext.X >= nSizeX)
                        continue;
                    if (poiNext.Y < 0 || poiNext.Y >= nSizeY)
                        continue;
                    Tile tile = new Tile(dungeon[poiNext.X + poiNext.Y * nSizeX]);
                    if (tile.getImageID() != 0) {

                        // 次の検索場所にする
                        listCur.Add(new DungeonDir(poiNext, id));
                    }
                    else 
                        continue;

                    // 道をつなげる
                    dungeon[poiCur.X + poiCur.Y * nSizeX] = 0;
                    dungeon[poiNext.X + poiNext.Y * nSizeX] = 0;
                    dungeon[poiCur.X + poiDir[id].X / 2 + (poiCur.Y + poiDir[id].Y / 2) * nSizeX] = 0;
                }                
            }

            // 乱数で壁を壊す
            for (int icrash = 0; icrash < 1000; ++icrash)
            {
                int ix = (this.rand.Next() % (nSizeX - 2)) + 1;
                int iy = (this.rand.Next() % (nSizeY - 2)) + 1;

                if (dungeon[ix + iy * nSizeX] == 0 )
                    continue;
                bool bOK = false;
                if (dungeon[(ix - 1) + iy * nSizeX] == 0 && dungeon[(ix + 1) + iy * nSizeX] == 0)
                    bOK = true;
                if (dungeon[ix + (iy - 1) * nSizeX] == 0 && dungeon[ix + (iy + 1) * nSizeX] == 0)
                    bOK = true;
                if (bOK)
                {
                    dungeon[ix + iy * nSizeX] = 0;
                }
            }

            // 外への出口を配置する
            while (true)
            {
                int ix = this.rand.Next() % nSizeX;
                int iy = this.rand.Next() % nSizeY;
                if (dungeon[ix + iy * nSizeX] == 0)
                {
                    dungeon[ix + iy * nSizeX] = tileOut.getTileID();
                    break;
                }
            }

            // アイテムを配置する
            ItemInfo iteminfo;
            this.mParent.getItemInfo(out iteminfo);

            foreach(var item in iteminfo)
            {
                bool bMonster = this.rand.Next() % 100 >= 20;    // 8割がモンスターとする
                int ix, iy;
                while (true)
                {
                    ix = (this.rand.Next() % (nSizeX-2))+1;
                    iy = (this.rand.Next() % (nSizeY-2))+1;

                    if (bMonster)
                    {
                        // 出現場所は壁で、周囲に空きが必要
                        if (dungeon[ix + iy * nSizeX] != tileWall.getTileID())
                            continue;
                        if (
                            dungeon[(ix - 1) + (iy) * nSizeX] != 0 && dungeon[(ix + 1) + (iy) * nSizeX] != 0 &&
                            dungeon[(ix) + (iy - 1) * nSizeX] != 0 && dungeon[(ix) + (iy + 1) * nSizeX] != 0)
                            continue;

                    }
                    else
                    {
                        // 出現場所は空きである必要がある
                        if ( dungeon[ix+iy*nSizeX] != 0 )
                            continue;
                    }

                    // モンスタかどうかを書き込む
                    this.mParent.setMonsterForRandomDungeon(item.getItemID(), bMonster);

                    // 出現場所に書き込む
                    var nObjectID = objectinfo.findObjectByItemID(item.getItemID());
                    var obj = objectinfo.getObject(nObjectID);

                    Tile tile = new Tile((uint)obj.getObjectID(), (uint)item.getItemImageID());
                    dungeon[ix + iy * nSizeX] = tile.getTileID();

                    break;
                }
            }

            return true;
        }

        // ダンジョンの入口を大陸に作成する
        public bool makeDungeonEnter()
        {
            // タイルを得る
            TileInfo tileinfo;
            this.mParent.getTileList(out tileinfo);
            ObjectAttrInfo objectinfo;
            this.mParent.getObjectAttrInfo(out objectinfo);

            Tile tileIn = null;
            List<Tile> listRandTile = new List<Tile>();
            List<Tile> listCanWalkTile = new List<Tile>();

            foreach (var tile in tileinfo)
            {
                var obj = objectinfo.getObject((int)tile.getObjectID());
                if (obj.getObjectCommand() == EObjectCommand.IntoDungeon && tileIn == null)
                {
                    tileIn = tile;
                }
                else if (obj.getItemID() == 0 && obj.getObjectCommand() == EObjectCommand.Nothing )
                {
                    listRandTile.Add(tile);
                    if (obj.canWalk())
                        listCanWalkTile.Add(tile);
                }
            }
            if( tileIn == null || listCanWalkTile.Count <= 1 )
                return false;

            //
            DungeonInfo dungeon;
            this.mParent.getDungeon(out dungeon, 0, 0);

            IslandGroundInfo groundinfo;
            this.mParent.getIslandGroundInfo(out groundinfo);

            // 既存の入り口を削除する
            // ランダムにオブジェクトを配置する
            for (int iy = 0; iy < dungeon.getSizeY(); ++iy)
            {
                for (int ix = 0; ix < dungeon.getSizeX(); ++ix)
                {
                    int nUserID = groundinfo.getUserIDByCoord(ix, iy);
                    if ( nUserID != 0 )
                        continue;

                    if (dungeon.getDungeonTileAt(ix, iy) == tileIn.getTileID())
                    {
                        dungeon.setDungeonTileAt(ix, iy, 0);
                    }
                    else
                    {
                        int nCommand = this.rand.Next() % 100;
                        if (nCommand == 0 && listRandTile.Count > 0 ) 
                        {
                            int nSelect = this.rand.Next() % listRandTile.Count();
                            dungeon.setDungeonTileAt(ix, iy, listRandTile[nSelect].getTileID());
                        }
                        else
                        {
                            dungeon.setDungeonTileAt(ix, iy, 0);
                        }
                    }
                }
            }

            // 入り口を置く
            while(true) {

                int ix = this.rand.Next() % dungeon.getSizeX();
                int iy = this.rand.Next() % dungeon.getSizeY();

                int nUserID = groundinfo.getUserIDByCoord(ix, iy);
                if (nUserID == 0)
                {
                    dungeon.setDungeonTileAt(ix, iy, tileIn.getTileID());

                    // 道筋を作る
                    int nMiti = (this.rand.Next() % (listCanWalkTile.Count-1))+1;

                    while (ix > 5 || iy > 5 )
                    {
                        if (this.rand.Next() % 2 == 0 && ix > 5 || iy <= 5)
                        {
                            --ix;
                        }
                        else
                        {
                            --iy;
                        }

                        if ((this.rand.Next() % 3) == 0)
                        {
                            if ( groundinfo.getUserIDByCoord(ix,iy) != 0 )
                                continue;

                            if (dungeon.getDungeonTileAt(ix,iy) != 0 )
                                continue;

                            dungeon.setDungeonTileAt(ix, iy, listCanWalkTile[nMiti].getTileID());
                        }
                    }

                    break;
                }
            }

            // 保存
            this.mParent.setDungeonByNPC(dungeon.getDungeon(), dungeon.getSizeX(), dungeon.getSizeY(), 0, 0);

            return true;
        }
    }

    class DungeonDir
    {
        public Point Pos { get; set; }
        public int Dir { get; set; }
        public DungeonDir(Point cur)
        {
            this.Pos = cur;
        }
        public DungeonDir(Point cur, int dir)
        {
            this.Pos = cur;
            this.Dir = dir;
        }
    }
}

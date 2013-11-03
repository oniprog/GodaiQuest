using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    // TileIDはMap登録のたびに自動追加する

    [Serializable()]
    public class Tile
    {
        private ulong mTileID;

        public Tile(uint nObjectID, uint nImageID)
        {
            this.mTileID = ((ulong)nImageID << 32 ) + (ulong)nObjectID;
        }
        public Tile(ulong nTileID_)
        {
            this.mTileID = nTileID_;
        }
        public Tile() { }

        public Tile(godaiquest.Tile tile)
        {
            mTileID = tile.tile_id;
        }

        public godaiquest.Tile getSerialize()
        {
			var ret = new godaiquest.Tile();
            ret.tile_id = mTileID;
            return ret;
        }

        public ulong getTileID()
        {
            return this.mTileID;
        }
        public uint getImageID()
        {
            return (uint)(this.mTileID >> 32);
        }
        public uint getObjectID()
        {
            return (uint)(this.mTileID & 0xffffffff);
        }
    }

    [Serializable()]
    public class TileInfo : IEnumerable<Tile>
    {
        private Dictionary<ulong, Tile> mDicTile = new Dictionary<ulong, Tile>();

        public TileInfo() { }
        public TileInfo(godaiquest.TileInfo info)
        {
			foreach (var tiledic in info.tile_dic)
			{
                mDicTile.Add(tiledic.index, new Tile(tiledic.tile));
			}
        }

        public godaiquest.TileInfo getSerialize()
        {
            var ret = new godaiquest.TileInfo();
			foreach (var tiledic in mDicTile)
			{
				var newtiledic = new godaiquest.TileDic();
				newtiledic.index = tiledic.Key;
				newtiledic.tile = tiledic.Value.getSerialize();
                ret.tile_dic.Add(newtiledic);
			}
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<Tile> GetEnumerator()
        {
            return mDicTile.Values.GetEnumerator();
        }

        public void addTile(Tile tile_)
        {
            this.mDicTile.Add(tile_.getTileID(), tile_);
        }
    }
}

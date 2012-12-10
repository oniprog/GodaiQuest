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
        public Tile() {}

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

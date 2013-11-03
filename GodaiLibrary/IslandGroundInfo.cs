using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class IslandGround
    {
        private int mUserID;
        private int mIx1;
        private int mIx2;
        private int mIy1;
        private int mIy2;

        public IslandGround(int nUserID_, int ix1_, int iy1_, int ix2_, int iy2_)
        {
            this.mUserID = nUserID_;
            this.mIx1 = ix1_;
            this.mIy1 = iy1_;
            this.mIx2 = ix2_;
            this.mIy2 = iy2_;
        }

        public IslandGround(int nUserID_)
        {
            this.mUserID = nUserID_;
        }

        public IslandGround(godaiquest.IslandGround ground)
        {
            mUserID = ground.user_id;
            mIx1 = ground.ix1;
            mIy1 = ground.iy1;
            mIx2 = ground.ix2;
            mIy2 = ground.iy2;
        }

        public godaiquest.IslandGround getSerialize()
        {
            var ret = new godaiquest.IslandGround();
            ret.user_id = mUserID;
            ret.ix1 = mIx1;
            ret.ix2 = mIx2;
            ret.iy1 = mIy1;
            ret.iy2 = mIy2;
            return ret;
        }

        public int getUserID()
        {
            return this.mUserID;
        }
        public int getIx1()
        {
            return this.mIx1;
        }
        public int getIy1()
        {
            return this.mIy1;
        }
        public int getIx2()
        {
            return this.mIx2;
        }
        public int getIy2()
        {
            return this.mIy2;
        }
        public bool contains( int ix, int iy ) {
            return 
                Math.Min(this.mIx1, this.mIx2) <= ix &&
                ix <= Math.Max(this.mIx1, this.mIx2) &&
                Math.Min(this.mIy1, this.mIy2) <= iy &&
                iy <= Math.Max(this.mIy1, this.mIy2);
        }
    }

    [Serializable()]
    public class IslandGroundInfo : IEnumerable<IslandGround>
    {
        private List<IslandGround> mIslandGroundList = new List<IslandGround>();

        public IslandGroundInfo() { }
        public IslandGroundInfo(godaiquest.IslandGroundInfo info)
        {
			foreach (var tmp in info.ground_list)
			{
                mIslandGroundList.Add(new IslandGround(tmp));
            }
        }
        public godaiquest.IslandGroundInfo getSerialize()
        {
            var ret = new godaiquest.IslandGroundInfo();
			foreach (var tmp in mIslandGroundList)
			{
                ret.ground_list.Add(tmp.getSerialize());
			}
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IslandGround> GetEnumerator()
        {
            return this.mIslandGroundList.GetEnumerator();
        }

        public List<IslandGround> getIslandGroundByUserID(int nUserID)
        {
            List<IslandGround> listRet = new List<IslandGround>();
            foreach (var island in this.mIslandGroundList)
            {
                if (island.getUserID() == nUserID)
                {
                    listRet.Add(island);
                }
            }
            return listRet;
        }
        public void addIslandGround(IslandGround island)
        {
            this.mIslandGroundList.Add(island);
        }
        public void removeIslandGround(IslandGround island)
        {
            this.mIslandGroundList.Remove(island);
        }
        public int count()
        {
            return this.mIslandGroundList.Count;
        }

        public int getUserIDByCoord(int ix, int iy ) {

            foreach (var ground in this.mIslandGroundList)
            {
                if (ground.contains(ix, iy))
                {
                    return ground.getUserID();
                }
            }

            return 0;
        }
    }
}

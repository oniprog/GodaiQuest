using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class ALocation
    {
        private int mUserID;

        private int mIX, mIY;
        private int mDungeonUserID;
        private int mDungeonNumber;

        public ALocation(int nUserID_)
        {
            this.mUserID = nUserID_;
            this.mDungeonNumber = 0;
            this.mDungeonUserID = 0;
        }
        public ALocation(int nUserID_, int nIx_, int nIy_, int nDungeonUserID_, int nDungeonNumber_)
        {
            this.mUserID = nUserID_;
            this.mIX = nIx_;
            this.mIY = nIy_;
            this.mDungeonUserID = nDungeonUserID_;
            this.mDungeonNumber = nDungeonNumber_;
        }

        public int getUserID()
        {
            return mUserID;
        }
        public void setUserID(int nUserID)
        {
            this.mUserID = nUserID;
        }
        public int getIX()
        {
            return mIX;
        }
        public int getIY()
        {
            return mIY;
        }
        public void setXY(int ix_, int iy_)
        {
            mIX = ix_; mIY = iy_;
        }

        public int getDungeonUserID()
        {
            return this.mDungeonUserID;
        }
        public int getDungeonNumber()
        {
            return this.mDungeonNumber;
        }
    }

    [Serializable()]
    public class LocationInfo : IEnumerable<ALocation> {

        private Dictionary<int, ALocation> mDicLocation = new Dictionary<int, ALocation>();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<ALocation> GetEnumerator() {
            return this.mDicLocation.Values.GetEnumerator();
        }

        public void addLocation(int nUserID, ALocation loc)
        {
            this.mDicLocation.Add(nUserID, loc);
        }

        public ALocation getLocationByUserID(int nUserID)
        {
            ALocation loc;
            this.mDicLocation.TryGetValue(nUserID, out loc);
            return loc;
        }
    }
}

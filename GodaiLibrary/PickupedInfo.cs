﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class APickuped
    {
        private int mItemID;
        private int mUserID;        // 注意：見た人のIDですよ！
        private DateTime mDateTime;

        public APickuped(int nUserID, int nItemID, DateTime datetime_)
        {
            this.mUserID = nUserID;
            this.mItemID = nItemID;
            this.mDateTime = datetime_;
        }
        public APickuped(godaiquest.APickuped pickuped)
        {
            mUserID = pickuped.user_id;
            mItemID = pickuped.item_id;
            mDateTime = DateTime.FromBinary(pickuped.date_time);
        }
        public godaiquest.APickuped getSerialize()
        {
            var ret = new godaiquest.APickuped();
            ret.user_id = mUserID;
            ret.item_id = mItemID;
            ret.date_time = mDateTime.ToBinary();
            return ret;
        }

        public int getUserID()
        {
            return this.mUserID;
        }
        public int getItemID()
        {
            return this.mUserID;
        }
        public DateTime getDateTime()
        {
            return this.mDateTime;
        }
    }

    [Serializable()]
    public class PickupedInfo : IEnumerable<APickuped>
    {
        private List<APickuped> mPickupedList = new List<APickuped>();

        public PickupedInfo() { }
        public PickupedInfo(godaiquest.PickupedInfo apickup)
        {
            foreach (var tmp in apickup.apickuped) 
			{
                mPickupedList.Add(new APickuped(tmp));
			}
        }
        public godaiquest.PickupedInfo getSerialize()
        {
            var ret = new godaiquest.PickupedInfo();
			foreach (var tmp in mPickupedList)
			{
                ret.apickuped.Add(tmp.getSerialize());
			}
            return ret;
        }

        public void addPickupedInfo( APickuped pickup ) {
            this.mPickupedList.Add(pickup);
        }
        public List<APickuped> getPickupedList(int nItemID)
        {
            List<APickuped> listRet = new List<APickuped>();
            foreach (var pickuped in this.mPickupedList)
            {
                if (pickuped.getItemID() == nItemID)
                {
                    listRet.Add(pickuped);
                }
            }
            return listRet;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<APickuped> GetEnumerator()
        {
            return this.mPickupedList.GetEnumerator();
        }
    }

    /// 拾っていないアイテムの情報
    public class UnpickedupInfo : IEnumerable<int>
    {
        private Dictionary<int, int> mDicItem = new Dictionary<int, int>();

        public void addItemID(int nItemID_)
        {
            if ( !this.mDicItem.ContainsKey(nItemID_))
                this.mDicItem.Add(nItemID_, nItemID_);
        }

        public void removeItemID(int nItemID_)
        {
            this.mDicItem.Remove(nItemID_);
        }

        public bool containItemID(int nItemID_)
        {
            return this.mDicItem.ContainsKey(nItemID_);
        }

        public int count()
        {
            return this.mDicItem.Count;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<int> GetEnumerator()
        {
            return this.mDicItem.Values.GetEnumerator();
        }
    }
}

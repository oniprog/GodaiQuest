using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GodaiLibrary.GodaiQuest
{

    /// アイテムの時間関連
    [Serializable()]
    public class AItemTime
    {
        private int mItemID; 
		private DateTime mCreated;
		private DateTime mLastModified;

        public AItemTime() { }
        public AItemTime(godaiquest.AItemTime aitem)
        {
            mItemID = aitem.item_id;
			mCreated = DateTime.FromBinary(aitem.created);
			mLastModified = DateTime.FromBinary(aitem.last_modified);
        }
        public godaiquest.AItemTime getSerialize()
        {
			var ret = new godaiquest.AItemTime();
            ret.item_id = mItemID;
			ret.created = mCreated.ToBinary();
			ret.last_modified = mLastModified.ToBinary();
            return ret;
        }

        public int getItemID() {
            return this.mItemID;
        }
        public void setItemID(int nID)
        {
            this.mItemID = nID;
        }
        public DateTime getCreatedTime()
        {
            return this.mCreated;
        }
        public void setCreatedTime(DateTime created)
        {
            this.mCreated = created;
        }
        public DateTime getLastModifiedTime()
        {
            return this.mLastModified;
        }
        public void setLastModifiedTime(DateTime modified)
        {
            this.mLastModified= modified;
        }
        public AItemTime(int nItemID, DateTime created, DateTime lastModified) 
        {
            this.mItemID = nItemID;
            this.mCreated = created;
            this.mLastModified = lastModified;
        }
    }

	// 時間の集合体
    [Serializable()]
    public class ItemTimeInfo: IEnumerable<AItemTime>
    {
        private Dictionary<int, AItemTime> mDicItems = new Dictionary<int, AItemTime>();

        public ItemTimeInfo() { }
        public ItemTimeInfo(godaiquest.ItemTimeInfo iteminfo)
        {
			foreach (var tmp in iteminfo.item_time_list)
			{
                mDicItems.Add(tmp.item_id, new AItemTime(tmp));
			}
        }

        public godaiquest.ItemTimeInfo getSerialize()
        {
            var ret = new godaiquest.ItemTimeInfo();
			foreach (var tmp in mDicItems)
			{
                var itemtime = tmp.Value.getSerialize();
                ret.item_time_list.Add(itemtime);
			}
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<AItemTime> GetEnumerator()
        {
            return mDicItems.Values.GetEnumerator();
        }

        public void addItem(AItemTime item_)
        {
            this.mDicItems.Add(item_.getItemID(), item_);
        }

        public AItemTime getAItemTime(int nItemID)
        {
            AItemTime ret = null;
            this.mDicItems.TryGetValue(nItemID, out ret );
            return ret;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class RDReadItemInfo
    {
        private int mUserID;
        private HashSet<int> mDicItem = new HashSet<int>();

        public RDReadItemInfo(int nUserID)
        {
            this.mUserID = nUserID;
        }
        public RDReadItemInfo(godaiquest.RDReadItemInfo info)
        {
            mUserID = info.user_id;
			foreach (var tmp in info.read_item_dic)
			{
                mDicItem.Add(tmp.item_id);
			}
        }

        public godaiquest.RDReadItemInfo getSerialize()
        {
            var ret = new godaiquest.RDReadItemInfo();
            ret.user_id = mUserID;
			foreach (var tmp in mDicItem) 
			{
                var newtmp = new godaiquest.RDReadItemDic();
                newtmp.item_id = tmp;
                ret.read_item_dic.Add(newtmp);

			}
            return ret;
        }

        public bool isReadItem(int nItemID)
        {
            return this.mDicItem.Contains(nItemID);
        }

        public void readItem(int nItemID)
        {
            if (!this.mDicItem.Contains(nItemID))
                this.mDicItem.Add(nItemID);
        }
    }
}

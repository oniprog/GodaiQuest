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

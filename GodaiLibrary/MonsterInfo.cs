using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{

    [Serializable()]
    public class MonsterInfo
    {
        private Dictionary<int, int> mDicMonster = new Dictionary<int, int>();

        public void addMonster(int nItemID)
        {
            if( !this.mDicMonster.ContainsKey(nItemID))
                this.mDicMonster.Add(nItemID, nItemID);
        }

        public void removeMonster(int nItemID)
        {
            if (this.mDicMonster.ContainsKey(nItemID))
                this.mDicMonster.Remove(nItemID);
        }

        public bool isMonster(int nItemID)
        {
            return this.mDicMonster.ContainsKey(nItemID);
        }
    }
}

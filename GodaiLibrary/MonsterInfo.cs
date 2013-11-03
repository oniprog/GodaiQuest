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

        public MonsterInfo() { }
        public MonsterInfo(godaiquest.MonsterInfo monster_info)
        {
			foreach (var tmp in monster_info.monster_dic)
			{
                mDicMonster.Add(tmp.key, tmp.value);
			}
        }
        public godaiquest.MonsterInfo getSerialize()
        {
            var ret = new godaiquest.MonsterInfo();
			foreach ( var tmp in mDicMonster )
			{
				var newtmp = new godaiquest.MonsterDic();
				newtmp.key = tmp.Key;
				newtmp.value = tmp.Value;
                ret.monster_dic.Add(newtmp);
			}
            return ret;
        }

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

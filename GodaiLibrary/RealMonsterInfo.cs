using System;
using System.Collections.Generic;
using System.Drawing;
using godaiquest;


/*
 * 外部モンスターの定義．外部モンスターはユーザが定義したモンスターではない
 */
namespace GodaiLibrary.GodaiQuest
{
	[Serializable()]
    public class RealMonster
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int SrcID { get; set; }
		public Image MonsterImage { get; set; }
		public int ExpValue { get; set; }
		public string Spell { get; set; }

		// src_idは元となったモンスターのID
	    public RealMonster(int id, int src_id, string name, Image image, int expvalue, string spell)
	    {
	        ID = id;
	        SrcID = src_id;
	        Name = name;
	        MonsterImage = image;
	        ExpValue = expvalue;
	        Spell = spell;
	    }

	    public RealMonster(RealMonster amon)
	    {
	        ID = amon.ID;
	        SrcID = amon.SrcID;
	        Name = amon.Name;
	        MonsterImage = amon.MonsterImage;
	        ExpValue = amon.ExpValue;
	        Spell = amon.Spell;
	    }
		
	    public RealMonster()
	    {
	    }

	    public RealMonster(godaiquest.ARealMonster mon)
	    {
	        Name = mon.monster_name;
	        MonsterImage = Network.ByteArrayToImage(mon.monster_image);
	        ExpValue = mon.monster_expvalue;
	        Spell = mon.monster_spell;
	        ID = mon.monster_id;
	        SrcID = mon.monster_src_id;
	    }

	    public godaiquest.ARealMonster getSerialize()
	    {
	        var mon = new ARealMonster();
	        mon.monster_name = Name;
	        mon.monster_id = ID;
	        mon.monster_image = Network.ImageToByteArray(MonsterImage);
	        mon.monster_expvalue = ExpValue;
	        mon.monster_spell = Spell;
	        return mon;
	    }
	}

    [Serializable()]
    public class RealMonsterInfo : IEnumerable<RealMonster>
    {
        private List<RealMonster> _listMonster = new List<RealMonster>();

        public RealMonsterInfo()
        {
        }

        public RealMonsterInfo(RealMonsterInfo info)
        {
            foreach (var amon in info._listMonster)
            {
				addRealMonster( new RealMonster(amon));               
            }
        }
        public RealMonsterInfo(godaiquest.RealMonsterInfo monster)
        {
            foreach (var mon in monster.real_monster)
            {
                _listMonster.Add( new RealMonster(mon));
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<RealMonster> GetEnumerator()
        {
            return _listMonster.GetEnumerator();
        }

        public RealMonster this[int nIndex]
        {
            get { return _listMonster[nIndex]; }
			set { _listMonster[nIndex] = value; }
        }

        public godaiquest.RealMonsterInfo getSerialize()
        {
            var ret = new godaiquest.RealMonsterInfo();
            foreach (var mon in _listMonster)
            {
                var newmon = mon.getSerialize();
				ret.real_monster.Add(newmon);
            }
            return ret;
        }

        public int size()
        {
            return _listMonster.Count;
        }

        public void addRealMonster(RealMonster mon)
        {
            _listMonster.Add(mon);
        }
    }
}

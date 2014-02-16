using System;
using System.Collections.Generic;

namespace GodaiLibrary.GodaiQuest
{
	[Serializable()]
    public class ARealMonsterLocation
	{
		public int MonsterId { get; set; }
	    public int MonsterIx { get; set; }
		public int MonsterIy { get; set; }
		public int DungeonNumber { get; set; }
		public int MonsterSrcId { get; set; }

	    public ARealMonsterLocation()
	    {
	    }

	    public ARealMonsterLocation(ARealMonsterLocation loc)
	    {
	        MonsterId = loc.MonsterId;
	        MonsterIx = loc.MonsterIx;
	        MonsterIy = loc.MonsterIy;
	        DungeonNumber = loc.DungeonNumber;
	        MonsterSrcId = loc.MonsterSrcId;
	    }

	    public ARealMonsterLocation(godaiquest.ARealMonsterLocation location)
	    {
	        MonsterIx = location.monster_ix;
	        MonsterIy = location.monster_iy;
	        MonsterId = location.monster_id;
	        DungeonNumber = location.dungeon_number;
	        MonsterSrcId = location.monster_src_id;
	    }

	    public godaiquest.ARealMonsterLocation getSerialize()
	    {
	        var location = new godaiquest.ARealMonsterLocation();
	        location.monster_id = MonsterId;
	        location.monster_ix = MonsterIx;
	        location.monster_iy = MonsterIy;
	        location.dungeon_number = DungeonNumber;
	        location.monster_src_id = MonsterSrcId;
	        return location;
	    }
	}

    [Serializable()]
    public class RealMonsterLocationInfo : IEnumerable<ARealMonsterLocation>
    {
        private List<ARealMonsterLocation> _listRealMonsterLocation = new List<ARealMonsterLocation>();

		public RealMonsterLocationInfo() {}

        public RealMonsterLocationInfo(RealMonsterLocationInfo src)
        {
            foreach (var monloc in src._listRealMonsterLocation)
            {
                addRealMonsterLocation(new ARealMonsterLocation(monloc));
            }
        }

        public RealMonsterLocationInfo(godaiquest.RealMonsterLocationInfo info)
        {
            foreach (var aloc in info.location_list ) {

				_listRealMonsterLocation.Add( new ARealMonsterLocation(aloc));
            }
        }

        public godaiquest.RealMonsterLocationInfo getSerialize()
        {
			var info = new godaiquest.RealMonsterLocationInfo();
            foreach (var aloc in _listRealMonsterLocation) 
            {
				info.location_list.Add( aloc.getSerialize() );
            }
            return info;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<ARealMonsterLocation> GetEnumerator()
        {
            return _listRealMonsterLocation.GetEnumerator();
        }

        public void addRealMonsterLocation(ARealMonsterLocation location)
        {
            _listRealMonsterLocation.Add(location);
        }

        public ARealMonsterLocation this[int nIndex]
        {
            get { return _listRealMonsterLocation[nIndex]; }
            set { _listRealMonsterLocation[nIndex] = value; }
        }

        public int size()
        {
            return _listRealMonsterLocation.Count;
        }
    }
}

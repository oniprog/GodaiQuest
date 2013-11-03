using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public enum EObjectCommand
    {
        Nothing,
        GoUp,
        GoDown,
        IntoDungeon,
        GoOutDungeon
    }

    [Serializable()]
    public class ObjectAttr
    {
        private int mObjectID;
        private bool mCanWalk;      // 歩けるか?
        private int mItemID; // 1:1関係になってしまっている…
        private bool mNew;
        private EObjectCommand mCommand;
        private int mCommandSub;

        public ObjectAttr( int nObjectID_, bool bCanWalk_, int nItemID_, EObjectCommand command_, int nCommandSub_, bool bNew ) {
            this.mObjectID = nObjectID_;
            this.mCanWalk = bCanWalk_;
            this.mItemID = nItemID_;
            this.mCommand = command_;
            this.mCommandSub = nCommandSub_;
            this.mNew = bNew;
        }
		public ObjectAttr(godaiquest.ObjectAttr objattr) {
			this.mObjectID = objattr.object_id;
			this.mCanWalk = objattr.can_walk;
			this.mItemID = objattr.item_id;
			this.mCommand = (EObjectCommand)objattr.command;
			this.mCommandSub = objattr.command_sub;
			this.mNew = objattr.bNew;
        }

        public godaiquest.ObjectAttr getSerialize()
        {
            var ret = new godaiquest.ObjectAttr();
            ret.object_id = mObjectID;
            ret.can_walk = mCanWalk;
            ret.item_id = mItemID;
            ret.command = (int)mCommand;
            ret.command_sub = mCommandSub;
            ret.bNew = mNew;
            return ret;
        }

        public int getObjectID()
        {
            return this.mObjectID;
        }
        public bool canWalk()
        {
            return this.mCanWalk;
        }
        public int getItemID()
        {
            return this.mItemID;
        }
        public EObjectCommand getObjectCommand()
        {
            return this.mCommand;
        }
        public int getObjectCommandSub()
        {
            return this.mCommandSub;
        }
        public bool isNew()
        {
            return this.mNew;
        }
    }

    [Serializable()]
    public class ObjectAttrInfo : IEnumerable<ObjectAttr>
    {
        private Dictionary<int, ObjectAttr> mDicObject = new Dictionary<int, ObjectAttr>();
        private int mNewID;

		public ObjectAttrInfo() {}
		public ObjectAttrInfo( godaiquest.ObjectAttrInfo attrinfo ) {
			mNewID = attrinfo.new_id;

			foreach( var objattr in attrinfo.object_attr_dic ) {

				ObjectAttr newobjattr = new ObjectAttr(objattr.object_attr);
				mDicObject.Add( objattr.index, newobjattr );
            }
        }

        public godaiquest.ObjectAttrInfo getSerialize()
        {
            var ret = new godaiquest.ObjectAttrInfo();
            ret.new_id = mNewID;

			foreach (var dicobj in mDicObject)
			{
                var objattrdic = new godaiquest.ObjectAttrDic();
                objattrdic.index = dicobj.Key;
                objattrdic.object_attr = dicobj.Value.getSerialize();
                ret.object_attr_dic.Add(objattrdic);
			}
            return ret;
        }

        public void addObject(ObjectAttr obj_)
        {
            mDicObject.Add(obj_.getObjectID(), obj_);
            if (obj_.getObjectID() > this.mNewID)
                this.mNewID = obj_.getObjectID();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ObjectAttr> GetEnumerator()
        {
            return this.mDicObject.Values.GetEnumerator();
        }

        public int newObjectID()
        {
            if (this.mDicObject.Count == 0)
                return 0;
            else
                return ++this.mNewID;
        }

        public ObjectAttr getObject(int nObjectID)
        {
            if (this.mDicObject.ContainsKey(nObjectID))
                return this.mDicObject[nObjectID];
            else
                return null;
        }

        public int findObjectByItemID(int nItemID)
        {
            foreach (var objattr in this)
            {
                if (objattr.getItemID() == nItemID)
                {
                    return objattr.getObjectID();
                }
            }
            return 0;
        }

        public void removeObject(ObjectAttr obj)
        {
            this.mDicObject.Remove(obj.getObjectID());
        }
    }
}

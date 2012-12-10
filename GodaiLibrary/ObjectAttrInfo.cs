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

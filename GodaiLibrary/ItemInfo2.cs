using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GodaiLibrary.GodaiQuest
{

    /// 1つのアイテム 日付情報付き
    [Serializable()]
    public class AItem2
    {
        private int mItemID; // フォルダ場所はItemIDより求まるだろう。
        
        private int mItemImageID;

        private String mHeaderString;
        private Image mHeaderImage;

        private bool mNew;

        private DateTime mCreated;
        private DateTime mLastModified;

        public AItem2() { }
        public AItem2(godaiquest.AItem2 aitem)
        {
            mItemID = aitem.item_id;
            mItemImageID = aitem.item_image_id;
            mHeaderString = aitem.header_string;
            mHeaderImage = Network.ByteArrayToImage(aitem.header_image);
            mNew = aitem.bNew;
            mCreated = new DateTime(aitem.created);
            mLastModified = new DateTime(aitem.last_modified);
        }
        public godaiquest.AItem2 getSerialize()
        {
			var ret = new godaiquest.AItem2();
            ret.item_id = mItemID;
            ret.item_image_id = mItemImageID;
            ret.header_string = mHeaderString;
            ret.header_image = Network.ImageToByteArray(mHeaderImage);
            ret.created = mCreated.Ticks;
            ret.last_modified = mLastModified.Ticks;
            return ret;
        }

        public int getItemID() {
            return this.mItemID;
        }
        public void setItemID(int nID)
        {
            this.mItemID = nID;
        }
        public int getItemImageID()
        {
            return this.mItemImageID;
        }
        public void setItemImageID(int nID_)
        {
            this.mItemImageID = nID_;
        }
        public String getHeaderString()
        {
            return this.mHeaderString;
        }
        public Image getHeaderImage()
        {
            return this.mHeaderImage;
        }
        public bool isNew()
        {
            return this.mNew;
        }
        public void resetNew()
        {
            this.mNew = false;
        }
        public AItem2(int nItemID, int nItemImageID, String strHeader, Image imageHeader, bool bNew, DateTime created, DateTime lastModified)
        {
            this.mItemID = nItemID;
            this.mItemImageID = nItemImageID;
            this.mHeaderString = strHeader;
            this.mHeaderImage = imageHeader;
            this.mCreated = created;
            this.mLastModified = lastModified;
        }
    }

    /// アイテム集合の情報
    [Serializable()]
    public class ItemInfo2: IEnumerable<AItem2>
    {
        private Dictionary<int, AItem2> mDicItems = new Dictionary<int, AItem2>();

        public ItemInfo2() { }
        public ItemInfo2(godaiquest.ItemInfo2 iteminfo)
        {
			foreach (var tmp in iteminfo.item_list)
			{
                mDicItems.Add(tmp.item_id, new AItem2(tmp));
			}
        }

        public godaiquest.ItemInfo2 getSerialize()
        {
            var ret = new godaiquest.ItemInfo2();
			foreach (var tmp in mDicItems)
			{
                ret.item_list.Add(tmp.Value.getSerialize());
			}
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<AItem2> GetEnumerator()
        {
            return mDicItems.Values.GetEnumerator();
        }

        public void addItem(AItem2 item_)
        {
            this.mDicItems.Add(item_.getItemID(), item_);
        }

        public AItem2 getAItem(int nItemID)
        {
            AItem2 ret = null;
            this.mDicItems.TryGetValue(nItemID, out ret );
            return ret;
        }
    }

}

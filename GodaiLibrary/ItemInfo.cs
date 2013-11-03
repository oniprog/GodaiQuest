using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GodaiLibrary.GodaiQuest
{


    /// 1つのアイテム
    [Serializable()]
    public class AItem
    {
        private int mItemID; // フォルダ場所はItemIDより求まるだろう。
        
        private int mItemImageID;

        private String mHeaderString;
        private Image mHeaderImage;

        private bool mNew;

        public AItem() { }
        public AItem(godaiquest.AItem aitem)
        {
            mItemID = aitem.item_id;
            mItemImageID = aitem.item_image_id;
            mHeaderString = aitem.header_string;
            mHeaderImage = Network.ByteArrayToImage(aitem.header_image);
            mNew = aitem.bNew;
        }
        public godaiquest.AItem getSerialize()
        {
			var ret = new godaiquest.AItem();
            ret.item_id = mItemID;
            ret.item_image_id = mItemImageID;
            ret.header_string = mHeaderString;
            ret.header_image = Network.ImageToByteArray(mHeaderImage);
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
        public AItem(int nItemID, int nItemImageID, String strHeader, Image imageHeader, bool bNew)
        {
            this.mItemID = nItemID;
            this.mItemImageID = nItemImageID;
            this.mHeaderString = strHeader;
            this.mHeaderImage = imageHeader;
        }
    }

    /// アイテム集合の情報
    [Serializable()]
    public class ItemInfo: IEnumerable<AItem>
    {
        private Dictionary<int, AItem> mDicItems = new Dictionary<int, AItem>();

        public ItemInfo() { }
        public ItemInfo(godaiquest.ItemInfo iteminfo)
        {
			foreach (var tmp in iteminfo.aitem_dic)
			{
                mDicItems.Add(tmp.index, new AItem(tmp.aitem));
			}
        }

        public godaiquest.ItemInfo getSerialize()
        {
            var ret = new godaiquest.ItemInfo();
			foreach (var tmp in mDicItems)
			{
                var aitemdic = new godaiquest.AItemDic();
                aitemdic.index = tmp.Key;
                aitemdic.aitem = tmp.Value.getSerialize();
                ret.aitem_dic.Add(aitemdic);
			}
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<AItem> GetEnumerator()
        {
            return mDicItems.Values.GetEnumerator();
        }

        public void addItem(AItem item_)
        {
            this.mDicItems.Add(item_.getItemID(), item_);
        }

        public AItem getAItem(int nItemID)
        {
            AItem ret = null;
            this.mDicItems.TryGetValue(nItemID, out ret );
            return ret;
        }
    }

}

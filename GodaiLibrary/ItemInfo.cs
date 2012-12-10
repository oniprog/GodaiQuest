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

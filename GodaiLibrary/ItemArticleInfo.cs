using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class ItemArticle
    {
        private int mItemID;  // 本当は親で決まっている
        private int mArticleID;
        private int mUserID;
        private string mContents;
        private DateTime mCreateTime;

        public ItemArticle(int nItemID_, int nArticleID_, int nUserID_, string strContent_, DateTime createTime_)
        {
            this.mItemID = nItemID_;
            this.mArticleID = nArticleID_;
            this.mUserID = nUserID_;
            this.mContents = strContent_;
            this.mCreateTime = createTime_;
        }

        public int getItemID()
        {
            return this.mItemID;
        }
        public int getArticleID()
        {
            return this.mArticleID;
        }
        public int getUserID()
        {
            return this.mUserID;
        }
        public string getContents()
        {
            return this.mContents;
        }
        public DateTime getCreateTime() 
        {
            return this.mCreateTime;
        }
        public void setArticleID(int nArticleID_)
        {
            this.mArticleID = nArticleID_;
        }
    }

    [Serializable()]
    public class ItemArticleInfo
    {
        private int mItemID;
        private Dictionary<int, ItemArticle> mDicItemArticle = new Dictionary<int, ItemArticle>();

        public ItemArticleInfo(int nItemID_)
        {
            this.mItemID = nItemID_;
        }

        public int getItemID()
        {
            return this.mItemID;
        }

        public void addItemArticle(ItemArticle article_)
        {
            this.mDicItemArticle.Add(article_.getArticleID() , article_);
        }
    }

    [Serializable()]
    public class UnreadArticle
    {
        private int mItemID;
        private int mArticleID;
        private int mUserID;        // 誰が読んでないか？

        public UnreadArticle(int nItemID_, int nArticleID_, int nUserID_)
        {
            this.mItemID = nItemID_;
            this.mArticleID = nArticleID_;
            this.mUserID = nUserID_;
        }

        public int getItemID()
        {
            return this.mItemID;
        }
        public int getArticleID()
        {
            return this.mArticleID;
        }
        public int getUserID()
        {
            return this.mUserID;
        }
    }

    [Serializable()]
    public class UnreadArticleInfo : IEnumerable<UnreadArticle>
    {
        private Dictionary<int, UnreadArticle> mDicUnread = new Dictionary<int,UnreadArticle>();

        public void addUnreadArticle(UnreadArticle unread)
        {
            this.mDicUnread.Add( unread.getArticleID(), unread );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<UnreadArticle> GetEnumerator()
        {
            return this.mDicUnread.Values.GetEnumerator();
        }
    }
}

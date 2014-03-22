using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    public class AKeywordItem
    {
        private int mItemID;
        private int mItemPriority;

        public AKeywordItem()
        {
        }

        public AKeywordItem(int nItemID, int nItemPriority)
        {
            mItemID = nItemID;
            mItemPriority = nItemPriority;
        }

        public AKeywordItem(godaiquest.AKeywordItem akeyworditem)
        {
			mItemID = akeyworditem.item_id;
            mItemPriority = akeyworditem.item_priority;
        }

        public godaiquest.AKeywordItem getSerialize()
        {
            var ret = new godaiquest.AKeywordItem();
            ret.item_id = mItemID;
            ret.item_priority = mItemPriority;
            return ret;
        }

        public int getItemID()
        {
            return mItemID;
        }

        public int getItemPriority()
        {
            return mItemPriority;
        }
    }

	public class AKeyword : IEnumerable<AKeywordItem>
	{
	    private int mKeywordID;
	    private string mKeyword;
	    private int mKeywordPriority;
	    private HashSet<AKeywordItem> mSetKeywordItem = new HashSet<AKeywordItem>();

	    public AKeyword()
	    {
	        
	    }
	    public  AKeyword(int nKeywordID, string keyword, int nKeywordPriority)
	    {
	        mKeywordID = nKeywordID;
	        mKeyword = keyword;
	        mKeywordPriority = nKeywordPriority;
	    }

		public AKeyword(godaiquest.AKeyword akeyword) 
		{
			mKeywordID = akeyword.keyword_id;
		    mKeyword = akeyword.keyword;
		    mKeywordPriority = akeyword.keyword_priority;
		    foreach (var akeywordItem in akeyword.keyword_item_set)
		    {
		        mSetKeywordItem.Add(new AKeywordItem(akeywordItem));
		    }
		}

        public godaiquest.AKeyword getSerialize()
        {
            var ret = new godaiquest.AKeyword();
            ret.keyword = mKeyword;
            ret.keyword_id = mKeywordID;
            ret.keyword_priority = mKeywordPriority;
            foreach (var akeywordItem in mSetKeywordItem)
            {
                ret.keyword_item_set.Add(akeywordItem.getSerialize());
            }
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<AKeywordItem> GetEnumerator()
        {
            return mSetKeywordItem.GetEnumerator();
        }

	    public void addKeywordItem(AKeywordItem item)
	    {
	        mSetKeywordItem.Add(item);
	    }
        public int getKewordID()
        {
            return mKeywordID;
        }

	    public int getKeywordPriority()
	    {
	        return mKeywordPriority;
	    }

        public string getKeyword()
        {
            return mKeyword;
        }
     }

    public class KeywordUserInfo : IEnumerable<AKeyword>
    {
        private int mUserID;
        private HashSet< AKeyword> mSetKeyword = new HashSet<AKeyword>();

        public KeywordUserInfo()
        {
        }

        public KeywordUserInfo(godaiquest.KeywordUserInfo keywordInfo)
        {
            foreach (var akeyword in keywordInfo.keyword_set)
            {
                mSetKeyword.Add(new AKeyword(akeyword));
            }
        }

        public godaiquest.KeywordUserInfo getSerialize()
        {
            var ret = new godaiquest.KeywordUserInfo();
            ret.user_id = mUserID;
            foreach (var akeyword in mSetKeyword)
            {
                ret.keyword_set.Add(akeyword.getSerialize());
            }
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<AKeyword> GetEnumerator()
        {
            return mSetKeyword.GetEnumerator();
        }

        public void addKeyword(AKeyword keyword)
        {
            mSetKeyword.Add(keyword);
        }
    }
}

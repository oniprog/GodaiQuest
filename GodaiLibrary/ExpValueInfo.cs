using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class ExpValue
    {
        private int mUseriD;
        private int mExpValue;
        private int mTotalValue;

        public ExpValue(int nUserID_, int nExpValue_, int nTotalValue_)
        {
            this.mUseriD = nUserID_;
            this.mExpValue = nExpValue_;
            this.mTotalValue = nTotalValue_;
        }

        public int getUserID()
        {
            return this.mUseriD;
        }
        public int getExpValue()
        {
            return this.mExpValue;
        }
        public void setExpValue(int nNewExp_)
        {
            this.mExpValue = nNewExp_;
        }
        public int getTotalValue()
        {
            return this.mTotalValue;
        }
        public void setTotalValue(int nTotalValue_)
        {
            this.mTotalValue = nTotalValue_;
        }
        public void incValue(int nValue_)
        {
            this.mExpValue += nValue_;
            if (nValue_ > 0)
                this.mTotalValue += nValue_;
        }
    }

    [Serializable()]
    public class ExpValueInfo
    {
        private Dictionary<int, ExpValue> mDicExpValue = new Dictionary<int, ExpValue>();

        public ExpValue getExpValue(int nUserID)
        {
            return this.mDicExpValue[nUserID];
        }
        public void addExpValue(int nUserID, ExpValue expvalue_)
        {
            if (this.mDicExpValue.ContainsKey(nUserID))
                this.mDicExpValue.Remove(nUserID);
            this.mDicExpValue.Add(nUserID, expvalue_);
        }
    }
}

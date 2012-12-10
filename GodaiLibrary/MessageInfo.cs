using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class AMessage
    {
        private int mUserID;
        private String mMessage;

        public AMessage(int nUseriD_, String strMes_)
        {
            this.mUserID = nUseriD_;
            this.mMessage = strMes_;
        }

        public int getUserID()
        {
            return this.mUserID;
        }
        public String getMessage()
        {
            return this.mMessage;
        }
    }

    [Serializable()]
    public class MessageInfo : IEnumerable<AMessage>
    {
        private Dictionary<int, AMessage> mDicMessage = new Dictionary<int, AMessage>();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<AMessage> GetEnumerator()
        {
            return this.mDicMessage.Values.GetEnumerator();
        }

        public void addAMessage(AMessage mes)
        {
            this.mDicMessage.Add(mes.getUserID(), mes);
        }
        public AMessage getAMessage(int nUserID)
        {
            AMessage ret;
            this.mDicMessage.TryGetValue(nUserID, out ret);
            return ret;
        }
    }
}

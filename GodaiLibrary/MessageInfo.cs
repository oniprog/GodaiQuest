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
        public AMessage(godaiquest.AMessage ames)
        {
            mUserID = ames.uesr_id;
            mMessage = ames.message;
        }

        public godaiquest.AMessage getSerialize()
        {
            var ret = new godaiquest.AMessage();
            ret.uesr_id = mUserID;
            ret.message = mMessage;
            return ret;
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

        public MessageInfo() { }
        public MessageInfo(godaiquest.MessageInfo mesinfo)
        {
			foreach (var tmp in mesinfo.message_dic)
			{
                mDicMessage.Add(tmp.index, new AMessage(tmp.amessage));
			}
        }

        public godaiquest.MessageInfo getSerialize()
        {
            var ret = new godaiquest.MessageInfo();
			foreach (var tmp in mDicMessage)
			{
                var mes_dic = new godaiquest.AMessageDic();
                mes_dic.index = tmp.Key;
                mes_dic.amessage = tmp.Value.getSerialize();
                ret.message_dic.Add(mes_dic);
			}
            return ret;
        }

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

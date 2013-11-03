using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class AUser
    {
        private int mUserID;
        private String mMail;
        private String mName;
        private Image mImageCharacter;

        public AUser() { }

        public AUser(int nUserID_, String strMail_, String strName_, Image imageCharacter_)
        {

            this.mUserID = nUserID_;
            this.mMail = strMail_;
            this.mName = strName_;
            this.mImageCharacter = imageCharacter_;
        }
        public AUser(godaiquest.AUser auser)
        {
            mUserID = auser.user_id;
            mMail = auser.mail_address;
            mName = auser.user_name;
            mImageCharacter = Network.ByteArrayToImage(auser.user_image);
        }
        public godaiquest.AUser getSerialize()
        {
            var ret = new godaiquest.AUser();
            ret.user_id = mUserID;
            ret.mail_address = mMail;
            ret.user_name = mName;
            ret.user_image = Network.ImageToByteArray(mImageCharacter);
            return ret;
        }

        public int getUserID() {
            return this.mUserID;

        }
        public String getMail() {
            return this.mMail;
        }
        public String getName() {
            return this.mName;
        }
        public void setName(String name_)
        {
            this.mName = name_;
        }
        public Image getCharacterImage() {
            return this.mImageCharacter;
        }
        public void setCharacterImage(Image image_)
        {
            this.mImageCharacter = image_;
        }
    }

    [Serializable()]
    public class UserInfo : IEnumerable<AUser> {
        private Dictionary<int, AUser> mDicUser = new Dictionary<int, AUser>();

        public UserInfo() { }
        public UserInfo(godaiquest.UserInfo userinfo)
        {
			foreach (var auserdic in userinfo.uesr_dic)
			{
                int index = auserdic.index;
                AUser auser = new AUser(auserdic.auser);
				mDicUser.Add(index, auser);
            }
        }
        public godaiquest.UserInfo getSerialize()
        {
            var ret = new godaiquest.UserInfo();
			foreach (var ausertmp in mDicUser)
			{
                var auser_dic = new godaiquest.AUserDic();
                auser_dic.index = ausertmp.Key;
                auser_dic.auser = ausertmp.Value.getSerialize();
                ret.uesr_dic.Add(auser_dic);
			}
            return ret;
        }

        System.Collections.IEnumerator  System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public IEnumerator<AUser>   GetEnumerator() {
            return this.mDicUser.Values.GetEnumerator();
        }

        public void addUesr(AUser user_)
        {
            this.mDicUser.Add(user_.getUserID(), user_);
        }

        public AUser getAUser(int nUserID)
        {
            if (this.mDicUser.ContainsKey(nUserID))
                return this.mDicUser[nUserID];
            else
                return new AUser(); // 居ないユーザ、削除済みのユーザのために
        }
    }



}

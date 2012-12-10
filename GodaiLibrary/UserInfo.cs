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

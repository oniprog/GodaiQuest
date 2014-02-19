using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GodaiQuestServer
{
    public class UserInfoInside
    {
        private String mMail = "";
        private String mName = "";
        private String mPasswordHash = "";
        private Image mImageCharacter;

        public UserInfoInside(String strMail, String strName, String strPasswordHash, Image imageCharacter)
        {
            this.mMail = strMail;
            this.mName = strName;
            this.mPasswordHash = strPasswordHash;
            this.mImageCharacter = imageCharacter;
        }

        public DBUser asDBUser()
        {
            DBUser ret = new DBUser();
            ret.Mail = this.mMail;
            ret.Name = this.mName;
            ret.PasswordHash = this.mPasswordHash;
            ret.ImageCharacter = this.mImageCharacter is Bitmap
                ? this.mImageCharacter as Bitmap
                : new Bitmap(this.mImageCharacter);
            return ret;
        }
    }
}

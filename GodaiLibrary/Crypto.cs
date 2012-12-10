using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary
{
    public static class Crypto
    {
        // Hash値を計算する
        public static String calcPasswordHash(String strPassword)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(strPassword);
//            System.Security.Cryptography.SHA512Cng sha = new System.Security.Cryptography.SHA512Cng();
            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();

            byte[] hash = sha.ComputeHash(data);

            System.Text.StringBuilder result = new System.Text.StringBuilder();
            foreach (byte b in hash)
            {
                result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }
    }
}

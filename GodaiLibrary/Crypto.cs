using System;

namespace GodaiLibrary
{
    public static class Crypto
    {
        // Hash値を計算する
        public static String CalcPasswordHash(String strPassword)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(strPassword);
//            System.Security.Cryptography.SHA512Cng sha = new System.Security.Cryptography.SHA512Cng();
            var sha = new System.Security.Cryptography.SHA512Managed();

            byte[] hash = sha.ComputeHash(data);

            var result = new System.Text.StringBuilder();
            foreach (byte b in hash)
            {
                result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }
    }
}

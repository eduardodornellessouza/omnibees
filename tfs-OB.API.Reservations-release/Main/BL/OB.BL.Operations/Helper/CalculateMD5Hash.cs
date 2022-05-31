using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Helper
{
    public static class CalculateMD5Hash
    {
        public static string CalculateMD5(string input)
        {
            if (input == null)
                return null;

            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static byte[] CalculateMD5Bytes(string input)
        {
            if (input == null)
                return null;

            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            return md5.ComputeHash(inputBytes);
        }

        public static byte[] CalculateSHA256Bytes(string input)
        {
            if (input == null)
                return null;

            var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return sha256.ComputeHash(inputBytes);
        }

        public static string RemoveAccents(string str)
        {
            if (str == null)
                return (null);

            Encoding destEncoding = Encoding.GetEncoding("iso-8859-8");

            return destEncoding.GetString(
              Encoding.Convert(Encoding.UTF8, destEncoding, Encoding.UTF8.GetBytes(str)));
        }
    }
}
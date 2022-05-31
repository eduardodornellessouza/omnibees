using System;

namespace OB.BL.Operations.Helper
{
    public static class EncodeString
    {
        #region "Constant (s)"
        private const string TAMPER_PROOF_KEY = "astkvsnanvpi";
        #endregion


        #region "Public Method(s)"

        /// <summary>
        ///Function to encode the string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        static public string TamperProofStringEncode(string value, string key)
        {
            System.Security.Cryptography.MACTripleDES mac3des = new System.Security.Cryptography.MACTripleDES();
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            mac3des.Key = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value)) + System.Convert.ToChar("-") + System.Convert.ToBase64String(mac3des.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value)));
        }

        /// <summary>
        ///Function to decode the string
        ///Throws an exception if the data is corrupt 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        static public string TamperProofStringDecode(string value, string key)
        {
            String dataValue = "";
            if (value == null)
                value = string.Empty;

            if (key == null)
                key = string.Empty;

            value = value.Trim();
            value = value.Replace(" ", "+");

            System.Security.Cryptography.MACTripleDES mac3des = new System.Security.Cryptography.MACTripleDES();
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            mac3des.Key = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));

            try
            {
                dataValue = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(value.Split(System.Convert.ToChar("-"))[0]));
            }
            catch 
            {
                throw new ArgumentException("Invalid TamperProofString");
            }
            return dataValue;
        }

        static public string Encode(string value)
        {
            return TamperProofStringEncode(value, TAMPER_PROOF_KEY);
        }

        static public string Decode(string value)
        {
            return TamperProofStringDecode(value, TAMPER_PROOF_KEY);
        }

        static public string QueryStringEncode(string value)
        {
            return System.Web.HttpUtility.UrlEncode(Encode(value));
            //return null;    // Temparay changes
        }
        static public string QueryStringDecode(string value)
        {
            return System.Web.HttpUtility.UrlDecode(Decode(value));
            //return null;    // Temparay changes
        }
        #endregion
    }
}

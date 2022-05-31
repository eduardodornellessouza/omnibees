using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBSecurityRepository : RestRepository<OB.BL.Contracts.Data.Rates.Extra>, IOBSecurityRepository
    {
        public List<string> EncryptCreditCards(OB.BL.Contracts.Requests.ListCreditCardRequest request)

        {
            var data = new List<string>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListCreditCardResponse>(request, "Security", "EncryptCreditCards");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.CreditCards.ToList();

            return data;
        }

        public List<string> DecryptCreditCards(OB.BL.Contracts.Requests.ListCreditCardRequest request)

        {
            var data = new List<string>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListCreditCardResponse>(request, "Security", "DecryptCreditCards");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.CreditCards.ToList();

            return data;
        }

        public byte[] GetCreditCardHash(OB.BL.Contracts.Requests.GetCreditCardHashRequest request)
        {
            byte[] data = null;
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.GetCreditCardHashResponse>(request, "Security", "GetCreditCardHash");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.CreditCardHashCode;

            return data;
        }

        public string EncryptString(string publicKey, string dataToEncrypt)
        {
            // TODO: Passar isto para um POCO.
            
            string encryptedData = string.Empty;
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(publicKey);
                    byte[] byteArray = Encoding.Default.GetBytes(dataToEncrypt);
                    encryptedData = Convert.ToBase64String(rsa.Encrypt(byteArray, false));

                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }

            return encryptedData;
        }
    }
}

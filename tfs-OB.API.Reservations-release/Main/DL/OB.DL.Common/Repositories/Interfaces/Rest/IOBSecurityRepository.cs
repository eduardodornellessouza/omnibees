using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBSecurityRepository : IRestRepository<OB.BL.Contracts.Data.Rates.Extra>
    {
        List<string> EncryptCreditCards(OB.BL.Contracts.Requests.ListCreditCardRequest request);
        List<string> DecryptCreditCards(OB.BL.Contracts.Requests.ListCreditCardRequest request);
        byte[] GetCreditCardHash(OB.BL.Contracts.Requests.GetCreditCardHashRequest request);

        string EncryptString(string publicKey, string dataToEncrypt);

    }
}

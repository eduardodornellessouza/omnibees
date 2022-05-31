namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBBePartialPaymentCcMethodRepository : IRestRepository<OB.BL.Contracts.Data.Payments.BePartialPaymentCcMethod>
    {
        OB.BL.Contracts.Responses.ListBePartialPaymentCcMethodResponse ListBePartialPaymentCcMethods(OB.BL.Contracts.Requests.ListBePartialPaymentCcMethodRequest request);
    }
}

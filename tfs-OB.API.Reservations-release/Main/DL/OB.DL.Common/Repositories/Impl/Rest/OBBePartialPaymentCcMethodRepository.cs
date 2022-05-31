using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    public class OBBePartialPaymentCcMethodRepository : IOBBePartialPaymentCcMethodRepository
    {
        public ListBePartialPaymentCcMethodResponse ListBePartialPaymentCcMethods(ListBePartialPaymentCcMethodRequest request)
        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListBePartialPaymentCcMethodResponse>(request, "BE", "ListBePartialPaymentCcMethods");
        }
    }
}

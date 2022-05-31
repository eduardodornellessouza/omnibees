using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBPaymentMethodTypeRepository : RestRepository<OB.BL.Contracts.Data.Payments.PaymentMethodType>, IOBPaymentMethodTypeRepository
    {
        public List<OB.BL.Contracts.Data.Payments.PaymentMethodType> ListPaymentMethodTypes(OB.BL.Contracts.Requests.ListPaymentMethodTypesRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Payments.PaymentMethodType>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPaymentMethodTypesResponse>(request, "General", "ListPaymentMethodTypes");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.Payments.PaymentMethod> ListPaymentMethods(OB.BL.Contracts.Requests.ListPaymentMethodsRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Payments.PaymentMethod>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPaymentMethodsResponse>(request, "General", "ListPaymentMethods");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }
    }
}

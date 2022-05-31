using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBPaymentMethodTypeRepository : IRestRepository<OB.BL.Contracts.Data.Payments.PaymentMethodType>
    {
        List<OB.BL.Contracts.Data.Payments.PaymentMethodType> ListPaymentMethodTypes(OB.BL.Contracts.Requests.ListPaymentMethodTypesRequest request);

        List<OB.BL.Contracts.Data.Payments.PaymentMethod> ListPaymentMethods(OB.BL.Contracts.Requests.ListPaymentMethodsRequest request);
    }
}

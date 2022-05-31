using OB.DL.Common.QueryResultObjects;
using OB.Domain.Payments;
using System.Collections.Generic;

namespace OB.DL.Common.Interfaces
{
    public interface IPaymentGatewayConfigurationRepository : IRepository<PaymentGatewayConfiguration>
    {
        PaymentGatewayQR1 GetActivePaymentGatewayConfiguration(long propertyId, long currencyId, long paymentMethodId);

        PaymentGatewayQR1 GetActivePaymentGatewayConfigurationReduced(long propertyId, long currencyId);

        List<PaymentGatewayConfiguration> FindByCriteria(out int totalRecords, List<long> propertyIds, List<long> gatewayIds,
            List<string> gatewayNames, List<string> gatewayCodes,
            bool returnTotal = false, int pageIndex = 0, int pageSize = 0);

        void Invalidate(bool forceUpdate = false); 
    }
}
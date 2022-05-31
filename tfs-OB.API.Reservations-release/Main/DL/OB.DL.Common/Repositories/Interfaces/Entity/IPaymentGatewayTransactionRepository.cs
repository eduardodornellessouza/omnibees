using System.Collections.Generic;
using OB.Domain.Payments;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Criteria;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface IPaymentGatewayTransactionRepository : IRepository<PaymentGatewayTransaction>
    {
        IEnumerable<PaymentGatewayTransaction> FindByCriteria(ListPaymentGatewayTransactionsCriteria criteria);
    }
}

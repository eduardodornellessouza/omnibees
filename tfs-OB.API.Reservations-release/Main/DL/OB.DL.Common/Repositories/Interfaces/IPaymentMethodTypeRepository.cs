using OB.DL.Common.Interfaces;
using OB.Domain.Payments;

namespace OB.DL.Common.Impl
{
    public interface IPaymentMethodTypeRepository : IRepository<PaymentMethodType>
    {
        /// <summary>
        /// Desc :: To Get PaymentMethodType by code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        PaymentMethodType FindPaymentMethodTypeByCode(int code);
    }
}
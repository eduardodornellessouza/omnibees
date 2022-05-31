using OB.Domain.General;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OB.DL.Common.Interfaces
{
    public interface IExchangeRateRepository : IRepository<OB.Domain.General.ExchangeRate>
    {
        IEnumerable<ExchangeRate> GetAll();
        ExchangeRate FirstOrDefault(Expression<Func<ExchangeRate, bool>> predicate);
        IEnumerable<ExchangeRate> GetByCurrencyIds(List<long> currencyIds);
    }
}
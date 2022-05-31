using OB.Domain.General;
using System.Collections.Generic;
using System.Globalization;

namespace OB.DL.Common.Interfaces
{
    public interface ICurrencyRepository : IRepository<Currency>
    {
        CultureInfo GetCultureInfoByCurrencyUID(long currency_UID);

        IEnumerable<Currency> FindCurrencyByCriteria(out int totalRecords, List<long> UIDs = null, List<string> Names = null, List<string> Symbols = null, List<string> CurrencySymbols = null, List<string> PaypalCurrencyCodes = null,
            int pageIndex = 0, int pageSize = 0, bool returnTotal = false);
    }
}
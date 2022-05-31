using System.Collections.Generic;
using System.Globalization;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBCurrencyRepository : IRestRepository<OB.BL.Contracts.Data.General.Currency>
    {
        Dictionary<long, OB.BL.Contracts.Data.General.Currency> ListPropertyBaseCurrencyByPropertyUID(OB.BL.Contracts.Requests.ListPropertyBaseCurrencyByPropertyUIDRequest request);

        List<OB.BL.Contracts.Data.General.ExchangeRate> ListExchangeRatesByCurrencyIds(OB.BL.Contracts.Requests.ListExchangeRatesRequest request);

        List<OB.BL.Contracts.Data.General.Currency> ListCurrencies(OB.BL.Contracts.Requests.ListCurrencyRequest request);

        CultureInfo GetCultureInfoByCurrencyUID(OB.BL.Contracts.Requests.GetCultureInfoRequest request);
    }
}

using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBCurrencyRepository : RestRepository<OB.BL.Contracts.Data.General.Currency>, IOBCurrencyRepository
    {
        public Dictionary<long, OB.BL.Contracts.Data.General.Currency> ListPropertyBaseCurrencyByPropertyUID(OB.BL.Contracts.Requests.ListPropertyBaseCurrencyByPropertyUIDRequest request)

        {
            var data = new Dictionary<long, OB.BL.Contracts.Data.General.Currency>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListPropertyBaseCurrencyByPropertyUIDResponse>(request, "Properties", "ListPropertyBaseCurrencyByPropertyUID");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.General.ExchangeRate> ListExchangeRatesByCurrencyIds(OB.BL.Contracts.Requests.ListExchangeRatesRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.General.ExchangeRate>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListExchangeRatesResponse>(request, "General", "ListExchangeRatesByCurrencyIds");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.General.Currency> ListCurrencies(OB.BL.Contracts.Requests.ListCurrencyRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.General.Currency>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListCurrencyResponse>(request, "General", "ListCurrencies");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public CultureInfo GetCultureInfoByCurrencyUID(OB.BL.Contracts.Requests.GetCultureInfoRequest request)

        {
            CultureInfo data = null;
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.GetCultureInfoResponse>(request, "General", "GetCultureInfoByCurrencyUID");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }
    }
}

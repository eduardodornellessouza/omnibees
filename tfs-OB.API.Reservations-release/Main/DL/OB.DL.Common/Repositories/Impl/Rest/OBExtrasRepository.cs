using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBExtrasRepository : RestRepository<OB.BL.Contracts.Data.Rates.Extra>, IOBExtrasRepository
    {
        public List<OB.BL.Contracts.Data.Rates.Extra> ListIncludedRateExtras(OB.BL.Contracts.Requests.ListIncludedRateExtrasRequest request)
        {
            var data = new List<OB.BL.Contracts.Data.Rates.Extra>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListIncludedRateExtrasResponse>(request, "Extras", "ListIncludedRateExtras");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.Rates.RatesExtrasPeriod> ListRatesExtrasPeriod(OB.BL.Contracts.Requests.ListRatesExtrasPeriodRequest request)
        {
            var data = new List<OB.BL.Contracts.Data.Rates.RatesExtrasPeriod>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRatesExtrasPeriodResponse>(request, "Rates", "ListRatesExtrasPeriod");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.Rates.Extra> ListExtras(OB.BL.Contracts.Requests.ListExtraRequest request)
        {
            var data = new List<OB.BL.Contracts.Data.Rates.Extra>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListExtraResponse>(request, "Rates", "ListExtras");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<OB.BL.Contracts.Data.Rates.ExtrasBillingType> ListExtrasBillingTypesForReservation(OB.BL.Contracts.Requests.ListExtrasBillingTypesForReservationRequest request)
        {
            var data = new List<OB.BL.Contracts.Data.Rates.ExtrasBillingType>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListExtrasBillingTypesForReservationResponse>(request, "Extras", "ListExtrasBillingTypesForReservation");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public Dictionary<long, List<OB.BL.Contracts.Data.Rates.Extra>> ListRatesExtras(OB.BL.Contracts.Requests.ListRatesExtrasRequest request)
        {
            var data = new Dictionary<long, List<OB.BL.Contracts.Data.Rates.Extra>>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRatesExtrasResponse>(request, "Rates", "ListRatesExtras");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.RateUIDVsExtra;

            return data;
        }
    }
}

using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using contracts = OB.BL.Contracts.Data;
using requests = OB.BL.Contracts.Requests;
using responses = OB.BL.Contracts.Responses;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBPromotionalCodeRepository : RestRepository<contracts.Rates.PromotionalCode>, IOBPromotionalCodeRepository
    {
        public contracts.Rates.PromotionalCode ListPromotionalCodeForReservation(requests.ListPromotionalCodeForReservationRequest request)
        {
            var data = new contracts.Rates.PromotionalCode();
            var response = RESTServicesFacade.Call<responses.ListPromotionalCodeForReservationResponse>(request, "PromotionalCode", "ListPromotionalCodeForReservation");

            if (response.Status == responses.Status.Success)
                data = response.Result;
            
            return data;
        }

        public List<contracts.Rates.PromotionalCode> ListPromotionalCode(requests.ListPromotionalCodeRequest request)
        {
            var data = new List<contracts.Rates.PromotionalCode>();
            var response = RESTServicesFacade.Call<responses.ListPromotionalCodeResponse>(request, "PromotionalCode", "ListPromotionalCode");

            if (response.Status == responses.Status.Success)
                data = response.Result;

            return data;
        }

        public responses.ListPromotionalCodeForValidRatesResponse ListPromotionalCodeForValidRates(requests.ListPromotionalCodeForValidRatesRequest request)
        {
            return RESTServicesFacade.Call<responses.ListPromotionalCodeForValidRatesResponse>(request, "PromotionalCode", "ListPromotionalCodeForValidRates");
        }

        public responses.UpsertPromotionalCodesByDaysResponse UpsertPromotionalCodesByDays(requests.UpsertPromotionalCodesByDaysRequest request)
        {
            return RESTServicesFacade.Call<responses.UpsertPromotionalCodesByDaysResponse>(request, "PromotionalCode", "UpsertPromotionalCodesByDays");
        }


        public responses.ListPromotionalCodesByDayResponse ListPromotionalCodesByDay(requests.ListPromotionalCodesByDayRequest request)
        {
            return RESTServicesFacade.Call<responses.ListPromotionalCodesByDayResponse>(request, "PromotionalCode", "ListPromotionalCodesByDay");
        }

    }
}

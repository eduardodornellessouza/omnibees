using System.Collections.Generic;
using contracts = OB.BL.Contracts.Data;
using requests = OB.BL.Contracts.Requests;
using responses = OB.BL.Contracts.Responses;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBPromotionalCodeRepository : IRestRepository<contracts.Rates.PromotionalCode>
    {
        contracts.Rates.PromotionalCode ListPromotionalCodeForReservation(requests.ListPromotionalCodeForReservationRequest request);

        List<contracts.Rates.PromotionalCode> ListPromotionalCode(requests.ListPromotionalCodeRequest request);

        responses.ListPromotionalCodeForValidRatesResponse ListPromotionalCodeForValidRates(requests.ListPromotionalCodeForValidRatesRequest request);

        responses.UpsertPromotionalCodesByDaysResponse UpsertPromotionalCodesByDays(requests.UpsertPromotionalCodesByDaysRequest request);

        responses.ListPromotionalCodesByDayResponse ListPromotionalCodesByDay(requests.ListPromotionalCodesByDayRequest request);
    }
}

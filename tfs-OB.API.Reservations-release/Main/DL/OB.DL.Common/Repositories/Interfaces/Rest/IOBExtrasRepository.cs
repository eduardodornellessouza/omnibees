using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBExtrasRepository : IRestRepository<OB.BL.Contracts.Data.Rates.Extra>
    {
        List<OB.BL.Contracts.Data.Rates.Extra> ListIncludedRateExtras(OB.BL.Contracts.Requests.ListIncludedRateExtrasRequest request);

        List<OB.BL.Contracts.Data.Rates.RatesExtrasPeriod> ListRatesExtrasPeriod(OB.BL.Contracts.Requests.ListRatesExtrasPeriodRequest request);

        List<OB.BL.Contracts.Data.Rates.Extra> ListExtras(OB.BL.Contracts.Requests.ListExtraRequest request);

        List<OB.BL.Contracts.Data.Rates.ExtrasBillingType> ListExtrasBillingTypesForReservation(OB.BL.Contracts.Requests.ListExtrasBillingTypesForReservationRequest request);

        Dictionary<long, List<OB.BL.Contracts.Data.Rates.Extra>> ListRatesExtras(OB.BL.Contracts.Requests.ListRatesExtrasRequest request);
    }
}

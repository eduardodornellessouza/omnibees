using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBRateRepository : IRestRepository<OB.BL.Contracts.Data.Rates.Rate>
    {
        void ExecStoredProc_DW_ProcessRatesAvailabilityRealTime(BL.Contracts.Requests.ProcessRatesAvailabilityRealTimeRequest request);

        List<BL.Contracts.Data.Rates.RateLight> ListRatesLight(OB.BL.Contracts.Requests.ListRateLightRequest request);

        int ConnectorEventQueueInsert(OB.BL.Contracts.Requests.ConnectorEventQueueInsertRequest request);

        Dictionary<long, List<long>> GetRateChannelsList(OB.BL.Contracts.Requests.GetRateChannelsListRequest request);

        List<OB.BL.Contracts.Data.Rates.RateChannel> ListRateChannelsDetails(OB.BL.Contracts.Requests.ListRateChannelsRequest request);

        List<BL.Contracts.Data.Rates.RateRestriction> ListRateRestrictions(OB.BL.Contracts.Requests.ListRateRestrictionsRequest request);

        List<BL.Contracts.Data.Rates.TaxPolicy> ListTaxPolicies(OB.BL.Contracts.Requests.ListTaxPoliciesRequest request);

        List<BL.Contracts.Data.Rates.GroupCode> ListGroupCodesForReservation(OB.BL.Contracts.Requests.ListGroupCodesForReservationRequest request);

        List<BL.Contracts.Data.Rates.Rate> ListRatesForReservation(OB.BL.Contracts.Requests.ListRatesForReservationRequest request);

        List<BL.Contracts.Data.Rates.RateModel> ListRateModels(OB.BL.Contracts.Requests.ListRateModelsRequest request);

        Dictionary<long, int> ListRatesAvailablityType(OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest request);

        List<BL.Contracts.Data.Rates.RateRoom> ListRateRooms(OB.BL.Contracts.Requests.ListRateRoomsRequest request);
    }
}

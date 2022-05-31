using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBRateRepository : RestRepository<OB.BL.Contracts.Data.Rates.Rate>, IOBRateRepository
    {
        public void ExecStoredProc_DW_ProcessRatesAvailabilityRealTime(BL.Contracts.Requests.ProcessRatesAvailabilityRealTimeRequest request)
        {
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ProcessRatesAvailabilityRealTimeResponse>(request, "Rates", "ExecStoredProc_DW_ProcessRatesAvailabilityRealTime");
        }

        public List<BL.Contracts.Data.Rates.RateLight> ListRatesLight(OB.BL.Contracts.Requests.ListRateLightRequest request)
        {
            var data = new List<BL.Contracts.Data.Rates.RateLight>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRateLightResponse>(request, "Rates", "ListRatesLight");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public int ConnectorEventQueueInsert(OB.BL.Contracts.Requests.ConnectorEventQueueInsertRequest request)
        {
          return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ConnectorEventQueueInsertResponse>(request, "ConnectorsEventQueue", "ConnectorEventQueueInsert").Result;
        }

        public Dictionary<long, List<long>> GetRateChannelsList(OB.BL.Contracts.Requests.GetRateChannelsListRequest request)
        {
            var data = new Dictionary<long, List<long>>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.GetRateChannelsListResponse>(request, "Rates", "GetRateChannelsList");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.Rates.RateChannel> ListRateChannelsDetails(OB.BL.Contracts.Requests.ListRateChannelsRequest request)
        {
            var data = new List<OB.BL.Contracts.Data.Rates.RateChannel>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRateChannelsResponse>(request, "Rates", "ListRateChannels");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<BL.Contracts.Data.Rates.RateRestriction> ListRateRestrictions(OB.BL.Contracts.Requests.ListRateRestrictionsRequest request)
        {
            var data = new List<BL.Contracts.Data.Rates.RateRestriction>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRateRestrictionsResponse>(request, "Rates", "ListRateRestrictions");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.Rates.TaxPolicy> ListTaxPolicies(OB.BL.Contracts.Requests.ListTaxPoliciesRequest request)
        {
            var data = new List<BL.Contracts.Data.Rates.TaxPolicy>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListTaxPoliciesResponse>(request, "Rates", "ListTaxPolicies");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.Rates.GroupCode> ListGroupCodesForReservation(OB.BL.Contracts.Requests.ListGroupCodesForReservationRequest request)
        {
            var data = new List<BL.Contracts.Data.Rates.GroupCode>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListGroupCodesForReservationResponse>(request, "Rates", "ListGroupCodesForReservation");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.Rates.Rate> ListRatesForReservation(OB.BL.Contracts.Requests.ListRatesForReservationRequest request)
        {
            var data = new List<BL.Contracts.Data.Rates.Rate>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRatesForReservationResponse>(request, "Rates", "ListRatesForReservation");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<BL.Contracts.Data.Rates.RateModel> ListRateModels(OB.BL.Contracts.Requests.ListRateModelsRequest request)
        {
            var data = new List<BL.Contracts.Data.Rates.RateModel>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRateModelsResponse>(request, "Rates", "ListRateModels");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public Dictionary<long,int> ListRatesAvailablityType(OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest request)
        {
            var data = new Dictionary<long, int>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRateAvailabilityTypeResponse>(request, "Rates", "ListRateAvailabilityType");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<BL.Contracts.Data.Rates.RateRoom> ListRateRooms(OB.BL.Contracts.Requests.ListRateRoomsRequest request)
        {
            var data = new List<BL.Contracts.Data.Rates.RateRoom>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRateRoomsResponse>(request, "Rates", "ListRateRooms");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }
    }
}
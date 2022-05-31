using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;
using OB.BL.Contracts.Responses;
using System.Threading.Tasks;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBRateRoomDetailsForReservationRoomRepository :
        RestRepository<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>,
        IOBRateRoomDetailsForReservationRoomRepository
    {
        public List<OB.BL.Contracts.Data.Rates.RateRoomDetail> ListRateRoomDetails(OB.BL.Contracts.Requests.ListRateRoomDetailsRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Rates.RateRoomDetail>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRateRoomDetailsResponse>(request,
                "RateRoomDetails", "ListRateRoomDetails");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public async Task<List<OB.BL.Contracts.Data.Rates.RateRoomDetail>> ListRateRoomDetailsAsync(OB.BL.Contracts.Requests.ListRateRoomDetailsRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Rates.RateRoomDetail>();
            var response =  RESTServicesFacade.Call<ListRateRoomDetailsResponse>(request,
                "RateRoomDetails", "ListRateRoomDetailsAsync");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }


        public List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> ListRateRoomDetailForReservationRoom(
            OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>();
            var response =
                RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListRateRoomDetailForReservationRoomResponse>(
                    request, "RateRoomDetails", "ListRateRoomDetailForReservationRoom");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public int UpdateRatesAvailabilityForPropertyAndCorrelationID(OB.BL.Contracts.Requests.UpdateRatesAvailabilityRequest request)

        {
            return
                RESTServicesFacade.Call<OB.BL.Contracts.Responses.UpdateRatesAvailabilityResponse>(request,
                    "RateRoomDetails", "UpdateRatesAvailabilityForPropertyAndCorrelationID").Result;
        }

        public int UpdateRateRoomDetailAllotments(OB.BL.Contracts.Requests.UpdateRateRoomDetailAllotmentsRequest request)
        {
            return
                RESTServicesFacade.Call<OB.BL.Contracts.Responses.UpdateRateRoomDetailAllotmentsResponse>(request,
                    "RateRoomDetails", "UpdateRateRoomDetailAllotments").Result;
        }
        /// <summary>
        /// Calls UpdateRateRoomDetails from OB.API.
        /// Suppose to do the request error logs here? or in RestServicesFacade?
        /// </summary>
        /// <param name="request">exactly the request that will do to UpdateRateRoomDetails.</param>
        /// <returns>exactly the response returned by the OB.API</returns>
        public UpdateRateRoomDetailsResponse UpdateRateRoomDetails(OB.BL.Contracts.Requests.UpdateRateRoomDetailsRequest request)
        {
            return
                RESTServicesFacade.Call<OB.BL.Contracts.Responses.UpdateRateRoomDetailsResponse>(request,
                    "RateRoomDetails", "UpdateRateRoomDetails");
        }

        public void LoggingRateAndAvailabilities(OB.BL.Contracts.Requests.LoggingRateAndAvailabilitiesRequest request)

        {
            RESTServicesFacade.Call<OB.BL.Contracts.Responses.LoggingRateAndAvailabilitiesResponse>(request,
                "RateRoomDetails", "LoggingRateAndAvailabilities");
        }
    }
}
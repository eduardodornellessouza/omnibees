using OB.BL.Contracts.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBRateRoomDetailsForReservationRoomRepository : IRestRepository<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>
    {
        List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> ListRateRoomDetailForReservationRoom(OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest request);
        int UpdateRatesAvailabilityForPropertyAndCorrelationID(OB.BL.Contracts.Requests.UpdateRatesAvailabilityRequest request);
        int UpdateRateRoomDetailAllotments(OB.BL.Contracts.Requests.UpdateRateRoomDetailAllotmentsRequest request);

        List<OB.BL.Contracts.Data.Rates.RateRoomDetail> ListRateRoomDetails(OB.BL.Contracts.Requests.ListRateRoomDetailsRequest request);

        Task<List<OB.BL.Contracts.Data.Rates.RateRoomDetail>> ListRateRoomDetailsAsync(OB.BL.Contracts.Requests.ListRateRoomDetailsRequest request);

        UpdateRateRoomDetailsResponse UpdateRateRoomDetails(OB.BL.Contracts.Requests.UpdateRateRoomDetailsRequest request);

        void LoggingRateAndAvailabilities(OB.BL.Contracts.Requests.LoggingRateAndAvailabilitiesRequest request);

    }
}

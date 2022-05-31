using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBIncentiveRepository : IRestRepository<OB.BL.Contracts.Data.Properties.Incentive>
    {
        OB.BL.Contracts.Responses.ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse ListIncentivesWithBookingAndStayPeriodsForReservationRoom(
            OB.BL.Contracts.Requests.ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest request);

        List<OB.BL.Contracts.Data.Properties.Incentive> ListIncentivesForReservationRoom(OB.BL.Contracts.Requests.ListIncentivesForReservationRoomRequest request);

        List<OB.BL.Contracts.Data.Properties.Incentive> ListIncentives(OB.BL.Contracts.Requests.ListIncentiveRequest request);

    }
}

using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBIncentiveRepository : RestRepository<OB.BL.Contracts.Data.Properties.Incentive>, IOBIncentiveRepository
    {
        public List<OB.BL.Contracts.Data.Properties.Incentive> ListIncentivesForReservationRoom(OB.BL.Contracts.Requests.ListIncentivesForReservationRoomRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Properties.Incentive>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListIncentivesForReservationRoomResponse>(request, "Rates", "ListIncentivesForReservationRoom");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public OB.BL.Contracts.Responses.ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse ListIncentivesWithBookingAndStayPeriodsForReservationRoom(
            OB.BL.Contracts.Requests.ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest request){

            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse>(request, "Rates", "ListIncentivesWithBookingAndStayPeriodsForReservationRoom");
        }

        public List<OB.BL.Contracts.Data.Properties.Incentive> ListIncentives(OB.BL.Contracts.Requests.ListIncentiveRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Properties.Incentive>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListIncentiveResponse>(request, "Properties", "ListIncentives");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

    }
}

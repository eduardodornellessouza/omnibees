using OB.BL.Contracts.Requests;
using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    class OBReservationLookupsRepository : RestRepository<OB.BL.Contracts.Data.Reservations.ReservationLookups>, IOBReservationLookupsRepository
    {
        public OB.BL.Contracts.Data.Reservations.ReservationLookups ListReservationLookups(ListReservationLookupsRequest request)

        {
            OB.BL.Contracts.Data.Reservations.ReservationLookups data = null;
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListReservationLookupsResponse>(request, "ReservationLookup", "ListReservationLookups");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }
    }
}
using OB.BL.Contracts.Requests;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBReservationLookupsRepository : IRestRepository<OB.BL.Contracts.Data.Reservations.ReservationLookups>
    {
        OB.BL.Contracts.Data.Reservations.ReservationLookups ListReservationLookups(ListReservationLookupsRequest request);
    }
}

using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.General;
using contractCrm = OB.BL.Contracts.Data.CRM;
using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class TreatReservationFiltersParameters
    {
        public Domain.Reservations.ReservationFilter ReservationFilter { get; set; }
        public contractsReservation.Reservation NewReservation { get; set; }
        public contractCrm.Guest Guest { get; set; }

        public ServiceName ServiceName { get; set; }
    }
}

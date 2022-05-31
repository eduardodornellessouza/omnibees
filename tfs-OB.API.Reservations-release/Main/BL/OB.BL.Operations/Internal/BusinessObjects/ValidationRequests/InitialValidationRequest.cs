using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class InitialValidationRequest : ValidationBaseRequest
    {
        public contractsReservations.Reservation Reservation { get; set; }

        public List<contractsReservations.ReservationRoom> ReservationRooms { get; set; }

        public List<contractsReservations.ReservationRoomDetail> ReservationRoomDetails { get; set; }

    }
}

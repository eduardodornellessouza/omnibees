using contractsGuest = OB.Reservation.BL.Contracts.Data.CRM;
using contractsReservation= OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;

namespace OB.Reservation.BL.Contracts.Logs
{

    public class ReservationLoggingMessage : LoggingObject
    {
        public contractsGuest.Guest objguest { get; set; }
        public contractsReservation.Reservation objReservation { get; set; }
        public contractsReservation.ReservationsAdditionalData ReservationAdditionalData { get; set; }
        public List<long> objGuestActivity { get; set; }
        public List<contractsReservation.ReservationRoom> objReservationRoom { get; set; }
        public List<contractsReservation.ReservationRoomDetail> objReservationRoomDetail { get; set; }
        public List<contractsReservation.ReservationRoomExtra> objReservationRoomExtras { get; set; }
        public List<contractsReservation.ReservationRoomChild> objReservationRoomChild { get; set; }
        public contractsReservation.ReservationPaymentDetail objReservationPaymentDetail { get; set; }
        public List<contractsReservation.ReservationRoomExtrasSchedule> objReservationExtraSchedule { get; set; }
        public List<contractsReservation.ReservationRoomExtrasAvailableDate> objReservationRoomExtrasAvailableDates { get; set; }

        public long? ReservationAdditionalDataUID { get; set; }
        public int? GroupRule { get; set; }
    }
}

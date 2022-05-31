using OB.Reservation.BL.Contracts.Data.CRM;
using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;
using contractsCRMOB = OB.BL.Contracts.Data.CRM;
using System.Collections.Generic;

namespace OB.Reservation.BL.Contracts.Logs
{

    public class ReservationLogRequest : LoggingObject
    {
        public contractsReservation.Reservation OldReservation { get; set; }
        public contractsReservation.Reservation Reservation { get; set; }
        public List<long> GuestActivity { get; set; }
        public Constants.ReservationTransactionStatus ReservationTransactionState { get; set; }
        public string ReservationTransactionId { get; set; }
        public long? ReservationAdditionalDataUID { get; set; }
        public contractsReservation.ReservationsAdditionalData ReservationAdditionalData { get; set; }
        public contractsCRMOB.Guest Guest { get; set; }
        public long HangfireId { get; set; }
        public int TransactionRetries { get; set; }
        public int? GroupRule { get; set; }
    }
}

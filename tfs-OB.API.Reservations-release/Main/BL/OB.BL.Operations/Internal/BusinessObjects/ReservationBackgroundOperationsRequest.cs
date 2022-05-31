using OB.Reservation.BL.Contracts.Requests;
using System.Collections.Generic;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.Reservation.BL.Contracts.Logs
{
    public class ReservationBackgroundOperationsRequest : LoggingObject
    {
        public ReservationBackgroundOperationsRequest()
        {
            DecrementInventory = true;
            IncrementInventory = true;
        }

        public contractsReservation.Reservation NewReservation { get; set; }
        public contractsReservation.Reservation OldReservation { get; set; }
        public bool SendOperatorCreditLimitExcededEmail { get; set; }
        public bool SendTPICreditLimitExcededEmail { get; set; }
        public OB.Reservation.BL.Constants.ReservationTransactionStatus ReservationTransactionState { get; set; }
        public string ReservationTransactionId { get; set; }
        public long? ReservationAdditionalDataUID { get; set; }
        public OB.Domain.Reservations.ReservationsAdditionalData ReservationAdditionalData { get; set; }
        public long HangfireId { get; set; }
        public ReservationBaseRequest ReservationRequest { get; set; }
        public int TransactionRetries { get; set; }
        public bool DecrementInventory { get; set; }
        public bool IncrementInventory { get; set; }
        public List<contractsProperties.Inventory> Inventories { get; set; }
    }
}

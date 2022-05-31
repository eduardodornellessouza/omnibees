using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class PaymentGatewayCommitOrCancelReservationOnRequestResult : ContractBase
    {
        public PaymentGatewayCommitOrCancelReservationOnRequestResult()
        {
        }

        [DataMember]
        public long Reservation_UID { get; set; }

        [DataMember]
        public long ReservationCurrency_UID { get; set; }

        [DataMember]
        public long Channel_UID { get; set; }

        [DataMember]
        public string TransactionUID { get; set; }

        [DataMember]
        public int TransactionState { get; set; }

        [DataMember]
        public long HangfireID { get; set; }
        
        [DataMember]
        public bool WasCommited { get; set; }
        
        [DataMember]
        public bool WasCancelled { get; set; }
    }
}
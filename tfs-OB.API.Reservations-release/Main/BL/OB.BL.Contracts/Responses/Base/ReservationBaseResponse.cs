using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ReservationBaseResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TransactionId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<OB.Reservation.BL.Constants.ReservationTransactionStatus> TransactionStatus { get; set; }
    }
}
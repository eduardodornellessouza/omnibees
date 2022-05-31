using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListCancelReservationReasonResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<CancelReservationReason> Result { get; set; }
    }
}

using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.VisualStates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GetReservationReadStatusResponse : ResponseBase
    {
        [DataMember]
        public ReservationReadStatus Result { get; set; }
  
    }
}
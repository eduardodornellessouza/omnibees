using OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class InsertLostReservationRequest : RequestBase
    {
        [DataMember(IsRequired=true, EmitDefaultValue=false)]
        public LostReservationDetail LostReservationDetail { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public LostReservation LostReservation { get; set; }
    }
}

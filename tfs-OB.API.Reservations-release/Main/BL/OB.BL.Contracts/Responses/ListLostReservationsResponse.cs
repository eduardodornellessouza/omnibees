using OB.Reservation.BL.Contracts.Data.Reservations;
using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListLostReservationsResponse : PagedResponseBase
    {
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public List<LostReservation> Result { get; set; }

        public ListLostReservationsResponse()
        {
        }
    }
}

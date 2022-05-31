using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OB.Reservation.BL.Contracts.Data.VisualStates;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListMarkReservationsAsViewedResponse : PagedResponseBase
    {
        [DataMember]
        public List<ReservationReadStatus> Result { get; set; }
    }
}

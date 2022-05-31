using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListMarkReservationsAsViewedRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> UIDs { get; set; }

        [DataMember]
        public List<long> ReservationUIDs { get; set; }
    }
}

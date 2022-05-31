using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateReservationIsPaidRequest : RequestBase
    {
        [DataMember]
        public long ReservationId { get; set; }

        [DataMember]
        public long UserId { get; set; }

        [DataMember]
        public bool MakeDiscount = true;
    }
}

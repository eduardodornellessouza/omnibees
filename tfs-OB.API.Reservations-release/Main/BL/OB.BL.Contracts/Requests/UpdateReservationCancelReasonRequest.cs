using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateReservationCancelReasonRequest : RequestBase
    {
        [DataMember]
        public long ReservationId { get; set; }

        [DataMember]
        public int? CancelReservationReasonID { get; set; }

        [DataMember]
        public string CancelReservationComments { get; set; }
    }
}

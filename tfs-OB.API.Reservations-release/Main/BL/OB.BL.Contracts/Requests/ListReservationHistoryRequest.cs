using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListReservationHistoryRequest : PagedRequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> ReservationHistoryUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> ReservationUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> ReservationNumbers { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> Statuses { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime MinChangedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime MaxChangedDate { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListLostReservationsRequest : GridPagedRequest
    {
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public List<long> Uids { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUids { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeDetails { get; set; }

        public ListLostReservationsRequest()
        {
            Uids = new List<long>();
            PropertyUids = new List<long>();
        }
    }
}

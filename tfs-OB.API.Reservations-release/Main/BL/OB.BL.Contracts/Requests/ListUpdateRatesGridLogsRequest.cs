using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListFriendlyLogsRequest : GridPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? PropertyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? StartDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? EndDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> UserIds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UseUtcDates { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetBEClosedDaysRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateFrom { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ChannelUID { get; set; }
    }
}

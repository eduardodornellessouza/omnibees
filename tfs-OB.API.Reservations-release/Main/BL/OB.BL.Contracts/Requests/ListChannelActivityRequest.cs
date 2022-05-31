using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListChannelActivityRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> ChannelUIDs { get; set; }

        [DataMember]
        public int? ChannelStatus { get; set; }

        [DataMember]
        public long? PropertyUID { get; set; }

        [DataMember]
        public DateTime? DateFrom { get; set; }

        [DataMember]
        public DateTime? DateTo { get; set; }
    }
}
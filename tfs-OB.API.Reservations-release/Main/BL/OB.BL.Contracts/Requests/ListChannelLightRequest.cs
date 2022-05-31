using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    public class ListChannelLightRequest : RequestBase
    {
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        [DataMember]
        public List<long> ChannelUIDs { get; set; }

        [DataMember]
        public bool? Enabled { get; set; }
    }
}
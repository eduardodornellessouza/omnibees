using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for ChannelPropertyLight.
    /// </summary>
    [DataContract]
    public class ListActiveChannelPropertiesLightRequest : RequestBase
    {
        [DataMember]
        public long ChannelUID { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public bool IsDemo { get; set; }

        [DataMember]
        public List<long> PropertyUIDs { get; set; }
    }
}
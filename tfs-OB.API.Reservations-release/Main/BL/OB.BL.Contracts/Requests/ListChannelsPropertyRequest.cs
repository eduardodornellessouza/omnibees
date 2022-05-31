using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for ChannelsProperty.
    /// </summary>
    [DataContract]
    public class ListChannelsPropertyRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> UIDs { get; set; }

        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        [DataMember]
        public List<long> ChannelUIDs { get; set; }

        [DataMember]
        public bool? IsActive { get; set; }

        [DataMember]
        public bool? IsPendingRequest{ get; set; }

        [DataMember]
        public bool? ExcludeDeleteds { get; set; }

        [DataMember]
        public bool? IsEnabled { get; set; }
    }
}
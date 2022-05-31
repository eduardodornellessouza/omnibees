using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for Channels.
    /// </summary>
    [DataContract]
    public class ListChannelRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        [DataMember]
        public List<long> ChannelUIDs { get; set; }

        [DataMember]
        public List<string> ChannelNames { get; set; }

        [DataMember]
        public bool? Enabled { get; set; }
        
        [DataMember]
        public List<int> Types { get; set; }

        [DataMember]
        public List<string> ChannelCodes { get; set; }

        [DataMember]
        public List<int> OperatorTypes { get; set; }

        [DataMember]
        public List<string> OperatorCodes { get; set; }

        [DataMember]
        public bool? IncludeDescription { get; set; }
    }
}
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for ChannelRoomTypeCodes.
    /// </summary>
    [DataContract]
    public class ListChannelRateCodeRequest : PagedRequestBase
    {
        [DataMember]
        public long ChannelUID { get; set; }

        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        [DataMember]
        public List<string> Codes { get; set; }
    }
}
using OB.Reservation.BL.Contracts.Requests;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that corresponds to a paged set of ListPMSRateRoomMappingLightRequest configuration objects.
    /// </summary>
    [DataContract]
    public class ListPMSRateRoomMappingLightRequest : PagedRequestBase
    {
        public ListPMSRateRoomMappingLightRequest()
        {
        }

        [DataMember]
        public long PMSUID { get; set; }

        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        [DataMember]
        public long? MaxPropertyUID { get; set; }

        [DataMember]
        public long? MinPropertyUID { get; set; }
    }
}
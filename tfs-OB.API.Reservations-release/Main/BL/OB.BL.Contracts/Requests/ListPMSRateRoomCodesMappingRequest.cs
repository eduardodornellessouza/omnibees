using OB.Reservation.BL.Contracts.Requests;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that corresponds to a paged set of ListPMSRateRoomCodesMappingRequest configuration objects.
    /// </summary>
    [DataContract]
    public class ListPMSRateRoomCodesMappingRequest : PagedRequestBase
    {
        public ListPMSRateRoomCodesMappingRequest()
        {
        }

        [DataMember]
        public long PMSUID { get; set; }

        [DataMember]
        public long PropertyUID { get; set; }

    }
}
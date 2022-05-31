using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for Channels.
    /// </summary>
    [DataContract]
    public class ListActivePMSRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        [DataMember]
        public bool IncludePmsName { get; set; }
    }
}
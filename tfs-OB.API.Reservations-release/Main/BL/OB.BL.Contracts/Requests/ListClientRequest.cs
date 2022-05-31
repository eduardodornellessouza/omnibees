using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListClientRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> ClientUIDs { get; set; }
    }
}
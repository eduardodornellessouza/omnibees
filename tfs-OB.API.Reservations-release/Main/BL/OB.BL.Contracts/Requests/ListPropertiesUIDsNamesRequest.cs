using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPropertiesUIDsNamesRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> UIDs { get; set; }

        [DataMember]
        public List<string> PropertyNames { get; set; }

        [DataMember]
        public bool? IsActive { get; set; }

        [DataMember]
        public bool? IsDemo { get; set; }
    }
}
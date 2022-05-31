using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPagedRequestBase : PagedRequestBase
    {
        [DataMember]
        public string TypeName { get; set; }

        [DataMember]
        public List<long> UIDs { get; set; }
    }
}
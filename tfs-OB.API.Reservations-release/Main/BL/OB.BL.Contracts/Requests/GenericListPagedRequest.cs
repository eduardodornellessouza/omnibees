using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GenericListPagedRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Language { get; set; }
    }
}
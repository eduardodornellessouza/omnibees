using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListIncentiveRequest : PagedRequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> IncentiveUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Constants.IncentiveType> IncentiveTypes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeIncentiveLanguages { get; set; }
    }
}
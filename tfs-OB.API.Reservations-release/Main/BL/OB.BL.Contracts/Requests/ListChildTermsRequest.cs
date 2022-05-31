using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for Channels.
    /// </summary>
    [DataContract]
    public class ListChildTermsRequest : ListPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeChildTermLanguage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeChildTermCurrencies { get; set; }
    }
}
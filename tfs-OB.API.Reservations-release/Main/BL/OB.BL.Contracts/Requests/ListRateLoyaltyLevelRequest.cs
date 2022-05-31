using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListRateLoyaltyLevelRequest : ListPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> RateUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> LoyaltyLevelUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludePropertyName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeRateName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeRateCurrencyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyLevelName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyProgramName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyProgramUID { get; set; }
    }
}

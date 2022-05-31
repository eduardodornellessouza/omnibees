using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListLoyaltyLevelsRequest : ListPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> LoyaltyProgram_UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyLevelsLanguages { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyLevelsCurrencies { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeRateLoyaltyLevels { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyLevelsLimitsPeriodicity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool ExcludeInactive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeDeleted { get; set; }
    }
}

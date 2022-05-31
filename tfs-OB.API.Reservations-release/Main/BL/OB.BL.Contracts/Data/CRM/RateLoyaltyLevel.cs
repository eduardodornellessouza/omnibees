using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class RateLoyaltyLevel : ContractBase
    {
        public RateLoyaltyLevel()
        { 
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Rate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long LoyaltyLevel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PropertyName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string RateName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? RateCurrencyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? LoyaltyProgramUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LoyaltyProgramName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LoyaltyLevelName { get; set; }
    }
}

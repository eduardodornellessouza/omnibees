using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListLoyaltyProgramRequest : ListPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> Client_UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeCreatedAndModifiedUserName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeDefaultCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeDefaultLanguage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyLevels { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> LoyaltyLevel_Uids { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyLevelsLanguages { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyProgramLanguages { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool ExcludeInactive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool ExcludeRateLoyaltyLevels { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeBackgroundImageBytes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeAttachmentPDFBytes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeLoyaltyLevelsCurrencies {get; set;}
    }
}
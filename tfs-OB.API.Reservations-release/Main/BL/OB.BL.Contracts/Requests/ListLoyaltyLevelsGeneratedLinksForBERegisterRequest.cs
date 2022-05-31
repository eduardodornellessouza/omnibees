using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListLoyaltyLevelsGeneratedLinksForBERegisterRequest : ListPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Guid> GUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> Client_UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> LoyaltyLevels_UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? ExcludeUseds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? ExcludeExpireds { get; set; }
    }
}

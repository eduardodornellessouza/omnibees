using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Properties
{
    [DataContract]
    public class ChildTermLanguage : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ChildTerm_UID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Language_UID { get; set; }
    }
}

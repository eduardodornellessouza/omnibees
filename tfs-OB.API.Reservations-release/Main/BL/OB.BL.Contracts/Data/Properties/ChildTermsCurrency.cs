using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Properties
{
    [DataContract]
    public class ChildTermsCurrency : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ChildTerm_UID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Currency_UID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? Value { get; set; }
    }
}

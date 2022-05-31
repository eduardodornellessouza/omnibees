using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.ProactiveActions
{
    [DataContract]
    public class SystemEvent : ContractBase
    {
        public SystemEvent()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsProactiveActionHidden { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int SEOrder { get; set; }
    }
}
namespace OB.Reservation.BL.Contracts.Data.General
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    
    [DataContract]
    public partial class BillingTypesLanguage  : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long BillingTypes_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Language_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Language Language { get; set; }
    }
}

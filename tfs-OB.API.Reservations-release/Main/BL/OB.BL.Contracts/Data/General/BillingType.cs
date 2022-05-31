namespace OB.Reservation.BL.Contracts.Data.General
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class BillingType : ContractBase
    {
        public BillingType()
        {
            this.BillingTypesLanguages = new List<BillingTypesLanguage>();
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> Type { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual IList<BillingTypesLanguage> BillingTypesLanguages { get; set; }
    }
}

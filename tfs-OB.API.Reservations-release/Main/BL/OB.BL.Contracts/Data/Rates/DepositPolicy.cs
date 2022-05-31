using OB.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class DepositPolicy : ContractBase
    {
        public DepositPolicy()
        {
        }

        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TranslatedName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TranslatedDescription { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> Percentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? Days { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDepositCostsAllowed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool DepositCosts { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> PaymentModel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> NrNights { get; set; }
    }
}
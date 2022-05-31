using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Rates
{
    [DataContract]
    public class OtherPolicy : ContractBase
    {
        public OtherPolicy()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string OtherPolicy_Name { get; set; }

        [DataMember]
        public string OtherPolicy_Description { get; set; }

        [DataMember]
        public string TranslatedName { get; set; }

        [DataMember]
        public string TranslatedDescription { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> OtherPolicyCategory_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }

    }
}
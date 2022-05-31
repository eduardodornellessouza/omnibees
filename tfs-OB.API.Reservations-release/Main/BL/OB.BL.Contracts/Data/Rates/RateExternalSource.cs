using System;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateExternalSource : ContractBase
    {
        public RateExternalSource()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Rate_UID { get; set; }

        [DataMember]
        public long ExternalSource_UID { get; set; }

        [DataMember]
        public string ExternalRateID { get; set; }

        [DataMember]
        public System.DateTime CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

    }
}
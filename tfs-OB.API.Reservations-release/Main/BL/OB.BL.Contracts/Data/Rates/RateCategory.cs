using OB.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateCategory : ContractBase
    {
        public RateCategory()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> OTACodeUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string OTACodeName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> OTACodeValue { get; set; }

    }
}
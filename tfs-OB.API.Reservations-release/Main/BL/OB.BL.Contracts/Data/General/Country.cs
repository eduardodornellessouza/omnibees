using System;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.General
{
    [DataContract]
    public class Country : ContractBase
    {
        public Country()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CountryCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> GeonameId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CountryIsoCode { get; set; }
    }
}
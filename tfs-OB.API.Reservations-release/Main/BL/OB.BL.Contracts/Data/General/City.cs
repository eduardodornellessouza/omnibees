using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.BL.Contracts.Data.General
{
    [DataContract]
    public class City : ContractBase
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CountryCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Int64 GeonameId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Int64 Country_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Admin1Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Admin2Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CityOriginalName { get; set; }
    }
}

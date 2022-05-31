using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.BL.Contracts.Data.General
{

    [DataContract]
    public class CityData : ContractBase
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> country_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string countryName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string stateName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> geonameid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string asciiname { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string alternatenames { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string latitude { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string longitude { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string featureclass { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string featurecode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string countrycode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string cc2 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string admin1code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string admin2code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string admin3code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string admin4code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string population { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string elevation { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string dem { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string timezone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string modificationdate { get; set; }
    }
}

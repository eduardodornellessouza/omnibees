using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListCityDataRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> UIDs { get; set; }        

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Language { get; set; }

        [DataMember]
        public bool IncludeCountryName { get; set; }

        [DataMember]
        public bool IncludeStateName { get; set; }
    }
}

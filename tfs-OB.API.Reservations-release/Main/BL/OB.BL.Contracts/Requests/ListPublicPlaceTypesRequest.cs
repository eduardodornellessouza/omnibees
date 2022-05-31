using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPublicPlaceTypesRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> Names { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> Codes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Language_UID { get; set; }
    }
}

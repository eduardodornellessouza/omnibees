using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListZonesRequest : PagedRequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Country_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Admin1Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Admin2Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CitiesDataUid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Language { get; set; }

    }
}

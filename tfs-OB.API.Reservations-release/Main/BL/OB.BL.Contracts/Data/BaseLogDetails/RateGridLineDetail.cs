using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    [DataContract]
    public class RateGridLineDetail : BaseGridLineDetail
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Rate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Rooms { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Channels { get; set; }
    }
}

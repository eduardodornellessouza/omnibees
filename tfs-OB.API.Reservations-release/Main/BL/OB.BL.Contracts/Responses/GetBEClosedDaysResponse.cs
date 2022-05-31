using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OB.Reservation.BL.Contracts.Data.BE;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GetBEClosedDaysResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<DateTime> Results { get; set; }
    }
}

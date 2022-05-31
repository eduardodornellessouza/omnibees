using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetGuestActiveRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Client_UID { get; set; }
    }
}

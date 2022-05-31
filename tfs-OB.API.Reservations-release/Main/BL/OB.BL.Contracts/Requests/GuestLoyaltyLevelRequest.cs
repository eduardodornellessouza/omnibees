using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GuestLoyaltyLevelRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? Guest_UID { get; set; }
    }
}
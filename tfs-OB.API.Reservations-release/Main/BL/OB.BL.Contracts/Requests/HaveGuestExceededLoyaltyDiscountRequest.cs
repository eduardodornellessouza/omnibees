using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class HaveGuestExceededLoyaltyDiscountRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? Guest_UID { get; set; }
    }
}

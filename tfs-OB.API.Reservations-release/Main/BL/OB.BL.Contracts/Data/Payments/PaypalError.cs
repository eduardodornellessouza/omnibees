using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Payments
{
    [DataContract]
    public class PaypalError
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ErrorCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LongMessage { get; set; }

    }
}

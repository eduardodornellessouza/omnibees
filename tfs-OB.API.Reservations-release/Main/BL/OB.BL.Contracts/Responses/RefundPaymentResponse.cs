using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class RefundPaymentResponse : ResponseBase
    {
        [DataMember]
        public bool Result { get; set; }
    }
}

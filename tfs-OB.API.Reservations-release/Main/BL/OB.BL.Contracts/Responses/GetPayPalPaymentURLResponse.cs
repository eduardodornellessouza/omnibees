using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GetPayPalPaymentURLResponse : ResponseBase
    {
        [DataMember]
        public string Result { get; set; }

        [DataMember]
        public string Token { get; set; }
    }
}

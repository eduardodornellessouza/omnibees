using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ProcessCreditCardAccessTokenResponse : ResponseBase
    {
        [DataMember]
        public string Number { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public DateTime? CardExpiration { get; set; }

        [DataMember]
        public string CardHolder { get; set; }
    }
}

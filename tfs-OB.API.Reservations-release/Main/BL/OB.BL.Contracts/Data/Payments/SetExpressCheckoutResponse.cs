using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Payments
{
    [DataContract]
    public class SetExpressCheckoutResponse : ContractBase
    {
        public SetExpressCheckoutResponse()
        {

        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PaypalError> Errors { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Token { get; set; }
    }
}

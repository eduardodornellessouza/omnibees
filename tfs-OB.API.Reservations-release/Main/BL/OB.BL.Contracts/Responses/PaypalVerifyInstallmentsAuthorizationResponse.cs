using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class PaypalVerifyInstallmentsAuthorizationResponse : ResponseBase
    {
        [DataMember]
        public bool Result { get; set; }
    }
}

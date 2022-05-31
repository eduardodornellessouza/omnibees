using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class CancelAndRefundRecurringBillingResponse : ResponseBase
    {
        [DataMember]
        public bool Result { get; set; }
    }
}

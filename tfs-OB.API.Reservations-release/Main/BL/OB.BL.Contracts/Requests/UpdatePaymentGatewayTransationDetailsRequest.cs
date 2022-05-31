using OB.Reservation.BL.Contracts.Data.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdatePaymentGatewayTransationDetailsRequest : RequestBase
    {
        [DataMember]
        public PaymentGatewayTransactionsDetail PaymentGatewayTransactionsDetail { get; set; }
    }
}

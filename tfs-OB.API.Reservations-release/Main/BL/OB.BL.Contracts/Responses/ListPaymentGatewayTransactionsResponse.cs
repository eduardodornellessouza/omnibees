using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OB.Reservation.BL.Contracts.Data.Payments;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPaymentGatewayTransactionsResponse : ResponseBase
    {
        [DataMember]
        public List<PaymentGatewayTransaction> Result { get; set; }
    }
}

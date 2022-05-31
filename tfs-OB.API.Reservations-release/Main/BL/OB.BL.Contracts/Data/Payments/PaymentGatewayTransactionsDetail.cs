using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Payments
{
    [DataContract]
    public class PaymentGatewayTransactionsDetail : ContractBase
    {
        [DataMember]
        public long UID { get; set; }
        [DataMember]
        public string ResponseCode { get; set; }
        [DataMember]
        public string RequestJson { get; set; }
        [DataMember]
        public string ResponseJson { get; set; }
        [DataMember]
        public long PaymentGatewayTransactionId { get; set; }
        [DataMember]
        public string RequestType { get; set; }
        [DataMember]
        public string RefundTransactionID { get; set; }
    }
}

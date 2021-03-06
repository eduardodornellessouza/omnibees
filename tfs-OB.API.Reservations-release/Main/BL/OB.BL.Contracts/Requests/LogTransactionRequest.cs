using OB.Reservation.BL.Contracts.Data.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using static OB.Reservation.BL.Constants;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class LogTransactionRequest : RequestBase
    {
        [DataMember]
        public string UniqueId { get; set; }

        [DataMember]
        public SetExpressCheckoutResponse response { get; set; }

        [DataMember]
        public bool isTestMode { get; set; }

        [DataMember]
        public long transactionId { get; set; }

        [DataMember]
        public string requestType { get; set; }

        [DataMember]
        public string responseCode { get; set; }

        [DataMember]
        public string responseJson { get; set; }

        [DataMember]
        public string requestJson { get; set; }

        [DataMember]
        public string refundTransactionId { get; set; }

        [DataMember]
        public string paypalAutoGeneratedId { get; set; }

        [DataMember]
        public LogTransactionType LogType { get; set; }
    }
}

using System.Runtime.Serialization;
using static OB.Reservation.BL.Constants;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class RefundPaymentRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? TransactionId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PaypalTransactionId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Constants.PaypalRefundType RefundType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string RefundAmount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ReservationCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyId { get; set; }
    }
}

using System.Runtime.Serialization;
using static OB.Reservation.BL.Constants;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.Reservation.BL.Contracts.Requests
{
     [DataContract]
    public class CancelAndRefundRecurringBillingRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PaymentGatewayTransactionId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ProfileId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPartialRefund { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public contractsReservations.Reservation Reservation { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public PayPalAction Action { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Note { get; set; }
    }
}

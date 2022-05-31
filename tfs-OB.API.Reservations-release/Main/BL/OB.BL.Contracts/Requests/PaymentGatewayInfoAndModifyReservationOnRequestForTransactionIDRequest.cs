using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class PaymentGatewayInfoAndModifyReservationOnRequestForTransactionIDRequest : RequestBase
    {
        [DataMember]
        public string TransactionID { get; set; }

        [DataMember]
        public bool IsCommitOnRequest { get; set; }

        [DataMember]
        public bool IsCancelOnRequest { get; set; }

        [DataMember]
        public bool AlreadyCancelledFromPaymentGateway { get; set; }
    }
}
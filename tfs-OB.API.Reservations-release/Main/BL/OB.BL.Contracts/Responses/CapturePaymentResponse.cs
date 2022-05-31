using System.Runtime.Serialization;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class CapturePaymentResponse : ResponseBase
    {
        [DataMember]
        public string PaypalTransationId { get; set; }

        [DataMember]
        public contractsReservations.Reservation Reservation { get; set; }

    }
}

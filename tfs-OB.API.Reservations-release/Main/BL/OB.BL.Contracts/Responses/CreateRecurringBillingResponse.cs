using System.Runtime.Serialization;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class CreateRecurringBillingResponse : ResponseBase
    {
        [DataMember]
        public string ProfileId { get; set; }

        [DataMember]
        public contractsReservations.Reservation Reservation { get; set; }
    }
}

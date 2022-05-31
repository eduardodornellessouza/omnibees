using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    public class UpdateReservationIsPaidBulkRequest : ListReservationRequest
    {
        [DataMember]
        public long UserId { get; set; }
    }
}

using OB.Reservation.BL.Contracts.Responses;
namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ReservationResult : ResponseBase
    {
        public long UID { get; set; }
        public string Number { get; set; }

        public OB.Reservation.BL.Constants.ReservationStatus ReservationStatus { get; set; }
    }
}

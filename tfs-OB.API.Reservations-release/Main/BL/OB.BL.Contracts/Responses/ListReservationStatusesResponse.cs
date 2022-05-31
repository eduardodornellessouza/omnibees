using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Reservations;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of ReservationStatus objects.
    /// </summary>
    [DataContract]
    public class ListReservationStatusesResponse : ListGenericPagedResponse<ReservationStatus>
    {
    }
}

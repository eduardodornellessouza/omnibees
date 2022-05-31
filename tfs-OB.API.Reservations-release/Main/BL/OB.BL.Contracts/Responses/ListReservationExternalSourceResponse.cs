using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Request class to be used in operations that search for a set of ReservationsExternalSource.
    /// </summary>
    [DataContract]
    public class ListReservationExternalSourceResponse : PagedResponseBase
    {
        [DataMember]
        public IList<ReservationExternalSource> Result { get; set; }
    }
}
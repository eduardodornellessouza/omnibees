using System.Collections.Generic;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for ReservationsExternalSources.
    /// </summary>
    [DataContract]
    public class InsertOrUpdateReservationExternalSourceRequest : PagedRequestBase
    {

        /// <summary>
        /// The list of ReservationsExternalSource's for which to Insert/Update.
        /// </summary>
        [DataMember]
        public List<ReservationExternalSource> ReservationsExternalSources { get; set; }
    }
}
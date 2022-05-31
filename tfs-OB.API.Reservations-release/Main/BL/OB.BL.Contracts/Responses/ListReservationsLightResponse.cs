using OB.Reservation.BL.Contracts.Data.Rates;
using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Response class used to store the Result of FindReservationLight operations.
    /// </summary>
    [DataContract]
    public class ListReservationsLightResponse : PagedResponseBase
    {
        public ListReservationsLightResponse()
        {
            Result = new List<ReservationLight>();
        }

        /// <summary>
        /// Collection with the ReservationLight instances that were find for the Request Criteria.
        /// </summary>
        [DataMember]
        public IList<ReservationLight> Result { get; set; }
    }
}
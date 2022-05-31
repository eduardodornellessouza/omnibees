using OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
  
    /// <summary>
    /// Response class used to store the Result of FindReservation operations.
    /// </summary>
    [DataContract]
    public class ListReservationsFilterResponse : PagedResponseBase
    {
        public ListReservationsFilterResponse()
        {
            Result = new List<ReservationFilter>();
        }

        /// <summary>
        /// Collection with the ReservationFilter instances that were find for the Request Criteria.
        /// </summary>
        [DataMember]
        public IList<ReservationFilter> Result { get; set; }
    }
}

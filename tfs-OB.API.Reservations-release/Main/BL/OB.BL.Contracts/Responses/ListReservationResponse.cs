using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Response class used to store the Result of FindReservation operations.
    /// </summary>
    [DataContract]
    public class ListReservationResponse : PagedResponseBase
    {
        public ListReservationResponse()
        {
            Result = new List<contractsReservation.Reservation>();
        }

        /// <summary>
        /// Collection with the Reservation instances that were find for the Request Criteria.
        /// </summary>
        [DataMember]
        public IList<contractsReservation.Reservation> Result { get; set; }
    }
}
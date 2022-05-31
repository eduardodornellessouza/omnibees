using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for ListPropertiesWithReservationsForChannelOrTPI.
    /// </summary>
    [DataContract]
    public class ListPropertiesWithReservationsForChannelOrTPIRequest : PagedRequestBase
    {

        /// <summary>
        /// The Channel_UID for which to find the Properties with Reservations.
        /// </summary>
        [DataMember]
        public long Channel_UID { get; set; }

        /// <summary>
        /// The TPI_UID for which to find the Properties with Reservations.
        /// </summary>
        [DataMember]
        public long? TPI_UID { get; set; }

        [DataMember]
        public List<long> PropertyUIDs { get; set; }

    }
}
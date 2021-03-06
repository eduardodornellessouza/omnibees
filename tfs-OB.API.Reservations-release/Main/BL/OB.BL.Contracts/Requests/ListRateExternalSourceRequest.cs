using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for ReservationsExternalSources.
    /// </summary>
    [DataContract]
    public class ListRateExternalSourceRequest : PagedRequestBase
    {

        /// <summary>
        /// The list of ExternalSource UID's for which to list the ReservationsExternalSources.
        /// </summary>
        [DataMember]
        public List<long> ExternalSource_UIDs { get; set; }

        /// <summary>
        /// List of Reservation_UID's for which to list the ReservationsExternalSources.
        /// </summary>
        [DataMember]
        public List<long> Rate_UIDs { get; set; }

        /// <summary>
        /// List of ExternalGuestID's for which to list the GuestsExternalSources.
        /// </summary>
        [DataMember]
        public List<string> ExternalRateIDs { get; set; }

        /// <summary>
        /// Boolean to select to exclude deleted Rates / RoomTypes from results.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }

    }
}
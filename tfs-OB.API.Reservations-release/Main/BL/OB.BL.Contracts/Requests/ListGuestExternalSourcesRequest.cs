using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for GuestsExternalSources.
    /// </summary>
    [DataContract]
    public class ListGuestExternalSourcesRequest : PagedRequestBase
    {

        /// <summary>
        /// The list of ExternalSource UID's for which to list the GuestsExternalSources.
        /// </summary>
        [DataMember]
        public List<long> ExternalSource_UIDs { get; set; }

        /// <summary>
        /// List of Guest_UID's for which to list the GuestsExternalSources.
        /// </summary>
        [DataMember]
        public List<long> Guest_UIDs { get; set; }

        /// <summary>
        /// List of ExternalGuestID's for which to list the GuestsExternalSources.
        /// </summary>
        [DataMember]
        public List<string> ExternalGuestIDs { get; set; }

        /// <summary>
        /// Boolean to select to exclude deleted Rates / RoomTypes from results.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }

    }
}
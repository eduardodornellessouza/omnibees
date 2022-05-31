using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a light set of Rates.
    /// </summary>
    [DataContract]
    public class ListRateRoomsRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the RateRooms to return
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// The list of UID's for the Rates in the RateRooms to return
        /// </summary>
        [DataMember]
        public List<long> RateUIDs { get; set; }

        /// <summary>
        /// The list of UID's for the RoomTypes in the RateRooms to return
        /// </summary>
        [DataMember]
        public List<long> RoomTypeUIDs { get; set; }

        /// <summary>
        /// Boolean to select to exclude deleted RateRooms from results.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }
    }
}
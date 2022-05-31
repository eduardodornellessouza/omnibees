using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a light set of RoomTypes.
    /// </summary>
    [DataContract]
    public class ListRoomTypeRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the RoomTypes to return
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// List of PropertyUID's for which to list the Rooms.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        /// <summary>
        /// Boolean to select to exclude deleted Rooms from results.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }
    }
}
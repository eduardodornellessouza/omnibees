using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a light set of Extras.
    /// </summary>
    [DataContract]
    public class ListExtraRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Extras to return
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// List of Name's for which to list the Extras.
        /// </summary>
        [DataMember]
        public List<string> Names { get; set; }

        /// <summary>
        /// List of PropertyUID's for which to list the Extras.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        /// <summary>
        /// Boolean to select returning only BoardType's Extras.
        /// </summary>
        [DataMember]
        public bool? IsBoardType { get; set; }

        /// <summary>
        /// List of BoardTypeUID's for which to list the Extras.
        /// </summary>
        [DataMember]
        public List<long> BoardTypeUIDs { get; set; }

        /// <summary>
        /// Boolean to select to exclude deleted Extras from results.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }

        /// <summary>
        /// Boolean to select to returning only Active Extras from results.
        /// </summary>
        [DataMember]
        public bool? IsActive { get; set; }
    }
}
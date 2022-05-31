using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Prefix's.
    /// </summary>
    [DataContract]
    public class ListPrefixRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Prefix's to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// The list of Names for the Prefix's to return.
        /// </summary>
        [DataMember]
        public List<string> Names { get; set; }

    }
}
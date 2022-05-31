using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Languages.
    /// </summary>
    [DataContract]
    public class ListLanguageRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Languages to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// The list of Names for the Languages to return.
        /// </summary>
        [DataMember]
        public List<string> Names { get; set; }

        /// <summary>
        /// The list of ISO Codes for the Languages to return.
        /// </summary>
        [DataMember]
        public List<string> Codes { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of ExternalSources.
    /// </summary>
    [DataContract]
    public class ListExternalSourceRequest : PagedRequestBase
    {
        public ListExternalSourceRequest()
        {
            IncludeUserInformation = true;
        }

        /// <summary>
        /// The list of UID's for the ExternalSources to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// List of ClientUID's for which to list the ExternalSources.
        /// </summary>
        [DataMember]
        public List<long> UserUIDs { get; set; }

        /// <summary>
        /// List of Names for which to list the ExternalSources.
        /// </summary>
        [DataMember]
        public List<string> Names { get; set; }

        /// <summary>
        /// List User Information of ExternalSources
        /// Default is true
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        [DefaultValue(true)]
        public bool IncludeUserInformation { get; set; }
    }
}
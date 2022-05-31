using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of TPIs.
    /// </summary>
    [DataContract]
    public class ListThirdPartyIntermediariesLightRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }
        
        /// <summary>
        /// The list of TPI_Types for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<byte> TPI_Types { get; set; }

        /// <summary>
        /// Return only Public TPIs.
        /// </summary>
        [DataMember]
        public bool? IsPublic { get; set; }
                
        /// <summary>
        /// The status of IsActive for the TPIs to return.
        /// </summary>
        [DataMember]
        public bool? IsActive { get; set; }
        
        /// <summary>
        /// Exclude Deleteds TPIs from response.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }
    }
}
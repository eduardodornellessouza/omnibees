using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of TPIs.
    /// </summary>
    [DataContract]
    public class ListThirdPartyIntermediaryLightRequest : PagedRequestBase
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
        /// The list of PropertyUID's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<long?> PropertyUIDs { get; set; }

        /// <summary>
        /// The list of CountryUID's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<long> CountryUIDs { get; set; }

        /// <summary>
        /// The list of UserName's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<string> UserNames { get; set; }

        /// <summary>
        /// The list of CurrencyUID's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<long> CurrencyUIDs { get; set; }

        /// <summary>
        /// The list of LanguageUID's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<long> LanguageUIDs { get; set; }

        /// <summary>
        /// The list of Email's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<string> Emails { get; set; }

        /// <summary>
        /// The list of FirstNames for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<string> Names { get; set; }
                
        /// <summary>
        /// The status of IsActive for the TPIs to return.
        /// </summary>
        [DataMember]
        public bool? IsActive { get; set; }
        
        /// <summary>
        /// To Return only Public TPIs.
        /// </summary>
        [DataMember]
        public bool? IsPublic { get; set; }
        
        /// <summary>
        /// To Return Country Names in response.
        /// </summary>
        [DataMember]
        public bool? IncludeCountryName { get; set; }
        
        /// <summary>
        /// Exclude Deleteds TPIs from response.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }
                                
        /// <summary>
        /// The list of ClientUID's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<long?> ClientUIDs { get; set; }

        /// <summary>
        /// The list of PCC's for the TPIs to return.
        /// </summary>
        [DataMember]
        public List<string> PCC { get; set; }
    }
}
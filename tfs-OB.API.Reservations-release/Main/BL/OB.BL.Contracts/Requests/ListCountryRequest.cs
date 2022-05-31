using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Countries.
    /// </summary>
    [DataContract]
    public class ListCountryRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Countries to return.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// The list of Names for the Countries to return.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> Names { get; set; }

        /// <summary>
        /// The list of CountryCodes in ISO Format for the Countries to return.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// The list of GeoNameID's for the Countries to return.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> GeoNameIDs { get; set; }

        /// <summary>
        /// Iso language to translate countries
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Language { get; set; }

    }
}
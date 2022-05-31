using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Properties and its external sources.
    /// </summary>
    [DataContract]
    public class ListPropertiesExternalSourcePermissionsRequest : PagedRequestBase
    {
        public ListPropertiesExternalSourcePermissionsRequest()
        {
        }

        /// <summary>
        /// Filter results by UIDs
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// Filter results by property UID
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertiesExternalSourcesUids { get; set; }

        /// <summary>
        /// Filter results by Property
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertiesUids { get; set; }

        /// <summary>
        /// True to return only active external sources in the property, false to return all external sources available
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool WithPermissionOnly { get; set; }

        /// <summary> Add in response Property information, external source information, rates
        /// information and roomtypes information If true it will be included in response:
        /// Dictionary&lt;long, RateLight&lt; RatesLookup Dictionary&lt;long, RoomType&lt; RoomTypesLookup </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool SendLookups { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Language_UID { get; set; }
    }
}
using OB.Reservation.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Properties and its external sources.
    /// </summary>
    [DataContract]
    public class ListPropertiesExternalSourceRequest : PagedRequestBase
    {
        public ListPropertiesExternalSourceRequest()
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
        public List<long> FilterPropertyUid { get; set; }

        /// <summary>
        /// Filter results by active external source UID
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> FilterExternalSourceUid { get; set; }

        /// <summary>
        /// True to return only active external sources in the property, false to return all external sources available
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool ActivePropertiesExternalSourcesOnly { get; set; }

        /// <summary>
        /// true to return external sources RateRoom permissions
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeExternalSourcesPermissions { get; set; }

        /// <summary>
        /// true to return external source type
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeExternalSourceType { get; set; }        

        /// <summary>
        /// Add in response Property information, external source information, rates information and roomtypes information
        /// If true it will be included in response:
        /// <![CDATA[
        /// Dictionary<long, PropertyLight> PropertiesLookup
        /// Dictionary<long, ExternalSource> ExternalSourcesLookup
        /// Dictionary<long, RateLight> RatesLookup
        /// Dictionary<long, RoomType> RoomTypesLookup
        /// ]]>
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool SendLookups { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Language_UID { get; set; }
    }
}

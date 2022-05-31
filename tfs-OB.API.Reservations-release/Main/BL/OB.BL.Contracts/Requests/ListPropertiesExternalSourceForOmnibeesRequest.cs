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
    public class ListPropertiesExternalSourceForOmnibeesRequest : GridPagedRequest
    {
        public ListPropertiesExternalSourceForOmnibeesRequest()
        {

        }

        /// <summary>
        /// Filter results by user UID
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UserUid { get; set; }

        /// <summary>
        /// Filter results by property UID
        /// </summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<long> FilterPropertyUid { get; set; }

        /// <summary>
        /// Filter results by property Name
        /// </summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public string FilterPropertyName { get; set; }

        /// <summary>
        /// Filter results by active external source UID
        /// </summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<long> FilterExternalSourceUid { get; set; }

        /// <summary>
        /// Filter results by external source Name
        /// </summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public string FilterExternalSourceName { get; set; }

        /// <summary>
        /// Add in response Property information, external source information, rates information and roomtypes information
        /// If true it will be included in response:
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

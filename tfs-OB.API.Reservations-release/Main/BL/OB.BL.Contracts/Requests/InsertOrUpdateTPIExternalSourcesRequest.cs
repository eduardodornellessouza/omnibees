using System.Collections.Generic;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.CRM;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for TPIsExternalSources.
    /// </summary>
    [DataContract]
    public class InsertOrUpdateTPIExternalSourcesRequest : PagedRequestBase
    {

        /// <summary>
        /// The list of TPIsExternalSource's for which to Insert/Update.
        /// </summary>
        [DataMember]
        public List<TPIsExternalSource> TPIsExternalSources { get; set; }
    }
}
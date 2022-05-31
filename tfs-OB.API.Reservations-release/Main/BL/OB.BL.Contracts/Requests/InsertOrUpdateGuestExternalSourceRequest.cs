using System.Collections.Generic;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.CRM;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for GuestsExternalSources.
    /// </summary>
    [DataContract]
    public class InsertOrUpdateGuestExternalSourceRequest : PagedRequestBase
    {

        /// <summary>
        /// The list of GuestsExternalSource's for which to Insert/Update.
        /// </summary>
        [DataMember]
        public List<GuestsExternalSource> GuestsExternalSources { get; set; }
    }
}
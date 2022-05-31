using System.Collections.Generic;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.CRM;
using System.Collections.ObjectModel;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Request class to be used in operations that search for a set of GuestsExternalSource.
    /// </summary>
    [DataContract]
    public class ListTPIsExternalSourcesResponse : PagedResponseBase
    {
        [DataMember]
        public IList<TPIsExternalSource> Result { get; set; }
    }
}
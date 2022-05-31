using OB.Reservation.BL.Contracts.Data.General;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of Setting objects.
    /// </summary>
    [DataContract]
    public class ListSettingResponse : PagedResponseBase
    {
        [DataMember]
        public IList<Setting> Result { get; set; }
    }
}
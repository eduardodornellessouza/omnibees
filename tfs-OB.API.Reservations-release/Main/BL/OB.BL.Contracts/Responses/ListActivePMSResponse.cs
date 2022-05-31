using OB.Reservation.BL.Contracts.Data.PMS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of Channel objects.
    /// </summary>
    [DataContract]
    public class ListActivePMSResponse : PagedResponseBase
    {
        [DataMember]
        public List<PMSConfiguration> Result { get; set; }
    }
}
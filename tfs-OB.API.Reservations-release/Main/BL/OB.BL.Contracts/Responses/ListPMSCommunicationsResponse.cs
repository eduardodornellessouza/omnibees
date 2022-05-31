using OB.Reservation.BL.Contracts.Data.PMS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPMSCommunicationsResponse : PagedResponseBase
    {
        [DataMember]
        public List<PMSActivityMessage> Result { get; set; }
    }
}
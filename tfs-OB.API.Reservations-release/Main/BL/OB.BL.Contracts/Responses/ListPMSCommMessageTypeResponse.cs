using OB.Reservation.BL.Contracts.Data.General;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPMSCommMessageTypeResponse : PagedResponseBase
    {
        [DataMember]
        public List<PMSCommMessageType> Result { get; set; }
    }
}
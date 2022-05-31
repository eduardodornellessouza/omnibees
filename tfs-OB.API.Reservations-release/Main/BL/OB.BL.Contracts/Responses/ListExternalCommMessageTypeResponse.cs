using OB.Reservation.BL.Contracts.Data.General;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListExternalCommMessageTypeResponse : PagedResponseBase
    {
        [DataMember]
        public List<ExternalCommMessageType> Result { get; set; }
    }
}
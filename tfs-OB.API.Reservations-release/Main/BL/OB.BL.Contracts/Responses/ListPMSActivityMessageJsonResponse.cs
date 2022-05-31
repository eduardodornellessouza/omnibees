using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPMSActivityMessageJsonResponse : PagedResponseBase
    {
        [DataMember]
        public Dictionary<Guid, string> Result { get; set; }
    }
}
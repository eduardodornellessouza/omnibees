using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListExternalSourceResponse : PagedResponseBase
    {
        [DataMember]
        public IList<ExternalSource> Result { get; set; }
    }
}
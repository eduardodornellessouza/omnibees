using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListExtraResponse : PagedResponseBase
    {
        [DataMember]
        public IList<Extra> Result { get; set; }
    }
}
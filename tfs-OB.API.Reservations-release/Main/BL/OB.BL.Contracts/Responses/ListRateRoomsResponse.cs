using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListRateRoomsResponse : PagedResponseBase
    {
        [DataMember]
        public IList<RateRoom> Result { get; set; }
    }
}
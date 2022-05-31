using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListBoardTypeResponse : PagedResponseBase
    {
        [DataMember]
        public IList<BoardType> Result { get; set; }
    }
}
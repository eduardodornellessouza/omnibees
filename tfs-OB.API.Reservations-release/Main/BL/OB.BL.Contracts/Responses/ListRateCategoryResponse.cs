using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListRateCategoryResponse : PagedResponseBase
    {
        [DataMember]
        public IList<RateCategory> Result { get; set; }
    }
}
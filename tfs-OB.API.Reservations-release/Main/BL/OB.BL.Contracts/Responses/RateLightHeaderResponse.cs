using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class RateLightHeaderResponse : PagedResponseBase
    {
        [DataMember]
        public IList<RateHeaderLightResume> Result { get; set; }
    }




}
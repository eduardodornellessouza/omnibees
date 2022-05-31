using OB.Reservation.BL.Contracts.Data.CRM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListLoyaltyLevelLimitsPeriodicityResponse : PagedResponseBase
    {
        [DataMember]
        public IList<LoyaltyLevelLimitsPeriodicity> Result { get; set; }
    }
}

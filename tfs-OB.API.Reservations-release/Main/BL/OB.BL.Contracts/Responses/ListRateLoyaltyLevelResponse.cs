using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OB.Reservation.BL.Contracts.Data.CRM;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListRateLoyaltyLevelResponse : PagedResponseBase
    {
        public ListRateLoyaltyLevelResponse()
        {
            Results = new List<RateLoyaltyLevel>();
        }

        [DataMember]
        public List<RateLoyaltyLevel> Results { get; set; }
    }
}

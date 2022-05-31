using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateRateLoyaltyLevelsRequest : RequestBase
    {
        [DataMember]
        public List<RateLoyaltyLevel> updatedObjs { get; set; }

        [DataMember]
        public long Rate_UID { get; set; }
    }
}

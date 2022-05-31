using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class LoyaltyLevelLimitsPeriodicity : ContractBase
    {
        public LoyaltyLevelLimitsPeriodicity()
        {
        }

        [DataMember]
        public int UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }


        [DataMember]
        public List<LoyaltyLevelLimitsPeriodicityLanguage> LoyaltyLevelLimitsPeriodicityLanguages { get; set; }
    }
}

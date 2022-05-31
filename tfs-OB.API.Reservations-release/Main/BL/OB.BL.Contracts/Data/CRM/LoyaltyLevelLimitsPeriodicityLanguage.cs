using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class LoyaltyLevelLimitsPeriodicityLanguage : ContractBase
    {
        public LoyaltyLevelLimitsPeriodicityLanguage()
        {
        }

        [DataMember]
        public int UID { get; set; }
    
        [DataMember]
        public int LoyaltyLevelLimitsPeriodicity_UID { get; set; }
    
        [DataMember]
        public long Language_UID { get; set; }
    
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }
    }
}

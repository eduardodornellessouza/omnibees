using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{

    [DataContract]
    public class LoyaltyLevelsGeneratedLinksForBERegister : ContractBase
    {

        public LoyaltyLevelsGeneratedLinksForBERegister()
        { 
        }

        [DataMember]
        public System.Guid GUID { get; set; }

        [DataMember]
        public long Client_UID { get; set; }

        [DataMember]
        public long LoyaltyLevel_UID { get; set; }

        [DataMember]
        public bool WasUsed { get; set; }

        [DataMember]
        public long CreatedBy { get; set; }

        [DataMember]
        public System.DateTime CreatedDate { get; set; }

        [DataMember]
        public System.DateTime ExpireDate { get; set; }

    }
}

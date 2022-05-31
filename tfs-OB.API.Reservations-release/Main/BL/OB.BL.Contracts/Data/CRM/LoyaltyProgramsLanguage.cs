using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class LoyaltyProgramsLanguage : ContractBase
    {
        public LoyaltyProgramsLanguage()
        { 
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Language_UID { get; set; }

        [DataMember]
        public long LoyaltyProgram_UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public byte[] Revision { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }
    }
}

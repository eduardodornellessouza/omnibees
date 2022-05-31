using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class LoyaltyLevelsCurrency : ContractBase
    {
        public LoyaltyLevelsCurrency()
        { 
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Currency_UID { get; set; }

        [DataMember]
        public long LoyaltyLevel_UID { get; set; }

        [DataMember]
        public decimal Value { get; set; }

        [DataMember]
        public byte[] Revision { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }
    }
}

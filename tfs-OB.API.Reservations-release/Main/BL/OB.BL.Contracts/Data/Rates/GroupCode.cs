namespace OB.Reservation.BL.Contracts.Data.Rates
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class GroupCode : ContractBase
    {
        public GroupCode()
        {

        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string InternalCode { get; set; }

        [DataMember]
        public string GroupCode1 { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }
        
        [DataMember]
        public Nullable<System.DateTime> DateFrom { get; set; }

        [DataMember]
        public Nullable<System.DateTime> DateTo { get; set; }

        [DataMember]
        public Nullable<System.DateTime> BeginSell { get; set; }

        [DataMember]
        public Nullable<System.DateTime> EndSell { get; set; }

        [DataMember]
        public Nullable<long> Rate_UID { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}

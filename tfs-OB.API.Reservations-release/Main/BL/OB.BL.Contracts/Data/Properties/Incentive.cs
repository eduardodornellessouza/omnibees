using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Properties
{
    /// <summary>
    /// Incentive Data Transfer Object
    /// </summary>
    [DataContract]
    public class Incentive : ContractBase
    {
        public Incentive()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public OB.Reservation.BL.Constants.IncentiveType IncentiveType { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int? Days { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> FreeDays { get; set; }

        [DataMember]
        public int DiscountPercentage { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsFreeDaysAtBegin { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }

        [DataMember]
        public int MinDays { get; set; }

        [DataMember]
        public int MaxDays { get; set; }

        [DataMember]
        public int Hours { get; set; }

        [DataMember]
        public bool IsBetweenNights { get; set; }

        [DataMember]
        public bool IsLastMinuteInHours { get; set; }

    }
}
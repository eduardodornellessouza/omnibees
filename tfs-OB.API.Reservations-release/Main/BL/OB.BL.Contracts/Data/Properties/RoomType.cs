using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Properties
{
    [DataContract]
    public class RoomType : ContractBase
    {
        public RoomType()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Qty { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ShortDescription { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> AdultMaxOccupancy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> AdultMinOccupancy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> AcceptsChildren { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> ChildMaxOccupancy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> ChildMinOccupancy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> BasePrice { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsBase { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsValueDecrease { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> MaxValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> MinValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> AcceptsExtraBed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> MaxOccupancy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> MaxFreeChild { get; set; }
    }
}
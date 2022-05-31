using OB.Reservation.BL.Contracts.Data.General;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Rates
{
    [DataContract]
    public class Extra : ContractBase
    {
        public Extra()
        {
            BillingTypes = new List<BillingType>();
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Image_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ExtraBillingType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<short> VAT { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string NotificationEmail { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsBoardType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> BoardType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> ExtraOrder { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<BillingType> BillingTypes { get; set; }
    }
}
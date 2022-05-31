using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.General
{
    [DataContract]
    public class User : ContractBase
    {
        public User()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Prefix_UID { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LastName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Phone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PhoneExt { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string UserName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Language_UID { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Category_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> PasswordExpiryDate { get; set; }

        [DataMember]
        public System.DateTime CreateDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> Birthday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Client_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> LastLogin { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsLocked { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ModifyBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifyDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string UserSettings { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RoleType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDummyUser { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class Guest : ContractBase
    {
        public Guest()
        {
        }
        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Prefix { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LastName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Address1 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Address2 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string City { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PostalCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingAddress1 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingAddress2 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingCity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingPostalCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingPhone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingExt { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> BillingCountry_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Country_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Phone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PhoneExt { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string MobilePhone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string UserName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string UserPassword { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PasswordHint { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> GuestCategory_UID { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public long Currency_UID { get; set; }

        [DataMember]
        public long Language_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> Birthday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> CreatedByTPI_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> LastLoginDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string FacebookUser { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TwitterUser { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TripAdvisorUser { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> AllowMarketing { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ModifyBy { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifyDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Question_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string State { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingState { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsFacebookFan { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsTwitterFollower { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string IDCardNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> BillingState_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> State_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingEmail { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Client_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingContactName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingTaxCardNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> UseDifferentBillingInfo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsImportedFromExcel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CreateDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Gender { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ExternalSource_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? LoyaltyLevel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<GuestSocialProvider> GuestSocialProviders { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<GuestActivity> GuestActivities { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<GuestFavoriteExtra> GuestFavoriteExtras { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<GuestFavoriteSpecialRequest> GuestFavoriteSpecialRequests { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PrefixName { get; set; }
    }
}
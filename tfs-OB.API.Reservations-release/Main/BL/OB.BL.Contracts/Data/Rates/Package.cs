using OB.BL.Contracts.Validators;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class Package : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        //[Required]
        public Nullable<DateTime> DateFrom { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        //[Required]
        public Nullable<DateTime> DateTo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [CustomValidation(typeof(CustomValidator), "ValidateDropDown")]
        public long Rate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<DateTime> BeginSale { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        //[CustomValidation(typeof(CustomValidator), "ValidatePackageSaleBeginEndDate")]
        public Nullable<DateTime> EndSale { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [Required]
        public Int16 Days { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPackageExtensible { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsStayExtensible { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<Int16> MaxDays { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> RateForExtendedStay_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] AttachmentPdf { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActive { get; set; }
    }
}
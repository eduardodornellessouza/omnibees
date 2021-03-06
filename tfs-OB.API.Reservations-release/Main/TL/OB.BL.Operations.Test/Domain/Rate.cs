//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OB.BL.Operations.Test.Domain.Rates
{
    using System;
    using System.Collections.Generic;
    
    using OB.Domain;
    using System.CodeDom.Compiler;
    public partial class Rate 
    {
        public long UID { get; set; }
        public string Name { get; set; }
        public Nullable<long> RateTier_UID { get; set; }
        public Nullable<long> Rate_UID { get; set; }
        public Nullable<long> RateCategory_UID { get; set; }
        public bool IsPriceDerived { get; set; }
        public Nullable<decimal> Value { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsValueDecrease { get; set; }
        public bool IsYielding { get; set; }
        public bool IsAvailableToTPI { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public Nullable<long> DepositPolicy_UID { get; set; }
        public Nullable<long> CancellationPolicy_UID { get; set; }
        public Nullable<long> OtherPolicies { get; set; }
        public long Property_UID { get; set; }
        public bool IsParity { get; set; }
        public Nullable<System.DateTime> BeginSale { get; set; }
        public Nullable<System.DateTime> EndSale { get; set; }
        public Nullable<int> RateOrder { get; set; }
        public string Description { get; set; }
        public Nullable<long> RateFolder_UID { get; set; }
        public bool IsExclusiveForPackage { get; set; }
        public bool IsExclusiveForGroupCode { get; set; }
        public bool PriceModel { get; set; }
        public string GDSSabreRateName { get; set; }
        public byte[] Revision { get; set; }
        public byte[] AttachmentPdf { get; set; }
        public string PdfName { get; set; }
        public bool IsAllExtrasIncluded { get; set; }
        public long Currency_UID { get; set; }
        public bool IsCurrencyChangeAllowed { get; set; }
        public Nullable<long> ExternalSource_UID { get; set; }

        public virtual ICollection<RateCancellationPolicy> RateCancellationPolicies { get; set; }
        public virtual ICollection<Rate> Rates1 { get; set; }
        public virtual Rate Rate1 { get; set; }
        public virtual ICollection<RatesChannel> RatesChannels { get; set; }
        public virtual CancellationPolicy CancellationPolicy { get; set; }
    }
}

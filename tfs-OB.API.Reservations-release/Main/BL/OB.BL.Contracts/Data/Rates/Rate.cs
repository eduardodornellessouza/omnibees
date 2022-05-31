using OB.Reservation.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Rates
{
    [DataContract]
    public class Rate : ContractBase
    {
        public Rate()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? Rate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPriceDerived { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsValueDecrease { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsYielding { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsAvailableToTPI { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? DepositPolicy_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? CancellationPolicy_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? OtherPolicy_UID { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsParity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? BeginSale { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? EndSale { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool PriceModel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GDSSabreRateName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsAllExtrasIncluded { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Currency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencyISO { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int AvailabilityType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PromotionalCode> PromotionalCodes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Incentive> Incentives { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int OtaCodeValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string InternalName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RateCategoryId { get; set; }
    }
}
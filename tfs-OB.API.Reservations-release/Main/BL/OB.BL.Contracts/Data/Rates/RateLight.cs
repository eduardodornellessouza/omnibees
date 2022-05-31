using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateLight : ContractBase
    {
        public RateLight()
        {
        }

        [DataMember]
        public long Index { get; set; }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? BoardType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BoardType_Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<RateRoomLight> RateRoomsLight { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> RateCategory_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string RateCategory_OTACodeName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RateCategory_OTACodeValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsYielding { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPriceDerived { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Rate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsAvailableToTPI { get; set; }

        [DataMember]
        public long CreatedBy { get; set; }

        [DataMember]
        public System.DateTime CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ModifiedBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsExclusiveForPackage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsExclusiveForGroupCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> BeginSale { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> EndSale { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool PriceModel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Currency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencyISO { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencySymbol { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsAllExtrasIncluded { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ExternalSource_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsActive { get; set; }


        // Policies
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DepositPolicy DefaultDepositPolicy { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public CancellationPolicy DefaultCancellationPolicy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public OtherPolicy GeneralPolicy { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<RateDepositPolicy> DepositPolicies { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<RateCancellationPolicy> CancellationPolicies { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<RateTaxPolicy> TaxPolicies { get; set; }


        // Extras
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<RateExtra> IncludedExtras { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<RateExtra> AdditionalExtras { get; set; }


        // Incentives
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<RateIncentive> Incentives { get; set; }
        

        // RateBuyerGroups
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<RateBuyerGroup> RateBuyerGroups { get; set; }

    }
}
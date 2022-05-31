using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateChannel
    {
        public RateChannel()
        {
        }


        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Rate_UID { get; set; }

        [DataMember]
        public long Channel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string RateCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> RateModel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ChannelCommissionCategory_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Createdby { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ModifiedBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ContractName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string IncomingOfficeCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Sequence { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> PriceAddOnIsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> PriceAddOnIsValueDecrease { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> PriceAddOnValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Currency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> CustomExchangeRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsCustomExchangeRateSelected { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Commission { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Markup { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Package { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PaymentTypesUIDs { get; set; }
    }
}

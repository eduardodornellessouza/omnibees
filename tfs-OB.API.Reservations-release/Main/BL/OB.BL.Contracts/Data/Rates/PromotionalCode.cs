using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Rates
{
    [DataContract]
    public class PromotionalCode : ContractBase
    {
        public PromotionalCode()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsValid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ValidFrom { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ValidTo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> MaxReservations { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> ReservationsCompleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> DiscountValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsCommission { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string URL { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsRegisterTPI { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> PromotionalCode_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsPromotionalCodeVisibleRate { get; set; }
    }
}
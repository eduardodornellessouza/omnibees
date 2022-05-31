using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationRoomDetailsAppliedPromotionalCode : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ReservationRoomDetail_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PromotionalCode_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public System.DateTime Date { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal DiscountValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> DiscountPercentage { get; set; }
    }
}
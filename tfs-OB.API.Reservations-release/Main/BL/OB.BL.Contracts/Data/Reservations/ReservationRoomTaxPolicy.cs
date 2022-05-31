namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using OB.Reservation.BL.Contracts.Data.Rates;
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoomTaxPolicy : ContractBase
    {
        public ReservationRoomTaxPolicy()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ReservationRoom_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BillingType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> TaxId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TaxName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TaxDescription { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> TaxDefaultValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> TaxIsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> TaxCalculatedValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TaxPolicy TaxPolicy { get; set; }
    }
}
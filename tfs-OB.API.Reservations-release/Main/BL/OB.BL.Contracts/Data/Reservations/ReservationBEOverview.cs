using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationBEOverview
    {
        public ReservationBEOverview()
        { }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Reservation_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ReservationRoom_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationRoomNo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateFrom { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int Nights { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ReservationTotal { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int ReservationBaseCurrency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int ReservationCurrency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ReservationCurrencyExchangeRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyBaseCurrency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PropertyBaseCurrencyExchangeRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int ReservationRoomStatus { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int ReservationStatus { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal CommissionValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ReservationCommission { get; set; }
    }
}

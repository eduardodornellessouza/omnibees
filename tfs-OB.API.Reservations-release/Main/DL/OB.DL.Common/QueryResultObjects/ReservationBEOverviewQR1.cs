using System;
using System.Collections.Generic;

namespace OB.DL.Common.QueryResultObjects
{
    /// <summary>
    /// Information About the Reservation
    /// For Information About the ReservationRoom Please Use the Property - ReservationRooms
    /// </summary>
    ///
    [Serializable]
    public partial class ReservationBEOverviewQR1
    {
        public long Reservation_UID { get; set; }
        public long ReservationRoom_UID { get; set; }
        public string ReservationRoomNo { get; set; }
        public DateTime DateFrom { get; set; }
        public int Nights { get; set; }
        public decimal ReservationTotal { get; set; }
        public int ReservationBaseCurrency_UID { get; set; }
        public int ReservationCurrency_UID { get; set; }
        public decimal ReservationCurrencyExchangeRate { get; set; }
        public long PropertyBaseCurrency_UID { get; set; }
        public decimal PropertyBaseCurrencyExchangeRate { get; set; }
        public long Property_UID { get; set; }
        public int ReservationRoomStatus { get; set; }
        public int ReservationStatus { get; set; }
        public decimal CommissionValue { get; set; }
        public decimal ReservationCommission { get; set; }
    }
}

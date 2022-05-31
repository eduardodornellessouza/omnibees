using System;
using System.Collections.Generic;

namespace OB.DL.Common.QueryResultObjects
{
    /// <summary>
    /// To see Daily Information About the Room
    /// For Information About the ReservationRoom Please Use the Parent Struture - BookingEngineReservationRoom
    /// </summary>
    ///
    [Serializable]
    public partial class ReservationRoomDetailQR1
    {
        public long UID { get; set; }

        public long ReservationRoom_UID { get; set; }

        public string RoomTypeName { get; set; }

        public Nullable<long> RateRoomDetailId { get; set; }

        public Nullable<long> RommTypeId { get; set; }

        public Nullable<Decimal> AdultPrice { get; set; }

        public Nullable<Decimal> ChildPrice { get; set; }

        public Nullable<Decimal> DailyTotalPrice { get; set; }

        public string DailyTotalPriceFormated { get; set; }

        public DateTime Date { get; set; }

        public Nullable<decimal> RateValue { get; set; }

        public string RateValueFormated { get; set; }

        public List<int?> ChildAges { get; set; }

        //TMOREIRA:ENTITY FRAMEWORK OBJECTS SHOULDN'T BE HERE !!! Otherwise it can blow with the Database and IIS when the object is serialized.
        //public List<ReservationRoomDetailsAppliedIncentive> AppliedIncentives { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.CheckIn")]
        public Nullable<DateTime> ChekIn { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.CheckOut")]
        public Nullable<DateTime> ChekOut { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.AdultCount")]
        public Nullable<int> AdultCount { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.ChildCount")]
        public Nullable<int> ChildCount { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.RoomGuestName")]
        public string RoomGuestName { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.TotalAmount")]
        public Nullable<Decimal> TotalAmount { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.TotalTax")]
        public Nullable<Decimal> TotalTax { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.RateName")]
        public string RateName { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.RateID")]
        public Nullable<long> RateID { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.IsPackage")]
        public bool IsPackage { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.Package_UID")]
        public Nullable<long> Package_UID { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.GuestName")]
        public string GuestName { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservationRoom.ReservationNumber")]
        public string Number { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservation.PropertyName")]
        public string PropertyName { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservation.CurrencySymbol")]
        public string CurrencySymbol { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservation.CurrencySymbolName")]
        public string CurrencySymbolName { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservation.ChannelName")]
        public string ChannelName { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservation.GroupCodeName")]
        public string GroupCodeName { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservation.PromoCodeName")]
        public string PromoCodeName { get; set; }

        [Obsolete("Deprecated Use - BookingEngineReservation.ChannelAffiliateName")]
        public string ChannelAffiliateName { get; set; }

        public Nullable<decimal> Price { get; set; }
    }
}
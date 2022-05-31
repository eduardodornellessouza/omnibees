using System;
using System.Collections.Generic;

namespace OB.DL.Common.QueryResultObjects
{
    /// <summary>
    /// Information About the Reservation Room
    /// To see Daily Information of the Room Please Use the Property - ReservationRoomDetail
    /// </summary>
    ///
    [Serializable]
    public partial class ReservationRoomQR1
    {
        public long Reservation_UID { get; set; }

        public long ReservationRoom_UID { get; set; }

        public Nullable<long> RommTypeId { get; set; }

        public string Room_Type { get; set; }

        public string GuestName { get; set; }

        public Nullable<DateTime> CheckIn { get; set; }

        public Nullable<DateTime> CheckOut { get; set; }

        public Nullable<int> NumberDays { get; set; }

        public Nullable<int> Adults { get; set; }

        public Nullable<int> Childrens { get; set; }

        //NEW Propertys - Remove from ReservationRoomDetails

        public bool IsPackage { get; set; }

        public Nullable<long> Package_UID { get; set; }

        public Nullable<Decimal> TotalTax { get; set; }

        public string TotalTaxFormated { get; set; }

        public string CancellationPoliciy { get; set; }

        public string DepositPoliciy { get; set; }

        public string OtherPoliciy { get; set; }

        public Nullable<int> CancellationPoliciy_Days { get; set; }

        public string ReservationNumber { get; set; }

        public string RoomTypeName { get; set; }

        public Nullable<long> RateID { get; set; }

        public Nullable<long> RateRoomDetailId { get; set; }

        public string RateName { get; set; }

        public Nullable<Decimal> TotalAmount { get; set; }

        public string TotalAmountFormated { get; set; }

        public Nullable<Decimal> ReservationRoomPriceSum { get; set; }

        public string ReservationRoomPriceSumFormated { get; set; }

        public Nullable<Decimal> ReservationRoomExtrasPriceSum { get; set; }

        public string ReservationRoomExtrasPriceSumFormated { get; set; }

        public Nullable<Decimal> ReservationRoomTotalAmount { get; set; }

        public string ReservationRoomTotalAmountFormated { get; set; }

        public TimeSpan? ArrivalTime { get; set; }

        public string ArrivalTimeFormatted { get; set; }

        /// <summary>
        /// Daily Details of the Room
        /// </summary>
        public List<ReservationRoomDetailQR1> ReservationRoomDetail { get; set; }

        /// <summary>
        /// Daily Details of the Room Extras
        /// </summary>
        public List<ReservationRoomExtraQR1> ReservationRoomExtras { get; set; }

        /// <summary>
        /// Daily Details of the Room Incentives
        /// </summary>
        public List<ReservationRoomIncentiveQR1> ReservationRoomIncentives { get; set; }

        /// <summary>
        /// reservation room taxes
        /// </summary>
        public List<ReservationRoomTaxPolicyQR1> ReservationRoomTaxPolicies { get; set; }

        /// <summary>
        /// List of child ages
        /// </summary>
        public List<int?> ChildAges { get; set; }

        // room status
        public int? Status { get; set; }

        public string StatusFormatted { get; set; }

        public string RateDescription { get; set; }

        public bool? IsCancellationAllowed { get; set; }

        public bool? CancellationCosts { get; set; }

        public decimal? CancellationValue { get; set; }

        public int? CancellationPaymentModel { get; set; }

        public int? CancellationNrNights { get; set; }

        public bool? TPIDiscountIsPercentage { get; set; }

        public bool? TPIDiscountIsValueDecrease { get; set; }

        public decimal? TPIDiscountValue { get; set; }

        public string TaxName { get; set; }

        public bool? TaxIsPercentage { get; set; }

        public decimal? TaxDefaultValue { get; set; }

        public decimal? CommissionValue { get; set; }

        public long? CommissionType { get; set; }

        public string ReservationRoomNo { get; set; }
    }
}
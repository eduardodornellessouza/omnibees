using System;
using System.ComponentModel.DataAnnotations;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public partial class ReservationRoomCustom
    {
        public long Reservation_UID { get; set; }

        public int RoomQty { get; set; }

        public long RoomType_UID { get; set; }

        public string RoomName { get; set; }

        public string guestName { get; set; }

        public Nullable<DateTime> DateFrom { get; set; }

        public Nullable<DateTime> DateTo { get; set; }

        public Nullable<int> AdultCount { get; set; }

        public Nullable<int> ChildCount { get; set; }

        [Key]
        public long UID { get; set; }

        public Nullable<long> Package_UID { get; set; }

        public string CancellationPolicy { get; set; }

        public string OtherPolicy { get; set; }

        public string DepositPolicy { get; set; }

        public Nullable<int> CancellationPolicyDays { get; set; }

        public string ReservationRoomNo { get; set; }

        public string ReservationNumber { get; set; }

        public string RoomNumber { get; set; }

        public bool Status { get; set; }

        public decimal RoomPrice { get; set; }

        public Nullable<decimal> RoomTax { get; set; }

        public bool? IsCancellationAllowed { get; set; }

        public Boolean CancellationCosts { get; set; }
        public decimal? CancellationValue { get; set; }
        public int? CancellationPaymentModel { get; set; }
        public int? CancellationNrNights { get; set; }

        public string PmsReservationNumber { get; set; }
        public Nullable<bool> SmokingPreferences { get; set; }
        public Nullable<decimal> TotalTax { get; set; }
    }

}

using System;

namespace OB.DL.Common.QueryResultObjects
{
    public class AppliedPromotionalCodeQR1
    {
        public long UID { get; set; }
        public long ReservationRoomDetail_UID { get; set; }
        public long PromotionalCode_UID { get; set; }
        public DateTime Date { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? DiscountPercentage { get; set; }
    }
}

using System;
namespace OB.DL.Common.QueryResultObjects
{
    public class GuestLoyaltyReservationQR1
    {
        public DateTime DateTo { get; set; }
		public DateTime DateFrom { get; set; }
		public long ReservationId { get; set; }
		public decimal TotalAmount { get; set; }
        public long BaseCurrency_UID { get; set; }
        public long TimeZone_UID { get; set; }
        public DateTime CreatedDate { get; set; }
        public long PropertyId { get; set; }
    }
}
using System;

namespace OB.DL.Common.Criteria
{
    public class ValidateReservationCriteria
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long? Channel_UID { get; set; }
        public long Property_UID { get; set; }
        public long? Rate_UID { get; set; }
        public long? RoomType_UID { get; set; }
        public int NumAdults { get; set; }
        public int NumChilds { get; set; }
        public string ChildAges { get; set; }
        public string PCC { get; set; }
        public string CompanyCode { get; set; }
        public string Currency { get; set; }
        public long? BoardType_UID { get; set; }
        public int NumberOfRooms { get; set; }
        public decimal? HigherDayPrice { get; set; }
        public long? PaymentMethodType { get; set; }
        public long? GroupCode { get; set; }
        public long? PromoCode { get; set; }
        public bool ValidateAllotment { get; set; }
        public long? Tpi_UID { get; set; }
    }
}

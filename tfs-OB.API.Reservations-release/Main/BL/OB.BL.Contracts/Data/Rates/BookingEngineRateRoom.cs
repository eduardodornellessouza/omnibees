using System;

namespace OB.BL.Contracts.Data.Rates
{
    public partial class BookingEngineRateRoom
    {
        public BookingEngineRateRoom()
        {
        }

        public long RateRoom_UID { get; set; }

        public long RateRoomDetail_UID { get; set; }

        public string RateName { get; set; }

        public string RoomName { get; set; }

        public string RoomNo { get; set; }

        public int TempAllotment { get; set; }

        public int TempAllotmentUsed { get; set; }

        public long Property_UID { get; set; }

        public DateTime Date { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsValid { get; set; }
    }
}
using System;

namespace OB.DL.Common.QueryResultObjects
{
    public class PEOccupancyAlertQR1
    {
        public long UID { get; set; }

        public DateTime Date { get; set; }

        public long RoomType_UID { get; set; }

        public bool IsActivateCloseSalesInBE { get; set; }

        public long Property_UID { get; set; }

        //public long RateRoomDetails_UID { get; set; }

        public long Rate_UID { get; set; }

        //public bool isBookingEngineBlocked { get; set; }

        //public string BlockedChannelsListUID { get; set; }
    }
}
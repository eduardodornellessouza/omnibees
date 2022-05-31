using System;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ReservationRoomDetailRealInventoryDateRange
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public long? RoomType_UID { get; set; }
        public long? Rate_UID { get; set; }
    }

}

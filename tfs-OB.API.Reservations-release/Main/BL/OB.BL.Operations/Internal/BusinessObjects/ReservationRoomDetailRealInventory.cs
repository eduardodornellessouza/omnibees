using System;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ReservationRoomDetailRealInventory 
    {
        public DateTime Date { get; set; }
        public long? RoomType_UID { get; set; }
        public long? Rate_UID { get; set; }
    }
}

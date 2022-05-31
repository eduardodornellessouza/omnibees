using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class UpdateAllotmentAndInventoryDayParameters
    {
        public long RateRoomDetailUID { get; set; }
        public long RoomTypeUID { get; set; }
        public int RateAvailabilityType { get; set; }
        public DateTime Day { get; set; }
        public int AddQty { get; set; }
    }
}

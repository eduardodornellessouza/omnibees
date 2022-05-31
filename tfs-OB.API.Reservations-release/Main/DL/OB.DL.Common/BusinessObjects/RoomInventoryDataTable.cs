using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.BusinessObjects
{
    public class RoomInventoryDataTable
    {
        public long RoomTypeId { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public int Count { get; set; }
    }
}

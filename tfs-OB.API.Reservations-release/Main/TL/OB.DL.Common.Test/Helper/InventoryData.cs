using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using domainProperties = OB.Domain.Properties;

namespace OB.BL.Operations.Test.Helper
{
    [Serializable]
    public class InventoryInputData
    {
        public List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable> RoomsInventories { get; set; }

        public InventoryInputData() 
        {
            RoomsInventories = new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>();
        }
    }

    [Serializable]
    public class InventoryExpectedData
    {
        public List<domainProperties.Inventory> Inventories { get; set; }

        public InventoryExpectedData() 
        {
            Inventories = new List<domainProperties.Inventory>();
        }
    }
}

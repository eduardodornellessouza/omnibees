using OB.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using OB.Api.Core;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.BL.Operations.Test.Helper
{
    public class InventoryBuilder
    {
        public static Inventory GetInventory(long roomTypeUid, DateTime date, IUnitOfWork unitOfWork, IOBPropertyRepository _context)
        {
            var inventory = _context.ListInventory(
                new Contracts.Requests.ListInventoryRequest() {
                    roomTypeIdsAndDateRange = new List<InventorySearch>()
                    {
                        new InventorySearch()
                        {
                            RoomType_UID = roomTypeUid,
                            DateFrom = date
                        }
                    }
                }
            ).FirstOrDefault();

            if (inventory == null)
            {
                return InventoryBuilder.CreateInventory(roomTypeUid, date, unitOfWork, _context);
            }

            return inventory;
        }

        public static Inventory CreateInventory(long roomTypeUid, DateTime date, IUnitOfWork unitOfWork, IOBPropertyRepository _context)
        {
            var inventory = _context.ListInventory(
                new Contracts.Requests.ListInventoryRequest()
                {
                    roomTypeIdsAndDateRange = new List<InventorySearch>()
                    {
                        new InventorySearch()
                        {
                            RoomType_UID = roomTypeUid,
                            DateFrom = date
                        }
                    }
                }
            ).FirstOrDefault();

            if (inventory == null)
            {
                inventory = new Inventory {
                    Date = date.Date,
                    QtyRoomOccupied = 0,
                    RoomType_UID = roomTypeUid
                };

                //_context.Add(inventory);
                unitOfWork.Save();
            }

            return inventory;
        }

        public static Inventory UpdateInventory(Inventory inventory, int Qty, DateTime date, IUnitOfWork unitOfWork, IOBPropertyRepository _context)
        {
            inventory.QtyRoomOccupied = Qty;
            unitOfWork.Save();
            return inventory;
        }
    }
}

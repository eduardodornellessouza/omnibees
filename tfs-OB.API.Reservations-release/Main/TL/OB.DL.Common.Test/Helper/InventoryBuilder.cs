using Microsoft.Practices.Unity;
using OB.BL.Operations.Test.Helper;
using OB.DL.Common.Impl;
using OB.DL.Common.Interfaces;
using OB.Domain.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using domainProperties = OB.Domain.Properties;

namespace OB.DL.Common.Test.Helper
{
    class InventoryBuilder
    {
        public InventoryInputData InputData { get; set; }
        public InventoryExpectedData ExpectedData { get; set; }
        private ISessionFactory sessionFactory;
        private IRepositoryFactory repositoryFactory;

        public InventoryBuilder(IUnityContainer container)
        {
            InputData = new InventoryInputData();
            ExpectedData = new InventoryExpectedData();

            sessionFactory = container.Resolve<ISessionFactory>() as SessionFactory;
            repositoryFactory = container.Resolve<IRepositoryFactory>();
        }

        private Inventory InsertInventory(long roomTypeUid, DateTime date, int qty)
        {
            Inventory inventory = null;

            using (var unitOfWork = sessionFactory.GetUnitOfWork())
            {
                var inventoryRepo = repositoryFactory.GetInventoryRepository(unitOfWork);
                inventory = inventoryRepo.FirstOrDefault(q => q.Date == date.Date && q.RoomType_UID == roomTypeUid);

                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        Date = date.Date,
                        QtyRoomOccupied = qty,
                        RoomType_UID = roomTypeUid
                    };

                    inventoryRepo.Add(inventory);
                    unitOfWork.Save();
                }
            }

            return inventory;
        }

        private bool SetRoomTypeQty(long roomTypeUID, int desiredQty)
        {
            bool sucess = false;

            using (var unitOfWork = sessionFactory.GetUnitOfWork())
            {
                var roomTypeRepo = repositoryFactory.GetRoomTypesRepository(unitOfWork);
                var existingRoomType = roomTypeRepo.FirstOrDefault(x => x.UID == roomTypeUID);

                if (existingRoomType != null)
                {
                    if (existingRoomType.Qty == desiredQty)
                        return true;

                    existingRoomType.Qty = desiredQty;
                    sucess = true;

                    unitOfWork.Save();
                }
            }

            return sucess;
        }


        public InventoryBuilder WithNewAllocated()
        {
            InputData.RoomsInventories.AddRange(new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>
            {
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5593,
                    Type = 0,
                    Date = new DateTime(2050, 1, 1),
                    Count = 5
                },
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5593,
                    Type = 0,
                    Date = new DateTime(2050, 1, 2),
                    Count = 5
                }
            });

            // Set room type quantity
            if (!this.SetRoomTypeQty(5593, 50))
                return this;

            ExpectedData.Inventories.AddRange(new List<domainProperties.Inventory>() 
            { 
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5593,
                    Date = new DateTime(2050, 1, 1),
                    QtyRoomOccupied = 45
                },
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5593,
                    Date = new DateTime(2050, 1, 2),
                    QtyRoomOccupied = 45
                }
            });

            return this;
        }

        public InventoryBuilder WithExistingAllocated_DiferentQty()
        {
            // Update Data
            InputData.RoomsInventories.AddRange(new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>
            {
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5877,
                    Type = 0,
                    Date = new DateTime(2051, 2, 1),
                    Count = 10
                },
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5877,
                    Type = 0,
                    Date = new DateTime(2051, 2, 2),
                    Count = 10
                }
            });

            // Set room type quantity
            if (!this.SetRoomTypeQty(5877, 50))
                return this;

            // Inserts inventory
            this.InsertInventory(5877, new DateTime(2051, 2, 1), 15);
            this.InsertInventory(5877, new DateTime(2051, 2, 2), 15);

            // Expected Data after update
            ExpectedData.Inventories.AddRange(new List<domainProperties.Inventory>() 
            { 
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5877,
                    Date = new DateTime(2051, 2, 1),
                    QtyRoomOccupied = 40
                },
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5877,
                    Date = new DateTime(2051, 2, 2),
                    QtyRoomOccupied = 40
                }
            });

            return this;
        }

        public InventoryBuilder WithNewUsed()
        {
            InputData.RoomsInventories.AddRange(new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>
            {
                 new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5593,
                    Type = 1,
                    Date = new DateTime(2050, 2, 1),
                    Count = 20
                },
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5593,
                    Type = 1,
                    Date = new DateTime(2050, 2, 2),
                    Count = 20
                }
            });


            ExpectedData.Inventories.AddRange(new List<domainProperties.Inventory>() 
            { 
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5593,
                    Date = new DateTime(2050, 2, 1),
                    QtyRoomOccupied = 20
                },
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5593,
                    Date = new DateTime(2050, 2, 2),
                    QtyRoomOccupied = 20
                }
            });

            return this;
        }

        public InventoryBuilder WithExistingUsed_DiferentQty()
        {
            // Inserts inventory
            this.InsertInventory(5148, new DateTime(2051, 2, 1), 15);
            this.InsertInventory(5148, new DateTime(2051, 2, 2), 15);

            // Update Data
            InputData.RoomsInventories.AddRange(new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>
            {
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5148,
                    Type = 1,
                    Date = new DateTime(2051, 2, 1),
                    Count = 10
                },
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5148,
                    Type = 1,
                    Date = new DateTime(2051, 2, 2),
                    Count = 10
                }
            });

            // Expected Data after update
            ExpectedData.Inventories.AddRange(new List<domainProperties.Inventory>() 
            { 
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5148,
                    Date = new DateTime(2051, 2, 1),
                    QtyRoomOccupied = 10
                },
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5148,
                    Date = new DateTime(2051, 2, 2),
                    QtyRoomOccupied = 10
                }
            });

            return this;
        }

        public InventoryBuilder WithNewAllocatedAdjustment()
        {
            InputData.RoomsInventories.AddRange(new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>
            {
                 new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5822,
                    Type = 2,
                    Date = new DateTime(2050, 3, 1),
                    Count = 30
                },
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5822,
                    Type = 2,
                    Date = new DateTime(2050, 3, 2),
                    Count = 30
                }
            });


            ExpectedData.Inventories.AddRange(new List<domainProperties.Inventory>() 
            { 
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5822,
                    Date = new DateTime(2050, 3, 1),
                    QtyRoomOccupied = 30
                },
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5822,
                    Date = new DateTime(2050, 3, 2),
                    QtyRoomOccupied = 30
                }
            });

            return this;
        }

        public InventoryBuilder WithExistingAllocatedAdjustment_DiferentQty()
        {
            // Inserts inventory
            this.InsertInventory(5878, new DateTime(2051, 3, 1), 15);
            this.InsertInventory(5878, new DateTime(2051, 3, 2), 15);

            // Update Data
            InputData.RoomsInventories.AddRange(new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>
            {
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5878,
                    Type = 2,
                    Date = new DateTime(2051, 3, 1),
                    Count = 10
                },
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5878,
                    Type = 2,
                    Date = new DateTime(2051, 3, 2),
                    Count = 10
                }
            });

            // Expected Data after update
            ExpectedData.Inventories.AddRange(new List<domainProperties.Inventory>() 
            { 
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5878,
                    Date = new DateTime(2051, 3, 1),
                    QtyRoomOccupied = 25
                },
                new domainProperties.Inventory()
                {
                    RoomType_UID = 5878,
                    Date = new DateTime(2051, 3, 2),
                    QtyRoomOccupied = 25
                }
            });

            return this;
        }

        public InventoryBuilder WithExistingAnyType_EqualQty()
        {
            // Inserts inventory
            this.InsertInventory(5837, new DateTime(2052, 2, 1), 30);
            this.InsertInventory(5837, new DateTime(2052, 2, 2), 30);

            // Update Data
            InputData.RoomsInventories.AddRange(new List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>
            {
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5837,
                    Type = 1,
                    Date = new DateTime(2052, 2, 1),
                    Count = 30
                },
                new OB.DL.Common.BusinessObjects.RoomInventoryDataTable()
                {
                    RoomTypeId = 5837,
                    Type = 1,
                    Date = new DateTime(2052, 2, 2),
                    Count = 30
                }
            });

            // No expected data because quantity is equal

            return this;
        }
    }
}

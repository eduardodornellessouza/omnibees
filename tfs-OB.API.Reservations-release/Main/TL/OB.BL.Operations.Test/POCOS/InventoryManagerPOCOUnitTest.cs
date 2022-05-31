using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Operations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using domainProperties = OB.Domain.Properties;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using Microsoft.Practices.Unity;
using OB.DL.Common.Interfaces;
using OB.BL.Operations.Test.Helper;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class InventoryManagerPOCOUnitTest : UnitBaseTest
    {
        private IInventoryManagerPOCO _inventoryManagerPOCO = null;

        private Mock<IUnitOfWork> _unitOfWorkMock = null;
        private Mock<ISessionFactory> _sessionFactoryMock = null;
        private Mock<IRepositoryFactory> _repoFactoryMock = null;
        private Mock<IOccupancyLevelsManagerPOCO> _occupancyLevelsManagerPOCOMock = null;
        private Mock<IRepository<OB.Domain.General.User>> _userRepoMock = null;
        private Mock<IRepository<OB.Domain.General.ChangeLogDetail>> _changeLogDetailRepoMock = null;
        private Mock<IInventoryRepository> _inventoryRepoMock = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _sessionFactoryMock = new Mock<ISessionFactory>();
            _repoFactoryMock = new Mock<IRepositoryFactory>();
            _occupancyLevelsManagerPOCOMock = new Mock<IOccupancyLevelsManagerPOCO>(MockBehavior.Default);
            _userRepoMock = new Mock<IRepository<OB.Domain.General.User>>();
            _changeLogDetailRepoMock = new Mock<IRepository<Domain.General.ChangeLogDetail>>();
            _inventoryRepoMock = new Mock<IInventoryRepository>();

            _sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(_unitOfWorkMock.Object);
            _repoFactoryMock.Setup(x => x.GetInventoryRepository(It.IsAny<IUnitOfWork>())).Returns(_inventoryRepoMock.Object);
             
            this.Container = this.Container.RegisterInstance<ISessionFactory>(_sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(_repoFactoryMock.Object);
            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            _inventoryManagerPOCO = this.Container.Resolve<IInventoryManagerPOCO>();
        }

        #region UpdateInventory
        /// <summary>
        /// Mocks the update occupancy levels and log on custom update inventory method.
        /// </summary>
        private void MockUpdateOccLevelsAndLogOnCustomUpdateInventory()
        {
            _inventoryRepoMock.Setup(x => x.UpdateInventory(It.IsAny<List<OB.DL.Common.BusinessObjects.RoomInventoryDataTable>>())).Returns(new List<domainProperties.Inventory>());
            _occupancyLevelsManagerPOCOMock.Setup(x => x.ApplyOccupancyLevelsFromInventory(It.IsAny<long>(), It.IsAny<List<domainProperties.Inventory>>(), It.IsAny<string>())).Returns(true);
            _userRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns<OB.Domain.General.User>(null);
            _changeLogDetailRepoMock.Setup(x => x.Add(It.IsAny<OB.Domain.General.ChangeLogDetail>())).Returns<OB.Domain.General.ChangeLogDetail>(null);
            _unitOfWorkMock.Setup(x => x.Save(It.IsAny<int?>())).Returns(0);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        public void TestRoomsInventoryDemuxDays_WithNoInterceptions()
        {
            // Arrange
            var inputData = new List<contractsProperties.RoomInventory>()
            {
                new contractsProperties.RoomInventory()
                {
                    RoomTypeUID = 5837,
                    Type = Constants.InventoryUpdateType.Allocated,
                    StartDate = new DateTime(2050, 6, 6),
                    EndDate = new DateTime(2050, 6, 7),
                    Count = 10
                },
                new contractsProperties.RoomInventory()
                {
                    RoomTypeUID = 5862,
                    Type = Constants.InventoryUpdateType.Used,
                    StartDate = new DateTime(2050, 6, 6),
                    EndDate = new DateTime(2050, 6, 7),
                    Count = 20
                },
            };

            // Act
            var demuxedRoomsInventories = OB.BL.Operations.Internal.TypeConverters.OtherConverter.ConvertToRoomInventoryDataTable(inputData);

            // Asserts
            Assert.IsNotNull(demuxedRoomsInventories, "demuxedRoomsInventories must be different of null");
            Assert.AreEqual(4, demuxedRoomsInventories.Count, "demuxedRoomsInventories count must be 4");
            
            Assert.AreEqual(1, demuxedRoomsInventories.Count(x => 
                x.RoomTypeId == 5837 && 
                x.Type == (int)Constants.InventoryUpdateType.Allocated && 
                x.Date.CompareTo(new DateTime(2050, 6, 6)) == 0 &&
                x.Count == 10));

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5837 &&
                x.Type == (int)Constants.InventoryUpdateType.Allocated &&
                x.Date.CompareTo(new DateTime(2050, 6, 7)) == 0 &&
                x.Count == 10));

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5862 &&
                x.Type == (int)Constants.InventoryUpdateType.Used &&
                x.Date.CompareTo(new DateTime(2050, 6, 6)) == 0 &&
                x.Count == 20));

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5862 &&
                x.Type == (int)Constants.InventoryUpdateType.Used &&
                x.Date.CompareTo(new DateTime(2050, 6, 7)) == 0 &&
                x.Count == 20));
        }

        [TestMethod]
        [TestCategory("Inventory")]
        public void TestRoomsInventoryDemuxDays_WithInterceptions()
        {
            // Arrange
            var inputData = new List<contractsProperties.RoomInventory>()
            {
                new contractsProperties.RoomInventory()
                {
                    RoomTypeUID = 5837,
                    Type = Constants.InventoryUpdateType.Allocated,
                    StartDate = new DateTime(2050, 6, 6),
                    EndDate = new DateTime(2050, 6, 7),
                    Count = 10
                },

                // intercept one day with same count
                new contractsProperties.RoomInventory()
                {
                    RoomTypeUID = 5837,
                    Type = Constants.InventoryUpdateType.Allocated,
                    StartDate = new DateTime(2050, 6, 7),
                    EndDate = new DateTime(2050, 6, 8),
                    Count = 10
                },

                // intercept day with diferent count
                new contractsProperties.RoomInventory()
                {
                    RoomTypeUID = 5837,
                    Type = Constants.InventoryUpdateType.Allocated,
                    StartDate = new DateTime(2050, 6, 8),
                    EndDate = new DateTime(2050, 6, 8),
                    Count = 11
                },
            };

            // Act
            var demuxedRoomsInventories = OB.BL.Operations.Internal.TypeConverters.OtherConverter.ConvertToRoomInventoryDataTable(inputData);

            // Asserts
            Assert.IsNotNull(demuxedRoomsInventories, "demuxedRoomsInventories must be different of null");
            Assert.AreEqual(4, demuxedRoomsInventories.Count, "demuxedRoomsInventories count must be 4");

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5837 &&
                x.Type == (int)Constants.InventoryUpdateType.Allocated &&
                x.Date.CompareTo(new DateTime(2050, 6, 6)) == 0 &&
                x.Count == 10));

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5837 &&
                x.Type == (int)Constants.InventoryUpdateType.Allocated &&
                x.Date.CompareTo(new DateTime(2050, 6, 7)) == 0 &&
                x.Count == 10));

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5837 &&
                x.Type == (int)Constants.InventoryUpdateType.Allocated &&
                x.Date.CompareTo(new DateTime(2050, 6, 8)) == 0 &&
                x.Count == 10));

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5837 &&
                x.Type == (int)Constants.InventoryUpdateType.Allocated &&
                x.Date.CompareTo(new DateTime(2050, 6, 8)) == 0 &&
                x.Count == 11));
        }

        [TestMethod]
        [TestCategory("Inventory")]
        public void TestRoomsInventoryDemuxDays_WithRepeatedInventories()
        {
            // Arrange
            var inputData = new List<contractsProperties.RoomInventory>()
            {
                new contractsProperties.RoomInventory()
                {
                    RoomTypeUID = 5837,
                    Type = Constants.InventoryUpdateType.Allocated,
                    StartDate = new DateTime(2050, 6, 7),
                    EndDate = new DateTime(2050, 6, 8),
                    Count = 15
                },
                new contractsProperties.RoomInventory()
                {
                    RoomTypeUID = 5837,
                    Type = Constants.InventoryUpdateType.Allocated,
                    StartDate = new DateTime(2050, 6, 7),
                    EndDate = new DateTime(2050, 6, 8),
                    Count = 15
                }
            };

            // Act
            var demuxedRoomsInventories = OB.BL.Operations.Internal.TypeConverters.OtherConverter.ConvertToRoomInventoryDataTable(inputData);

            // Asserts
            Assert.IsNotNull(demuxedRoomsInventories, "demuxedRoomsInventories must be different of null");
            Assert.AreEqual(2, demuxedRoomsInventories.Count, "demuxedRoomsInventories count must be 2");

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5837 &&
                x.Type == (int)Constants.InventoryUpdateType.Allocated &&
                x.Date.CompareTo(new DateTime(2050, 6, 7)) == 0 &&
                x.Count == 15));

            Assert.AreEqual(1, demuxedRoomsInventories.Count(x =>
                x.RoomTypeId == 5837 &&
                x.Type == (int)Constants.InventoryUpdateType.Allocated &&
                x.Date.CompareTo(new DateTime(2050, 6, 8)) == 0 &&
                x.Count == 15));
        }

        #endregion
    }
}

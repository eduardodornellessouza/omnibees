using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using OB.DL.Common;
using OB.DL.Model.Reservations;
using OB.BL.Operations.Interfaces;
using System.Transactions;
using OB.BL.Operations.Test.Helper;
using System.Threading;
using System.Threading.Tasks;
using OB.DL.Common.Interfaces;
using OB.Domain.Reservations;
using System.Linq;
using System.Collections.Generic;
using OB.Domain.Channels;
using OB.Domain;
using System.IO;
using System.Reflection;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using OB.BL.Operations.Internal.TypeConverters;
using System.Data.Entity.Infrastructure;
using contractsCRM = OB.BL.Contracts.Data.CRM;
using contractsReservations = OB.BL.Contracts.Data.Reservations;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Helper;
using OB.Domain.Rates;
using OB.Domain.CRM;
using OB.Domain.Properties;
using OB.Domain.ProactiveActions;
using System.Runtime.ExceptionServices;
using OB.Domain.Payments;
using Moq;
using OB.DL.Common.QueryResultObjects;
using OB.BL.Contracts.Data;
using PaymentGatewaysLibrary;
using OB.BL.Contracts.Requests;
using System.Security.Policy;
using OB.BL.Operations.Internal.BusinessObjects;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using OB.Domain.General;
using OB.BL.Contracts.Data.Properties;
using domainProperty = OB.Domain.Properties;

namespace OB.BL.Operations.Test
{
    [Serializable]
    [TestClass]
    public class InventoryManagerPOCOTest : IntegrationBaseTest
    {
        private IInventoryManagerPOCO _inventoryManagerPOCO;
        public IInventoryManagerPOCO InventoryManagerPOCO
        {
            get
            {
                if (_inventoryManagerPOCO == null)
                    _inventoryManagerPOCO = this.Container.Resolve<IInventoryManagerPOCO>();

                return _inventoryManagerPOCO;
            }
            set { _inventoryManagerPOCO = value; }
        }


        [TestInitialize]
        public override void Initialize()
        {
            _inventoryManagerPOCO = null;

            base.Initialize();
        }

        public long ApplyRealInventoryToAllChannels(List<ReservationRoomDetailRealInventory> reservationRoomDetailsRealInventory, long PropertyUID, long fromChannel)
        {
            long result = -1;
            var resetEvet = new ManualResetEvent(false);
            Exception ex = null;
            var task = new Thread(new ThreadStart(() =>
            {
                try
                {
                    // call the method                    
                    InventoryManagerPOCO = Container.Resolve<IInventoryManagerPOCO>();

                    result = InventoryManagerPOCO.ApplyRealInventoryToAllChannels(reservationRoomDetailsRealInventory, PropertyUID, fromChannel);

                    InventoryManagerPOCO.WaitForAllBackgroundWorkers();
                }
                catch (Exception e)
                {
                    ex = e;
                }
                finally
                {
                    resetEvet.Set();
                }

            }));
            task.Start();
            resetEvet.WaitOne();

            if (ex != null)
                ExceptionDispatchInfo.Capture(ex).Throw();

            return result;
        }


        public long ApplyRealAllotmentToAllChannels(List<ReservationRoomDetailRealAllotment> reservationRoomDetailsRealAllotment, long PropertyUID, long fromChannel)
        {
            long result = -1;
            var resetEvet = new ManualResetEvent(false);
            Exception ex = null;
            var task = new Thread(new ThreadStart(() =>
            {
                try
                {
                    // call the method                    
                    InventoryManagerPOCO = Container.Resolve<IInventoryManagerPOCO>();

                    result = InventoryManagerPOCO.ApplyRealAllotmentToAllChannels(reservationRoomDetailsRealAllotment, PropertyUID, fromChannel);

                    InventoryManagerPOCO.WaitForAllBackgroundWorkers();
                }
                catch (Exception e)
                {
                    ex = e;
                }
                finally
                {
                    resetEvet.Set();
                }

            }));
            task.Start();
            resetEvet.WaitOne();

            if (ex != null)
                ExceptionDispatchInfo.Capture(ex).Throw();

            return result;
        }

        public List<long> CustomUpdateInventory(long propertyUID, List<contractsProperties.RoomInventory> inventoriesToUpdate, string senderIdentifier, bool generatedByBOUser)
        {
            List<long> result = null;
            var resetEvet = new ManualResetEvent(false);
            Exception ex = null;
            var task = new Thread(new ThreadStart(() =>
            {
                try
                {
                    // call the method                    
                    InventoryManagerPOCO = Container.Resolve<IInventoryManagerPOCO>();

                    result = InventoryManagerPOCO.CustomUpdateInventory(propertyUID, inventoriesToUpdate, senderIdentifier, generatedByBOUser);

                    InventoryManagerPOCO.WaitForAllBackgroundWorkers();
                }
                catch (Exception e)
                {
                    ex = e;
                }
                finally
                {
                    resetEvet.Set();
                }

            }));
            task.Start();
            resetEvet.WaitOne();

            if (ex != null)
                ExceptionDispatchInfo.Capture(ex).Throw();

            return result;
        }


        #region Update Inventory
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestCustomUpdateInventory_Used")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestCustomUpdateInventory_Used")]
        [DeploymentItem("./DL")]
        public void TestCustomUpdateInventory_Used()
        {
            #region arrange
            List<RoomInventory> inventories = new List<RoomInventory>();
            domainProperty.Inventory day1 = new domainProperty.Inventory();
            domainProperty.Inventory day2 = new domainProperty.Inventory();
            ChangeLogDetail resultChangeLog = new ChangeLogDetail();
            PropertyQueue pq = new PropertyQueue();
            List<RateRoomDetail> changedDetails = new List<RateRoomDetail>();

            // TODO trace
            long before = DateTime.Now.Ticks;
            long startBytes = System.GC.GetTotalMemory(true);
            #endregion

            //act
            inventories.Add(new RoomInventory
            {
                RoomTypeUID = 5148,
                Count = 9,
                StartDate = new DateTime(2050, 12, 30, 10, 23, 34),
                EndDate = new DateTime(2050, 12, 31, 10, 23, 34),
                Type = OB.BL.Constants.InventoryUpdateType.Used
            });

            //act Update Inventory
            var result = this.CustomUpdateInventory(1635, inventories, "Admin", true);
            Debug.WriteLine("total size: " + ((long)(System.GC.GetTotalMemory(true)) - startBytes) / 1024 + " KB");
            Debug.WriteLine("total time: " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // act inventory
                day1 = InventoryBuilder.GetInventory(5148, new DateTime(2050, 12, 30), unitOfWork,
                  this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                day2 = InventoryBuilder.GetInventory(5148, new DateTime(2050, 12, 31), unitOfWork,
                  this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                //act log
                var changeLogRepo = this.RepositoryFactory.GetRepository<ChangeLogDetail>(unitOfWork);
                resultChangeLog = changeLogRepo.GetQuery(c => c.TypeIdentifierId == 1635).OrderByDescending(c => c.UID).FirstOrDefault();

                // act proactive warning
                var propertyQueueRepo = this.RepositoryFactory.GetPropertyQueueRepository(unitOfWork);
                pq = propertyQueueRepo.GetAll().OrderByDescending(o => o.UID).FirstOrDefault();


                // act proactive action close days
                var yesterday = DateTime.Now.AddDays(-1).Date;
                Thread.Sleep(5000);
                var rateRoomDetailsRepo = this.RepositoryFactory.GetRateRoomDetailRepository(unitOfWork);
                changedDetails = rateRoomDetailsRepo.GetQuery(rrd => rrd.ModifyDate >= yesterday).ToList();
            }

            // assert result
            Assert.IsNotNull(result);

            // assert inventory
            Assert.AreEqual(9, day1.QtyRoomOccupied);
            Assert.AreEqual(9, day2.QtyRoomOccupied);

            // assert log
            Assert.IsNull(resultChangeLog.CorrelationID);
            Assert.AreEqual("Inventory update generated by user. Availability Close Sales = True, Availability Notify = True, Occupancy Levels = False", resultChangeLog.Description);
            Assert.IsNull(resultChangeLog.GeneratedBy);
            Assert.AreEqual(DateTime.Now.Date, resultChangeLog.ModifiedDate.Date);
            Assert.AreEqual(65, resultChangeLog.ModifiedUserId);
            Assert.AreEqual(false, resultChangeLog.IsCreated);
            Assert.AreEqual(22, resultChangeLog.TypeId);
            Assert.AreEqual(1635, resultChangeLog.TypeIdentifierId);
            Assert.AreEqual("{\r\n  \"PropertyUID\": 1635,\r\n  \"GeneratedBy\": \"user\",\r\n  \"Inventories\": [\r\n    {\r\n      \"RoomTypeUID\": 5148,\r\n      \"StartDate\": \"2050-12-30T00:00:00\",\r\n      \"EndDate\": \"2050-12-31T00:00:00\",\r\n      \"Type\": 1,\r\n      \"TypeName\": \"Used\",\r\n      \"Count\": 9\r\n    }\r\n  ],\r\n  \"OccupancyLevelsProcessed\": false,\r\n  \"AvailabilityNotifyRooms\": true,\r\n  \"AvailabilityCloseRooms\": true\r\n}"
            , resultChangeLog.JsonLog);

            // assert proactive warning
            Assert.AreEqual(DateTime.UtcNow.Date, pq.Date.Date);
            Assert.AreEqual(false, pq.IsProcessed);
            Assert.AreEqual(false, pq.IsProcessing);
            //Assert.AreEqual("[{\"Key\":5148,\"Value\":[635402880000000000,635403744000000000]}]", pq.MailBody);
            Assert.IsNull(pq.MailFrom);
            Assert.IsNull(pq.MailSubject);
            Assert.IsNull(pq.MailTo);
            Assert.AreEqual(4279, pq.PropertyEvent_UID);
            Assert.AreEqual(0, pq.Retry);

            // assert proactive action close days
            Assert.AreEqual(2, changedDetails.Count);
        }

        #endregion

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [ClassCleanup]
        public static void TestClassCleanup()
        {
            BaseTest.ClassCleanup();
        }

    }
}

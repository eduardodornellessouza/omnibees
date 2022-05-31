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


namespace OB.BL.Operations.Test
{


    [Serializable]
    [TestClass]
    public class OccupancyLevelsManagerPOCOTest : IntegrationBaseTest
    {

        private IOccupancyLevelsManagerPOCO _occupancyLevelsManagerPOCO;
        public IOccupancyLevelsManagerPOCO OccupancyLevelsManagerPOCO
        {
            get { 
                if(_occupancyLevelsManagerPOCO == null)
                    _occupancyLevelsManagerPOCO = this.Container.Resolve<IOccupancyLevelsManagerPOCO>();

                return _occupancyLevelsManagerPOCO; 
            }
            set { _occupancyLevelsManagerPOCO = value; }
        }

        private IRateRoomDetailsManagerPOCO _rateRoomDetailsManagerPOCO;

        public IRateRoomDetailsManagerPOCO RateRoomDetailsManagerPOCO
        {
            get
            {
                if (_rateRoomDetailsManagerPOCO == null)
                    _rateRoomDetailsManagerPOCO = this.Container.Resolve<IRateRoomDetailsManagerPOCO>();

                return _rateRoomDetailsManagerPOCO;
            }   
        }

        [TestInitialize]
        public override void Initialize()
        {          
            _occupancyLevelsManagerPOCO = null;
            _rateRoomDetailsManagerPOCO = null;

            base.Initialize();
        }

           /// <summary>Method called after insert/change reservation to apply the occupancy level rules</summary>
        //public bool ApplyOccupancyLevelsFromReservation(long reservationUID, string operation)
        //{
        //    bool result = false;
        //    var resetEvet = new ManualResetEvent(false);
        //    Exception ex = null;
        //    var task = new Thread(new ThreadStart(() =>
        //    {
        //        try
        //        {
        //            // call the method                    
        //            OccupancyLevelsManagerPOCO = Container.Resolve<IOccupancyLevelsManagerPOCO>();

        //            result = OccupancyLevelsManagerPOCO.ApplyOccupancyLevelsFromReservation(reservationUID, operation);

        //            OccupancyLevelsManagerPOCO.WaitForAllBackgroundWorkers();
        //            RateRoomDetailsManagerPOCO.WaitForAllBackgroundWorkers();
        //        }
        //        catch (Exception e)
        //        {
        //            ex = e;
        //        }
        //        finally
        //        {
        //            resetEvet.Set();
        //        }

        //    }));
        //    task.Start();
        //    resetEvet.WaitOne();

        //    if (ex != null)
        //        ExceptionDispatchInfo.Capture(ex).Throw();

        //    return result;
        //}

        private bool ApplyOccupancyLevelsFromInventory(long propertyUID, List<Inventory> inventory, string operation)
        {        
            bool result = false;
            var resetEvet = new ManualResetEvent(false);
            Exception ex = null;
            var task = new Thread(new ThreadStart(() =>
            {
                try
                {
                    // call the method                    
                    OccupancyLevelsManagerPOCO = Container.Resolve<IOccupancyLevelsManagerPOCO>();

                    result = OccupancyLevelsManagerPOCO.ApplyOccupancyLevelsFromInventory(propertyUID, inventory, operation);

                    OccupancyLevelsManagerPOCO.WaitForAllBackgroundWorkers();
                    RateRoomDetailsManagerPOCO.WaitForAllBackgroundWorkers();
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



        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestOccupancyLevels_WithoutConfiguration")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestOccupancyLevels_WithoutConfiguration")]
        [DeploymentItem("./DL")]
        public void TestOccupancyLevels_WithoutConfiguration()
        {
            var inventoryList = new List<Inventory>();

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                #region Arrange

                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SaveChanges();

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, 
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork,
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                inventoryList.Add(inventory);

                #endregion

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));                
            }

            // Act
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");

            // Assert
            Assert.IsFalse(result);
        }

        #region From Inventory

        #region Hotel Occupancy

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithCloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithCloseSales")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithCloseSales()
        {
            var inventoryList = new List<Inventory>();

            //OB.Log.LogContext.AddGlobal("TestName", "TestHotelOccupancyLevelsFromInventory_WithCloseSales");
            using (var transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    #region Arrange
                    // Create the Builder
                    var builder = new OccupancyLevelsBuilder(1635, this.Container)
                        .ResetConfiguration()
                        .SetupHotelOccupancyModel()
                        .SaveChanges();

                    // Criar Periodo
                    var period = builder.CreatePeriod("Teste");
                    builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                    // Criar Nivel de Ocupação
                    var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100);

                    // Criar Regra do Nivel de Ocupaçao
                    builder.AddOccupancyLevelRule(occupancyLevel, true, null, false, false, 47971, 12347, 5148);

                    // Set Rate as Yielding
                    RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                    // Criar ou Obter Inventario
                    var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork,
                            this.RepositoryFactory.GetRepository<Inventory>(unitOfWork));
                    InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork,
                            this.RepositoryFactory.GetRepository<Inventory>(unitOfWork));

                    // Obter os Canais Abertos
                    RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                    RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                    var OpenChannels = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory).ChannelsListUID;
                    var OpenChannelsList = OpenChannels.Split(',').ToList();

                    inventoryList.Add(inventory);

                    unitOfWork.Save();
                    transactionScope.Complete();
                    var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                    inventoryList.ForEach(x => inventoryRepo.Detach(x));

                    #endregion
                }
              
            }

            // Act            
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");


            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {

                #region Asserts
                Assert.IsTrue(result);

                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                //Assert.AreEqual(OpenChannelsList, ClosedChannelsList);             
                Assert.IsTrue(afterRateRoomDetail.isBookingEngineBlocked ?? false);
                #endregion
            }

            //OB.Log.LogContext.RemoveGlobal("TestName");
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithCloseSales_JustBookingEngine")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithCloseSales_JustBookingEngine")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithCloseSales_JustBookingEngine()
        {
            var inventoryList = new List<Inventory>();

            using (var transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    #region Arrange
                    // Create the Builder
                    var builder = new OccupancyLevelsBuilder(1635, this.Container)
                        .ResetConfiguration()
                        .SetupHotelOccupancyModel()
                        .SaveChanges();

                    // Criar Periodo
                    var period = builder.CreatePeriod("Teste");
                    builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                    // Criar Nivel de Ocupação
                    var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100);

                    // Criar Regra do Nivel de Ocupaçao
                    builder.AddOccupancyLevelRule(occupancyLevel, true, null, false, false, 47971, 12347, 5148);

                    // Set Rate as Yielding
                    RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                    var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);

                    // Criar ou Obter Inventario
                    var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork,
                            inventoryRepo);
                    InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork,
                            inventoryRepo);

                    // Setup Rate Room Detail
                    RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                    RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);

                    inventoryList.Add(inventory);

                    unitOfWork.Save();
                    transactionScope.Complete();
                    #endregion
                }



            }

            // Act
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                #region Asserts

                Assert.IsTrue(result);
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                Assert.IsTrue(afterRateRoomDetail.isBookingEngineBlocked ?? false);
                #endregion
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_RateWithoutYield")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_RateWithoutYield")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_RateWithoutYield()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupHotelOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, true, null, false, false, 47971, 12347, 5148);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork,
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork,
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }

            // Call Service and Assert (EXPECTED: FALSE)
            Assert.IsFalse(this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test"));

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithRateWithYield")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithRateWithYield")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithRateWithYield()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupHotelOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, true, null, false, false, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork,
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork,
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }



            // Call Service and Assert (EXPECTED: TRUE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");


            Assert.IsTrue(result);


        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupHotelOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 100, true, true, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork,
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork,
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();

                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }
            // Call Service and Assert (EXPECTED: TRUE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");
            Assert.IsTrue(result);
        


            using (var localUnitOfWork = SessionFactory.GetUnitOfWork())
            {

                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, localUnitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);

                Assert.AreEqual(afterRateRoomDetail.Adult_1, 20);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 40);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithUpdateDownOneRate")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithUpdateDownOneRate")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithUpdateDownOneRate()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupHotelOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 50, true, false, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork,
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();

                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }


            // Call Service and Assert (EXPECTED: TRUE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");
            Assert.IsTrue(result);

            using (var localUnitOfWork = SessionFactory.GetUnitOfWork())
            {

                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, localUnitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);

                Assert.AreEqual(afterRateRoomDetail.Adult_1, 5);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 10);
            }
        }

        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithUpdateDownOneRate_WithInvalidRule")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithUpdateDownOneRate_WithInvalidRule")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithUpdateDownOneRate_WithInvalidRule()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupHotelOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 9);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 50, true, false, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, 
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, 
                    this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();
                
                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }

            // Call Service and Assert (EXPECTED: TRUE)
            Assert.IsFalse(this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test"));

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);

                Assert.AreEqual(afterRateRoomDetail.Adult_1, 10);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 20);
            }

        }

        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_WithInvalidRule")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_WithInvalidRule")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_WithInvalidRule()
        {
            var inventoryList = new List<Inventory>();             
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupHotelOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 9);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 50, true, true, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();
                
                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }

            // Call Service and Assert (EXPECTED: TRUE)
            Assert.IsFalse(this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test"));

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);

                Assert.AreEqual(afterRateRoomDetail.Adult_1, 10);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 20);
            }
        }

        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_TryApplyMultipleTimes")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_TryApplyMultipleTimes")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_TryApplyMultipleTimes()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupHotelOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 100, true, true, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();
               
                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
             }

            // act
            // Call Service and Assert (EXPECTED: TRUE)
            Assert.IsTrue(this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test"));

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);

                Assert.AreEqual(afterRateRoomDetail.Adult_1, 20);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 40);
                unitOfWork.Save();
            }

            // act
            // Call Service and Assert (EXPECTED: FALSE)
            Assert.IsFalse(this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test"));

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                var afterAfterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                Assert.AreEqual(afterAfterRateRoomDetail.Adult_1, 20);
                Assert.AreEqual(afterAfterRateRoomDetail.Adult_2, 40);
            }
        }

        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_ApplySecondRule")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_ApplySecondRule")]
        [DeploymentItem("./DL")]
        public void TestHotelOccupancyLevelsFromInventory_WithUpdateUpOneRate_ApplySecondRule()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupHotelOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 9);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 100, true, true, 47971, 12347, 5148);

                // Criar Nivel de Ocupação
                var secondOccupancyLevel = builder.CreateOccupancyLevel(period, 10, 100);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(secondOccupancyLevel, true, null, true, true, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();

                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }

            // Call Service and Assert (EXPECTED: TRUE)
            Assert.IsTrue(this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test"));

            if (this.SessionFactory.CurrentUnitOfWork != null && !this.SessionFactory.CurrentUnitOfWork.IsDisposed)
            {
                this.SessionFactory.CurrentUnitOfWork.Dispose();
            }

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();
            
                // CAN NOT CLOSE SALES
                Assert.IsTrue(afterRateRoomDetail.isBookingEngineBlocked ?? false);
            
                Assert.AreEqual(afterRateRoomDetail.Adult_1, 10);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 20);
            }
        }

        #endregion

        #region Room Occupancy

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithCloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithCloseSales")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithCloseSales()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                #region Arrange
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, true, null, false, false, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var OpenChannels = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory).ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();
                
                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
                #endregion
            }
            
            // Act
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");
                  
            if (this.SessionFactory.CurrentUnitOfWork != null && !this.SessionFactory.CurrentUnitOfWork.IsDisposed)
            {
                this.SessionFactory.CurrentUnitOfWork.Dispose();
            }
            
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                #region Asserts
                    Assert.IsTrue(result);

                    // ASSERT CLOSED CHANNELS
                    var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                    var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                    var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                    //Assert.AreEqual(OpenChannelsList, ClosedChannelsList);
                    Assert.IsTrue(afterRateRoomDetail.isBookingEngineBlocked ?? false);
                    #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithCloseSales_JustBookingEngine")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithCloseSales_JustBookingEngine")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithCloseSales_JustBookingEngine()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                #region Arrange
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, true, null, false, false, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Setup Rate Room Detail
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                
                inventoryList.Add(inventory);

                unitOfWork.Save();
                #endregion

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }

            // Act
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                #region Asserts
                Assert.IsTrue(result);
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                Assert.IsTrue(afterRateRoomDetail.isBookingEngineBlocked ?? false);
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_RateWithoutYield")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_RateWithoutYield")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_RateWithoutYield()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, true, null, false, false, 47971, 12347, 5148);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }

            // Call Service and Assert (EXPECTED: FALSE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");


            Assert.IsFalse(result);

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithRateWithYield")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithRateWithYield")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithRateWithYield()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, true, null, false, false, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                inventoryList.Add(inventory);

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));  
            }

            // Call Service and Assert (EXPECTED: TRUE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");


            Assert.IsTrue(result);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate()
        {
            var inventoryList = new List<Inventory>();

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 100, true, true, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();

                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));
            }

            // Call Service and Assert (EXPECTED: TRUE)
            Assert.IsTrue(this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test"));

            if (this.SessionFactory.CurrentUnitOfWork != null && !this.SessionFactory.CurrentUnitOfWork.IsDisposed)
                this.SessionFactory.CurrentUnitOfWork.Dispose();

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);

                Assert.AreEqual(afterRateRoomDetail.Adult_1, 20);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 40);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithUpdateDownOneRate")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithUpdateDownOneRate")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithUpdateDownOneRate()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 50, true, false, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();
                               
                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));
            }

            // Call Service and Assert (EXPECTED: TRUE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");
                       
            
            Assert.IsTrue(result);

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();
            
                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);
            
                Assert.AreEqual(afterRateRoomDetail.Adult_1, 5);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 10);
            }
        }

        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithUpdateDownOneRate_WithInvalidRule")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithUpdateDownOneRate_WithInvalidRule")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithUpdateDownOneRate_WithInvalidRule()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 10, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 50, true, false, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();
                
                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));
            }
            
            // Call Service and Assert (EXPECTED: TRUE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");

            Assert.IsFalse(result);

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);

                Assert.AreEqual(afterRateRoomDetail.Adult_1, 10);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 20);
            }
        }

        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_WithInvalidRule")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_WithInvalidRule")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_WithInvalidRule()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 10, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 50, true, true, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();

                inventoryList.Add(inventory);

                unitOfWork.Save();


                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));
            }

            // Call Service and Assert (EXPECTED: TRUE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");

            Assert.IsFalse(result);

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {            
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();
            
                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);
            
                Assert.AreEqual(afterRateRoomDetail.Adult_1, 10);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 20);
            }   
        }

        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_TryApplyMultipleTimes")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_TryApplyMultipleTimes")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_TryApplyMultipleTimes()
        {
            var inventoryList = new List<Inventory>();
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 100, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 100, true, true, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();

                inventoryList.Add(inventory);
                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));
            }


            // Call Service and Assert (EXPECTED: TRUE)
            Assert.IsTrue(this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test"));

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // ASSERT CLOSED CHANNELS
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();
            
                // CAN NOT CLOSE SALES
                Assert.IsFalse(afterRateRoomDetail.isBookingEngineBlocked ?? false);
            
                Assert.AreEqual(afterRateRoomDetail.Adult_1, 20);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 40);
            }

            // Call Service and Assert (EXPECTED: FALSE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");

            Assert.IsFalse(result);

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {                
                var afterAfterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);

                Assert.AreEqual(afterAfterRateRoomDetail.Adult_1, 20);
                Assert.AreEqual(afterAfterRateRoomDetail.Adult_2, 40);
            }
        }

        [TestMethod]
        [TestCategory("OccupancyLevelsService")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_ApplySecondRule")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_ApplySecondRule")]
        [DeploymentItem("./DL")]
        public void TestRoomOccupancyLevelsFromInventory_WithUpdateUpOneRate_ApplySecondRule()
        {
            var inventoryList = new List<Inventory>();

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // Create the Builder
                var builder = new OccupancyLevelsBuilder(1635, this.Container)
                    .ResetConfiguration()
                    .SetupRoomOccupancyModel()
                    .SaveChanges();

                // Criar Periodo
                var period = builder.CreatePeriod("Teste");
                builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

                // Criar Nivel de Ocupação
                var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 10, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(occupancyLevel, false, 100, true, true, 47971, 12347, 5148);

                // Criar Nivel de Ocupação
                var secondOccupancyLevel = builder.CreateOccupancyLevel(period, 11, 100, 5148);

                // Criar Regra do Nivel de Ocupaçao
                builder.AddOccupancyLevelRule(secondOccupancyLevel, true, null, true, true, 47971, 12347, 5148);

                // Set Rate as Yielding
                RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

                // Criar ou Obter Inventario
                var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
                InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

                // Obter os Canais Abertos
                RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
                RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
                var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
                var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
                var OpenChannelsList = OpenChannels.Split(',').ToList();

                inventoryList.Add(inventory);

                unitOfWork.Save();

                var inventoryRepo = this.RepositoryFactory.GetInventoryRepository(unitOfWork);
                inventoryList.ForEach(x => inventoryRepo.Detach(x));
            }

            // Call Service and Assert (EXPECTED: TRUE)
            var result = this.ApplyOccupancyLevelsFromInventory(1635, inventoryList, "Test");

            Assert.IsTrue(result);
            
            // ASSERT CLOSED CHANNELS                
            using (var localUnitOfWork = SessionFactory.GetUnitOfWork())
            {
                var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, localUnitOfWork, this.RepositoryFactory);
                var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
                var ClosedChannelsList = ClosedChannels.Split(',').ToList();

                // CAN NOT CLOSE SALES
                Assert.IsTrue(afterRateRoomDetail.isBookingEngineBlocked ?? false);

                Assert.AreEqual(afterRateRoomDetail.Adult_1, 10);
                Assert.AreEqual(afterRateRoomDetail.Adult_2, 20);
            }
        }

        #endregion

        #endregion

        #region From Reservation

        //[TestMethod]
        //[TestCategory("OccupancyLevelsService")]
        //[DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestHotelOccupancyLevelsFromReservation_WithUpdateUpOneRate_ApplySecondRule")]
        //[DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestHotelOccupancyLevelsFromReservation_WithUpdateUpOneRate_ApplySecondRule")]
        //[DeploymentItem("./DL")]
        //public void TestHotelOccupancyLevelsFromReservation_WithUpdateUpOneRate_ApplySecondRule()
        //{
        //    Reservation reservation = null;
        //    using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
        //    {
        //        // Create the Builder
        //        var builder = new OccupancyLevelsBuilder(1635, this.Container)
        //            .ResetConfiguration()
        //            .SetupHotelOccupancyModel()
        //            .SaveChanges();

        //        // Criar Periodo
        //        var period = builder.CreatePeriod("Teste");
        //        builder.AddPeriodDates(period, DateTime.Today, DateTime.Today);

        //        // Criar Nivel de Ocupação
        //        var occupancyLevel = builder.CreateOccupancyLevel(period, 0, 10);

        //        // Criar Regra do Nivel de Ocupaçao
        //        builder.AddOccupancyLevelRule(occupancyLevel, false, 100, true, true, 47971, 12347, 5148);

        //        // Criar Nivel de Ocupação
        //        var secondOccupancyLevel = builder.CreateOccupancyLevel(period, 11, 100);

        //        // Criar Regra do Nivel de Ocupaçao
        //        builder.AddOccupancyLevelRule(secondOccupancyLevel, true, null, true, true, 47971, 12347, 5148);

        //        // Set Rate as Yielding
        //        RateBuilder.SetRateYield(12347, true, unitOfWork, this.RepositoryFactory);

        //        // Criar ou Obter Inventario
        //        var inventory = InventoryBuilder.GetInventory(5148, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));
        //        InventoryBuilder.UpdateInventory(inventory, 5, DateTime.Today, unitOfWork, this.RepositoryFactory.GetInventoryRepository(unitOfWork));

        //        // Obter os Canais Abertos
        //        RateBuilder.ChangeRateRoomDetailRateRoom(5372944, 47971, unitOfWork, this.RepositoryFactory);
        //        RateBuilder.ChangeRateRoomDetailDate(5372944, DateTime.Today, unitOfWork, this.RepositoryFactory);
        //        var beforeRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, unitOfWork, this.RepositoryFactory);
        //        var OpenChannels = beforeRateRoomDetail.ChannelsListUID;
        //        var OpenChannelsList = OpenChannels.Split(',').ToList();


        //        var inventoryList = new List<Inventory>();
        //        inventoryList.Add(inventory);

        //        // Get Reservation and Modify Required Fields
        //        var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
        //        reservation = reservationRepo.FirstOrDefault(q => q.UID == 76516);
        //        var reservationRoom = reservation.ReservationRooms.FirstOrDefault();
        //        reservationRoom.DateFrom = DateTime.Today.Date;
        //        reservationRoom.DateTo = DateTime.Today.Date;
        //        reservationRoom.Rate_UID = 12347;
        //        var reservationRoomDetail = reservationRoom.ReservationRoomDetails.FirstOrDefault();
        //        reservationRoomDetail.Date = DateTime.Today.Date;
        //        reservationRoomDetail.RateRoomDetails_UID = 47971;

        //        unitOfWork.Save();


        //        reservationRepo.Detach(reservation);
        //    }

        //    // Call Service and Assert (EXPECTED: TRUE)
        //    var result = this.ApplyOccupancyLevelsFromReservation(reservation.UID, "Test");

        //    Assert.IsTrue(result);


        //    using (var localUnitOfWork = SessionFactory.GetUnitOfWork())
        //    {

        //        // ASSERT CLOSED CHANNELS
        //        var afterRateRoomDetail = RateBuilder.GetRateRoomDetail(5372944, localUnitOfWork, this.RepositoryFactory);
        //        var ClosedChannels = afterRateRoomDetail.BlockedChannelsListUID;
        //        var ClosedChannelsList = ClosedChannels.Split(',').ToList();

        //        // CANNOT CLOSE SALES
        //        Assert.IsTrue(afterRateRoomDetail.isBookingEngineBlocked ?? false);

        //        Assert.AreEqual(afterRateRoomDetail.Adult_1, 10);
        //        Assert.AreEqual(afterRateRoomDetail.Adult_2, 20);
        //    }

        //}

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

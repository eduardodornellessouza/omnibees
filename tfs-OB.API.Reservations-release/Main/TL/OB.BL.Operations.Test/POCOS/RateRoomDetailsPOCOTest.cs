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
using OB.DL.Common.QueryResultObjects;
using OB.BL.Contracts.Data;
using OB.BL.Contracts.Requests;
using System.Security.Policy;
using OB.BL.Contracts.Data.Rates;


namespace OB.BL.Operations.Test
{


    [Serializable]
    [TestClass]
    public class RateRoomDetailsPOCOTest : IntegrationBaseTest
    {

        private IRateRoomDetailsManagerPOCO _rateRoomDetailsManagerPOCO;
        public IRateRoomDetailsManagerPOCO RateRoomDetailsManagerPOCO
        {
            get { 
                if(_rateRoomDetailsManagerPOCO == null)
                    _rateRoomDetailsManagerPOCO = this.Container.Resolve<IRateRoomDetailsManagerPOCO>();

                return _rateRoomDetailsManagerPOCO; 
            }
            set { _rateRoomDetailsManagerPOCO = value; }
        }

        [TestInitialize]
        public override void Initialize()
        {          
            _rateRoomDetailsManagerPOCO = null;

            base.Initialize();
        }

        #region TESTS

        private const int NUMBER_OF_DAYS_IN_PERIOD_1 = 7;
        private const int NUMBER_OF_DAYS_IN_PERIOD_2 = 7;

        #region INSERT AND UPDATE PRICES

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesOnly")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesOnly()
        {
            // arrange
            var random = new Random();
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today);//.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            //builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var adultPrices = this.CreatePrices(3, 100,//random.Next(200, 200),
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,//random.Next(200, 200),
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                        adultPrices, childrenPrices);

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesDifferentByDay")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesDifferentByDay")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesDifferentByDay()
        {
            // arrange
            var random = new Random();
            

            // First Day
            var builder = CreateBuilder(100, new List<Period>() { new Period() { DateFrom = DateTime.Today, DateTo = DateTime.Today } });
            builder.CreateExpectedData();
            var result = UpdateRates(builder);
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Second Day
            builder = CreateBuilder(110, new List<Period>() { new Period() { DateFrom = DateTime.Today.AddDays(1), DateTo = DateTime.Today.AddDays(1) } });
            builder.CreateExpectedData();
            result = UpdateRates(builder);
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Third Day
            builder = CreateBuilder(110, new List<Period>() { new Period() { DateFrom = DateTime.Today.AddDays(3), DateTo = DateTime.Today.AddDays(4) } });
            builder.CreateExpectedData();
            result = UpdateRates(builder);
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var adultPrices = this.CreatePrices(3, 100,//random.Next(200, 200),
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,//random.Next(200, 200),
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                        adultPrices, childrenPrices);

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            // act
            result = UpdateRates(builder);

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        private UpdateRatesBuilder CreateBuilder(long price, List<Period> periods)
        {
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);

            foreach (var item in periods)
            {
                builder.AddPeriod(item.DateFrom, item.DateTo);
            }

            return builder;
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePrices1Adult2ChildsAndExtraBed")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePrices1Adult2ChildsAndExtraBed")]
        [DeploymentItem("./DL")]
        public void TestUpdatePrices1Adult2ChildsAndExtraBed()
        {
            // arrange
            var random = new Random();
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = builder.AddRate();
            builder = builder.AddRoom(1, 1, 2, 1, 3, true, true)
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), this.CreatePrices(2, 50), 10, 100, null);

            builder.AddRateRooms();
            builder.AddPeriod(DateTime.Today, DateTime.Today);//.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            //builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

        }

        #region TestCase 4978 - Insert Prices Different Weekdays

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays1")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays1()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays2")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays2()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays3")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays3()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays4")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays4()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays5")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays5()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays6")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays6()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays7")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays7()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays8")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays8()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays9")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays9()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWeekDays10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWeekDays10")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWeekDays10()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region Update Prices Different Weekdays

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays1")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays1()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =  false; 
            var terca =    false; 
            var quarta =   false; 
            var quinta =   false; 
            var sexta =    false;
            var sabado =   false; 

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays2")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays2()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays3")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays3()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays4")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays4()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays5")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays5()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays6")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays6()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays7")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays7()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays8")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays8()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays9")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays9()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesWeekDays10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesWeekDays10")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesWeekDays10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 300,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 10,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        [TestMethod] // TestCase 4988
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesSomeWeekDaysUpdatePricesAllDaysWithAllotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesSomeWeekDaysUpdatePricesAllDaysWithAllotment")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesSomeWeekDaysUpdatePricesAllDaysWithAllotment()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { true, false, true, false, false, true, false });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1);

            // Second Prices
            adultPrices = this.CreatePrices(20, random.Next(300, 300),
                                        10, false);

            childrenPrices = this.CreatePrices(20, random.Next(300, 300),
                                        10, false);

            // Second Room
            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 10, 50);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod] // TestCase 4989
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesSomeWeekDaysUpdatePricesAllDaysWithoutAllotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesSomeWeekDaysUpdatePricesAllDaysWithoutAllotment")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesSomeWeekDaysUpdatePricesAllDaysWithoutAllotment()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = CreateRoomStructure(builder, 10, new List<bool> { true, false, true, false, false, true, false });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(3, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(0),
                                        builder.Data.RoomAndParameters.ElementAt(0).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1);

            // Second Prices
            adultPrices = this.CreatePrices(20, 200,
                                        10, false);

            childrenPrices = this.CreatePrices(20, 500,
                                        10, false);

            // Second Room
            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 10);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #region TestUpdateWeekDaysDifferentPrices

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices1")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices1()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices2")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices2()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices3")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices3()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices4")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices4()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices5")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices5()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices6")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices6()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices7")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices7()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices8")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices8()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices9")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices9()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPrices10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPrices10")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPrices10()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            var childrenPrices = this.CreatePrices(0, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, random.Next(200, 300),
                                        10, random.NextDouble() > 0.5);

            childrenPrices = this.CreatePrices(0, random.Next(250, 300),
                                        10, random.NextDouble() > 0.5);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, -1, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region TestUpdateWeekDaysDifferentPricesVariation

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation1")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation2")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation3")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation4")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation5")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation6")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation7")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation8")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation9")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentPricesVariation10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentPricesVariation10")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentPricesVariation10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   true;
            var terca =     true;
            var quarta =    true;
            var quinta =    true;
            var sexta =     true;
            var sabado =    true;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPricesVariation = this.CreatePricesVariation(1, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1), adultPricesVariation, childrenPricesVariation,
                                        -1, null, new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariation")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariation")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariation()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            //builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 10,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 10,
                                        10, false);


            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation);

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesMinAdult3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesMinAdult3")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesMinAdult3()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = builder.AddRoom(3, 3, 0, 0, 1, false, true).AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1)).AddRateRooms();
            //builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(1, 10,
                                        10, false);


            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, null);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesMinAdult3MaxAdult15WithExtraBed")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesMinAdult3MaxAdult15WithExtraBed")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesMinAdult3MaxAdult15WithExtraBed()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = builder.AddRoom(15, 3, 0, 0, 13, true, false).AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(13, 100), null, 10, 100);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1)).AddRateRooms();
            //builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;


            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(13, 10,
                                        10, false);


            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, null);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesOneAdultVariation")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesOneAdultVariation")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesOneAdultVariation()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 10,
                                        10, false);

            foreach (var item in adultPricesVariation.Skip(1))
            {
                item.Value = 0;
            }

            var childrenPricesVariation = this.CreatePricesVariation(0, 10,
                                        10, false);


            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation);

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWithDifferentPricesPerDay")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWithDifferentPricesPerDay")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWithDifferentPricesPerDay()
        {
            // arrange
            

            // First Day
            var builder = CreateBuilder(100, new List<Period>() { new Period() { DateFrom = DateTime.Today, DateTo = DateTime.Today } });
            builder.CreateExpectedData();
            var result = UpdateRates(builder);
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Second Day
            builder = CreateBuilder(110, new List<Period>() { new Period() { DateFrom = DateTime.Today.AddDays(1), DateTo = DateTime.Today.AddDays(1) } });
            builder.CreateExpectedData();
            result = UpdateRates(builder);
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Third Day
            builder = CreateBuilder(120, new List<Period>() { new Period() { DateFrom = DateTime.Today.AddDays(2), DateTo = DateTime.Today.AddDays(2) } });
            builder.CreateExpectedData();
            result = UpdateRates(builder);
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 10,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 10,
                                        10, false);


            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation);

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            // act
            result = UpdateRates(builder);

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWithExtraBed")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWithExtraBed")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWithExtraBed()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            //builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation);

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation, -1,
                                        new PriceVariationValues { IsPercentage = false, IsValueDecreased = false, Value = 10 });

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #region TestUpdatePricesVariationWeekDays

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWeekDays1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWeekDays1")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWeekDays1()
        {
            // arrange
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation, -1, null,
                                                    new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWeekDays2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWeekDays2")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWeekDays2()
        {
            // arrange
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation, -1, null,
                                                    new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWeekDays3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWeekDays3")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWeekDays3()
        {
            // arrange
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation, -1, null,
                                                    new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWeekDays4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWeekDays4")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWeekDays4()
        {
            // arrange
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation, -1, null,
                                                    new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWeekDays5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWeekDays5")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWeekDays5()
        {
            // arrange
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation, -1, null,
                                                    new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWeekDays6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWeekDays6")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWeekDays6()
        {
            // arrange
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation, -1, null,
                                                    new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWeekDays7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWeekDays7")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWeekDays7()
        {
            // arrange
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation, -1, null,
                                                    new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdatePricesVariationWeekDays8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdatePricesVariationWeekDays8")]
        [DeploymentItem("./DL")]
        public void TestUpdatePricesVariationWeekDays8()
        {
            // arrange
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Update Prices Variation
            var adultPricesVariation = this.CreatePricesVariation(3, 0,
                                        10, false);

            var childrenPricesVariation = this.CreatePricesVariation(0, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.First(), builder.Data.RoomAndParameters.First().RoomPrices.First(),
                                                adultPricesVariation, childrenPricesVariation, -1, null,
                                                    new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Room
            adultPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            childrenPricesVariation = this.CreatePricesVariation(20, 0,
                                        10, false);

            builder.ChangeRoomPricesWithVariation(builder.Data.RoomAndParameters.ElementAt(1),
                                        builder.Data.RoomAndParameters.ElementAt(1).RoomPrices.ElementAt(0),
                                        adultPricesVariation, childrenPricesVariation);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        [TestMethod] // TestCase 4974
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWithoutAllotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWithoutAllotment")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWithoutAllotment()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, -1);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod] // TestCase 4978
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertPricesWithoutAllotmentSomeWeekdays")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertPricesWithoutAllotmentSomeWeekdays")]
        [DeploymentItem("./DL")]
        public void TestInsertPricesWithoutAllotmentSomeWeekdays()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, -1, new List<bool> { true, false, true, false, true, true, false });

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Prices")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateExtraBedPriceOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateExtraBedPriceOnly")]
        [DeploymentItem("./DL")]
        public void TestUpdateExtraBedPriceOnly()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today);//.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            //builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            builder.ChangeExtraBedPrice(builder.Data.RoomAndParameters.ElementAt(1), 50);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region ALLOTMENT

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentAllRatesParentAndDerived")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentAllRatesParentAndDerived")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentAllRatesParentAndDerived()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            builder.ChangeAllotment(builder.Data.RoomAndParameters.First(), 5);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.Data.SendAllRates = true;
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod] // TestCase 4979
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotment")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotment()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            builder.ChangeAllotment(builder.Data.RoomAndParameters.First(), 5);

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #region TestCase 4980 - TestUpdateAllotmentWeekdays

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays1")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays2")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays3")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays4")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays5")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays6")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays7")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays8")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays9")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateAllotmentWeekdays10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateAllotmentWeekdays10")]
        [DeploymentItem("./DL")]
        public void TestUpdateAllotmentWeekdays10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            // Update Allotment
            foreach (var item in builder.Data.RoomAndParameters)
            {
                builder.ChangeAllotment(item, 5);
                foreach (var roomPrice in item.RoomPrices)
                {
                    roomPrice.WeekDays = new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado };
                }
            }

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region TestUpdateWeekDaysDifferentAllotments

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments1")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments2")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments3")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments4")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments5")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments6")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments7")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments8")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments9")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Allotment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateWeekDaysDifferentAllotments10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateWeekDaysDifferentAllotments10")]
        [DeploymentItem("./DL")]
        public void TestUpdateWeekDaysDifferentAllotments10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;
            builder = builder.AddRate();
            builder.AddRoom()
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado })
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 150), null, 10, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado })
                .AddRateRooms();

            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            var before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            // Update Prices
            var random = new Random();

            // First Prices
            var adultPrices = this.CreatePrices(1, 100,
                                        10, false);

            var childrenPrices = this.CreatePrices(0, 200,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(0),
                                        adultPrices, childrenPrices, 5, 0,
                                        new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Second Prices
            adultPrices = this.CreatePrices(1, 500,
                                        10, false);

            childrenPrices = this.CreatePrices(0, 100,
                                        10, false);

            builder.ChangeRoomPrices(builder.Data.RoomAndParameters.First(),
                                        builder.Data.RoomAndParameters.First().RoomPrices.ElementAt(1),
                                        adultPrices, childrenPrices, 6, 0,
                                        new List<bool> { !domingo, !segunda, !terca, !quarta, !quinta, !sexta, !sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #endregion

        #region RESTRIÇÕES

        // TestCase 4983, 4983

        #region Close On Arrival

        #region TestInsertCloseOnArrivalOnly

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly1")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly2")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly3")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly4")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly5")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly6")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly7")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly8")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly9")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnArrivalOnly10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnArrivalOnly10")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnArrivalOnly10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region TestUpdateCloseOnArrivalOnly

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly1")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly2")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly3")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly4")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly5")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly6")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly7")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly8")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly9")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnArrivalOnly10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnArrivalOnly10")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnArrivalOnly10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnArrivalRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #endregion

        #region Close On Departure

        #region TestInsertCloseOnDepartureOnly

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly1")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly2")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly3")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly4")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly5")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly6")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly7")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly8")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly9")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseOnDepartureOnly10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseOnDepartureOnly10")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseOnDepartureOnly10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            //builder = CreateRapidTestStructure(builder);

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region TestUpdateCloseOnDepartureOnly

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly1")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly2")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly3")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly4")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly5")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly6")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly7")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly8")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly9")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseOnDepartureOnly10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseOnDepartureOnly10")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseOnDepartureOnly10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;

            builder.WithClosedOnDepartureRestriction(true, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #endregion

        #region MinDays

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertMinDaysOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertMinDaysOnly")]
        [DeploymentItem("./DL")]
        public void TestInsertMinDaysOnly()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            builder.WithMinDaysRestriction(2);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateMinDaysOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateMinDaysOnly")]
        [DeploymentItem("./DL")]
        public void TestUpdateMinDaysOnly()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithMinDaysRestriction(2);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region MaxDays

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertMaxDaysOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertMaxDaysOnly")]
        [DeploymentItem("./DL")]
        public void TestInsertMaxDaysOnly()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });


            builder.WithMaxDaysRestriction(2);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateMaxDaysOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateMaxDaysOnly")]
        [DeploymentItem("./DL")]
        public void TestUpdateMaxDaysOnly()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            //builder = CreateRapidTestStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today);//.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            // builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;
            builder.WithMaxDaysRestriction(2);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateMaxDaysOnlyAllRatesParentsAndDerived")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateMaxDaysOnlyAllRatesParentsAndDerived")]
        [DeploymentItem("./DL")]
        public void TestUpdateMaxDaysOnlyAllRatesParentsAndDerived()
        {
            // arrange


            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            //builder = CreateRapidTestStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today);//.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            // builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;
            builder.WithMaxDaysRestriction(2);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.Data.SendAllRates = true;
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region StayThrough

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertStayThroughOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertStayThroughOnly")]
        [DeploymentItem("./DL")]
        public void TestInsertStayThroughOnly()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            builder.WithStayThroughRestriction(2);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateStayThroughOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateStayThroughOnly")]
        [DeploymentItem("./DL")]
        public void TestUpdateStayThroughOnly()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;
            builder.WithStayThroughRestriction(2);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region Release Days

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertReleaseDaysOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertReleaseDaysOnly")]
        [DeploymentItem("./DL")]
        public void TestInsertReleaseDaysOnly()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            builder.WithReleaseDaysRestriction(2);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateReleaseDaysOnly")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateReleaseDaysOnly")]
        [DeploymentItem("./DL")]
        public void TestUpdateReleaseDaysOnly()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;
            builder.WithReleaseDaysRestriction(2);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region Multiple Restrictions

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertMultipleRestrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertMultipleRestrictions")]
        [DeploymentItem("./DL")]
        public void TestInsertMultipleRestrictions()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10, new List<bool> { true, true, true, true, true, true, true });

            builder.WithReleaseDaysRestriction(2);
            builder.WithMaxDaysRestriction(2);
            builder.WithMinDaysRestriction(2);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_Restrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateMultipleRestrictions")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateMultipleRestrictions")]
        [DeploymentItem("./DL")]
        public void TestUpdateMultipleRestrictions()
        {
            // arrange
            

            var builder = new UpdateRatesBuilder(this.Container);

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2));

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Insert Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "Insertion Failed Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;

            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;
            builder.WithReleaseDaysRestriction(2);
            builder.WithMaxDaysRestriction(2);
            builder.WithMinDaysRestriction(2);

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #endregion

        #region CLOSE SALES

        [TestMethod] // TestCase 4985
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSales")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSales()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
            .AddRateChannels(new List<long> { 2, 3, 45 })
                .WithClosedSales(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod] // TestCase 4985
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithoutRateChannels")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithoutRateChannels")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithoutRateChannels()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
            .AddRateChannels(new List<long> { 2, 3, 45 })
                .WithClosedSales(new List<long> { 2, 3, 45, 55, 60 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithBlockedChannelsWithNullValue")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithBlockedChannelsWithNullValue")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithBlockedChannelsWithNullValue()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
            .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            

            // Set blocked channel to null
            builder.SetRateRoomDetailsBlockedChannelFieldToNull();

            // Update only Close Sales
            builder.WithClosedSales(new List<long> { 2, 3, 45 });

            // New Expected Data
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSales")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSales()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesAllRatesParentAndDerived")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesAllRatesParentAndDerived")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesAllRatesParentAndDerived()
        {
            // arrange

            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.Data.SendAllRates = true;
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWithoutRateChannels")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWithoutRateChannels")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWithoutRateChannels()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45, 55, 60 });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #region TestCase 4986 - TestInsertCloseSalesWithWeekDays

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays1")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays2")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays3")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays4")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays5")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays6")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays7")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays8")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays9")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestInsertCloseSalesWithWeekDays10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestInsertCloseSalesWithWeekDays10")]
        [DeploymentItem("./DL")]
        public void TestInsertCloseSalesWithWeekDays10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var channels = new List<long> { 2, 3, 45 };
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 })
                    .WithClosedSales(channels, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region TestUpdateCloseSalesWeekdays

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays1")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays1")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays1()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo =   true;
            var segunda =   false;
            var terca =     false;
            var quarta =    false;
            var quinta =    false;
            var sexta =     false;
            var sabado =    false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays2")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays2")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays2()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = true;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays3")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays3")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays3()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = true;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays4")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays4")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays4()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays5")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays5")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays5()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = true;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays6")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays6")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays6()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = true;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays7")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays7")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays7()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays8")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays8")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays8()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = false;
            var terca = false;
            var quarta = true;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays9")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays9")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays9()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = false;
            var segunda = false;
            var terca = false;
            var quarta = false;
            var quinta = false;
            var sexta = false;
            var sabado = false;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateCloseSalesWeekdays10")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateCloseSalesWeekdays10")]
        [DeploymentItem("./DL")]
        public void TestUpdateCloseSalesWeekdays10()
        {
            // arrange
            var builder = new UpdateRatesBuilder(this.Container);
            var domingo = true;
            var segunda = true;
            var terca = true;
            var quarta = true;
            var quinta = true;
            var sexta = true;
            var sabado = true;

            builder = this.CreateRateStructure(builder);
            builder = this.CreateRoomStructure(builder, 10);
            builder.AddPeriod(DateTime.Today, DateTime.Today.AddDays(NUMBER_OF_DAYS_IN_PERIOD_1));
            builder.AddPeriod(DateTime.Today.AddDays(20), DateTime.Today.AddDays(20 + NUMBER_OF_DAYS_IN_PERIOD_2))
                .AddRateChannels(new List<long> { 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();

            // Update
            builder.Data.IsInsert = false;
            builder.Data.IsPriceChanged = false;
            //builder.Data.IsAllotmentChanged = false;

            builder.WithClosedSales(new List<long> { 2, 3, 45 }, new List<bool> { domingo, segunda, terca, quarta, quinta, sexta, sabado });

            // New Expected Data
            //builder.BeforeUpdateExpectedData = Clone(builder.ExpectedData);
            builder.ExpectedData.Clear();
            builder.CreateExpectedData();

            #region trace
            before = DateTime.Now.Ticks;
            #endregion

            // act
            result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Update Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        [TestMethod]
        [TestCategory("UpdateRatesTest_CloseSales")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestCloseSalesWithBookingEngine")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestCloseSalesWithBookingEngine")]
        [DeploymentItem("./DL")]
        public void TestCloseSalesWithBookingEngine()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = builder.AddRate();
            builder.AddRoom().AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10);
            builder.AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(5))
                .AddRateChannels(new List<long> { 1, 2, 3, 45 })
                .WithClosedSales(new List<long> { 1, 2, 3, 45 });

            // Create Expected Data
            builder.CreateExpectedData();

            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            this.UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            //var result = UpdateRates(builder);

            // assert
            //Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region Performance And Multithreading Tests

        [TestMethod]
        [TestCategory("UpdateRatesTest")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestMultipleProperties")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestMultipleProperties")]
        [DeploymentItem("./DL")]
        public void TestMultipleProperties()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = builder.AddRate();
            builder.AddRoom().AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10);
            builder.AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(5));

            var builder2 = new UpdateRatesBuilder(this.Container);
            builder2 = builder2.AddRate();
            builder2.AddRoom().AddRoomPrices(builder2.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10);
            builder2.AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today.AddDays(5));

            // Create Expected Data
            builder.CreateExpectedData();
            builder2.CreateExpectedData();

            var tasks = new List<Task<bool>>();

            // act
            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            tasks.Add(Task.Run(() => this.UpdateRates(builder)));
            tasks.Add(Task.Run(() => this.UpdateRates(builder2)));

            Task.WaitAll(tasks.ToArray());

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            //var result = UpdateRates(builder);

            // assert
            //Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
            builder2.AssertUpdate();
        }

        [TestMethod]
        [TestCategory("UpdateRatesTest")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestThreadPerformance")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestThreadPerformance")]
        [DeploymentItem("./DL")]
        [Ignore]
        public void TestThreadPerformance()
        {
            // arrange
            int numberOfRates = 1;
            int numberOfRooms = 1;
            int numberOfPeriods = 1;
            int periodTime = 180;
            int periodGap = 0;
            int numberOfDerived = 3;
            int numberOfLevels = 4;
            if (periodGap == 0) periodGap = 1;

            
            var builder = new UpdateRatesBuilder(this.Container);

            // Add Rates
            for (int i = 0; i < numberOfRates; i++)
            {
                builder.AddRate();
                CreateMultiLevelDerivedRates(ref builder, builder.Data.Rates.Last().UID, numberOfDerived, numberOfLevels);
            }

            // Add Rooms
            //var random = new Random();
            var adultMaxOccupancy = 3;//random.Next(1, 20);
            var adultMinOccupancy = 1;// random.Next(1, adultMaxOccupancy);
            var childrenMaxOccupancy = 1;// random.Next(1, 20);
            var childrenMinOccupancy = 1;// random.Next(1, childrenMaxOccupancy);
            var maxOccupancy = (adultMaxOccupancy - adultMinOccupancy) + (childrenMaxOccupancy - childrenMinOccupancy);

            var adultCount = (adultMaxOccupancy == adultMinOccupancy) ? 1 : (adultMaxOccupancy - adultMinOccupancy) + 1;
            var childrenCount = (childrenMaxOccupancy == childrenMinOccupancy) ? 1 : (childrenMaxOccupancy - childrenMinOccupancy) + 1;

            //var adultPrices = this.CreatePrices(adultCount, random.Next(100, 200),
            //                            random.Next(0, 10), random.NextDouble() > 0.5);

            //var childrenPrices = this.CreatePrices(childrenCount, random.Next(100, 200),
            //                            random.Next(0, 10), random.NextDouble() > 0.5);

            var adultPrices = this.CreatePrices(adultCount, 100,
                                        10, true);

            var childrenPrices = this.CreatePrices(childrenCount, 200,
                                        10, true);

            for (var i = 0; i < numberOfRooms; i++)
                builder.AddRoom(adultMaxOccupancy, adultMinOccupancy, childrenMaxOccupancy, childrenMinOccupancy, maxOccupancy)
                    .AddRoomPrices(builder.Data.RoomAndParameters.Last(), adultPrices, childrenPrices, 10);

            var periodLapse = 0;
            for (var i = 0; i < numberOfPeriods; i++)
            {
                builder.AddPeriod(DateTime.Today.AddDays(periodLapse), DateTime.Today.AddDays(periodTime));
                periodLapse += periodTime + periodGap;
            }

            builder.AddRateRooms();

            // Create Expected Data
            builder.CreateExpectedData();
            UpdateRates(builder);
            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            // act
            var result = UpdateRates(builder);

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            //Thread.Sleep(5000);

            // assert
            Assert.IsTrue(result, "UpdateRates Failed");
            //builder.AssertUpdate();
        }

        // Ira falhar sempre enquanto for Read uncommited
        [TestMethod]
        [TestCategory("UpdateRatesTest")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestSingleRateSingleRoomOneAdultOnePeriodTwice")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestSingleRateSingleRoomOneAdultOnePeriodTwice")]
        [DeploymentItem("./DL")]
        //[Ignore]
        public void TestSingleRateSingleRoomOneAdultOnePeriodTwice()
        {
            // arrange
            
            var builder = new UpdateRatesBuilder(this.Container);
            builder = builder.AddRate();
            builder.AddRoom().AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10);
            builder.AddRateRooms()
                .AddPeriod(DateTime.Today, DateTime.Today);//.AddDays(5));

            // Create Expected Data
            builder.CreateExpectedData();

            var tasks = new List<Task<bool>>();

            // act
            #region trace
            long before = DateTime.Now.Ticks;
            #endregion

            for (int i = 0; i < 5; i++)
            {
                Task<bool> ts = Task.Run(() => this.UpdateRates(builder));
                tasks.Add(ts);
            }

            Task.WaitAll(tasks.ToArray());

            #region trace
            Debug.WriteLine("Final Time - " + new TimeSpan(DateTime.Now.Ticks - before).TotalMilliseconds + " ms");
            #endregion

            //var result = UpdateRates(builder);

            // assert
            //Assert.IsTrue(result, "UpdateRates Failed");
            builder.AssertUpdate();
        }

        #endregion

        #region Helper

        /// <summary>
        /// Updates the rates.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private bool UpdateRates(UpdateRatesBuilder builder)
        {
            if (SessionFactory.CurrentUnitOfWork != null && !SessionFactory.CurrentUnitOfWork.IsDisposed)
                SessionFactory.CurrentUnitOfWork.Dispose();

            bool result = false;
            var resetEvet = new ManualResetEvent(false);
            Exception ex = null;
            var task = new Thread(new ThreadStart(() =>
            {
                try
                {
                    // Update Rates
                    var list = new List<RateRoomDetailsDataWithChildRecordsCustom>();

                    if (!builder.Data.IsInsert)
                        builder.Data.CorrelationId = Guid.NewGuid();

                    foreach (var date in builder.Data.Periods)
                    {
                        foreach (var item in builder.Data.RateRooms)
                        {
                            // Check if is Derived Rate
                            var rate = builder.Data.Rates.FirstOrDefault(x => x.UID == item.Rate_UID);
                            if (rate.Rate_UID.HasValue && !builder.Data.SendAllRates)
                                continue;

                            var room = builder.Data.RoomAndParameters.FirstOrDefault(x => x.Room.UID == item.RoomType_UID);
                            if (room == null)
                            {
                                result = false;
                                return;
                            }

                            foreach (var roomPrices in room.RoomPrices)
                            {
                                // Create Object to Update Rate
                                var tmp = new RateRoomDetailsDataWithChildRecordsCustom();
                                tmp.AdultPriceList = new List<decimal>();
                                tmp.ChildPriceList = new List<decimal>();
                                tmp.NoOfAdultList = new List<int>();
                                tmp.NoOfChildsList = new List<int>();
                                tmp.AdultPriceVariationIsPercentage = new List<bool>();
                                tmp.AdultPriceVariationIsValueDecrease = new List<bool>();
                                tmp.AdultPriceVariationValue = new List<decimal>();

                                if (builder.Data.IsPriceChanged) // Don't send prices if prices don't changed
                                {
                                    // Adult Prices
                                    if (roomPrices.IsPriceVariation)
                                        tmp.AdultPriceList.AddRange(roomPrices.AdultsPriceVariationList);
                                    else
                                        tmp.AdultPriceList.AddRange(roomPrices.AdultsPriceList ?? new List<decimal>());

                                    tmp.NoOfAdultList.AddRange(roomPrices.NoOfAdultsList); // Adult_1,_2...

                                    // Variation
                                    if (roomPrices.AdultPriceVariationIsPercentage != null)
                                        tmp.AdultPriceVariationIsPercentage.AddRange(roomPrices.AdultPriceVariationIsPercentage);

                                    if (roomPrices.AdultPriceVariationIsValueDecrease != null)
                                        tmp.AdultPriceVariationIsValueDecrease.AddRange(roomPrices.AdultPriceVariationIsValueDecrease);

                                    if (roomPrices.AdultsPriceVariationList != null)
                                        tmp.AdultPriceVariationValue.AddRange(roomPrices.AdultsPriceVariationList);

                                    // Children Prices
                                    if (roomPrices.IsPriceVariation)
                                        tmp.ChildPriceList.AddRange(roomPrices.ChildrenPriceVariationList);
                                    else
                                        tmp.ChildPriceList.AddRange(roomPrices.ChildrenPriceList ?? new List<decimal>());

                                    tmp.ChildPriceVariationIsPercentage = roomPrices.ChildPriceVariationIsPercentage;
                                    tmp.ChildPriceVariationIsValueDecrease = roomPrices.ChildPriceVariationIsValueDecrease;
                                    tmp.ChildPriceVariationValue = roomPrices.ChildrenPriceVariationList;
                                    tmp.NoOfChildsList.AddRange(roomPrices.NoOfChildrenList); // Child_1,_2...
                                }

                                // EXTRA BED
                                // Hack To Update ExtraBed Price
                                if (builder.Data.IsInsert)
                                    tmp.ExtraBedPrice = roomPrices.ExtraBedPrice;

                                if (roomPrices.ExtraBedPrice > 0 && !builder.Data.SendAllRates)
                                {
                                    tmp.AdultPriceList.Insert(0, roomPrices.ExtraBedPrice.Value);
                                    tmp.NoOfAdultList.Insert(0, 0);
                                }

                                // Extra Bed Variation
                                tmp.PriceVariationExtraBedValue = roomPrices.ExtraBedVariationValue;
                                tmp.PriceVariationIsExtraBedPercentage = roomPrices.ExtraBedVariationIsPercentage;
                                tmp.PriceVariationIsExtraBedValueDecrease = roomPrices.ExtraBedVariationIsValueDecrease;
                                if (roomPrices.ExtraBedPrice > 0 && !builder.Data.SendAllRates)
                                {
                                    tmp.AdultPriceVariationIsPercentage.Insert(0, roomPrices.ExtraBedVariationIsPercentage);
                                    tmp.AdultPriceVariationIsValueDecrease.Insert(0, roomPrices.ExtraBedVariationIsValueDecrease);
                                    tmp.AdultPriceVariationValue.Insert(0, roomPrices.ExtraBedVariationValue);
                                }

                                // Allotment
                                tmp.Allotment = roomPrices.Allotment;

                                // Is Price Variation
                                tmp.IsPriceVariation = roomPrices.IsPriceVariation;

                                // Close/Open Sales and Channel List
                                tmp.IsBlocked = builder.Data.isCloseSales;
                                tmp.SelectedChannelList = builder.Data.channelList;
                                tmp.closedDays = ShiftLeft(builder.Data.CloseChannelsWeekdays.ToArray()).ToList();

                                // Restrições
                                tmp.IsClosedOnArr = builder.Data.ClosedOnArrival;
                                tmp.arrClosedDays = ShiftLeft(builder.Data.ClosedOnArrivalWeekdays.ToArray()).ToList();
                                tmp.IsClosedOnDep = builder.Data.ClosedOnDeparture;
                                tmp.depClosedDays = ShiftLeft(builder.Data.ClosedOnDepartureWeekdays.ToArray()).ToList();
                                tmp.MaxDays = builder.Data.MaximumLengthOfStay;
                                tmp.MinDays = builder.Data.MinimumLengthOfStay;
                                tmp.StayThrough = builder.Data.StayThrough;
                                tmp.ReleaseDays = builder.Data.ReleaseDays;

                                // WeekDays
                                tmp.IsSunday = roomPrices.WeekDays[0];
                                tmp.IsMonday = roomPrices.WeekDays[1];
                                tmp.IsTuesday = roomPrices.WeekDays[2];
                                tmp.IsWednesday = roomPrices.WeekDays[3];
                                tmp.IsThursday = roomPrices.WeekDays[4];
                                tmp.IsFriday = roomPrices.WeekDays[5];
                                tmp.IsSaturday = roomPrices.WeekDays[6];

                                // To Discover
                                tmp.Allocation = null;

                                // User that made the update
                                tmp.CreatedBy = builder.Data.UserId;

                                // Changes Made
                                tmp.RateRoomDetailChangedCustom_IsAllotmentChanged = roomPrices.Allotment != -1 && roomPrices.Allotment.HasValue;
                                tmp.RateRoomDetailChangedCustom_IsCancellationPolicyChanged = false;
                                tmp.RateRoomDetailChangedCustom_IsClosedOnArrivalChanged = builder.Data.IsClosedOnArrivalChanged;
                                tmp.RateRoomDetailChangedCustom_IsClosedOnDepartureChanged = builder.Data.IsClosedOnDepartureChanged;
                                tmp.RateRoomDetailChangedCustom_IsDepositPolicyChanged = false;
                                tmp.RateRoomDetailChangedCustom_IsExtraBedPriceChanged = roomPrices.ExtraBedPrice > 0;
                                tmp.RateRoomDetailChangedCustom_IsMaxDaysChanged = builder.Data.IsMaxDaysChanged;
                                tmp.RateRoomDetailChangedCustom_IsMinDaysChanged = builder.Data.IsMinDaysChanged;
                                tmp.RateRoomDetailChangedCustom_IsPriceChanged = builder.Data.IsPriceChanged;
                                tmp.RateRoomDetailChangedCustom_IsReleaseDaysChanged = builder.Data.IsReleaseDaysChanged;
                                tmp.RateRoomDetailChangedCustom_IsStayThroughChanged = builder.Data.IsStayThroughChanged;
                                tmp.RateRoomDetailChangedCustom_IsStoppedSaleChanged = builder.Data.IsStoppedSaleChanged;

                                // Rate, Room, Dates
                                tmp.RateId = item.Rate_UID;
                                tmp.RoomTypeId = item.RoomType_UID;
                                tmp.RRDFromDate = date.DateFrom.Ticks;
                                tmp.RRDToDate = date.DateTo.Ticks;

                                // OLD PRICE (PREÇO POR QUARTO) NOT USED
                                tmp.Price = -1;
                                tmp.PriceVariationIsPercentage = false;
                                tmp.PriceVariationIsValueDecrease = false;
                                tmp.PriceVariationValue = 0;

                                list.Add(tmp);
                            }
                        }
                    }

                    RateRoomDetailsManagerPOCO.RateRoomDetailsDataInsertWithChildRecordsBatch(list, true, builder.Data.PropertyId,
                                builder.Data.CorrelationId.ToString());
                    
                    RateRoomDetailsManagerPOCO.WaitForAllBackgroundWorkers();

                    result = true;

                }
                catch (Exception e)
                {
                    ex = e;
                    result = false;
                }
                finally
                {
                    resetEvet.Set();
                }

            }));
            task.Start();
            resetEvet.WaitOne();

            //if (ex != null)
            //    ExceptionDispatchInfo.Capture(ex).Throw();

            return result;
        }

        /// <summary>
        /// Creates the prices.
        /// </summary>
        /// <param name="nPersons">The n persons.</param>
        /// <param name="price">The price.</param>
        /// <param name="variationForEachAdult">The variation for each adult.</param>
        /// <param name="isVariationDecrease">if set to <c>true</c> [is variation decrease].</param>
        /// <returns></returns>
        private List<decimal> CreatePrices(int nPersons, decimal price, decimal variationForEachAdult = 0, bool isVariationDecrease = false)
        {
            var Prices = new List<decimal>();
            for (var i = 1; i <= nPersons; i++)
            {
                if (isVariationDecrease)
                {
                    Prices.Add(price - variationForEachAdult);
                    price -= variationForEachAdult;
                }
                else
                {
                    Prices.Add(price + variationForEachAdult);
                    price += variationForEachAdult;
                }
            }

            return Prices;
        }

        /// <summary>
        /// Creates prices Variations.
        /// </summary>
        /// <param name="nPersons">The n persons.</param>
        /// <param name="price">The price.</param>
        /// <param name="variationForEachAdult">The variation for each adult.</param>
        /// <param name="isVariationDecrease">if set to <c>true</c> [is variation decrease].</param>
        /// <returns></returns>
        private List<PriceVariationValues> CreatePricesVariation(int nPersons, decimal startVariation, decimal variationForEachAdult = 0, bool isVariationDecrease = false)
        {
            var Prices = new List<PriceVariationValues>();
            var rand = new Random();
            for (var i = 1; i <= nPersons; i++)
            {
                var variation = new PriceVariationValues();
                if (isVariationDecrease)
                {
                    variation.Value = Math.Abs(startVariation - variationForEachAdult);
                    variation.IsPercentage = rand.NextDouble() > 0.5;
                    variation.IsValueDecreased = rand.NextDouble() > 0.5;
                    Prices.Add(variation);
                    startVariation -= variationForEachAdult;
                }
                else
                {
                    //variation.Value = Math.Abs(startVariation - variationForEachAdult);
                    variation.Value = 10;
                    variation.IsPercentage = false;
                    variation.IsValueDecreased = true;
                    //variation.IsPercentage = rand.NextDouble() > 0.5;
                    //variation.IsValueDecreased = rand.NextDouble() > 0.5;
                    Prices.Add(variation);
                    startVariation += variationForEachAdult;
                }
            }

            return Prices;
        }

        /// <summary>
        /// Creates the multi level derived rates.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="filhos">The filhos.</param>
        /// <param name="level">The level.</param>
        private void CreateMultiLevelDerivedRates(ref UpdateRatesBuilder builder, long parentId, int filhos, int level)
        {
            if (level == 0) return;
            var rand = new Random();
            // Add Rates
            for (int i = 0; i < filhos; i++)
            {
                builder.AddDerivedRate(parentId, 10 /*rand.Next(0, 100)*/, true /* rand.NextDouble() > 0.5*/, false/* rand.NextDouble() > 0.5*/);
                if (level > 0)
                    CreateMultiLevelDerivedRates(ref builder, builder.Data.Rates.Last().UID, filhos, level - 1);
            }
        }

        /// <summary>
        /// Shit array items to left
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        public T[] ShiftLeft<T>(T[] arr)
        {
            T[] tmpArr = new T[arr.Length];
            for (int i = 1; i < arr.Length; i++)
                tmpArr[i - 1] = arr[i];

            // copy First to last
            tmpArr[arr.Length - 1] = arr[0];

            return tmpArr;
        }

        private UpdateRatesBuilder CreateRateStructure(UpdateRatesBuilder builder)
        {
            // Rate 1
            builder.AddRate();
            var rate1Id = builder.Data.Rates.Last().UID;

            // 1.1
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, (decimal)10.5, true, true);

            // 1.1.1
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, 0, false, false);

            // 1.1.1.1
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, (decimal)10.33, false, true);

            // 1.2
            builder.AddDerivedRate(rate1Id, (decimal)10.45, false, false);

            // 1.2.1
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, (decimal)10.88, true, true);

            // Rate 2
            builder.AddRate();
            var rate2Id = builder.Data.Rates.Last().UID;

            // 2.1
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, (decimal)6.5, true, true);

            // 2.1.1
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, 0, false, false);

            // 2.1.1.1
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, (decimal)8.33, false, true);

            // 2.2
            builder.AddDerivedRate(rate2Id, (decimal)7.45, false, false);

            // 2.2.1
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, (decimal)2.88, true, true);

            return builder;
        }

        private UpdateRatesBuilder CreateRoomStructure(UpdateRatesBuilder builder, int allotment, List<bool> weekdays = null, decimal price = 100)
        {
            if (weekdays == null)
                weekdays = new List<bool> { true, true, true, true, true, true, true };

            builder.AddRoom(3, 1, 0, 0, 3, false, false)
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(3, price), null, allotment, 0, weekdays)

                .AddRoom(20, 1, 20, 1, 40, true, true)
                .AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(20, price), this.CreatePrices(20, 50, 2), allotment, price, weekdays)

                .AddRateRooms();

            return builder;
        }

        private UpdateRatesBuilder CreateRapidTestStructure(UpdateRatesBuilder builder)
        {
            builder = builder.AddRate();
            builder.AddDerivedRate(builder.Data.Rates.Last().UID, 10, false, false);
            builder.AddRoom().AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(1, 100), null, 10);
            builder.AddRoom(20, 1, 20, 1, 40, false, true).AddRoomPrices(builder.Data.RoomAndParameters.Last(), this.CreatePrices(20, 100), this.CreatePrices(20, 100), 10);
            builder.AddRateRooms();
            return builder;
        }

        #endregion

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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using OB.DL.Common;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Interfaces;
using System.Linq;
using System.Collections.Generic;
using OB.BL.Operations.Helper;
using OB.Services.IntegrationTests.Helpers;
using OB.BL.Contracts.Responses;
using OB.BL.Contracts.Data.Rates;
using OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.BL.Operations.Test
{


    [TestClass]
    public class ReservationValidatorPOCOIntegrationTest : IntegrationBaseTest
    {
        private const int BACKGROUND_THREAD_WAIT_TIME = 12000;
        private const int BACKGROUND_THREAD_MAX_WAIT_TIME = 20000;
         
        
        private IReservationManagerPOCO _reservationManagerPOCO;
        public IReservationManagerPOCO ReservationManagerPOCO
        {
            get
            {
                if (_reservationManagerPOCO == null)
                    _reservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();

                return _reservationManagerPOCO;
            }
            set { _reservationManagerPOCO = value; }
        
        }

        private IPropertyEventsManagerPOCO _propertyEventsManagerPOCO;
        public IPropertyEventsManagerPOCO PropertyEventsManagerPOCO
        {
            get
            {
                if (_propertyEventsManagerPOCO == null)
                    _propertyEventsManagerPOCO = this.Container.Resolve<IPropertyEventsManagerPOCO>();

                return _propertyEventsManagerPOCO;
            }
            set { _propertyEventsManagerPOCO = value; }

        }

        private IReservationValidatorPOCO _reservationValidatorPOCO;
        public IReservationValidatorPOCO ReservationValidatorPOCO
        {
            get
            {
                if (_reservationValidatorPOCO == null)
                    _reservationValidatorPOCO = this.Container.Resolve<IReservationValidatorPOCO>();

                return _reservationValidatorPOCO;
            }
            set
            {
                _reservationValidatorPOCO = value;
            }
        }

       

        [ClassInitialize]
        public static void TestClassInitialize(TestContext context)
        {
            //string outFile = "c:\\entityProfile.EFProf";
            //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.InitializeOfflineProfiling(outFile);
            if (context.Properties.Contains("EntityFrameworkProfiler"))
            {
                if(string.Equals("true", context.Properties["EntityFrameworkProfiler"] as string, StringComparison.InvariantCultureIgnoreCase))
                    HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
            }

            IUnityContainer container = new UnityContainer();
            container = container.AddExtension(new DataAccessLayerModule());

            using (var unitOfWork = container.Resolve<ISessionFactory>().GetUnitOfWork())
            {
                var reservationsRepo = container.Resolve<IRepositoryFactory>().GetReservationsRepository(unitOfWork);

                //Force the Sequence Cache for the Reservation Number to fetch one at a time from the DB (basically it turns off reservation number caching)
                //in order for some tests to pass.
                reservationsRepo.SetSequenceReservationNumberRange(1);
            }
        }

        [TestInitialize]
        public override void Initialize()        
        {
            //_occupancyLevelsManagerPOCO = null;
            //_rateRoomDetailsManagerPOCO = null;
            _reservationManagerPOCO = null;
            //_inventoryManagerPOCO = null;
            _propertyEventsManagerPOCO = null;
            base.Initialize();
        }

        [ClassCleanup]
        public static void TestClassCleanup()
        {
            BaseTest.ClassCleanup();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_ValidRestrictions")]
        //[DeploymentItem("./DL")]
        //public void Test_ValidRestrictions()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var cancellationPolicy = new CancellationPolicy
        //        {
        //            CancellationCosts = false,
        //            Days = 0,
        //            IsCancellationAllowed = true,
        //            PaymentModel = 1,
        //            Name = "Teste",
        //            Description = "teste description"
        //        };

        //    var periods = new List<Period>() { new Period { DateFrom = DateTime.Now.AddDays(1), DateTo = DateTime.Now.AddDays(5) } };
        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll().AddRateChannelsAll(channels)
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 1,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5)
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithAdultPrices(3, 100)
        //                .WithChildPrices(3, 50).WithAllotment(1000);

        //    builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID, "", "", false, 0, true, 1 , periods);
        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Initiate,
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //            RuleType = Reservation.BL.Constants.RuleType.BE,
        //            ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 0, "Expected errors count = 0");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_AllotmentNotAvailable")]
        //[DeploymentItem("./DL")]
        //public void Test_AllotmentNotAvailable()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll().AddRateChannelsAll(channels)
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 1,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5),
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithAllotment(0);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.AllotmentNotAvailable.ToString()), "Expected AllotmentNotAvailable");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidPropertyChannel")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidPropertyChannel()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll().AddRateChannelsAll(channels)
        //                .AddSearchParameters(new SearchParameters
        //                    {
        //                        AdultCount = 1,
        //                        ChildCount = 1,
        //                        CheckIn = DateTime.Now.AddDays(1),
        //                        CheckOut = DateTime.Now.AddDays(5)
        //                    });

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO,
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidPropertyChannel.ToString()), "Expected InvalidPropertyChannel");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidRateChannel")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidRateChannel()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //        //.AddRateChannelsAll(channels)
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 1,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5),
        //                })
        //                .AddPropertyChannels(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRateChannel.ToString()), "Expected InvalidRateChannel");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidRateRoomDetails")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidRateRoomDetails()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //        //.AddRateChannelsAll(channels)
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 1,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(10),
        //                    CheckOut = DateTime.Now.AddDays(12),
        //                })
        //                .AddPropertyChannels(channels)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO,
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.AllotmentNotAvailable.ToString()), "Expected AllotmentNotAvailable");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_MaxOccupancyExceeded")]
        //[DeploymentItem("./DL")]
        //public void Test_MaxOccupancyExceeded()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5),
        //                })
        //                .AddPropertyChannels(channels)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 4,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MaxOccupancyExceeded.ToString()), "Expected MaxOccupancyExceeded");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_AdultMaxOccupancyExceeded")]
        //[DeploymentItem("./DL")]
        //public void Test_AdultMaxOccupancyExceeded()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5),
        //                })
        //                .AddPropertyChannels(channels)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 3,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MaxAdultsExceeded.ToString()), "Expected MaxAdultsExceeded");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_ChildMaxOccupancyExceeded")]
        //[DeploymentItem("./DL")]
        //public void Test_ChildMaxOccupancyExceeded()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5)
        //                })
        //                .AddPropertyChannels(channels)
        //                .AddRateChannelsAll(channels)
        //                .AddChiltTerm();

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 2,
        //                    ChildAges = new List<int>(){1 , 1},
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MaxChildrenExceeded.ToString()), "Expected MaxChildrenExceeded");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_CloseDaysRestriction")]
        //[DeploymentItem("./DL")]
        //public void Test_CloseDaysRestriction()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5)
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithBlockedChannelsListUID(channels)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ClosedDayRestrictionError.ToString()), "Expected ClosedDayRestrictionError");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_MinLenghtOfStay")]
        //[DeploymentItem("./DL")]
        //public void Test_MinLenghtOfStay()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5)
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithMinLenghtOfStay(10)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MinimumLengthOfStayRestrictionError.ToString()), "Expected MinimumLengthOfStayRestrictionError");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_MaxLenghtOfStay")]
        //[DeploymentItem("./DL")]
        //public void Test_MaxLenghtOfStay()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5)
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithMaxLenghtOfStay(2)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.MaximumLengthOfStayRestrictionError.ToString()), "Expected MaximumLengthOfStayRestrictionError");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidStayTrought")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidStayTrought()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5)
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithStayTrought(4)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.StayTroughtRestrictionError.ToString()), "Expected StayTroughtRestrictionError");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidReleaseDays")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidReleaseDays()
        //{
        //    arrange
        //   var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1),
        //                    CheckOut = DateTime.Now.AddDays(5)
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithReleaseDays(5)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO,
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>()
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2),
        //                    DateTo = DateTime.Now.AddDays(5),
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ReleaseDaysRestrictionError.ToString()), "Expected ReleaseDaysRestrictionError");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidCloseOnArrival")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidCloseOnArrival()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1).Date,
        //                    CheckOut = DateTime.Now.AddDays(5).Date
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithCloseOnArrival(true)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ClosedOnArrivalRestrictionError.ToString()), "Expected ClosedOnArrivalRestrictionError");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidCloseOnDeparture")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidCloseOnDeparture()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate().AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1).Date,
        //                    CheckOut = DateTime.Now.AddDays(5).Date
        //                })
        //                .AddPropertyChannels(channels)
        //                .WithCloseOnDeparture(true)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ClosedOnDepartureRestrictionError.ToString()), "Expected ClosedOnDepartureRestrictionError");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_RateBeginSellingPeriod")]
        //[DeploymentItem("./DL")]
        //public void Test_RateBeginSellingPeriod()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate(string.Empty, DateTime.Now.AddDays(10).Date)
        //                .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1).Date,
        //                    CheckOut = DateTime.Now.AddDays(5).Date
        //                })
        //                .AddPropertyChannels(channels)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.RateIsNotForSale.ToString()), "Expected RateIsNotForSale");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_RateEndSellingPeriod")]
        //[DeploymentItem("./DL")]
        //public void Test_RateEndSellingPeriod()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };

        //    builder = builder.AddRate(string.Empty, DateTime.Now.Date.AddDays(-5), DateTime.Now.AddDays(-1).Date)
        //                .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //                .AddRateRoomsAll()
        //                .AddSearchParameters(new SearchParameters
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    CheckIn = DateTime.Now.AddDays(1).Date,
        //                    CheckOut = DateTime.Now.AddDays(5).Date
        //                })
        //                .AddPropertyChannels(channels)
        //                .AddRateChannelsAll(channels);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.RateIsNotForSale.ToString()), "Expected RateIsNotForSale");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_OperatorPaymentMethodNotAllowed")]
        //[DeploymentItem("./DL")]
        //public void Test_OperatorPaymentMethodNotAllowed()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 84 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(84, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(4);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 84
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidPaymentMethod.ToString()), "Expected InvalidPaymentMethod");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_BeTpiPaymentMethodNotAllowed")]
        //[DeploymentItem("./DL")]
        //public void Test_BeTpiPaymentMethodNotAllowed()
        //{
        //    // arrange
        //    var propertyId = 1806;
        //    var builder = new SearchBuilder(Container, propertyId);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 3, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        //.AddTPI(propertyId, Constants.TPIType.TravelAgent)
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, propertyId)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithTPI(builder.InputData.Tpi)
        //                    .WithBilledTypePayment(4);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidPaymentMethod.ToString()), "Expected InvalidPaymentMethod");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidReservation")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidReservation()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationUid = long.MaxValue,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = long.MaxValue + "/1"
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ReservationDoesNotExist.ToString()), "Expected ReservationDoesNotExist");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_ReservationIsOnRequest")]
        //[DeploymentItem("./DL")]
        //public void Test_ReservationIsOnRequest()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1)
        //                    .WithBookingOnRequest();

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ReservationIsOnRequest.ToString()), "Expected ReservationIsOnRequest");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidRoomNumber")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidRoomNumber()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO,
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = "asdasd"
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRoom.ToString()), "Expected InvalidRoom");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_EmptyRoomNumber")]
        //[DeploymentItem("./DL")]
        //public void Test_EmptyRoomNumber()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = ""
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRoom.ToString()), "Expected InvalidRoom");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidUpdateRequest")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidUpdateRequest()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO,
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>(),
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRoom.ToString()), "Expected InvalidRoom");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_InvalidUpdateRequest2")]
        //[DeploymentItem("./DL")]
        //public void Test_InvalidUpdateRequest2()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>()
        //        {
        //            new UpdateRoom()
        //        },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.InvalidRoom.ToString()), "Expected InvalidRoom");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_CancelationCosts")]
        //[DeploymentItem("./DL")]
        //public void Test_CancelationCosts()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(true, 2, true, 1, 100)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.CancelationCostsAppliedError.ToString()), "Expected CancelationCostsAppliedError");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_ChildrenAgesMissing_NULL")]
        //[DeploymentItem("./DL")]
        //public void Test_ChildrenAgesMissing_NULL()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    //ChildAges = childAges,
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ChildrenAgesMissing.ToString()), "Expected ChildrenAgesMissing");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_ChildrenAgesMissing_Empty")]
        //[DeploymentItem("./DL")]
        //public void Test_ChildrenAgesMissing_Empty()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = new List<int>(),
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ChildrenAgesMissing.ToString()), "Expected ChildrenAgesMissing");
        //}

        //[TestMethod]
        //[TestCategory("ReservationValidatorIntegrationTest")]
        //[DeploymentItem("./Databases_WithData", "Test_ChildrenAgesMissing_IncorrectNumber")]
        //[DeploymentItem("./DL")]
        //public void Test_ChildrenAgesMissing_IncorrectNumber()
        //{
        //    // arrange
        //    var builder = new SearchBuilder(Container, 1806);
        //    var channels = new List<long>() { 1 };
        //    var childAges = new List<int>() { 1 };
        //    var paymentTypes = new List<long>() { 1 };

        //    builder = builder.AddRate()
        //        .AddRoom(string.Empty, true, false, 2, 1, 1, 0, 4)
        //        .AddRateRoomsAll()
        //        .AddSearchParameters(new SearchParameters
        //        {
        //            AdultCount = 2,
        //            ChildCount = 1,
        //            CheckIn = DateTime.Now.AddDays(1).Date,
        //            CheckOut = DateTime.Now.AddDays(5).Date
        //        })
        //        .AddPropertyChannels(channels)
        //        .AddRateChannelsAll(channels);
        //                //.AddRateChannelsPaymentMethodsAll(builder.InputData.RateChannels.Select(x => x.UID).ToList(), paymentTypes);

        //    builder.CreateRateRoomDetails();

        //    var resBuilder = new ReservationDataBuilder(1, 1806)
        //                    .WithNewGuest()
        //                    .WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
        //                        builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
        //                        builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID)
        //                    .WithCancelationPolicy(false, 0, true, 1)
        //                    .WithRoomDetails(1, builder.InputData.SearchParameter[0].CheckIn, builder.InputData.SearchParameter[0].CheckOut)
        //                    .WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges)
        //                    .WithBilledTypePayment(1);

        //    var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, 
        //        PropertyEventsManagerPOCO, false, true, false, false);

        //    var response = resBuilder.GetReservation(ReservationManagerPOCO, result);

        //    var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new Reservation.BL.Contracts.Requests.ModifyReservationRequest()
        //    {
        //        ReservationNumber = response.Result.First().Number,
        //        ReservationRooms = new List<UpdateRoom>() 
        //            {
        //                new UpdateRoom
        //                {
        //                    AdultCount = 2,
        //                    ChildCount = 1,
        //                    ChildAges = new List<int>(){1 ,2},
        //                    DateFrom = DateTime.Now.AddDays(2).Date,
        //                    DateTo = DateTime.Now.AddDays(5).Date,
        //                    Number = response.Result.First().ReservationRooms.First().ReservationRoomNo
        //                }
        //            },
        //        RuleType = Reservation.BL.Constants.RuleType.BE,
        //        ChannelId = 1
        //    }, Reservation.BL.Constants.ReservationAction.Modify);

        //    var errors = modifyResponse.Errors;

        //    Assert.IsTrue(errors.Count == 1, "Expected errors count = 1");
        //    Assert.IsTrue(errors.First().ErrorType.Equals(Errors.ChildrenAgesMissing.ToString()), "Expected ChildrenAgesMissing");
        //}
    }
}

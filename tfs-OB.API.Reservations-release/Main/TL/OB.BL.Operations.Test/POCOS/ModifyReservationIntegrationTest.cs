using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using OB.DL.Common;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Interfaces;
using OB.Domain.Reservations;
using System.Linq;
using System.Collections.Generic;
using OB.BL.Operations.Internal.TypeConverters;
using contractsReservations = OB.BL.Contracts.Data.Reservations;
using OB.BL.Operations.Helper;
using OB.Domain.Properties;
using OB.DL.Common.QueryResultObjects;
using OB.BL.Contracts.Requests;
using OB.Services.IntegrationTests.Helpers;
using OB.BL.Contracts.Responses;
using OB.Domain.General;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;

namespace OB.BL.Operations.Test
{


    [TestClass]
    public class ModifyReservationIntegrationTest : IntegrationBaseTest
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


        private IOccupancyLevelsManagerPOCO _occupancyLevelsManagerPOCO;
        public IOccupancyLevelsManagerPOCO OccupancyLevelsManagerPOCO
        {
            get
            {
                if (_occupancyLevelsManagerPOCO == null)
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
            set
            {
                _rateRoomDetailsManagerPOCO = value;
            }
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
                if (string.Equals("true", context.Properties["EntityFrameworkProfiler"] as string, StringComparison.InvariantCultureIgnoreCase))
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
            _occupancyLevelsManagerPOCO = null;
            _rateRoomDetailsManagerPOCO = null;
            _reservationManagerPOCO = null;
            _inventoryManagerPOCO = null;
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

        #region PRICE CALCULATION

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculation")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculation()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);

            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;

            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 190, 190, 190, 190, 190, 190);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;

            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 180, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #region CHILDTERMS

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationWithChildTermsFreeChilds")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationWithChildTermsFreeChilds()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 0 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20, 2)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 0, 2, false, null, null, null, true, true, false, false, null) // Free Child
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            // Free Child
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 0, 110, 110, 110, 110, 110, 110);

            // 2 Free Child
            room.AdultCount = 1;
            room.ChildCount = 2;
            ages.Add(1);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 0, 110, 110, 110, 110, 110, 110);

            // 3 Free Child tieh max free childs = 2
            room.AdultCount = 1;
            room.ChildCount = 3;
            ages.Add(1);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);

            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationWithChildTermsNormalChild")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationWithChildTermsNormalChild()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null) // Normal Child
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 190, 190, 190, 190, 190, 190);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 3;
            ages.Add(4);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 80, 190, 190, 190, 190, 190, 190);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentagePlus")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentagePlus()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 9 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 9, 10, false, 10, true, false, false, true, false, true, currencies) // Variation percentage +
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            //  Variation percentage +
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 66, 176, 176, 176, 176, 176, 176);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentageMinus")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentageMinus()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 11 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 11, 12, false, 10, true, false, false, true, true, true, currencies) // Variation percentage -
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 2,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            //  Variation percentage +
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 54, 174, 174, 174, 174, 174, 174);

            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationWithChildTermsNormalChildWithVariationValuePlus")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationValuePlus()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 13 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 13, 14, false, 10, false, false, false, true, false, true, currencies) // Variation Value +
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            //  Variation percentage +
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 180, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationWithChildTermsNormalChildWithVariationValueMinus")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationValueMinus()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 9 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 15, 16, false, 10, false, false, false, true, true, true, currencies) // Variation Value -
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            // Variation Value -
            room.AdultCount = 1;
            room.ChildCount = 2;
            ages.Clear();
            ages.Add(15);
            ages.Add(16);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 50, 160, 160, 160, 160, 160, 160);

            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentageAndValue")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationWithChildTermsNormalChildWithVariationPercentageAndValue()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 9, 15 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 9, 10, false, 10, true, false, false, true, false, true, currencies) // Variation percentage +
                        .AddChiltTerm("", 15, 16, false, 10, false, false, false, true, true, true, currencies) // Variation Value -
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            //  Variation percentage +
            //  Variation Value -
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 63.50M, 173.5M, 173.5M, 173.5M, 173.5M, 173.5M, 173.5M);

            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationWithChildTermsAdultChild")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationWithChildTermsAdultChild()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 0 };
            var currencies = new List<ChildTermsCurrency>();
            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            });

            currencies.Add(new ChildTermsCurrency
            {
                Currency_UID = 16,
                Value = 20
            });

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 6, 8, true, null, null, null, false, true, false, false, null) // Child As Adult
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            // Free Child
            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 0, 120, 120, 120, 120, 120, 120);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            // Normal Child
            room.AdultCount = 2;
            room.ChildCount = 1;
            ages.Clear();
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 130, 0, 130, 130, 130, 130, 130, 130);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            // Child as Adult
            room.AdultCount = 1;
            room.ChildCount = 2;
            ages.Clear();
            ages.Add(7);
            ages.Add(7);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 130, 0, 130, 130, 130, 130, 130, 130);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #endregion

        #region PRICE ADDON

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationPlusValueAddon")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationPlusValueAddon()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, false, false)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 180, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 200, 200, 200, 200, 200, 200);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 190, 190, 190, 190, 190, 190);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationMinusValueAddon")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationMinusValueAddon()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, false, true)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 160, 160, 160, 160, 160, 160);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 180, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationPlusPercentageAddon")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationPlusPercentageAddon()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, true, false)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 187, 187, 187, 187, 187, 187);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 209, 209, 209, 209, 209, 209);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 198, 198, 198, 198, 198, 198);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationMinusPercentageAddon")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationMinusPercentageAddon()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, true, true)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 153, 153, 153, 153, 153, 153);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 2;
            room.ChildCount = 2;
            ages.Add(3);
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 120, 70, 171, 171, 171, 171, 171, 171);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");

            room.AdultCount = 1;
            room.ChildCount = 2;
            parameters.AdultCount = room.AdultCount.Value;
            parameters.ChildCount = room.ChildCount.Value;
            parameters.Ages = ages;
            rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 70, 162, 162, 162, 162, 162, 162);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #endregion

        #region PRICE RATE BUYER GROUP

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationGDSRateBuyerGroup")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationGDSRateBuyerGroup()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddTPI(1806, Constants.TPIType.TravelAgent)
                        .AddRateBuyerGroup(builder.InputData.Rates[0].UID, builder.InputData.Tpi.UID, false, false, 10, 20, false, false);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.GDS);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
                TpiId = builder.InputData.Tpi.UID
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 190, 190, 190, 190, 190);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationRateBuyerGroup")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationRateBuyerGroup()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddTPI(1806, Constants.TPIType.TravelAgent)
                        .AddRateBuyerGroup(builder.InputData.Rates[0].UID, builder.InputData.Tpi.UID, false, false, 10, 20, false, false);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                TPIDiscountValue = room.TPIDiscountValue,
                TpiId = builder.InputData.Tpi.UID
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 180, 180, 180, 180, 180);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #endregion

        #region PRICE PROMOTIONAL CODE

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationPromotionalCodeValue")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationPromotionalCodeValue()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddPromoCode(1806, builder.InputData.Rates[0].UID, "123456", DateTime.Now.Date, DateTime.Now.AddDays(10).Date, 10M, false)
                        .AddPromoCodeCurrencies(builder.InputData.PromoCode.UID, 34, 15M);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                PromotionalCodeId = builder.InputData.PromoCode.UID
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 167, 170, 167, 167);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationPromotionalCodePercentage")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationPromotionalCodePercentage()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddPromoCode(1806, builder.InputData.Rates[0].UID, "123456", DateTime.Now.Date, DateTime.Now.AddDays(10).Date, 10M, true)
                        .AddPromoCodeCurrencies(builder.InputData.PromoCode.UID, 34, 15M);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                PromotionalCodeId = builder.InputData.PromoCode.UID
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 153, 170, 153, 153);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationInvalidPromotionalCode")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationInvalidPromotionalCode()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddPromoCode(1806, builder.InputData.Rates[0].UID, "123", DateTime.Now.Date, DateTime.Now.AddDays(10).Date, 10, false)
                        .AddPromoCodeCurrencies(builder.InputData.PromoCode.UID, 34, 15M);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(5),
                BaseCurrency = 34,
                AdultCount = 1,
                ChildCount = 1,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = false,
                PropertyId = 1806,
                RateId = builder.InputData.Rates[0].UID,
                RoomTypeId = builder.InputData.RoomTypes[0].UID,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                PromotionalCode = "123"
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationUsedPromotionalCode")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationUsedPromotionalCode()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddPromoCode(1806, builder.InputData.Rates[0].UID, "123456", DateTime.Now.Date, DateTime.Now.AddDays(10).Date, 10, false, 10, 10)
                        .AddPromoCodeCurrencies(builder.InputData.PromoCode.UID, 34, 15M);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(5),
                BaseCurrency = 34,
                AdultCount = 1,
                ChildCount = 1,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = false,
                PropertyId = 1806,
                RateId = builder.InputData.Rates[0].UID,
                RoomTypeId = builder.InputData.RoomTypes[0].UID,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                PromotionalCode = "123456"
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #endregion

        #region PRICE INCENTIVES

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationIncentive")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationIncentive()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, false);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 153, 153, 153, 153);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationIncentiveCumulative")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationIncentiveCumulative()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 137.7M, 137.7M, 137.7M, 137.7M);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationIncentiveDiferentPriceDays")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationIncentiveDiferentPriceDays()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate().AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, true)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 5, 10, 1, 3, false, false);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            var expectedRrdList = new List<RateRoomDetailQR1>();

            #region EXPECTED DATA

            expectedRrdList.Add(new RateRoomDetailQR1
            {
                Date = DateTime.Now,
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailQR1
            {
                Date = DateTime.Now.AddDays(1),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailQR1
            {
                Date = DateTime.Now.AddDays(2),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailQR1
            {
                Date = DateTime.Now.AddDays(3),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 170,
                PriceAfterIncentives = 170,
                PriceAfterRateModel = 170,
                FinalPrice = 170
            });
            expectedRrdList.Add(new RateRoomDetailQR1
            {
                Date = DateTime.Now.AddDays(4),
                AdultPrice = 110,
                ChildPrice = 60,
                PriceAfterAddOn = 170,
                PriceAfterBuyerGroups = 170,
                PriceAfterPromoCodes = 0,
                PriceAfterIncentives = 0,
                PriceAfterRateModel = 0,
                FinalPrice = 0
            });

            #endregion

            AssertCalculateReservationRoomPrices(rrdList, expectedRrdList);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #endregion

        #region PRICE RATE MODEL

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationRateModelPackage")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationRateModelPackage()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 2, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 5
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 84,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 153, 153);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationRateModelMarkup")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationRateModelMarkup()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 2, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 2
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 84,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 136, 136);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationRateModelCommission")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationRateModelCommission()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 84,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationRateModelNETCommission")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationRateModelNETCommission()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 84 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, false).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 2, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 84,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 242.86M, 242.86M);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationRateModelNETNotHoteisNet")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationRateModelNETNotHoteisNet()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, false).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 2, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationRateModelRetailNotHoteisNet")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationRateModelRetailNotHoteisNet()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 3 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 0, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 3, 5, false, null, null, null, false, true, false, false, null)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1,
                CommissionType = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 60, 170, 170, 170, 170, 170, 170);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #endregion

        #region MIX MULTIPLE

        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_PriceCalculationMixMultiple")]
        [DeploymentItem("./DL")]
        public void Test_PriceCalculationMixMultiple()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var channels = new List<long>() { 1 };
            var ages = new List<int>() { 1 };

            builder.AddRate("", null, null, true).AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                        .AddRateRoomsAll().AddRateChannelsAll(channels, 1, 10, false, false, true, 10, 20, 30, 40)
                        .AddSearchParameters(new SearchParameters
                        {
                            AdultCount = 1,
                            ChildCount = 1,
                            CheckIn = DateTime.Now,
                            CheckOut = DateTime.Now.AddDays(5)
                        })
                        .AddPropertyChannels(channels)
                        .WithAdultPrices(3, 100, 10)
                        .WithChildPrices(3, 50, 10)
                        .AddChiltTerm("", 0, 2, false, null, null, null, true, true, false, false, null) // Free Child
                        .AddTPI(1806, Constants.TPIType.TravelAgent)
                        .AddRateBuyerGroup(builder.InputData.Rates[0].UID, builder.InputData.Tpi.UID, false, false, 10, 20, false, false)
                        .AddIncentive(1806, builder.InputData.Rates[0].UID, 0, 10, 1, 2, true, false)
                        .WithAllotment(100);

            builder.AddRateCancellationPolicy(builder.InputData.Rates[0].UID);
            builder.CreateRateRoomDetails();

            var helper = Container.Resolve<IReservationHelperPOCO>();
            var room = new ReservationRoom
            {
                Rate_UID = builder.InputData.Rates[0].UID,
                RoomType_UID = builder.InputData.RoomTypes[0].UID,
                DateFrom = builder.InputData.SearchParameter[0].CheckIn.Date,
                DateTo = builder.InputData.SearchParameter[0].CheckOut.Date,
                AdultCount = 1,
                ChildCount = 1
            };

            var unitOfWork = SessionFactory.GetUnitOfWork();
            var groupRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
            var groupRule = groupRepo.GetByGroupType(RuleType.Pull);
            var childTerms = builder.InputData.ChildTerms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
            var parameters = new CalculateFinalPriceParameters
            {
                CheckIn = room.DateFrom.Value,
                CheckOut = room.DateTo.Value,
                BaseCurrency = 34,
                AdultCount = room.AdultCount.Value,
                ChildCount = room.ChildCount ?? 0,
                Ages = ages,
                ChildTerms = childTerms,
                GroupRule = groupRule,
                ExchangeRate = 1,
                IsModify = true,
                PropertyId = 1806,
                RateId = room.Rate_UID.Value,
                RoomTypeId = room.RoomType_UID.Value,
                RateModelId = room.CommissionType != null ? (int)room.CommissionType.Value : 0,
                ChannelId = 1,
                TpiId = builder.InputData.Tpi.UID
            };

            var rrdList = helper.CalculateReservationRoomPrices(parameters);

            AssertCalculateReservationRoomPrices(rrdList, builder.InputData.SearchParameter[0].CheckIn, 110, 0, 120, 130, 117, 117, 117, 117);
            Assert.IsTrue(rrdList.Count == 5, "Expected RRDs = 5");
        }

        #endregion

        private void AssertCalculateReservationRoomPrices(List<RateRoomDetailQR1> rrdList, DateTime dateFrom, decimal adultPrice, decimal childPrice,
            decimal priceAfterAddOn, decimal priceAfterBuyerGroups, decimal priceAfterPromoCodes, decimal priceAfterIncentives, decimal priceAfterRateModel,
            decimal finalPrice)
        {
            for (int i = 0; i < rrdList.Count; i++)
            {
                Assert.IsTrue(rrdList[i].Date.Date == dateFrom.AddDays(i).Date);
                Assert.IsTrue(rrdList[i].ChildPrice == childPrice);
                Assert.IsTrue(rrdList[i].AdultPrice == adultPrice);
                Assert.IsTrue(rrdList[i].PriceAfterAddOn == priceAfterAddOn);
                Assert.IsTrue(rrdList[i].PriceAfterBuyerGroups == priceAfterBuyerGroups);
                Assert.IsTrue(rrdList[i].PriceAfterPromoCodes == priceAfterPromoCodes);
                Assert.IsTrue(rrdList[i].PriceAfterIncentives == priceAfterIncentives);
                Assert.IsTrue(rrdList[i].PriceAfterRateModel == priceAfterRateModel);
                Assert.IsTrue(rrdList[i].FinalPrice == finalPrice);
            }
        }

        private void AssertCalculateReservationRoomPrices(List<RateRoomDetailQR1> rrdList, List<RateRoomDetailQR1> expectedRrdList)
        {
            for (int i = 0; i < rrdList.Count; i++)
            {
                Assert.IsTrue(rrdList[i].Date.Date == expectedRrdList[i].Date.Date);
                Assert.IsTrue(rrdList[i].ChildPrice == expectedRrdList[i].ChildPrice);
                Assert.IsTrue(rrdList[i].AdultPrice == expectedRrdList[i].AdultPrice);
                Assert.IsTrue(rrdList[i].PriceAfterAddOn == expectedRrdList[i].PriceAfterAddOn);
                Assert.IsTrue(rrdList[i].PriceAfterBuyerGroups == expectedRrdList[i].PriceAfterBuyerGroups);
                Assert.IsTrue(rrdList[i].PriceAfterPromoCodes == expectedRrdList[i].PriceAfterPromoCodes);
                Assert.IsTrue(rrdList[i].PriceAfterIncentives == expectedRrdList[i].PriceAfterIncentives);
                Assert.IsTrue(rrdList[i].PriceAfterRateModel == expectedRrdList[i].PriceAfterRateModel);
                Assert.IsTrue(rrdList[i].FinalPrice == expectedRrdList[i].FinalPrice);
            }
        }

        #endregion

        #region POLICIES

        #region CANCELATION POLICIES



        #endregion

        #endregion

        #region UPDATE CREDITS

        #region OPERATORS

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        [DeploymentItem("./Databases_WithData", "Test_UpdateOperatorCreditUsed_ChannelDontHandleCredit")]
        [DeploymentItem("./DL")]
        public void Test_UpdateOperatorCreditUsed_ChannelDontHandleCredit()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            bool sendCreditLimitExceedEmail = false;
            string channelName = "";
            decimal creditLimit = 0;

            var builder = new SearchBuilder(Container, 1806);
            builder.AddPropertyChannels(new List<long> { 1 }, true, false, true, 10, 0);

            helper.UpdateOperatorCreditUsed(1806, 1, 4, false, 100, out sendCreditLimitExceedEmail, out channelName, out creditLimit);

            Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
            Assert.IsTrue(string.IsNullOrEmpty(channelName), "Expected empty string");
            Assert.IsTrue(creditLimit == 0, "Expected 0");
        }

        [TestMethod]
        [TestCategory("ModifyReservation_UpdateCredits")]
        [DeploymentItem("./Databases_WithData", "Test_UpdateOperatorCreditUsed_ExceededLimit")]
        [DeploymentItem("./DL")]
        public void Test_UpdateOperatorCreditUsed_ExceededLimit()
        {
            var helper = Container.Resolve<IReservationHelperPOCO>();
            bool sendCreditLimitExceedEmail = false;
            string channelName = "";
            decimal creditLimit = 0;

            var builder = new SearchBuilder(Container, 1806);
            builder.AddPropertyChannels(new List<long> { 90 }, true, false, true, 10, 0, true);

            helper.UpdateOperatorCreditUsed(1806, 90, 4, false, 100, out sendCreditLimitExceedEmail, out channelName, out creditLimit);

            Assert.IsTrue(sendCreditLimitExceedEmail, "Expected true");
            Assert.IsTrue(channelName.Equals("Atrium Operadora"), "Expected Atrium Operadora");
            Assert.IsTrue(creditLimit == 10, "Expected 10");
        }

        //[TestMethod]
        //[TestCategory("ModifyReservation_UpdateCredits")]
        //public void Test_UpdateOperatorCreditUsed_NoPaymentMethodType()
        //{
        //    var helper = Container.Resolve<IReservationHelperPOCO>();

        //    bool sendCreditLimitExceedEmail = true;
        //    string channelName = "asdasd";
        //    decimal creditLimit = 10;

        //    helper.UpdateOperatorCreditUsed(fixture.Create<long>(), fixture.Create<long>(), null, fixture.Create<bool>(),
        //        fixture.Create<decimal>(), out sendCreditLimitExceedEmail, out channelName, out creditLimit);

        //    Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        //    Assert.IsTrue(string.IsNullOrEmpty(channelName), "Expected string empty");
        //    Assert.IsTrue(creditLimit == 0, "Expected 0");
        //}

        //[TestMethod]
        //[TestCategory("ModifyReservation_UpdateCredits")]
        //public void Test_UpdateOperatorCreditUsed_IsOnRequest()
        //{
        //    var helper = Container.Resolve<IReservationHelperPOCO>();
        //    bool sendCreditLimitExceedEmail = true;
        //    string channelName = "asdasd";
        //    decimal creditLimit = 10;

        //    helper.UpdateOperatorCreditUsed(fixture.Create<long>(), fixture.Create<long>(), fixture.Create<long>(), true,
        //        fixture.Create<decimal>(), out sendCreditLimitExceedEmail, out channelName, out creditLimit);

        //    Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        //    Assert.IsTrue(string.IsNullOrEmpty(channelName), "Expected string empty");
        //    Assert.IsTrue(creditLimit == 0, "Expected 0");
        //}

        //[TestMethod]
        //[TestCategory("ModifyReservation_UpdateCredits")]
        //public void Test_UpdateOperatorCreditUsed_InvalidPaymentType()
        //{
        //    var helper = Container.Resolve<IReservationHelperPOCO>();
        //    bool sendCreditLimitExceedEmail = true;
        //    string channelName = "asdasd";
        //    decimal creditLimit = 10;

        //    _paymentMethodTypeRepoMock.Setup(x => x.Get(new object[] { It.IsAny<long>() }));

        //    _channelPropertiesRepoMock.Setup(x => x.UpdateOperatorCreditUsed(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<decimal>(),
        //        out sendCreditLimitExceedEmail, out channelName, out creditLimit));

        //    helper.UpdateOperatorCreditUsed(fixture.Create<long>(), fixture.Create<long>(), fixture.Create<long>(), true,
        //        fixture.Create<decimal>(), out sendCreditLimitExceedEmail, out channelName, out creditLimit);

        //    Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        //    Assert.IsTrue(string.IsNullOrEmpty(channelName), "Expected string empty");
        //    Assert.IsTrue(creditLimit == 0, "Expected 0");
        //}

        #endregion OPERATORS

        #region TPI

        //[TestMethod]
        //[TestCategory("ModifyReservation_UpdateCredits")]
        //public void Test_UpdateTPICreditUsed_NoTPI()
        //{
        //    var helper = Container.Resolve<IReservationHelperPOCO>();

        //    bool sendCreditLimitExceedEmail = true;

        //    helper.UpdateTPICreditUsed(fixture.Create<long>(), null, fixture.Create<long>(), fixture.Create<decimal>(),
        //        out sendCreditLimitExceedEmail);

        //    Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        //}

        //[TestMethod]
        //[TestCategory("ModifyReservation_UpdateCredits")]
        //public void Test_UpdateTPICreditUsed_NoPaymentMethodType()
        //{
        //    var helper = Container.Resolve<IReservationHelperPOCO>();

        //    bool sendCreditLimitExceedEmail = true;

        //    helper.UpdateTPICreditUsed(fixture.Create<long>(), fixture.Create<long>(), null, fixture.Create<decimal>(),
        //        out sendCreditLimitExceedEmail);

        //    Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        //}

        //[TestMethod]
        //[TestCategory("ModifyReservation_UpdateCredits")]
        //[ExpectedException(typeof(BusinessLayerException))]
        //public void Test_UpdateTPICreditUsed_InvalidPaymentType()
        //{
        //    var helper = Container.Resolve<IReservationHelperPOCO>();
        //    _paymentMethodTypeRepoMock.Setup(x => x.Get(new object[] { It.IsAny<long>() }));

        //    bool sendCreditLimitExceedEmail = true;

        //    helper.UpdateTPICreditUsed(fixture.Create<long>(), fixture.Create<long>(), fixture.Create<long>(), fixture.Create<decimal>(),
        //        out sendCreditLimitExceedEmail);

        //    Assert.IsTrue(!sendCreditLimitExceedEmail, "Expected false");
        //}

        #endregion TPI

        #endregion UPDATE CREDITS

        /// <summary>
        /// Test Adult Count
        /// Test Child Count
        /// Test Child Ages
        /// Test CheckIn
        /// Test CheckOut
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_ModificationBasicOneRoom")]
        [DeploymentItem("./DL")]
        public void Test_ModificationBasicOneRoom()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new ModifyReservationRequest()
            {
                //ReservationUid = oldReservation.UID,
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 2,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 1, 1 },
                            DateFrom = DateTime.Now.AddDays(12).Date,
                            DateTo = DateTime.Now.AddDays(17).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        }
                    },
                RuleType = OB.BL.Constants.RuleType.BE,
                ChannelId = 1,
                TransactionAction = Constants.ReservationTransactionAction.Initiate,
                TransactionType = Constants.ReservationTransactionType.A,
                TransactionId = transactionId
            }, Constants.ReservationAction.Modify);

            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((ModifyReservationResponse)modifyResponse).Reservation;

            var json = modifiedReservation.ToJSON();
            var t = json;

            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
        }


        /// <summary>
        /// Test Only Change Main Guest Name
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_ModifyReservation_ChangeOnlyMainGuestName")]
        [DeploymentItem("./DL")]
        public void Test_ModifyReservation_ChangeOnlyMainGuestName()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new ModifyReservationRequest()
            {
                //ReservationUid = oldReservation.UID,
                Guest = new contractsReservations.ReservationGuest()
                {
                    FirstName = "Modified",
                    LastName = "Name Test"
                },
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 0,
                            DateFrom = DateTime.Now.AddDays(10).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo
                        },
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 0, 2 },
                            DateFrom = DateTime.Now.AddDays(11).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.Last().ReservationRoomNo
                        }
                    },
                RuleType = OB.BL.Constants.RuleType.BE,
                ChannelId = 1,
                TransactionAction = Constants.ReservationTransactionAction.Initiate,
                TransactionType = Constants.ReservationTransactionType.A,
                TransactionId = transactionId
            }, Constants.ReservationAction.Modify);

            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((ModifyReservationResponse)modifyResponse).Reservation;

            var json = modifiedReservation.ToJSON();
            var t = json;

            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            Assert.IsTrue(retunedReservation.GuestFirstName != oldReservation.GuestFirstName);
            Assert.IsTrue(retunedReservation.GuestLastName != oldReservation.GuestLastName);

            ReservationAssert.AssertAllReservationObjectsForMainGuestChange(oldReservation, modifiedReservation);

            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
        }


        /// <summary>
        /// Test Only Change Main Guest Name
        /// </summary>
        [TestMethod]
        [TestCategory("ModifyReservationTest")]
        [DeploymentItem("./Databases_WithData", "Test_ModifyReservation_ChangeOnlyRoomGuestName")]
        [DeploymentItem("./DL")]
        public void Test_ModifyReservation_ChangeOnlyRoomGuestName()
        {
            // arrange
            var builder = new SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806);

            var transactionId = Guid.NewGuid().ToString();
            var oldReservation = InsertReservation(builder, resBuilder, transactionId);

            var modifyResponse = ReservationManagerPOCO.ReservationCoordinator(new ModifyReservationRequest()
            {
                //ReservationUid = oldReservation.UID,
                ReservationNumber = oldReservation.Number,
                ReservationRooms = new List<contractsReservations.UpdateRoom>()
                    {
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 0,
                            DateFrom = DateTime.Now.AddDays(10).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.First().ReservationRoomNo,
                            Guest = new contractsReservations.ReservationGuest()
                            {
                                FirstName = "Modified",
                                LastName = "Name Test"
                            }
                        },
                        new contractsReservations.UpdateRoom
                        {
                            AdultCount = 1,
                            ChildCount = 2,
                            ChildAges = new List<int>() { 0, 2 },
                            DateFrom = DateTime.Now.AddDays(11).Date,
                            DateTo = DateTime.Now.AddDays(15).Date,
                            Number = oldReservation.ReservationRooms.Last().ReservationRoomNo,
                            Guest = new contractsReservations.ReservationGuest()
                            {
                                FirstName = "Modified",
                                LastName = "Name Test"
                            }
                        }
                    },
                RuleType = OB.BL.Constants.RuleType.BE,
                ChannelId = 1,
                TransactionAction = Constants.ReservationTransactionAction.Initiate,
                TransactionType = Constants.ReservationTransactionType.A,
                TransactionId = transactionId
            }, Constants.ReservationAction.Modify);

            var modifiedReservation = resBuilder.GetReservation(ReservationManagerPOCO, oldReservation.UID).Result.First();
            var retunedReservation = ((ModifyReservationResponse)modifyResponse).Reservation;

            var json = modifiedReservation.ToJSON();
            var t = json;

            Assert.IsTrue(modifyResponse.Errors.Count == 0, "Expected errors count = 0");
            Assert.IsTrue(retunedReservation.ReservationRooms[0].GuestName != oldReservation.ReservationRooms[0].GuestName);
            Assert.IsTrue(retunedReservation.ReservationRooms[1].GuestName != oldReservation.ReservationRooms[1].GuestName);

            ReservationAssert.AssertAllReservationObjectsForRoomsGuestChange(oldReservation, modifiedReservation);

            ReservationAssert.AssertAllReservationObjects(modifiedReservation, retunedReservation);
        }



        private contractsReservations.Reservation InsertReservation(SearchBuilder searchBuilder, ReservationDataBuilder resBuilder, string transactionId = "")
        {
            var channels = new List<long>() { 1 };
            var childAges = new List<int>() { 0, 2 };
            var currencies = new List<ChildTermsCurrency>() { new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            }};

            searchBuilder
                .AddRate()
                .AddRoom(string.Empty, true, false, 20, 1, 20, 0, 20)
                .AddRateRoomsAll()
                .AddRateChannelsAll(channels)
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 1,
                    ChildCount = 0,
                    CheckIn = DateTime.Now.AddDays(10),
                    CheckOut = DateTime.Now.AddDays(15)
                })
                .AddSearchParameters(new SearchParameters
                {
                    AdultCount = 1,
                    ChildCount = 2,
                    CheckIn = DateTime.Now.AddDays(11),
                    CheckOut = DateTime.Now.AddDays(15),
                })
                .AddPropertyChannels(channels)
                .WithAdultPrices(3, 100)
                .WithChildPrices(3, 50)
                .AddChiltTerm()
                .AddChiltTerm("", 2, 3, false, 10, false, false, true, true, true, true, currencies)
                .WithAllotment(100)
                .AddPromoCode(1806, searchBuilder.InputData.Rates[0].UID, "123456", DateTime.Now.Date, DateTime.Now.AddDays(10).Date, 10M, false)
                .AddPromoCodeCurrencies(searchBuilder.InputData.PromoCode.UID, 34, 15M)
                .AddIncentive(1806, searchBuilder.InputData.Rates[0].UID, 0, 10, 1, 2, true, false);

            searchBuilder.AddRateCancellationPolicy(searchBuilder.InputData.Rates[0].UID);
            searchBuilder.CreateRateRoomDetails();

            // Prices For The Modification
            searchBuilder.CreateRateRoomDetails(DateTime.Now.AddDays(16), DateTime.Now.AddDays(20), 50);

            resBuilder = GetReservationBuilder(searchBuilder, childAges);

            var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, OccupancyLevelsManagerPOCO, RateRoomDetailsManagerPOCO, InventoryManagerPOCO,
                PropertyEventsManagerPOCO, false, true, false, false, false, transactionId);

            var response = resBuilder.GetReservation(ReservationManagerPOCO, result);
            return response.Result.FirstOrDefault();
        }

        private ReservationDataBuilder GetReservationBuilder(SearchBuilder builder, List<int> childAges = null)
        {
            var resBuilder = new ReservationDataBuilder(1, 1806).WithNewGuest();

            resBuilder.WithRoom(1, builder.InputData.SearchParameter[0].AdultCount,
                            builder.InputData.SearchParameter[0].ChildCount, builder.InputData.SearchParameter[0].CheckIn,
                            builder.InputData.SearchParameter[0].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID);

            resBuilder.WithChildren(1, builder.InputData.SearchParameter[0].ChildCount, childAges);

            resBuilder.WithRoomDetails(1, builder.InputData.RateRoomDetails.Where(x => x.Date >= builder.InputData.SearchParameter[0].CheckIn
                && x.Date <= builder.InputData.SearchParameter[0].CheckOut).ToList());

            // Second Room
            resBuilder.WithRoom(2, builder.InputData.SearchParameter[1].AdultCount,
                            builder.InputData.SearchParameter[1].ChildCount, builder.InputData.SearchParameter[1].CheckIn,
                            builder.InputData.SearchParameter[1].CheckOut, builder.InputData.Rates[0].UID, builder.InputData.RoomTypes[0].UID);

            resBuilder.WithChildren(2, builder.InputData.SearchParameter[1].ChildCount, childAges);

            resBuilder.WithRoomDetails(2, builder.InputData.RateRoomDetails.Where(x => x.Date >= builder.InputData.SearchParameter[1].CheckIn
                && x.Date <= builder.InputData.SearchParameter[1].CheckOut).ToList());

            resBuilder.WithCancelationPolicy(false, 0, true, 1, null, 0);
            resBuilder.WithCancelationPolicy(false, 0, true, 1, null, 1);

            return resBuilder;
        }
    }
}

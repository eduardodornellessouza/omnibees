using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using OB.DL.Common.Interfaces;
using OB.BL.Operations.Interfaces;
using System.Linq;
using System.Collections.Generic;
using OB.Domain.Rates;
using OB.Domain.Properties;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Data.Rates;
using Moq;
using OB.BL.Contracts.Responses;
using OB.Domain.General;
using Microsoft.QualityTools.Testing.Fakes;
using System.Collections;
using System.Linq.Expressions;
using OB.DL.Common;
using OB.DL.Common.Filter;


namespace OB.BL.Operations.Test
{


    [Serializable]
    [TestClass]
    public class RateRoomDetailsPOCOTest_ : IntegrationBaseTest
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

        private Mock<IRateRoomDetailRepository> _rateRoomDetailsRepoMock = null;

        private List<OB.Domain.Rates.RateRoomDetail> _rateRoomDetailsInDatabase = new List<Domain.Rates.RateRoomDetail>();
        private List<AppSetting> _appSettingInDataBase = new List<AppSetting>();
        private List<OB.Domain.Rates.RateRestriction> _rateRestrictionInDataBase = new List<OB.Domain.Rates.RateRestriction>();
        private List<Property> _propertyInDataBase = new List<Property>();
        

        private List<OB.Domain.Rates.Rate> _ratesInDatabase = new List<OB.Domain.Rates.Rate>();
        private List<Currency> _currenciesInDatabase = new List<Currency>();
        private List<OB.Domain.Rates.RateRoom> _rateRoomsInDatabase = new List<OB.Domain.Rates.RateRoom>();
        private List<RoomType> _roomTypesInDatabase = new List<RoomType>();
        private Mock<IRateRepository> _rateRepoMock = null;
        private Mock<ICurrencyRepository> _currencyRepoMock = null;
        private Mock<IRateRoomRepository> _rateRoomRepoMock = null;
        private Mock<IRoomTypesRepository> _roomTypeRepoMock = null;

        private Mock<IAppSettingRepository> _appSettingRepoMock = null;
        private Mock<IRepository<RateRestriction>> _rateRestrictionRepoMock = null;
        private Mock<IRepository<Property>> _propertyRepoMock = null;
        private Mock<ISqlManager> _sqlManagerRepoMock = null;
        private Mock<IRateChannelRepository> _rateChannelRepoMock = null;


        [TestInitialize]
        public override void Initialize()
        {          
            _rateRoomDetailsManagerPOCO = null;

            base.Initialize();

            _rateRoomDetailsInDatabase = new List<Domain.Rates.RateRoomDetail>();
            _appSettingInDataBase = new List<AppSetting>();
            _rateRestrictionInDataBase = new List<RateRestriction>();
            _propertyInDataBase = new List<Property>();

            //Mock RateRoomDetailsRepository to return list of RateRoomDetails instead of macking queries to the databse.
            _rateRoomDetailsRepoMock = new Mock<IRateRoomDetailRepository>(MockBehavior.Default);

            //Mock Repository factory to return mocked Repository
            var repoFactoryMock = new Mock<IRepositoryFactory>();

            _appSettingRepoMock = new Mock<IAppSettingRepository>(MockBehavior.Default);
            _rateRestrictionRepoMock = new Mock<IRepository<RateRestriction>>(MockBehavior.Default);
            _propertyRepoMock = new Mock<IRepository<Property>>(MockBehavior.Default);
            _sqlManagerRepoMock = new Mock<ISqlManager>(MockBehavior.Default);
            _rateChannelRepoMock = new Mock<IRateChannelRepository>(MockBehavior.Default);



            //Mock RateRepository to return list of Rates instead of macking queries to the databse.
            _rateRepoMock = new Mock<IRateRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetRateRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateRepoMock.Object);

            //Mock CurrencyRepository to return list of Currencies instead of macking queries to the databse.
            _currencyRepoMock = new Mock<ICurrencyRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetCurrencyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_currencyRepoMock.Object);

            //Mock BoardTypeRepository to return list of RateRooms instead of macking queries to the databse.
            _rateRoomRepoMock = new Mock<IRateRoomRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetRateRoomRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateRoomRepoMock.Object);

            //Mock RoomTypesRepository to return list of RoomTypes instead of macking queries to the databse.
            _roomTypeRepoMock = new Mock<IRoomTypesRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetRoomTypesRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_roomTypeRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRateRoomDetailRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateRoomDetailsRepoMock.Object);
            
            repoFactoryMock.Setup(x => x.GetAppSettingRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_appSettingRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<RateRestriction>(It.IsAny<IUnitOfWork>()))
                           .Returns(_rateRestrictionRepoMock.Object);
            
            repoFactoryMock.Setup(x => x.GetRepository<Property>(It.IsAny<IUnitOfWork>()))
                           .Returns(_propertyRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<IUnitOfWork>(), OB.Domain.Rates.RateRoomDetail.DomainScope))
                           .Returns(_sqlManagerRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRateChannelRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_rateChannelRepoMock.Object);

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);

            this.Container = this.Container.RegisterInstance<IRepository<OB.Domain.Rates.RateRoomDetail>>(_rateRoomDetailsRepoMock.Object, new TransientLifetimeManager());

            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.RateRoomDetailsManagerPOCO = this.Container.Resolve<IRateRoomDetailsManagerPOCO>();

            FillTheAppSettingforTest();
            FillTheRateRestrictionforTest();
            FillThePropertyforTest();

        }


        #region Fill Data for Testing
        private void FillTheAppSettingforTest()
        {
            _appSettingInDataBase.Add(
                new AppSetting
                {
                    UID = 1,
                    Name = "UpdateRatesMaxThreads",
                    Value = "5",
                    Category = "Omnibees"
                });
        }

        private void FillTheRateRestrictionforTest()
        {
            _rateRestrictionInDataBase.Add(
                new RateRestriction
                {
                    UID = 1,
                    Name = "Minimum Length of Stay",
                    Description = "Minimum Length of Stay",
                });

            _rateRestrictionInDataBase.Add(
                new RateRestriction
                {
                    UID = 2,
                    Name = "Stay through",
                    Description = "Stay through",
                });

            _rateRestrictionInDataBase.Add(
                new RateRestriction
                {
                    UID = 3,
                    Name = "Closed on arrival",
                    Description = "Closed on arrival",
                });

            _rateRestrictionInDataBase.Add(
                new RateRestriction
                {
                    UID = 4,
                    Name = "Closed on departure",
                    Description = "Closed on departure",
                });

            _rateRestrictionInDataBase.Add(
                new RateRestriction
                {
                    UID = 5,
                    Name = "Release days",
                    Description = "Release days",
                });

            _rateRestrictionInDataBase.Add(
                new RateRestriction
                {
                    UID = 6,
                    Name = "Allocation",
                    Description = "Allocation",
                });

            _rateRestrictionInDataBase.Add(
                new RateRestriction
                {
                    UID = 7,
                    Name = "Blocked",
                    Description = "Blocked",
                });

            _rateRestrictionInDataBase.Add(
                new RateRestriction
                {
                    UID = 8,
                    Name = "Maximum Length of Stay",
                    Description = "Maximum Length of Stay",
                });
        }


        private void FillThePropertyforTest()
        {
            _propertyInDataBase.Add(
                new Property
                {
                    UID = 1001,
                    Name = "Hotel Unit Test",
                    TimeZone_UID = null
                });
        }


        void FillRRDforTest()
        {
            _rateRoomDetailsInDatabase.Add(new Domain.Rates.RateRoomDetail()
            {
                UID = 10001,
                RateRoom_UID = 10001,
                Allotment = 10,
                AllotmentUsed = 0,
                CreateBy = 65,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifyBy = 65,
                ModifyDate = new DateTime(2015, 03, 03),

                Date = new DateTime(2015, 06, 01),
                Adult_1 = 110.99m,
                Adult_2 = 120.98m,
                Child_1 = 12.99m,
                Child_2 = 18.98m,
                ExtraBedPrice = 35.00m,

                ChannelsListUID = "1,2,30,72,220",
                BlockedChannelsListUID = "",
                isBookingEngineBlocked = false,

                ClosedOnArrival = false,
                ClosedOnDeparture = false,
                MaximumLengthOfStay = null,
                MinimumLengthOfStay = null,
                ReleaseDays = null,

                correlationID = Guid.NewGuid().ToString(),

                CancellationPolicy_UID = null,
                DepositPolicy_UID = null
            });
            _rateRoomDetailsInDatabase.Add(new Domain.Rates.RateRoomDetail()
            {
                UID = 10002,
                RateRoom_UID = 10001,
                Allotment = 10,
                AllotmentUsed = 0,
                CreateBy = 65,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifyBy = 65,
                ModifyDate = new DateTime(2015, 03, 03),

                Date = new DateTime(2015, 06, 02),
                Adult_1 = 110.99m,
                Adult_2 = 120.98m,
                Child_1 = 12.99m,
                Child_2 = 18.98m,
                ExtraBedPrice = 35.00m,

                ChannelsListUID = "1,2,30,72,220",
                BlockedChannelsListUID = "",
                isBookingEngineBlocked = false,

                ClosedOnArrival = false,
                ClosedOnDeparture = false,
                MaximumLengthOfStay = null,
                MinimumLengthOfStay = null,
                ReleaseDays = null,

                correlationID = Guid.NewGuid().ToString(),

                CancellationPolicy_UID = null,
                DepositPolicy_UID = null
            });
            _rateRoomDetailsInDatabase.Add(new Domain.Rates.RateRoomDetail()
            {
                UID = 10003,
                RateRoom_UID = 10001,
                Allotment = 10,
                AllotmentUsed = 0,
                CreateBy = 65,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifyBy = 65,
                ModifyDate = new DateTime(2015, 03, 03),

                Date = new DateTime(2015, 06, 03),
                Adult_1 = 110.99m,
                Adult_2 = 120.98m,
                Child_1 = 12.99m,
                Child_2 = 18.98m,
                ExtraBedPrice = 35.00m,

                ChannelsListUID = "1,2,30,72,220",
                BlockedChannelsListUID = "",
                isBookingEngineBlocked = false,

                ClosedOnArrival = false,
                ClosedOnDeparture = false,
                MaximumLengthOfStay = null,
                MinimumLengthOfStay = null,
                ReleaseDays = null,

                correlationID = Guid.NewGuid().ToString(),

                CancellationPolicy_UID = null,
                DepositPolicy_UID = null
            });
            _rateRoomDetailsInDatabase.Add(new Domain.Rates.RateRoomDetail()
            {
                UID = 10004,
                RateRoom_UID = 10001,
                Allotment = 10,
                AllotmentUsed = 0,
                CreateBy = 65,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifyBy = 65,
                ModifyDate = new DateTime(2015, 03, 03),

                Date = new DateTime(2015, 06, 04),
                Adult_1 = 110.99m,
                Adult_2 = 120.98m,
                Child_1 = 12.99m,
                Child_2 = 18.98m,
                ExtraBedPrice = 35.00m,

                ChannelsListUID = "1,2,30,72,220",
                BlockedChannelsListUID = "1,2,30",
                isBookingEngineBlocked = true,

                ClosedOnArrival = false,
                ClosedOnDeparture = false,
                MaximumLengthOfStay = null,
                MinimumLengthOfStay = null,
                ReleaseDays = null,

                correlationID = Guid.NewGuid().ToString(),

                CancellationPolicy_UID = null,
                DepositPolicy_UID = null
            });
            _rateRoomDetailsInDatabase.Add(new Domain.Rates.RateRoomDetail()
            {
                UID = 10005,
                RateRoom_UID = 10001,
                Allotment = 10,
                AllotmentUsed = 0,
                CreateBy = 65,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifyBy = 65,
                ModifyDate = new DateTime(2015, 03, 03),

                Date = new DateTime(2015, 06, 05),
                Adult_1 = 110.99m,
                Adult_2 = 120.98m,
                Child_1 = 12.99m,
                Child_2 = 18.98m,
                ExtraBedPrice = 35.00m,

                ChannelsListUID = "1,2,30,72,220",
                BlockedChannelsListUID = "1,2",
                isBookingEngineBlocked = true,

                ClosedOnArrival = false,
                ClosedOnDeparture = false,
                MaximumLengthOfStay = null,
                MinimumLengthOfStay = null,
                ReleaseDays = null,

                correlationID = Guid.NewGuid().ToString(),

                CancellationPolicy_UID = null,
                DepositPolicy_UID = null
            });

        }

        private void FillTheRatesforTest()
        {
            _ratesInDatabase.Add(
                new OB.Domain.Rates.Rate
                {
                    UID = 11001,
                    Property_UID = 1001,
                    Name = "1st Unit Test Rate",
                    Description = "Unit Testing Rate Description",
                    BeginSale = null,
                    EndSale = null,
                    RateCategory_UID = 1,
                    RateCategory = new OB.Domain.Rates.RateCategory()
                    {
                        UID = 1,
                        Name = "Regular/rack",
                        OTACodeUID = 1,
                        OTA_Codes = new OTA_Codes()
                        {
                            UID = 1,
                            OTACategoryUID = 1,
                            CodeName = "Regular/rack",
                            CodeValue = 13
                        }
                    },
                    Rate_UID = null,
                    CreatedDate = new DateTime(2015, 01, 01),
                    ModifiedDate = null,
                    CreatedBy = 65,
                    Currency_UID = 34,
                    IsActive = true,
                    IsDeleted = false,
                    RateRooms = new List<OB.Domain.Rates.RateRoom>()
                    {
                        new OB.Domain.Rates.RateRoom()
                        {
                            UID = 10001,
                            RoomType_UID = 2001,
                            Rate_UID = 11001,
                            IsDeleted = false
                        },
                        new OB.Domain.Rates.RateRoom()
                        {
                            UID = 10002,
                            RoomType_UID = 2002,
                            Rate_UID = 11001,
                            IsDeleted = true
                        },
                        new OB.Domain.Rates.RateRoom()
                        {
                            UID = 10003,
                            RoomType_UID = 2003,
                            Rate_UID = 11001,
                            IsDeleted = false
                        }
                    },
                    RatesExtras = new List<RatesExtra>()
                    {
                        new RatesExtra()
                        {
                            Rate_UID = 11001,
                            Extra_UID = 1,
                            IsIncluded = true,
                            IsDeleted = false,
                            Extra = new OB.Domain.Rates.Extra()
                            {
                                UID = 1,
                                Name = "Breakfast",
                                IsBoardType = true,
                                BoardType_UID = 1,
                                Property_UID = 1001
                            }
                        },
                        new RatesExtra()
                        {
                            Rate_UID = 11001,
                            Extra_UID = 2,
                            IsIncluded = true,
                            IsDeleted = true,
                            Extra = new OB.Domain.Rates.Extra()
                            {
                                UID = 2,
                                Name = "FullBoard",
                                IsBoardType = true,
                                BoardType_UID = 3,
                                Property_UID = 1001
                            }
                        },
                        new RatesExtra()
                        {
                            Rate_UID = 11001,
                            Extra_UID = 3,
                            IsIncluded = true,
                            IsDeleted = false,
                            Extra = new OB.Domain.Rates.Extra()
                            {
                                UID = 3,
                                Name = "Champagne",
                                IsBoardType = false,
                                BoardType_UID = null,
                                Value = 30,
                                Property_UID = 1001,
                                IsActive = true
                            }
                        }
                    }
                });
        }

        private void FillCurrenciesForTest()
        {
            _currenciesInDatabase.Add(new Currency()
            {
                UID = 34,
                Name = "Euro",
                Symbol = "EUR",
                CurrencySymbol = "€",
                DefaultPositionNumber = null,
                PaypalCurrencyCode = "98"
            });
            _currenciesInDatabase.Add(new Currency()
            {
                UID = 16,
                Name = "Brazil Real",
                Symbol = "BRL",
                CurrencySymbol = "R$",
                DefaultPositionNumber = 3,
                PaypalCurrencyCode = "22"
            });
            _currenciesInDatabase.Add(new Currency()
            {
                UID = 109,
                Name = "US Dollar",
                Symbol = "USD",
                CurrencySymbol = "$",
                DefaultPositionNumber = 2,
                PaypalCurrencyCode = "125"
            });
            _currenciesInDatabase.Add(new Currency()
            {
                UID = 108,
                Name = "UK Pound",
                Symbol = "GBP",
                CurrencySymbol = "£",
                DefaultPositionNumber = null,
                PaypalCurrencyCode = "161"
            });
        }

        private void FillTheRateRoomsforTest()
        {
            _rateRoomsInDatabase.Add(new OB.Domain.Rates.RateRoom()
            {
                UID = 10001,
                RoomType_UID = 2001,
                Rate_UID = 11001,
                IsDeleted = false
            });

            _rateRoomsInDatabase.Add(new OB.Domain.Rates.RateRoom()
            {
                UID = 10002,
                RoomType_UID = 2002,
                Rate_UID = 11001,
                IsDeleted = true
            });

            _rateRoomsInDatabase.Add(new OB.Domain.Rates.RateRoom()
            {
                UID = 10003,
                RoomType_UID = 2003,
                Rate_UID = 11001,
                IsDeleted = false
            });
        }

        private void FillTheRoomTypesforTest()
        {
            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 2001,
                Name = "1st Room Type",
                Description = "1st Room Type in this list, 1st Property, 2 Single Beds, Standard View",
                ShortDescription = "Room with 2 SGL Beds ",
                Property_UID = 1001,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 3,
                AcceptsExtraBed = true,
                AcceptsChildren = true,
                ChildMinOccupancy = 1,
                ChildMaxOccupancy = 2,
                MaxFreeChild = 1,
                MaxOccupancy = 2,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = null,
                MaxValue = 1000,
                MinValue = 50,
                Qty = 100,
                IsBase = true,
                IsDeleted = false,
            });
            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 2002,
                Name = "2nd Room Type",
                Description = "2nd Room Type in this list, 1st Property, 2 Single Beds, Standard View",
                ShortDescription = "Room with 2 SGL Beds ",
                Property_UID = 1001,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 3,
                AcceptsExtraBed = false,
                AcceptsChildren = true,
                ChildMinOccupancy = 1,
                ChildMaxOccupancy = 1,
                MaxFreeChild = 1,
                MaxOccupancy = 3,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = null,
                MaxValue = 1000,
                MinValue = 50,
                Qty = 100,
                IsBase = true,
                IsDeleted = false,
            });
            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 2003,
                Name = "3rd Deluxe Room Type",
                Description = "3rd Room Type in this list, 1st Property, 2 King Beds, Expectacular View",
                ShortDescription = "Deluxe Room with 2 King Beds ",
                Property_UID = 1001,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 4,
                AcceptsExtraBed = false,
                AcceptsChildren = true,
                ChildMinOccupancy = 1,
                ChildMaxOccupancy = 1,
                MaxFreeChild = 1,
                MaxOccupancy = 5,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = new DateTime(2015, 02, 01),
                MaxValue = 2000,
                MinValue = 150,
                Qty = 20,
                IsBase = true,
                IsDeleted = true,
            });
        }
        #endregion


        #region Test ListRateRoomDetails
        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_RateRoomDetails_ListRateRoomDetailsReturnOneDayTest()
        {
            FillRRDforTest();

            int totalRecords = -1;
            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 02)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 02)), It.IsAny<bool?>(), It.IsAny<List<long>>(), 
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015,06,02)));
            
            var response = RateRoomDetailsManagerPOCO.ListRateRoomDetails( new ListRateRoomDetailsRequest()
            {
                PageIndex = 1,
                RateRoom_UID = 10001,
                RRDFromDate = new DateTime(2015,06,02),
                RRDToDate = new DateTime(2015,06,02)
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateRoomDetailsResponse), "Expected Response Type = ListRateRoomDetailsResponse");
            Assert.IsTrue(response.Result.First().UID == 10002, "Expected UID = 10002");
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_RateRoomDetails_ListRateRoomDetailsReturnAll()
        {
            FillRRDforTest();

            int totalRecords = -1;
            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001));

            var response = RateRoomDetailsManagerPOCO.ListRateRoomDetails(new ListRateRoomDetailsRequest()
            {
                PageIndex = 1,
                RateRoom_UID = 10001,
                RRDFromDate = null,
                RRDToDate = null
            });

            //Response should contain only 5 result
            Assert.IsTrue(response.Result.Count == 5, "Expected result count = 5");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateRoomDetailsResponse), "Expected Response Type = ListRateRoomDetailsResponse");
            Assert.IsTrue(response.Result.First().UID == 10001, "Expected UID = 10001");
            Assert.IsTrue(response.Result.Last().UID == 10005, "Expected UID = 10005");
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_RateRoomDetails_ListRateRoomDetailsReturnAllOnlyCloseSales()
        {
            FillRRDforTest();

            int totalRecords = -1;
            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.Is<bool?>(arg => arg.HasValue && arg.Value), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && !string.IsNullOrWhiteSpace(x.BlockedChannelsListUID)));

            var response = RateRoomDetailsManagerPOCO.ListRateRoomDetails(new ListRateRoomDetailsRequest()
            {
                PageIndex = 1,
                RateRoom_UID = 10001,
                RRDFromDate = null,
                RRDToDate = null,
                IncludeOnlyCloseSales = true,
                
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateRoomDetailsResponse), "Expected Response Type = ListRateRoomDetailsResponse");
            Assert.IsTrue(response.Result.First().UID == 10004, "Expected UID = 10004");
            Assert.IsTrue(response.Result.Last().UID == 10005, "Expected UID = 10005");
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_RateRoomDetails_ListRateRoomDetailsReturnAllOnlyCloseSalesOneChannel()
        {
            FillRRDforTest();

            int totalRecords = -1;
            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.Is<bool?>(arg => arg.HasValue && arg.Value), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 30),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && (!string.IsNullOrWhiteSpace(x.BlockedChannelsListUID)) && x.BlockedChannelsListUID.Trim().Split(',').ToList().Contains("30")));

            var response = RateRoomDetailsManagerPOCO.ListRateRoomDetails(new ListRateRoomDetailsRequest()
            {
                PageIndex = 1,
                RateRoom_UID = 10001,
                RRDFromDate = null,
                RRDToDate = null,
                IncludeOnlyCloseSales = true,
                SelectedChannelList = new List<long>() { 30 }

            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateRoomDetailsResponse), "Expected Response Type = ListRateRoomDetailsResponse");
            Assert.IsTrue(response.Result.First().UID == 10004, "Expected UID = 10004");
        }
        

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_RateRoomDetails_ListRateRoomDetailsReturnAllOnlyCloseSalesOneChannelNoDetails()
        {
            FillRRDforTest();

            int totalRecords = 1;
            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 02)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 02)), It.IsAny<bool?>(), It.IsAny<List<long>>(), 
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015,06,02)));

            var response = RateRoomDetailsManagerPOCO.ListRateRoomDetails(new ListRateRoomDetailsRequest()
            {
                PageIndex = 1,
                RateRoom_UID = 10001,
                RRDFromDate = new DateTime(2015,06,02),
                RRDToDate = new DateTime(2015,06,02),
                IncludeOnlyCloseSales = true,
                SelectedChannelList = new List<long>() { 30 },
                ExcludeDetails = true
            });

            //Response should contain only 0 result
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");
            Assert.IsTrue(response.TotalRecords == 1, "Expected TotalRecords = 1");
            Assert.IsTrue(response.HasRRDForAllDays == true, "Expected HasRRDForAllDays = true");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateRoomDetailsResponse), "Expected Response Type = ListRateRoomDetailsResponse");

        }
        #endregion


        #region Test UpdateRateRoomDetailLight
        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_NoData()
        {
            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Please specify at least one Rate object"), "Expected Error = Please specify at least one Rate object");
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_NoProperty_UID()
        {
            FillRRDforTest();

            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                //Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                    new RateRoomDetailsLightDataWithChildRecordsCustom()
                    {
                         RateId = 11001,
                         RoomTypeId = 2001,
                         RRDFromDate = new DateTime(2050,06,01),
                         RRDToDate = new DateTime(2050,06,01),
                         IsMonday = true,
                         IsTuesday = true,
                         IsWednesday = true,
                         IsThursday = true,
                         IsFriday = true,
                         IsSaturday = true,
                         IsSunday = true,
                         Allotment = 10,
                         //NoOfAdultList = new List<int>(){ 0, 1, 2 },
                         //AdultPriceList = new List<decimal>() { 35, 110, 150 },
                         NoOfChildsList = new List<int>(){ 1, 2 },
                         ChildPriceList = new List<decimal>() { 15, 25 },
                         MinDays = 2,
                         MaxDays = null,
                         ReleaseDays = 1,
                         StayThrough = null,
                         IsClosedOnArr = false,
                         IsClosedOnDep = false,
                         IsBlocked = false,
                         SelectedChannelList = new List<long>(){ 1, 30, 279},
                         CreatedBy = 2020
                    }
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Please specify the Property_UID for this operation"), "Expected Error = Please specify the Property_UID for this operation");
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidIDs()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                    new RateRoomDetailsLightDataWithChildRecordsCustom()
                    {
                         UpdateId = "1",
                         //RateId = 11001,
                         //RoomTypeId = 2001,
                         CurrencyISO = "EUR",
                         RRDFromDate = new DateTime(2050,06,01),
                         RRDToDate = new DateTime(2050,06,01),
                         IsMonday = true,
                         IsTuesday = true,
                         IsWednesday = true,
                         IsThursday = true,
                         IsFriday = true,
                         IsSaturday = true,
                         IsSunday = true,
                         Allotment = 10,
                         NoOfAdultList = new List<int>(){ 0, 1, 2 },
                         AdultPriceList = new List<decimal>() { 35, 110, 150 },
                         NoOfChildsList = new List<int>(){ 1, 2 },
                         ChildPriceList = new List<decimal>() { 15, 25 },
                         MinDays = 2,
                         MaxDays = null,
                         ReleaseDays = 1,
                         StayThrough = null,
                         IsClosedOnArr = false,
                         IsClosedOnDep = false,
                         IsBlocked = false,
                         SelectedChannelList = new List<long>(){ 1, 30, 279},
                         CreatedBy = 2020
                    }
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateId or RoomTypeId, all of this fields must be filled"), "Expected Error = Invalid RateId or RoomTypeId, all of this fields must be filled");
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidUserID()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                    new RateRoomDetailsLightDataWithChildRecordsCustom()
                    {
                         UpdateId = "1",
                         RateId = 11001,
                         RoomTypeId = 2001,
                         CurrencyISO = "EUR",
                         RRDFromDate = new DateTime(2050,06,01),
                         RRDToDate = new DateTime(2050,06,01),
                         IsMonday = true,
                         IsTuesday = true,
                         IsWednesday = true,
                         IsThursday = true,
                         IsFriday = true,
                         IsSaturday = true,
                         IsSunday = true,
                         Allotment = 10,
                         NoOfAdultList = new List<int>(){ 0, 1, 2 },
                         AdultPriceList = new List<decimal>() { 35, 110, 150 },
                         NoOfChildsList = new List<int>(){ 1, 2 },
                         ChildPriceList = new List<decimal>() { 15, 25 },
                         MinDays = 2,
                         MaxDays = null,
                         ReleaseDays = 1,
                         StayThrough = null,
                         IsClosedOnArr = false,
                         IsClosedOnDep = false,
                         IsBlocked = false,
                         SelectedChannelList = new List<long>(){ 1, 30, 279},
                         //CreatedBy = 2020
                    }
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid or not filled User_UID in CreatedBy field."), "Expected Error = Invalid or not filled User_UID in CreatedBy field.");
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidRateID()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11002 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11002 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10002), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10002 && x.Date == new DateTime(2050, 06, 01)));

            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                    new RateRoomDetailsLightDataWithChildRecordsCustom()
                    {
                         UpdateId = "1",
                         RateId = 11002,
                         RoomTypeId = 2001,
                         CurrencyISO = "EUR",
                         RRDFromDate = new DateTime(2050,06,01),
                         RRDToDate = new DateTime(2050,06,01),
                         IsMonday = true,
                         IsTuesday = true,
                         IsWednesday = true,
                         IsThursday = true,
                         IsFriday = true,
                         IsSaturday = true,
                         IsSunday = true,
                         Allotment = 10,
                         NoOfAdultList = new List<int>(){ 0, 1, 2 },
                         AdultPriceList = new List<decimal>() { 35, 110, 150 },
                         NoOfChildsList = new List<int>(){ 1, 2 },
                         ChildPriceList = new List<decimal>() { 15, 25 },
                         MinDays = 2,
                         MaxDays = null,
                         ReleaseDays = 1,
                         StayThrough = null,
                         IsClosedOnArr = false,
                         IsClosedOnDep = false,
                         IsBlocked = false,
                         SelectedChannelList = new List<long>(){ 1, 30, 279},
                         CreatedBy = 2020
                    }
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateId."), "Expected Error = Invalid RateId.");
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidRoomID()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 3001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 3001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                    new RateRoomDetailsLightDataWithChildRecordsCustom()
                    {
                         UpdateId = "1",
                         RateId = 11001,
                         RoomTypeId = 3001,
                         CurrencyISO = "EUR",
                         RRDFromDate = new DateTime(2050,06,01),
                         RRDToDate = new DateTime(2050,06,01),
                         IsMonday = true,
                         IsTuesday = true,
                         IsWednesday = true,
                         IsThursday = true,
                         IsFriday = true,
                         IsSaturday = true,
                         IsSunday = true,
                         Allotment = 10,
                         NoOfAdultList = new List<int>(){ 0, 1, 2 },
                         AdultPriceList = new List<decimal>() { 35, 110, 150 },
                         NoOfChildsList = new List<int>(){ 1, 2 },
                         ChildPriceList = new List<decimal>() { 15, 25 },
                         MinDays = 2,
                         MaxDays = null,
                         ReleaseDays = 1,
                         StayThrough = null,
                         IsClosedOnArr = false,
                         IsClosedOnDep = false,
                         IsBlocked = false,
                         SelectedChannelList = new List<long>(){ 1, 30, 279},
                         CreatedBy = 2020
                    }
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RoomTypeId"), "Expected Error = Invalid RoomTypeId");
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidRateIDvsRoomID()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                    new RateRoomDetailsLightDataWithChildRecordsCustom()
                    {
                         UpdateId = "1",
                         RateId = 11002,
                         RoomTypeId = 2001,
                         CurrencyISO = "EUR",
                         RRDFromDate = new DateTime(2050,06,01),
                         RRDToDate = new DateTime(2050,06,01),
                         IsMonday = true,
                         IsTuesday = true,
                         IsWednesday = true,
                         IsThursday = true,
                         IsFriday = true,
                         IsSaturday = true,
                         IsSunday = true,
                         Allotment = 10,
                         NoOfAdultList = new List<int>(){ 0, 1, 2 },
                         AdultPriceList = new List<decimal>() { 35, 110, 150 },
                         NoOfChildsList = new List<int>(){ 1, 2 },
                         ChildPriceList = new List<decimal>() { 15, 25 },
                         MinDays = 2,
                         MaxDays = null,
                         ReleaseDays = 1,
                         StayThrough = null,
                         IsClosedOnArr = false,
                         IsClosedOnDep = false,
                         IsBlocked = false,
                         SelectedChannelList = new List<long>(){ 1, 30, 279},
                         CreatedBy = 2020
                    }
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateId"), "Expected Error = Invalid RateId");
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidRateIDvsRoomID1()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                    new RateRoomDetailsLightDataWithChildRecordsCustom()
                    {
                         UpdateId = "1",
                         RateId = 11001,
                         RoomTypeId = 2002,
                         CurrencyISO = "EUR",
                         RRDFromDate = new DateTime(2050,06,01),
                         RRDToDate = new DateTime(2050,06,01),
                         IsMonday = true,
                         IsTuesday = true,
                         IsWednesday = true,
                         IsThursday = true,
                         IsFriday = true,
                         IsSaturday = true,
                         IsSunday = true,
                         Allotment = 10,
                         NoOfAdultList = new List<int>(){ 0, 1, 2 },
                         AdultPriceList = new List<decimal>() { 35, 110, 150 },
                         NoOfChildsList = new List<int>(){ 1, 2 },
                         ChildPriceList = new List<decimal>() { 15, 25 },
                         MinDays = 2,
                         MaxDays = null,
                         ReleaseDays = 1,
                         StayThrough = null,
                         IsClosedOnArr = false,
                         IsClosedOnDep = false,
                         IsBlocked = false,
                         SelectedChannelList = new List<long>(){ 1, 30, 279},
                         CreatedBy = 2020
                    }
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RoomTypeId"), "Expected Error = Invalid RateRoomId");
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidCurrency()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
            {
                RequestGuid = new Guid(),
                Property_UID = 1001,
                RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                {
                    new RateRoomDetailsLightDataWithChildRecordsCustom()
                    {
                         UpdateId = "1",
                         RateId = 11001,
                         RoomTypeId = 2001,
                         CurrencyISO = "USD",
                         RRDFromDate = new DateTime(2050,06,01),
                         RRDToDate = new DateTime(2050,06,01),
                         IsMonday = true,
                         IsTuesday = true,
                         IsWednesday = true,
                         IsThursday = true,
                         IsFriday = true,
                         IsSaturday = true,
                         IsSunday = true,
                         Allotment = 10,
                         NoOfAdultList = new List<int>(){ 0, 1, 2 },
                         AdultPriceList = new List<decimal>() { 35, 110, 150 },
                         NoOfChildsList = new List<int>(){ 1, 2 },
                         ChildPriceList = new List<decimal>() { 15, 25 },
                         MinDays = 2,
                         MaxDays = null,
                         ReleaseDays = 1,
                         StayThrough = null,
                         IsClosedOnArr = false,
                         IsClosedOnDep = false,
                         IsBlocked = false,
                         SelectedChannelList = new List<long>(){ 1, 30, 279},
                         CreatedBy = 2020
                    }
                }
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid CurrencyISO."), "Expected Error = Invalid CurrencyISO.");
        }
        

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidDateFrom()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 4, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,03,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RRDFromDate, it's not filled or cannot be in past."), "Expected Error = Invalid RRDFromDate, it's not filled or cannot be in past.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidPeriod()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 4, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,03,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RRDToDate, it can not be previous to RRDFromDate."), "Expected Error = Invalid RRDToDate, it can not be previous to RRDFromDate.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidDateTo()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,01,01),
                             RRDToDate = new DateTime(2015,08,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Too large Date Period, Omnibees can only process updates within an maximum period of 6 months."), "Expected Error = Too large Date Period, Omnibees can only process updates within an maximum period of 6 months.");
            }
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidAdultsFill()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid AdultPriceList vs NoOfAdultList."), "Expected Error = Invalid AdultPriceList vs NoOfAdultList.");
            }
        }
        
        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_ExtraBedNegative()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { -35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid ExtraBedPrice, cannot be negative."), "Expected Error = Invalid ExtraBedPrice, cannot be negative.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestWarning_ExtraBedIgnored()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            _roomTypesInDatabase.First().AcceptsExtraBed = false;

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("Ignored ExtraBedPrice, this room cannot accept ExtraBed."), "Expected Warning = Ignored ExtraBedPrice, this room cannot accept ExtraBed.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestWarning_ExtraBedIgnored2()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            _roomTypesInDatabase.First().AcceptsExtraBed = null;

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("Ignored ExtraBedPrice, this room cannot accept ExtraBed."), "Expected Warning = Ignored ExtraBedPrice, this room cannot accept ExtraBed.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidAdultsConfiguration()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid AdultPrice configuration, some prices are missing."), "Expected Error = Invalid AdultPrice configuration, some prices are missing.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_AdultNegative()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 0, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid Adult price, must be greater than zero."), "Expected Error = Invalid Adult price, must be greater than zero.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestWarning_AllotmentNegative()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = -100,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("Omnibees cannot accept negative allotment, so instead the Allotment were set to 0."), "Expected Warning = Omnibees cannot accept negative allotment, so instead the Allotment were set to 0.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestWarning_AdultsIgnored()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3, 4 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190, 999 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");

                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("Ignored some AdultPrices outside the room occupancy configuration."), "Expected Warning = Ignored some AdultPrices outside the room occupancy configuration.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidChildsFill()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid ChildPriceList vs NoOfChildList."), "Expected Error = Invalid ChildPriceList vs NoOfChildList.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_InvalidChildsConfiguration()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            _roomTypesInDatabase.First().ChildMaxOccupancy = 3;

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 2 },
                             ChildPriceList = new List<decimal>() { 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid ChildPrice configuration."), "Expected Error = Invalid ChildPrice configuration.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestWarning_Only1Child()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            _roomTypesInDatabase.First().ChildMaxOccupancy = 3;

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1 },
                             ChildPriceList = new List<decimal>() { 15 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
                Assert.IsTrue(response.Warnings.Count == 2, "Expected Warnings count = 2");
                Assert.IsTrue(response.Warnings.First().Description.Contains("[UpdateId: 1] Price for Child2 where set based on requested price for Child1 multiplied per 2."), "Expected Warning = [UpdateId: 1] Price for Child2 where set based on requested price for Child1 multiplied per 2.");
                Assert.IsTrue(response.Warnings.Last().Description.Contains("[UpdateId: 1] Price for Child3 where set based on requested price for Child1 multiplied per 3."), "Expected Warning = [UpdateId: 1] Price for Child3 where set based on requested price for Child1 multiplied per 3.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_ChildNegative()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2},
                             ChildPriceList = new List<decimal>() { -15, -25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
                Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
                Assert.IsTrue(response.Errors.First().Description.Contains("Invalid Child price, cannot be negative."), "Expected Error = Invalid Child price, cannot be negative.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestWarning_ChildsNotAllowed()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2, 3 },
                             ChildPriceList = new List<decimal>() { 15, 25, 99 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("Ignored some ChildPrices outside the room occupancy configuration."), "Expected Warning = Ignored some ChildPrices outside the room occupancy configuration.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestWarning_ChildsIgnored()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            _roomTypesInDatabase.First().AcceptsChildren = false;

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("Ignored ChildPrices, this room cannot accept Childs."), "Expected Warning = Ignored ChildPrices, this room cannot accept Childs.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestWarning_ChildsIgnored2()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            _roomTypesInDatabase.First().AcceptsChildren = null;

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2050, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2050, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("Ignored ChildPrices, this room cannot accept Childs."), "Expected Warning = Ignored ChildPrices, this room cannot accept Childs.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_NoPartialUpd()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());
            

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 04, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 04, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 04, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,04,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             //NoOfChildsList = new List<int>(){ 1, 2 },
                             //ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.PartialSuccess, "Expected Status = PartialSuccess");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("[UpdateId: 1] It's a partial update and some dates could not be updated."), "Expected Warning = [UpdateId: 1] It's a partial update and some dates could not be updated.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_NoPartialUpd2()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 04, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 04, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 04, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,04,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             //NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             //AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.PartialSuccess, "Expected Status = PartialSuccess");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("[UpdateId: 1] It's a partial update and some dates could not be updated."), "Expected Warning = [UpdateId: 1] It's a partial update and some dates could not be updated.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_TestError_NoPartialUpd3()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 04, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 04, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 04, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,04,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             //Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.PartialSuccess, "Expected Status = PartialSuccess");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("[UpdateId: 1] It's a partial update and some dates could not be updated."), "Expected Warning = [UpdateId: 1] It's a partial update and some dates could not be updated.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_Test_AllowPartialUpd()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            int totalRecordsRRD = 1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecordsRRD, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                //Mock<IRateRoomDetailsManagerPOCO> _mockRRDpoco = new Mock<IRateRoomDetailsManagerPOCO>();
                //_mockRRDpoco.Setup(x => x.RateRoomDetailsDataInsertWithChildRecordsBatch(It.IsAny<List<RateRoomDetailsDataWithChildRecordsCustom>>(), It.IsAny<bool?>(), It.Is<long>(arg => arg == 1001), It.IsAny<string>())).Verifiable();

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,06,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             //Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.PartialSuccess, "Expected Status = PartialSuccess");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("[UpdateId: 1] It's a partial update and some dates could not be updated."), "Expected Warning = [UpdateId: 1] It's a partial update and some dates could not be updated.");

            }
        }
        

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_Test_AllowPartialUpdOnlyAvail()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            int totalRecordsRRD = 1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecordsRRD, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                //Mock<IRateRoomDetailsManagerPOCO> _mockRRDpoco = new Mock<IRateRoomDetailsManagerPOCO>();
                //_mockRRDpoco.Setup(x => x.RateRoomDetailsDataInsertWithChildRecordsBatch(It.IsAny<List<RateRoomDetailsDataWithChildRecordsCustom>>(), It.IsAny<bool?>(), It.Is<long>(arg => arg == 1001), It.IsAny<string>())).Verifiable();

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             RRDFromDate = new DateTime(2015,06,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             //CurrencyISO = "EUR",
                             //NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             //AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             //NoOfChildsList = new List<int>(){ 1, 2 },
                             //ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.PartialSuccess, "Expected Status = PartialSuccess");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("[UpdateId: 1] It's a partial update and some dates could not be updated."), "Expected Warning = [UpdateId: 1] It's a partial update and some dates could not be updated.");
            }
        }

        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_Test_AllowPartialUpdOnlyRateAmounts()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            int totalRecordsRRD = 1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecordsRRD, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                //Mock<IRateRoomDetailsManagerPOCO> _mockRRDpoco = new Mock<IRateRoomDetailsManagerPOCO>();
                //_mockRRDpoco.Setup(x => x.RateRoomDetailsDataInsertWithChildRecordsBatch(It.IsAny<List<RateRoomDetailsDataWithChildRecordsCustom>>(), It.IsAny<bool?>(), It.Is<long>(arg => arg == 1001), It.IsAny<string>())).Verifiable();

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             RRDFromDate = new DateTime(2015,06,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             //Allotment = 10,
                             CurrencyISO = "EUR",
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             //MinDays = 2,
                             //MaxDays = null,
                             //ReleaseDays = 1,
                             //StayThrough = null,
                             //IsClosedOnArr = false,
                             //IsClosedOnDep = false,
                             //IsBlocked = false,
                             //SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.PartialSuccess, "Expected Status = PartialSuccess");
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Warnings.First().Description.Contains("[UpdateId: 1] It's a partial update and some dates could not be updated."), "Expected Warning = [UpdateId: 1] It's a partial update and some dates could not be updated.");
            }
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_Test_FullUpd1()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            int totalRecordsRRD = 1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecordsRRD, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 06, 01)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,06,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            }
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_Test_FullUpd2()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            int totalRecordsRRD = 1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecordsRRD, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 04, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 04, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 04, 01)));



            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,04,01),
                             RRDToDate = new DateTime(2015,04,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            }
        }


        [TestMethod]
        [TestCategory("RateRoomDetails")]
        public void Test_UpdateRateRoomDetailsLight_Test_AllowMultipleUpdts()
        {
            FillRRDforTest();
            FillTheRatesforTest();
            FillCurrenciesForTest();
            FillTheRateRoomsforTest();
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            int totalRecordsRRD = 1;

            _appSettingRepoMock.Setup(x => x.FindSingleByName(It.IsAny<string>())).Returns(_appSettingInDataBase.FirstOrDefault());
            _propertyRepoMock.Setup(x => x.Get(It.IsAny<long>())).Returns(_propertyInDataBase.FirstOrDefault());

            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2001), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => x.UID == 2001 && x.Property_UID == 1001 && x.IsDeleted != true));

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyRateUIDs(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.Is<bool?>(arg => arg == true),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                           .Returns(_rateRoomsInDatabase.Where(x => x.Rate_UID == 11001 && x.IsDeleted != true));

            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecordsRRD, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 01)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 06, 01)));

            _rateRoomDetailsRepoMock.Setup(x => x.FindRateRoomDetailByCriteria(out totalRecordsRRD, It.IsAny<List<long>>(), It.Is<long>(arg => arg == 10001), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 02)), It.Is<DateTime?>(arg => arg.HasValue && arg.Value == new DateTime(2015, 06, 02)), It.IsAny<bool?>(), It.IsAny<List<long>>(),
    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_rateRoomDetailsInDatabase.Where(x => x.RateRoom_UID == 10001 && x.Date == new DateTime(2015, 06, 02)));

            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.UtcNowGet = () => new DateTime(2015, 1, 1);

                //Mock<IRateRoomDetailsManagerPOCO> _mockRRDpoco = new Mock<IRateRoomDetailsManagerPOCO>();
                //_mockRRDpoco.Setup(x => x.RateRoomDetailsDataInsertWithChildRecordsBatch(It.IsAny<List<RateRoomDetailsDataWithChildRecordsCustom>>(), It.IsAny<bool?>(), It.Is<long>(arg => arg == 1001), It.IsAny<string>())).Verifiable();

                var response = RateRoomDetailsManagerPOCO.UpdateRateRoomDetailsLight(new UpdateRateRoomDetailsLightRequest()
                {
                    RequestGuid = new Guid(),
                    Property_UID = 1001,
                    RateList = new List<RateRoomDetailsLightDataWithChildRecordsCustom>()
                    {
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,06,01),
                             RRDToDate = new DateTime(2015,06,01),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             //NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             //AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             //NoOfChildsList = new List<int>(){ 1, 2 },
                             //ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        },
                        new RateRoomDetailsLightDataWithChildRecordsCustom()
                        {
                             UpdateId = "1",
                             RateId = 11001,
                             RoomTypeId = 2001,
                             CurrencyISO = "EUR",
                             RRDFromDate = new DateTime(2015,06,02),
                             RRDToDate = new DateTime(2015,06,02),
                             IsMonday = true,
                             IsTuesday = true,
                             IsWednesday = true,
                             IsThursday = true,
                             IsFriday = true,
                             IsSaturday = true,
                             IsSunday = true,
                             Allotment = 10,
                             NoOfAdultList = new List<int>(){ 0, 1, 2, 3 },
                             AdultPriceList = new List<decimal>() { 35, 110, 150, 190 },
                             NoOfChildsList = new List<int>(){ 1, 2 },
                             ChildPriceList = new List<decimal>() { 15, 25 },
                             MinDays = 2,
                             MaxDays = null,
                             ReleaseDays = 1,
                             StayThrough = null,
                             IsClosedOnArr = false,
                             IsClosedOnDep = false,
                             IsBlocked = false,
                             SelectedChannelList = new List<long>(){ 1, 30, 279},
                             CreatedBy = 2020
                        }
                    }
                });

                //Assert Response Format
                Assert.IsTrue(response.Warnings.Count == 1, "Expected Warnings count = 1");
                Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
                Assert.IsTrue(response.Status == Status.PartialSuccess, "Expected Status = PartialSuccess");
            }
        }


        #endregion Test UpdateRateRoomDetailLight


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

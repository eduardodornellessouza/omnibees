using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Operations.Interfaces;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.Domain;
using OB.Domain.Rates;
using OB.Domain.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using OB.BL.Contracts.Responses;
using OB.BL.Contracts.Requests;
using OB.Domain.Properties;
using OB.DL.Common.Filter;
namespace OB.BL.Operations.Test.Rates
{
    [TestClass]
    public class RatesManagerPOCOTest : UnitBaseTest
    {
        public IRateManagerPOCO RateManagerPOCO
        {
            get;
            set;
        }

        private List<Property> _propertiesInDatabase = new List<Property>();
        private List<Rate> _ratesInDatabase = new List<Rate>();
        private List<Currency> _currenciesInDatabase = new List<Currency>();
        private List<RateCategory> _rateCategoriesInDatabase = new List<RateCategory>();
        private List<Extra> _extrasInDatabase = new List<Extra>();
        private List<BoardType> _boardTypesInDatabase = new List<BoardType>();
        private List<RateRoom> _rateRoomsInDatabase = new List<RateRoom>();
        private Mock<IPropertyRepository> _propertyRepoMock = null;
        private Mock<IRateRepository> _rateRepoMock = null;
        private Mock<ICurrencyRepository> _currencyRepoMock = null;
        private Mock<IRateCategoryRepository> _rateCatRepoMock = null;
        private Mock<IExtraRepository> _extraRepoMock = null;
        private Mock<IBoardTypeRepository> _boardTypeRepoMock = null;
        private Mock<IRateRoomRepository> _rateRoomRepoMock = null;

        private List<PropertyCurrency> _propertyCurrenciesInDatabase = new List<PropertyCurrency>();
        private Mock<IPropertyCurrencyRepository> _propertyCurrencyRepoMock = null;
        private List<RoomType> _roomTypesInDatabase = new List<RoomType>();
        private Mock<IRoomTypesRepository> _roomTypeRepoMock = null;


        private Mock<IRepository<ChangeLogDetail>> _changeLodDtlRepositoryFactoryMock = null;


        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _ratesInDatabase = new List<Rate>();

            //Mock RateRepository to return list of Properties instead of macking queries to the databse.
            _propertyRepoMock = new Mock<IPropertyRepository>(MockBehavior.Default);

            //Mock RateRepository to return list of Rates instead of macking queries to the databse.
            _rateRepoMock = new Mock<IRateRepository>(MockBehavior.Default);

            //Mock BoardTypeRepository to return list of BoardTypes instead of macking queries to the databse.
            _boardTypeRepoMock = new Mock<IBoardTypeRepository>(MockBehavior.Default);

            //Mock BoardTypeRepository to return list of RateRooms instead of macking queries to the databse.
            _rateRoomRepoMock = new Mock<IRateRoomRepository>(MockBehavior.Default);

            //Mock CurrencyRepository to return list of Currencies instead of macking queries to the databse.
            _currencyRepoMock = new Mock<ICurrencyRepository>(MockBehavior.Default);

            //Mock RateCategoryRepository to return list of Categories instead of macking queries to the databse.
            _rateCatRepoMock = new Mock<IRateCategoryRepository>(MockBehavior.Default);

            //Mock ExtraRepository to return list of Extras instead of macking queries to the databse.
            _extraRepoMock = new Mock<IExtraRepository>(MockBehavior.Default);

            //Mock IRepository<ChangeLogDetail>
            _changeLodDtlRepositoryFactoryMock = new Mock<IRepository<ChangeLogDetail>>(MockBehavior.Default);


            //Mock Repository factory to return mocked RateRepository
            var repoFactoryMock = new Mock<IRepositoryFactory>();


            repoFactoryMock.Setup(x => x.GetPropertyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRateRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<Rate>(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetBoardTypeRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_boardTypeRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetCurrencyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_currencyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRateRoomRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateRoomRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRateCategoryRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateCatRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetExtraRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_extraRepoMock.Object);


            //Mock RoomTypesRepository to return list of RoomTypes instead of macking queries to the databse.
            _roomTypeRepoMock = new Mock<IRoomTypesRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetRoomTypesRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_roomTypeRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<RoomType>(It.IsAny<IUnitOfWork>()))
                            .Returns(_roomTypeRepoMock.Object);

            //Mock Repository factory to return mocked PropertyCurrencyRepository
            _propertyCurrencyRepoMock = new Mock<IPropertyCurrencyRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetPropertyCurrencyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertyCurrencyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<PropertyCurrency>(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertyCurrencyRepoMock.Object);

            //Mock IRepository<ChangeLogDetail>
            repoFactoryMock.Setup(x => x.GetRepository<ChangeLogDetail>(It.IsAny<IUnitOfWork>()))
                            .Returns(_changeLodDtlRepositoryFactoryMock.Object);

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);

            this.Container = this.Container.RegisterInstance<IRepository<Rate>>(_rateRepoMock.Object, new TransientLifetimeManager());

            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.RateManagerPOCO = this.Container.Resolve<IRateManagerPOCO>();
        }


        #region Fill Testing Data
        private void FillTheRatesforTest()
        {
            _propertiesInDatabase.Add(
                new Property
                {
                    UID = 1001,
                    Name = "1st Unit Test Property",
                    BaseCurrency_UID = 34

                });
            _propertiesInDatabase.Add(
                new Property
                {
                    UID = 1002,
                    Name = "2nd Unit Test Property",
                    BaseCurrency_UID = 34

                });


            _ratesInDatabase.Add(
                new Rate
                {
                    UID = 11001,
                    Property_UID = 1001,
                    Name = "1st Unit Test Rate",
                    Description = "Unit Testing Rate Description",
                    BeginSale = null,
                    EndSale = null,
                    RateCategory_UID = 1,
                    RateCategory = new RateCategory()
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
                    RateRooms = new List<RateRoom>()
                    {
                        new RateRoom()
                        {
                            UID = 220001,
                            RoomType_UID = 10001,
                            Rate_UID = 11001,
                            IsDeleted = false
                        },
                        new RateRoom()
                        {
                            UID = 220002,
                            RoomType_UID = 10002,
                            Rate_UID = 11001,
                            IsDeleted = true
                        },
                        new RateRoom()
                        {
                            UID = 220003,
                            RoomType_UID = 10003,
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
                            Extra = new Extra()
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
                            Extra = new Extra()
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
                            Extra = new Extra()
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
            _ratesInDatabase.Add(
                new Rate
                {
                    UID = 11002,
                    Property_UID = 1001,
                    Name = "2nd Unit Test Rate",
                    Description = "Unit Testing Rate Description",
                    BeginSale = null,
                    EndSale = null,
                    RateCategory_UID = 2,
                    RateCategory = new RateCategory()
                    {
                        UID = 2,
                        Name = "Promotional",
                        OTACodeUID = 2,
                        OTA_Codes = new OTA_Codes()
                        {
                            UID = 2,
                            OTACategoryUID = 1,
                            CodeName = "Promotional",
                            CodeValue = 12
                        }
                    },
                    Rate_UID = null,
                    CreatedDate = new DateTime(2015, 01, 01),
                    ModifiedDate = null,
                    CreatedBy = 65,
                    Currency_UID = 108,
                    IsActive = false,
                    IsDeleted = true,
                    RateRooms = new List<RateRoom>()
                    {
                        new RateRoom()
                        {
                            UID = 220004,
                            RoomType_UID = 10001,
                            Rate_UID = 11002,
                            IsDeleted = false
                        },
                        new RateRoom()
                        {
                            UID = 220005,
                            RoomType_UID = 10002,
                            Rate_UID = 11002,
                            IsDeleted = true
                        },
                        new RateRoom()
                        {
                            UID = 220006,
                            RoomType_UID = 10003,
                            Rate_UID = 11002,
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
                            IsDeleted = true,
                            Extra = new Extra()
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
                            IsDeleted = false,
                            Extra = new Extra()
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
                            IsIncluded = false,
                            IsDeleted = false,
                            Extra = new Extra()
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
            _ratesInDatabase.Add(
                new Rate
                {
                    UID = 11003,
                    Property_UID = 1002,
                    Name = "1st Unit Test Rate",
                    Description = "Unit Testing Rate Description",
                    BeginSale = null,
                    EndSale = null,
                    RateCategory_UID = 1,
                    RateCategory = new RateCategory()
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
                    RateRooms = new List<RateRoom>()
                    {
                        new RateRoom()
                        {
                            UID = 220007,
                            RoomType_UID = 10004,
                            Rate_UID = 11003,
                            IsDeleted = false
                        },
                        new RateRoom()
                        {
                            UID = 220008,
                            RoomType_UID = 10005,
                            Rate_UID = 11003,
                            IsDeleted = true
                        },
                        new RateRoom()
                        {
                            UID = 220009,
                            RoomType_UID = 10006,
                            Rate_UID = 11003,
                            IsDeleted = false
                        }
                    }
                });
            _ratesInDatabase.Add(
                new Rate
                {
                    UID = 11004,
                    Property_UID = 1002,
                    Name = "2nd Unit Test Rate",
                    Description = "Unit Testing Rate Description",
                    BeginSale = null,
                    EndSale = null,
                    RateCategory_UID = 2,
                    RateCategory = new RateCategory()
                    {
                        UID = 2,
                        Name = "Promotional",
                        OTACodeUID = 2,
                        OTA_Codes = new OTA_Codes()
                        {
                            UID = 2,
                            OTACategoryUID = 1,
                            CodeName = "Promotional",
                            CodeValue = 12
                        }
                    },
                    Rate_UID = null,
                    CreatedDate = new DateTime(2015, 01, 01),
                    ModifiedDate = null,
                    CreatedBy = 65,
                    Currency_UID = 109,
                    IsActive = true,
                    IsDeleted = true,
                    RateRooms = new List<RateRoom>()
                    {
                        new RateRoom()
                        {
                            UID = 220010,
                            RoomType_UID = 10004,
                            Rate_UID = 11004,
                            IsDeleted = false
                        },
                        new RateRoom()
                        {
                            UID = 220011,
                            RoomType_UID = 10005,
                            Rate_UID = 11004,
                            IsDeleted = true
                        },
                        new RateRoom()
                        {
                            UID = 220012,
                            RoomType_UID = 10006,
                            Rate_UID = 11004,
                            IsDeleted = false
                        }
                    }
                });
        }

        private void FillTheCurrenciesforTest()
        {
            _currenciesInDatabase.Add(new Currency()
            {
                UID = 34,
                Symbol = "EUR"
            });
            _currenciesInDatabase.Add(new Currency()
            {
                UID = 108,
                Symbol = "GBP"
            });
            _currenciesInDatabase.Add(new Currency()
            {
                UID = 109,
                Symbol = "USD"
            });
        }

        private void FillTheRateCategoriesforTest()
        {
            _rateCategoriesInDatabase.Add(new RateCategory()
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
            });
            _rateCategoriesInDatabase.Add(new RateCategory()
            {
                UID = 2,
                Name = "Promotional",
                OTACodeUID = 2,
                OTA_Codes = new OTA_Codes()
                {
                    UID = 2,
                    OTACategoryUID = 1,
                    CodeName = "Promotional",
                    CodeValue = 12
                }
            });
        }

        private void FillTheExtrasforTest()
        {
            _extrasInDatabase.Add(new Extra()
            {
                UID = 1,
                Name = "Breakfast",
                IsBoardType = true,
                BoardType_UID = 1,
                Property_UID = 1001,
                CreatedDate = new DateTime(2015,01,01),
                Value = 10,
                IsDeleted = false
            });
            _extrasInDatabase.Add(new Extra()
            {
                UID = 2,
                Name = "FullBoard",
                IsBoardType = true,
                BoardType_UID = 3,
                Property_UID = 1001,
                CreatedDate = new DateTime(2015, 01, 01),
                Value = 20,
                IsDeleted = false
            });
            _extrasInDatabase.Add(new Extra()
            {
                UID = 3,
                Name = "Champagne",
                IsBoardType = false,
                BoardType_UID = null,
                Property_UID = 1001,
                IsActive = true,
                CreatedDate = new DateTime(2015, 01, 01),
                Value = 30,
                IsDeleted = false
            });
            _extrasInDatabase.Add(new Extra()
            {
                UID = 4,
                Name = "Breakfast",
                IsBoardType = true,
                BoardType_UID = 1,
                Property_UID = 1002,
                CreatedDate = new DateTime(2015, 01, 01),
                Value = 10,
                IsDeleted = false
            });
            _extrasInDatabase.Add(new Extra()
            {
                UID = 5,
                Name = "FullBoard",
                IsBoardType = true,
                BoardType_UID = 3,
                Property_UID = 1002,
                CreatedDate = new DateTime(2015, 01, 01),
                Value = 20,
                IsDeleted = false
            });
        }

        private void FillTheBoardTypesforTest()
        {
            _boardTypesInDatabase.Add(new BoardType()
                {
                    UID = 1,
                    Name = "Breakfast"
                });
            _boardTypesInDatabase.Add(new BoardType()
                {
                    UID = 2,
                    Name = "Half Board"
                });
            _boardTypesInDatabase.Add(new BoardType()
                {
                    UID = 3,
                    Name = "Full Board"
                });
            _boardTypesInDatabase.Add(new BoardType()
                {
                    UID = 4,
                    Name = "All Inclusive"
                });
        }

        private void FillTheRateRoomsforTest()
        {
             _rateRoomsInDatabase.Add( new RateRoom()
                {
                    UID = 220001,
                    RoomType_UID = 10001,
                    Rate_UID = 11001,
                    IsDeleted = false
                });

             _rateRoomsInDatabase.Add( new RateRoom()
                {
                    UID = 220002,
                    RoomType_UID = 10002,
                    Rate_UID = 11001,
                    IsDeleted = true
                });

             _rateRoomsInDatabase.Add(new RateRoom()
                {
                    UID = 220003,
                    RoomType_UID = 10003,
                    Rate_UID = 11001,
                    IsDeleted = false
                });

             _rateRoomsInDatabase.Add(new RateRoom()
                 {
                     UID = 220004,
                     RoomType_UID = 10001,
                     Rate_UID = 11002,
                     IsDeleted = false
                 });
            
             _rateRoomsInDatabase.Add( new RateRoom()
                 {
                     UID = 220005,
                     RoomType_UID = 10002,
                     Rate_UID = 11002,
                     IsDeleted = true
                 });

             _rateRoomsInDatabase.Add( new RateRoom()
                 {
                     UID = 220006,
                     RoomType_UID = 10003,
                     Rate_UID = 11002,
                     IsDeleted = false
                 });
        }

        private void FillTheRoomTypesforTest()
        {
            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 10001,
                Name = "1st Room Type",
                Description = "1st Room Type in this list, 1st Property, 2 Single Beds, Standard View",
                ShortDescription = "Room with 2 SGL Beds ",
                Property_UID = 1001,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 3,
                AcceptsExtraBed = false,
                AcceptsChildren = true,
                ChildMinOccupancy = null,
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
                UID = 10002,
                Name = "2nd Room Type",
                Description = "2nd Room Type in this list, 1st Property, 2 Single Beds, Standard View",
                ShortDescription = "Room with 2 SGL Beds ",
                Property_UID = 1001,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 3,
                AcceptsExtraBed = false,
                AcceptsChildren = true,
                ChildMinOccupancy = null,
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
                UID = 10003,
                Name = "3rd Deluxe Room Type",
                Description = "3rd Room Type in this list, 1st Property, 2 King Beds, Expectacular View",
                ShortDescription = "Deluxe Room with 2 King Beds ",
                Property_UID = 1001,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 4,
                AcceptsExtraBed = false,
                AcceptsChildren = true,
                ChildMinOccupancy = null,
                ChildMaxOccupancy = 2,
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


            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 10011,
                Name = "1st Room Type",
                Description = "1st Room Type in this list, 2nd Property, 2 Single Beds, Standard View",
                ShortDescription = "Room with 2 SGL Beds ",
                Property_UID = 1002,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 3,
                AcceptsExtraBed = false,
                AcceptsChildren = true,
                ChildMinOccupancy = null,
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
                UID = 10012,
                Name = "2nd Room Type",
                Description = "2nd Room Type in this list, 2nd Property, 2 Single Beds, Standard View",
                ShortDescription = "Room with 2 SGL Beds ",
                Property_UID = 1002,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 3,
                AcceptsExtraBed = false,
                AcceptsChildren = true,
                ChildMinOccupancy = null,
                ChildMaxOccupancy = 1,
                MaxFreeChild = 1,
                MaxOccupancy = 3,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = null,
                MaxValue = 1000,
                MinValue = 50,
                Qty = 100,
                IsBase = true,
                IsDeleted = true,
            });
            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 10013,
                Name = "3rd Deluxe Room Type",
                Description = "3rd Room Type in this list, 2nd Property, 2 King Beds, Expectacular View",
                ShortDescription = "Deluxe Room with 2 King Beds ",
                Property_UID = 1002,
                AdultMinOccupancy = 1,
                AdultMaxOccupancy = 4,
                AcceptsExtraBed = false,
                AcceptsChildren = true,
                ChildMinOccupancy = null,
                ChildMaxOccupancy = 2,
                MaxFreeChild = 1,
                MaxOccupancy = 5,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = new DateTime(2015, 02, 01),
                MaxValue = 2000,
                MinValue = 150,
                Qty = 20,
                IsBase = true,
                IsDeleted = false,
            });

        }

        private void FillThePropertyCurrencyforTest()
        {
            _propertyCurrenciesInDatabase.Add(new PropertyCurrency()
            {
                UID = 10002,
                Property_UID = 1001,
                Currency_UID = 108,
                ExchangeRate = 0.7m,
                IsActive = true,
            });
            _propertyCurrenciesInDatabase.Add(new PropertyCurrency()
            {
                UID = 10003,
                Property_UID = 1001,
                Currency_UID = 109,
                IsAutomaticExchangeRate = true,
                IsActive = true,
            });
            _propertyCurrenciesInDatabase.Add(new PropertyCurrency()
            {
                UID = 10005,
                Property_UID = 1002,
                Currency_UID = 109,
                IsAutomaticExchangeRate = true,
                IsActive = false,
            });

        }
        #endregion


        #region Test ListRatesLight
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRatesByUIDReturnOneRateTest()
        {
            FillTheRatesforTest();
            FillTheCurrenciesforTest();
            //FillTheOTACategoriesforTest();
            FillTheBoardTypesforTest();

            int totalRecords = -1;
            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 11001), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                            .Returns(_ratesInDatabase.Where(x => x.UID == 11001 && x.Property_UID == 1001));

            _boardTypeRepoMock.Setup(x => x.GetAll())
                            .Returns(_boardTypesInDatabase);

            _currencyRepoMock.Setup(x => x.GetAll())
                            .Returns(_currenciesInDatabase);

            //_rateOTACodeCatRepoMock.Setup(x => x.FindRateCategoryOTACodes(It.Is<List<int?>>(arg => arg.Count == 1 && arg.First() == 1)))
            //                .Returns(_otaCategoriesInDatabase);

            var response = RateManagerPOCO.ListRatesLight(new Contracts.Requests.ListRateLightRequest
            {
                PageIndex = 1,
                UIDs = new List<long> { 11001 },
                PropertyUIDs = new List<long> { 1001 },
                ExcludeDeleteds = null,
                IncludeRateRooms = false,
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //All Response elements should not contain any RateRoomsLight results
            Assert.IsTrue(response.Result.Where(x => x.RateRoomsLight == null).Count() == 1, "Expected result for RateRooms == null, count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateLightResponse), "Expected Response Type = ListRateLightResponse");
            Assert.IsTrue(response.Result.First().UID == 11001, "Expected UID = 11001");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRatesByUIDReturnOnePropertyTest()
        {
            FillTheRatesforTest();
            FillTheCurrenciesforTest();
            //FillTheOTACategoriesforTest();
            FillTheBoardTypesforTest();

            int totalRecords = -1;
            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1001), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                            .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001));

            _boardTypeRepoMock.Setup(x => x.GetAll())
                            .Returns(_boardTypesInDatabase);

            _currencyRepoMock.Setup(x => x.GetAll())
                            .Returns(_currenciesInDatabase);

            //_rateOTACodeCatRepoMock.Setup(x => x.FindRateCategoryOTACodes(It.IsAny<List<int?>>()))
            //                .Returns(_otaCategoriesInDatabase);

            var response = RateManagerPOCO.ListRatesLight(new Contracts.Requests.ListRateLightRequest
            {
                PageIndex = 1,
                UIDs = new List<long> { },
                PropertyUIDs = new List<long> { 1001 },
                ExcludeDeleteds = null,
                IncludeRateRooms = false,
            });

            //Response should contain only 2 results
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //All Response elements should not contain any RateRoomsLight results
            Assert.IsTrue(response.Result.Where(x => x.RateRoomsLight == null).Count() == 2, "Expected result for RateRooms == null, count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateLightResponse), "Expected Response Type = ListRateLightResponse");
            Assert.IsTrue(response.Result.First().UID == 11001, "Expected UID = 11001");
            Assert.IsTrue(response.Result.Last().UID == 11002, "Expected UID = 11002");
            Assert.IsTrue(response.Result.First().RateRoomsLight == null, "Expected RateRoomsLight = null");
            Assert.IsTrue(response.Result.Last().RateRoomsLight == null, "Expected RateRoomsLight = null");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRatesByUIDReturnAllPropertyTest()
        {
            FillTheRatesforTest();
            FillTheCurrenciesforTest();
            //FillTheOTACategoriesforTest();
            FillTheBoardTypesforTest();

            int totalRecords = -1;
            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1001 && arg.Last() == 1002), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                            .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 || x.Property_UID == 1002));

            _boardTypeRepoMock.Setup(x => x.GetAll())
                            .Returns(_boardTypesInDatabase);

            _currencyRepoMock.Setup(x => x.GetAll())
                            .Returns(_currenciesInDatabase);

            //_rateOTACodeCatRepoMock.Setup(x => x.FindRateCategoryOTACodes(It.IsAny<List<int?>>()))
            //                .Returns(_otaCategoriesInDatabase);

            var response = RateManagerPOCO.ListRatesLight(new Contracts.Requests.ListRateLightRequest
            {
                PageIndex = 1,
                UIDs = null,
                PropertyUIDs = new List<long> { 1001, 1002 },
                ExcludeDeleteds = null,
                IncludeRateRooms = false,
            });

            //Response should contain only 4 results
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");

            //All Response elements should not contain any RateRoomsLight results
            Assert.IsTrue(response.Result.Where(x => x.RateRoomsLight == null).Count() == 4, "Expected result for RateRooms == null, count = 4");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateLightResponse), "Expected Response Type = ListRateLightResponse");
            Assert.IsTrue(response.Result.First().UID == 11001, "Expected UID = 11001");
            Assert.IsTrue(response.Result.Last().UID == 11004, "Expected UID = 11004");
            Assert.IsTrue(response.Result.First().RateRoomsLight == null, "Expected RateRoomsLight = null");
            Assert.IsTrue(response.Result.Last().RateRoomsLight == null, "Expected RateRoomsLight = null");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRatesByUIDReturnAllPropertyTestRateRooms()
        {
            FillTheRatesforTest();
            FillTheCurrenciesforTest();
            //FillTheOTACategoriesforTest();
            FillTheBoardTypesforTest();

            int totalRecords = -1;
            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1001 && arg.Last() == 1002), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                            .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 || x.Property_UID == 1002));

            _boardTypeRepoMock.Setup(x => x.GetAll())
                            .Returns(_boardTypesInDatabase);

            _currencyRepoMock.Setup(x => x.GetAll())
                            .Returns(_currenciesInDatabase);

            //_rateOTACodeCatRepoMock.Setup(x => x.FindRateCategoryOTACodes(It.IsAny<List<int?>>()))
            //                .Returns(_otaCategoriesInDatabase);

            var response = RateManagerPOCO.ListRatesLight(new Contracts.Requests.ListRateLightRequest
            {
                PageIndex = 1,
                UIDs = null,
                PropertyUIDs = new List<long> { 1001, 1002 },
                ExcludeDeleteds = null,
                IncludeRateRooms = true,
            });

            //Response should contain only 4 results
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");

            //All Response elements should contain RateRoomsLight results
            Assert.IsTrue(response.Result.Where(x => x.RateRoomsLight.Count() > 0).Count() == 4, "Expected result for RateRooms>0 count = 4");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateLightResponse), "Expected Response Type = ListRateLightResponse");
            Assert.IsTrue(response.Result.First().UID == 11001, "Expected UID = 11001");
            Assert.IsTrue(response.Result.Last().UID == 11004, "Expected UID = 11004");
            Assert.IsTrue(response.Result.First().RateRoomsLight != null, "Expected RateRoomsLight != null");
            Assert.IsTrue(response.Result.Last().RateRoomsLight != null, "Expected RateRoomsLight != null");
            Assert.IsTrue(response.Result.Last().RateRoomsLight != null, "Expected RateRoomsLight != null");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Count() == 3, "Expected RateRoomsLight.Count() == 3");
            Assert.IsTrue(response.Result.Last().RateRoomsLight.Count() == 3, "Expected RateRoomsLight.Count() == 3");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRatesByUIDReturnAllPropertyTestRateRoomsWithoutDeleteds()
        {
            FillTheRatesforTest();
            FillTheCurrenciesforTest();
            //FillTheOTACategoriesforTest();
            FillTheBoardTypesforTest();

            int totalRecords = -1;
            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1001 && arg.Last() == 1002), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                            .Returns(_ratesInDatabase.Where(x => (x.Property_UID == 1001 || x.Property_UID == 1002) && x.IsDeleted != true));

            _boardTypeRepoMock.Setup(x => x.GetAll())
                            .Returns(_boardTypesInDatabase);

            _currencyRepoMock.Setup(x => x.GetAll())
                            .Returns(_currenciesInDatabase);

            //_rateOTACodeCatRepoMock.Setup(x => x.FindRateCategoryOTACodes(It.IsAny<List<int?>>()))
            //                .Returns(_otaCategoriesInDatabase);

            var response = RateManagerPOCO.ListRatesLight(new Contracts.Requests.ListRateLightRequest
            {
                PageIndex = 1,
                UIDs = null,
                PropertyUIDs = new List<long> { 1001, 1002 },
                ExcludeDeleteds = true,
                IncludeRateRooms = true,
            });

            //Response should contain only 2 results
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //All Response elements should contain RateRoomsLight results
            Assert.IsTrue(response.Result.Where(x => x.RateRoomsLight.Count() > 0).Count() == 2, "Expected result for RateRooms>0 count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateLightResponse), "Expected Response Type = ListRateLightResponse");
            Assert.IsTrue(response.Result.First().UID == 11001, "Expected UID = 11001");
            Assert.IsTrue(response.Result.Last().UID == 11003, "Expected UID = 11003");
            Assert.IsTrue(response.Result.First().RateRoomsLight != null, "Expected RateRoomsLight != null");
            Assert.IsTrue(response.Result.Last().RateRoomsLight != null, "Expected RateRoomsLight != null");
            Assert.IsTrue(response.Result.Last().RateRoomsLight != null, "Expected RateRoomsLight != null");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Count() == 2, "Expected RateRoomsLight.Count() == 2");
            Assert.IsTrue(response.Result.Last().RateRoomsLight.Count() == 2, "Expected RateRoomsLight.Count() == 2");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRatesByUIDReturnAllPropertyTestWithoutDeleteds()
        {
            FillTheRatesforTest();
            FillTheCurrenciesforTest();
            //FillTheOTACategoriesforTest();
            FillTheBoardTypesforTest();

            int totalRecords = -1;
            _rateRepoMock.Setup(x => x.FindRateLightByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1001 && arg.Last() == 1002), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<FilterByInfo>>(), It.IsAny<List<SortByInfo>>(), It.IsAny<bool?>()))
                            .Returns(_ratesInDatabase.Where(x => (x.Property_UID == 1001 || x.Property_UID == 1002) && x.IsDeleted != true));

            _boardTypeRepoMock.Setup(x => x.GetAll())
                            .Returns(_boardTypesInDatabase);

            _currencyRepoMock.Setup(x => x.GetAll())
                            .Returns(_currenciesInDatabase);

            //_rateOTACodeCatRepoMock.Setup(x => x.FindRateCategoryOTACodes(It.IsAny<List<int?>>()))
            //                .Returns(_otaCategoriesInDatabase);

            var response = RateManagerPOCO.ListRatesLight(new Contracts.Requests.ListRateLightRequest
            {
                PageIndex = 1,
                UIDs = null,
                PropertyUIDs = new List<long> { 1001, 1002 },
                ExcludeDeleteds = true,
                IncludeRateRooms = false,
            });

            //Response should contain only 2 results
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //All Response elements should not contain any RateRoomsLight results
            Assert.IsTrue(response.Result.Where(x => x.RateRoomsLight == null).Count() == 2, "Expected result for RateRooms == null, count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateLightResponse), "Expected Response Type = ListRateLightResponse");
            Assert.IsTrue(response.Result.First().UID == 11001, "Expected UID = 11001");
            Assert.IsTrue(response.Result.Last().UID == 11003, "Expected UID = 11003");
            Assert.IsTrue(response.Result.First().RateRoomsLight == null, "Expected RateRoomsLight = null");
            Assert.IsTrue(response.Result.Last().RateRoomsLight == null, "Expected RateRoomsLight = null");

        }
        #endregion Test ListRatesLight


        #region Test ListBoardTypes
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListBoardTypesAllTest()
        {
            FillTheBoardTypesforTest();

            int totalRecords = -1;

            _boardTypeRepoMock.Setup(x => x.FindBoardTypeByCriteria(out totalRecords, It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_boardTypesInDatabase);

            var response = RateManagerPOCO.ListBoardTypes(new Contracts.Requests.GenericListPagedRequest
            {
                PageIndex = 1,
            });

            //Response should contain only 4 results
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListBoardTypeResponse), "Expected Response Type = ListBoardTypeResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 4, "Expected UID = 4");
            Assert.IsTrue(response.Result.First().Name == "Breakfast", "Expected Name = Breakfast");
            Assert.IsTrue(response.Result.Last().Name == "All Inclusive", "Expected Name = All Inclusive");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListBoardTypesByUIDTest()
        {
            FillTheBoardTypesforTest();

            int totalRecords = -1;

            _boardTypeRepoMock.Setup(x => x.FindBoardTypeByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 1 && arg.Last() == 3),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_boardTypesInDatabase.Where(x => x.UID == 1 || x.UID == 3));

            var response = RateManagerPOCO.ListBoardTypes(new Contracts.Requests.GenericListPagedRequest
            {
                UIDs = new List<long>()
                {
                    1,
                    3
                },
                PageIndex = 1,
            });

            //Response should contain only 2 results
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListBoardTypeResponse), "Expected Response Type = ListBoardTypeResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 3, "Expected UID = 3");
            Assert.IsTrue(response.Result.First().Name == "Breakfast", "Expected Name = Breakfast");
            Assert.IsTrue(response.Result.Last().Name == "Full Board", "Expected Name = Full Board");
        }
        #endregion Test ListBoardTypes
        

        #region Test ListRateRooms
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRateRoomsAllTest()
        {
            FillTheRateRoomsforTest();

            int totalRecords = -1;

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomsInDatabase);

            var response = RateManagerPOCO.ListRateRooms(new ListRateRoomsRequest()
                {
                    ExcludeDeleteds = false,
                    PageIndex = 1
                });

            //Response should contain only 6 results
            Assert.IsTrue(response.Result.Count == 6, "Expected result count = 6");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateRoomsResponse), "Expected Response Type = ListRateRoomsResponse");
            Assert.IsTrue(response.Result.First().UID == 220001, "Expected first UID = 220001");
            Assert.IsTrue(response.Result.Last().UID == 220006, "Expected last UID = 220006");
            Assert.IsTrue(response.Result.First().Rate_UID == 11001, "Expected first RateUID = 11001");
            Assert.IsTrue(response.Result.Last().Rate_UID == 11002, "Expected last RateUID = 11002");
            Assert.IsTrue(response.Result.First().RoomType_UID == 10001, "Expected first RoomTypeUID = 10001");
            Assert.IsTrue(response.Result.Last().RoomType_UID == 10003, "Expected last RoomTypeUID = 10003");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRateRoomsByRateUIDWithoutDeleteds()
        {
            FillTheRateRoomsforTest();

            int totalRecords = -1;

            _rateRoomRepoMock.Setup(x => x.FindRateRoomsbyCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.IsAny<List<long>>(), It.Is<bool?>(arg => arg == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateRoomsInDatabase.Where(x => x.IsDeleted == false && x.Rate_UID == 11002));

            var response = RateManagerPOCO.ListRateRooms(new ListRateRoomsRequest()
            {
                ExcludeDeleteds = true,
                RateUIDs = new List<long>(){ 11002 },
                PageIndex = 1
            });

            //Response should contain only 2 results
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateRoomsResponse), "Expected Response Type = ListRateRoomsResponse");
            Assert.IsTrue(response.Result.First().UID == 220004, "Expected first UID = 220004");
            Assert.IsTrue(response.Result.Last().UID == 220006, "Expected last UID = 220006");
            Assert.IsTrue(response.Result.First().Rate_UID == 11002, "Expected first RateUID = 11002");
            Assert.IsTrue(response.Result.Last().Rate_UID == 11002, "Expected last RateUID = 11002");
            Assert.IsTrue(response.Result.First().RoomType_UID == 10001, "Expected first RoomTypeUID = 10001");
            Assert.IsTrue(response.Result.Last().RoomType_UID == 10003, "Expected last RoomTypeUID = 10003");
        }
        #endregion Test ListRateRooms


        #region Test ListExtras
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListExtrasAllTest()
        {
            FillTheExtrasforTest();

            int totalRecords = -1;

            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_extrasInDatabase);

            var response = RateManagerPOCO.ListExtras(new Contracts.Requests.ListExtraRequest
            {
                PageIndex = 1
            });

            //Response should contain only 5 results
            Assert.IsTrue(response.Result.Count == 5, "Expected result count = 5");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListExtraResponse), "Expected Response Type = ListExtraResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 5, "Expected UID = 5");
            Assert.IsTrue(response.Result.First().Name == "Breakfast", "Expected Name = Breakfast");
            Assert.IsTrue(response.Result.Last().Name == "FullBoard", "Expected Name = FullBoard");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListExtrasByPropertyTest()
        {
            FillTheExtrasforTest();

            int totalRecords = -1;

            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001));

            var response = RateManagerPOCO.ListExtras(new Contracts.Requests.ListExtraRequest
            {
                PageIndex = 1,
                PropertyUIDs = new List<long>() { 1001 }
            });

            //Response should contain only 3 results
            Assert.IsTrue(response.Result.Count == 3, "Expected result count = 3");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListExtraResponse), "Expected Response Type = ListExtraResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 3, "Expected UID = 3");
            Assert.IsTrue(response.Result.First().Name == "Breakfast", "Expected Name = Breakfast");
            Assert.IsTrue(response.Result.Last().Name == "Champagne", "Expected Name = Champagne");
        }
        #endregion Test ListExtras


        #region Test ListRateCategories
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_ListRateCategoriesAllTest()
        {
            FillTheRateCategoriesforTest();

            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(), 
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_rateCategoriesInDatabase);

            var response = RateManagerPOCO.ListRateCategories(new Contracts.Requests.GenericListPagedRequest
            {
                PageIndex = 1
            });

            //Response should contain only 2 results
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRateCategoryResponse), "Expected Response Type = ListRateCategoryResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 2, "Expected UID = 2");
        }
        #endregion Test ListRateCategories


        #region Test CreateRatesLight
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_Success()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();         

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020,
                ReturnTotal = false
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_1"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "TestRate_1"));

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1001).SingleOrDefault());

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1001));


            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(), 
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));
                                

            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Result.First().Name == "TestRate_1", "Expected Rate Name = TestRate_1");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Count() == 2 , "Expected RateRooms count = 2");

            Assert.IsTrue(response.TotalRecords == -1, "Expected TotalRecords count = -1");
        }


        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_UserError()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                //UserUID = 2020
            };

            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("required to fill the UserUID"), "Expected Error = required to fill the UserUID");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_UIDError()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 1,
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020
            };

            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("the UID in Rates creations must be = 0"), "Expected Error = the UID in Rates creations must be = 0");
        }
        
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_RequestError()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 0,
                        //Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        //Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020
            };

            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("some of the required parameter(s) have not an valid value for this Rate"), "Expected Error = some of the required parameter(s) have not an valid value for this Rate");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_InvalidUID()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 1425,
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020
            };

            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("Cannot process this request, the UID in Rates creations must be = 0."), "Expected Error = Cannot process this request, the UID in Rates creations must be = 0.");
        }
        
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_DuplicatedRoomType()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 0,
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                        }
                    }
                
                },
                UserUID = 2020
            };

            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_1"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "TestRate_1"));

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1001).SingleOrDefault());

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1001));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateRoomsLight with duplicated RoomType_UID(s)"), "Expected Error = Invalid RateRoomsLight with duplicated RoomType_UID(s)");
        }


        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_InvalidRoomType()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 0,
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10003,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020
            };

            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_1"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "TestRate_1"));

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1001).SingleOrDefault());

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1001));


            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 3 && arg.First() == 10001 && arg.Last() == 10003), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002 || x.UID == 10003) && x.Property_UID == 1001));

            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateRoomsLight with deleted RoomType_UID(s) for this property"), "Expected Error = Invalid RateRoomsLight with deleted RoomType_UID(s) for this property");
        }
        

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_TwoRates_Success()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1002,
                        Name = "TestRate_2",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = null,
                        EndSale = null,
                        IsActive = true,
                        BoardType_UID = null,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10011,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10012,
                                IsDeleted = true
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10013,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020,
                ReturnTotal = true
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_1"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "TestRate_1"));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_2"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1002 && x.Name == "TestRate_2"));

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 11111).SingleOrDefault());

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1002).SingleOrDefault());


            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1001));

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1002));


            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 3 && arg.First() == 10011 && arg.Last() == 10013), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10011 || x.UID == 10012 || x.UID == 10013) && x.Property_UID == 1002));


            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 2 result(s)
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Result.First().Name == "TestRate_1", "Expected Rate Name = TestRate_1");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Count() == 2, "Expected RateRooms count = 2");
            Assert.IsTrue(response.Result.Last().Name == "TestRate_2", "Expected Rate Name = TestRate_1");
            Assert.IsTrue(response.Result.Last().RateRoomsLight.Count() == 3, "Expected RateRooms count = 3");

            Assert.IsTrue(response.TotalRecords == 2, "Expected TotalRecords count = 2");
        }



        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_InvalidProperty()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 11111,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11111), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_1"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 11111 && x.Name == "TestRate_1"));

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 11111).SingleOrDefault());

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11111), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 11111));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = 1");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid Property_UID"), "Expected Error = Invalid Property_UID");
        }



        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_OneRate_InvalidCurrency()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1002,
                        Name = "TestRate_2",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = null,
                        EndSale = null,
                        IsActive = true,
                        BoardType_UID = null,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 450,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10011,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10012,
                                IsDeleted = true
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10013,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_2"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1002 && x.Name == "TestRate_2"));

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1002).SingleOrDefault());

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1002 && x.Currency_UID == 450));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = 1");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid Currency_UID"), "Expected Error = Invalid Currency_UID");
        }


        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_TwoRates_InvalidSalePeriodAndInvalidRoomTypes()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,03,01),
                        EndSale = new DateTime(2015,01,01),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1002,
                        Name = "TestRate_2",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = null,
                        EndSale = null,
                        IsActive = true,
                        BoardType_UID = null,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 109,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 0,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 100012,
                                IsDeleted = true
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 100013,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_1"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "TestRate_2"));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_2"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1002 && x.Name == "TestRate_2"));

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 11111).SingleOrDefault());

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1002).SingleOrDefault());


            //_propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1001));

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1002 && x.Currency_UID == 109));


            //_roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 3 && arg.First() == 0 && arg.Last() == 100013), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 0 || x.UID == 100012 || x.UID == 100013) && x.Property_UID == 1002));


            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 2, "Expected Errors count = 2");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid BeginSale/EndSale period"), "Expected Error = Invalid BeginSale/EndSale period");
            Assert.IsTrue(response.Errors.Last().Description.Contains("have an not valid RoomType_UID(s)"), "Expected Error = have an not valid RoomType_UID(s)");
        }


        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_TwoRates_InvalidNameAndInvalidRoomTypes()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1002,
                        Name = "TestRate_2",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = null,
                        EndSale = null,
                        IsActive = true,
                        BoardType_UID = null,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 109,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 100011,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 100012,
                                IsDeleted = true
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 100013,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_2"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1002 && x.Name == "TestRate_2"));

            //_propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1001).SingleOrDefault());

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1002).SingleOrDefault());

            //_propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1001));

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1002 && x.Currency_UID == 109));


            //_roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 3 && arg.First() == 100011 && arg.Last() == 100013), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 100011 || x.UID == 100012 || x.UID == 100013) && x.Property_UID == 1002));


            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 2, "Expected Errors count = 2");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid Rate Name"), "Expected Error = Invalid Rate Name");
            Assert.IsTrue(response.Errors.Last().Description.Contains("Invalid RateRoomsLight/RoomType_UID(s)"), "Expected Error = Invalid RateRoomsLight/RoomType_UID(s)");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_CreateRatesLight_TwoRates_InvalidRateCategoryAndExtra()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();
            FillThePropertyCurrencyforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest newRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1001,
                        Name = "TestRate_1",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 3,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        Property_UID = 1002,
                        Name = "TestRate_2",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = null,
                        EndSale = null,
                        IsActive = true,
                        BoardType_UID = 4,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 109,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10011,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10012,
                                IsDeleted = true
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10013,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_1"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "TestRate_1"));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "TestRate_2"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1002 && x.Name == "TestRate_2"));

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 11111).SingleOrDefault());

            _propertyRepoMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Property, bool>>>())).Returns(_propertiesInDatabase.Where(x => x.UID == 1002).SingleOrDefault());


            //_propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1001));

            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1002 && x.Currency_UID == 109));


            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 3 && arg.First() == 10011 && arg.Last() == 10013), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10011 || x.UID == 10012 || x.UID == 10013) && x.Property_UID == 1002));


            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 4), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1002 && x.IsBoardType == true && x.BoardType_UID == 4 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.CreateRatesLight(newRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 2, "Expected Errors count = 2");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateCategory_UID"), "Expected Error = Invalid RateCategory_UID");
            Assert.IsTrue(response.Errors.Last().Description.Contains("Invalid BoardType_UID"), "Expected Error = Invalid BoardType_UID");
        }
        #endregion
        

        #region Test UpdateRatesLight
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_OneRate_Success()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Result.First().Name == "1st Unit Test Rate", "Expected Rate Name = 1st Unit Test Rate");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Count() == 3, "Expected RateRooms count = 3");
            Assert.IsTrue(response.Result.First().RateRoomsLight.First().RoomType_UID == 10001, "Expected First RateRooms/RoomType_UID = 10001");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Last().RoomType_UID == 10003, "Expected First RateRooms/RoomType_UID = 10003");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_OneRate_SuccessChangeName()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate Changed",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate Changeg"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate Changed"));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Result.First().Name == "1st Unit Test Rate Changed", "Expected Rate Name = 1st Unit Test Rate Changed");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Count() == 3, "Expected RateRooms count = 3");
            Assert.IsTrue(response.Result.First().RateRoomsLight.First().RoomType_UID == 10001, "Expected First RateRooms/RoomType_UID = 10001");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Last().RoomType_UID == 10003, "Expected First RateRooms/RoomType_UID = 10003");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_OneRate_InvalidUserID()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                //UserUID = 2020
            };


            //int totalRecords = -1;

            //_rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_rateCategoriesInDatabase);

            //_rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //        .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            //_rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));


            //_roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("it's required to fill the UserUID"), "Expected Error = it's required to fill the UserUID");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_OneRate_InvalidRateUID()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        //UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020
            };


            //int totalRecords = -1;

            //_rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_rateCategoriesInDatabase);

            //_rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //        .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            //_rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));

            //_roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("the UID in Rates updates must be filled."), "Expected Error = the UID in Rates updates must be filled.");
        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_OneRate_InvalidRateCategoryUID()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 3,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));

            //_roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateCategory_UID"), "Expected Error = Invalid RateCategory_UID");
        }
        

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_TwoRates_Success()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11002,
                        Property_UID = 1001,
                        Name = "2nd Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 3,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 108,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11002));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "2nd Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "2nd Unit Test Rate"));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 2 result(s)
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Result.First().Name == "1st Unit Test Rate", "Expected Rate Name = 1st Unit Test Rate");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Count() == 3, "Expected RateRooms count = 3");
            Assert.IsTrue(response.Result.First().RateRoomsLight.First().RoomType_UID == 10001, "Expected First RateRooms/RoomType_UID = 10001");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Last().RoomType_UID == 10003, "Expected First RateRooms/RoomType_UID = 10003");
            Assert.IsTrue(response.Result.Last().Name == "2nd Unit Test Rate", "Expected Rate Name = 1st Unit Test Rate");
            Assert.IsTrue(response.Result.Last().RateRoomsLight.Count() == 3, "Expected RateRooms count = 3");
            Assert.IsTrue(response.Result.Last().RateRoomsLight.First().RoomType_UID == 10001, "Expected First RateRooms/RoomType_UID = 10001");
            Assert.IsTrue(response.Result.Last().RateRoomsLight.Last().RoomType_UID == 10003, "Expected First RateRooms/RoomType_UID = 10003");
        }
        
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_TwoRates_InvalidParameters()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        //Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        //Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11002,
                        Property_UID = 1001,
                        //Name = "2nd Unit Test Rate",
                        //Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 3,
                        //RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 108,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            //int totalRecords = -1;

            //_rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_rateCategoriesInDatabase);

            //_rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //        .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            //_rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //        .Returns(_ratesInDatabase.Where(x => x.UID == 11002));

            //_rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));

            //_rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "2nd Unit Test Rate"), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "2nd Unit Test Rate"));

            //_roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 2, "Expected Errors count = 2");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("some of the required parameter(s) have not an valid value for this Rate"), "Expected Error = some of the required parameter(s) have not an valid value for this Rate");
            Assert.IsTrue(response.Errors.Last().Description.Contains("some of the required parameter(s) have not an valid value for this Rate"), "Expected Error = some of the required parameter(s) have not an valid value for this Rate");

        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_TwoRates_InvalidPropertyAndCurrency()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1002,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11002,
                        Property_UID = 1001,
                        Name = "2nd Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 3,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11002));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "2nd Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "2nd Unit Test Rate"));

            //_roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 2, "Expected Errors count = 2");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid Property_UID"), "Expected Error = Invalid Property_UID");
            Assert.IsTrue(response.Errors.Last().Description.Contains("Invalid Currency_UID"), "Expected Error = Invalid Currency_UID");

        }

        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_TwoRates_InvalidNameInvalidUID()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "2nd Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 110025,
                        Property_UID = 1001,
                        Name = "2nd Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 3,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 108,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11002));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "2nd Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                               .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "2nd Unit Test Rate"));

            //_roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 2, "Expected Errors count = 2");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid Rate Name"), "Expected Error = Invalid Rate Name");
            Assert.IsTrue(response.Errors.Last().Description.Contains("Invalid Rate UID"), "Expected Error = Invalid Rate UID");

        }


        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_TwoRates_InvalidRoomTypes()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = true
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11002,
                        Property_UID = 1001,
                        Name = "2nd Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 3,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 108,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10003,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11002));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "2nd Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                               .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "2nd Unit Test Rate"));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 3 && arg.First() == 10001 && arg.Last() == 10003), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002 || x.UID == 10003) && x.Property_UID == 1001));

            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 2, "Expected Errors count = 2");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateRoomsLight with duplicated RoomType_UID(s)"), "Expected Error = Invalid RateRoomsLight with duplicated RoomType_UID(s)");
            Assert.IsTrue(response.Errors.Last().Description.Contains("Invalid RateRoomsLight with deleted RoomType_UID(s) for this property"), "Expected Error = Invalid RateRoomsLight with deleted RoomType_UID(s) for this property");

        }
        
        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_TwoRates_InvalidRoomTypes2()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 1,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10005,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11002,
                        Property_UID = 1001,
                        Name = "2nd Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 3,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 108,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10003,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11002));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "2nd Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                               .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "2nd Unit Test Rate"));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 3 && arg.First() == 10001 && arg.Last() == 10005), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002 || x.UID == 10005) && x.Property_UID == 1001));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 3 && arg.First() == 10001 && arg.Last() == 10003), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002 || x.UID == 10003) && x.Property_UID == 1001));

            //_extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
            //    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            //                    .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 2, "Expected Errors count = 2");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("Invalid RateRoomsLight/RoomType_UID(s)"), "Expected Error = Invalid RateRoomsLight/RoomType_UID(s)");
            Assert.IsTrue(response.Errors.Last().Description.Contains("Invalid RateRoomsLight with deleted RoomType_UID(s) for this property"), "Expected Error = Invalid RateRoomsLight with deleted RoomType_UID(s) for this property");

        }


        [TestMethod]
        [TestCategory("Rates")]
        public void Test_Rates_UpdateRatesLight_TwoRates_NullAndInvalidBoardTypes()
        {
            FillTheRatesforTest();
            FillTheRateCategoriesforTest();
            FillTheBoardTypesforTest();
            FillTheExtrasforTest();
            FillTheRoomTypesforTest();

            // Build RQ with the new Rate for create
            RateLightHeaderRequest updtRateRQ = new RateLightHeaderRequest()
            {
                Rates = new List<Contracts.Data.Rates.RateHeaderLight>()
                {
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11001,
                        Property_UID = 1001,
                        Name = "1st Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = null,
                        RateCategory_UID = 2,
                        PriceModel = true,
                        Currency_UID = 34,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    },
                    new Contracts.Data.Rates.RateHeaderLight()
                    {
                        UID = 11002,
                        Property_UID = 1001,
                        Name = "2nd Unit Test Rate",
                        Description = "Rate Only for Testing Purposes",
                        BeginSale = new DateTime(2015,01,01),
                        EndSale = new DateTime(2016,12,31),
                        IsActive = true,
                        BoardType_UID = 2,
                        RateCategory_UID = 1,
                        PriceModel = true,
                        Currency_UID = 108,
                        RateRoomsLight = new List<Contracts.Data.Rates.RateRoomLight>()
                        {
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10001,
                                IsDeleted = false
                            },
                            new Contracts.Data.Rates.RateRoomLight()
                            {
                                RoomType_UID = 10002,
                                IsDeleted = false
                            }
                        }
                    }
                },
                UserUID = 2020
            };


            int totalRecords = -1;

            _rateCatRepoMock.Setup(x => x.FindRateCategoryByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_rateCategoriesInDatabase);

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11001));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 11002), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(_ratesInDatabase.Where(x => x.UID == 11002));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "1st Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "1st Unit Test Rate"));

            _rateRepoMock.Setup(x => x.FindRateByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "2nd Unit Test Rate"), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_ratesInDatabase.Where(x => x.Property_UID == 1001 && x.Name == "2nd Unit Test Rate"));

            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 10001 && arg.Last() == 10002), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_roomTypesInDatabase.Where(x => (x.UID == 10001 || x.UID == 10002) && x.Property_UID == 1001));

            _extraRepoMock.Setup(x => x.FindExtraByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1001), It.Is<bool?>(arg => arg == true), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<bool?>(arg => arg == true), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_extrasInDatabase.Where(x => x.Property_UID == 1001 && x.IsBoardType == true && x.BoardType_UID == 1 && x.IsDeleted == false));


            _changeLodDtlRepositoryFactoryMock.Setup(x => x.Add(It.IsAny<ChangeLogDetail>()));


            var response = RateManagerPOCO.UpdateRatesLight(updtRateRQ);

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.PartialSuccess, "Expected Status = PartialSuccess");
            Assert.IsTrue(response.GetType() == typeof(RateLightHeaderResponse), "Expected Response Type = RateLightHeaderResponse");

            Assert.IsTrue(response.Errors.Last().Description.Contains("Invalid BoardType_UID"), "Expected Error = Invalid BoardType_UID");
            
            Assert.IsTrue(response.Result.First().Name == "1st Unit Test Rate", "Expected Rate Name = 1st Unit Test Rate");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Count() == 3, "Expected RateRooms count = 3");
            Assert.IsTrue(response.Result.First().RateRoomsLight.First().RoomType_UID == 10001, "Expected First RateRooms/RoomType_UID = 10001");
            Assert.IsTrue(response.Result.First().RateRoomsLight.Last().RoomType_UID == 10003, "Expected First RateRooms/RoomType_UID = 10003");
        }
        #endregion

    }
}

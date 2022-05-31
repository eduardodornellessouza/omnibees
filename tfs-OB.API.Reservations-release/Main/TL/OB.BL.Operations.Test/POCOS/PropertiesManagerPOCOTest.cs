using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Contracts.Responses;
using OB.BL.Operations.Interfaces;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.Domain;
using OB.Domain.General;
using OB.Domain.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
namespace OB.BL.Operations.Test.Properties_
{
    [TestClass]
    public class PropertiesManagerPOCOTest : UnitBaseTest
    {
        public IPropertyManagerPOCO PropertyManagerPOCO
        {
            get;
            set;
        }

        private List<PropertyQR1> _propertiesInDatabase = new List<PropertyQR1>();
        private List<Currency> _currenciesInDatabase = new List<Currency>();
        private List<Country> _countriesInDatabase = new List<Country>();

        private List<RoomType> _roomTypesInDatabase = new List<RoomType>();
        private List<PropertyCurrency> _propertyCurrenciesInDatabase = new List<PropertyCurrency>();
        private Mock<IPropertyRepository> _propertyRepoMock = null;
        private Mock<IRoomTypesRepository> _roomTypeRepoMock = null;
        private Mock<IPropertyCurrencyRepository> _propertyCurrencyRepoMock = null;
        private Mock<ICurrencyRepository> _currencyRepoMock = null;
        private Mock<ICountryRepository> _countryRepoMock = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _propertiesInDatabase = new List<PropertyQR1>();

            //Mock PropertyRepository to return list of Properties instead of macking queries to the databse.
            _propertyRepoMock = new Mock<IPropertyRepository>(MockBehavior.Default);

            //Mock Repository factory to return mocked PropertyRepository
            var repoFactoryMock = new Mock<IRepositoryFactory>();
            repoFactoryMock.Setup(x => x.GetPropertyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<Property>(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertyRepoMock.Object);

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

            //Mock Repository factory to return mocked PCurrencyRepository
            _currencyRepoMock = new Mock<ICurrencyRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetCurrencyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_currencyRepoMock.Object);

            _countryRepoMock = new Mock<ICountryRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetCountryRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_countryRepoMock.Object);
            
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);

            this.Container = this.Container.RegisterInstance<IRepository<Property>>(_propertyRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IRepository<RoomType>>(_roomTypeRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IRepository<PropertyCurrency>>(_propertyCurrencyRepoMock.Object, new TransientLifetimeManager());

            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.PropertyManagerPOCO = this.Container.Resolve<IPropertyManagerPOCO>();

            _currenciesInDatabase.Add(new Currency
                {
                    CurrencySymbol = "EUR",
                    Symbol = "€",
                    UID = 1
                });

            _countriesInDatabase.Add(new Country
            {
                UID = 1,
                Name = "Teste"
            });

            _currencyRepoMock.Setup(x => x.GetAll())
                            .Returns(_currenciesInDatabase.ToList());

            _countryRepoMock.Setup(x => x.GetAll())
                            .Returns(_countriesInDatabase.ToList());
        }


        #region Test ListProperties
        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListPropertiesByUIDReturnOneTest()
        {

            _propertiesInDatabase.Add(
                new PropertyQR1
                {
                    UID = 1020,
                    Name = "Unit Test Property",
                    BaseCurrency_UID = 1,
                    Country_UID = 1
                });


            int totalRecords = -1;
            _propertyRepoMock.Setup(x => x.FindByCriteriaQR1(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1  && arg.First() == 1020), It.IsAny<List<string>>(),
                It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_propertiesInDatabase.Where(x => x.UID == 1020));
          


            var response = PropertyManagerPOCO.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest
            {
                PageIndex = 1,
                UIDs = new List<long> {  1020 }

            });

            //Response should contain only one result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");
        }


        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListPropertiesByUIDReturnEmptyTest()
        {
            _propertiesInDatabase.Add(
                 new PropertyQR1
                 {
                     UID = 1020,
                     Name = "Unit Test Property"

                 });

            int totalRecords = -1;
            _propertyRepoMock.Setup(x => x.FindByCriteriaQR1(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 8924), It.IsAny<List<string>>(),
                It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_propertiesInDatabase.Where(x => x.UID == 8924));
          
            var response = PropertyManagerPOCO.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest
            {
                PageIndex = 1,
                UIDs = new List<long> { 8924 }

            });

            //Response should be empty
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");
        }


        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListPropertiesByUIDReturnMultipleTest()
        {
            _propertiesInDatabase.Add(
                 new PropertyQR1
                 {
                     UID = 1020,
                     Name = "Unit Test Property",
                     BaseCurrency_UID = 1,
                     Country_UID = 1
                 });
            _propertiesInDatabase.Add(
                 new PropertyQR1
                 {
                     UID = 9999999,
                     Name = "Unit Test Property 2",
                     BaseCurrency_UID = 1,
                     Country_UID = 1
                 });
            _propertiesInDatabase.Add(
                 new PropertyQR1
                 {
                     UID = 5020,
                     Name = "Unit Test Property 2",
                     BaseCurrency_UID = 1,
                     Country_UID = 1
                 });

            var uidsToSearch = new List<long> { 5020, 8924, 1020 };

            int totalRecords = -1;
            _propertyRepoMock.Setup(x => x.FindByCriteriaQR1(out totalRecords, 
                It.Is<List<long>>(arg => arg.Count == 3 && arg.Contains(5020) && arg.Contains(8924) && arg.Contains(1020)), 
                It.IsAny<List<string>>(),
                It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_propertiesInDatabase.Where(x => x.UID == 5020 || x.UID == 8924 || x.UID == 1020));

            var response = PropertyManagerPOCO.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest
            {
                PageIndex = 1,
                UIDs = uidsToSearch

            });

            //Response should have 2 elements
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");
        }
        #endregion


        #region Test List RoomTypes
        private void FillTheRoomTypesforTest()
        {
            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 100001,
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
                UID = 100002,
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
                IsDeleted = true,
            });
            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 100003,
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
                IsDeleted = false,
            });


            _roomTypesInDatabase.Add(new RoomType()
            {
                UID = 100011,
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
                UID = 100012,
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
                UID = 100013,
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

        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListRoomTypesReturnOnePropertyTest()
        {
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1002), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_roomTypesInDatabase.Where(x => x.Property_UID == 1002)); 

            var response = PropertyManagerPOCO.ListRoomTypes(new Contracts.Requests.ListRoomTypeRequest
            {
                UIDs = new List<long>{},
                PropertyUIDs =  new List<long> { 1002 },
                ExcludeDeleteds = null,
                PageIndex = 1
            });

            //Response should contain only 3 results
            Assert.IsTrue(response.Result.Count == 3, "Expected result count = 3");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRoomTypeResponse), "Expected Response Type = ListRoomTypeResponse");
            Assert.IsTrue(response.Result.First().UID == 100011, "Expected UID = 100011");
            Assert.IsTrue(response.Result.Last().UID == 100013, "Expected UID = 100013");
        }

        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListRoomTypesReturnTwoPropertyTest()
        {
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1001 && arg.Last() == 1002), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_roomTypesInDatabase);

            var response = PropertyManagerPOCO.ListRoomTypes(new Contracts.Requests.ListRoomTypeRequest
            {
                UIDs = new List<long> { },
                PropertyUIDs = new List<long> { 1001, 1002 },
                ExcludeDeleteds = null,
                PageIndex = 1
            });

            //Response should contain only 6 results
            Assert.IsTrue(response.Result.Count == 6, "Expected result count = 6");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRoomTypeResponse), "Expected Response Type = ListRoomTypeResponse");
            Assert.IsTrue(response.Result.First().UID == 100001, "Expected UID = 100001");
            Assert.IsTrue(response.Result.Last().UID == 100013, "Expected UID = 100013");
        }

        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListRoomTypesReturnOnePropertyTestWithoutDeleteds()
        {
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1001), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_roomTypesInDatabase.Where(x => x.Property_UID == 1001 && x.IsDeleted != true));

            var response = PropertyManagerPOCO.ListRoomTypes(new Contracts.Requests.ListRoomTypeRequest
            {
                UIDs = new List<long> { },
                PropertyUIDs = new List<long> { 1001 },
                ExcludeDeleteds = true,
                PageIndex = 1
            });

            //Response should contain only 2 results
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRoomTypeResponse), "Expected Response Type = ListRoomTypeResponse");
            Assert.IsTrue(response.Result.First().UID == 100001, "Expected UID = 100001");
            Assert.IsTrue(response.Result.Last().UID == 100003, "Expected UID = 100003");
        }

        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListRoomTypesReturnAllPropertiesWithoutDeleteds()
        {
            FillTheRoomTypesforTest();

            int totalRecords = -1;
            _roomTypeRepoMock.Setup(x => x.FindRoomTypesByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_roomTypesInDatabase.Where(x => x.IsDeleted != true));

            var response = PropertyManagerPOCO.ListRoomTypes(new Contracts.Requests.ListRoomTypeRequest
            {
                UIDs = new List<long> { },
                PropertyUIDs = new List<long> { },
                ExcludeDeleteds = true,
                PageIndex = 1
            });

            //Response should contain only 4 results
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListRoomTypeResponse), "Expected Response Type = ListRoomTypeResponse");
            Assert.IsTrue(response.Result.First().UID == 100001, "Expected UID = 100001");
            Assert.IsTrue(response.Result.Last().UID == 100013, "Expected UID = 100013");
        }
        #endregion


        #region Test List PropertyCurrency
        private void FillThePropertyCurrencyforTest()
        {
            _propertyCurrenciesInDatabase.Add(new PropertyCurrency()
                {
                    UID = 10001,
                    Property_UID = 1001,
                    Currency_UID = 34,
                    IsAutomaticExchangeRate = true,
                    IsActive = true,
                });
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
                UID = 10004,
                Property_UID = 1002,
                Currency_UID = 34,
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

        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListPropertyCurrenciesAll()
        {
            FillThePropertyCurrencyforTest();

            int totalRecords = -1;
            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_propertyCurrenciesInDatabase);

            var response = PropertyManagerPOCO.ListPropertyCurrencies(new Contracts.Requests.ListPropertyCurrencyRequest
            {
                UIDs = new List<long> { },
                PageIndex = 1
            });

            //Response should contain only 5 results
            Assert.IsTrue(response.Result.Count == 5, "Expected result count = 5");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPropertyCurrencyResponse), "Expected Response Type = ListPropertyCurrencyResponse");
            Assert.IsTrue(response.Result.First().UID == 10001, "Expected UID = 10001");
            Assert.IsTrue(response.Result.Last().UID == 10005, "Expected UID = 10005");
        }

        [TestMethod]
        [TestCategory("Properties")]
        public void Test_Properties_ListPropertyCurrenciesByOneProperty()
        {
            FillThePropertyCurrencyforTest();

            int totalRecords = -1;
            _propertyCurrencyRepoMock.Setup(x => x.FindPropertyCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1002), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_propertyCurrenciesInDatabase.Where(x => x.Property_UID == 1002));

            var response = PropertyManagerPOCO.ListPropertyCurrencies(new Contracts.Requests.ListPropertyCurrencyRequest
            {
                UIDs = new List<long> { },
                PropertyUIDs = new List<long>() { 1002 },
                PageIndex = 1
            });

            //Response should contain only 2 results
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPropertyCurrencyResponse), "Expected Response Type = ListPropertyCurrencyResponse");
            Assert.IsTrue(response.Result.First().UID == 10004, "Expected UID = 10004");
            Assert.IsTrue(response.Result.Last().UID == 10005, "Expected UID = 10005");
        }
        #endregion

    }
}

using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Operations.Interfaces;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.Domain;
using OB.Domain.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.ObjectModel;
using OB.BL.Contracts.Responses;
using OB.Domain.Properties;

namespace OB.BL.Operations.Test.General
{
    [TestClass]
    public class GeneralManagerPOCOTest : UnitBaseTest
    {
        public IGeneralManagerPOCO GeneralManagerPOCO
        {
            get;
            set;
        }

        private List<User> _usersInDatabase = new List<User>();
        private List<ExternalSource> _externalSourcesInDatabase = new List<ExternalSource>();
        private List<Country> _countriesInDatabase = new List<Country>();
        private List<Currency> _currenciesInDatabase = new List<Currency>();
        private List<Language> _languagesInDatabase = new List<Language>();
        private List<Prefix> _prefixsInDatabase = new List<Prefix>();
        private List<PrefixLanguage> _prefixLanguagesInDatabase = new List<PrefixLanguage>();

        private Mock<IUserRepository> _userRepoMock = null;
        private Mock<IExternalSourceRepository> _externalSourceRepoMock = null;
        private Mock<ICountryRepository> _countryRepoMock = null;
        private Mock<ICurrencyRepository> _currencyRepoMock = null;
        private Mock<ILanguageRepository> _languageRepoMock = null;
        private Mock<IPrefixRepository> _prefixRepoMock = null;
        private Mock<IPrefixLanguageRepository> _prefixLanguageRepoMock = null;


        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _usersInDatabase = new List<User>();
            _externalSourcesInDatabase = new List<ExternalSource>();

            //Mock UserRepository to return list of Users instead of macking queries to the databse.
            _userRepoMock = new Mock<IUserRepository>(MockBehavior.Default);

            //Mock ExternalSourceRepository to return list of ExternalSource instead of macking queries to the databse.
            _externalSourceRepoMock = new Mock<IExternalSourceRepository>(MockBehavior.Default);

            //Mock CountryRepository to return list of Countries instead of macking queries to the databse.
            _countryRepoMock = new Mock<ICountryRepository>(MockBehavior.Default);

            //Mock CurrencyRepository to return list of Currencies instead of macking queries to the databse.
            _currencyRepoMock = new Mock<ICurrencyRepository>(MockBehavior.Default);

            //Mock LanguageRepository to return list of Languages instead of macking queries to the databse.
            _languageRepoMock = new Mock<ILanguageRepository>(MockBehavior.Default);

            //Mock PrefixRepository to return list of Prefixs instead of macking queries to the databse.
            _prefixRepoMock = new Mock<IPrefixRepository>(MockBehavior.Default);

            //Mock PrefixLanguageRepository to return list of PrefixLanguages instead of macking queries to the databse.
            _prefixLanguageRepoMock = new Mock<IPrefixLanguageRepository>(MockBehavior.Default);


            //Mock Repository factory to return mocked RateRepository
            var repoFactoryMock = new Mock<IRepositoryFactory>();
            repoFactoryMock.Setup(x => x.GetUserRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_userRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetExternalSourceRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_externalSourceRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetCountryRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_countryRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetCurrencyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_currencyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetLanguageRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_languageRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetPrefixRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_prefixRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetPrefixLanguageRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_prefixLanguageRepoMock.Object);

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);

            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.GeneralManagerPOCO = this.Container.Resolve<IGeneralManagerPOCO>();
        }


        #region Test Users and ExternalSources
        private void FillUsersforTest()
        {
            _usersInDatabase.Add(
                new User
                {
                    UID = 1001,
                    FirstName = "User1",
                    LastName = "Last1",
                    Email = "test1.omnibees@omnibees.com",
                    UserName = "User1",
                    UserPassword = "Password1",
                    IsActive = true,
                    CreatedDate = new DateTime(2015,01,01),
                    CreateBy = 65,
                    CreateDate = new DateTime(2015, 01, 01),
                    IsDummyUser = true,
                    LoginAttempts = 0
                });
            _usersInDatabase.Add(
                new User
                {
                    UID = 1002,
                    FirstName = "User2",
                    LastName = "Last2",
                    Email = "test2.omnibees@omnibees.com",
                    UserName = "User2",
                    UserPassword = "Password2",
                    IsActive = true,
                    CreatedDate = new DateTime(2015, 01, 01),
                    CreateBy = 65,
                    CreateDate = new DateTime(2015, 01, 01),
                    IsDummyUser = true,
                    LoginAttempts = 0
                });
            _usersInDatabase.Add(
                new User
                {
                    UID = 1003,
                    FirstName = "User3",
                    LastName = "Last3",
                    Email = "test3.omnibees@omnibees.com",
                    UserName = "User3",
                    UserPassword = "Password3",
                    IsActive = true,
                    CreatedDate = new DateTime(2015, 01, 01),
                    CreateBy = 65,
                    CreateDate = new DateTime(2015, 01, 01),
                    IsDummyUser = true,
                    LoginAttempts = 0
                });
            _usersInDatabase.Add(
                new User
                {
                    UID = 1004,
                    FirstName = "User4",
                    LastName = "Last4",
                    Email = "test4.omnibees@omnibees.com",
                    UserName = "User4",
                    UserPassword = "Password4",
                    IsActive = true,
                    CreatedDate = new DateTime(2015, 01, 01),
                    CreateBy = 65,
                    CreateDate = new DateTime(2015, 01, 01),
                    IsDummyUser = true,
                    LoginAttempts = 0,
                    IsDeleted = true
                });
        }

        private void FillExternalSourcesforTest()
        {
            _externalSourcesInDatabase.Add(
                new ExternalSource
                {
                    UID = 1,
                    Name = "Source1",
                    User_UID = 1001
                });

            _externalSourcesInDatabase.Add(
                new ExternalSource
                {
                    UID = 2,
                    Name = "Source2",
                    User_UID = 1002
                });

            _externalSourcesInDatabase.Add(
                new ExternalSource
                {
                    UID = 3,
                    Name = "Source3",
                    User_UID = 1003
                });

            _externalSourcesInDatabase.Add(
                new ExternalSource
                {
                    UID = 4,
                    Name = "Source4",
                    User_UID = 1004
                });
        }


        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListExternalSourcesByUIDReturnOneTest()
        {
            FillUsersforTest();
            FillExternalSourcesforTest();

            int totalRecords = -1;
            _externalSourceRepoMock.Setup(x => x.FindExternalSourceByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1), It.IsAny<List<long>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_externalSourcesInDatabase.Where(x => x.UID == 1));

            int totalRecords2 = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords2, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase);

            var response = GeneralManagerPOCO.ListExternalSource( new Contracts.Requests.ListExternalSourceRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1 },
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.Result.Any(x => x.User != null), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListExternalSourceResponse), "Expected Response Type = ListExternalSourceResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListExternalSourcesByUserUIDReturnOneTest()
        {
            FillUsersforTest();
            FillExternalSourcesforTest();

            int totalRecords = -1;
            _externalSourceRepoMock.Setup(x => x.FindExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1001), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_externalSourcesInDatabase.Where(x => x.User_UID == 1001));

            int totalRecords2 = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords2, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase);

            var response = GeneralManagerPOCO.ListExternalSource(new Contracts.Requests.ListExternalSourceRequest()
            {
                PageIndex = 1,
                UserUIDs = new List<long> { 1001 },
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.Result.Any(x => x.User != null), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListExternalSourceResponse), "Expected Response Type = ListExternalSourceResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListExternalSourcesByNameReturnOneTest()
        {
            FillUsersforTest();
            FillExternalSourcesforTest();

            int totalRecords = -1;
            _externalSourceRepoMock.Setup(x => x.FindExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.Is<List<string>>(arg => arg.Count()== 1 && arg.First() == "Source1"),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_externalSourcesInDatabase.Where(x => x.Name == "Source1"));

            int totalRecords2 = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords2, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase);

            var response = GeneralManagerPOCO.ListExternalSource(new Contracts.Requests.ListExternalSourceRequest()
            {
                PageIndex = 1,
                Names = new List<string> { "Source1" },
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.Result.Any(x => x.User != null), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListExternalSourceResponse), "Expected Response Type = ListExternalSourceResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListExternalSourcesReturnAll()
        {
            FillUsersforTest();
            FillExternalSourcesforTest();

            int totalRecords = -1;
            _externalSourceRepoMock.Setup(x => x.FindExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_externalSourcesInDatabase);

            int totalRecords2 = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords2, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase);

            var response = GeneralManagerPOCO.ListExternalSource(new Contracts.Requests.ListExternalSourceRequest()
            {
                PageIndex = 1,
                UIDs = null,
            });

            //Response should contain only 4 result
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");

            //All Response elements should contain an User details
            Assert.IsTrue(response.Result.Any(x => x.User != null), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListExternalSourceResponse), "Expected Response Type = ListExternalSourceResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 4, "Expected UID = 4");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListExternalSourcesReturnNone()
        {
            FillUsersforTest();
            FillExternalSourcesforTest();

            int totalRecords = -1;
            _externalSourceRepoMock.Setup(x => x.FindExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(new List<ExternalSource>());

            int totalRecords2 = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords2, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase);

            var response = GeneralManagerPOCO.ListExternalSource(new Contracts.Requests.ListExternalSourceRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1895 },
            });

            //Response should contain none result
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListExternalSourceResponse), "Expected Response Type = ListExternalSourceResponse");
        }


        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListUserByUIDReturnOneTest()
        {
            FillUsersforTest();

            int totalRecords = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1001), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase.Where(x => x.UID == 1001));

            var response = GeneralManagerPOCO.ListUsers(new Contracts.Requests.ListUserRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1001 },
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListUserResponse), "Expected Response Type = ListUserResponse");
            Assert.IsTrue(response.Result.First().UID == 1001, "Expected UID = 1001");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListUserByUIDReturnTwoTest()
        {
            FillUsersforTest();

            int totalRecords = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1001 && arg.Last() == 1002), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase.Where(x => x.UID == 1001 || x.UID == 1002 ));

            var response = GeneralManagerPOCO.ListUsers(new Contracts.Requests.ListUserRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1001, 1002 },
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListUserResponse), "Expected Response Type = ListUserResponse");
            Assert.IsTrue(response.Result.First().UID == 1001, "Expected UID = 1001");
            Assert.IsTrue(response.Result.Last().UID == 1002, "Expected UID = 1002");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListUserByNamesReturnTwoTest()
        {
            FillUsersforTest();

            int totalRecords = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.Is<List<string>>(arg => arg.Count() == 2 && arg.First() == "User1" && arg.Last() == "User3" ), It.Is<List<string>>(arg => arg.Count() == 2 && arg.First() == "Last1" && arg.Last() == "Last3"), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase.Where(x => x.FirstName == "User1" || x.FirstName == "User3"));

            var response = GeneralManagerPOCO.ListUsers(new Contracts.Requests.ListUserRequest()
            {
                PageIndex = 1,
                FirstNames = new List<string>() { "User1", "User3" },
                LastNames = new List<string>() {"Last1", "Last3"}
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListUserResponse), "Expected Response Type = ListUserResponse");
            Assert.IsTrue(response.Result.First().UID == 1001, "Expected UID = 1001");
            Assert.IsTrue(response.Result.Last().UID == 1003, "Expected UID = 1003");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListUserByUIDReturnAllTest()
        {
            FillUsersforTest();

            int totalRecords = -1;
            _userRepoMock.Setup(x => x.FindUserByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_usersInDatabase);

            var response = GeneralManagerPOCO.ListUsers(new Contracts.Requests.ListUserRequest()
            {
                PageIndex = 1,
                UIDs = null,
            });

            //Response should contain only 4 result
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListUserResponse), "Expected Response Type = ListUserResponse");
            Assert.IsTrue(response.Result.First().UID == 1001, "Expected UID = 1001");
            Assert.IsTrue(response.Result.Last().UID == 1004, "Expected UID = 1004");
        }
        #endregion


        #region Test Countries
        private void FillCountriesforTest()
        {
            _countriesInDatabase.Add(
                new Country
                {
                    UID = 1,
                    Name = "Afghanistan",
                    countrycode = "AF",
                    CountryIsoCode = null,
                    geonameid = 1011
                });
            _countriesInDatabase.Add(
                new Country
                {
                    UID = 2,
                    Name = "Argentina",
                    countrycode = "AR",
                    CountryIsoCode = "ARG",
                    geonameid = 1012
                });
            _countriesInDatabase.Add(
                new Country
                {
                    UID = 3,
                    Name = "Botswana",
                    countrycode = "BW",
                    CountryIsoCode = null,
                    geonameid = 1013
                });
            _countriesInDatabase.Add(
                new Country
                {
                    UID = 4,
                    Name = "Brasil",
                    countrycode = "BR",
                    CountryIsoCode = "BAR",
                    geonameid = 1014
                });
            _countriesInDatabase.Add(
                new Country
                {
                    UID = 5,
                    Name = "Portugal",
                    countrycode = "PT",
                    CountryIsoCode = null,
                    geonameid = 1015
                });
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListCountriesByUIDReturnOneTest()
        {
            FillCountriesforTest();

            int totalRecords = -1;
            _countryRepoMock.Setup(x => x.FindCountriesByCriteria(out totalRecords, It.IsAny<string>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_countriesInDatabase.Where(x => x.UID == 1));

            var response = GeneralManagerPOCO.ListCountries(new Contracts.Requests.ListCountryRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1 },
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListCountryResponse), "Expected Response Type = ListCountryResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListCountriesByUIDReturnTwoTest()
        {
            FillCountriesforTest();

            int totalRecords = -1;
            _countryRepoMock.Setup(x => x.FindCountriesByCriteria(out totalRecords, It.IsAny<string>(), It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1 && arg.Last() == 3), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_countriesInDatabase.Where(x => x.UID == 1 || x.UID == 3));

            var response = GeneralManagerPOCO.ListCountries(new Contracts.Requests.ListCountryRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1, 3 },
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListCountryResponse), "Expected Response Type = ListCountryResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 3, "Expected UID = 3");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListCountriesByUIDReturnAllTest()
        {
            FillCountriesforTest();

            int totalRecords = -1;
            _countryRepoMock.Setup(x => x.FindCountriesByCriteria(out totalRecords, It.IsAny<string>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_countriesInDatabase);

            var response = GeneralManagerPOCO.ListCountries(new Contracts.Requests.ListCountryRequest()
            {
                PageIndex = 1,
                UIDs = null,
            });

            //Response should contain only 5 result
            Assert.IsTrue(response.Result.Count == 5, "Expected result count = 5");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListCountryResponse), "Expected Response Type = ListCountryResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 5, "Expected UID = 5");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListCountriesByISOReturnTwoTest()
        {
            FillCountriesforTest();

            int totalRecords = -1;
            _countryRepoMock.Setup(x => x.FindCountriesByCriteria(out totalRecords, It.IsAny<string>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<string>>(arg => arg.Count == 2 && arg.First() == "PT" && arg.Last() == "BR"), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_countriesInDatabase.Where(x => x.countrycode == "PT" || x.countrycode == "BR"));

            var response = GeneralManagerPOCO.ListCountries(new Contracts.Requests.ListCountryRequest()
            {
                PageIndex = 1,
                CountryCodes = new List<string> { "PT", "BR" },
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListCountryResponse), "Expected Response Type = ListCountryResponse");
            Assert.IsTrue(response.Result.First().UID == 4, "Expected UID = 4");
            Assert.IsTrue(response.Result.Last().UID == 5, "Expected UID = 5");
        }
        #endregion


        #region Test Currencies
        private void FillCurrenciesForTest()
        {
            _currenciesInDatabase.Add(new Currency()
                {
                    UID = 1,
                    Name = "Euro",
                    Symbol = "EUR",
                    CurrencySymbol = "€",
                    DefaultPositionNumber = null,
                    PaypalCurrencyCode = "98"
                });
            _currenciesInDatabase.Add(new Currency()
                {
                    UID = 2,
                    Name = "Brazil Real",
                    Symbol = "BRL",
                    CurrencySymbol = "R$",
                    DefaultPositionNumber = 3,
                    PaypalCurrencyCode = "22"
                });
            _currenciesInDatabase.Add(new Currency()
                {
                    UID = 3,
                    Name = "US Dollar",
                    Symbol = "USD",
                    CurrencySymbol = "$",
                    DefaultPositionNumber = 2,
                    PaypalCurrencyCode = "125"
                });
            _currenciesInDatabase.Add(new Currency()
                {
                    UID = 4,
                    Name = "UK Pound",
                    Symbol = "GBP",
                    CurrencySymbol = "£",
                    DefaultPositionNumber = null,
                    PaypalCurrencyCode = "161"
                });
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListCurrenciesByUIDReturnOneTest()
        {
            FillCurrenciesForTest();

            int totalRecords = -1;
            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase.Where(x => x.UID == 1));

            var response = GeneralManagerPOCO.ListCurrencies(new Contracts.Requests.ListCurrencyRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1 },
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListCurrencyResponse), "Expected Response Type = ListCurrencyResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListCurrenciesByUIDReturnTwoTest()
        {
            FillCurrenciesForTest();

            int totalRecords = -1;
            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1 && arg.Last() == 3), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase.Where(x => x.UID == 1 || x.UID == 3));

            var response = GeneralManagerPOCO.ListCurrencies(new Contracts.Requests.ListCurrencyRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1, 3 },
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListCurrencyResponse), "Expected Response Type = ListCurrencyResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 3, "Expected UID = 3");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListCurrenciesByUIDReturnAllTest()
        {
            FillCurrenciesForTest();

            int totalRecords = -1;
            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase);

            var response = GeneralManagerPOCO.ListCurrencies(new Contracts.Requests.ListCurrencyRequest()
            {
                PageIndex = 1,
                UIDs = null,
            });

            //Response should contain only 4 result
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListCurrencyResponse), "Expected Response Type = ListCurrencyResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 4, "Expected UID = 4");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListCurrenciesByISOReturnThreeTest()
        {
            FillCurrenciesForTest();

            int totalRecords = -1;
            _currencyRepoMock.Setup(x => x.FindCurrencyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<string>>(arg => arg.Count() == 3 && (arg[0] == "EUR" & arg[1] == "USD" & arg[2] == "BRL")), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_currenciesInDatabase.Where(x => x.Symbol == "EUR" || x.Symbol == "USD" | x.Symbol == "BRL"));

            var response = GeneralManagerPOCO.ListCurrencies(new Contracts.Requests.ListCurrencyRequest()
            {
                PageIndex = 1,
                UIDs = null,
                Symbols = new List<string>() { "EUR", "USD" , "BRL" }
            });

            //Response should contain only 3 result
            Assert.IsTrue(response.Result.Count == 3, "Expected result count = 3");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListCurrencyResponse), "Expected Response Type = ListCurrencyResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 3, "Expected UID = 3");
        }

        #endregion


        #region Test Languages
        private void FillLanguagesForTest()
        {
            _languagesInDatabase.Add(new Language
            {
                UID = 1,
                Name = "English",
                Code = "en-US",
            });
            _languagesInDatabase.Add(new Language
            {
                UID = 2,
                Name = "Français",
                Code = "fr-FR",
            });
            _languagesInDatabase.Add(new Language
            {
                UID = 3,
                Name = "Español",
                Code = "es-ES",
            });
            _languagesInDatabase.Add(new Language
            {
                UID = 4,
                Name = "Português - PT",
                Code = "pt-PT",
            });
            _languagesInDatabase.Add(new Language
            {
                UID = 5,
                Name = "Dansk",
                Code = "da-DK",
            });
            _languagesInDatabase.Add(new Language
            {
                UID = 6,
                Name = "Português - BR",
                Code = "pt-BR",
            });
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListLanguagesByUIDReturnOneTest()
        {
            FillLanguagesForTest();

            int totalRecords = -1;
            _languageRepoMock.Setup(x => x.FindLanguageByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_languagesInDatabase.Where(x => x.UID == 1));

            var response = GeneralManagerPOCO.ListLanguages(new Contracts.Requests.ListLanguageRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1 },
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListLanguageResponse), "Expected Response Type = ListLanguageResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListLanguagesReturnAllTest()
        {
            FillLanguagesForTest();

            int totalRecords = -1;
            _languageRepoMock.Setup(x => x.FindLanguageByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_languagesInDatabase);

            var response = GeneralManagerPOCO.ListLanguages(new Contracts.Requests.ListLanguageRequest()
            {
                PageIndex = 1,
                UIDs = null,
            });

            //Response should contain only 6 results
            Assert.IsTrue(response.Result.Count == 6, "Expected result count = 6");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListLanguageResponse), "Expected Response Type = ListLanguageResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 6, "Expected UID = 6");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListLanguagesByISOReturnTwoTest()
        {
            FillLanguagesForTest();

            int totalRecords = -1;
            _languageRepoMock.Setup(x => x.FindLanguageByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.Is<List<string>>(arg => arg.Count() == 2 && arg.First() == "fr-FR" && arg.Last() == "pt-PT"),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_languagesInDatabase.Where(x => x.Code == "fr-FR" || x.Code == "pt-PT"));

            var response = GeneralManagerPOCO.ListLanguages(new Contracts.Requests.ListLanguageRequest()
            {
                PageIndex = 1,
                UIDs = null,
                Codes = new List<string>() { "fr-FR", "pt-PT" }
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListLanguageResponse), "Expected Response Type = ListLanguageResponse");
            Assert.IsTrue(response.Result.First().UID == 2, "Expected UID = 2");
            Assert.IsTrue(response.Result.Last().UID == 4, "Expected UID = 4");
        }
        #endregion


        #region Test Prefixs and PrefixLanguages
        private void FillPrefixsToTest()
        {
            _prefixsInDatabase.Add(
                new Prefix()
                {
                    UID = 1,
                    Name = "Mr."
                });
            _prefixsInDatabase.Add(
                new Prefix()
                {
                    UID = 2,
                    Name = "Mrs."
                });
            _prefixsInDatabase.Add(
                new Prefix()
                {
                    UID = 3,
                    Name = "Miss."
                });
        }

        private void FillPrefixLanguagesToTest()
        {
            _prefixLanguagesInDatabase.Add(
                new PrefixLanguage()
                {
                    UID = 1,
                    Name = "Sr.",
                    Prefix_UID = 1,
                    Prefix = new Prefix()
                        {
                            UID = 1,
                            Name = "Mr."
                        },
                    Language_UID =3,
                    Language = new Language()
                    {
                        UID = 3,
                        Name = "Español",
                        Code = "es-ES"
                    }
                });
            _prefixLanguagesInDatabase.Add(
                new PrefixLanguage()
                {
                    UID = 2,
                    Name = "Sra.",
                    Prefix_UID = 2,
                    Prefix = new Prefix()
                    {
                        UID = 2,
                        Name = "Mrs."
                    },
                    Language_UID = 3,
                    Language = new Language()
                    {
                        UID = 3,
                        Name = "Español",
                        Code = "es-ES"
                    }
                });
            _prefixLanguagesInDatabase.Add(
                new PrefixLanguage()
                {
                    UID = 3,
                    Name = "Sr.",
                    Prefix_UID = 1,
                    Prefix = new Prefix()
                    {
                        UID = 1,
                        Name = "Mr."
                    },
                    Language_UID = 4,
                    Language = new Language()
                    {
                        UID = 4,
                        Name = "Português - PT",
                        Code = "pt-PT"
                    }
                });
            _prefixLanguagesInDatabase.Add(
                new PrefixLanguage()
                {
                    UID = 4,
                    Name = "Sra.",
                    Prefix_UID = 2,
                    Prefix = new Prefix()
                    {
                        UID = 2,
                        Name = "Mrs."
                    },
                    Language_UID = 4,
                    Language = new Language()
                    {
                        UID = 4,
                        Name = "Português - PT",
                        Code = "pt-PT"
                    }
                });
            _prefixLanguagesInDatabase.Add(
                new PrefixLanguage()
                {
                    UID = 5,
                    Name = "Mr.",
                    Prefix_UID = 1,
                    Prefix = new Prefix()
                    {
                        UID = 1,
                        Name = "Mr."
                    },
                    Language_UID = 1,
                    Language = new Language()
                    {
                        UID = 1,
                        Name = "English",
                        Code = "en-US"
                    }
                });
            _prefixLanguagesInDatabase.Add(
                new PrefixLanguage()
                {
                    UID = 6,
                    Name = "Mrs.",
                    Prefix_UID = 2,
                    Prefix = new Prefix()
                    {
                        UID = 2,
                        Name = "Mrs."
                    },
                    Language_UID = 1,
                    Language = new Language()
                    {
                        UID = 1,
                        Name = "English",
                        Code = "en-US"
                    }
                });
            _prefixLanguagesInDatabase.Add(
                new PrefixLanguage()
                {
                    UID = 7,
                    Name = "Miss.",
                    Prefix_UID = 3,
                    Prefix = new Prefix()
                    {
                        UID = 3,
                        Name = "Miss."
                    },
                    Language_UID = 1,
                    Language = new Language()
                    {
                        UID = 1,
                        Name = "English",
                        Code = "en-US"
                    }
                });
        }


        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListPrefixsByUIDReturnOneTest()
        {
            FillPrefixsToTest();

            int totalRecords = -1;
            _prefixRepoMock.Setup(x => x.FindPrefixByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_prefixsInDatabase.Where(x => x.UID == 1));

            var response = GeneralManagerPOCO.ListPrefixs(new Contracts.Requests.ListPrefixRequest()
            {
                PageIndex = 1,
                UIDs = new List<long> { 1 },
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPrefixResponse), "Expected Response Type = ListPrefixResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListPrefixsByNameReturnOneTest()
        {
            FillPrefixsToTest();

            int totalRecords = -1;
            _prefixRepoMock.Setup(x => x.FindPrefixByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<string>>(arg => arg.Count == 1 && arg.First() == "Miss."),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_prefixsInDatabase.Where(x => x.Name == "Miss."));

            var response = GeneralManagerPOCO.ListPrefixs(new Contracts.Requests.ListPrefixRequest()
            {
                PageIndex = 1,
                UIDs = null,
                Names = new List<string>() { "Miss."}
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPrefixResponse), "Expected Response Type = ListPrefixResponse");
            Assert.IsTrue(response.Result.First().UID == 3, "Expected UID = 3");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListPrefixsReturnAllTest()
        {
            FillPrefixsToTest();

            int totalRecords = -1;
            _prefixRepoMock.Setup(x => x.FindPrefixByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_prefixsInDatabase);

            var response = GeneralManagerPOCO.ListPrefixs(new Contracts.Requests.ListPrefixRequest()
            {
                PageIndex = 1,
                UIDs = null,
                Names = null
            });

            //Response should contain only 3 result
            Assert.IsTrue(response.Result.Count == 3, "Expected result count = 3");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPrefixResponse), "Expected Response Type = ListPrefixResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 3, "Expected UID = 3");
        }


        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListPrefixLanguagesByUIDReturnTwoTest()
        {
            FillPrefixLanguagesToTest();

            int totalRecords = -1;
            _prefixLanguageRepoMock.Setup(x => x.FindPrefixLanguageByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 1 && arg.Last() == 3), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_prefixLanguagesInDatabase.Where(x => x.UID == 1 || x.UID == 3));

             var response = GeneralManagerPOCO.ListPrefixLanguages(new Contracts.Requests.ListPrefixLanguageRequest()
            {
                PageIndex = 1,
                UIDs = new List<long>() { 1, 3 }
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPrefixLanguageResponse), "Expected Response Type = ListPrefixLanguageResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 3, "Expected UID = 3");
            Assert.IsTrue(!response.Result.Any(x => x.Language == null), "Expected Results must have not null element Language");
            Assert.IsTrue(!response.Result.Any(x => x.Prefix == null), "Expected Results must have not null elemnte Prefix");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListPrefixLanguagesReturnAllTest()
        {
            FillPrefixLanguagesToTest();

            int totalRecords = -1;
            _prefixLanguageRepoMock.Setup(x => x.FindPrefixLanguageByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_prefixLanguagesInDatabase);

            var response = GeneralManagerPOCO.ListPrefixLanguages(new Contracts.Requests.ListPrefixLanguageRequest()
            {
                PageIndex = 1,
                UIDs = null,
            });

            //Response should contain only 7 result
            Assert.IsTrue(response.Result.Count == 7, "Expected result count = 7");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPrefixLanguageResponse), "Expected Response Type = ListPrefixLanguageResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 7, "Expected UID = 7");
            Assert.IsTrue(!response.Result.Any(x => x.Language == null), "Expected Results must have not null element Language");
            Assert.IsTrue(!response.Result.Any(x => x.Prefix == null), "Expected Results must have not null elemnte Prefix");
        }

        [TestMethod]
        [TestCategory("General")]
        public void Test_General_ListPrefixLanguagesByNameReturnTwoTest()
        {
            FillPrefixLanguagesToTest();

            int totalRecords = -1;
            _prefixLanguageRepoMock.Setup(x => x.FindPrefixLanguageByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<string>>(arg => arg.Count == 1 && arg.First() == "Sra."), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_prefixLanguagesInDatabase.Where(x => x.Name == "Sra."));

            var response = GeneralManagerPOCO.ListPrefixLanguages(new Contracts.Requests.ListPrefixLanguageRequest()
            {
                PageIndex = 1,
                UIDs = null,
                Names =new List<string>() { "Sra."}
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPrefixLanguageResponse), "Expected Response Type = ListPrefixLanguageResponse");
            Assert.IsTrue(response.Result.First().UID == 2, "Expected UID = 2");
            Assert.IsTrue(response.Result.Last().UID == 4, "Expected UID = 4");
            Assert.IsTrue(!response.Result.Any(x => x.Language == null), "Expected Results must have not null element Language");
            Assert.IsTrue(!response.Result.Any(x => x.Prefix == null), "Expected Results must have not null elemnte Prefix");
        }

        #endregion
    }
}

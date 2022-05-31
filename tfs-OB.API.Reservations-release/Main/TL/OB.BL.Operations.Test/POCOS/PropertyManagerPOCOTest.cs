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
using contractsProperties = OB.BL.Contracts.Data.Properties;

namespace OB.BL.Operations.Test.Rates
{
    [TestClass]
    public class PropertyManagerPOCOTest : UnitBaseTest
    {
        public IPropertyManagerPOCO PropertyManagerPOCO
        {
            get;
            set;
        }

        private List<Property> _propertiesInDatabase = new List<Property>();
        private List<ExternalSource> _externalSourcesInDatabase = new List<ExternalSource>();
        private List<PropertiesExternalSource> _propertiesExternalSourcesInDatabase = new List<PropertiesExternalSource>();
        private List<PropertiesExternalSourcesPermission> _PropertiesExternalSourcesPermissionsInDatabase = new List<PropertiesExternalSourcesPermission>();
        private List<Currency> _currenciesInDatabase = new List<Currency>();
        private List<RateCategory> _rateCategoriesInDatabase = new List<RateCategory>();

        private Mock<IRateRepository> _rateRepoMock = null;
        private Mock<ICurrencyRepository> _currencyRepoMock = null;

        //sprivate Mock<IPropertyManagerPOCO> _propertiesManagerPocoMock = null;
        private Mock<IPropertyRepository> _propertiesRepoMock = null;
        private Mock<IExternalSourceRepository> _externalSourcesRepoMock = null;
        private Mock<IPropertiesExternalSourcesRepository> _propertiesExternalSourcesRepoMock = null;
        private Mock<IPropertiesExternalSourcesPermissionsRepository> _PropertiesExternalSourcesPermissionRepoMock = null;
        

        private List<PropertyCurrency> _propertyCurrenciesInDatabase = new List<PropertyCurrency>();
        private Mock<IPropertyCurrencyRepository> _propertyCurrencyRepoMock = null;
        private List<RoomType> _roomTypesInDatabase = new List<RoomType>();
        private Mock<IRoomTypesRepository> _roomTypeRepoMock = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            //Mock Repository factory to return mocked RateRepository
            var repoFactoryMock = new Mock<IRepositoryFactory>();

            // Mock Properties Repository
            _propertiesRepoMock = new Mock<IPropertyRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetPropertyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertiesRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<Property>(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertiesRepoMock.Object);

            // Mock External Sources Repository
            _externalSourcesRepoMock = new Mock<IExternalSourceRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetExternalSourceRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_externalSourcesRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<ExternalSource>(It.IsAny<IUnitOfWork>()))
                            .Returns(_externalSourcesRepoMock.Object);

            // Mock Properties External Source Repository
            _propertiesExternalSourcesRepoMock = new Mock<IPropertiesExternalSourcesRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetPropertiesExternalSourcesRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertiesExternalSourcesRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<PropertiesExternalSource>(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertiesExternalSourcesRepoMock.Object);

            // Mock Properties External Source Repository
            _PropertiesExternalSourcesPermissionRepoMock = new Mock<IPropertiesExternalSourcesPermissionsRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetPropertiesExternalSourcesPermissionsRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_PropertiesExternalSourcesPermissionRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<PropertiesExternalSourcesPermission>(It.IsAny<IUnitOfWork>()))
                            .Returns(_PropertiesExternalSourcesPermissionRepoMock.Object);

            //Mock RateRepository to return list of Rates instead of macking queries to the databse.
            _rateRepoMock = new Mock<IRateRepository>(MockBehavior.Default);

            //Mock CurrencyRepository to return list of Currencies instead of macking queries to the databse.
            _currencyRepoMock = new Mock<ICurrencyRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetCurrencyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_currencyRepoMock.Object);

            // Mock Rates Repository
            repoFactoryMock.Setup(x => x.GetRateRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<Rate>(It.IsAny<IUnitOfWork>()))
                            .Returns(_rateRepoMock.Object);

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

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);

            this.Container = this.Container.RegisterInstance<IRepository<Rate>>(_rateRepoMock.Object, new TransientLifetimeManager());

            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.PropertyManagerPOCO = this.Container.Resolve<IPropertyManagerPOCO>();
        }


        #region Fill Testing Data

        private void FillThePropertiesforTest()
        {
            _propertiesInDatabase.Add(
                new Property
                {
                    UID = 1,
                    Name = "Property 1",
                    Description = "Unit Testing Rate Description",
                    CreateDate = new DateTime(2015, 01, 01),
                    ModifiedDate = null,
                    CreatedUser_UID = 65,
                    BaseCurrency_UID = 34,
                    IsActive = true,
                });

            _propertiesInDatabase.Add(
                new Property
                {
                    UID = 2,
                    Name = "Property 2",
                    Description = "Unit Testing Rate Description",
                    CreateDate = new DateTime(2015, 01, 01),
                    ModifiedDate = null,
                    CreatedUser_UID = 65,
                    BaseCurrency_UID = 34,
                    IsActive = true,
                });

            _propertiesInDatabase.Add(
                new Property
                {
                    UID = 3,
                    Name = "Property 3",
                    Description = "Unit Testing Rate Description",
                    CreateDate = new DateTime(2015, 01, 01),
                    ModifiedDate = null,
                    CreatedUser_UID = 65,
                    BaseCurrency_UID = 34,
                    IsActive = true,
                });

            _propertiesInDatabase.Add(
                new Property
                {
                    UID = 4,
                    Name = "Property 4",
                    Description = "Unit Testing Rate Description",
                    CreateDate = new DateTime(2015, 01, 01),
                    ModifiedDate = null,
                    CreatedUser_UID = 65,
                    BaseCurrency_UID = 34,
                    IsActive = true,
                });
        }

        private void FillTheExternalSourcesforTest()
        {
            _externalSourcesInDatabase.Add(
                new ExternalSource
                {
                    UID = 1,
                    Name = "External Source 1",
                    User_UID = 1
                });

            _externalSourcesInDatabase.Add(
                new ExternalSource
                {
                    UID = 2,
                    Name = "External Source 1",
                    User_UID = 2
                });
        }

        private void FillThePropertiesExternalSourcesforTest()
        {
            _propertiesExternalSourcesInDatabase.Add(
                new PropertiesExternalSource
                {
                    UID = 1,
                    ExternalSource_UID = 1,
                    ExternalSource = _externalSourcesInDatabase.FirstOrDefault(x => x.UID == 1),
                    IsActive = true,
                    Property_UID = 1,
                });

            _propertiesExternalSourcesInDatabase.Add(
                new PropertiesExternalSource
                {
                    UID = 2,
                    ExternalSource_UID = 2,
                    ExternalSource = _externalSourcesInDatabase.FirstOrDefault(x => x.UID == 2),
                    IsActive = false,
                    Property_UID = 1,
                });

            _propertiesExternalSourcesInDatabase.Add(
                new PropertiesExternalSource
                {
                    UID = 3,
                    ExternalSource_UID = 1,
                    ExternalSource = _externalSourcesInDatabase.FirstOrDefault(x => x.UID == 1),
                    IsActive = true,
                    Property_UID = 2,
                });

            _propertiesExternalSourcesInDatabase.Add(
                new PropertiesExternalSource
                {
                    UID = 4,
                    ExternalSource_UID = 1,
                    ExternalSource = _externalSourcesInDatabase.FirstOrDefault(x => x.UID == 1),
                    IsActive = false,
                    Property_UID = 4,
                });
        }

        private void FillThePropertiesExternalSourcesPermissionsforTest()
        {
            _PropertiesExternalSourcesPermissionsInDatabase.Add(
                new PropertiesExternalSourcesPermission
                {
                    UID = 1,
                    PropertiesExternalSources_UID = 1,
                    HavePermission = true,
                    RateRoom_UID = 1
                });

            _PropertiesExternalSourcesPermissionsInDatabase.Add(
                new PropertiesExternalSourcesPermission
                {
                    UID = 2,
                    PropertiesExternalSources_UID = 1,
                    HavePermission = false,
                    RateRoom_UID = 2
                });

            _PropertiesExternalSourcesPermissionsInDatabase.Add(
                new PropertiesExternalSourcesPermission
                {
                    UID = 3,
                    PropertiesExternalSources_UID = 2,
                    HavePermission = true,
                    RateRoom_UID = 1
                });
        }

        #endregion


        #region Test ListPropertiesExternalSources

        [TestMethod]
        [TestCategory("PropertiesExternalSources")]
        public void Test_Properties_ListPropertiesExternalSources()
        {
            FillThePropertiesforTest();
            FillTheExternalSourcesforTest();
            FillThePropertiesExternalSourcesforTest();
            int totalRecords = -1;

            _propertiesExternalSourcesRepoMock.Setup(x => x.FindPropertyExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                    It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                                .Returns(_propertiesExternalSourcesInDatabase);

            var response = PropertyManagerPOCO.ListPropertiesExternalSource(new Contracts.Requests.ListPropertiesExternalSourceRequest
            {
                PageIndex = 1,
                UIDs = new List<long>(),
                FilterPropertyUid = new List<long>(),
                FilterExternalSourceUid = new List<long>(),
                ReturnTotal = true
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");
            Assert.IsTrue(response.Result.Where(x => x.IsActive).Count() == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPropertiesExternalSourceResponse), "Expected Response Type = ListPropertiesExternalSourceResponse");
        }

        [TestMethod]
        [TestCategory("PropertiesExternalSources")]
        public void Test_Properties_ListPropertiesExternalSourcesWithPropertiesFilter()
        {
            FillThePropertiesforTest();
            FillTheExternalSourcesforTest();
            FillThePropertiesExternalSourcesforTest();

            int totalRecords = -1;

            _propertiesRepoMock.Setup(x => x.GetQuery())
                            .Returns(_propertiesInDatabase.AsQueryable());

            _externalSourcesRepoMock.Setup(x => x.GetQuery())
                            .Returns(_externalSourcesInDatabase.AsQueryable());

            _propertiesExternalSourcesRepoMock.Setup(x => x.FindPropertyExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(),
                It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 2), It.IsAny<List<long>>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                                .Returns(_propertiesExternalSourcesInDatabase.AsQueryable().Where(x => x.Property_UID == 2).ToList());

            var response = PropertyManagerPOCO.ListPropertiesExternalSource(new Contracts.Requests.ListPropertiesExternalSourceRequest
            {
                PageIndex = 1,
                UIDs = new List<long>(),
                FilterPropertyUid = new List<long>() { 2 },
                FilterExternalSourceUid = new List<long>(),
                ReturnTotal = true
            });

            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");
            Assert.IsTrue(response.Result.Where(x => x.IsActive).Count() == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPropertiesExternalSourceResponse), "Expected Response Type = ListPropertiesExternalSourceResponse");
        }

        [TestMethod]
        [TestCategory("PropertiesExternalSources")]
        public void Test_Properties_ListPropertiesExternalSourcesWithExternalSourcesFilter()
        {
            FillThePropertiesforTest();
            FillTheExternalSourcesforTest();
            FillThePropertiesExternalSourcesforTest();

            int totalRecords = -1;

            _propertiesRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<Property, bool>>>()))
                            .Returns(_propertiesInDatabase.Where(x => x.UID == 1 || x.UID == 2).AsQueryable());

            _externalSourcesRepoMock.Setup(x => x.GetQuery(It.IsAny<Expression<Func<ExternalSource, bool>>>()))
                            .Returns(_externalSourcesInDatabase.Where(x => x.UID == 1).AsQueryable());

            _propertiesExternalSourcesRepoMock.Setup(x => x.FindPropertyExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(),
                It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1),
                It.IsAny<bool>(), It.Is<bool>(arg => true), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                                .Returns(_propertiesExternalSourcesInDatabase.AsQueryable().Where(x => x.ExternalSource_UID == 1 && x.IsActive).ToList());

            var response = PropertyManagerPOCO.ListPropertiesExternalSource(new Contracts.Requests.ListPropertiesExternalSourceRequest
            {
                PageIndex = 1,
                UIDs = new List<long>(),
                FilterPropertyUid = new List<long>(),
                FilterExternalSourceUid = new List<long>() { 1 },
                SendLookups = true,
                ReturnTotal = true
            });

            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");
            Assert.IsTrue(response.Result.Where(x => x.IsActive).Count() == 2, "Expected result count = 2");
            //Assert.IsTrue(response.PropertiesLookup.Count == 2, "Expected result count = 2");
            //Assert.IsTrue(response.ExternalSourcesLookup.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPropertiesExternalSourceResponse), "Expected Response Type = ListPropertiesExternalSourceResponse");
        }

        #region Test ListPropertiesExternalSourcesForOmnibees

        //[TestMethod]
        //[TestCategory("PropertiesExternalSources")]
        //public void Test_Properties_ListPropertiesExternalSourcesForOmnibees()
        //{
        //    FillThePropertiesforTest();
        //    FillTheExternalSourcesforTest();
        //    FillThePropertiesExternalSourcesforTest();

        //    int totalRecords = -1;

        //    _propertiesRepoMock.Setup(x => x.GetQuery())
        //                    .Returns(_propertiesInDatabase.AsQueryable());

        //    _externalSourcesRepoMock.Setup(x => x.GetQuery())
        //                    .Returns(_externalSourcesInDatabase.AsQueryable());

        //    totalRecords = MockFindPropertyExternalSourceByCriteriaIncludeAllProperties(totalRecords);

        //    var response = PropertyManagerPOCO.ListPropertiesExternalSourceForOmnibees(new Contracts.Requests.ListPropertiesExternalSourceForOmnibeesRequest
        //    {
        //        PageIndex = 1,
        //        UIDs = new List<long>(),
        //        FilterPropertyUid = new List<long>(),
        //        FilterExternalSourceUid = new List<long>(),
        //        SendLookups = true,
        //        ReturnTotal = true
        //    });

        //    Assert.IsTrue(response.Result.Count == 8, "Expected result count = 8");
        //    Assert.IsTrue(response.Result.Where(x => x.IsActive).Count() == 2, "Expected result count = 2");
        //    Assert.IsTrue(response.Result.First(x => x.UID == 1).IsActive, "Expected result IsActive = true");
        //    Assert.IsFalse(response.Result.First(x => x.UID == 2).IsActive, "Expected result IsActive = false");
        //    Assert.IsFalse(response.Result.First(x => x.UID == 0).IsActive, "Expected result IsActive = false");

        //    //Assert Response Format
        //    Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
        //    Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
        //    Assert.IsTrue(response.Status == 0, "Expected Status = 0");
        //    Assert.IsTrue(response.GetType() == typeof(ListPropertiesExternalSourceResponse), "Expected Response Type = ListPropertiesExternalSourceResponse");
        //}

        //[TestMethod]
        //[TestCategory("PropertiesExternalSources")]
        //public void Test_Properties_ListPropertiesExternalSourcesWithPropertiesFilterForOmnibees()
        //{
        //    FillThePropertiesforTest();
        //    FillTheExternalSourcesforTest();
        //    FillThePropertiesExternalSourcesforTest();

        //    int totalRecords = -1;

        //    _propertiesRepoMock.Setup(x => x.GetQuery())
        //                    .Returns(_propertiesInDatabase.AsQueryable());

        //    _externalSourcesRepoMock.Setup(x => x.GetQuery())
        //                    .Returns(_externalSourcesInDatabase.AsQueryable());

        //    totalRecords = MockFindPropertyExternalSourceByCriteriaIncludeAllProperties(totalRecords, (x => x.Property_UID == 2));

        //    var response = PropertyManagerPOCO.ListPropertiesExternalSourceForOmnibees(new Contracts.Requests.ListPropertiesExternalSourceForOmnibeesRequest
        //    {
        //        PageIndex = 1,
        //        UIDs = new List<long>(),
        //        FilterPropertyUid = new List<long>() { 2 },
        //        FilterExternalSourceUid = new List<long>(),
        //        ReturnTotal = true
        //    });

        //    Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");
        //    Assert.IsTrue(response.Result.Where(x => x.IsActive).Count() == 1, "Expected result count = 1");
        //    Assert.IsTrue(response.Result.First(x => x.UID == 3).IsActive, "Expected result IsActive = true");
        //    Assert.IsFalse(response.Result.First(x => x.UID == 0).IsActive, "Expected result IsActive = false");

        //    //Assert Response Format
        //    Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
        //    Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
        //    Assert.IsTrue(response.Status == 0, "Expected Status = 0");
        //    Assert.IsTrue(response.GetType() == typeof(ListPropertiesExternalSourceResponse), "Expected Response Type = ListPropertiesExternalSourceResponse");
        //}

        //[TestMethod]
        //[TestCategory("PropertiesExternalSources")]
        //public void Test_Properties_ListPropertiesExternalSourcesWithExternalSourcesFilterForOmnibees()
        //{
        //    FillThePropertiesforTest();
        //    FillTheExternalSourcesforTest();
        //    FillThePropertiesExternalSourcesforTest();

        //    int totalRecords = -1;

        //    _propertiesRepoMock.Setup(x => x.GetQuery())
        //                    .Returns(_propertiesInDatabase.AsQueryable());

        //    _externalSourcesRepoMock.Setup(x => x.GetQuery())
        //                    .Returns(_externalSourcesInDatabase.AsQueryable());

        //    totalRecords = MockFindPropertyExternalSourceByCriteriaIncludeAllProperties(totalRecords, (x => x.ExternalSource_UID == 1));

        //    var response = PropertyManagerPOCO.ListPropertiesExternalSource(new Contracts.Requests.ListPropertiesExternalSourceRequest
        //    {
        //        PageIndex = 1,
        //        UIDs = new List<long>(),
        //        FilterPropertyUid = new List<long>(),
        //        FilterExternalSourceUid = new List<long>() { 1 },
        //        ReturnTotal = true
        //    });

        //    Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");
        //    Assert.IsTrue(response.Result.Where(x => x.IsActive).Count() == 2, "Expected result count = 2");
        //    Assert.IsTrue(response.PropertiesLookup.Count == 2, "Expected result count = 2");
        //    Assert.IsTrue(response.ExternalSourcesLookup.Count == 2, "Expected result count = 2");

        //    //Assert Response Format
        //    Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
        //    Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
        //    Assert.IsTrue(response.Status == 0, "Expected Status = 0");
        //    Assert.IsTrue(response.GetType() == typeof(ListPropertiesExternalSourceResponse), "Expected Response Type = ListPropertiesExternalSourceResponse");
        //}

        #endregion Test ListPropertiesExternalSourcesForOmnibees

        #endregion Test ListPropertiesExternalSources

        #region Test ListPropertiesExternalSourcesPermissions

        [TestMethod]
        [TestCategory("PropertiesExternalSourcesPermissions")]
        public void Test_Properties_ListPropertiesExternalSourcesPermissions()
        {
            FillThePropertiesforTest();
            FillTheExternalSourcesforTest();
            FillThePropertiesExternalSourcesforTest();
            FillThePropertiesExternalSourcesPermissionsforTest();

            int totalRecords = -1;

            totalRecords = MockFindPropertyExternalSourceRateRoomsByCriteria(totalRecords);

            var response = PropertyManagerPOCO.ListPropertiesExternalSourcePermissions(new Contracts.Requests.ListPropertiesExternalSourcePermissionsRequest
            {
                UIDs = new List<long>(),
                Language_UID = 1,
                ReturnTotal = true
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 3, "Expected result count = 3");
            Assert.IsTrue(response.Result.Where(x => x.HavePermission).Count() == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPropertiesExternalSourcePermissionsResponse), "Expected Response Type = ListPropertiesExternalSourceRateRoomsResponse");
        }

        [TestMethod]
        [TestCategory("PropertiesExternalSourcesPermissions")]
        public void Test_Properties_ListPropertiesExternalSourcesPermissionsWithFilter()
        {
            FillThePropertiesforTest();
            FillTheExternalSourcesforTest();
            FillThePropertiesExternalSourcesforTest();
            FillThePropertiesExternalSourcesPermissionsforTest();

            int totalRecords = -1;

            totalRecords = MockFindPropertyExternalSourceRateRoomsByCriteria(totalRecords, (x => x.UID == 1));

            var response = PropertyManagerPOCO.ListPropertiesExternalSourcePermissions(new Contracts.Requests.ListPropertiesExternalSourcePermissionsRequest
            {
                UIDs = new List<long>() { 1 },
                Language_UID = 1,
                ReturnTotal = true
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");
            Assert.IsTrue(response.Result.Where(x => x.HavePermission).Count() == 1, "Expected result count = 2");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected result UID = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListPropertiesExternalSourcePermissionsResponse), "Expected Response Type = ListPropertiesExternalSourceRateRoomsResponse");
        }

        private int MockFindPropertyExternalSourceRateRoomsByCriteria(int totalRecords, Expression<Func<PropertiesExternalSourcesPermission, bool>> predicate = null)
        {
            if (predicate != null)
            {
                _PropertiesExternalSourcesPermissionRepoMock.Setup(x => x.FindPropertyExternalSourceRateRoomsByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                    It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_PropertiesExternalSourcesPermissionsInDatabase.AsQueryable().Where(predicate).ToList());
            }
            else
            {
                _PropertiesExternalSourcesPermissionRepoMock.Setup(x => x.FindPropertyExternalSourceRateRoomsByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                    It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(_PropertiesExternalSourcesPermissionsInDatabase);
            }

            return totalRecords;
        }

        #endregion

    }
}

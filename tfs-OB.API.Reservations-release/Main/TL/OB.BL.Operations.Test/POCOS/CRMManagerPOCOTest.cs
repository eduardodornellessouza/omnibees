using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Operations.Interfaces;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.Domain;
using OB.Domain.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.ObjectModel;
using OB.BL.Contracts.Responses;
using OB.Domain.General;
using OB.BL.Contracts.Data.CRM;


namespace OB.BL.Operations.Test.CRM
{
    [TestClass]
    public class CRMManagerPOCOTest : UnitBaseTest
    {
        public ICRMManagerPOCO CRMManagerPOCO
        {
            get;
            set;
        }

        private List<OB.Domain.CRM.ThirdPartyIntermediary> _tpisInDatabase = new List<OB.Domain.CRM.ThirdPartyIntermediary>();
        private List<OB.Domain.CRM.Guest> _guestsInDatabase = new List<OB.Domain.CRM.Guest>();
        private List<OB.Domain.Properties.ExternalSourcesMapping> _guestExtSrcsInDatabase = new List<OB.Domain.Properties.ExternalSourcesMapping>();
        private List<OB.Domain.CRM.LoyaltyLevelsGeneratedLinksForBERegister> _loyaltyGeneratedLinksForBERegister = new List<OB.Domain.CRM.LoyaltyLevelsGeneratedLinksForBERegister>();

        private Mock<IGuestRepository> _guestRepoMock = null;
        private Mock<IThirdPartyIntermediaryRepository> _tpiRepoMock = null;
        private Mock<IExternalSourceMappingRepository> _guestExtSrcsRepoMock = null;
        private Mock<IExternalSourceMappingTypeRepository> _guestExtSrcsTypeRepoMock = null;
        private Mock<ILoyaltyLevelsGeneratedLinksForBERegisterRepository> _loyaltyGeneratedLinksRepoMock = null;
        private Mock<IAppSettingRepository> _appSettingRepoMock = null;

        private Mock<IRepository<ChangeLogDetail>> _changeLodDtlRepositoryFactoryMock = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _guestsInDatabase = new List<OB.Domain.CRM.Guest>();
            _tpisInDatabase = new List<OB.Domain.CRM.ThirdPartyIntermediary>();

            //Mock UserRepository to return list of Guests instead of macking queries to the databse.
            _guestRepoMock = new Mock<IGuestRepository>(MockBehavior.Default);
            _tpiRepoMock = new Mock<IThirdPartyIntermediaryRepository>(MockBehavior.Default);
            _changeLodDtlRepositoryFactoryMock = new Mock<IRepository<ChangeLogDetail>>(MockBehavior.Default);
            //Mock UserRepository to return list of GuestsExternalSource instead of macking queries to the databse.
            _guestExtSrcsRepoMock = new Mock<IExternalSourceMappingRepository>(MockBehavior.Default);
            _guestExtSrcsTypeRepoMock = new Mock<IExternalSourceMappingTypeRepository>(MockBehavior.Default);
            _loyaltyGeneratedLinksRepoMock = new Mock<ILoyaltyLevelsGeneratedLinksForBERegisterRepository>(MockBehavior.Default);
            _appSettingRepoMock = new Mock<IAppSettingRepository>(MockBehavior.Default);
                      

            //Mock Repository factory to return mocked Repository's
            var repoFactoryMock = new Mock<IRepositoryFactory>();

            repoFactoryMock.Setup(x => x.GetRepository<OB.Domain.CRM.ThirdPartyIntermediary>(It.IsAny<IUnitOfWork>()))
                            .Returns(_tpiRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetThirdPartyIntermediaryRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_tpiRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<OB.Domain.CRM.Guest>(It.IsAny<IUnitOfWork>()))
                            .Returns(_guestRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetGuestRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_guestRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<OB.Domain.Properties.ExternalSourcesMapping>(It.IsAny<IUnitOfWork>()))
                            .Returns(_guestExtSrcsRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetExternalSourceMappingRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_guestExtSrcsRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetRepository<ChangeLogDetail>(It.IsAny<IUnitOfWork>()))
                           .Returns(_changeLodDtlRepositoryFactoryMock.Object);

            repoFactoryMock.Setup(x => x.GetExternalSourceMappingRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_guestExtSrcsRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetExternalSourceMappingTypeRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_guestExtSrcsTypeRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetLoyaltyLevelsGeneratedLinksForBERegisterRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_loyaltyGeneratedLinksRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetAppSettingRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_appSettingRepoMock.Object);
           

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);

            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.CRMManagerPOCO = this.Container.Resolve<ICRMManagerPOCO>();
        }


        #region Fill Data For Testing
        private IDictionary<string, OB.Domain.CRM.Guest> FillTwinGuestsforTest()
        {
            return new Dictionary<string, OB.Domain.CRM.Guest>()
            {
                {
                "guestmail@xpto.com",
                _guestsInDatabase[0]
                }
            };
        }

        private IDictionary<string, OB.Domain.CRM.ThirdPartyIntermediary> FillTwinTPIsforTest()
        {
            return new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>()
            {
                {
                "guestmail@xpto.com",
                _tpisInDatabase[0]
                }
            };
        }

        private void FillGuestforTest()
        {
            _guestsInDatabase.Add(
                new OB.Domain.CRM.Guest()
                {
                    UID = 1,
                    BillingContactName = "Billing Contact Name",
                    BillingAddress1 = "Billing Address Line 1",
                    BillingAddress2 = "Billing Address Line 2",
                    BillingCity = "Billing City",
                    BillingState_UID = 1,
                    BillingState = "Billing State",
                    BillingPostalCode = "Billing Postal Code",
                    BillingCountry_UID = 157,
                    //BillingCountry_ISO = "PT",
                    BillingPhone = "Billing Phone",
                    BillingEmail = "Billing Email",
                    BillingTaxCardNumber = "Billing Tax Card Number",

                    FirstName = "GuestFirstName",
                    LastName = "GuestLastName",
                    IDCardNumber = "Document Number",
                    Address1 = "Guest Address Line 1",
                    Address2 = "Guest Address Line 2",
                    City = "Guest City",
                    State = "Guest State",
                    Country_UID = 157,
                    Email = "guestmail@xpto.com",
                    Phone = "289123456",
                    MobilePhone = "941234567",
                    Property_UID = 10,
                    Client_UID = 1,
                    ExternalSource_UID = 2

                });
            _guestsInDatabase.Add(
                new OB.Domain.CRM.Guest()
                {
                    UID = 2,
                    BillingContactName = "Billing Contact Name",
                    BillingAddress1 = "Billing Address Line 1",
                    BillingAddress2 = "Billing Address Line 2",
                    BillingCity = "Billing City",
                    BillingState_UID = 1,
                    BillingState = "Billing State",
                    BillingPostalCode = "Billing Postal Code",
                    BillingCountry_UID = 157,
                    //BillingCountry_ISO = "PT",
                    BillingPhone = "Billing Phone",
                    BillingEmail = "Billing Email",
                    BillingTaxCardNumber = "Billing Tax Card Number",

                    FirstName = "Guest2FirstName",
                    LastName = "Guest2LastName",
                    IDCardNumber = "Document Number",
                    Address1 = "Guest2 Address Line 1",
                    Address2 = "Guest2 Address Line 2",
                    City = "Guest2 City",
                    State = "Guest2 State",
                    Country_UID = 157,
                    Email = "guest2mail@xpto.com",
                    Phone = "289123456",
                    MobilePhone = "941234567",

                    Property_UID = 10,
                    Client_UID = 1,
                    ExternalSource_UID = 2

                });
            _guestsInDatabase.Add(
                new OB.Domain.CRM.Guest()
                {
                    UID = 3,
                    BillingContactName = "Billing Contact Name",
                    BillingAddress1 = "Billing Address Line 1",
                    BillingAddress2 = "Billing Address Line 2",
                    BillingCity = "Billing City",
                    BillingState_UID = 1,
                    BillingState = "Billing State",
                    BillingPostalCode = "Billing Postal Code",
                    BillingCountry_UID = 157,
                    //BillingCountry_ISO = "PT",
                    BillingPhone = "Billing Phone",
                    BillingEmail = "Billing Email",
                    BillingTaxCardNumber = "Billing Tax Card Number",

                    FirstName = "Guest3FirstName",
                    LastName = "Guest3LastName",
                    IDCardNumber = "Document Number",
                    Address1 = "Guest3 Address Line 1",
                    Address2 = "Guest3 Address Line 2",
                    City = "Guest3 City",
                    State = "Guest3 State",
                    Country_UID = 157,
                    Email = "guest3mail@xpto.com",
                    Phone = "289123456",
                    MobilePhone = "941234567",

                    Property_UID = 10,
                    Client_UID = 1,
                    ExternalSource_UID = 2

                });
            _guestsInDatabase.Add(
               new OB.Domain.CRM.Guest()
               {
                   UID = 4,
                   BillingContactName = "Manuel Silva Billing",
                   BillingAddress1 = "Billing Rua xpto",
                   BillingAddress2 = "Billing lote 2",
                   BillingCity = "Billing Amarleja",
                   BillingState_UID = 1,
                   BillingState = "Billing Amarl",
                   BillingPostalCode = "Billing 8900-895",
                   BillingCountry_UID = 157,
                   //BillingCountry_ISO = "PT",
                   BillingPhone = "Billing 986523189",
                   BillingEmail = "Billing@xpto.pt",
                   BillingTaxCardNumber = "Billing 1256-235",

                   FirstName = "Manuel",
                   LastName = "Silva",
                   IDCardNumber = "123456789 Number",
                   Address1 = "Rua xpto",
                   Address2 = "lote 2",
                   City = "Amarleja",
                   State = "Amarl",
                   Country_UID = 157,
                   Email = "xmail@xpto.com",
                   Phone = "289123456",
                   MobilePhone = "941234567",

                   Property_UID = 10,
                   Client_UID = 1,
                   ExternalSource_UID = 2

               });

        }

        private void FillTPIforTest()
        {
            _tpisInDatabase.Add(
                new OB.Domain.CRM.ThirdPartyIntermediary()
                {
                    UID = 1,                    
                    Address1 = "Guest Address Line 1",
                    Address2 = "Guest Address Line 2",
                    City = "Guest City",
                    State = "Guest State",
                    Country_UID = 157,
                    Email = "guestmail@xpto.com",
                    Phone = "289123456",
                    MobilePhone = "941234567",
                    Property_UID = 10,
                    Client_UID = 1,
                    ExternalSource_UID = 2                    

                });
            _tpisInDatabase.Add(
                new OB.Domain.CRM.ThirdPartyIntermediary()
                {
                    UID = 2,                    
                    Address1 = "Guest2 Address Line 1",
                    Address2 = "Guest2 Address Line 2",
                    City = "Guest2 City",
                    State = "Guest2 State",
                    Country_UID = 157,
                    Email = "guest2mail@xpto.com",
                    Phone = "289123456",
                    MobilePhone = "941234567",

                    Property_UID = 10,
                    Client_UID = 1,
                    ExternalSource_UID = 2

                });
            _tpisInDatabase.Add(
                new OB.Domain.CRM.ThirdPartyIntermediary()
                {
                    UID = 3,
                    
                    Address1 = "Guest3 Address Line 1",
                    Address2 = "Guest3 Address Line 2",
                    City = "Guest3 City",
                    State = "Guest3 State",
                    Country_UID = 157,
                    Email = "guest3mail@xpto.com",
                    Phone = "289123456",
                    MobilePhone = "941234567",

                    Property_UID = 10,
                    Client_UID = 1,
                    ExternalSource_UID = 2

                });
            _tpisInDatabase.Add(
               new OB.Domain.CRM.ThirdPartyIntermediary()
               {
                   UID = 4,
                   
                   Address1 = "Rua xpto",
                   Address2 = "lote 2",
                   City = "Amarleja",
                   State = "Amarl",
                   Country_UID = 157,
                   Email = "xmail@xpto.com",
                   Phone = "289123456",
                   MobilePhone = "941234567",

                   Property_UID = 10,
                   Client_UID = 1,
                   ExternalSource_UID = 2

               });

        }

        private void FillTheGuestsExternalSourcesforTest()
        {
            _guestExtSrcsInDatabase.Add(new OB.Domain.Properties.ExternalSourcesMapping()
            {
                UID = 1,
                Mapping_UID = 1001,
                ExternalSource_UID = 1,
                ExternalMappingID = "GUEST1",
                IsDeleted = false,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = null,
                ExternalSourceMappingType = null
            });
            _guestExtSrcsInDatabase.Add(new OB.Domain.Properties.ExternalSourcesMapping()
            {
                UID = 2,
                Mapping_UID = 1002,
                ExternalSource_UID = 1,
                ExternalMappingID = "GUEST2",
                IsDeleted = false,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = null,
                ExternalSourceMappingType = null
            });
            _guestExtSrcsInDatabase.Add(new OB.Domain.Properties.ExternalSourcesMapping()
            {
                UID = 3,
                Mapping_UID = 1003,
                ExternalSource_UID = 1,
                ExternalMappingID = "GUEST3",
                IsDeleted = true,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = null,
                ExternalSourceMappingType = null
            });
            _guestExtSrcsInDatabase.Add(new OB.Domain.Properties.ExternalSourcesMapping()
            {
                UID = 4,
                Mapping_UID = 1001,
                ExternalSource_UID = 2,
                ExternalMappingID = "GUEST1",
                IsDeleted = false,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = null,
                ExternalSourceMappingType = null
            });
            _guestExtSrcsInDatabase.Add(new OB.Domain.Properties.ExternalSourcesMapping()
            {
                UID = 5,
                Mapping_UID = 1002,
                ExternalSource_UID = 2,
                ExternalMappingID = "GUEST2",
                IsDeleted = true,
                CreatedDate = new DateTime(2015, 01, 01),
                ModifiedDate = new DateTime(2015, 03, 01),
                ExternalSourceMappingType = null
            });
        }

        public static OB.BL.Contracts.Data.CRM.Guest GenerateGuest()
        {
            return new OB.BL.Contracts.Data.CRM.Guest()
            {
                Index = 1,
                UID = 1,
                BillingContactName = "Billing Contact Name",
                BillingAddress1 = "Billing Address Line 1",
                BillingAddress2 = "Billing Address Line 2",
                BillingCity = "Billing City",
                BillingState_UID = 1,
                BillingState = "Billing State",
                BillingPostalCode = "Billing Postal Code",
                BillingCountry_UID = 157,
                //BillingCountry_ISO = "PT",
                BillingPhone = "Billing Phone",
                BillingEmail = "Billing Email",
                BillingTaxCardNumber = "Billing Tax Card Number",

                Client_UID = 1,
                FirstName = "GuestFirstName",
                LastName = "GuestLastName",
                IDCardNumber = "Document Number",
                Address1 = "Guest Address Line 1",
                Address2 = "Guest Address Line 2",
                City = "Guest City",
                State = "Guest State",
                Country_UID = 157,
                Email = "guestmail@xpto.com",
                Phone = "289123456",
                MobilePhone = "941234567",
                Property_UID = 1930,

                ExternalSource_UID = 2
            };
        }

        public static OB.BL.Contracts.Data.CRM.TPICustom GenerateTPI()
        {
            return new OB.BL.Contracts.Data.CRM.TPICustom()
            {
                Index = 1,
                UID = 1,               

                Client_UID = 1,
               
                Address1 = "Guest Address Line 1",
                Address2 = "Guest Address Line 2",
                City = "Guest City",
                State = "Guest State",
                Country_UID = 157,
                Email = "guestmail@xpto.com",
                Phone = "289123456",
                MobilePhone = "941234567",
                Property_UID = 1930,

                ExternalSource_UID = 2
            };
        }

        private void FillLoyaltyGeneratedLinksForBERegisterForTest()
        {
            _loyaltyGeneratedLinksForBERegister.Add( new OB.Domain.CRM.LoyaltyLevelsGeneratedLinksForBERegister
                {
                    GUID = new Guid("11feb154-da41-4a26-976c-ab53109e5a50"),
                    Client_UID = 1,
                    LoyaltyLevel_UID = 2,
                    CreatedBy = 65,
                    CreatedDate = new DateTime(2016, 1, 1),
                    ExpireDate = new DateTime(2016, 2, 1),
                    wasUsed = false
                }
            );
        }
        #endregion


        #region Test TPI
        [TestMethod]
        public void Test_CRM_InsertTPI_WithTwinTPI()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string,string>>>>()))
                .Returns(this.FillTwinTPIsforTest());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_tpisInDatabase[0]);


            var response = CRMManagerPOCO.InsertThirdPartyIntermediaries(new Contracts.Requests.InsertThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { GenerateTPI() },
                UserUID = 2020
            });



            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Once(), "Need to be call only once");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertThirdPartyIntermediaryResponse), "Expected Response Type = InsertThirdPartyIntermediaryResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");


        }

        [TestMethod]
        public void Test_CRM_InsertTPI_NoTwinTPI()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_tpisInDatabase[1]);


            var response = CRMManagerPOCO.InsertThirdPartyIntermediaries(new Contracts.Requests.InsertThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { GenerateTPI() },
                UserUID = 2020
            });

            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertThirdPartyIntermediaryResponse), "Expected Response Type = InsertThirdPartyIntermediaryResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsTrue(tpiResult.IsSuccess, "Must be a success task");
                Assert.IsFalse(tpiResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_InsertTPI_NoEmail()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()));


            Contracts.Data.CRM.TPICustom tpiToSend = GenerateTPI();
            tpiToSend.Email = null;
            var response = CRMManagerPOCO.InsertThirdPartyIntermediaries(new Contracts.Requests.InsertThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { tpiToSend },
                UserUID = 2020
            });



            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertThirdPartyIntermediaryResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsFalse(tpiResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(tpiResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_InsertTPI_NoProperty()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()));


            Contracts.Data.CRM.TPICustom tpiToSend = GenerateTPI();
            tpiToSend.Property_UID = 0;
            var response = CRMManagerPOCO.InsertThirdPartyIntermediaries(new Contracts.Requests.InsertThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { tpiToSend },
                UserUID = 2020
            });



            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertThirdPartyIntermediaryResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsFalse(tpiResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(tpiResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_InsertTPI_NoTPI()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()));


            Contracts.Data.CRM.TPICustom tpiToSend = GenerateTPI();
            tpiToSend.Client_UID = 0;
            var response = CRMManagerPOCO.InsertThirdPartyIntermediaries(new Contracts.Requests.InsertThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { tpiToSend },
                UserUID = 2020
            });



            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertThirdPartyIntermediaryResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsFalse(tpiResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(tpiResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_InsertTPI_NoUser()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_tpisInDatabase[1]);

            var response = CRMManagerPOCO.InsertThirdPartyIntermediaries(new Contracts.Requests.InsertThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { GenerateTPI() },
                UserUID = 0
            });

            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsNull(response.ThirdPartyIntermediaryResults, "Error must occur when UserID is equal to 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = fail");
            Assert.IsTrue(response.GetType() == typeof(InsertThirdPartyIntermediaryResponse), "Expected Response Type = InsertThirdPartyIntermediaryResponse");


            foreach (Error error in response.Errors)
            {
                Assert.AreEqual(ErrorType.InvalidUserID, error.ErrorType, "Must be a InvalidUserID");

            }
        }

        [TestMethod]
        public void Test_CRM_UpdateTPI_WithTwinTPI()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(this.FillTwinTPIsforTest());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_tpisInDatabase[1]);

            var response = CRMManagerPOCO.UpdateThirdPartyIntermediaries(new Contracts.Requests.UpdateThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { GenerateTPI() },
                UserUID = 2020
            });



            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Once(), "Need to be call once");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateThirdPartyIntermediaryResponse), "Expected Response Type = UpdateThirdPartyIntermediaryResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 2, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsTrue(tpiResult.IsSuccess, "Must be a success task");
                Assert.IsTrue(tpiResult.IsModified, "Must be an update and not an insert");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateTPI_NoTwinTPI()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_tpisInDatabase[1]);


            var response = CRMManagerPOCO.UpdateThirdPartyIntermediaries(new Contracts.Requests.UpdateThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { GenerateTPI() },
                UserUID = 2020
            });

            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateThirdPartyIntermediaryResponse), "Expected Response Type = UpdateThirdPartyIntermediaryResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsFalse(tpiResult.IsSuccess, "Must be a success task");
                Assert.IsFalse(tpiResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateTPI_NoEmail()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()));

            Contracts.Data.CRM.TPICustom tpiToSend = GenerateTPI();
            tpiToSend.Email = null;
            var response = CRMManagerPOCO.UpdateThirdPartyIntermediaries(new Contracts.Requests.UpdateThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { tpiToSend },
                UserUID = 2020
            });



            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateThirdPartyIntermediaryResponse), "Expected Response Type = UpdateThirdPartyIntermediaryResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsFalse(tpiResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(tpiResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateTPI_NoProperty()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()));

            Contracts.Data.CRM.TPICustom tpiToSend = GenerateTPI();
            tpiToSend.Property_UID = 0;
            var response = CRMManagerPOCO.UpdateThirdPartyIntermediaries(new Contracts.Requests.UpdateThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { tpiToSend },
                UserUID = 2020
            });



            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateThirdPartyIntermediaryResponse), "Expected Response Type = UpdateThirdPartyIntermediaryResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsFalse(tpiResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(tpiResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateTPI_NoTPI()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()));

            Contracts.Data.CRM.TPICustom tpiToSend = GenerateTPI();
            tpiToSend.Client_UID = 0;
            var response = CRMManagerPOCO.UpdateThirdPartyIntermediaries(new Contracts.Requests.UpdateThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { tpiToSend },
                UserUID = 2020
            });

            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateThirdPartyIntermediaryResponse), "Expected Response Type = UpdateThirdPartyIntermediaryResponse");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.ThirdPartyIntermediaryResults.First().Index == 1, "Expected Index = 1");

            foreach (ThirdPartyIntermediaryResult tpiResult in response.ThirdPartyIntermediaryResults)
            {
                Assert.IsFalse(tpiResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(tpiResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateTPI_NoUser()
        {
            FillTPIforTest();

            int totalRecords = -1;

            _tpiRepoMock.Setup(x => x.FindTwinTPIs(out totalRecords, It.IsAny<Dictionary<long?, List<Tuple<string, string>>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.ThirdPartyIntermediary>());

            _tpiRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_tpisInDatabase[1]);

            var response = CRMManagerPOCO.UpdateThirdPartyIntermediaries(new Contracts.Requests.UpdateThirdPartyIntermediaryRequest()
            {
                ThirdPartyIntermediaries = new List<Contracts.Data.CRM.TPICustom> { GenerateTPI() },
                UserUID = 0
            });

            _tpiRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsNull(response.ThirdPartyIntermediaryResults, "Error must occur when UserID is equal to 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = fail");
            Assert.IsTrue(response.GetType() == typeof(UpdateThirdPartyIntermediaryResponse), "Expected Response Type = InsertGuestResponse");


            foreach (Error error in response.Errors)
            {
                Assert.AreEqual(ErrorType.InvalidUserID, error.ErrorType, "Must be a InvalidUserID");

            }
        }
        #endregion


        #region Test Guests
        [TestMethod]
        public void Test_CRM_InsertGuest_WithTwinGuest()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(this.FillTwinGuestsforTest());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_guestsInDatabase[1]);


            var response = CRMManagerPOCO.InsertGuest(new Contracts.Requests.InsertGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { GenerateGuest() },
                UserUID = 2020
            });



            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Once(), "Need to be call once");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 2, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");


        }

        [TestMethod]
        public void Test_CRM_InsertGuest_NoTwinGuest()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_guestsInDatabase[1]);


            var response = CRMManagerPOCO.InsertGuest(new Contracts.Requests.InsertGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { GenerateGuest() },
                UserUID = 2020
            });

            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsTrue(guestResult.IsSuccess, "Must be a success task");
                Assert.IsFalse(guestResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_InsertGuest_NoEmail()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()));


            Contracts.Data.CRM.Guest guestToSend = GenerateGuest();
            guestToSend.Email = null;
            var response = CRMManagerPOCO.InsertGuest(new Contracts.Requests.InsertGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { guestToSend },
                UserUID = 2020
            });



            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsFalse(guestResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(guestResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_InsertGuest_NoProperty()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()));


            Contracts.Data.CRM.Guest guestToSend = GenerateGuest();
            guestToSend.Property_UID = 0;
            var response = CRMManagerPOCO.InsertGuest(new Contracts.Requests.InsertGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { guestToSend },
                UserUID = 2020
            });



            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsFalse(guestResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(guestResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_InsertGuest_NoClient()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()));


            Contracts.Data.CRM.Guest guestToSend = GenerateGuest();
            guestToSend.Client_UID = 0;
            var response = CRMManagerPOCO.InsertGuest(new Contracts.Requests.InsertGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { guestToSend },
                UserUID = 2020
            });



            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(InsertGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsFalse(guestResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(guestResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_InsertGuest_NoUser()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_guestsInDatabase[1]);

            var response = CRMManagerPOCO.InsertGuest(new Contracts.Requests.InsertGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { GenerateGuest() },
                UserUID = 0
            });

            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsNull(response.GuestResults, "Error must occur when UserID is equal to 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = fail");
            Assert.IsTrue(response.GetType() == typeof(InsertGuestResponse), "Expected Response Type = InsertGuestResponse");


            foreach (Error error in response.Errors)
            {
                Assert.AreEqual(ErrorType.InvalidUserID, error.ErrorType, "Must be a InvalidUserID");

            }
        }

        [TestMethod]
        public void Test_CRM_UpdateGuest_WithTwinGuest()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(this.FillTwinGuestsforTest());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_guestsInDatabase[1]);

            var response = CRMManagerPOCO.UpdateGuest(new Contracts.Requests.UpdateGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { GenerateGuest() },
                UserUID = 2020
            });



            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Once(), "Need to be call once");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 2, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsTrue(guestResult.IsSuccess, "Must be a success task");
                Assert.IsTrue(guestResult.IsModified, "Must be an update and not an insert");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateGuest_NoTwinGuest()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_guestsInDatabase[1]);


            var response = CRMManagerPOCO.UpdateGuest(new Contracts.Requests.UpdateGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { GenerateGuest() },
                UserUID = 2020
            });

            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsFalse(guestResult.IsSuccess, "Must be a success task");
                Assert.IsFalse(guestResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateGuest_NoEmail()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()));

            Contracts.Data.CRM.Guest guestToSend = GenerateGuest();
            guestToSend.Email = null;
            var response = CRMManagerPOCO.UpdateGuest(new Contracts.Requests.UpdateGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { guestToSend },
                UserUID = 2020
            });



            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsFalse(guestResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(guestResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateGuest_NoProperty()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()));

            Contracts.Data.CRM.Guest guestToSend = GenerateGuest();
            guestToSend.Property_UID = 0;
            var response = CRMManagerPOCO.UpdateGuest(new Contracts.Requests.UpdateGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { guestToSend },
                UserUID = 2020
            });



            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsFalse(guestResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(guestResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateGuest_NoClient()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()));

            Contracts.Data.CRM.Guest guestToSend = GenerateGuest();
            guestToSend.Client_UID = 0;
            var response = CRMManagerPOCO.UpdateGuest(new Contracts.Requests.UpdateGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { guestToSend },
                UserUID = 2020
            });



            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsTrue(response.GuestResults.Count == 1, "Expected result count = 1");

            //All Response elements should contain an User details
            Assert.IsTrue(response.GuestResults.Any(x => x.UID != 0), "Expected result for ExternalSource/User should not be 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(UpdateGuestResponse), "Expected Response Type = InsertGuestResponse");
            Assert.IsTrue(response.GuestResults.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.GuestResults.First().Index == 1, "Expected Index = 1");

            foreach (GuestResult guestResult in response.GuestResults)
            {
                Assert.IsFalse(guestResult.IsSuccess, "Must be a insuccess task");
                Assert.IsFalse(guestResult.IsModified, "Must be an insert and not an update");
            }
        }

        [TestMethod]
        public void Test_CRM_UpdateGuest_NoUser()
        {
            FillGuestforTest();

            int totalRecords = -1;

            _guestRepoMock.Setup(x => x.FindTwinGuests(out totalRecords, It.IsAny<Dictionary<long, List<string>>>()))
                .Returns(new Dictionary<string, OB.Domain.CRM.Guest>());

            _guestRepoMock.Setup(x => x.Get(It.IsAny<object>()))
        .Returns(_guestsInDatabase[1]);

            var response = CRMManagerPOCO.UpdateGuest(new Contracts.Requests.UpdateGuestRequest()
            {
                Guests = new List<Contracts.Data.CRM.Guest> { GenerateGuest() },
                UserUID = 0
            });

            _guestRepoMock.Verify(c => c.Get(It.IsAny<object>()), Times.Never(), "This can't never be called");

            //Response should contain only 1 result
            Assert.IsNull(response.GuestResults, "Error must occur when UserID is equal to 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = fail");
            Assert.IsTrue(response.GetType() == typeof(UpdateGuestResponse), "Expected Response Type = InsertGuestResponse");


            foreach (Error error in response.Errors)
            {
                Assert.AreEqual(ErrorType.InvalidUserID, error.ErrorType, "Must be a InvalidUserID");

            }
        }
        #endregion


        #region Test GuestsExternalSources
        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_ListAllTest()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase);

            var response = CRMManagerPOCO.ListGuestsExternalSources(new Contracts.Requests.ListGuestExternalSourcesRequest()
            {
                PageIndex = 1,
                ExcludeDeleteds = null,
                //ReturnTotal = true
            });

            //Response should contain only 5 result(s)
            Assert.IsTrue(response.Result.Count == 5, "Expected result count = 5");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 5, "Expected UID = 5");
            //Assert.IsTrue(response.TotalRecords == 5, "Expected TotalRecords = 5");
        }


        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_ListAllUndeletedTest()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 2 && arg.First() == 1 && arg.Last() == 2), It.IsAny<List<string>>(), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => (x.ExternalSource_UID == 1 || x.ExternalSource_UID == 2) && x.IsDeleted == false));

            var response = CRMManagerPOCO.ListGuestsExternalSources(new Contracts.Requests.ListGuestExternalSourcesRequest()
            {
                PageIndex = 1,
                ExcludeDeleteds = true,
                ExternalSource_UIDs = new List<long>() { 1, 2 }
            });

            //Response should contain only 3 result(s)
            Assert.IsTrue(response.Result.Count == 3, "Expected result count = 3");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 4, "Expected UID = 4");
        }



        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_InsertOneSuccess()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "NewGuest1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "NewGuest1") && x.IsDeleted == false));

            _guestRepoMock.Setup(x => x.FindGuestByLightCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false, 0, 0, false)).Returns(_guestsInDatabase);


            var response = CRMManagerPOCO.InsertOrUpdateGuestsExternalSources(new Contracts.Requests.InsertOrUpdateGuestExternalSourceRequest()
            {
                ReturnTotal = true,
                GuestsExternalSources = new List<Contracts.Data.CRM.GuestsExternalSource>()
                {
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 0,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "NewGuest1",
                        Guest_UID = 1001,
                        IsDeleted = false
                    },
                }
            });

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_UpdateOneSuccess()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => x.ExternalSource_UID == 1 && x.ExternalMappingID == "GUEST1"));

            _guestRepoMock.Setup(x => x.FindGuestByLightCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false, 0, 0, false)).Returns(_guestsInDatabase);


            var response = CRMManagerPOCO.InsertOrUpdateGuestsExternalSources(new Contracts.Requests.InsertOrUpdateGuestExternalSourceRequest()
            {
                ReturnTotal = true,
                GuestsExternalSources = new List<Contracts.Data.CRM.GuestsExternalSource>()
                {
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 0,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "GUEST1",
                        Guest_UID = 1001,
                        IsDeleted = true
                    },
                }
            });

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_NoneUpdateSuccess()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => x.ExternalSource_UID == 1 && x.ExternalMappingID == "GUEST1"));

            _guestRepoMock.Setup(x => x.FindGuestByLightCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false, 0, 0, false)).Returns(_guestsInDatabase);


            var response = CRMManagerPOCO.InsertOrUpdateGuestsExternalSources(new Contracts.Requests.InsertOrUpdateGuestExternalSourceRequest()
            {
                ReturnTotal = true,
                GuestsExternalSources = new List<Contracts.Data.CRM.GuestsExternalSource>()
                {
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 0,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "GUEST1",
                        Guest_UID = 1001,
                        IsDeleted = false
                    },
                }
            });

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_CreationIdenticalDeleted_Success()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST2"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => (x.ExternalSource_UID == 2 && x.ExternalMappingID == "GUEST2") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST2"), false,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => (x.ExternalSource_UID == 2 && x.ExternalMappingID == "GUEST2")));

            _guestRepoMock.Setup(x => x.FindGuestByLightCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false, 0, 0, false)).Returns(_guestsInDatabase);


            var response = CRMManagerPOCO.InsertOrUpdateGuestsExternalSources(new Contracts.Requests.InsertOrUpdateGuestExternalSourceRequest()
            {
                ReturnTotal = true,
                GuestsExternalSources = new List<Contracts.Data.CRM.GuestsExternalSource>()
                {
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 0,
                        ExternalSource_UID = 2,
                        ExternalGuestID = "GUEST2",
                        Guest_UID = 1002,
                        IsDeleted = false
                    },
                }
            });

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_DeletedIdenticalDeleted_Success()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST2"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => (x.ExternalSource_UID == 2 && x.ExternalMappingID == "GUEST2") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 2), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST2"), false,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => (x.ExternalSource_UID == 2 && x.ExternalMappingID == "GUEST2")));

            _guestRepoMock.Setup(x => x.FindGuestByLightCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false, 0, 0, false)).Returns(_guestsInDatabase);


            var response = CRMManagerPOCO.InsertOrUpdateGuestsExternalSources(new Contracts.Requests.InsertOrUpdateGuestExternalSourceRequest()
            {
                ReturnTotal = true,
                GuestsExternalSources = new List<Contracts.Data.CRM.GuestsExternalSource>()
                {
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 0,
                        ExternalSource_UID = 2,
                        ExternalGuestID = "GUEST2",
                        Guest_UID = 1002,
                        IsDeleted = true
                    },
                }
            });

            //Response should contain only 1 result(s)
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_InsertSeveralSuccess()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "NewGuest1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "NewGuest1") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "GUEST1") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "NewGuest2"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "NewGuest2") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST3"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "GUEST3") && x.IsDeleted == false));


            _guestRepoMock.Setup(x => x.FindGuestByLightCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false, 0, 0, false)).Returns(_guestsInDatabase);
            _guestExtSrcsRepoMock
                .Setup(x => x.GetQuery());

            var response = CRMManagerPOCO.InsertOrUpdateGuestsExternalSources(new Contracts.Requests.InsertOrUpdateGuestExternalSourceRequest()
            {
                ReturnTotal = true,
                GuestsExternalSources = new List<Contracts.Data.CRM.GuestsExternalSource>()
                {
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 0,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "NewGuest1",
                        Guest_UID = 1001,
                        IsDeleted = false
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 1,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "GUEST1",
                        Guest_UID = 1001,
                        IsDeleted = false
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 2,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "NewGuest2",
                        Guest_UID = 1002,
                        IsDeleted = true
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 3,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "GUEST3",
                        Guest_UID = 1003,
                        IsDeleted = false
                    },
                }
            });

            //Response should contain only 4 result(s)
            Assert.IsTrue(response.Result.Count == 4, "Expected result count = 4");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
        }


        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_InvalidRequest()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "NewGuest1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "NewGuest1") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "GUEST1") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "NewGuest2"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "NewGuest2") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST3"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "GUEST3") && x.IsDeleted == false));


            _guestRepoMock.Setup(x => x.FindGuestByLightCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false, 0, 0, false)).Returns(_guestsInDatabase);


            var response = CRMManagerPOCO.InsertOrUpdateGuestsExternalSources(new Contracts.Requests.InsertOrUpdateGuestExternalSourceRequest()
            {
                ReturnTotal = true,
                GuestsExternalSources = new List<Contracts.Data.CRM.GuestsExternalSource>()
                {
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 0,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "",
                        Guest_UID = 1001,
                        IsDeleted = false
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 1,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "",
                        Guest_UID = 1001,
                        IsDeleted = false
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 2,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "NewGuest2",
                        Guest_UID = 1002,
                        IsDeleted = true
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 3,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "GUEST3",
                        Guest_UID = 1003,
                        IsDeleted = false
                    },
                }
            });

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("multiple row existences of the same ExternalSource_UID vs ExternalGuestID combination(s)"), "Expected Error: multiple row existences of the same ExternalSource_UID vs ExternalGuestID combination(s).");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_GuestsExternalSources_InvalidRequest2()
        {
            FillTheGuestsExternalSourcesforTest();

            int totalRecords = -1;
            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "NewGuest1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "NewGuest1") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST1"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "GUEST1") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "NewGuest2"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "NewGuest2") && x.IsDeleted == false));

            _guestExtSrcsRepoMock.Setup(x => x.FindGuestsExternalSourceByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1), It.Is<List<string>>(arg => arg.Count() == 1 && arg.First() == "GUEST3"), true,
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_guestExtSrcsInDatabase.Where(x => ((x.ExternalSource_UID == 1) && x.ExternalMappingID == "GUEST3") && x.IsDeleted == false));


            _guestRepoMock.Setup(x => x.FindGuestByLightCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false, 0, 0, false)).Returns(_guestsInDatabase);


            var response = CRMManagerPOCO.InsertOrUpdateGuestsExternalSources(new Contracts.Requests.InsertOrUpdateGuestExternalSourceRequest()
            {
                ReturnTotal = true,
                GuestsExternalSources = new List<Contracts.Data.CRM.GuestsExternalSource>()
                {
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 0,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "",
                        Guest_UID = 1001,
                        IsDeleted = false
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 1,
                        ExternalSource_UID = 0,
                        ExternalGuestID = "GUEST1",
                        Guest_UID = 0,
                        IsDeleted = false
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 2,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "NewGuest2",
                        Guest_UID = 1002,
                        IsDeleted = true
                    },
                    new Contracts.Data.CRM.GuestsExternalSource()
                    {
                        UID = 3,
                        ExternalSource_UID = 1,
                        ExternalGuestID = "GUEST3",
                        Guest_UID = 0,
                        IsDeleted = false
                    },
                }
            });

            //Response should contain only 0 result(s)
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.GetType() == typeof(ListGuestsExternalSourcesResponse), "Expected Response Type = ListGuestsExternalSourcesResponse");
            Assert.IsTrue(response.Errors.First().Description.Contains("some rows have not filled their ExternalGuestID, ExternalSource_UID and Guest_UID."), "Expected Error: some rows have not filled their ExternalGuestID, ExternalSource_UID and Guest_UID.");
        }


        #endregion Test GuestsExternalSources


        #region Test Loyalty Generated Links For BE Register
        [TestMethod]
        [TestCategory("CRM")]
        public void Test_CRM_ListLoyaltyLevelsGeneratedLinksForBERegister_Success()
        {
            FillLoyaltyGeneratedLinksForBERegisterForTest();

            int totalRecords = -1;

            _loyaltyGeneratedLinksRepoMock.Setup(x => x.FindLoyaltyLevelsGeneratedLinksForBERegisterByCriteria(out totalRecords, It.IsAny<List<Guid>>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 2),
                It.IsAny<bool?>(), It.IsAny<bool?>(), 
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_loyaltyGeneratedLinksForBERegister.Where(x => x.GUID.ToString() == "11feb154-da41-4a26-976c-ab53109e5a50"));
            
            var response = CRMManagerPOCO.ListLoyaltyLevelsGeneratedLinksForBERegister(new Contracts.Requests.ListLoyaltyLevelsGeneratedLinksForBERegisterRequest
            {
                Client_UIDs = new List<long>() { 1 },
                LoyaltyLevels_UIDs = new List<long>() { 2 },
                ExcludeUseds = true,
                ExcludeExpireds = false
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result.Count == 1, "Expected result count = 1");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListLoyaltyLevelsGeneratedLinksForBERegisterResponse), "Expected Response Type = ListLoyaltyLevelsGeneratedLinksForBERegisterResponse");
            Assert.IsTrue(response.Result.First().GUID.ToString() == "11feb154-da41-4a26-976c-ab53109e5a50", "Expected GUID = \"11feb154-da41-4a26-976c-ab53109e5a50\"");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_CRM_ListLoyaltyLevelsGeneratedLinksForBERegister_Expired()
        {
            FillLoyaltyGeneratedLinksForBERegisterForTest();

            int totalRecords = -1;

            _loyaltyGeneratedLinksRepoMock.Setup(x => x.FindLoyaltyLevelsGeneratedLinksForBERegisterByCriteria(out totalRecords, It.IsAny<List<Guid>>(), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1), It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 2),
                It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_loyaltyGeneratedLinksForBERegister.Where(x => x.GUID.ToString() == "11feb154-da41-4a26-976c-ab53109e5a50" && x.ExpireDate > DateTime.UtcNow));
            
            var response = CRMManagerPOCO.ListLoyaltyLevelsGeneratedLinksForBERegister(new Contracts.Requests.ListLoyaltyLevelsGeneratedLinksForBERegisterRequest
            {
                Client_UIDs = new List<long>() { 1 },
                LoyaltyLevels_UIDs = new List<long>() { 2 },
                ExcludeUseds = true,
                ExcludeExpireds = true
            });

            //Response should contain only 0 result
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListLoyaltyLevelsGeneratedLinksForBERegisterResponse), "Expected Response Type = ListLoyaltyLevelsGeneratedLinksForBERegisterResponse");
        }


        [TestMethod]
        [TestCategory("CRM")]
        public void Test_CRM_GenerateLoyaltyGeneratedLinksForBERegister_Error()
        {
            var response = CRMManagerPOCO.GenerateLinksForLoyaltyLevelToBERegister(new Contracts.Requests.GenerateLinksForLoyaltyLevelToBERegisterRequest
            {
                //Client_UID = 1,
                //LoyaltyLevel_UID = 2,
                //User_UID = 65,
                Qty = 10
            });

            //Response should contain only 0 result
            Assert.IsTrue(response.Result == null, "Expected result = null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = 1");
            Assert.IsTrue(response.GetType() == typeof(GenerateLinksForLoyaltyLevelToBERegisterResponse), "Expected Response Type = GenerateLinksForLoyaltyLevelToBERegisterResponse");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.GenerateLinksForLoyaltyLevelToBERegisterError, "Expected Response Type = ErrorType.GenerateLinksForLoyaltyLevelToBERegisterError");
        }
        
        [TestMethod]
        [TestCategory("CRM")]
        public void Test_CRM_GenerateLoyaltyGeneratedLinksForBERegister_Success()
        {
            var response = CRMManagerPOCO.GenerateLinksForLoyaltyLevelToBERegister(new Contracts.Requests.GenerateLinksForLoyaltyLevelToBERegisterRequest
            {
                Client_UID = 1,
                LoyaltyLevel_UID = 2,
                User_UID = 65,
                Qty = 2
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count() == 2, "Expected result = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(GenerateLinksForLoyaltyLevelToBERegisterResponse), "Expected Response Type = GenerateLinksForLoyaltyLevelToBERegisterResponse");
        }
        

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_CRM_RemoveGeneratedLinkForLoyaltyLevelToBERegister_Error1()
        {
            FillLoyaltyGeneratedLinksForBERegisterForTest();

            var response = CRMManagerPOCO.RemoveGeneratedLinkForLoyaltyLevelToBERegister(new Contracts.Requests.RemoveGeneratedLinkForLoyaltyLevelToBERegisterRequest
            {
                GUID = new Guid("11feb154-da41-4a26-976c-000000000000")
            });

            //Response should contain only 0 result
            Assert.IsTrue(response.Result == null, "Expected result = null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = 1");
            Assert.IsTrue(response.GetType() == typeof(RemoveGeneratedLinkForLoyaltyLevelToBERegisterResponse), "Expected Response Type = RemoveGeneratedLinkForLoyaltyLevelToBERegisterResponse");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.RemoveGeneratedLinkForLoyaltyLevelToBERegisterError, "Expected Response Type = ErrorType.RemoveGeneratedLinkForLoyaltyLevelToBERegisterError");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_CRM_RemoveGeneratedLinkForLoyaltyLevelToBERegister_Error2()
        {
            FillLoyaltyGeneratedLinksForBERegisterForTest();

            var response = CRMManagerPOCO.RemoveGeneratedLinkForLoyaltyLevelToBERegister(new Contracts.Requests.RemoveGeneratedLinkForLoyaltyLevelToBERegisterRequest
            {
                //GUID = new Guid("11feb154-da41-4a26-976c-000000000000")
            });

            //Response should contain only 0 result
            Assert.IsTrue(response.Result == null, "Expected result = null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = 1");
            Assert.IsTrue(response.GetType() == typeof(RemoveGeneratedLinkForLoyaltyLevelToBERegisterResponse), "Expected Response Type = RemoveGeneratedLinkForLoyaltyLevelToBERegisterResponse");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.RemoveGeneratedLinkForLoyaltyLevelToBERegisterError, "Expected Response Type = ErrorType.RemoveGeneratedLinkForLoyaltyLevelToBERegisterError");
        }

        [TestMethod]
        [TestCategory("CRM")]
        public void Test_CRM_RemoveGeneratedLinkForLoyaltyLevelToBERegister_Success()
        {
            FillLoyaltyGeneratedLinksForBERegisterForTest();

            _loyaltyGeneratedLinksRepoMock.Setup(x => x.FindOne(It.IsAny<System.Linq.Expressions.Expression<System.Func<OB.Domain.CRM.LoyaltyLevelsGeneratedLinksForBERegister,bool>>>()))
                  .Returns(_loyaltyGeneratedLinksForBERegister.Where(x => x.GUID.ToString() == "11feb154-da41-4a26-976c-ab53109e5a50").SingleOrDefault());

            var response = CRMManagerPOCO.RemoveGeneratedLinkForLoyaltyLevelToBERegister(new Contracts.Requests.RemoveGeneratedLinkForLoyaltyLevelToBERegisterRequest
            {
                GUID = new Guid("11feb154-da41-4a26-976c-ab53109e5a50")
            });

            //Response should contain only 1 result
            Assert.IsTrue(response.Result != null, "Expected result = not null");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(RemoveGeneratedLinkForLoyaltyLevelToBERegisterResponse), "Expected Response Type = RemoveGeneratedLinkForLoyaltyLevelToBERegisterResponse");
        }




        #endregion
    }
}

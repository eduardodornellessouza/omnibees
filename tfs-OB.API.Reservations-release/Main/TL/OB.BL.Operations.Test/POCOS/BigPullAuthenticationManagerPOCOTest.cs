using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.ObjectModel;
using OB.BL.Operations.Interfaces;
using OB.Domain.General;
using OB.DL.Common.Interfaces;
using OB.BL.Operations.Test;
using Moq;
using OB.BL.Operations;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;

namespace OB.REST.Services.Test.Controllers
{
    [TestClass]
    public class BigPullAuthenticationManagerPOCOTest : UnitBaseTest
    {
        public IBigPullAuthenticationManagerPOCO BPAManagerPOCO
        {
            get;
            set;
        }


        private List<BigPullAuthentication> _UsersBPAInDatabase = new List<BigPullAuthentication>();

        private Mock<IOperatorAuthenticationRepository> _BPAUsersRepoMock = null;

        private Mock<IRepository<BigPullAuthentication>> _BPAUsersRepositoryFactoryMock = null;



        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _BPAUsersRepoMock = new Mock<IOperatorAuthenticationRepository>(MockBehavior.Default);

            _BPAUsersRepositoryFactoryMock = new Mock<IRepository<BigPullAuthentication>>(MockBehavior.Default);


            //Mock Repository factory to return mocked OperatorAuthenticationRepository
            var repoFactoryMock = new Mock<IRepositoryFactory>();
            repoFactoryMock.Setup(x => x.GetOperationAuthenticationRepository(It.IsAny<IUnitOfWork>()))
                                .Returns(_BPAUsersRepoMock.Object);

            //Mock IRepository<BigPullAuthentication>
            repoFactoryMock.Setup(x => x.GetRepository<BigPullAuthentication>(It.IsAny<IUnitOfWork>()))
                            .Returns(_BPAUsersRepositoryFactoryMock.Object);

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);

            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.BPAManagerPOCO = this.Container.Resolve<IBigPullAuthenticationManagerPOCO>();
        }


        #region Fill Data for Testing
        private void FillBPAUsersforTest()
        {
            _UsersBPAInDatabase = new List<BigPullAuthentication>();

            _UsersBPAInDatabase.Add(
                new BigPullAuthentication()
                {
                    UID = 1,
                    IsDeleted = false,
                    PropertiesRestricted = false,
                    ChannelUID = 80,
                    ChannelCode = "1234",
                    Username = "UserXpto1",
                    Password = "Visual@2015",
                    IPList = null,
                    Properties = null,
                    POSCode = "MyPortal1",
                    POSType = null,
                    PartnerId = 1000,
                    PartnerName = "My Partner 1",
                    ChannelName = "4 Cantos",
                    UserType = 1
                }
            );

            _UsersBPAInDatabase.Add(
                new BigPullAuthentication()
                {
                    UID = 2,
                    IsDeleted = false,
                    PropertiesRestricted = false,
                    ChannelUID = 80,
                    ChannelCode = "1235",
                    Username = "UserXpto2",
                    Password = "Visual@2015",
                    IPList = null,
                    Properties = null,
                    POSCode = "MyPortal2",
                    POSType = null,
                    PartnerId = 2000,
                    PartnerName = "My Partner 2",
                    ChannelName = "4 Cantos",
                    UserType = 1
                }
            );

            _UsersBPAInDatabase.Add(
                new BigPullAuthentication()
                {
                    UID = 3,
                    IsDeleted = false,
                    PropertiesRestricted = false,
                    ChannelUID = 80,
                    ChannelCode = "1236",
                    Username = "UserXpto3",
                    Password = "Visual@2015",
                    IPList = null,
                    Properties = null,
                    POSCode = "MyPortal3",
                    POSType = null,
                    PartnerId = 3000,
                    PartnerName = "My Partner 3",
                    ChannelName = "4 Cantos",
                    UserType = 1
                }
            );
        }
        #endregion


        #region Insert TPIs Users
        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_InsertBPAUser_Test_Error_NoData()
        {
            var response = BPAManagerPOCO.InsertPOTPIUser(new InsertBigPullAuthenticationUserRequest()
            {
                RequestGuid = new Guid()
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.BigPullAuthenticationInsert_InvalidRequest_Error, "Expected ErrorType = BigPullAuthenticationInsert_InvalidRequest_Error!!!");
            Assert.IsTrue(response.GetType() == typeof(InsertBigPullAuthenticationUserResponse), "Expected Response Type = InsertBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result == null, "Expected Result count = null");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_InsertBPAUser_WrongUsername_Error()
        {
            var response = BPAManagerPOCO.InsertPOTPIUser(new InsertBigPullAuthenticationUserRequest()
            {
                ChannelUID = 80,
                Username = "a * bá",
                Password = "Visual@2015",
                POSCode = "MyPortal3",
                POSType = null,
                PartnerId = 6000,
                PartnerName = "My Partner 4",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.BigPullAuthenticationInsert_InvalidRequest_Error, "Expected ErrorType = BigPullAuthenticationInsert_InvalidRequest_Error!!!");
            Assert.IsTrue(response.GetType() == typeof(InsertBigPullAuthenticationUserResponse), "Expected Response Type = InsertBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result == null, "Expected Result count = null");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_InsertBPAUser_WrongPassword_Error()
        {
            var response = BPAManagerPOCO.InsertPOTPIUser(new InsertBigPullAuthenticationUserRequest()
            {
                ChannelUID = 80,
                Username = "UserTest1",
                Password = "ab*",
                POSCode = "MyPortal3",
                POSType = null,
                PartnerId = 6000,
                PartnerName = "My Partner 4",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.BigPullAuthenticationInsert_InvalidRequest_Error, "Expected ErrorType = BigPullAuthenticationInsert_InvalidRequest_Error!!!");
            Assert.IsTrue(response.GetType() == typeof(InsertBigPullAuthenticationUserResponse), "Expected Response Type = InsertBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result == null, "Expected Result count = null");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_InsertBPAUser_RepeatedUsername_Error()
        {
            FillBPAUsersforTest();

            int totalRecords = 0;
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "UserXpto1"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "UserXpto1"));

            var response = BPAManagerPOCO.InsertPOTPIUser(new InsertBigPullAuthenticationUserRequest()
            {
                ChannelUID = 80,
                Username = "UserXpto1",
                Password = "MyVisual@2015",
                POSCode = "MyPortal3",
                POSType = null,
                PartnerId = 6000,
                PartnerName = "My Partner 4",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.BigPullAuthenticationInsert_UsernameAlreadyExists_Error, "Expected ErrorType = BigPullAuthenticationInsert_UsernameAlreadyExists_Error!!!");
            Assert.IsTrue(response.GetType() == typeof(InsertBigPullAuthenticationUserResponse), "Expected Response Type = InsertBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result == null, "Expected Result count = null");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_InsertBPAUser_Success()
        {
            FillBPAUsersforTest();

            int totalRecords = 0;
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "UserTest1"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "UserTest1"));

            var response = BPAManagerPOCO.InsertPOTPIUser(new InsertBigPullAuthenticationUserRequest()
            {
                ChannelUID = 80,
                Username = "UserTest1",
                Password = "MyVisual@2015",
                POSCode = "MyPortal3",
                POSType = null,
                PartnerId = 6000,
                PartnerName = "My Partner 4",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(InsertBigPullAuthenticationUserResponse), "Expected Response Type = InsertBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result != null && response.Result.Username == "UserTest1", "Expected response.Result.Username = \"UserTest1\"");
        }
        #endregion

        #region Update TPIs Users
        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_UpdateBPAUser_WrongUsername_Error()
        {
            var response = BPAManagerPOCO.UpdatePOTPIUser(new UpdateBigPullAuthenticationUserRequest()
            {
                Username = "a * bá",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.BigPullAuthenticationUpdate_InvalidRequest_Error, "Expected ErrorType = BigPullAuthenticationUpdate_InvalidRequest_Error!!!");
            Assert.IsTrue(response.GetType() == typeof(UpdateBigPullAuthenticationUserResponse), "Expected Response Type = UpdateBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result == null, "Expected Result count = null");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_UpdateBPAUser_WrongNewPassword_Error()
        {
            var response = BPAManagerPOCO.UpdatePOTPIUser(new UpdateBigPullAuthenticationUserRequest()
            {
                Password = "áb",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.BigPullAuthenticationUpdate_InvalidRequest_Error, "Expected ErrorType = BigPullAuthenticationUpdate_InvalidRequest_Error!!!");
            Assert.IsTrue(response.GetType() == typeof(UpdateBigPullAuthenticationUserResponse), "Expected Response Type = UpdateBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result == null, "Expected Result count = null");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_UpdateBPAUser_ChangeUsername_SameUsername_Error()
        {
            FillBPAUsersforTest();

            int totalRecords = 0;
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "UserXpto2"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "UserXpto2"));
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "UserXpto1"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "UserXpto1"));

            var response = BPAManagerPOCO.UpdatePOTPIUser(new UpdateBigPullAuthenticationUserRequest()
            {
                Username = "UserXpto1",
                NewUsername = "UserXpto2",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.BigPullAuthenticationUpdate_UsernameAlreadyExists_Error, "Expected ErrorType = BigPullAuthenticationUpdate_UsernameAlreadyExists_Error!!!");
            Assert.IsTrue(response.GetType() == typeof(UpdateBigPullAuthenticationUserResponse), "Expected Response Type = UpdateBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result == null, "Expected Result count = null");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_UpdateBPAUser_ChangeUsername_Success()
        {
            FillBPAUsersforTest();

            int totalRecords = 0;
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "Visual1234"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "Visual1234"));
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "UserXpto1"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "UserXpto1"));

            var response = BPAManagerPOCO.UpdatePOTPIUser(new UpdateBigPullAuthenticationUserRequest()
            {
                Username = "UserXpto1",
                NewUsername = "Visual1234",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(UpdateBigPullAuthenticationUserResponse), "Expected Response Type = UpdateBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result != null && response.Result.Username == "Visual1234", "Expected Result.Username = \"Visual1234\"");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_UpdateBPAUser_ChangePassword_Error()
        {
            FillBPAUsersforTest();

            int totalRecords = 0;
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "UserXpto1"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "UserXpto1"));

            var response = BPAManagerPOCO.UpdatePOTPIUser(new UpdateBigPullAuthenticationUserRequest()
            {
                Username = "UserXpto1",
                OldPassword = "Visualyjetyjy",
                Password = "Visual@2016",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Status == Status.Fail, "Expected Status = Fail");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.BigPullAuthenticationUpdate_WrongOldPassword_Error, "Expected ErrorType = BigPullAuthenticationUpdate_WrongOldPassword_Error!!!");
            Assert.IsTrue(response.GetType() == typeof(UpdateBigPullAuthenticationUserResponse), "Expected Response Type = UpdateBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result == null, "Expected Result count = null");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_UpdateBPAUser_ChangePassword_Success()
        {
            FillBPAUsersforTest();

            int totalRecords = 0;
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "UserXpto1"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "UserXpto1"));

            var response = BPAManagerPOCO.UpdatePOTPIUser(new UpdateBigPullAuthenticationUserRequest()
            {
                Username = "UserXpto1",
                OldPassword = "Visual@2015",
                Password = "Visual@2016",
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(UpdateBigPullAuthenticationUserResponse), "Expected Response Type = UpdateBigPullAuthenticationUserResponse");
            Assert.IsTrue(response.Result != null && response.Result.Password == "Visual@2016", "Expected Result.Password = \"Visual@2016\"");
        }

        [TestMethod]
        [TestCategory("BigPullAuthentication")]
        public void Test_Users_UpdateBPAUser_ChangeIsDeleted_Success()
        {
            FillBPAUsersforTest();

            int totalRecords = 0;
            _BPAUsersRepoMock.Setup(x => x.AutheticateByCriteria(out totalRecords, It.Is<string>(arg => arg != null && arg == "UserXpto1"), It.Is<string>(arg => arg == null), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(_UsersBPAInDatabase.Where(x => x.Username == "UserXpto1"));

            var response = BPAManagerPOCO.UpdatePOTPIUser(new UpdateBigPullAuthenticationUserRequest()
            {
                Username = "UserXpto1",
                IsDeleted = true,
                ISO_LanguageCode = "pt-BR",
                UserType = BL.Contracts.Data.General.EnumUserType.Portal
            });

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == Status.Success, "Expected Status = Success");
            Assert.IsTrue(response.GetType() == typeof(UpdateBigPullAuthenticationUserResponse), "Expected Response Type = UpdateBigPullAuthenticationUserResponse");
        }
        #endregion
    }
}

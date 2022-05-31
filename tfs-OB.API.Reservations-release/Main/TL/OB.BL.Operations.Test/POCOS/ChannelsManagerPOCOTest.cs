using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Operations.Interfaces;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.Domain;
using OB.Domain.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using OB.BL.Contracts.Responses;

namespace OB.BL.Operations.Test.Channels
{
    [TestClass]
    public class ChannelManagerPOCOTest : UnitBaseTest
    {
        public IChannelsManagerPOCO ChannelsManagerPOCO
        {
            get;
            set;
        }

        private List<Channel> _channelsInDatabase = new List<Channel>();
        private List<ChannelsProperty> _channelsPropertyInDatabase = new List<ChannelsProperty>();
        private List<ChannelOperator> _channelsOperatorInDatabase = new List<ChannelOperator>();
        private Mock<IChannelsRepository> _channelsRepoMock = null;
        private Mock<IChannelsPropertyRepository> _channelsPropertyRepoMock = null;
        private Mock<IChannelOperatorRepository> _channelOperatorRepoMock = null;


        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _channelsInDatabase = new List<Channel>();

            //Mock ChannelsRepository to return list of Channels instead of macking queries to the databse.
            _channelsRepoMock = new Mock<IChannelsRepository>(MockBehavior.Default);

            //Mock ChannelsPropertyRepository to return list of ChannelsProperty instead of macking queries to the database.
            _channelsPropertyRepoMock = new Mock<IChannelsPropertyRepository>(MockBehavior.Default);

            //Mock ChannelOperatorRepository to return list of ChannelOperator instead of macking queries to the database.
            _channelOperatorRepoMock = new Mock<IChannelOperatorRepository>(MockBehavior.Default);

            //Mock Repository factory to return mocked RateRepository
            var repoFactoryMock = new Mock<IRepositoryFactory>();
            repoFactoryMock.Setup(x => x.GetChannelsRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_channelsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<Channel>(It.IsAny<IUnitOfWork>()))
                            .Returns(_channelsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetChannelsPropertyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_channelsPropertyRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetChannelOperatorRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_channelOperatorRepoMock.Object);



            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);

            this.Container = this.Container.RegisterInstance<IRepository<Channel>>(_channelsRepoMock.Object, new TransientLifetimeManager());

            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.ChannelsManagerPOCO = this.Container.Resolve<IChannelsManagerPOCO>();
        }


        #region Fill Data For Testing
        private void FillTheChannelsPropertyforTest()
        {
            _channelsPropertyInDatabase.Add(
                new ChannelsProperty()
                {
                    UID = 1,
                    Channel_UID = 1,
                    Channel = new Channel()
                    {
                        UID = 1,
                        Enabled = true
                    },
                    Property_UID = 1001,
                    IsActive = true,
                    RateModel_UID = 1,
                    Value = 10,
                    IsPercentage = true
                });
            _channelsPropertyInDatabase.Add(
                new ChannelsProperty()
                {
                    UID = 2,
                    Channel_UID = 2,
                    Channel = new Channel()
                    {
                        UID = 2,
                        Enabled = true
                    },
                    Property_UID = 1001,
                    IsActive = true,
                    RateModel_UID = 1,
                    Value = 10,
                    IsPercentage = true
                });
            _channelsPropertyInDatabase.Add(
                new ChannelsProperty()
                {
                    UID = 3,
                    Channel_UID = 1,
                    Channel = new Channel()
                    {
                        UID = 3,
                        Enabled = false
                    },
                    Property_UID = 1001,
                    IsActive = true,
                    RateModel_UID = 2,
                    Value = 10,
                    IsPercentage = true
                });
            _channelsPropertyInDatabase.Add(
                new ChannelsProperty()
                {
                    UID = 4,
                    Channel_UID = 1,
                    Channel = new Channel()
                    {
                        UID = 1,
                        Enabled = true
                    },
                    Property_UID = 1002,
                    IsActive = true,
                    RateModel_UID = 1,
                    Value = 10,
                    IsPercentage = true
                });
            _channelsPropertyInDatabase.Add(
                new ChannelsProperty()
                {
                    UID = 5,
                    Channel_UID = 2,
                    Channel = new Channel()
                    {
                        UID = 2,
                        Enabled = true
                    },
                    Property_UID = 1002,
                    IsActive = true,
                    RateModel_UID = 1,
                    Value = 10,
                    IsPercentage = true
                });
            _channelsPropertyInDatabase.Add(
                new ChannelsProperty()
                {
                    UID = 6,
                    Channel_UID = 3,
                    Channel = new Channel()
                    {
                        UID = 3,
                        Enabled = false
                    },
                    Property_UID = 1002,
                    IsActive = true,
                    RateModel_UID = 2,
                    Value = 10,
                    IsPercentage = true
                });
        }

        private void FillTheChannelOperatorsforTest() {
            _channelsOperatorInDatabase.Add(new ChannelOperator()
            {
                Channel_UID = 80,
                CNPJ = "123456789",
                CreditUsed = null,
                Address1 = "Avv. Rio Branco, 181 - Salas 707/708",
                Address2 = "teste - 31.155.670/0001-00",
                PostalCode = "20040-001",
                Country_UID = 157,
                State_UID = 7466773,
                City_UID = 1797162,
                CreatedDate = new DateTime(2013, 12, 11),
                CreatedBy = 65,
                ModifiedDate = new DateTime(2015, 11, 9),
                ModifiedBy = 70,
                Email = "lenia.andrade@visualforma.pt111"
            });
            _channelsOperatorInDatabase.Add(new ChannelOperator()
            {
                Channel_UID = 81,
                CNPJ = "123456789",
                CreditUsed = null,
                Address1 = "Av. Ayrton Senna, 70",
                Address2 = null,
                PostalCode = "20040-001",
                Country_UID = 157,
                State_UID = 7466773,
                City_UID = 1797162,
                CreatedDate = new DateTime(2013, 12, 11),
                CreatedBy = 65,
                ModifiedDate = new DateTime(2015, 11, 9),
                ModifiedBy = 70,
                Email = "mail@mail.com"
            });
            _channelsOperatorInDatabase.Add(new ChannelOperator()
            {
                Channel_UID = 82,
                CNPJ = "16.325.318/0003-44",
                CreditUsed = null,
                Address1 = "Rua da Alfazema, 761 Ed Iguatemi Business Flat loja 23",
                Address2 = null,
                PostalCode = "41.820.710",
                Country_UID = 40,
                State_UID = 7705856,
                City_UID = 4108172,
                CreatedDate = new DateTime(2013, 12, 11),
                CreatedBy = 65,
                ModifiedDate = new DateTime(2015, 11, 9),
                ModifiedBy = 70,
                Email = null
            });
            _channelsOperatorInDatabase.Add(new ChannelOperator()
            {
                Channel_UID = 83,
                CNPJ = "08.528.647/0001-00",
                CreditUsed = null,
                Address1 = "Rua Deodoro Gonçalves, 19",
                Address2 = null,
                PostalCode = "20040-001",
                Country_UID = 157,
                State_UID = 7466773,
                City_UID = 1797162,
                CreatedDate = new DateTime(2013, 12, 11),
                CreatedBy = 65,
                ModifiedDate = new DateTime(2015, 11, 9),
                ModifiedBy = 70,
                Email = "lenia.andrade@visualforma.pt111"
            });
        }
        #endregion


        #region Test List ChannelsProperty
        [TestMethod]
        [TestCategory("Channels")]
        public void Test_ChannelsProperty_ListAll()
        {
            FillTheChannelsPropertyforTest();

            int totalRecords = -1;
            _channelsPropertyRepoMock.Setup(x => x.FindChannelsPropertyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_channelsPropertyInDatabase);

            var response = ChannelsManagerPOCO.ListChannelsProperty(new Contracts.Requests.ListChannelsPropertyRequest
            {
                PageIndex = 1
            });

            //Response should contain only 6 result
            Assert.IsTrue(response.Result.Count == 6, "Expected result count = 6");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListChannelsPropertyResponse), "Expected Response Type = ListChannelsPropertyResponse");
            Assert.IsTrue(response.Result.First().UID == 1, "Expected UID = 1");
            Assert.IsTrue(response.Result.Last().UID == 6, "Expected UID = 6");
        }

        [TestMethod]
        [TestCategory("Channels")]
        public void Test_ChannelsProperty_ByPropertyByEnabledList()
        {
            FillTheChannelsPropertyforTest();

            int totalRecords = -1;
            _channelsPropertyRepoMock.Setup(x => x.FindChannelsPropertyByCriteria(out totalRecords, It.IsAny<List<long>>(), It.Is<List<long>>(arg => arg.Count() == 1 && arg.First() == 1002), It.IsAny<List<long>>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.Is<bool?>(arg => arg.HasValue && arg.Value == true),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .Returns(_channelsPropertyInDatabase.Where(x => x.Property_UID == 1002 && x.Channel.Enabled == true));

            var response = ChannelsManagerPOCO.ListChannelsProperty(new Contracts.Requests.ListChannelsPropertyRequest
            {
                PageIndex = 1,
                PropertyUIDs = new List<long>(){ 1002 },
                IsEnabled = true
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListChannelsPropertyResponse), "Expected Response Type = ListChannelsPropertyResponse");
            Assert.IsTrue(response.Result.First().UID == 4, "Expected UID = 4");
            Assert.IsTrue(response.Result.Last().UID == 5, "Expected UID = 5");
        }
        #endregion

        #region Test List ChannelOperator
        [TestMethod]
        [TestCategory("Channels")]
        public void Test_ChannelOperator_ListAll()
        {
            FillTheChannelOperatorsforTest();

            int totalRecords = -1;
            _channelOperatorRepoMock.Setup(x => x.FindByCriteria(out totalRecords, It.IsAny<List<long>>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                            .Returns(_channelsOperatorInDatabase);

            var response = ChannelsManagerPOCO.ListChannelOperators(new Contracts.Requests.ListChannelOperatorsRequest
            {
                ChannelUIDs = null
            });

            //Response should contain more than 1 result
            Assert.IsTrue(response.Result.Count > 1, "Expected result count > 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListChannelOperatorsResponse), "Expected Response Type = ListChannelOperatorsResponse");
        }

        [TestMethod]
        [TestCategory("Channels")]
        public void Test_ChannelOperator_ByChannel()
        {
            FillTheChannelOperatorsforTest();

            var channelList = new List<long>(){ 80, 81};

            int totalRecords = -1;
            _channelOperatorRepoMock.Setup(x => x.FindByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 2 && arg.First() == 80 && arg.Last() == 81),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                            .Returns(_channelsOperatorInDatabase.Where(x=> channelList.Contains(x.Channel_UID)));

            var response = ChannelsManagerPOCO.ListChannelOperators(new Contracts.Requests.ListChannelOperatorsRequest
            {
                ChannelUIDs = channelList
            });

            //Response should contain only 2 result
            Assert.IsTrue(response.Result.Count == 2, "Expected result count = 2");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListChannelOperatorsResponse), "Expected Response Type = ListChannelOperatorsResponse");
        }

        [TestMethod]
        [TestCategory("Channels")]
        public void Test_ChannelOperator_ByChannelWithoutOperatorAssociate()
        {
            FillTheChannelOperatorsforTest();

            int totalRecords = -1;
            _channelOperatorRepoMock.Setup(x => x.FindByCriteria(out totalRecords, It.Is<List<long>>(arg => arg.Count == 1 && arg.First() == 1),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                            .Returns(_channelsOperatorInDatabase.Where(x=>x.Channel_UID == 1));

            var response = ChannelsManagerPOCO.ListChannelOperators(new Contracts.Requests.ListChannelOperatorsRequest
            {
                ChannelUIDs = new List<long>() { 1 }
            });

            //Response should contain only 0 results
            Assert.IsTrue(response.Result.Count == 0, "Expected result count = 0");

            //Assert Response Format
            Assert.IsTrue(response.Warnings.Count == 0, "Expected Warnings count = 0");
            Assert.IsTrue(response.Errors.Count == 0, "Expected Errors count = 0");
            Assert.IsTrue(response.Status == 0, "Expected Status = 0");
            Assert.IsTrue(response.GetType() == typeof(ListChannelOperatorsResponse), "Expected Response Type = ListChannelOperatorsResponse");
        }
        #endregion
    }
}

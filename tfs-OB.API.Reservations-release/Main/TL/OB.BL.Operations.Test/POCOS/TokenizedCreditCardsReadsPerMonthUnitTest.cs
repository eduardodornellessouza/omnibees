using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Operations.Impl;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class TokenizedCreditCardsReadsPerMonthUnitTest : UnitBaseTest
    {
        protected ITokenizedCreditCardsReadsPerMonthManagerPOCO _tokenizedCreditCardsReadsPerMonthManagerPOCO = null;

        // repos
        protected Mock<IRepositoryFactory> _repositoryFactory = null;
        protected Mock<ITokenizedCreditCardsReadsPerMonthRepository> _tokenizedCreditCardsReadsPerMonthRepoMock = null;

        // lists
        protected List<TokenizedCreditCardsReadsPerMonth> _tokenizedCreditCardsReadsPerMonthInDataBase = new List<TokenizedCreditCardsReadsPerMonth>();


        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            #region Repositories
            _repositoryFactory = new Mock<IRepositoryFactory>(MockBehavior.Default);
            _tokenizedCreditCardsReadsPerMonthRepoMock = new Mock<ITokenizedCreditCardsReadsPerMonthRepository>(MockBehavior.Default);
            #endregion

            #region Repository factory
            RepositoryFactoryMock.Setup(x => x.GetTokenizedCreditCardsReadsPerMonthRepository(It.IsAny<IUnitOfWork>())).Returns(_tokenizedCreditCardsReadsPerMonthRepoMock.Object);
            #endregion

            #region Container
            _tokenizedCreditCardsReadsPerMonthManagerPOCO = Container.Resolve<ITokenizedCreditCardsReadsPerMonthManagerPOCO>();
            #endregion


            FillTheListTokenizedCreditCardsReadsPerMonthForTest();
        }

        private void FillTheListTokenizedCreditCardsReadsPerMonthForTest()
        {
            _tokenizedCreditCardsReadsPerMonthInDataBase.Add(new TokenizedCreditCardsReadsPerMonth()
            {
                UID = 1,
                Property_UID = 1801,
                YearNr = 2016,
                MonthNr = 1,
                NrOfCreditCardReads = 1,
                LastModifiedDate = new DateTime()
            });
            _tokenizedCreditCardsReadsPerMonthInDataBase.Add(new TokenizedCreditCardsReadsPerMonth()
            {
                UID = 1,
                Property_UID = 1801,
                YearNr = 2016,
                MonthNr = 2,
                NrOfCreditCardReads = 1,
                LastModifiedDate = new DateTime()
            });

            _tokenizedCreditCardsReadsPerMonthInDataBase.Add(new TokenizedCreditCardsReadsPerMonth()
            {
                UID = 2,
                Property_UID = 1802,
                YearNr = 2016,
                MonthNr = 2,
                NrOfCreditCardReads = 2,
                LastModifiedDate = new DateTime()
            });

            _tokenizedCreditCardsReadsPerMonthInDataBase.Add(new TokenizedCreditCardsReadsPerMonth()
            {
                UID = 3,
                Property_UID = 1803,
                YearNr = 2016,
                MonthNr = 3,
                NrOfCreditCardReads = 3,
                LastModifiedDate = new DateTime()
            });


        }


        [TestMethod]
        [TestCategory("TokenizedCreditCardsReadsPerMonthManager")]
        public void Test_FindTokenizedCreditCardsReadsPerMonthByCriteria_All()
        {
            var findTokenizedCreditCardsReadsPerMonthByCriteriaRequest = new FindTokenizedCreditCardsReadsPerMonthByCriteriaRequest();

            int totalRecords = 4;
            _tokenizedCreditCardsReadsPerMonthRepoMock.Setup(x => x.FindTokenizedCreditCardsReadsPerMonthByCriteria(out totalRecords, null, null, null, null, 0, 0, false)).Returns(_tokenizedCreditCardsReadsPerMonthInDataBase.ToList());

            var response = _tokenizedCreditCardsReadsPerMonthManagerPOCO.FindTokenizedCreditCardsReadsPerMonthByCriteria(findTokenizedCreditCardsReadsPerMonthByCriteriaRequest);

            Assert.AreEqual(response.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(response.TotalRecords, 4);
        }


        [TestMethod]
        [TestCategory("TokenizedCreditCardsReadsPerMonthManager")]
        public void Test_FindTokenizedCreditCardsReadsPerMonthByCriteria_ByPropertyAndYearAndMonth()
        {
            var findTokenizedCreditCardsReadsPerMonthByCriteriaRequest = new FindTokenizedCreditCardsReadsPerMonthByCriteriaRequest();

            int totalRecords = 1;
            _tokenizedCreditCardsReadsPerMonthRepoMock.Setup(x => x.FindTokenizedCreditCardsReadsPerMonthByCriteria(out totalRecords, null, null, null, null, 0, 0, false))
                .Returns(_tokenizedCreditCardsReadsPerMonthInDataBase
                .Where(x => x.Property_UID == 1801 && x.YearNr == 2016 && x.MonthNr == 1).ToList());

            var response = _tokenizedCreditCardsReadsPerMonthManagerPOCO.FindTokenizedCreditCardsReadsPerMonthByCriteria(findTokenizedCreditCardsReadsPerMonthByCriteriaRequest);

            Assert.AreEqual(response.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(response.TotalRecords, 1);
        }


        [TestMethod]
        [TestCategory("TokenizedCreditCardsReadsPerMonthManager")]
        public void Test_FindTokenizedCreditCardsReadsPerMonthByCriteria_ByPropertyAndYearAndMonth_Empty()
        {
            var findTokenizedCreditCardsReadsPerMonthByCriteriaRequest = new FindTokenizedCreditCardsReadsPerMonthByCriteriaRequest();

            int totalRecords = 0;
            _tokenizedCreditCardsReadsPerMonthRepoMock.Setup(x => x.FindTokenizedCreditCardsReadsPerMonthByCriteria(out totalRecords, null, null, null, null, 0, 0, false))
                .Returns(_tokenizedCreditCardsReadsPerMonthInDataBase
                .Where(x => x.Property_UID == 1804 && x.YearNr == 2016 && x.MonthNr == 1).ToList());

            var response = _tokenizedCreditCardsReadsPerMonthManagerPOCO.FindTokenizedCreditCardsReadsPerMonthByCriteria(findTokenizedCreditCardsReadsPerMonthByCriteriaRequest);

            Assert.AreEqual(response.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(response.TotalRecords, 0);
        }

        [TestMethod]
        [TestCategory("TokenizedCreditCardsReadsPerMonthManager")]
        public void Test_IncrementTokenizedCreditCardsReadsPerMonthByCriteria_Update()
        {
            var incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest = new IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest
            {
                UID = 1,
                countsToIncrement = 1,
                time = DateTime.Now
            };

            _tokenizedCreditCardsReadsPerMonthRepoMock.Setup(x => x.IncrementTokenizedCreditCardsReadsPerMonthByCriteria(incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest))
                .Returns(1);

            var response = _tokenizedCreditCardsReadsPerMonthManagerPOCO.IncrementTokenizedCreditCardsReadsPerMonthByCriteria(incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest);

            Assert.AreEqual(response.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(response.Result, 1);
        }

        [TestMethod]
        [TestCategory("TokenizedCreditCardsReadsPerMonthManager")]
        public void Test_IncrementTokenizedCreditCardsReadsPerMonthByCriteria_Insert()
        {
            var incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest = new IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest
            {
                PropertyUID = 1801,
                YearNr = 2016,
                MonthNr = 1,
                countsToIncrement = 1,
                time = DateTime.Now
            };

            _tokenizedCreditCardsReadsPerMonthRepoMock.Setup(x => x.InsertTokenizedCreditCardsReadsPerMonthByCriteria(incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest))
                .Returns(1);

            var response = _tokenizedCreditCardsReadsPerMonthManagerPOCO.IncrementTokenizedCreditCardsReadsPerMonthByCriteria(incrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest);

            Assert.AreEqual(response.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(response.Result, 1);
        }
    }
}

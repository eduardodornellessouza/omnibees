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
using PaymentGatewaysLibrary.BrasPagGateway;
using OB.Domain.Reservations;
using PaymentGatewaysLibrary;
using OB.Domain.Properties;
using OB.BL.Contracts.Requests;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class SecurityManagerPOCOTest : UnitBaseTest
    {
        public ISecurityManagerPOCO SecurityManagerPOCO
        {
            get;
            set;
        }


        private List<TokenizedCreditCardsReadsPerMonth> _tokenizedCreditCardsReadsInDatabase = null;


        private Mock<IUnitOfWork> _unitOfWorkMock = null;

        private Mock<IBrasPag> _brasPagMock = null;
        private Mock<IRepository<ReservationPaymentDetail>> _reservationPaymentDetailRepo = null;

        private Mock<IPropertyRepository> _propertiesRepoMock = null;
        private Mock<ITokenizedCreditCardsReadsPerMonthRepository> _tokenizedCreditCardsReadsRepoMock = null;


        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();


            _tokenizedCreditCardsReadsInDatabase = new List<TokenizedCreditCardsReadsPerMonth>();

            
            //Mock Repository factory
            var repoFactoryMock = new Mock<IRepositoryFactory>();

            _brasPagMock = new Mock<IBrasPag>(MockBehavior.Default);
            
           
            //Mock IRepository<ReservationPaymentDetail>
            _reservationPaymentDetailRepo = new Mock<IRepository<ReservationPaymentDetail>>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetRepository<ReservationPaymentDetail>(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationPaymentDetailRepo.Object);

            // Mock Properties Repository
            _propertiesRepoMock = new Mock<IPropertyRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetPropertyRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_propertiesRepoMock.Object);

            // Mock TokenizedCreditCardsReadsPerMonthRepository Repository
            _tokenizedCreditCardsReadsRepoMock = new Mock<ITokenizedCreditCardsReadsPerMonthRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetTokenizedCreditCardsReadsPerMonthRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_tokenizedCreditCardsReadsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetRepository<TokenizedCreditCardsReadsPerMonth>(It.IsAny<IUnitOfWork>()))
                .Returns(_tokenizedCreditCardsReadsRepoMock.Object);


            _unitOfWorkMock = new Mock<IUnitOfWork>();

            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(_unitOfWorkMock.Object);

            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);
            
            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.SecurityManagerPOCO = this.Container.Resolve<ISecurityManagerPOCO>();
        }


        #region TestListDecryptedCreditCards
        [TestMethod]
        [TestCategory("Security")]
        public void TestListDecryptedCreditCards_WithToken()
        {
            // Arrange
            var token = Guid.NewGuid();
            long reservationUID = 3;

            _reservationPaymentDetailRepo.Setup(x => x.GetQuery()).Returns(new List<ReservationPaymentDetail>() { 
                new ReservationPaymentDetail() 
                {
                    OBTokenizationIsActive = true,
                    Reservation_UID = reservationUID, 
                    CreditCardToken = token.ToString(),
                    CVV = "MFrc1MrzrPWM9An4JiI40JBsSki87x3haiOw/fwe7onnL1vmR2sqcT5T1mTj9xpx6UWtjVlhVpvdxbotSsP2LROOvdk4p14AXleAOrThyHFcPdx2vEQKnJ9vM/pubDYxpyijMN924fFyf8Lf7ErbLzWtbsVjYPfSMlPnOE3aw90=" // 123
                }}.AsQueryable());

            _brasPagMock.Setup(x => x.GetCreditCard(It.IsAny<Guid>())).Returns(new PaymentGatewaysLibrary.BrasPagGateway.Classes.CreditCardResponse() 
            { 
                CardHolder = "HOLDER NAME",
                CardNumber = "123456789",
                CardExpiration = new DateTime(2050,01,01),
                MaskedCardNumber = "12345xxxx"
            });

            // TODO: DMARTINS tentar mockar o construtor de BrasPag para nao dar erro ao tentar obter o endpoint

            //// Act
            //var request = new OB.BL.Contracts.Requests.ListDecryptedCreditCardRequest()
            //{ 
            //    ReservationCardNumbersHashes = new Dictionary<long,string>()
            //};
            //request.ReservationCardNumbersHashes.Add(reservationUID, token.ToString());
            //var response = this.SecurityManagerPOCO.ListDecryptedCreditCards(request);

            //// Asserts
            //Assert.AreEqual(0, response.Errors.Count, "Response should not have Errors");
            //Assert.IsNotNull(response.DecryptedCards, "Decrypted cards should not be null");
            //Assert.AreEqual(1, response.DecryptedCards.Count, "Decrypted cards count must be 1");
            //Assert.IsTrue(response.DecryptedCards.ContainsKey(reservationUID), "Decrypted cards response should contains ReservationUID = " + reservationUID);

            //var resultCard = response.DecryptedCards[reservationUID];
            //Assert.AreEqual("HOLDER NAME", resultCard.CardHolder, "Card Holder is incorrect");
            //Assert.AreEqual("123456789", resultCard.CardNumber, "Card Number is incorrect");
            //Assert.AreEqual(0, new DateTime(2050, 01, 01).CompareTo(resultCard.CardExpiration), "Card Expiration is incorrect");
            //Assert.AreEqual("123", resultCard.CVV, "CVV is incorrect");
        }

        [TestMethod]
        [TestCategory("Security")]
        public void TestListDecryptedCreditCards_WithCardNumber()
        {
            // Arrange
            var cardNumber = "YUf07HyUBd5MaRogegUkRT23bG9JCt5RCEQS3EEWGiVB8s490pvp5UNox9s9BdTGAcVEu/WamTsE+WgCzvcH0F4nVTJq5pw+ZbywJMKRMyLGwHFPIIlHSKaQqGe+zoXFVI+XXCmhGRC6dwbk45ZSqD0hWCimXgFjM7/YoCCbVU4="; // 4485921881381340
            long reservationUID = 3;

            _reservationPaymentDetailRepo.Setup(x => x.GetQuery()).Returns(new List<ReservationPaymentDetail>() { 
                new ReservationPaymentDetail() 
                {
                    Reservation_UID = reservationUID, 
                    CardNumber = cardNumber,
                    CardName = "Card Name test",
                    ExpirationDate = new DateTime(2051,10,01),
                    CVV = "MFrc1MrzrPWM9An4JiI40JBsSki87x3haiOw/fwe7onnL1vmR2sqcT5T1mTj9xpx6UWtjVlhVpvdxbotSsP2LROOvdk4p14AXleAOrThyHFcPdx2vEQKnJ9vM/pubDYxpyijMN924fFyf8Lf7ErbLzWtbsVjYPfSMlPnOE3aw90=" // 123
                }}.AsQueryable());

            _brasPagMock.Setup(x => x.GetCreditCard(It.IsAny<Guid>())).Returns(new PaymentGatewaysLibrary.BrasPagGateway.Classes.CreditCardResponse());

            // TODO: DMARTINS tentar mockar o construtor de BrasPag para nao dar erro ao tentar obter o endpoint

            // Act
            //var request = new OB.BL.Contracts.Requests.ListDecryptedCreditCardRequest()
            //{
            //    ReservationCardNumbersHashes = new Dictionary<long, string>()
            //};
            //request.ReservationCardNumbersHashes.Add(reservationUID, cardNumber);
            //var response = this.SecurityManagerPOCO.ListDecryptedCreditCards(request);

            //// Asserts
            //Assert.AreEqual(0, response.Errors.Count, "Response should not have Errors");
            //Assert.IsNotNull(response.DecryptedCards, "Decrypted cards should not be null");
            //Assert.AreEqual(1, response.DecryptedCards.Count, "Decrypted cards count must be 1");
            //Assert.IsTrue(response.DecryptedCards.ContainsKey(reservationUID), "Decrypted cards response should contains ReservationUID = " + reservationUID);

            //var resultCard = response.DecryptedCards[reservationUID];
            //Assert.IsNotNull(resultCard, "The Credit Card must be diferent of null");
            //Assert.AreEqual("Card Name test", resultCard.CardHolder, "Card Holder is incorrect");
            //Assert.AreEqual("4485921881381340", resultCard.CardNumber, "Card Number is incorrect");
            //Assert.AreEqual(0, new DateTime(2051, 10, 01).CompareTo(resultCard.CardExpiration), "Card Expiration is incorrect");
            //Assert.AreEqual("123", resultCard.CVV, "CVV is incorrect");
        }

        [TestMethod]
        [TestCategory("Security")]
        public void TestListDecryptedCreditCards_WithInvalidToken()
        {
            // Arrange
            var token = Guid.NewGuid();
            long reservationUID = 3;

            _reservationPaymentDetailRepo.Setup(x => x.GetQuery()).Returns(new List<ReservationPaymentDetail>() { 
                new ReservationPaymentDetail() 
                {
                    OBTokenizationIsActive = true,
                    Reservation_UID = reservationUID, 
                    CreditCardToken = token.ToString(),
                    CVV = "MFrc1MrzrPWM9An4JiI40JBsSki87x3haiOw/fwe7onnL1vmR2sqcT5T1mTj9xpx6UWtjVlhVpvdxbotSsP2LROOvdk4p14AXleAOrThyHFcPdx2vEQKnJ9vM/pubDYxpyijMN924fFyf8Lf7ErbLzWtbsVjYPfSMlPnOE3aw90=" // 123
                }}.AsQueryable());

            _brasPagMock.Setup(x => x.GetCreditCard(It.IsAny<Guid>())).Returns(new PaymentGatewaysLibrary.BrasPagGateway.Classes.CreditCardResponse()
            {
                CardHolder = "HOLDER NAME",
                CardNumber = "123456789",
                CardExpiration = new DateTime(2050, 01, 01),
                MaskedCardNumber = "12345xxxx"
            });

            // TODO: DMARTINS tentar mockar o construtor de BrasPag para nao dar erro ao tentar obter o endpoint

            //// Act
            //var request = new OB.BL.Contracts.Requests.ListDecryptedCreditCardRequest()
            //{
            //    ReservationCardNumbersHashes = new Dictionary<long, string>()
            //};
            //request.ReservationCardNumbersHashes.Add(reservationUID, Guid.NewGuid().ToString());
            //var response = this.SecurityManagerPOCO.ListDecryptedCreditCards(request);

            //// Asserts
            //Assert.AreEqual(0, response.Errors.Count, "Response should not have Errors");
            //Assert.IsNotNull(response.DecryptedCards, "Decrypted cards should not be null");
            //Assert.AreEqual(1, response.DecryptedCards.Count, "Decrypted cards count must be 1");
            //Assert.IsTrue(response.DecryptedCards.ContainsKey(reservationUID), "Decrypted cards response should contains ReservationUID = " + reservationUID);

            //var resultCard = response.DecryptedCards[reservationUID];
            //Assert.IsNotNull(resultCard, "The Credit Card must be diferent of null");
            //Assert.IsNotNull(resultCard.Errors, "The Credit Card of ReservationUID = " + reservationUID + " must be 1 error");
            //Assert.AreEqual(1, resultCard.Errors.Count, "The Credit Card of ReservationUID = " + reservationUID + " must be 1 error");
            //Assert.IsTrue(string.IsNullOrEmpty(resultCard.CardHolder), "Card Holder must be null or empty");
            //Assert.IsTrue(string.IsNullOrEmpty(resultCard.CardNumber), "Card Number must be null or empty");
            //Assert.IsTrue(string.IsNullOrEmpty(resultCard.CVV), "CVV must be null or empty");
            //Assert.IsNull(resultCard.CardExpiration, "Card Expiration must be null");
        }

        [TestMethod]
        [TestCategory("Security")]
        public void TestListDecryptedCreditCards_WithInvalidCardNumber()
        {
            // Arrange
            var cardNumber = "YUf07HyUBd5MaRogegUkRT23bG9JCt5RCEQS3EEWGiVB8s490pvp5UNox9s9BdTGAcVEu/WamTsE+WgCzvcH0F4nVTJq5pw+ZbywJMKRMyLGwHFPIIlHSKaQqGe+zoXFVI+XXCmhGRC6dwbk45ZSqD0hWCimXgFjM7/YoCCbVU4="; // 4485921881381340
            long reservationUID = 3;

            _reservationPaymentDetailRepo.Setup(x => x.GetQuery()).Returns(new List<ReservationPaymentDetail>() { 
                new ReservationPaymentDetail() 
                {
                    Reservation_UID = reservationUID, 
                    CardNumber = cardNumber,
                    CVV = "MFrc1MrzrPWM9An4JiI40JBsSki87x3haiOw/fwe7onnL1vmR2sqcT5T1mTj9xpx6UWtjVlhVpvdxbotSsP2LROOvdk4p14AXleAOrThyHFcPdx2vEQKnJ9vM/pubDYxpyijMN924fFyf8Lf7ErbLzWtbsVjYPfSMlPnOE3aw90=" // 123
                }}.AsQueryable());

            _brasPagMock.Setup(x => x.GetCreditCard(It.IsAny<Guid>())).Returns(new PaymentGatewaysLibrary.BrasPagGateway.Classes.CreditCardResponse());

            // TODO: DMARTINS tentar mockar o construtor de BrasPag para nao dar erro ao tentar obter o endpoint

            //// Act
            //var request = new OB.BL.Contracts.Requests.ListDecryptedCreditCardRequest()
            //{
            //    ReservationCardNumbersHashes = new Dictionary<long, string>()
            //};
            //request.ReservationCardNumbersHashes.Add(reservationUID, "15");
            //var response = this.SecurityManagerPOCO.ListDecryptedCreditCards(request);

            //// Asserts
            //Assert.AreEqual(0, response.Errors.Count, "Response should not have Errors");
            //Assert.IsNotNull(response.DecryptedCards, "Decrypted cards should not be null");
            //Assert.AreEqual(1, response.DecryptedCards.Count, "Decrypted cards count must be 1");
            //Assert.IsTrue(response.DecryptedCards.ContainsKey(reservationUID), "Decrypted cards response should contains ReservationUID = " + reservationUID);

            //var resultCard = response.DecryptedCards[reservationUID];
            //Assert.IsNotNull(resultCard, "The Credit Card must be diferent of null");
            //Assert.IsNotNull(resultCard.Errors, "The Credit Card of ReservationUID = " + reservationUID + " must be 1 error");
            //Assert.AreEqual(1, resultCard.Errors.Count, "The Credit Card of ReservationUID = " + reservationUID + " must be 1 error");
            //Assert.IsTrue(string.IsNullOrEmpty(resultCard.CardHolder), "Card Holder must be null or empty");
            //Assert.IsTrue(string.IsNullOrEmpty(resultCard.CardNumber), "Card Number must be null or empty");
            //Assert.IsTrue(string.IsNullOrEmpty(resultCard.CVV), "CVV must be null or empty");
            //Assert.IsNull(resultCard.CardExpiration, "Card Expiration must be null");
        }
        #endregion


        #region TestListDecryptedCreditCards
        [TestMethod]
        [TestCategory("Security")]
        public void Test_Security_RegistTokenizedCreditCardsReadsRepo_InsertRegist()
        {
            DateTime now = DateTime.UtcNow;

            _propertiesRepoMock.Setup(x => x.ConvertToPropertyTimeZone(It.IsAny<long>(), It.IsAny<DateTime>())).Returns(now);

            _unitOfWorkMock.Setup(x => x.Save(It.IsAny<int?>())).Returns(1);
            

            var request = new RegistTokenizedCreditCardsReadsPerMonthRequest()
            {
                PropertyUIDs_NrReadsToIncrement = new Dictionary<long, long>() { { 1812, 1 } }
            };

            var response = SecurityManagerPOCO.RegistTokenizedCreditCardsReadsPerMonth(request);

            //Response should contain only one result
            Assert.IsTrue(response.Status == Status.Success, "Expected result Status == Success");
            Assert.IsTrue(response.PropertyUIDs_NrReadsToIncrement.Count == 1, "Expected PropertyUIDs_NrReadsToIncrement count = 1");
            Assert.IsTrue(response.PropertyUIDs_NrReadsToIncrement.FirstOrDefault().Key == 1812, "Expected PropertyUIDs_NrReadsToIncrement[1].Key = 1812");
            Assert.IsTrue(response.PropertyUIDs_NrReadsToIncrement.FirstOrDefault().Value == 1, "Expected PropertyUIDs_NrReadsToIncrement[1].Value = 1");
        }


        [TestMethod]
        [TestCategory("Security")]
        public void Test_Security_RegistTokenizedCreditCardsReadsRepo_UpdateRegist()
        {
            DateTime now = DateTime.UtcNow;


            _tokenizedCreditCardsReadsInDatabase.Add(new TokenizedCreditCardsReadsPerMonth()
            {
                UID = 1,
                Property_UID = 1812,
                YearNr = now.Year,
                MonthNr = (short)now.Month,
                LastModifiedDate = DateTime.UtcNow.AddMinutes(-5),
                NrOfCreditCardReads = 1
            });


            _propertiesRepoMock.Setup(x => x.ConvertToPropertyTimeZone(It.IsAny<long>(), It.IsAny<DateTime>())).Returns(now);

            _unitOfWorkMock.Setup(x => x.Save(It.IsAny<int?>())).Returns(1);

            _tokenizedCreditCardsReadsRepoMock.Setup(x => x.FirstOrDefault(It.IsAny<Expression<Func<TokenizedCreditCardsReadsPerMonth, bool>>>())).Returns(_tokenizedCreditCardsReadsInDatabase.First());

            _tokenizedCreditCardsReadsRepoMock.Setup(x => x.IncrementTokenizedCreditCardsReadsPerMonthByCriteria(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>())).Returns(2);

            var request = new RegistTokenizedCreditCardsReadsPerMonthRequest()
            {
                PropertyUIDs_NrReadsToIncrement = new Dictionary<long, long>() { { 1812, 1 } }
            };

            var response = SecurityManagerPOCO.RegistTokenizedCreditCardsReadsPerMonth(request);

            //Response should contain only one result
            Assert.IsTrue(response.Status == Status.Success, "Expected result Status == Success");
            Assert.IsTrue(response.PropertyUIDs_NrReadsToIncrement.Count == 1, "Expected PropertyUIDs_NrReadsToIncrement count = 1");
            Assert.IsTrue(response.PropertyUIDs_NrReadsToIncrement.FirstOrDefault().Key == 1812, "Expected PropertyUIDs_NrReadsToIncrement[1].Key = 1812");
            Assert.IsTrue(response.PropertyUIDs_NrReadsToIncrement.FirstOrDefault().Value == 2, "Expected PropertyUIDs_NrReadsToIncrement[1].Value = 2");
        }



        [TestMethod]
        [TestCategory("Security")]
        public void Test_Security_RegistTokenizedCreditCardsReadsRepo_Error()
        {
            DateTime now = DateTime.UtcNow;


            var request = new RegistTokenizedCreditCardsReadsPerMonthRequest()
            {
                PropertyUIDs_NrReadsToIncrement = new Dictionary<long, long>() { { 1812, 0 } }
            };

            var response = SecurityManagerPOCO.RegistTokenizedCreditCardsReadsPerMonth(request);

            //Response should contain only one result
            Assert.IsTrue(response.Status == Status.Fail, "Expected result Status == Fail");
            Assert.IsTrue(response.Errors.Count == 1, "Expected Errors count = 1");
            Assert.IsTrue(response.Errors.First().ErrorType == ErrorType.InvalidRequest, "Expected Error == InvalidRequest");
        }
        #endregion
    }
}

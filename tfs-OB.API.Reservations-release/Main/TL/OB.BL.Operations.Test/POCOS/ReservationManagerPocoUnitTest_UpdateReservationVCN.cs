using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Criteria;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.Reservation.BL.Contracts.Responses;
using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Linq;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OBRequests = OB.BL.Contracts.Requests;
using System.Collections.Generic;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class ReservationManagerPocoUnitTest_UpdateReservationVCN : UnitBaseTest
    {
        private Mock<IReservationsRepository> _reservationRepoMock;
        private Mock<IOBPropertyRepository> _propertyRepoMock;
        private Mock<IOBSecurityRepository> _securityRepoMock;
        private IReservationManagerPOCO _reservationPoco;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _reservationRepoMock = new Mock<IReservationsRepository>();
            _propertyRepoMock = new Mock<IOBPropertyRepository>();
            _securityRepoMock = new Mock<IOBSecurityRepository>();

            RepositoryFactoryMock.Setup(x => x.GetReservationsRepository(It.IsAny<IUnitOfWork>())).Returns(_reservationRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBPropertyRepository()).Returns(_propertyRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBSecurityRepository()).Returns(_securityRepoMock.Object);

            _reservationPoco = this.Container.Resolve<IReservationManagerPOCO>();
        }

        // TODO: DMARTINS 2019-10-30 validar os campos de input ao gravar cartao

        #region Mock and verify methods

        private UpdateReservationVcnCriteria _repoMethodInput = null;
        private void MockUpdateReservationVcnRepoMethod(int resultsMock)
        {
            _reservationRepoMock.Setup(x => x.UpdateReservationVcn(It.IsAny<UpdateReservationVcnCriteria>()))
                .Callback<UpdateReservationVcnCriteria>(x => _repoMethodInput = x)
                .Returns(resultsMock);
        }

        private void VerifyUpdateReservationVcn(int hits)
        {
            _reservationRepoMock.Verify(x => x.UpdateReservationVcn(It.IsAny<UpdateReservationVcnCriteria>()), Times.Exactly(hits));
        }

        private long _resRepoGetPropertyInput;
        private void MockGetPropertyIdByReservationId(long propertyId)
        {
            _reservationRepoMock.Setup(x => x.GetPropertyIdByReservationId(It.IsAny<long>()))
                .Callback<long>(x => _resRepoGetPropertyInput = x)
                .Returns(propertyId);
        }

        private void VerifyGetPropertyIdByReservationId(int hits)
        {
            _reservationRepoMock.Verify(x => x.GetPropertyIdByReservationId(It.IsAny<long>()), Times.Exactly(hits));
        }

        private OBRequests.ListPropertySecurityConfigurationRequest _securityRepoInput;
        private void MockGetPropertySecurityConfiguration(List<Contracts.Data.Properties.PropertySecurityConfiguration> results)
        {
            _propertyRepoMock.Setup(x => x.GetPropertySecurityConfiguration(It.IsAny<OBRequests.ListPropertySecurityConfigurationRequest>()))
                .Callback<OBRequests.ListPropertySecurityConfigurationRequest>(x => _securityRepoInput = x)
                .Returns(results);
        }

        private void VerifyGetPropertySecurityConfiguration(int hits)
        {
            _propertyRepoMock.Verify(x => x.GetPropertySecurityConfiguration(It.IsAny<OBRequests.ListPropertySecurityConfigurationRequest>()), Times.Exactly(hits));
        }

        private OBRequests.ListCreditCardRequest _securityRepoEncrypInput = null;
        private void MockEncryptCreditCards(List<string> numbers)
        {
            _securityRepoMock.Setup(x => x.EncryptCreditCards(It.IsAny<OBRequests.ListCreditCardRequest>()))
                .Callback<OBRequests.ListCreditCardRequest>(x => _securityRepoEncrypInput = x)
                .Returns(numbers);
        }

        private void VerifyEncryptCreditCards(int hits)
        {
            _securityRepoMock.Verify(x => x.EncryptCreditCards(It.IsAny<OBRequests.ListCreditCardRequest>()), Times.Exactly(hits));
        }

        private OBRequests.GetCreditCardHashRequest _securityRepoCardHashInput;
        private void MockGetCreditCardHash(byte[] hashCode)
        {
            _securityRepoMock.Setup(x => x.GetCreditCardHash(It.IsAny<OBRequests.GetCreditCardHashRequest>()))
                .Callback<OBRequests.GetCreditCardHashRequest>(x => _securityRepoCardHashInput = x)
                .Returns(hashCode);
        }

        private void VerifyGetCreditCardHash(int hits)
        {
            _securityRepoMock.Verify(x => x.GetCreditCardHash(It.IsAny<OBRequests.GetCreditCardHashRequest>()), Times.Exactly(hits));
        }

        #endregion

        #region Negative Tests

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_NullRequest_StatusFailed()
        {
            // Assemble
            UpdateReservationVCNRequest request = null;

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.NoRequest, response.Errors[0].ErrorCode);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [DataTestMethod]
        [DataRow(0, DisplayName = "Reservation Id is zero")]
        [DataRow(-1, DisplayName = "Reservation Id is negative")]
        [DataRow(long.MinValue, DisplayName = "Reservation Id is min long value")]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_InvalidParameter_ReservationId_StatusFailed(long reservationId)
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = reservationId,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN Token unit test",
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.ReservationDoesNotExist, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_VcnReservationId_MaxLenghtExceeded_StatusFailed()
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = new string('r', 101),
                VcnTokenId = "VCN Token unit test",
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.ParameterMaxLengthExceeded, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [DataTestMethod]
        [DataRow(null, DisplayName = "Null VcnReservationId")]
        [DataRow("", DisplayName = "Empty VcnReservationId")]
        [DataRow(" ", DisplayName = "Whitespace VcnReservationId")]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_NullOrEmpty_VcnReservationId_StatusFailed(string vcnResId)
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = vcnResId,
                VcnTokenId = "VCN Token unit test",
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.RequiredParameter, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_VcnToken_MaxLenghtExceeded_StatusFailed()
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = new string('t', 1001),
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.ParameterMaxLengthExceeded, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [DataTestMethod]
        [DataRow(null, DisplayName = "Null VcnTokenId")]
        [DataRow("", DisplayName = "Empty VcnTokenId")]
        [DataRow(" ", DisplayName = "Whitespace VcnTokenId")]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_NullOrEmpty_VcnToken_StatusFailed(string vcnToken)
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = vcnToken,
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.RequiredParameter, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [DataTestMethod]
        [DataRow(null, DisplayName = "Null CreditCardHolderName")]
        [DataRow("", DisplayName = "Empty CreditCardHolderName")]
        [DataRow(" ", DisplayName = "Whitespace CreditCardHolderName")]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_NullOrEmpty_CreditCardHolderName_StatusFailed(string cardHolderName)
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN token unit test",
                CreditCardHolderName = cardHolderName,
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.RequiredParameter, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [DataTestMethod]
        [DataRow(null, DisplayName = "Null CreditCardNumber")]
        [DataRow("", DisplayName = "Empty CreditCardNumber")]
        [DataRow(" ", DisplayName = "Whitespace CreditCardNumber")]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_NullOrEmpty_CreditCardNumber_StatusFailed(string cardNumber)
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN token unit test",
                CreditCardHolderName = "CC Holder Name unit test",
                CreditCardNumber = cardNumber,
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.RequiredParameter, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_Invalid_CreditCardExpirationDate_StatusFailed()
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN token unit test",
                CreditCardHolderName = "CC Holder Name unit test",
                CreditCardNumber = "CC Number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = default(DateTime)
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.RequiredParameter, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [DataTestMethod]
        [DataRow(null, DisplayName = "Null CreditCardCVV")]
        [DataRow("", DisplayName = "Empty CreditCardCVV")]
        [DataRow(" ", DisplayName = "Whitespace CreditCardCVV")]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_NullOrEmpty_CreditCardCVV_StatusFailed(string cvv)
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN token unit test",
                CreditCardHolderName = "CC Holder Name unit test",
                CreditCardNumber = "CC Number unit test",
                CreditCardCVV = cvv,
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.RequiredParameter, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_InvalidParameter_All_StatusFailed()
        {
            // Assemble
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = -1,
                VcnReservationId = null,
                VcnTokenId = null,
                CreditCardHolderName = null,
                CreditCardNumber = null,
                CreditCardCVV = null,
                CreditCardExpirationDate = default(DateTime)
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            var expectedErrors = new int[] { 
                (int)Errors.ReservationDoesNotExist, 
                (int)Errors.RequiredParameter,
                (int)Errors.RequiredParameter,
                (int)Errors.RequiredParameter,
                (int)Errors.RequiredParameter,
                (int)Errors.RequiredParameter,
                (int)Errors.RequiredParameter}.ToArray();
            var actualErrors = response.Errors.Select(x => x.ErrorCode).ToArray();
            CollectionAssert.AreEquivalent(expectedErrors, actualErrors);
            
            VerifyGetPropertyIdByReservationId(0);
            VerifyGetPropertySecurityConfiguration(0);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [DataTestMethod]
        [DataRow(true, 0, DisplayName = "Null encrypted numbers")]
        [DataRow(false, 0, DisplayName = "Zero encrypted numbers")]
        [DataRow(false, 1, DisplayName = "One encrypted numbers")]
        [DataRow(false, 3, DisplayName = "Three encrypted numbers")]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_EncryptCreditCard_StatusFailed(bool isNullEncryptedResults, int encryptedNumbersCount)
        {
            // Assemble
            MockGetPropertyIdByReservationId(123);
            MockGetPropertySecurityConfiguration(new List<Contracts.Data.Properties.PropertySecurityConfiguration>
            {
                new Contracts.Data.Properties.PropertySecurityConfiguration
                {
                    IsProtectedWithOmnibees = false
                }
            });
            MockEncryptCreditCards(isNullEncryptedResults ? null : Enumerable.Repeat("encrypted number", encryptedNumbersCount).ToList());
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN token unit test",
                CreditCardHolderName = "CC Holder Name unit test",
                CreditCardNumber = "CC Number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.DefaultError, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            Assert.IsNotNull(_securityRepoEncrypInput);
            Assert.IsNotNull(_securityRepoEncrypInput.CreditCards);
            Assert.AreEqual(2, _securityRepoEncrypInput.CreditCards.Count);
            CollectionAssert.AreEquivalent(new string[] { request.CreditCardNumber, request.CreditCardCVV }, _securityRepoEncrypInput.CreditCards.ToArray());

            VerifyGetPropertyIdByReservationId(1);
            VerifyGetPropertySecurityConfiguration(1);
            VerifyEncryptCreditCards(1);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_ReservationNotFound_StatusFailed()
        {
            // Assemble
            MockGetPropertyIdByReservationId(123);
            MockGetPropertySecurityConfiguration(new List<Contracts.Data.Properties.PropertySecurityConfiguration>
            {
                new Contracts.Data.Properties.PropertySecurityConfiguration
                {
                    IsProtectedWithOmnibees = false
                }
            });
            MockEncryptCreditCards(new List<string> { "cc number encrypted", "cvv encrypted" });
            MockGetCreditCardHash(new byte[] { 1, 2, 3, 4, 5 });
            MockUpdateReservationVcnRepoMethod(-1);
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN Token unit test",
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Success, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.IsNotNull(response.Warnings);
            Assert.AreEqual(1, response.Warnings.Count);
            Assert.AreEqual((int)Errors.ReservationDoesNotExist, response.Warnings[0].WarningCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            Assert.IsNotNull(_repoMethodInput);
            Assert.AreEqual(request.ReservationId, _repoMethodInput.ReservationId);
            Assert.AreEqual(request.VcnReservationId, _repoMethodInput.VcnReservationId);
            Assert.AreEqual(request.VcnTokenId, _repoMethodInput.VcnToken);

            VerifyGetPropertyIdByReservationId(1);
            VerifyGetPropertySecurityConfiguration(1);
            VerifyEncryptCreditCards(1);
            VerifyGetCreditCardHash(1);
            VerifyUpdateReservationVcn(1);
        }

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_SomethingWrongWithDB_StatusFailed()
        {
            // Assemble
            MockGetPropertyIdByReservationId(123);
            MockGetPropertySecurityConfiguration(new List<Contracts.Data.Properties.PropertySecurityConfiguration>
            {
                new Contracts.Data.Properties.PropertySecurityConfiguration
                {
                    IsProtectedWithOmnibees = false
                }
            });
            MockEncryptCreditCards(new List<string> { "cc number encrypted", "cvv encrypted" });
            MockGetCreditCardHash(new byte[] { 1, 2, 3, 4, 5 });
            MockUpdateReservationVcnRepoMethod(4);
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN Token unit test",
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };
            
            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.DefaultError, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            Assert.IsNotNull(_repoMethodInput);
            Assert.AreEqual(request.ReservationId, _repoMethodInput.ReservationId);
            Assert.AreEqual(request.VcnReservationId, _repoMethodInput.VcnReservationId);
            Assert.AreEqual(request.VcnTokenId, _repoMethodInput.VcnToken);

            VerifyGetPropertyIdByReservationId(1);
            VerifyGetPropertySecurityConfiguration(1);
            VerifyEncryptCreditCards(1);
            VerifyGetCreditCardHash(1);
            VerifyUpdateReservationVcn(1);
        }

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_ThrowExceptionDuringExecution_StatusFailed()
        {
            // Assemble
            long resId = 10;
            string vcnResId = new string('r', 10);
            string vcnToken = new string('t', 20);
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = resId,
                VcnReservationId = vcnResId,
                VcnTokenId = vcnToken,
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };
            _reservationRepoMock.Setup(x => x.GetPropertyIdByReservationId(It.IsAny<long>()))
                .Throws(new InsufficientMemoryException());

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Fail, response.Status);
            Assert.IsNotNull(response.Errors);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual((int)Errors.DefaultError, response.Errors[0].ErrorCode);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
        }

        #endregion

        #region Positive Tests

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_NullOrEmpty_PropertySecurityConfiguration_StatusSuccess(bool isNull)
        {
            // Assemble
            long expectedPropertyId = 123;
            MockGetPropertyIdByReservationId(expectedPropertyId);
            MockGetPropertySecurityConfiguration(isNull ? null : new List<Contracts.Data.Properties.PropertySecurityConfiguration>());
            MockEncryptCreditCards(new List<string> { "cc number encrypted", "cvv encrypted" });
            MockGetCreditCardHash(new byte[] { 1, 2, 3, 4, 5 });
            MockUpdateReservationVcnRepoMethod(2);
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN token unit test",
                CreditCardHolderName = "CC Holder Name unit test",
                CreditCardNumber = "CC Number unit test",
                CreditCardCVV = "CC CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Success, response.Status);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(0, response.Warnings.Count);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            VerifyGetPropertyIdByReservationId(1);
            VerifyGetPropertySecurityConfiguration(1);
            VerifyEncryptCreditCards(1);
            VerifyGetCreditCardHash(1);
            VerifyUpdateReservationVcn(1);
        }

        [TestMethod]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_PropertySecurityConfiguration_ProtectedWithOmnibees_IngoreUpdate_StatusSucess()
        {
            // Assemble
            MockGetPropertyIdByReservationId(1234);
            MockGetPropertySecurityConfiguration(new List<Contracts.Data.Properties.PropertySecurityConfiguration>
            {
                new Contracts.Data.Properties.PropertySecurityConfiguration
                {
                    IsProtectedWithOmnibees = true
                }
            });
            MockEncryptCreditCards(new List<string> { "cc number encrypted", "cvv encrypted" });
            MockGetCreditCardHash(new byte[] { 1, 2, 3, 4, 5 });
            MockUpdateReservationVcnRepoMethod(2);
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN token unit test",
                CreditCardHolderName = "CC Holder Name unit test",
                CreditCardNumber = "CC Number unit test",
                CreditCardCVV = "CC CVV unit test",
                CreditCardExpirationDate = DateTime.UtcNow
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Success, response.Status);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(1, response.Warnings.Count);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            VerifyGetPropertyIdByReservationId(1);
            VerifyGetPropertySecurityConfiguration(1);
            VerifyEncryptCreditCards(0);
            VerifyGetCreditCardHash(0);
            VerifyUpdateReservationVcn(0);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(15)]
        [DataRow(31)]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_CreditCardExpirationDate_DayIgnored_StatusSucess(int day)
        {
            // Assemble
            MockGetPropertyIdByReservationId(1234);
            MockGetPropertySecurityConfiguration(new List<Contracts.Data.Properties.PropertySecurityConfiguration>
            {
                new Contracts.Data.Properties.PropertySecurityConfiguration
                {
                    IsProtectedWithOmnibees = false
                }
            });
            MockEncryptCreditCards(new List<string> { "cc number encrypted", "cvv encrypted" });
            MockGetCreditCardHash(new byte[] { 1, 2, 3, 4, 5 });
            MockUpdateReservationVcnRepoMethod(2);
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = "VCN ResId unit test",
                VcnTokenId = "VCN token unit test",
                CreditCardHolderName = "CC Holder Name unit test",
                CreditCardNumber = "CC Number unit test",
                CreditCardCVV = "CC CVV unit test",
                CreditCardExpirationDate = new DateTime(2050, 10, day)
            };

            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Success, response.Status);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);
            Assert.AreEqual(31, _repoMethodInput.CreditCardExpirationDate.Day);

            VerifyGetPropertyIdByReservationId(1);
            VerifyGetPropertySecurityConfiguration(1);
            VerifyEncryptCreditCards(1);
            VerifyGetCreditCardHash(1);
            VerifyUpdateReservationVcn(1);
        }

        [DataTestMethod]
        [DataRow(100, 1000, DisplayName = "VcnReservationId and VcnToken both equal to respective max size")]
        [DataRow(99, 999, DisplayName = "VcnReservationId and VcnToken both lower than respective max size")]
        [TestCategory("VirtualCardNumber")]
        public void Test_UpdateReservationVcn_StatusSuccess(int vcnResIdSize, int vcnTokenSize)
        {
            // Assemble
            long expectedPropertyId = 1234;
            string expectedEncryptedNumber = "cc number encrypted";
            string expectedEncryptedCvv = "cvv encrypted";
            byte[] expectedCardHashCode = new byte[] { 1, 2, 3, 4, 5 };
            MockGetPropertyIdByReservationId(expectedPropertyId);
            MockGetPropertySecurityConfiguration(new List<Contracts.Data.Properties.PropertySecurityConfiguration>
            {
                new Contracts.Data.Properties.PropertySecurityConfiguration
                {
                    IsProtectedWithOmnibees = false
                }
            });
            MockEncryptCreditCards(new List<string> { expectedEncryptedNumber, expectedEncryptedCvv });
            MockGetCreditCardHash(expectedCardHashCode);
            MockUpdateReservationVcnRepoMethod(2);
            var request = new UpdateReservationVCNRequest
            {
                RequestGuid = Guid.NewGuid(),
                RequestId = "unitTest",
                ReservationId = 10,
                VcnReservationId = new string('r', vcnResIdSize),
                VcnTokenId = new string('t', vcnTokenSize),
                CreditCardHolderName = "Holder Name unit test",
                CreditCardNumber = "CC number unit test",
                CreditCardCVV = "CVV unit test",
                CreditCardExpirationDate = new DateTime(2050, 10, 1)
            };
            
            // Act
            var response = _reservationPoco.UpdateReservationVCN(request);

            // Assert
            Assert.AreEqual(Status.Success, response.Status);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(request.RequestGuid, response.RequestGuid);
            Assert.AreEqual(request.RequestId, response.RequestId);

            Assert.AreEqual(request.ReservationId, _resRepoGetPropertyInput);

            Assert.AreEqual(1, _securityRepoInput.PropertyUIDs?.Count);
            Assert.AreEqual(expectedPropertyId, _securityRepoInput.PropertyUIDs[0]);

            CollectionAssert.AreEquivalent(new string[] { request.CreditCardNumber, request.CreditCardCVV }, _securityRepoEncrypInput?.CreditCards?.ToArray());

            Assert.AreEqual(request.CreditCardHolderName, _securityRepoCardHashInput.CardHolder);
            Assert.AreEqual(expectedEncryptedNumber, _securityRepoCardHashInput.EncryptedCardNumber);
            Assert.AreEqual(expectedEncryptedCvv, _securityRepoCardHashInput.EncryptedCVV);
            Assert.AreEqual(new DateTime(2050, 10, 31), _securityRepoCardHashInput.CardExpiration);

            Assert.IsNotNull(_repoMethodInput);
            Assert.AreEqual(request.ReservationId, _repoMethodInput.ReservationId);
            Assert.AreEqual(request.VcnReservationId, _repoMethodInput.VcnReservationId);
            Assert.AreEqual(request.VcnTokenId, _repoMethodInput.VcnToken);
            Assert.AreEqual(request.CreditCardHolderName, _repoMethodInput.CreditCardHolderName);
            Assert.AreEqual(expectedEncryptedNumber, _repoMethodInput.CreditCardNumber);
            Assert.AreEqual(expectedEncryptedCvv, _repoMethodInput.CreditCardCVV);
            Assert.AreEqual(new DateTime(2050, 10, 31), _repoMethodInput.CreditCardExpirationDate);
            Assert.AreEqual(expectedCardHashCode, _repoMethodInput.CreditCardHashCode);
           
            VerifyGetPropertyIdByReservationId(1);
            VerifyGetPropertySecurityConfiguration(1);
            VerifyEncryptCreditCards(1);
            VerifyGetCreditCardHash(1);
            VerifyUpdateReservationVcn(1);
        }

        #endregion
    }
}
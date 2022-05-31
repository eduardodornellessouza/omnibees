using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using domainReservations = OB.Domain.Reservations;

namespace OB.BL.Operations.Test.ReservationValidator
{
    [TestClass]
    public partial class InitialReservationValidationUnitTest : UnitBaseTest
    {
        public IReservationValidatorPOCO ReservationValidatorPOCO { get; set; }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            SessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(UnitOfWorkMock.Object);
            SessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<DomainScope>(), It.IsAny<DomainScope>())).Returns(UnitOfWorkMock.Object);

            UnitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<DomainScope>(), It.IsAny<IsolationLevel>()))
                .Returns(new Mock<IDbTransaction>(MockBehavior.Default).Object);

            this.ReservationValidatorPOCO = this.Container.Resolve<IReservationValidatorPOCO>();

            FillGroupRulesMock();
        }

        [TestMethod]
        [TestCategory("InitialValidation")]
        public void Test_InitialValidation_WithoutGroupRule_Success()
        {
            Assert.IsNull(GetBusinessLayerExceptionToAssert());
        }

        [TestMethod]
        [TestCategory("InitialValidation")]
        public void Test_InitialValidation_WithGroupRule_NoFlagValidateReservation_Success()
        {
            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.BE);

            Assert.IsNull(GetBusinessLayerExceptionToAssert(groupRule));
        }

        [TestMethod]
        [TestCategory("InitialValidation")]
        public void Test_InitialValidation_WithGroupRule_FlagValidateReservation_Success()
        {
            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull );
            groupRule.BusinessRules &= ~BusinessRules.ValidateBookingWindow;

            Assert.IsNull(GetBusinessLayerExceptionToAssert(groupRule));
        }

        [TestMethod]
        [TestCategory("InitialValidation")]
        public void Test_InitialValidation_Call_ValidateRateChannelPaymentParcels_Invalid_ThrowException()
        {
            // Arrange
            var request = new InitialValidationRequest
            {
                GroupRule = new GroupRule { RuleType = RuleType.BE, BusinessRules = BusinessRules.ValidateReservation },
                RequestId = Guid.NewGuid().ToString()
            };

            var validatorMock = new Mock<IReservationValidatorPOCO>();
            validatorMock
                .Setup(x => x.ValidateRateChannelPaymentParcels(It.IsAny<InitialValidationRequest>()))
                .Returns(OB.Reservation.BL.Contracts.Responses.Errors.PaymentMethodNotSupportPartialPayments.ToBusinessLayerException());
            Container.RegisterInstance(validatorMock.Object);

            try
            {
                // Act
                var validator = new Impl.ReservationValidatorPOCO() { Container = Container };
                validator.InitialValidation(request);

                Assert.Fail("Should throw an exception");
            }
            catch (BusinessLayerException ex)
            {
                // Assert
                Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.PaymentMethodNotSupportPartialPayments, ex.ErrorCode);
                validatorMock.Verify(x => x.ValidateRateChannelPaymentParcels(It.Is<InitialValidationRequest>(req => req == request)), Times.Once);
            }
        }

        [TestMethod]
        [TestCategory("InitialValidation")]
        public void Test_InitialValidation_Call_ValidateRateChannelPaymentParcels_Valid_NotThrowException()
        {
            // Arrange
            var request = new InitialValidationRequest
            {
                GroupRule = new GroupRule { RuleType = RuleType.BE, BusinessRules = BusinessRules.ValidateReservation },
                RequestId = Guid.NewGuid().ToString()
            };

            var validatorMock = new Mock<IReservationValidatorPOCO>();
            validatorMock
                .Setup(x => x.ValidateRateChannelPaymentParcels(It.IsAny<InitialValidationRequest>()))
                .Returns(default(BusinessLayerException));
            Container.RegisterInstance(validatorMock.Object);

            // Act
            var validator = new Impl.ReservationValidatorPOCO() { Container = Container };
            validator.InitialValidation(request);

            // Assert
            validatorMock.Verify(x => x.ValidateRateChannelPaymentParcels(It.Is<InitialValidationRequest>(req => req == request)), Times.Once);
        }

        private BusinessLayerException GetBusinessLayerExceptionToAssert(GroupRule groupRule = null)
        {
            BusinessLayerException exception = null;
            try
            {
                ReservationValidatorPOCO.InitialValidation(new InitialValidationRequest
                {
                    GroupRule = groupRule,
                    RequestId = Guid.NewGuid().ToString()
                });
            }
            catch (BusinessLayerException ex)
            {
                exception = ex;
            }

            return exception;
        }
    }
}

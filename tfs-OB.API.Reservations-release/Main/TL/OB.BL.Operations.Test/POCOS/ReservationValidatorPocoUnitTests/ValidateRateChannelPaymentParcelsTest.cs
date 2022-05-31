using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Operations.Impl;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OB.BL.Operations.Test.ReservationValidator
{
    [TestClass]
    public class ValidateRateChannelPaymentParcelsTest : UnitBaseTest
    {
        private Mock<IOBRateRepository> _rateRepoMock;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _rateRepoMock = new Mock<IOBRateRepository>();
            RepositoryFactoryMock.Setup(x => x.GetOBRateRepository()).Returns(_rateRepoMock.Object);
        }

        #region Setup
        
        private IReservationValidatorPOCO GetReservationValidatorPoco()
        {
            return new ReservationValidatorPOCO { Container = Container, RepositoryFactory = RepositoryFactoryMock.Object };
        }

        private void Setup_RateRepo_ListRateChannelsDetails(List<Contracts.Data.Rates.RateChannel> mockedResult)
        {
            _rateRepoMock
                .Setup(x => x.ListRateChannelsDetails(It.IsAny<Contracts.Requests.ListRateChannelsRequest>()))
                .Returns(mockedResult);
        }

        private void Verify_RateRepo_ListRateChannelsDetails(Expression<Func<Contracts.Requests.ListRateChannelsRequest, bool>> match, int hits)
        {
            _rateRepoMock.Verify(x => x.ListRateChannelsDetails(It.Is(match)), Times.Exactly(hits));
        }

        #endregion

        #region Tests

        [DataRow(true, DisplayName = "Null GroupRule")]
        [DataRow(false, DisplayName = "Empty GroupRule")]
        [DataTestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_BusinnessRuleNotMatch_Valid(bool isNull)
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = isNull ? null : new OB.Domain.Reservations.GroupRule
                {
                    BusinessRules = default(OB.Domain.Reservations.BusinessRules),
                },
            };

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNull(exception);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 0);
        }

        [DataRow(true, false, DisplayName = "Null Reservation")]
        [DataRow(false, true, DisplayName = "Null ReservationRooms")]
        [DataTestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_NullReservationInfo_Valid(bool nullReservation, bool nullResRooms)
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = new OB.Domain.Reservations.GroupRule 
                {
                    BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateRateChannelPartialPayments,
                },
                Reservation = nullReservation ? null : new Reservation.BL.Contracts.Data.Reservations.Reservation(),
                ReservationRooms = nullResRooms ? null : new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoom>(),
                ReservationRoomDetails = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>(),
            };

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNull(exception);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 0);
        }

        [TestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_ChannelIsNotBookingEngine_Valid()
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = new OB.Domain.Reservations.GroupRule
                {
                    BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateRateChannelPartialPayments,
                },
                Reservation = new Reservation.BL.Contracts.Data.Reservations.Reservation 
                {
                    Channel_UID = Constants.BookingEngineChannelId + 10, // Not Booking Engine channel
                    PaymentMethodType_UID = (long)Constants.PaymentMethodTypesCode.CreditCard,
                    IsPartialPayment = true,
                },
                ReservationRooms = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoom>(),
                ReservationRoomDetails = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>(),
            };

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNull(exception);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 0);
        }

        [TestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_PaymentIsNotCreditCard_Valid()
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = new OB.Domain.Reservations.GroupRule
                {
                    BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateRateChannelPartialPayments,
                },
                Reservation = new Reservation.BL.Contracts.Data.Reservations.Reservation
                {
                    Channel_UID = Constants.BookingEngineChannelId,
                    PaymentMethodType_UID = (long)Constants.PaymentMethodTypesCode.CreditCard + 10, // Not Credit Card payment
                    IsPartialPayment = true,
                },
                ReservationRooms = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoom>(),
                ReservationRoomDetails = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>(),
            };

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNull(exception);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 0);
        }

        [DataRow(true, DisplayName = "IsPartialPayment = null")]
        [DataRow(false, DisplayName = "IsPartialPayment = false")]
        [DataTestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_ReservationHasNotPartialPayments_Valid(bool isNull)
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = new OB.Domain.Reservations.GroupRule
                {
                    BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateRateChannelPartialPayments,
                },
                Reservation = new Reservation.BL.Contracts.Data.Reservations.Reservation
                {
                    Channel_UID = Constants.BookingEngineChannelId,
                    PaymentMethodType_UID = (long)Constants.PaymentMethodTypesCode.CreditCard,
                    IsPartialPayment = isNull ? null : (bool?)false,
                },
                ReservationRooms = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoom>(),
                ReservationRoomDetails = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>(),
            };

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNull(exception);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 0);
        }

        [TestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_TestRateChannelsCriteria()
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = new OB.Domain.Reservations.GroupRule
                {
                    BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateRateChannelPartialPayments,
                },
                Reservation = new Reservation.BL.Contracts.Data.Reservations.Reservation
                {
                    Channel_UID = Constants.BookingEngineChannelId,
                    PaymentMethodType_UID = (long)Constants.PaymentMethodTypesCode.CreditCard,
                    IsPartialPayment = true,
                },
                ReservationRooms = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoom> 
                {
                    new Reservation.BL.Contracts.Data.Reservations.ReservationRoom { Rate_UID = 10, RoomType_UID = 100 },
                    new Reservation.BL.Contracts.Data.Reservations.ReservationRoom { Rate_UID = 11, RoomType_UID = 101 },
                    new Reservation.BL.Contracts.Data.Reservations.ReservationRoom { Rate_UID = 11, RoomType_UID = 102 },
                },
                ReservationRoomDetails = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>(),
            };

            Setup_RateRepo_ListRateChannelsDetails(new List<Contracts.Data.Rates.RateChannel> 
            {
                new Contracts.Data.Rates.RateChannel 
                { 
                    RateId = 10,
                    PaymentTypes = new List<Contracts.Data.Rates.LinkedRateChannelPaymentMethod> 
                    {
                        new Contracts.Data.Rates.LinkedRateChannelPaymentMethod 
                        { 
                            Key = (long)Constants.PaymentMethodTypesCode.CreditCard, 
                            BlockPartialPayments = false 
                        } 
                    }
                }
            });

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNull(exception);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 1);
            Verify_RateRepo_ListRateChannelsDetails(x => x.RequestId == request.RequestId, 1);
            Verify_RateRepo_ListRateChannelsDetails(x => x.PropertyIds.SetEquals(new HashSet<long> { request.Reservation.Property_UID }), 1);
            Verify_RateRepo_ListRateChannelsDetails(x => x.ChannelIds.SetEquals(new HashSet<long> { request.Reservation.Channel_UID.Value }), 1);
            Verify_RateRepo_ListRateChannelsDetails(x => x.RateIds.SetEquals(new HashSet<long> { 10, 11 }), 1);
            Verify_RateRepo_ListRateChannelsDetails(x => x.IncludePaymentMethodIds, 1);
            Verify_RateRepo_ListRateChannelsDetails(x => x.PageSize == -1, 1);
        }

        [TestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_NoRateChannelsFound_Invalid()
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = new OB.Domain.Reservations.GroupRule
                {
                    BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateRateChannelPartialPayments,
                },
                Reservation = new Reservation.BL.Contracts.Data.Reservations.Reservation
                {
                    Channel_UID = Constants.BookingEngineChannelId,
                    PaymentMethodType_UID = (long)Constants.PaymentMethodTypesCode.CreditCard,
                    IsPartialPayment = true,
                },
                ReservationRooms = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoom>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ReservationRoom { Rate_UID = 10, RoomType_UID = 100 },
                },
                ReservationRoomDetails = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>(),
            };

            Setup_RateRepo_ListRateChannelsDetails(new List<Contracts.Data.Rates.RateChannel>());

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual((int)Errors.PaymentMethodNotSupportPartialPayments, exception.ErrorCode);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 1);
        }

        [TestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_OneRateChannel_BlockPartialPaymentIsFalse_Valid()
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = new OB.Domain.Reservations.GroupRule
                {
                    BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateRateChannelPartialPayments,
                },
                Reservation = new Reservation.BL.Contracts.Data.Reservations.Reservation
                {
                    Channel_UID = Constants.BookingEngineChannelId,
                    PaymentMethodType_UID = (long)Constants.PaymentMethodTypesCode.CreditCard,
                    IsPartialPayment = true,
                },
                ReservationRooms = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoom>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ReservationRoom { Rate_UID = 10, RoomType_UID = 100 },
                },
                ReservationRoomDetails = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>(),
            };

            Setup_RateRepo_ListRateChannelsDetails(new List<Contracts.Data.Rates.RateChannel> 
            {
                new Contracts.Data.Rates.RateChannel 
                {
                    RateId = 10,
                    PaymentTypes = new List<Contracts.Data.Rates.LinkedRateChannelPaymentMethod>
                    {
                        new Contracts.Data.Rates.LinkedRateChannelPaymentMethod 
                        {
                            Key =  (long)Constants.PaymentMethodTypesCode.CreditCard,
                            BlockPartialPayments = false,
                        }
                    }
                }
            });

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNull(exception);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 1);
        }

        [TestMethod]
        [TestCategory("ValidateRateChannelPaymentParcels")]
        public void ValidateRateChannelPaymentParcels_TwoRateChannels_BlockPartialPaymentIsTrueInOne_Invalid()
        {
            // Arrange
            var request = new Internal.BusinessObjects.InitialValidationRequest
            {
                RequestId = "ut-request-id",
                GroupRule = new OB.Domain.Reservations.GroupRule
                {
                    BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateRateChannelPartialPayments,
                },
                Reservation = new Reservation.BL.Contracts.Data.Reservations.Reservation
                {
                    Channel_UID = Constants.BookingEngineChannelId,
                    PaymentMethodType_UID = (long)Constants.PaymentMethodTypesCode.CreditCard,
                    IsPartialPayment = true,
                },
                ReservationRooms = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoom>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ReservationRoom { Rate_UID = 10, RoomType_UID = 100 },
                    new Reservation.BL.Contracts.Data.Reservations.ReservationRoom { Rate_UID = 11, RoomType_UID = 101 },
                },
                ReservationRoomDetails = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>(),
            };

            Setup_RateRepo_ListRateChannelsDetails(new List<Contracts.Data.Rates.RateChannel>
            {
                new Contracts.Data.Rates.RateChannel
                {
                    RateId = 10,
                    PaymentTypes = new List<Contracts.Data.Rates.LinkedRateChannelPaymentMethod>
                    {
                        new Contracts.Data.Rates.LinkedRateChannelPaymentMethod
                        {
                            Key =  (long)Constants.PaymentMethodTypesCode.CreditCard,
                            BlockPartialPayments = false,
                        }
                    }
                },
                new Contracts.Data.Rates.RateChannel
                {
                    RateId = 11,
                    PaymentTypes = new List<Contracts.Data.Rates.LinkedRateChannelPaymentMethod>
                    {
                        new Contracts.Data.Rates.LinkedRateChannelPaymentMethod
                        {
                            Key =  (long)Constants.PaymentMethodTypesCode.CreditCard,
                            BlockPartialPayments = true,
                        }
                    }
                }
            });

            // Act
            var exception = GetReservationValidatorPoco().ValidateRateChannelPaymentParcels(request);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual((int)Errors.PaymentMethodNotSupportPartialPayments, exception.ErrorCode);
            Verify_RateRepo_ListRateChannelsDetails(x => true, 1);
        }
        
        #endregion
    }
}

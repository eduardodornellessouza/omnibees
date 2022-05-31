using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Contracts.Data.Channels;
using OB.BL.Contracts.Data.CRM;
using OB.BL.Contracts.Data.Payments;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.BL.Operations.Internal.TypeConverters;
using OB.BL.Operations.Test.Domain;
using OB.BL.Operations.Test.Domain.CRM;
using OB.BL.Operations.Test.Domain.Rates;
using OB.BL.Operations.Test.Helper;
using OB.DL.Common;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Domain;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Requests;
using OB.Services.IntegrationTests.Helpers;
using PaymentGatewaysLibrary;
using PaymentGatewaysLibrary.BrasPagGateway;
using PaymentGatewaysLibrary.BrasPagGateway.Classes;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using contractsCRM = OB.BL.Contracts.Data.CRM;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservation = OB.Domain.Reservations;
using System.Reflection;
using OB.BL.Operations.Impl;
using reservationRequest = OB.Reservation.BL.Contracts.Requests.InsertReservationRequest;

using Microsoft.Practices.Unity.InterceptionExtension;
using OB.BL.Operations.Internal.Behaviors;
using OB.BL.Operations.Extensions;
using Newtonsoft.Json;
using OB.DL.Common.Repositories.Interfaces.SqlServer;
using PaymentGatewaysLibrary.BrasPagGateway.ServicesV2.Interface;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class ReservationManagerPOCOBraspagTest : ReservationManagerPOCOUnitTestInitialize
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        #region Success

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_Success()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult { IsTransactionValid = true });
            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult { IsTransactionValid = true });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            //Asserts
            Assert.AreEqual("RES000024-1263", reservationResponse.Number, "Expected reservation number inserted");
            Assert.AreEqual(1, reservationResponse.Result, "Expected uid to be 1");
            Assert.AreEqual(OB.Reservation.BL.Constants.ReservationStatus.Booked, reservationResponse.ReservationStatus, "Expected booked reservation");
            Assert.AreEqual(0, reservationResponse.Errors.Count, "Expected no errors");
        }

        #endregion

        #region Failed on Authorize

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingReservationNumberOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                    PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError(nameof(AuthorizeRequest.OrderId))
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.Number)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation Number on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerNameOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.Name)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestFirstName)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestFirstName on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerEmailOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.Email)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestEmail)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestEmail on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressStreetOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressStreet)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestAddress1)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestAddress1 on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressZipCodeOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressZipCode)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestPostalCode)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestPostalCode on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressCityOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressCity)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestCity)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestPostalCode on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressStateOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressState)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestState_UID)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestState on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressCountryOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressCountry)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestCountry_UID)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestCountry on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardProviderOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Provider)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual("Provider", reservationResponse.Errors[0].Data.First().Value, "Expected Provider on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardNumberOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardNumber)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.CardNumber on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardYearOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardYear)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.ExpirationDate on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardMonthOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardMonth)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.ExpirationDate on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardHolderNameOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardHolderName)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardName)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.CardName on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardSecurityCodeOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardSecurityCode)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CVV)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.CVV on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardRequestEnumOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardRequestEnum)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.CardNumber on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingDeviceFingerPrintOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError(nameof(AuthorizeRequest.DeviceFingerPrintId))
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual(nameof(reservationRequest.AntifraudDeviceFingerPrintId), reservationResponse.Errors[0].Data.First().Value, "Expected AntifraudDeviceFingerPrintId on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingBrowserIpAddressOnGatewayRequest_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError( $"{nameof(AuthorizeRequest.AntiFraudRequest) + '.' + nameof(AuthorizeRequest.AntiFraudRequest.Browser) + '.' + nameof(AuthorizeRequest.AntiFraudRequest.Browser.IpAddress)}")
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.IPAddress)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation.IPAddress on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_InvalidGuestCityFormat()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.InvalidCustumerCityFormat.ToContractError()
                },
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString(),
                IsTransactionValid = false,
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult());
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidGuestCityFormat, reservationResponse.Errors[0].ErrorCode, "Expected InvalidGuestCityFormat error");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_InvalidGuestPhoneFormat()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.InvalidCustumerPhoneFormat.ToContractError(),
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult { PaymentGatewayTransactionID = "transactionId", IsTransactionValid = true });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidGuestPhoneFormat, reservationResponse.Errors[0].ErrorCode, "Expected InvalidGuestPhoneFormat error");
        }
        #endregion

        //This section is used to test when the Authorize succeeds and then the void fails.
        #region Failed on Void

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingReservationNumberOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError(nameof(AuthorizeRequest.OrderId))
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.Number)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation Number on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerNameOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.Name)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestFirstName)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestFirstName on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerEmailOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.Email)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestEmail)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestEmail on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressStreetOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressStreet)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestAddress1)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestAddress1 on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressZipCodeOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressZipCode)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestPostalCode)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestPostalCode on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressCityOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressCity)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestCity)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestPostalCode on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressStateOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressState)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestState_UID)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestState on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCustumerAddressCountryOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CustomerRequest) + '.' + nameof(AuthorizeRequest.CustomerRequest.AddressCountry)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.GuestCountry_UID)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation GuestCountry on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardProviderOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Provider)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual("Provider", reservationResponse.Errors[0].Data.First().Value, "Expected Provider on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardNumberOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardNumber)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.CardNumber on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardYearOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardYear)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.ExpirationDate on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardMonthOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardMonth)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.ExpirationDate on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardHolderNameOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardHolderName)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardName)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.CardName on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardSecurityCodeOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardSecurityCode)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CVV)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.CVV on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingCreditCardRequestEnumOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError($"{nameof(AuthorizeRequest.CreditCardPayment) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card) + '.' + nameof(AuthorizeRequest.CreditCardPayment.Card.CardRequestEnum)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.ReservationPaymentDetail) + '.' + nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}", reservationResponse.Errors[0].Data.First().Value, "Expected ReservationPaymentDetail.CardNumber on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingDeviceFingerPrintOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError(nameof(AuthorizeRequest.DeviceFingerPrintId))
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual(nameof(reservationRequest.AntifraudDeviceFingerPrintId), reservationResponse.Errors[0].Data.First().Value, "Expected AntifraudDeviceFingerPrintId on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_MissingBrowserIpAddressOnGatewayRequest_VoidFail_RequiredParameter()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.RequiredParameter.ToContractError( $"{nameof(AuthorizeRequest.AntiFraudRequest) + '.' + nameof(AuthorizeRequest.AntiFraudRequest.Browser) + '.' + nameof(AuthorizeRequest.AntiFraudRequest.Browser.IpAddress)}")
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.RequiredParameter, reservationResponse.Errors[0].ErrorCode, "Expected RequiredParameter error");
            Assert.AreEqual(1, reservationResponse.Errors[0].Data.Count, "Expected 1 data parameter");
            Assert.AreEqual($"{nameof(reservationRequest.Reservation) + '.' + nameof(reservationRequest.Reservation.IPAddress)}", reservationResponse.Errors[0].Data.First().Value, "Expected Reservation.IPAddress on data");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_VoidFail_InvalidGuestCityFormat()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.InvalidCustumerCityFormat.ToContractError()
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidGuestCityFormat, reservationResponse.Errors[0].ErrorCode, "Expected InvalidGuestCityFormat error");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservationWithBraspagGateway_VoidFail_InvalidGuestPhoneFormat()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(new PaymentMessageResult
            {
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                {
                        PaymentGatewaysLibrary.BrasPagGateway.Responses.Errors.InvalidCustumerPhoneFormat.ToContractError()
                },
                IsTransactionValid = false,
                PaymentGatewayTransactionStatusCode = ((int)PaymentGatewaysLibrary.BrasPagGateway.Constants.GeneralPaymentStatusCodes.Authorized).ToString()
            });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false
            });
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);


            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            _brasPag.Verify(x => x.Void(It.IsAny<VoidRequest>()), Times.Once());

            //Asserts
            Assert.AreEqual(null, reservationResponse.Number, "Expected no reservation inserted");
            Assert.AreEqual(-900, reservationResponse.Result, "Expected -900 (braspag) error code");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected 1 error");
            Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.InvalidGuestPhoneFormat, reservationResponse.Errors[0].ErrorCode, "Expected InvalidGuestPhoneFormat error");
        }
        #endregion

        protected OB.Reservation.BL.Contracts.Responses.InsertReservationResponse InsertReservationV2(out long errorCode, SearchBuilder searchBuilder, ReservationDataBuilder resBuilder, string transactionId = "",
           long propUid = 1806, bool withChildsRoom = true, long channelUid = 1, int numberOfAdults = 1,
           bool withNoRooms = false, bool emptyGuest = false, bool resIsEmpty = false, List<Inventory> inventories = null, PromotionalCode promoCode = null,
           long guestUid = 1, bool addTranslations = false, bool handleCancelationCosts = false, GroupRule groupRule = null, bool hasAges = true,
           List<int> ages = null, int numberOfRooms = 1, List<PropertyEventQR1> propertyEventQueryResultList = null, bool defineNewGuestUid = true,
           bool? hndCredit = null, bool validateAllot = false, bool onRequestEnable = false, bool skipInterestCalculation = false, int channelOperatorType = 0, int availabilityType = 0)
        {
            errorCode = -1;

            var channels = new List<long>() { channelUid };
            var childAges = ages == null && hasAges ? new List<int>() { 0, 2 } : ages;
            var currencies = new List<ChildTermsCurrency>() { new ChildTermsCurrency
                    {
                        Currency_UID = 34,
                        Value = 10
                    }};

            if (!resIsEmpty)
            {
                searchBuilder.AddRate(UID: rateUidMock);

                if (!withNoRooms)
                {
                    int i = numberOfRooms;
                    while (i <= 0)
                    {
                        searchBuilder.AddRoom(string.Empty, true, false, 20, 1, 0, 0, 20);
                        i--;
                    }
                }

                searchBuilder.AddRateRoomsAll().AddRateChannelsAll(channels);

                if (!withNoRooms)
                {
                    searchBuilder.AddSearchParameters(new SearchParameters
                    {
                        AdultCount = numberOfAdults,
                        ChildCount = 0,
                        CheckIn = DateTime.Now.AddDays(1),
                        CheckOut = DateTime.Now.AddDays(2)
                    });
                }

                if (withChildsRoom)
                {
                    searchBuilder.AddSearchParameters(new SearchParameters
                    {
                        AdultCount = 1,
                        ChildCount = 2,
                        CheckIn = DateTime.Now.AddDays(1),
                        CheckOut = DateTime.Now.AddDays(2),
                    })
                    .WithChildPrices(3, 50);
                }

                searchBuilder.AddPropertyChannels(channels)
                .WithAdultPrices(1, 100)
                .WithAllotment(100);

                if (promoCode != null)
                {
                    searchBuilder.AddPromoCode(propUid, searchBuilder.InputData.Rates[0].UID, promoCode.Code, promoCode.ValidFrom.GetValueOrDefault(), promoCode.ValidTo.GetValueOrDefault(), promoCode.DiscountValue, promoCode.IsPercentage)
                        .AddPromoCodeCurrencies(searchBuilder.InputData.PromoCode.UID, 34, promoCode.DiscountValue);
                }

                if (resBuilder.InputData.reservationRoomDetails != null && resBuilder.InputData.reservationRoomDetails
                    .Any(x => x.ReservationRoomDetailsAppliedIncentives != null && x.ReservationRoomDetailsAppliedIncentives.Any()))
                {
                    var incentives = resBuilder.InputData.reservationRoomDetails.SelectMany(x => x.ReservationRoomDetailsAppliedIncentives)
                        .GroupBy(x => x.Incentive_UID).Select(x => x.First()).ToList();
                    foreach (var inc in incentives)
                    {
                        searchBuilder.AddIncentive(propUid, searchBuilder.InputData.Rates[0].UID, inc);
                    }
                }

                searchBuilder.CreateRateRoomDetails();

                // Prices For The Modification
                searchBuilder.CreateRateRoomDetails(DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), 50);
            }

            //Mock here!!!
            MockSqlManager();
            MockReservationsRepo(searchBuilder, resBuilder, transactionId, inventories: inventories, addTranlations: addTranslations,
                hndCredit: hndCredit, onRequestEnable: onRequestEnable, channelOperatorType: channelOperatorType);
            MockPropertyEventsRepo(propertyEventQueryResultList);
            MockObPropertyRepo(resBuilder, availabilityType);
            MockReservationsRoomsRepo();
            MockResRoomDetailsRepo();
            MockResChildRepo(resBuilder);
            MockObRatesRepo(searchBuilder);
            MockObCurrenciesRepo();
            MockVisualStates();
            MockObChannelsRepo(resBuilder);
            MockReservationFiltersRepo();
            MockResAddDataRepo();
            MockAppRepository();
            MockChildTermsRepo(searchBuilder);
            MockTaxPoliciesRepo();
            MockPaymentdetailsGenericRepoMock(resBuilder, searchBuilder);
            MockPartialPaymentGenericRepoMock(resBuilder, searchBuilder);
            MockIncentives(searchBuilder);
            MockBuyerGroupRepo(searchBuilder, resBuilder);
            MockPromoCodesRepo(searchBuilder);
            MockExtrasRepo(resBuilder);
            MockObPaymentMethodRepo();
            MockSecurityRepo();
            MockReservationRoomsExtraRepoGeneric(resBuilder);
            MockRoomExtrasSchedulesGenericRepo(resBuilder);
            MockResPartialPayment();

            if (resBuilder.InputData.guest != null && defineNewGuestUid)
            {
                if (!resIsEmpty && !emptyGuest)
                    resBuilder.InputData.guest.UID = guestUid;
                if (resIsEmpty)
                    resBuilder.InputData.guest = null;
            }
            //--

            if (resBuilder.ExpectedData.reservationRoomExtras != null && resBuilder.ExpectedData.reservationRoomExtras.Any())
            {
                resBuilder.ExpectedData.reservationRoomExtras.ForEach(x =>
                {
                    x.ReservationRoom = resBuilder.ExpectedData.reservationRooms.Where(y => y.UID == x.ReservationRoom_UID).SingleOrDefault();
                    x.ReservationRoomExtrasSchedules = resBuilder.ExpectedData.reservationExtraSchedule;
                    x.ReservationRoomExtrasAvailableDates = resBuilder.ExpectedData.reservationRoomExtrasAvailableDates;
                });
                resBuilder.InputData.reservationRoomExtras.ForEach(x =>
                {
                    x.ReservationRoomExtrasAvailableDates = resBuilder.InputData.reservationRoomExtrasAvailableDates;
                });
            }

            //Add commission type automatically to the expected data
            if (searchBuilder.InputData.RateChannels != null && searchBuilder.InputData.RateChannels.Any())
            {
                var rates = searchBuilder.InputData.RateChannels.Select(y => ConvertRateChannel(y)).ToList();
                if (rates != null && rates.Any() && resBuilder.InputData.reservationDetail.Channel_UID != 1)
                {
                    if (resBuilder.ExpectedData.reservationRooms != null && resBuilder.ExpectedData.reservationRooms.Any())
                    {
                        resBuilder.ExpectedData.reservationRooms.ForEach(x =>
                        {
                            x.CommissionType = rates[0].RateModel_UID;
                            x.CommissionValue = rates[0].Value;
                        });
                    }
                }
            }

            #region Mock add guest special requests in the guest
            var listReqs = new List<long>();
            if (resBuilder.ExpectedData.reservationDetail.BESpecialRequests1_UID.HasValue)
                listReqs.Add(resBuilder.ExpectedData.reservationDetail.BESpecialRequests1_UID.Value);
            if (resBuilder.ExpectedData.reservationDetail.BESpecialRequests2_UID.HasValue)
                listReqs.Add(resBuilder.ExpectedData.reservationDetail.BESpecialRequests2_UID.Value);
            if (resBuilder.ExpectedData.reservationDetail.BESpecialRequests3_UID.HasValue)
                listReqs.Add(resBuilder.ExpectedData.reservationDetail.BESpecialRequests3_UID.Value);
            if (resBuilder.ExpectedData.reservationDetail.BESpecialRequests4_UID.HasValue)
                listReqs.Add(resBuilder.ExpectedData.reservationDetail.BESpecialRequests4_UID.Value);
            AddSpecialRequestToGuest(resBuilder, listReqs);
            #endregion

            var result = resBuilder.InsertReservationV2(resBuilder, Container, ReservationManagerPOCO, handleCancelationCosts, validateAllot, transactionId, groupRule, skipInterestCalculation);

            return result;
        }

        protected void GeneralSetUp(out long errorCode, out SearchBuilder builder, out ReservationDataBuilder resBuilder, out string transactionId)
        {
            errorCode = 0;

            // arrange
            //Prepare the tpi
            Domain.CRM.ThirdPartyIntermediary tpi = null;
            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            var tmpTpi = tpi.Clone();
            tmpTpi.Property_UID = 1263;
            tmpTpi.IsCompany = true;
            tpisList.Remove(tpi);
            tpisList.Add(tmpTpi);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            builder = new Operations.Helper.SearchBuilder(Container, 1263);
            resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithTPI(tpi);
            transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock
                .Setup(x => x.GetActivePaymentGatewayConfiguration(
                    It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration { UID = 1, GatewayCode = "300", CountryIsoCode = "132131", PaymentAuthorizationTypeId = 1 });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });
        }
    }
}

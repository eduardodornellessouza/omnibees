using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Contracts.Data.Payments;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Requests;
using OB.BL.Operations.Helper;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using OB.Services.IntegrationTests.Helpers;
using PaymentGatewaysLibrary;
using PaymentGatewaysLibrary.PayU;
using System;
using System.Collections.Generic;
using payU = PaymentGatewaysLibrary.PayU;
using reservationRequest = OB.Reservation.BL.Contracts.Requests.InsertReservationRequest;
using cancelReservationRequest = OB.Reservation.BL.Contracts.Requests.CancelReservationRequest;
using System.Linq;
using PaymentGatewaysLibrary.PayU.Classes.Extensions;
using OB.BL.Operations.Extensions;
using payUErrors = PaymentGatewaysLibrary.PayU.Classes.Common.Errors;
using OB.Reservation.BL.Contracts.Responses;
using System.ComponentModel.DataAnnotations;
using domain = OB.Domain.Reservations;
using OB.BL.Operations.Internal.TypeConverters;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class ReservationManagerPocoPayU : ReservationManagerPOCOUnitTestInitialize
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }


        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservation_PayUGateway_Success()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Charge(It.IsAny<ChargeRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = true,
                PaymentGatewayTransactionID = "paymentId",
                PaymentGatewayOrderID = "chargeId",
                PaymentGatewayTransactionType = PaymentGatewaysLibrary.Common.Constants.CHARGE_TRANSACTION
            });
            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            //Asserts
            Assert.AreEqual("RES000024-1263", reservationResponse.Number, "Expected reservation number inserted");
            Assert.AreEqual(1, reservationResponse.Result, "Expected uid to be 1");
            Assert.AreEqual(OB.Reservation.BL.Constants.ReservationStatus.Booked, reservationResponse.ReservationStatus, "Expected booked reservation");
            Assert.AreEqual(0, reservationResponse.Errors.Count, "Expected no errors");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservation_PayUGateway_With_PendingStatus_Success()
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Charge(It.IsAny<ChargeRequest>())).Returns(new PaymentMessageResult { IsTransactionValid = true, IsOnReview = true, PaymentGatewayTransactionStatusCode = "0" });
            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            //Asserts
            Assert.AreEqual("RES000024-1263", reservationResponse.Number, "Expected reservation number inserted");
            Assert.AreEqual(1, reservationResponse.Result, "Expected uid to be 1");
            Assert.AreEqual(OB.Reservation.BL.Constants.ReservationStatus.BookingOnRequest, reservationResponse.ReservationStatus, "Expected booked reservation");
            Assert.AreEqual(0, reservationResponse.Errors.Count, "Expected no errors");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(new object[] { -2, "RequiredParameter", "PublicKey" })]
        [DataRow(new object[] { -2, "RequiredParameter", "ApiId" })]
        [DataRow(new object[] { -2, "RequiredParameter", "PrivateKey" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreateTokenRequest.TokenType" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreateTokenRequest.HolderName" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreateTokenRequest.CardNumber" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreateTokenRequest.ExpirationDate" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreateTokenRequest.IdentityDocument.Number" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreateCustomerRequest.CustomerReference" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreatePaymentRequest.CreateCustomer" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreatePaymentRequest.Amount" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreatePaymentRequest.Currency" })]
        [DataRow(new object[] { -2, "RequiredParameter", "CreatePaymentRequest.StatementSoftDescriptor" })]
        [DataRow(new object[] { -2, "RequiredParameter", "PaymentMethod.SourceType" })]
        [DataRow(new object[] { -2, "RequiredParameter", "PaymentMethod.HolderName" })]
        [DataRow(new object[] { -2, "RequiredParameter", "PaymentMethod.CardNumber" })]
        [DataRow(new object[] { -2, "RequiredParameter", "PaymentMethod.ExpirationDate" })]
        [DataRow(new object[] { -2, "RequiredParameter", "PaymentMethod.CreditCardCvv" })]
        [DataRow(new object[] { -2, "RequiredParameter", "ProviderSpecificData.AdditionalDetails.Cookie" })]
        [DataRow(new object[] { -2, "RequiredParameter", "ProviderSpecificData.AdditionalDetails.CustomerNationalIdentifyNumber" })]
        [DataRow(new object[] { -2, "RequiredParameter", "ProviderSpecificData.DeviceFingerPrint.Fingerprint" })]
        public void TestInsertReservation_PayUGateway_RequiredParameters_Failed(object[] error)
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Charge(It.IsAny<ChargeRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                    {
                        new PaymentGatewaysLibrary.Common.Error
                        {
                            ErrorCode = (int)error[0],
                            ErrorType = error[1].ToString(),
                            Description = payUErrors.RequiredParameter.ToContractError(error[2].ToString()).Description,
                            Data = new Dictionary<string, string>
                            {
                                { "param0" , error[2].ToString() }
                            }
                        }
                    }
            });

            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            //Asserts
            Assert.AreEqual(-1000, reservationResponse.Result, "Expected uid to be 1");
            Assert.AreEqual(Status.Fail, reservationResponse.Status, "Expected status to be fail");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected no errors");
            Assert.AreEqual(-2, reservationResponse.Errors.First().ErrorCode);
            Assert.AreEqual("RequiredParameter", reservationResponse.Errors.First().ErrorType);
            switch (error[2].ToString())
            {
                case "PublicKey":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, "PublicKey"), reservationResponse.Errors.First().Description);
                    break;
                case "ApiId":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, "ApiId"), reservationResponse.Errors.First().Description);
                    break;
                case "PrivateKey":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, "PrivateKey"), reservationResponse.Errors.First().Description);
                    break;
                case "CreateTokenRequest.TokenType":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, "TokenType"), reservationResponse.Errors.First().Description);
                    break;
                case "CreateTokenRequest.HolderName":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardName)}"), reservationResponse.Errors.First().Description);
                    break;
                case "PaymentMethod.HolderName":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardName)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreateTokenRequest.CardNumber":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}"), reservationResponse.Errors.First().Description);
                    break;
                case "PaymentMethod.CardNumber":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreateTokenRequest.ExpirationDate":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}"), reservationResponse.Errors.First().Description);
                    break;
                case "PaymentMethod.ExpirationDate":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreateTokenRequest.IdentityDocument.Number":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.GuestIDCardNumber)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreateCustomerRequest.CustomerReference":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(payU.CreateCustomerRequest.CustomerReference)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreatePaymentRequest.CreateCustomer":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"Payment.{nameof(payU.CreatePaymentRequest.CreateCustomer)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreatePaymentRequest.Amount":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.TotalAmount)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreatePaymentRequest.Currency":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.ReservationBaseCurrency_UID)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreatePaymentRequest.StatementSoftDescriptor":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, "Statement Soft Descriptor"), reservationResponse.Errors.First().Description);
                    break;
                case "PaymentMethod.SourceType":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(payU.Enum.PaymentMethodSourceTypeEnum.PaymentMethodSourceType)}"), reservationResponse.Errors.First().Description);
                    break;
                case "PaymentMethod.CreditCardCvv":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CVV)}"), reservationResponse.Errors.First().Description);
                    break;
                case "ProviderSpecificData.AdditionalDetails.Cookie":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.ReservationsAdditionalData)}.{nameof(reservationRequest.ReservationsAdditionalData.Cookie)}"), reservationResponse.Errors.First().Description);
                    break;
                case "ProviderSpecificData.AdditionalDetails.CustomerNationalIdentifyNumber":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.GuestIDCardNumber)}"), reservationResponse.Errors.First().Description);
                    break;
                case "ProviderSpecificData.DeviceFingerPrint.Fingerprint":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, nameof(reservationRequest.AntifraudDeviceFingerPrintId)), reservationResponse.Errors.First().Description);
                    break;
            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(new object[] { -3, "InvalidParameter", "CreateTokenRequest.ExpirationDate" })]
        [DataRow(new object[] { -3, "InvalidParameter", "PaymentMethod.ExpirationDate" })]
        [DataRow(new object[] { -3, "InvalidParameter", "CreateTokenRequest.CardNumber" })]
        [DataRow(new object[] { -3, "InvalidParameter", "PaymentMethod.CardNumber" })]
        [DataRow(new object[] { -3, "InvalidParameter", "CreatePaymentRequest.Currency" })]
        [DataRow(new object[] { -3, "InvalidParameter", "PaymentMethod.CreditCardCvv" })]
        public void TestInsertReservation_PayUGateway_InvalidParameters_Failed(object[] error)
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Charge(It.IsAny<ChargeRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                    {
                        new PaymentGatewaysLibrary.Common.Error
                        {
                            ErrorCode = (int)error[0],
                            ErrorType = error[1].ToString(),
                            Description = payUErrors.InvalidParameter.ToContractError(error[2].ToString()).Description,
                            Data = new Dictionary<string, string>
                            {
                                { "param0" , error[2].ToString() }
                            }
                        }
                    }
            });
            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            //Asserts
            Assert.AreEqual(-1000, reservationResponse.Result, "Expected uid to be 1");
            Assert.AreEqual(Status.Fail, reservationResponse.Status, "Expected status to be fail");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected no errors");
            Assert.AreEqual(-3, reservationResponse.Errors.First().ErrorCode);
            Assert.AreEqual("InvalidParameter", reservationResponse.Errors.First().ErrorType);
            switch (error[2].ToString())
            {
                case "CreateTokenRequest.ExpirationDate":
                    Assert.AreEqual(GetDescription(Errors.InvalidParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}"), reservationResponse.Errors.First().Description);
                    break;
                case "PaymentMethod.ExpirationDate":
                    Assert.AreEqual(GetDescription(Errors.InvalidParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.ExpirationDate)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreatePaymentRequest.Currency":
                    Assert.AreEqual(GetDescription(Errors.InvalidParameter, $"{nameof(reservationRequest.Reservation)}.{nameof(reservationRequest.Reservation.ReservationBaseCurrency_UID)}"), reservationResponse.Errors.First().Description);
                    break;
                case "PaymentMethod.CreditCardCvv":
                    Assert.AreEqual(GetDescription(Errors.InvalidParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CVV)}"), reservationResponse.Errors.First().Description);
                    break;
                case "CreateTokenRequest.CardNumber":
                    Assert.AreEqual(GetDescription(Errors.InvalidParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}"), reservationResponse.Errors.First().Description);
                    break;
                case "PaymentMethod.CardNumber":
                    Assert.AreEqual(GetDescription(Errors.InvalidParameter, $"{nameof(reservationRequest.ReservationPaymentDetail)}.{nameof(reservationRequest.ReservationPaymentDetail.CardNumber)}"), reservationResponse.Errors.First().Description);
                    break;

            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(new object[] { -1000, "api_authentication_error", "We were unable to authenticate your request.", "test1", "test2" })]
        [DataRow(new object[] { -1001, "api_error", "Something went wrong on our side.", "test12", "test2" })]
        [DataRow(new object[] { -1002, "api_request_error", "The request is invalid. More information can be found in the description and more_info attributes of the Error object.", "test1", "test2" })]
        [DataRow(new object[] { -1003, "blocked_by_decision_engine", "A Block Rule was applied to this transaction. You can view and modify your Block configuration in the Control Center.", "test1", "test2" })]
        [DataRow(new object[] { -1004, "provider_authentication_error", "The provider is unable to authenticate the request.", "test11", "test2" })]
        [DataRow(new object[] { -1005, "provider_error", "Something went wrong on the provider's side.", "test1", "test2" })]
        [DataRow(new object[] { -1006, "provider_network_error", "Unable to reach the provider network.", "test1", "test21" })]
        [DataRow(new object[] { -1007, "payment_method_error", "The payment method provided is not valid. More information can be found in the sub_category and the description attributes of the Result object.", "test1", "test2" })]
        [DataRow(new object[] { -1008, "payment_method_not_supported", "The payment method is not supported. More information can be found in the description and the more_info attributes.", "test1000", "test2" })]
        [DataRow(new object[] { -1009, "payment_method_declined", "Payment method declined. More information can be found in the sub_category and the description attributes of the Result object.", "test100", "test2" })]
        [DataRow(new object[] { -1010, "request_not_supported_by_provider", "The request that was invoked is not supported by the provider handling the transaction.", "test12", "test2" })]
        [DataRow(new object[] { -1011, "two_way_authentication_failed", "Two way authentication failed. This includes SMS, redirection to external website and 3DS authentication.", "test1", "test2" })]
        [DataRow(new object[] { -1012, "provider_request_error", "The request sent to the provider is invalid.", "test10", "test2" })]
        public void TestInsertReservation_PayUGateway_CategoryError_From_PayU_Failed(object[] error)
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Charge(It.IsAny<ChargeRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
            {
                new PaymentGatewaysLibrary.Common.Error
                {
                    ErrorCode = (int)error[0],
                    ErrorType = error[1].ToString(),
                    Description = error[2].ToString(),
                    Data = new Dictionary<string, string>
                    {
                        { "param0", error[3].ToString()},
                        { "param1", error[4].ToString()}
                    }
                }
            }
            });
            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            //Asserts
            Assert.AreEqual(-1000, reservationResponse.Result, "Expected uid to be 1");
            Assert.AreEqual(Status.Fail, reservationResponse.Status, "Expected status to be fail");
            Assert.AreEqual(1, reservationResponse.Errors.Count, "Expected no errors");
            Assert.AreEqual((int)error[0], reservationResponse.Errors.First().ErrorCode);
            Assert.AreEqual(error[1].ToString(), reservationResponse.Errors.First().ErrorType);
            Assert.AreEqual(error[2].ToString(), reservationResponse.Errors.First().Description);
            Assert.AreEqual(error[3].ToString(), reservationResponse.Errors.First().Data.Values.First());
            Assert.AreEqual(error[4].ToString(), reservationResponse.Errors.First().Data.Values.Last());
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(new object[] { -1002, "api_request_error", "The request is invalid. More information can be found in the description and more_info attributes of the Error object.", "test1", "test2", -1108, "invalid_card_cvv", "The card's 'cvv' security code is invalid." })]
        [DataRow(new object[] { -1005, "provider_error", "Something went wrong on the provider's side.", "test1", "test2", -1110, "transaction_denied_by_cardholder", "The cardholder has placed a restriction on the card, merchant or transaction." })]
        public void TestInsertReservation_PayUGateway_SubCategoryError_From_PayU_Failed(object[] error)
        {
            long errorCode;
            SearchBuilder builder;
            ReservationDataBuilder resBuilder;
            string transactionId;
            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Charge(It.IsAny<ChargeRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
            {
                new PaymentGatewaysLibrary.Common.Error
                {
                    ErrorCode = (int)error[0],
                    ErrorType = error[1].ToString(),
                    Description = error[2].ToString(),
                    Data = new Dictionary<string, string>
                    {
                        { "param0", error[3].ToString()},
                        { "param1", error[4].ToString()}
                    }
                },
                new PaymentGatewaysLibrary.Common.Error
                {
                    ErrorCode = (int)error[5],
                    ErrorType = error[6].ToString(),
                    Description = error[7].ToString()
                },
            }
            });
            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            //Do the reservation
            var reservationResponse = InsertReservationV2(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, channelUid: 1);

            //Asserts
            Assert.AreEqual(-1000, reservationResponse.Result, "Expected uid to be 1");
            Assert.AreEqual(Status.Fail, reservationResponse.Status, "Expected status to be fail");
            Assert.AreEqual(2, reservationResponse.Errors.Count, "Expected no errors");
            Assert.AreEqual((int)error[0], reservationResponse.Errors.First().ErrorCode);
            Assert.AreEqual(error[1].ToString(), reservationResponse.Errors.First().ErrorType);
            Assert.AreEqual(error[2].ToString(), reservationResponse.Errors.First().Description);
            Assert.AreEqual(error[3].ToString(), reservationResponse.Errors.First().Data.Values.First());
            Assert.AreEqual(error[4].ToString(), reservationResponse.Errors.First().Data.Values.Last());

            Assert.AreEqual((int)error[5], reservationResponse.Errors.Last().ErrorCode);
            Assert.AreEqual(error[6].ToString(), reservationResponse.Errors.Last().ErrorType);
            Assert.AreEqual(error[7].ToString(), reservationResponse.Errors.Last().Description);
            Assert.AreEqual(false, reservationResponse.Errors.Last().Data.Any());
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestCancelReservation_PayUGateway_CancellationRoom()
        {
            #region ARRANGE

            long propertyUID = 1263; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;
            long errorCode;
            SearchBuilder builder;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var paymentId = "transactionPaymentId";
            var refundId = "RefundId";

            _payUPag.Setup(x => x.Refund(It.IsAny<RefundRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = true,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                PaymentGatewayTransactionID = paymentId,
                PaymentGatewayOrderID = refundId,
                PaymentGatewayTransactionType = PaymentGatewaysLibrary.Common.Constants.REFUND_TRANSACTION,
                PaymentGatewayAutoGeneratedUID = reservation.Number
            });
            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            #endregion

            Random random = new Random();            
            reservation.PaymentGatewayName = "PayU";
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.PaymentGatewayTransactionStatusCode = "1";

            foreach (ReservationRoom resroom in reservationRooms)
            {
                resroom.CancellationPolicyDays = 0;
                resroom.IsCancellationAllowed = true;
                resroom.CancellationValue = null;
                resroom.CancellationPolicyDays = null;
                resroom.CancellationCosts = true;
                resroom.CancellationNrNights = null;
            }

            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);


            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            _payUPag.Verify(m => m.Refund(It.Is<RefundRequest>(requestR => requestR.Amount == resRoomsCancellationFirstRoom.First().ReservationRoomsTotalAmount)));
            _payUPag.Verify(m => m.Refund(It.Is<RefundRequest>(requestR => requestR.RefundReason == "Reservation cancelled by the guest")));

            Assert.AreEqual(1, result.Result); //error - transaction invalid consult maxipago
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(reservation.Number, reservation.PaymentGatewayAutoGeneratedUID);
            Assert.AreEqual(paymentId, reservation.PaymentGatewayTransactionID);
            Assert.AreEqual(refundId, reservation.PaymentGatewayOrderID);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestCancelReservation_PayUGateway_CancelReservation()
        {
            #region ARRANGE
            long propertyUID = 1263; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;
            long errorCode;
            SearchBuilder builder;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var paymentId = "paymentId";            
            var refundId = "RefundId";            
            
            _payUPag.Setup(x => x.Refund(It.IsAny<RefundRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = true,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                PaymentGatewayTransactionID = paymentId,
                PaymentGatewayOrderID = refundId,
                PaymentGatewayTransactionType = PaymentGatewaysLibrary.Common.Constants.REFUND_TRANSACTION,
                PaymentGatewayAutoGeneratedUID = reservation.Number
            });
            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            #endregion

            Random random = new Random();

            reservation.PaymentGatewayName = "PayU";
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.PaymentGatewayTransactionStatusCode = "1";

            foreach (ReservationRoom resroom in reservationRooms)
            {
                resroom.DateFrom = DateTime.Now.Date;
                resroom.IsCancellationAllowed = true;
                resroom.CancellationValue = null;
                resroom.CancellationPolicyDays = 0;
                resroom.CancellationCosts = true;
                resroom.CancellationNrNights = null;
            }

            #endregion


            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = reservation.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT

            _payUPag.Verify(m => m.Refund(It.Is<RefundRequest>(requestR => requestR.Amount == null)));
            _payUPag.Verify(m => m.Refund(It.Is<RefundRequest>(requestR => requestR.RefundReason == "Reservation cancelled by the guest")));

            Assert.AreEqual(1, result.Result); //error - transaction invalid consult maxipago
            Assert.AreEqual(reservation.Number, reservation.PaymentGatewayAutoGeneratedUID);
            Assert.AreEqual(paymentId, reservation.PaymentGatewayTransactionID);
            Assert.AreEqual(refundId, reservation.PaymentGatewayOrderID, refundId);
            Assert.AreEqual(2, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestCancelReservation_PayUGateway_Omnibees_CancelReservation()
        {
            #region ARRANGE
            long propertyUID = 1263; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;
            long errorCode;
            SearchBuilder builder;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            GeneralSetUp(out errorCode, out builder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Refund(It.IsAny<RefundRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = true,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0"
            });
            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);

            #endregion

            Random random = new Random();

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            reservation.PaymentGatewayName = "PayU";
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.PaymentGatewayTransactionStatusCode = "1";

            foreach (ReservationRoom resroom in reservationRooms)
            {
                resroom.DateFrom = DateTime.Now.Date;
                resroom.IsCancellationAllowed = true;
                resroom.CancellationValue = null;
                resroom.CancellationPolicyDays = 0;
                resroom.CancellationCosts = true;
                resroom.CancellationNrNights = null;
            }

            #endregion


            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = reservation.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = 3,
                CancelReservationComments = "Invalid Credit Card",
                RuleType = OB.Reservation.BL.Constants.RuleType.Omnibees
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT

            _payUPag.Verify(m => m.Refund(It.Is<RefundRequest>(requestR => requestR.Amount == null)));
            _payUPag.Verify(m => m.Refund(It.Is<RefundRequest>(requestR => requestR.RefundReason == "name 3")));

            Assert.AreEqual(1, result.Result); //error - transaction invalid consult maxipago
            Assert.AreEqual(2, resCancellationFirstRoom.Status);
            Assert.AreEqual(3, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual("Invalid Credit Card", resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(new object[] { -2, "RequiredParameter", "ApiId" })]
        [DataRow(new object[] { -2, "RequiredParameter", "PrivateKey" })]
        [DataRow(new object[] { -2, "RequiredParameter", "PaymentId" })]
        [DataRow(new object[] { -2, "RequiredParameter", "RefundReason" })]
        public void TestCancelReservation_PayUGateway_RequiredParameters_Failed(object[] error)
        {
            #region ARRANGE
            long propertyUID = 1263; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;
            long errorCode;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            GeneralSetUp(out errorCode, out searchBuilder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Refund(It.IsAny<RefundRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                    {
                        new PaymentGatewaysLibrary.Common.Error
                        {
                            ErrorCode = (int)error[0],
                            ErrorType = error[1].ToString(),
                            Description = payUErrors.RequiredParameter.ToContractError(error[2].ToString()).Description,
                            Data = new Dictionary<string, string>
                            {
                                { "param0" , error[2].ToString() }
                            }
                        }
                    }
            });
            #endregion


            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);


            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            reservation.PaymentGatewayName = "PayU";
            var reservationRooms = reservation.ReservationRooms.ToList();
            listPaymentGetwayConfigs.First().GatewayCode = "600";

            reservation.PaymentGatewayTransactionStatusCode = "1";
            #endregion

            //cancel the reservation

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = reservation.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = null,
                RuleType = OB.Reservation.BL.Constants.RuleType.Omnibees
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            //Asserts
            Assert.AreEqual(-1000, result.Result, "Expected uid to be 1");
            Assert.AreEqual(Status.Fail, result.Status, "Expected status to be fail");
            Assert.AreEqual(1, result.Errors.Count, "Expected no errors");
            Assert.AreEqual(-2, result.Errors.First().ErrorCode);
            Assert.AreEqual("RequiredParameter", result.Errors.First().ErrorType);
            switch (error[2].ToString())
            {
                case "ApiId":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, "ApiId"), result.Errors.First().Description);
                    break;
                case "PrivateKey":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, "PrivateKey"), result.Errors.First().Description);
                    break;
                case "PaymentId":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(cancelReservationRequest.ReservationUID)} or {nameof(cancelReservationRequest.ReservationNo)}"), result.Errors.First().Description);
                    break;
                case "RefundReason":
                    Assert.AreEqual(GetDescription(Errors.RequiredParameter, $"{nameof(cancelReservationRequest.CancelReservationReason_UID)}"), result.Errors.First().Description);
                    break;
            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(new object[] { -1000, "api_authentication_error", "We were unable to authenticate your request.", "test1", "test2" })]
        [DataRow(new object[] { -1001, "api_error", "Something went wrong on our side.", "test12", "test2" })]
        [DataRow(new object[] { -1002, "api_request_error", "The request is invalid. More information can be found in the description and more_info attributes of the Error object.", "test1", "test2" })]
        [DataRow(new object[] { -1003, "blocked_by_decision_engine", "A Block Rule was applied to this transaction. You can view and modify your Block configuration in the Control Center.", "test1", "test2" })]
        [DataRow(new object[] { -1004, "provider_authentication_error", "The provider is unable to authenticate the request.", "test11", "test2" })]
        [DataRow(new object[] { -1005, "provider_error", "Something went wrong on the provider's side.", "test1", "test2" })]
        [DataRow(new object[] { -1006, "provider_network_error", "Unable to reach the provider network.", "test1", "test21" })]
        [DataRow(new object[] { -1007, "payment_method_error", "The payment method provided is not valid. More information can be found in the sub_category and the description attributes of the Result object.", "test1", "test2" })]
        [DataRow(new object[] { -1008, "payment_method_not_supported", "The payment method is not supported. More information can be found in the description and the more_info attributes.", "test1000", "test2" })]
        [DataRow(new object[] { -1009, "payment_method_declined", "Payment method declined. More information can be found in the sub_category and the description attributes of the Result object.", "test100", "test2" })]
        [DataRow(new object[] { -1010, "request_not_supported_by_provider", "The request that was invoked is not supported by the provider handling the transaction.", "test12", "test2" })]
        [DataRow(new object[] { -1011, "two_way_authentication_failed", "Two way authentication failed. This includes SMS, redirection to external website and 3DS authentication.", "test1", "test2" })]
        [DataRow(new object[] { -1012, "provider_request_error", "The request sent to the provider is invalid.", "test10", "test2" })]
        public void TestCancelReservation_PayUGateway_CategoryError_From_PayU_Failed(object[] error)
        {
            #region ARRANGE
            long propertyUID = 1263; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;
            long errorCode;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            GeneralSetUp(out errorCode, out searchBuilder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Refund(It.IsAny<RefundRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                    {
                        new PaymentGatewaysLibrary.Common.Error
                        {
                            ErrorCode = (int)error[0],
                            ErrorType = error[1].ToString(),
                            Description = error[2].ToString(),
                            Data = new Dictionary<string, string>
                            {
                                { "param0" , error[3].ToString() },
                                { "param1" , error[4].ToString() }
                            }
                        }
                    }
            });
            #endregion


            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);


            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            reservation.PaymentGatewayName = "PayU";
            var reservationRooms = reservation.ReservationRooms.ToList();
            listPaymentGetwayConfigs.First().GatewayCode = "600";

            reservation.PaymentGatewayTransactionStatusCode = "1";
            #endregion

            //cancel the reservation

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = reservation.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = null,
                RuleType = OB.Reservation.BL.Constants.RuleType.Omnibees
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            //Asserts
            Assert.AreEqual(-1000, result.Result, "Expected uid to be 1");
            Assert.AreEqual(Status.Fail, result.Status, "Expected status to be fail");
            Assert.AreEqual(1, result.Errors.Count, "Expected no errors");
            Assert.AreEqual((int)error[0], result.Errors.First().ErrorCode);
            Assert.AreEqual(error[1].ToString(), result.Errors.First().ErrorType);
            Assert.AreEqual(error[2].ToString(), result.Errors.First().Description);
            Assert.AreEqual(error[3].ToString(), result.Errors.First().Data.Values.First());
            Assert.AreEqual(error[4].ToString(), result.Errors.First().Data.Values.Last());
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(new object[] { -1002, "api_request_error", "The request is invalid. More information can be found in the description and more_info attributes of the Error object.", "test1", "test2", -1108, "invalid_card_cvv", "The card's 'cvv' security code is invalid." })]
        [DataRow(new object[] { -1005, "provider_error", "Something went wrong on the provider's side.", "test1", "test2", -1110, "transaction_denied_by_cardholder", "The cardholder has placed a restriction on the card, merchant or transaction." })]
        public void TestCancelReservation_PayUGateway_SubCategoryError_From_PayU_Failed(object[] error)
        {
            #region ARRANGE
            long propertyUID = 1263; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;
            long errorCode;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            GeneralSetUp(out errorCode, out searchBuilder, out resBuilder, out transactionId);

            _payUPag.Setup(x => x.Refund(It.IsAny<RefundRequest>())).Returns(new PaymentMessageResult
            {
                IsTransactionValid = false,
                IsOnReview = false,
                PaymentGatewayTransactionStatusCode = "0",
                Errors = new List<PaymentGatewaysLibrary.Common.Error>
                    {
                        new PaymentGatewaysLibrary.Common.Error
                        {
                            ErrorCode = (int)error[0],
                            ErrorType = error[1].ToString(),
                            Description = error[2].ToString(),
                            Data = new Dictionary<string, string>
                            {
                                { "param0" , error[3].ToString() },
                                { "param1" , error[4].ToString() }
                            }
                        },
                        new PaymentGatewaysLibrary.Common.Error
                        {
                            ErrorCode = (int)error[5],
                            ErrorType = error[6].ToString(),
                            Description = error[7].ToString()
                        }
                    }
            });
            #endregion


            _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(_payUPag.Object);


            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            reservation.PaymentGatewayName = "PayU";
            var reservationRooms = reservation.ReservationRooms.ToList();
            listPaymentGetwayConfigs.First().GatewayCode = "600";

            reservation.PaymentGatewayTransactionStatusCode = "1";
            #endregion

            //cancel the reservation

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = reservation.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = null,
                RuleType = OB.Reservation.BL.Constants.RuleType.Omnibees
            };

            var reservationResponse = ReservationManagerPOCO.CancelReservation(request);

            //Asserts
            Assert.AreEqual(-1000, reservationResponse.Result, "Expected uid to be 1");
            Assert.AreEqual(Status.Fail, reservationResponse.Status, "Expected status to be fail");
            Assert.AreEqual(2, reservationResponse.Errors.Count, "Expected no errors");
            Assert.AreEqual((int)error[0], reservationResponse.Errors.First().ErrorCode);
            Assert.AreEqual(error[1].ToString(), reservationResponse.Errors.First().ErrorType);
            Assert.AreEqual(error[2].ToString(), reservationResponse.Errors.First().Description);
            Assert.AreEqual(error[3].ToString(), reservationResponse.Errors.First().Data.Values.First());
            Assert.AreEqual(error[4].ToString(), reservationResponse.Errors.First().Data.Values.Last());

            Assert.AreEqual((int)error[5], reservationResponse.Errors.Last().ErrorCode);
            Assert.AreEqual(error[6].ToString(), reservationResponse.Errors.Last().ErrorType);
            Assert.AreEqual(error[7].ToString(), reservationResponse.Errors.Last().Description);
            Assert.AreEqual(false, reservationResponse.Errors.Last().Data.Any());
        }


        #region aux methods

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
                .Returns(new PaymentGatewayConfiguration { UID = 1, GatewayName = "PayU", GatewayCode = "600", CountryIsoCode = "COL", PaymentAuthorizationTypeId = 1 });

            _iOBPPropertyRepoMock
        .Setup(x => x.GetActivePaymentGatewayConfigurationReduced(
            It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
        .Returns(new PaymentGatewayConfiguration { UID = 1, GatewayName = "PayU", GatewayCode = "600", CountryIsoCode = "COL", PaymentAuthorizationTypeId = 1 });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 53,
                            CountryCode = "CO",
                            CountryIsoCode = "COL"
                        }
                    });

            _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<ListCityDataRequest>())).Returns(
                new List<Contracts.Data.General.CityData> {
                    new Contracts.Data.General.CityData {
                        asciiname = "medellin",
                        name = "medellin"
                    }
                });
        }


        private string GetDescription(Errors enumError, string parameter)
        {
            return string.Format(GetDisplayAttribute(enumError), parameter);
        }

        private static string GetDisplayAttribute(object value)
        {
            DisplayAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        protected void GetBuildersUsingReservation(out SearchBuilder searchBuilder, out ReservationDataBuilder resBuilder, int resUid)
        {
            searchBuilder = null;
            resBuilder = null;

            var reservation = reservationsList.Where(x => x.UID == resUid).SingleOrDefault();

            resBuilder = new ReservationDataBuilder(reservation.Channel_UID, reservation.Property_UID, reservation.UID, true, reservation.Number, reservation.ReservationLanguageUsed_UID ?? 4, false);

            //res
            resBuilder.InputData.reservationDetail = reservation.Clone();
            resBuilder.ExpectedData.reservationDetail = reservation.Clone();

            //guest
            var guest = guestsList.Where(x => x.UID == reservation.Guest_UID).SingleOrDefault();
            resBuilder.InputData.guest = OtherConverter.Convert(guest);
            resBuilder.ExpectedData.guest = OtherConverter.Convert(guest);

            //guest activity
            resBuilder.InputData.guestActivity = guestActivityList.Where(x => x.Guest_UID == guest.UID).Select(x => x.Activity_UID).ToList();
            resBuilder.ExpectedData.guestActivityObj = guestActivityList.Where(x => x.Guest_UID == guest.UID).ToList();

            //not added for now - not needed
            //resBuilder.WithExtraSchedule

            if (reservation.ReservationRooms != null && reservation.ReservationRooms.Any())
            {
                //Reservation rooms
                resBuilder.WithRoom(reservation.ReservationRooms);

                int i = 1;
                foreach (var resRoom in reservation.ReservationRooms)
                {
                    if (resRoom.ReservationRoomChilds != null && resRoom.ReservationRoomChilds.Any())
                    {
                        //children rooms
                        resBuilder.WithChildren(resRoom.ReservationRoomChilds);
                    }

                    if (resRoom.ReservationRoomDetails != null && resRoom.ReservationRoomDetails.Any())
                    {
                        //room details
                        resBuilder.WithRoomDetails(resRoom.ReservationRoomDetails);
                    }

                    if (resRoom.ReservationRoomExtras != null && resRoom.ReservationRoomExtras.Any())
                    {
                        //extras
                        resBuilder.WithExtra(resRoom.ReservationRoomExtras.Select(r =>
                        {
                            var aux = r.Clone();
                            aux.UID = aux.ReservationRoom_UID;
                            return aux;
                        }));
                    }
                }
            }

            if (reservation.ReservationPaymentDetails != null && reservation.ReservationPaymentDetails.Any())
            {
                foreach (var pd in reservation.ReservationPaymentDetails)
                {
                    resBuilder.WithCreditCardPayment(pd);
                }
            }

            searchBuilder = GetFromResBuilder(resBuilder, reservation);
        }

        protected SearchBuilder GetFromResBuilder(OB.BL.Operations.Helper.ReservationDataBuilder resBuilder, OB.Domain.Reservations.Reservation reservation)
        {
            var searchBuilder = new Operations.Helper.SearchBuilder(Container, reservation.Property_UID);
            var channels = new List<long>() { reservation.Channel_UID.GetValueOrDefault() };
            var childAges = reservation.ReservationRooms.SelectMany(x => x.ReservationRoomChilds).Select(x => x.Age).ToList();
            var currencies = new List<ChildTermsCurrency>() { new ChildTermsCurrency
            {
                Currency_UID = 34,
                Value = 10
            }};

            foreach (var rr in reservation.ReservationRooms)
            {
                searchBuilder.AddRate(UID: rr.Rate_UID);
            }

            int i = reservation.ReservationRooms.Count;
            while (i <= 0)
            {
                searchBuilder.AddRoom(string.Empty, true, false, 20, 1, 0, 0, 20);
                i--;
            }

            searchBuilder.AddRateRoomsAll().AddRateChannelsAll(channels);
            searchBuilder.AddSearchParameters(new SearchParameters
            {
                AdultCount = reservation.Adults,
                ChildCount = 0,
                CheckIn = DateTime.Now.AddDays(1),
                CheckOut = DateTime.Now.AddDays(2)
            });

            if (reservation.Children > 0)
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

            if (resBuilder.InputData.reservationRoomDetails != null && resBuilder.InputData.reservationRoomDetails
                .Any(x => x.ReservationRoomDetailsAppliedIncentives != null && x.ReservationRoomDetailsAppliedIncentives.Any()))
            {
                var incentives = resBuilder.InputData.reservationRoomDetails.SelectMany(x => x.ReservationRoomDetailsAppliedIncentives)
                    .GroupBy(x => x.Incentive_UID).Select(x => x.First()).ToList();
                foreach (var inc in incentives)
                {
                    searchBuilder.AddIncentive(reservation.Property_UID, searchBuilder.InputData.Rates[0].UID, inc);
                }
            }

            searchBuilder.CreateRateRoomDetails();

            // Prices For The Modification
            searchBuilder.CreateRateRoomDetails(DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), 50);

            return searchBuilder;
        }

        protected void MockMultipleBasedOnBuilders(SearchBuilder searchBuilder, ReservationDataBuilder resBuilder, string transactionId, object inventories, bool addTranlations)
        {
            MockReservationsRepo(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);
            MockPropertyEventsRepo();
            MockObPropertyRepo(resBuilder);
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
            //MockPropertyQueueGenericRepoMock(resBuilder, searchBuilder);
            MockIncentives(searchBuilder);
            MockBuyerGroupRepo(searchBuilder, resBuilder);
            MockPromoCodesRepo(searchBuilder);
            MockExtrasRepo(resBuilder);
            MockObPaymentMethodRepo();
            MockSecurityRepo();
            MockReservationRoomsExtraRepoGeneric(resBuilder);
            MockRoomExtrasSchedulesGenericRepo(resBuilder);
            MockResPartialPayment();
        }

        #endregion
    }
}


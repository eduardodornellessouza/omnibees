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
using OB.DL.Common.Interfaces;
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
using PaymentGatewaysLibrary;
using PaymentGatewaysLibrary.BrasPagGateway;
using PaymentGatewaysLibrary.BrasPagGateway.Classes;
using contractsCRM = OB.BL.Contracts.Data.CRM;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservation = OB.Domain.Reservations;
using System.Reflection;
using OB.BL.Operations.Impl;
using payUContracts = PaymentGatewaysLibrary.PayU;
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
    public class ReservationManagerPOCOUnitTest : ReservationManagerPOCOUnitTestInitialize
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        #region ListReservationNumbers
        [TestMethod]
        [TestCategory("ListReservationNumbers")]
        public void TestListReservationNumbers_SinglePropertyOneNumber()
        {
            //Mock Generate Reservation Number
            MockResNumbersGenerator();

            // act            
            var result = this.ReservationManagerPOCO.ListReservationNumbers(new OB.Reservation.BL.Contracts.Requests.ListReservationNumbersRequest()
            {
                Values = new Dictionary<long, int>
                {
                    { 1635, 1}
                }
            });

            // assert
            Assert.AreEqual(1, result.Numbers.Count, "Only one Property expected");
            Assert.AreEqual(1, result.Numbers.First().Value.Count, "Only one reservation number for the given Property expected");
        }

        [TestMethod]
        [TestCategory("ListReservationNumbers")]
        public void TestListReservationNumbers_SinglePropertyMultipleNumbers()
        {
            //Mock Generate Reservation Number
            MockResNumbersGenerator();

            // act            
            var result = this.ReservationManagerPOCO.ListReservationNumbers(new OB.Reservation.BL.Contracts.Requests.ListReservationNumbersRequest()
            {
                Values = new Dictionary<long, int>
                {
                    { 1635, 4}
                }
            });

            // assert
            Assert.AreEqual(1, result.Numbers.Count, "Only one Property expected");
            Assert.AreEqual(4, result.Numbers.First().Value.Count, "Four reservation numbers for the given Property expected");
        }


        [TestMethod]
        [TestCategory("ListReservationNumbers")]
        public void TestListReservationNumbers_MultiplePropertiesOneNumber()
        {
            //Mock Generate Reservation Number
            MockResNumbersGenerator();

            List<long> propertyUIDs = new List<long>();
            propertyUIDs = listPropertiesLigth.OrderByDescending(x => x.UID).Select(x => x.UID).Take(100).ToList();

            var request = new OB.Reservation.BL.Contracts.Requests.ListReservationNumbersRequest() { Values = new Dictionary<long, int>() };
            foreach (var propertyUID in propertyUIDs)
            {
                request.Values.Add(propertyUID, 1);
            }

            // act                        
            var result = this.ReservationManagerPOCO.ListReservationNumbers(request);


            // assert
            Assert.AreEqual(propertyUIDs.Count, result.Numbers.Count, propertyUIDs.Count + " Properties expected");
            Assert.IsTrue(result.Numbers.All(x => x.Value.Count == 1), "Only one reservation number for each the given Property is expected");
        }

        [TestMethod]
        [TestCategory("ListReservationNumbers")]
        public void TestListReservationNumbers_MultiplePropertiesMultipleNumbers()
        {
            //Mock Generate Reservation Number
            MockResNumbersGenerator();

            List<long> propertyUIDs = new List<long>();
            propertyUIDs = listPropertiesLigth.OrderByDescending(x => x.UID).Select(x => x.UID).Take(100).ToList();

            var request = new OB.Reservation.BL.Contracts.Requests.ListReservationNumbersRequest() { Values = new Dictionary<long, int>() };
            foreach (var propertyUID in propertyUIDs)
            {
                request.Values.Add(propertyUID, 3);
            }

            // act                        
            var result = this.ReservationManagerPOCO.ListReservationNumbers(request);


            // assert
            Assert.AreEqual(propertyUIDs.Count, result.Numbers.Count, propertyUIDs.Count + " Properties expected");
            Assert.IsTrue(result.Numbers.All(x => x.Value.Count == 3), "Three reservation numbers for each the given Property are expected");
        }
        #endregion

        #region NEGATIVE TESTS
        [TestMethod]
        [ExpectedException(typeof(BusinessLayerException))]
        [TestCategory("InsertReservation_NegativeTests")]
        [DeploymentItem("Trace.trc")]
        public void TestInsertReservation_Empty()
        {
            long errorCode = 0;

            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1).WithEmpty();
            var transactionId = Guid.NewGuid().ToString();

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806, resIsEmpty: true);

            // assert
            // exception thrown
        }

        [TestMethod]
        [TestCategory("InsertReservation_NegativeTests")]
        public void TestInsertReservation_ExtrasNull()
        {
            long errorCode = 0;

            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806, ignoreChangeResNumberIfBE: true).WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithNewGuest().WithExtrasNull();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806, withChildsRoom: false, withCancellationPolicies: false);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_NegativeTests")]
        public void TestInsertReservation_NoRooms()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(32, propertyId: 1806, resNumber: resNumberExpected).WithNewGuest().WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, numberOfAdults: 0, resNumber: resNumberExpected, withNoRooms: true);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_NegativeTests")]
        public void TestInsertReservation_DetailAndEmptyGuestOnly()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(32, propertyId: 1806, resNumber: resNumberExpected).WithGuestEmpty();
            var transactionId = Guid.NewGuid().ToString();

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, numberOfAdults: 0, resNumber: resNumberExpected,
                withNoRooms: true, emptyGuest: true);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            var unitOfWork = this.SessionFactory.GetUnitOfWork();
            try
            {
                // assert
                ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID, false, unitOfWork));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BusinessLayerException))]
        [TestCategory("InsertReservation_NegativeTests")]
        public void TestInsertReservation_GuestNull()
        {
            long errorCode = 0;

            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1).WithRoom(1, 1).WithGuestNull();

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, numberOfAdults: 1);

            // assert
            // exception thrown
        }

        [TestMethod]
        [ExpectedException(typeof(BusinessLayerException))]
        [TestCategory("InsertReservation_NegativeTests")]
        public void TestInsertReservation_InvalidChannel()
        {
            long errorCode = 0;

            // arrange
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(0).WithNewGuest().WithRoom(1, 1).WithCreditCardPayment();

            // act
            var reservation = InsertReservation(out errorCode, builder, resBuilder, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 0, numberOfAdults: 1);

            // assert
            // exception thrown           
        }

        [TestMethod]
        [ExpectedException(typeof(BusinessLayerException))]
        [TestCategory("InsertReservation_NegativeTests")]
        public void TestInsertReservation_DetailOnly()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, detailOnly: true);
            var transactionId = Guid.NewGuid().ToString();

            // act
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, numberOfAdults: 1, resNumber: resNumberExpected);

            // assert
            // waits exception
        }
        #endregion

        #region BOOKING ENGINE
        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_NoPaymentDetails()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, propertyId: 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            MockSqlManager<Inventory>(1, new List<Inventory> {
                new Inventory
                {
                    QtyRoomOccupied = 1,
                    UID = 1,
                    RoomType_UID = 1
                }
            });

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_Simple()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, propertyId: 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_BlockPartialPaymentException()
        {
            long errorCode = 0;

            // Arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, propertyId: 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            var resValidatorPocoMock = new Mock<IReservationValidatorPOCO>();
            Container.RegisterInstance(resValidatorPocoMock.Object);

            resValidatorPocoMock
                .Setup(x => x.InitialValidation(It.IsAny<Internal.BusinessObjects.InitialValidationRequest>()))
                .Throws(OB.Reservation.BL.Contracts.Responses.Errors.PaymentMethodNotSupportPartialPayments.ToBusinessLayerException());

            try
            {
                // Act
                _ = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                    withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

                Assert.Fail("Should throw an exception");
            }
            catch (BusinessLayerException ex)
            {
                // Assert
                Assert.AreEqual((int)OB.Reservation.BL.Contracts.Responses.Errors.PaymentMethodNotSupportPartialPayments, ex.ErrorCode);
                resValidatorPocoMock.Verify(x => x.InitialValidation(It.IsAny<Internal.BusinessObjects.InitialValidationRequest>()), Times.Once);
            }
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_Simple_With_Close_Sales_Control()
        {
            long errorCode = 0;

            var unitOfWork = this.SessionFactory.GetUnitOfWork();

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, propertyId: 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            #region Mock Repo Calls
            MockIncentives(builder);
            MockBuyerGroupRepo(builder, resBuilder);
            MockPromoCodesRepo(builder);
            MockExtrasRepo(resBuilder);
            MockObPaymentMethodRepo();
            MockSecurityRepo();
            #endregion

            var inventory = InventoryBuilder.GetInventory(roomTypeUidMock, DateTime.Today, unitOfWork, this.RepositoryFactory.GetOBPropertyRepository());
            InventoryBuilder.UpdateInventory(inventory, 1, DateTime.Today, unitOfWork, this.RepositoryFactory.GetOBPropertyRepository());

            // Define Availability Control Rules
            unitOfWork.Save();

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected,
                inventories: new List<Inventory>() { inventory });

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
            // TODO: assert close sales

        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_Simple_HandleCancelationPolicy_False()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true).WithCreditCardPayment().WithCancelationPolicy();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: true, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));

        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_ExistingGuestEmptyAddress()
        {
            long errorCode = 0;

            //Change the Guest
            contractsCRM.Guest guest = guestsList.Where(x => x.UID == 63774).Single();
            var tmpGuest = guest.Clone();
            guestsList.Remove(guest);
            tmpGuest.UserName = "testemail@email.com";
            tmpGuest.Email = "testemail@email.com";
            guestsList.Add(tmpGuest);

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithExistingGuest(OtherConverter.Convert(tmpGuest), true).WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_ExistingGuestWithAddress()
        {
            long errorCode = 0;

            //Change the Guest
            contractsCRM.Guest guest = guestsList.Where(x => x.UID == 63774).Single();
            var tmpGuest = guest.Clone();
            guestsList.Remove(guest);
            tmpGuest.UserName = "testemail@email.com";
            tmpGuest.Email = "testemail@email.com";
            tmpGuest.Address1 = "test xpto1";
            tmpGuest.Address2 = "test xpto2";
            tmpGuest.BillingAddress1 = "test abcd1";
            tmpGuest.BillingAddress2 = "test abcd2";
            guestsList.Add(tmpGuest);

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithExistingGuest(OtherConverter.Convert(tmpGuest), true).WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_ExtraAdded()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithExtra(1, false, 1);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID), true);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_ExtraIncluded()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithExtra(1, true, 1);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID), true);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_ExtraAddedWithSchedule()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithExtra(1, false, 1).WithExtraSchedule(1, 1);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID), true);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_ExtraIncludedWithPeriods()
        {
            long errorCode = 0;

            RatesExtrasPeriod period = new RatesExtrasPeriod()
            {
                DateFrom = new DateTime(2014, 01, 01),
                DateTo = new DateTime(2014, 12, 31)
            };

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithExtra(1, true, 1).WithExtraPeriod(period);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID), true);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_TPI()
        {
            long errorCode = 0;

            // arrange
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            tpi.Property_UID = 1806;

            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithTPI(tpi).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID), true);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_TPI_IsZero()
        {
            long errorCode = 0;

            // arrange
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = new Domain.CRM.ThirdPartyIntermediary
            {
                UID = 0
            };

            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithTPI(tpi).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.TPI_UID = null;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID), true);
        }


        [Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_TPIWithCredit()
        {
            long errorCode = 0;

            // arrange
            IUnitOfWork unitOfWork = null;
            //Prepare the tpi and the tpiproperty
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            TPIProperty tpiProperty = null;

            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            tpi.Property_UID = 1263;

            tpiProperty = new TPIProperty();
            tpiProperty.TPI_UID = tpi.UID;
            tpiProperty.Property_UID = tpi.Property_UID.Value;
            tpiProperty.CreditLimit = 120;
            tpiProperty.CreditUsed = null;
            tpiPropertyList.Add(tpiProperty);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithBilledTypePayment(4).WithTPI(tpi);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                      
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            IOBCRMRepository crmRepo = RepositoryFactory.GetOBCRMRepository();
            var refreshedObject = crmRepo.ListTpiProperty(new ListTPIPropertyRequest() { PropertyIds = new List<long>() { 1263 } }).SingleOrDefault();
            Assert.AreEqual(110, refreshedObject.CreditUsed);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_TPIWithCreditWithCreditCardPayment()
        {
            long errorCode = 0;

            // arrange
            IUnitOfWork unitOfWork = null;
            //Prepare the tpi and the tpiproperty
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            TPIProperty tpiProperty = null;

            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            tpi.Property_UID = 1263;

            tpiProperty = new TPIProperty();
            tpiProperty.TPI_UID = tpi.UID;
            tpiProperty.Property_UID = tpi.Property_UID.Value;
            tpiProperty.CreditLimit = 120;
            tpiProperty.CreditUsed = null;
            tpiPropertyList.Add(tpiProperty);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithTPI(tpi);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                      
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            IOBCRMRepository crmRepo = RepositoryFactory.GetOBCRMRepository();
            var refreshedObject = crmRepo.ListTpiProperty(new ListTPIPropertyRequest() { PropertyIds = new List<long>() { tpiProperty.Property_UID } }).SingleOrDefault();
            Assert.AreEqual(null, refreshedObject.CreditUsed);
        }

        [Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_TPIWithNoCredit()
        {
            long errorCode = 0;

            // arrange
            IUnitOfWork unitOfWork = null;
            //Prepare the tpi and the tpiproperty
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            TPIProperty tpiProperty = null;

            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            tpi.Property_UID = 1263;

            tpiProperty = new TPIProperty();
            tpiProperty.TPI_UID = tpi.UID;
            tpiProperty.Property_UID = tpi.Property_UID.Value;
            tpiProperty.CreditLimit = 100;
            tpiProperty.CreditUsed = (decimal)123.45;
            tpiPropertyList.Add(tpiProperty);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithBilledTypePayment(4).WithTPI(tpi);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                      
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            IOBCRMRepository crmRepo = RepositoryFactory.GetOBCRMRepository();
            var refreshedObject = crmRepo.ListTpiProperty(new ListTPIPropertyRequest() { PropertyIds = new List<long>() { tpiProperty.Property_UID } }).SingleOrDefault();
            Assert.AreEqual((decimal)233.45, refreshedObject.CreditUsed);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_TPIComission()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi and the tpiproperty
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            TPIProperty tpiProperty = null;

            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            tpi.Property_UID = 1263;

            tpiProperty = new TPIProperty();
            tpiProperty.TPI_UID = tpi.UID;
            tpiProperty.Property_UID = tpi.Property_UID.Value;
            tpiProperty.CreditLimit = 120;
            tpiProperty.CreditUsed = null;
            tpiProperty.Commission = 10;
            tpiProperty.CommissionIsPercentage = true;
            tpiPropertyList.Add(tpiProperty);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithTPI(tpi)
                .WithTPICommission(tpiProperty.Commission);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_TPIComissionNull()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi and the tpiproperty
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            TPIProperty tpiProperty = null;

            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            tpi.Property_UID = 1263;

            tpiProperty = new TPIProperty();
            tpiProperty.TPI_UID = tpi.UID;
            tpiProperty.Property_UID = tpi.Property_UID.Value;
            tpiProperty.CreditLimit = 120;
            tpiProperty.CreditUsed = null;
            tpiProperty.Commission = null;
            tpiProperty.CommissionIsPercentage = null;
            tpiPropertyList.Add(tpiProperty);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithTPI(tpi)
                .WithTPICommission(tpiProperty.Commission);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }


        delegate void TreatBeReservationCallback(OB.BL.Operations.Internal.BusinessObjects.TreatBEReservationParameters parameters);
        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_PromotionalCode()
        {
            long errorCode = 0;

            // arrange            
            //Prepare the promocode
            OB.BL.Contracts.Data.Rates.PromotionalCode promoCode = new PromotionalCode()
            {
                UID = 1,
                Name = "PROMOCODE",
                Code = "PROMOCODE",
                MaxReservations = 1,
                ReservationsCompleted = null,
                DiscountValue = 0,
                IsCommission = false,
                IsDeleted = false,
                IsPercentage = false,
                IsPromotionalCodeVisibleRate = true,
                IsRegisterTPI = false,
                IsValid = true,
                //ModifiedDate = null,
                PromotionalCode_UID = null,
                //Property_UID = 1263,
                URL = "www.promocode.com"
            };
            promocodesList.Add(promoCode);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithPromotionalCodes(promoCode);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            var groupRule = new GroupRule();
            groupRule.RuleType = RuleType.BE;
            groupRule.BusinessRules = BusinessRules.BEReservationCalculation;

            #region Mock Setups
            var helper = new OB.BL.Operations.Impl.ReservationHelperPOCO();
            helper.RepositoryFactory = RepositoryFactory;
            helper.Container = Container;
            helper.SessionFactory = SessionFactory;
            Mock<IReservationPricesCalculationPOCO> rpcMock = new Mock<IReservationPricesCalculationPOCO>(MockBehavior.Default);
            rpcMock.Setup(x => x.CalculateReservationRoomPrices(It.IsAny<CalculateFinalPriceParameters>()))
                    .Returns(new List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>
                    {
                        new RateRoomDetailReservation
                        {
                            FinalPrice = resBuilder.InputData.reservationDetail.TotalAmount ?? 0,
                            AppliedPromotionalCode = new Contracts.Data.Reservations.ReservationRoomDetailsAppliedPromotionalCode
                            {
                                PromotionalCode_UID = promoCode.UID,
                            },
                        }
                    });

            helper.Container.RegisterInstance<IReservationPricesCalculationPOCO>(rpcMock.Object);

            _reservationHelperMock.Setup(x => x.TreatBeReservation(It.IsAny<OB.BL.Operations.Internal.BusinessObjects.TreatBEReservationParameters>()))
                .Callback(new TreatBeReservationCallback((OB.BL.Operations.Internal.BusinessObjects.TreatBEReservationParameters parameters) =>
                {
                    helper.TreatBeReservation(parameters);
                }));

            _reservationHelperMock.Setup(x => x.ValidatePromocodeForReservation(It.IsAny<OB.BL.Operations.Internal.BusinessObjects.ValidatePromocodeForReservationParameters>()))
                .Returns(new OB.BL.Operations.Internal.BusinessObjects.ValidPromocodeParameters
                {
                    RejectReservation = false,
                    PromoCodeObj = new PromotionalCode
                    {
                        IsValid = true,
                    },
                    NewDaysToApplyDiscount = new List<DateTime>
                    {
                        DateTime.Now,
                    }
                });
            #endregion

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, promoCode: promoCode, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_PromotionalCodeMaxReservationsLimit()
        {
            long errorCode = 0;

            // arrange            
            IUnitOfWork unitOfWork = null;
            //Prepare the promocode
            OB.BL.Contracts.Data.Rates.PromotionalCode promoCode = new PromotionalCode()
            {
                UID = 1,
                Name = "PROMOCODE",
                Code = "PROMOCODE",
                MaxReservations = 0,
                ReservationsCompleted = null,
                DiscountValue = 0,
                IsCommission = false,
                IsDeleted = false,
                IsPercentage = false,
                IsPromotionalCodeVisibleRate = true,
                IsRegisterTPI = false,
                IsValid = true,
                //ModifiedDate = null,
                //PromotionalCode_UID = 1,
                //Property_UID = 1263,
                URL = "www.promocode.com"
            };
            promocodesList.Add(promoCode);

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithPromotionalCodes(promoCode);

            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, promoCode: promoCode);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));

            var promotionalCodeRepo = this.RepositoryFactory.GetOBPromotionalCodeRepository();
            promoCode = promotionalCodeRepo.ListPromotionalCode(new ListPromotionalCodeRequest() { PromoCodeIds = new List<long>() { promoCode.UID } }).SingleOrDefault();
            Assert.AreEqual(null, promoCode.ReservationsCompleted);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_Mobile()
        {
            long errorCode = 0;

            // arrange     
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithMobile()
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        [ExpectedException(typeof(PaymentGatewayException))]
        public void TestInsertReservation_BookingEngine_PaymentGateway_InvalidCard()
        {
            long errorCode = 0;

            // arrange
            //Prepare the configs
            PaymentGatewayConfiguration paymentGatewayConfiguration =
                listPaymentGetwayConfigs.Where(x => x.UID == 17).SingleOrDefault();
            paymentGatewayConfiguration.PropertyUID = 1263;
            var tmp = paymentGatewayConfiguration.Clone();
            listPaymentGetwayConfigs.Remove(paymentGatewayConfiguration);
            listPaymentGetwayConfigs.Add(tmp);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithMobile().WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithInvalidCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;


            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_PaymentGateway_DifferentCUrrencies()
        {
            long errorCode = 0;

            // arrange
            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                    .Returns((ListActivePaymentGatewayConfigurationRequest request) =>
                    {

                        return listPaymentGetwayConfigs.FirstOrDefault();
                    });
            //Prepare the configs
            PaymentGatewayConfiguration paymentGatewayConfiguration =
                listPaymentGetwayConfigs.Where(x => x.UID == 17).SingleOrDefault();
            paymentGatewayConfiguration.PropertyUID = 1263;
            var tmp = paymentGatewayConfiguration.Clone();
            listPaymentGetwayConfigs.Remove(paymentGatewayConfiguration);
            listPaymentGetwayConfigs.Add(tmp);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true, resCurrencyId: 109, rateCurrencyId: 16).WithMobile()
                .WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            var groupRule = new GroupRule();
            groupRule.BusinessRules = BusinessRules.ConvertValuesFromClientToRates;

            //Mock the echangeRate
            var exchangeRate = 0.5M;
            this._reservationHelperMock.Setup(x => x.GetExchangeRateBetweenCurrenciesByPropertyId(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(exchangeRate);

            var totalAmount = Math.Round((decimal)(resBuilder.InputData.reservationPaymentDetail.Amount * exchangeRate), 2, MidpointRounding.AwayFromZero);

            //Mock a gateway method to allow authorize
            //We can use another gateway
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
            _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            Assert.AreEqual(16, reservation.ReservationBaseCurrency_UID);
            Assert.AreEqual(109, reservation.ReservationCurrency_UID);
            //Verify of we call the method was called with the specific ammount
            maxiPagoMock.Verify(x => x.Authorize(It.IsAny<string>(), It.Is<decimal>(a => a == totalAmount), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);




        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_Company()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            var tmpTpi = tpi.Clone();
            tmpTpi.Property_UID = 1263;
            tmpTpi.IsCompany = true;
            tpisList.Remove(tpi);
            tpisList.Add(tmpTpi);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithTPI(tpi);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;


            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_ChildsWithAges()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithChildren(1, 2, new List<int>() { 11, 3 });
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: true, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_ChildsWithNullAges()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithChildren(1, 2, null);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: true, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        //Also commented in OB API
        ////[TestMethod]
        ////[TestCategory("InsertReservation_BookingEngine")]
        ////[DeploymentItem("./Databases_WithData", "TestInsertReservation_BookingEngine_PaymentGatewayCancelTransaction")]
        ////[DeploymentItem("OB.DL.Model.Channels.dll")]
        ////[DeploymentItem("OB.DL.Model.CRM.dll")]
        ////[DeploymentItem("OB.DL.Model.General.dll")]
        ////[DeploymentItem("OB.DL.Model.Payments.dll")]
        ////[DeploymentItem("OB.DL.Model.PMS.dll")]
        ////[DeploymentItem("OB.DL.Model.Payments.dll")]
        ////[DeploymentItem("OB.DL.Model.ProactiveActions.dll")]
        ////[DeploymentItem("OB.DL.Model.Properties.dll")]
        ////[DeploymentItem("OB.DL.Model.Rates.dll")]
        ////[DeploymentItem("OB.DL.Model.Reservations.dll")]
        ////[DeploymentItem("EntityFramework.SqlServer.dll")]
        ////[DeploymentItem("EntityFramework.dll")]
        ////public void TestInsertReservation_BookingEngine_PaymentGatewayCancelTransaction()
        ////{
        ////    // arrange            
        ////    IUnitOfWork unitOfWork = null;
        ////    PaymentGatewayConfiguration paymentGatewayConfiguration = null;
        ////    IRepository<PaymentGatewayConfiguration> paymentGatewayConfigurationRepo = null;

        ////    using (unitOfWork = this.SessionFactory.GetUnitOfWork())
        ////    {
        ////        paymentGatewayConfigurationRepo = this.RepositoryFactory.GetPaymentGatewayConfigurationRepository(unitOfWork);

        ////        paymentGatewayConfiguration = paymentGatewayConfigurationRepo.Single(x => x.UID == 17);
        ////        paymentGatewayConfiguration.PropertyUID = 1263;
        ////        paymentGatewayConfigurationRepo.AttachAsModified(paymentGatewayConfiguration);
        ////        unitOfWork.Save();
        ////    }
        ////    var builder = new ReservationDataBuilder(1).WithMobile().WithNewGuest().WithRoom(1, 1).WithCreditCardPayment();

        ////    var today = DateTime.Today;
        ////    var depositoryPolicyMock = new Mock<IRateRoomRepository>(MockBehavior.Default);
        ////    depositoryPolicyMock.Setup(mocked => mocked.FindRatePoliciesByRateUID_And_ReservationCurrencyId_And_CheckInDate(
        ////        It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>())).Callback(() =>
        ////        {
        ////            throw new Exception("Simulated Exception");
        ////        });

        ////    this.Container.RegisterInstance(typeof(IRateRoomRepository), depositoryPolicyMock.Object, new ContainerControlledLifetimeManager());


        ////    var paymentGatewayServiceMock = new Mock<PaymentGatewayFactory>(MockBehavior.Loose);

        ////    //We don't want to test the Sale in here
        ////    paymentGatewayServiceMock.Setup(mocked => mocked.MaxiPago(new PaymentGatewayConfig()).Sale(It.Is<long>(x => x == 1263),
        ////                                                            It.Is<decimal>(x => x == 110),
        ////                                                            It.Is<int>(x => x == 1),
        ////                                                            It.Is<int>(x => x == 2016),
        ////                                                            It.Is<string>(x => x == "111"),
        ////                                                            It.Is<string>(x => x == "4929828969879794"),
        ////                                                            It.Is<string>(x => x == "1"),
        ////                                                            It.Is<string>(x => x == "EUR"),
        ////                                                            It.Is<string>(x => x == "Name: Test1 Teste2 - Email: tony.santos@visualforma.pt")))
        ////                              .Returns(() => new PaymentResultMessage
        ////                              {
        ////                                  Error = null,
        ////                                  IsTransactionValid = true,
        ////                                  PaymentAmountCaptured = 110,
        ////                                  PaymentGatewayAutoGeneratedUID = "6354001381467154",
        ////                                  PaymentGatewayName = "MaxiPago",
        ////                                  PaymentGatewayOrderID = "C0A8013F:0146FD9014B4:9D08:012ABE8B",
        ////                                  PaymentGatewayProcessorName = "TEST SIMULATOR",
        ////                                  PaymentGatewayTransactionDateTime = DateTime.Now,
        ////                                  PaymentGatewayTransactionID = "721795",
        ////                                  PaymentGatewayTransactionStatusCode = "0",
        ////                                  PaymentGatewayTransactionType = "Auth",
        ////                                  TransactionMessage = "AUTHORIZED"
        ////                              });


        ////    //We just want to test that Cancel was called.
        ////    paymentGatewayServiceMock.Setup(mocked => mocked.CancelTransaction(It.Is<long>(x => x == 1263),
        ////       It.Is<string>(x => x == "721795"), It.Is<string>(x => x == "C0A8013F:0146FD9014B4:9D08:012ABE8B"), It.Is<string>(x => x == "6354001381467154"), It.Is<decimal>(x => x == 110),
        ////       It.IsAny<DateTime>(), It.Is<string>((x => x == "Name: Test1 Teste2 Email: tony.santos@visualforma.pt"))))
        ////       .Returns(() => new PaymentResultMessage
        ////       {
        ////           Error = null,
        ////           ErrorMessage = "",
        ////           IsTransactionValid = true,
        ////           PaymentAmountCaptured = 110,
        ////           PaymentGatewayAutoGeneratedUID = "6354001409748529",
        ////           PaymentGatewayName = "MaxiPago",
        ////           PaymentGatewayOrderID = "C0A8013F:0146FD946481:5D02:019E6638",
        ////           PaymentGatewayProcessorName = "TEST SIMULATOR",
        ////           PaymentGatewayTransactionDateTime = DateTime.Now,
        ////           PaymentGatewayTransactionID = "721800",
        ////           PaymentGatewayTransactionStatusCode = "0",
        ////           PaymentGatewayTransactionType = "Void",
        ////           TransactionMessage = "VOIDED"
        ////       });


        ////    Container.RegisterInstance(typeof(IPaymentGatewayManagerPOCO), paymentGatewayServiceMock.Object, new ContainerControlledLifetimeManager());

        ////    // act
        ////    long result = this.InsertReservation(builder);

        ////    // assert            
        ////    var databaseData = GetReservationDataFromDatabase(builder.ExpectedData.reservationDetail.Number);
        ////    Assert.IsNull(databaseData);
        ////    Assert.AreEqual(result, 0);
        ////    paymentGatewayServiceMock.VerifyAll();
        ////}

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_GuestActivities()
        {
            long errorCode = 0;

            //Prepare the guest activity
            OB.Reservation.BL.Contracts.Data.CRM.GuestActivity guestActivity = null;
            guestActivity = guestActivityList.Where(x => x.UID == 2451).FirstOrDefault();

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithMobile()
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithGuestActivity(guestActivity);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            resBuilder.ExpectedData.guestActivityObj.Add(new OB.Reservation.BL.Contracts.Data.CRM.GuestActivity { Guest_UID = databaseData.guest.UID, Activity_UID = guestActivity.UID });
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_GuestExistingActivities()
        {
            long errorCode = 0;

            //Prepare the guest and the activity
            OB.Reservation.BL.Contracts.Data.CRM.GuestActivity guestActivity = null;
            guestActivity = guestActivityList.Where(x => x.UID == 2451).FirstOrDefault();
            guestActivity.Guest_UID = 63774;
            var tmpGuestActivity = guestActivity.Clone();
            guestActivityList.Remove(guestActivity);
            guestActivityList.Add(tmpGuestActivity);

            contractsCRM.Guest guest = guestsList.Where(x => x.UID == 63774).SingleOrDefault();
            var guestTmp = guest.Clone();
            guestTmp.UserName = "testemail@email.com";
            guestTmp.Email = "testemail@email.com";
            guestTmp.GuestActivities = new List<contractsCRM.GuestActivity>() {
                new contractsCRM.GuestActivity()
                {
                    UID = tmpGuestActivity.UID,
                    Guest_UID = guest.UID,
                    Activity_UID = tmpGuestActivity.UID
                }
            };
            guestsList.Remove(guest);
            guestsList.Add(guestTmp);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithExistingGuest(OtherConverter.Convert(guestTmp), true)
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = guestTmp.UID;
            resBuilder.ExpectedData.reservationDetail.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, guestUid: guestTmp.UID);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            resBuilder.ExpectedData.guestActivityObj.Add(guestActivity);
            ReservationData databaseData = GetReservationDataFromDatabase(resBuilder.ExpectedData.reservationDetail.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_SpecialRequest()
        {
            long errorCode = 0;

            // arrange
            IUnitOfWork unitOfWork = null;
            List<OB.BL.Contracts.Data.BE.BESpecialRequest> specialRequests = null;
            ReservationDataBuilder resBuilder = null;
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            string transactionId = null;
            var obCrmRepo = this.RepositoryFactory.GetOBCRMRepository();

            using (unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // TODO: CORRIGIR. Este teste nao funciona está a alterar um contract em vez de um domain para gravar.

                specialRequests = obCrmRepo.ListBESpecialRequestForReservation(new ListBESpecialRequestForReservationRequest() { }).ToList();
                foreach (var request in specialRequests)
                    request.Property_UID = 1263;

                unitOfWork.Save();

                resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithMobile().WithNewGuest()
                    .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment()
                    .WithSpecialRequest(specialRequests[0], specialRequests[1], specialRequests[2], specialRequests[3]);
                transactionId = Guid.NewGuid().ToString();
                resBuilder.InputData.guest.UID = 1;
                resBuilder.ExpectedData.reservationDetail.UID = 1;
            }


            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;


            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(resBuilder.ExpectedData.reservationDetail.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);


            var guestUID = databaseData.reservationDetail.Guest_UID;
            var guest = obCrmRepo.ListGuestsByLightCriteria(new ListGuestLightRequest() { UIDs = new List<long>() { guestUID } }).SingleOrDefault();

            foreach (var request in guest.GuestFavoriteSpecialRequests)
            {
                var guestFavoriteSpecialRequest = obCrmRepo.ListBESpecialRequestForReservation(new ListBESpecialRequestForReservationRequest()
                {
                    BESpecialRequestsIds = new List<long>() { request.BESpecialRequests_UID }
                }).FirstOrDefault();
                Assert.AreEqual(guestFavoriteSpecialRequest.UID, request.BESpecialRequests_UID);
            }
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_PartialPayment()
        {
            long errorCode = 0;

            // arrange
            //Prepare the partial payment
            BEPartialPaymentCCMethod partialPayment = partialPayment = new BEPartialPaymentCCMethod
            {
                UID = 1,
                InterestRate = 11,
                Parcel = 3,
                Property_UID = 1263,
                PaymentMethods_UID = 1
            };
            partialPaymentCCMethodList.Add(partialPayment);
            decimal installmentAmount = (110 * (1 + (partialPayment.InterestRate.Value / (decimal)100.0))) / partialPayment.Parcel;

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithPartialPayment(partialPayment, installmentAmount);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            resBuilder.ExpectedData.reservationDetail.UID = 1;


            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;


            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(resBuilder.ExpectedData.reservationDetail.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
            Assert.AreEqual(partialPayment.Parcel, databaseData.reservationPartialPaymentDetail.Count);
            for (int i = 0; i < partialPayment.Parcel; i++)
            {
                ReservationPartialPaymentDetail entry = databaseData.reservationPartialPaymentDetail[i];
                Assert.AreEqual(installmentAmount, entry.Amount);
                Assert.AreEqual(DateTime.UtcNow.Date, entry.CreatedDate.Date);
                Assert.AreEqual(i + 1, entry.InstallmentNo);
                Assert.AreEqual(partialPayment.InterestRate, entry.InterestRate);
                Assert.IsFalse(entry.IsPaid);
                Assert.AreEqual(DateTime.UtcNow.Date, entry.ModifiedDate.Date);
            }
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_GroupCode()
        {
            MockLookups();

            long errorCode = 0;

            // arrange
            //Prepare the group
            IUnitOfWork unitOfWork = null;
            IOBRateRepository groupCodeRepo = null;
            GroupCode groupCode = null;
            using (unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // TODO: CORRIGIR. Este metodo nao funciona porque está a alterar um contract em vez de um domain para gravar.

                groupCodeRepo = this.RepositoryFactory.GetOBRateRepository();
                groupCode = groupCodeRepo.ListGroupCodesForReservation(new ListGroupCodesForReservationRequest() { })
                    .FirstOrDefault(x => x.Property_UID == 1635);
                groupCode.Property_UID = 1263;
                groupCode.Rate_UID = 4639;
                unitOfWork.Save();
            }

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithGroupCode(groupCode);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_TransferLocation()
        {
            long errorCode = 0;

            // arrange
            //Prepare the transfer location
            IUnitOfWork unitOfWork = null;
            TransferLocation transferLocation = null;
            using (unitOfWork = SessionFactory.GetUnitOfWork())
            {
                // TODO: CORRIGIR. Este metodo nao funciona, pois está a modificar contracts em vez de domains.

                var transferLocationRepo = RepositoryFactory.GetOBPropertyRepository();
                transferLocation = transferLocationRepo.ListTransferLocationsForReservation(new ListTransferLocationsForReservationRequest() { }).FirstOrDefault(x => x.Property_UID == 1635);
                transferLocation.Property_UID = 1263;
                unitOfWork.Save();
            }

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithTransferLocation(transferLocation);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_OnRequest()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false, onRequest: true).WithCreditCardPayment().WithBookingOnRequest();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber | BusinessRules.ValidateAllotment,
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263, withChildsRoom: false,
                withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, validateAllot: true, groupRule: groupRule,
                onRequestEnable: true);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_Policies()
        {
            long errorCode = 0;

            #region Extra Mocks
            MockDepositPolicyRepo(true);
            MockCancelationPolicyRepo(true);
            MockOtherPolicyRepo(true);
            #endregion

            // arrange
            //Prepare the policies
            IUnitOfWork unitOfWork = null;
            DateTime dateCheckin;
            DateTime dateCheckout;
            CancellationPolicy cancellationPolicy = null;
            OtherPolicy otherPolicy = null;
            DepositPolicy depositPolicy = null;
            using (unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var nowPlusOne = DateTime.UtcNow.AddDays(1);
                dateCheckin = new DateTime(nowPlusOne.Year, nowPlusOne.Month, nowPlusOne.Day);
                dateCheckout = dateCheckin.AddDays(2);
                int daysDifference = dateCheckout.Subtract(dateCheckin).Days;

                var depositPolicyRepo = RepositoryFactory.GetOBDepositPolicyRepository();

                var rate = listRates.FirstOrDefault(r => r.UID == 4639);

                cancellationPolicy = cancellationPolicyList.FirstOrDefault(x => x.Property_UID == 1635);
                cancellationPolicy.Property_UID = 1263;
                rate.CancellationPolicy_UID = cancellationPolicy.UID;

                depositPolicy = depositPolicyRepo.ListDepositPolicies(new ListDepositPoliciesRequest() { RateId = rateUidMock }).FirstOrDefault(x => x.Property_UID == 1635);
                depositPolicy.Property_UID = 1263;
                rate.DepositPolicy_UID = depositPolicy.UID;


                otherPolicy = otherPolicyList.FirstOrDefault(x => x.Property_UID == 1635);
                otherPolicy.Property_UID = 1263;
                rate.OtherPolicy_UID = otherPolicy.UID;


                // add rate room details
                List<RateRoomDetail> rateRoomDetails = rateRoomDetailList.AsEnumerable().Where(x => x.RateRoom_UID == 16650).Take(daysDifference).ToList();
                DateTime temp = new DateTime(dateCheckin.Ticks);
                foreach (RateRoomDetail detail in rateRoomDetails)
                {
                    var detailTmp = detail.Clone();
                    detailTmp.Date = temp;
                    temp = temp.AddDays(1);
                    detailTmp.DepositPolicy_UID = null;
                    detailTmp.CancellationPolicy_UID = null;

                    //replace in the list
                    rateRoomDetailList.Remove(detail);
                    rateRoomDetailList.Add(detailTmp);
                }

                unitOfWork.Save();
            }

            #region Extra Mocks
            _reservationHelperMock.Setup(x => x.GetMostRestrictiveDepositPolicy(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>>()))
                .Returns((DateTime checkIn, DateTime checkOut, long? rateId, long? currencyId, long? languageId, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList) =>
                {
                    return depositPolicy;
                });
            #endregion

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, dateCheckin, dateCheckout, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithLanguage(3);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            resBuilder.InputData.reservationDetail.CancellationPolicy = "1";
            resBuilder.InputData.reservationDetail.DepositPolicy = "2";
            resBuilder.InputData.reservationDetail.OtherPolicy = "3";
            resBuilder.InputData.reservationDetail.CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.InputData.reservationRooms[0].CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.InputData.reservationRooms[0].IsCancellationAllowed = cancellationPolicy.IsCancellationAllowed;
            resBuilder.InputData.reservationRooms[0].CancellationCosts = cancellationPolicy.CancellationCosts ?? false;
            resBuilder.InputData.reservationRooms[0].CancellationValue = cancellationPolicy.Value;
            resBuilder.InputData.reservationRooms[0].CancellationPaymentModel = cancellationPolicy.PaymentModel;
            resBuilder.InputData.reservationRooms[0].CancellationNrNights = cancellationPolicy.NrNights;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, handleCancelationCosts: true);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            resBuilder.ExpectedData.reservationDetail.CancellationPolicy = "1";
            resBuilder.ExpectedData.reservationDetail.DepositPolicy = "2";
            resBuilder.ExpectedData.reservationDetail.OtherPolicy = "3";
            resBuilder.ExpectedData.reservationDetail.CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPolicy = cancellationPolicy.Name + " : " + cancellationPolicy.Description;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.ExpectedData.reservationRooms[0].IsCancellationAllowed = cancellationPolicy.IsCancellationAllowed;
            resBuilder.ExpectedData.reservationRooms[0].CancellationCosts = cancellationPolicy.CancellationCosts ?? false;
            resBuilder.ExpectedData.reservationRooms[0].CancellationValue = cancellationPolicy.Value;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPaymentModel = cancellationPolicy.PaymentModel;
            resBuilder.ExpectedData.reservationRooms[0].CancellationNrNights = cancellationPolicy.NrNights;
            resBuilder.ExpectedData.reservationRooms[0].DepositPolicy = depositPolicy.Name + " : " + depositPolicy.Description;
            resBuilder.ExpectedData.reservationRooms[0].OtherPolicy = otherPolicy.OtherPolicy_Name + " : " + otherPolicy.OtherPolicy_Description;

            using (unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                var afterReservationData = GetReservationDataFromDatabase(reservation.UID, unitOfWork: unitOfWork);
                ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, afterReservationData, verifyPolicies: true);
            }
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_PoliciesTranslated()
        {
            long errorCode = 0;

            // arrange            
            IUnitOfWork unitOfWork = null;
            CancellationPolicy cancellationPolicy = null;
            OtherPolicy otherPolicy = null;
            DepositPolicy depositPolicy = null;
            CancellationPoliciesLanguage cancellationPolicyLanguage = null;
            DepositPoliciesLanguage depositPolicyLanguage = null;
            Domain.OtherPoliciesLanguage otherPolicyLanguage = null;

            DateTime dateCheckin;
            DateTime dateCheckout;

            using (unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                var nowPlusOne = DateTime.UtcNow.AddDays(1);
                dateCheckin = new DateTime(nowPlusOne.Year, nowPlusOne.Month, nowPlusOne.Day);
                dateCheckout = dateCheckin.AddDays(2);
                int daysDifference = dateCheckout.Subtract(dateCheckin).Days;

                var depositPolicyRepo = RepositoryFactory.GetOBDepositPolicyRepository();

                var rate = listRates.FirstOrDefault(r => r.UID == 4639);

                cancellationPolicy = cancellationPolicyList.FirstOrDefault(x => x.Property_UID == 1635);
                cancellationPolicy.Property_UID = 1263;
                rate.CancellationPolicy_UID = cancellationPolicy.UID;
                cancellationPolicyLanguage = cancellationPolicyLanguageList.FirstOrDefault(x => x.Language_UID == 3
                    && x.CancellationPolicies_UID == cancellationPolicy.UID);

                depositPolicy = depositPolicyRepo.ListDepositPolicies(new ListDepositPoliciesRequest() { RateId = rateUidMock }).FirstOrDefault(x => x.Property_UID == 1635);
                depositPolicy.Property_UID = 1263;

                rate.DepositPolicy_UID = depositPolicy.UID;
                depositPolicyLanguage = depositPolicyLanguageList.FirstOrDefault(x => x.Language_UID == 3
                    && x.DepositPolicy_UID == depositPolicy.UID);

                depositPolicy.TranslatedName = depositPolicyLanguage.Name;
                depositPolicy.TranslatedDescription = depositPolicyLanguage.Description;

                otherPolicy = otherPolicyList.FirstOrDefault(x => x.Property_UID == 1635);
                otherPolicy.Property_UID = 1263;
                rate.OtherPolicy_UID = otherPolicy.UID;
                otherPolicyLanguage = new Domain.OtherPoliciesLanguage
                {
                    CreatedDate = DateTime.UtcNow,
                    Description = "Test Description",
                    Language_UID = 3,
                    Name = "Test Name",
                    OtherPolicy_UID = otherPolicy.UID
                };

                otherPolicyLanguageList.Add(otherPolicyLanguage);

                // add rate room details
                List<RateRoomDetail> rateRoomDetails = rateRoomDetailList.AsEnumerable().Where(x => x.RateRoom_UID == 16650).Take(daysDifference).ToList();
                DateTime temp = new DateTime(dateCheckin.Ticks);
                foreach (RateRoomDetail detail in rateRoomDetails)
                {
                    var detailTmp = detail.Clone();
                    detailTmp.Date = temp;
                    temp = temp.AddDays(1);

                    //replace in the list
                    rateRoomDetailList.Remove(detail);
                    rateRoomDetailList.Add(detailTmp);
                }

                unitOfWork.Save();
            }

            #region Extra Mocks
            MockDepositPolicyRepo(true);
            MockCancelationPolicyRepo(true);
            MockOtherPolicyRepo(true);

            _reservationHelperMock.Setup(x => x.GetMostRestrictiveDepositPolicy(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>>()))
                .Returns((DateTime checkIn, DateTime checkOut, long? rateId, long? currencyId, long? languageId, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList) =>
                {
                    return depositPolicy;
                });
            #endregion

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, dateCheckin, dateCheckout, ignoreBEResNumber: true, cancelationAllowed: true).WithCreditCardPayment().WithLanguage(2);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            resBuilder.InputData.reservationDetail.CancellationPolicy = "1";
            resBuilder.InputData.reservationDetail.DepositPolicy = "2";
            resBuilder.InputData.reservationDetail.OtherPolicy = "3";
            resBuilder.InputData.reservationDetail.CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.InputData.reservationRooms[0].CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.InputData.reservationRooms[0].IsCancellationAllowed = cancellationPolicy.IsCancellationAllowed;
            resBuilder.InputData.reservationRooms[0].CancellationCosts = cancellationPolicy.CancellationCosts ?? false;
            resBuilder.InputData.reservationRooms[0].CancellationValue = cancellationPolicy.Value;
            resBuilder.InputData.reservationRooms[0].CancellationPaymentModel = cancellationPolicy.PaymentModel;
            resBuilder.InputData.reservationRooms[0].CancellationNrNights = cancellationPolicy.NrNights;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: true, channelUid: 1, resNumber: resNumberExpected,
                addTranslations: true, handleCancelationCosts: true);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            resBuilder.ExpectedData.reservationDetail.CancellationPolicy = "1";
            resBuilder.ExpectedData.reservationDetail.DepositPolicy = "2";
            resBuilder.ExpectedData.reservationDetail.OtherPolicy = "3";
            resBuilder.ExpectedData.reservationDetail.CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPolicy = cancellationPolicyLanguage.Name + " : " + cancellationPolicyLanguage.Name;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.ExpectedData.reservationRooms[0].IsCancellationAllowed = cancellationPolicy.IsCancellationAllowed;
            resBuilder.ExpectedData.reservationRooms[0].CancellationCosts = cancellationPolicy.CancellationCosts ?? false;
            resBuilder.ExpectedData.reservationRooms[0].CancellationValue = cancellationPolicy.Value;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPaymentModel = cancellationPolicy.PaymentModel;
            resBuilder.ExpectedData.reservationRooms[0].CancellationNrNights = cancellationPolicy.NrNights;
            resBuilder.ExpectedData.reservationRooms[0].DepositPolicy = depositPolicyLanguage.Name + " : " + depositPolicyLanguage.Description;
            resBuilder.ExpectedData.reservationRooms[0].OtherPolicy = otherPolicyLanguage.Name + " : " + otherPolicyLanguage.Description;

            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID), verifyPolicies: true);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_PoliciesTranslatedMissedLang()
        {
            long errorCode = 0;

            // arrange            
            IUnitOfWork unitOfWork = null;
            CancellationPolicy cancellationPolicy = null;
            OtherPolicy otherPolicy = null;
            DepositPolicy depositPolicy = null;
            CancellationPoliciesLanguage cancellationPolicyLanguage = null;
            DepositPoliciesLanguage depositPolicyLanguage = null;
            Domain.OtherPoliciesLanguage otherPolicyLanguage = null;

            DateTime dateCheckin;
            DateTime dateCheckout;

            using (unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var nowPlusOne = DateTime.UtcNow.AddDays(1);
                dateCheckin = new DateTime(nowPlusOne.Year, nowPlusOne.Month, nowPlusOne.Day);
                dateCheckout = dateCheckin.AddDays(2);
                int daysDifference = dateCheckout.Subtract(dateCheckin).Days;

                var depositPolicyRepo = RepositoryFactory.GetOBDepositPolicyRepository();

                var rate = listRates.FirstOrDefault(r => r.UID == 4639);

                cancellationPolicy = cancellationPolicyList.FirstOrDefault(x => x.Property_UID == 1635);
                cancellationPolicy.Property_UID = 1263;
                rate.CancellationPolicy_UID = cancellationPolicy.UID;
                cancellationPolicyLanguage = cancellationPolicyLanguageList.FirstOrDefault(x => x.Language_UID == 3 && x.CancellationPolicies_UID == cancellationPolicy.UID);

                depositPolicy = depositPolicyRepo.ListDepositPolicies(new ListDepositPoliciesRequest() { RateId = rateUidMock }).FirstOrDefault(x => x.Property_UID == 1635);
                depositPolicy.Property_UID = 1263;
                rate.DepositPolicy_UID = depositPolicy.UID;
                depositPolicyLanguage = depositPolicyLanguageList.FirstOrDefault(x => x.Language_UID == 3 && x.DepositPolicy_UID == depositPolicy.UID);

                otherPolicy = otherPolicyList.FirstOrDefault(x => x.Property_UID == 1635);
                otherPolicy.Property_UID = 1263;
                rate.OtherPolicy_UID = otherPolicy.UID;
                otherPolicyLanguage = new Domain.OtherPoliciesLanguage
                {
                    CreatedDate = DateTime.UtcNow,
                    Description = "Test Description",
                    Language_UID = 3,
                    Name = "Test Name",
                    OtherPolicy_UID = otherPolicy.UID
                };
                otherPolicyLanguageList.Add(otherPolicyLanguage);

                // add rate room details
                List<RateRoomDetail> rateRoomDetails = rateRoomDetailList.AsEnumerable().Where(x => x.RateRoom_UID == 16650).Take(daysDifference).ToList();
                DateTime temp = new DateTime(dateCheckin.Ticks);
                foreach (RateRoomDetail detail in rateRoomDetails)
                {
                    var detailTmp = detail.Clone();
                    detailTmp.Date = temp;
                    temp = temp.AddDays(1);

                    //replace in the list
                    rateRoomDetailList.Remove(detail);
                    rateRoomDetailList.Add(detailTmp);
                }
                unitOfWork.Save();
            }

            #region Extra Mocks
            MockDepositPolicyRepo(true);
            MockCancelationPolicyRepo(true);
            MockOtherPolicyRepo(true);

            _reservationHelperMock.Setup(x => x.GetMostRestrictiveDepositPolicy(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>>()))
                .Returns((DateTime checkIn, DateTime checkOut, long? rateId, long? currencyId, long? languageId, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList) =>
                {
                    return depositPolicy;
                });
            #endregion

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, dateCheckin, dateCheckout, ignoreBEResNumber: true, cancelationAllowed: true).WithCreditCardPayment().WithLanguage(5);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            resBuilder.InputData.reservationDetail.CancellationPolicy = "1";
            resBuilder.InputData.reservationDetail.DepositPolicy = "2";
            resBuilder.InputData.reservationDetail.OtherPolicy = "3";
            resBuilder.InputData.reservationDetail.CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.InputData.reservationRooms[0].CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.InputData.reservationRooms[0].IsCancellationAllowed = cancellationPolicy.IsCancellationAllowed;
            resBuilder.InputData.reservationRooms[0].CancellationCosts = cancellationPolicy.CancellationCosts ?? false;
            resBuilder.InputData.reservationRooms[0].CancellationValue = cancellationPolicy.Value;
            resBuilder.InputData.reservationRooms[0].CancellationPaymentModel = cancellationPolicy.PaymentModel;
            resBuilder.InputData.reservationRooms[0].CancellationNrNights = cancellationPolicy.NrNights;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: true, channelUid: 1, resNumber: resNumberExpected,
                handleCancelationCosts: true);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            resBuilder.ExpectedData.reservationDetail.CancellationPolicy = "1";
            resBuilder.ExpectedData.reservationDetail.DepositPolicy = "2";
            resBuilder.ExpectedData.reservationDetail.OtherPolicy = "3";
            resBuilder.ExpectedData.reservationDetail.CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPolicy = "This information does not exist in your language...! \n " + cancellationPolicy.Name;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPolicyDays = cancellationPolicy.Days;
            resBuilder.ExpectedData.reservationRooms[0].IsCancellationAllowed = cancellationPolicy.IsCancellationAllowed;
            resBuilder.ExpectedData.reservationRooms[0].CancellationCosts = cancellationPolicy.CancellationCosts ?? false;
            resBuilder.ExpectedData.reservationRooms[0].CancellationValue = cancellationPolicy.Value;
            resBuilder.ExpectedData.reservationRooms[0].CancellationPaymentModel = cancellationPolicy.PaymentModel;
            resBuilder.ExpectedData.reservationRooms[0].CancellationNrNights = cancellationPolicy.NrNights;
            resBuilder.ExpectedData.reservationRooms[0].DepositPolicy = "This information does not exist in your language...! \n " + depositPolicy.Name;
            resBuilder.ExpectedData.reservationRooms[0].OtherPolicy = "This information does not exist in your language...! \n " + otherPolicy.OtherPolicy_Name;

            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID), verifyPolicies: true);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_Incentives()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithIncentives(1);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: true, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_IsValidated()
        {
            MockLookups();

            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, propertyId: 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            var groupRule = new GroupRule
            {
                BusinessRules = BusinessRules.ValidateRestrictions
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
            Assert.IsTrue(ValidateReservationV2Called, "Expected ValidateReservationV2Called SP to be called");
        }

        [TestMethod]
        [TestCategory("InsertReservation_BookingEngine")]
        public void TestInsertReservation_BookingEngine_SaveAdditionalData_SaveHistory_SendLog()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1806";
            var builder = new Operations.Helper.SearchBuilder(Container, 1806);
            var resBuilder = new ReservationDataBuilder(1, propertyId: 1806, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                .WithNewGuest().WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1806,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));

            _resAddDataRepoMock.Verify(x => x.Add(It.IsAny<ReservationsAdditionalData>()), Times.Once);
            _ReservationHistoryRepoMock.Verify(x => x.Add(It.IsAny<ReservationsHistory>()), Times.Once);
            _eventSystemManagerPocoMock.Verify(x => x.SendMessage(It.IsAny<OB.Events.Contracts.ICustomNotification>()), Times.Once);
        }

        #endregion

        #region CHANNELS
        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_PartialPayment()
        {
            long errorCode = 0;

            // arrange
            //Prepare the partial payment
            BEPartialPaymentCCMethod partialPayment = partialPayment = new BEPartialPaymentCCMethod
            {
                InterestRate = 11,
                Parcel = 3,
                Property_UID = 1263,
                PaymentMethods_UID = 1
            };
            partialPaymentCCMethodList.Add(partialPayment);
            decimal installmentAmount = (110 * (1 + (partialPayment.InterestRate.Value / (decimal)100.0))) / partialPayment.Parcel;

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            ReservationDataBuilder resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment().WithPartialPayment(partialPayment, installmentAmount);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
            Assert.AreEqual(0, databaseData.reservationPartialPaymentDetail.Count);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_SalesmanComission()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi, salesman and salesman commission
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = tpisList.SingleOrDefault(x => x.UID == 61);
            var tmpTpi = tpi.Clone();
            tmpTpi.Property_UID = 1263;
            tpisList.Remove(tpi); tpisList.Add(tmpTpi);

            Domain.CRM.Salesman salesman = salesManList.First(x => x.UID == 4);
            var salesmanTmp = salesman.Clone();
            salesmanTmp.Property_UID = 1263;
            salesManList.Remove(salesman); salesManList.Add(salesmanTmp);

            SalesmanThirdPartyIntermediariesComission comission = salesmanTPICommissionsList.First(x => x.UID == 2);
            var comissionTmp = comission.Clone();
            comissionTmp.ThirdPartyIntermediariesUID = tpi.UID;
            comissionTmp.SalesmanUID = salesman.UID;
            comissionTmp.SalesmanBaseCommission = 10;
            comissionTmp.SalesmanIsBaseCommissionPercentage = false;
            salesmanTPICommissionsList.Remove(comission); salesmanTPICommissionsList.Add(comissionTmp);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithTPI(tpi).WithSalesmanComission(salesman, comission).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_ReservationAdditionalData()
        {
            long errorCode = 0;

            _reservationHelperMock.Setup(x => x.GetCancelationCosts(It.IsAny<contractsReservations.Reservation>(), It.IsAny<bool>(), It.IsAny<DateTime?>()))
                .Returns(new List<contractsReservations.ReservationRoomCancelationCost> { });

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                AgencyPCC = "1234",
                AgencyAddress = "AAAAA",
                AgencyName = "BBBBBB",
                AgencyPhone = "123123123"
            };
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPCC, additionalData.AgencyPCC);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyAddress, additionalData.AgencyAddress);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyName, additionalData.AgencyName);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPhone, additionalData.AgencyPhone);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow(false)]
        [DataRow(true)]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_ReservationAdditionalDataBookerIsGenius(bool? bookerIsGenius)
        {
            long errorCode = 0;

            _reservationHelperMock.Setup(x => x.GetCancelationCosts(It.IsAny<contractsReservations.Reservation>(), It.IsAny<bool>(), It.IsAny<DateTime?>()))
                .Returns(new List<contractsReservations.ReservationRoomCancelationCost> { });

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                BookerIsGenius = bookerIsGenius,
            };
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.BookerIsGenius, additionalData.BookerIsGenius);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_ReservationAdditionalData_With_Parameter_ChannelReservationRoomId()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                AgencyPCC = "1234",
                AgencyAddress = "AAAAA",
                AgencyName = "BBBBBB",
                AgencyPhone = "123123123",
                ReservationRoomList = new List<contractsReservations.ReservationRoomAdditionalData>()
                {
                    new contractsReservations.ReservationRoomAdditionalData()
                    {
                        ReservationRoom_UID = 2,
                        ChannelReservationRoomId = "created"
                    }
                },
            };
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPCC, additionalData.AgencyPCC);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyAddress, additionalData.AgencyAddress);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyName, additionalData.AgencyName);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPhone, additionalData.AgencyPhone);
            for (int i = 0; i < resBuilder.InputData.reservationsAdditionalData.ReservationRoomList.Count; ++i)
            {
                Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.ReservationRoomList[i].ReservationRoom_UID, additionalData.ReservationRoomList[i].ReservationRoom_UID);
                Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.ReservationRoomList[i].ChannelReservationRoomId, additionalData.ReservationRoomList[i].ChannelReservationRoomId);
            }
        }
        [TestMethod]
        [TestCategory("InsertReservation_BigPullAuth")]
        public void TestInsertReservation_BigPullAuth_ReservationAdditionalData()
        {
            long errorCode = 0;

            _reservationHelperMock.Setup(x => x.GetCancelationCosts(It.IsAny<contractsReservations.Reservation>(), It.IsAny<bool>(), It.IsAny<DateTime?>()))
                .Returns(new List<contractsReservations.ReservationRoomCancelationCost> { });

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                AgencyPCC = "1234",
                AgencyAddress = "AAAAA",
                AgencyName = "BBBBBB",
                AgencyPhone = "123123123",
                BigPullAuthRequestor_UID = 10
            };
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.BigPullAuthRequestor_UID, additionalData.BigPullAuthRequestor_UID);

        }

        [TestMethod]
        [TestCategory("InsertReservation_BE")]
        public void TestInsertReservation_BE_ReservationAdditionalData_With_Parameters_BookingEngineTemplate_And_ReservationDomain()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                AgencyPCC = "1234",
                AgencyAddress = "AAAAA",
                AgencyName = "BBBBBB",
                AgencyPhone = "123123123",
                BookingEngineTemplate = "TemplateTest",
                ReservationDomain = "DomainTest",
            };
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPCC, additionalData.AgencyPCC);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyAddress, additionalData.AgencyAddress);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyName, additionalData.AgencyName);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPhone, additionalData.AgencyPhone);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.BookingEngineTemplate, additionalData.BookingEngineTemplate);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.ReservationDomain, additionalData.ReservationDomain);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BE")]
        public void TestInsertReservation_BE_ReservationAdditionalData_With_Parameters_BookingEngineTemplate_And_ReservationDomain_Empty()
        {
            long errorCode = 0;

            _reservationHelperMock.Setup(x => x.GetCancelationCosts(It.IsAny<contractsReservations.Reservation>(), It.IsAny<bool>(), It.IsAny<DateTime?>()))
                .Returns(new List<contractsReservations.ReservationRoomCancelationCost> { });

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                AgencyPCC = "1234",
                AgencyAddress = "AAAAA",
                AgencyName = "BBBBBB",
                AgencyPhone = "123123123",
                BookingEngineTemplate = "",
                ReservationDomain = "",
            };
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);
            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.BookingEngineTemplate, null);
            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.ReservationDomain, null);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPCC, additionalData.AgencyPCC);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyAddress, additionalData.AgencyAddress);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyName, additionalData.AgencyName);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPhone, additionalData.AgencyPhone);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.BookingEngineTemplate, additionalData.BookingEngineTemplate);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.ReservationDomain, additionalData.ReservationDomain);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BE")]
        public void TestInsertReservation_BE_ReservationAdditionalData_With_Parameters_BookingEngineTemplate_And_ReservationDomain_And_IsDirectReservation_True()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                AgencyPCC = "1234",
                AgencyAddress = "AAAAA",
                AgencyName = "BBBBBB",
                AgencyPhone = "123123123",
                BookingEngineTemplate = "TemplateTest",
                ReservationDomain = "DomainTest",
                IsDirectReservation = true
            };
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPCC, additionalData.AgencyPCC);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyAddress, additionalData.AgencyAddress);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyName, additionalData.AgencyName);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPhone, additionalData.AgencyPhone);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.BookingEngineTemplate, additionalData.BookingEngineTemplate);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.ReservationDomain, additionalData.ReservationDomain);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.IsDirectReservation, additionalData.IsDirectReservation);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BE")]
        public void TestInsertReservation_BE_ReservationAdditionalData_With_Parameters_BookingEngineTemplate_And_ReservationDomain_And_IsDirectReservation_False()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                AgencyPCC = "1234",
                AgencyAddress = "AAAAA",
                AgencyName = "BBBBBB",
                AgencyPhone = "123123123",
                BookingEngineTemplate = "TemplateTest",
                ReservationDomain = "DomainTest",
                IsDirectReservation = false
            };
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPCC, additionalData.AgencyPCC);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyAddress, additionalData.AgencyAddress);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyName, additionalData.AgencyName);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPhone, additionalData.AgencyPhone);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.BookingEngineTemplate, additionalData.BookingEngineTemplate);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.ReservationDomain, additionalData.ReservationDomain);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.IsDirectReservation, additionalData.IsDirectReservation);
        }

        [TestMethod]
        [TestCategory("InsertReservation_BE")]
        public void TestInsertReservation_BE_ReservationAdditionalData_With_Parameters_BookingEngineTemplate_And_ReservationDomain_And_IsDirectReservation_Null()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(3232, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            resBuilder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                AgencyPCC = "1234",
                AgencyAddress = "AAAAA",
                AgencyName = "BBBBBB",
                AgencyPhone = "123123123",
                BookingEngineTemplate = "TemplateTest",
                ReservationDomain = "DomainTest"
            };

            resBuilder.ExpectedData.reservationsAdditionalData = new ReservationsAdditionalData
            {
                isDirectReservation = false
            };

            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var reservationFromDb = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, reservationFromDb);

            Assert.AreEqual(reservationFromDb.reservationsAdditionalData.Reservation_UID, reservation.UID);

            contractsReservations.ReservationsAdditionalData additionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(reservationFromDb.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPCC, additionalData.AgencyPCC);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyAddress, additionalData.AgencyAddress);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyName, additionalData.AgencyName);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.AgencyPhone, additionalData.AgencyPhone);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.BookingEngineTemplate, additionalData.BookingEngineTemplate);
            Assert.AreEqual(resBuilder.InputData.reservationsAdditionalData.ReservationDomain, additionalData.ReservationDomain);
            Assert.AreEqual(resBuilder.ExpectedData.reservationsAdditionalData.isDirectReservation, false);
        }


        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_SalesmanComission()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi, salesman and salesman commission
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = tpisList.SingleOrDefault(x => x.UID == 61);
            var tmpTpi = tpi.Clone();
            tmpTpi.Property_UID = 1263;
            tpisList.Remove(tpi); tpisList.Add(tmpTpi);

            Domain.CRM.Salesman salesman = salesManList.First(x => x.UID == 4);
            var salesmanTmp = salesman.Clone();
            salesmanTmp.Property_UID = 1263;
            salesManList.Remove(salesman); salesManList.Add(salesmanTmp);

            SalesmanThirdPartyIntermediariesComission comission = salesmanTPICommissionsList.First(x => x.UID == 2);
            var comissionTmp = comission.Clone();
            comissionTmp.ThirdPartyIntermediariesUID = tpi.UID;
            comissionTmp.SalesmanUID = salesman.UID;
            comissionTmp.SalesmanBaseCommission = 10;
            comissionTmp.SalesmanIsBaseCommissionPercentage = false;
            salesmanTPICommissionsList.Remove(comission); salesmanTPICommissionsList.Add(comissionTmp);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithTPI(tpi).WithSalesmanComission(salesman, comission).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Push,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_Simple()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            Assert.IsNotNull(reservation);
            Assert.AreEqual(resNumberExpected, reservation.Number);
            Assert.AreEqual(resBuilder.ExpectedData.reservationDetail.Channel_UID, reservation.Channel_UID);

        }

        //TODO: Changing
        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_PropertyInventory_RateNull()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "XPTO123456789";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true, rateId: null).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var roomTypeId = resBuilder.InputData.reservationRooms[0].RoomType_UID.Value;

            //Mock inventory update sql
            MockSqlManager<Inventory>(2, new List<Inventory> {
                new Inventory
                {
                    QtyRoomOccupied = 1,
                    UID = 1,
                    RoomType_UID = roomTypeId
                }
            });

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected,
                availabilityType: (int)Contracts.Data.PropertyAvailabilityTypes.Inventory);

            //Verify if we call the inventory update
            _sqlManagerMock.Verify(x => x.ExecuteSql<Inventory>(It.Is<string>(y => y.Contains("UPDATE") && y.Contains("Inventory")
                    && y.Contains("I.RoomType_UID = " + roomTypeId)), null), Times.Once);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            Assert.IsNotNull(reservation);
            Assert.AreEqual(resNumberExpected, reservation.Number);
            Assert.AreEqual(resBuilder.ExpectedData.reservationDetail.Channel_UID, reservation.Channel_UID);

        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_SABRE_WithoutAllotmentValidation()
        {
            long errorCode = 0;

            // arrange
            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(59, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            #region Mock Repos
            MockObRateToomDetailsForResRepo(-1);  //Doesn't metter for now the parammeter passed to the method, so we send -1 (it can be changen if we need it)
            #endregion

            //Change the allotment
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                RateBuilder.ChangeRateRoomDetailAllotment(5137349, 0, unitOfWork, this.RepositoryFactory);
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 59, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
            _resAddDataRepoMock.Verify(x => x.Add(It.IsAny<ReservationsAdditionalData>()), Times.Once);
            _ReservationHistoryRepoMock.Verify(x => x.Add(It.IsAny<ReservationsHistory>()), Times.Once);
            _eventSystemManagerPocoMock.Verify(x => x.SendMessage(It.IsAny<OB.Events.Contracts.ICustomNotification>()), Times.Once);

            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var rateroomdetails = RateBuilder.GetRateRoomDetail(resBuilder.ExpectedData.reservationRoomDetails.First().RateRoomDetails_UID.Value, this.RepositoryFactory);

                // TODO after release as sqlManager is being used to make queries on POCO files and can't mock
                //Assert.AreEqual(-1, rateroomdetails.Allotment - rateroomdetails.AllotmentUsed);
            }
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_NoPaymentDetails()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_Simple()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void TestInsertReservation_PullChannel_HoteisNetOperator_Without_CommissionType()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, groupRule: groupRule, channelOperatorType: 2);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_HoteisNetOperator_With_CommissionType()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            resBuilder.InputData.reservationRooms.ForEach(x =>
            {
                x.CommissionType = 1;
            });

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, groupRule: groupRule, channelOperatorType: 2);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [Ignore("Ignored due to ValidateReservationV2 SP cannot being called. Fix me when that issue is done.")]
        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_IsValidated()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber | BusinessRules.ValidateRestrictions
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
            Assert.IsTrue(ValidateReservationV2Called, "Expected ValidateReservationV2Called SP to be called");
        }

        [TestMethod]
        [ExpectedException(typeof(ReservationAlreadyExistsException))]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_ExistingReservation()
        {
            long errorCode = 0;

            // arrange
            //Get and prepare the reservation
            ReservationDataBuilder resBuilder = null;
            SearchBuilder builder = null;
            string resNumberExpected = null;
            var transactionId = Guid.NewGuid().ToString();
            GroupRule groupRule = null;
            ReservationData reservationData = null;

            #region MockRepo
            MockReservationRepoFirstOrDefault();
            #endregion

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                reservationData = this.GetReservationDataFromDatabase("Orbitz-CJ1SVI", detached: true, unitOfWork: unitOfWork);
                reservationData.guest.UID = 0;

                resBuilder = new ReservationDataBuilder(null).WithExistingReservation(reservationData);
                resNumberExpected = reservationData.reservationDetail.Number;
                builder = new Operations.Helper.SearchBuilder(Container, reservationData.reservationDetail.Property_UID);
                groupRule = new GroupRule()
                {
                    RuleType = RuleType.Pull,
                    BusinessRules = BusinessRules.GenerateReservationNumber
                };
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: reservationData.reservationDetail.Channel_UID.GetValueOrDefault(), resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            var afterReservationData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, afterReservationData);
        }

        [TestMethod]
        [ExpectedException(typeof(ReservationAlreadyExistsException))]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_ExistingReservation()
        {
            long errorCode = 0;

            ReservationData reservationData = null;
            ReservationDataBuilder resBuilder = null;
            SearchBuilder builder = null;
            string resNumberExpected = null;
            var transactionId = Guid.NewGuid().ToString();
            GroupRule groupRule = null;

            #region MockRepo
            MockReservationRepoFirstOrDefault();
            _reservationPaymentDetailGenericRepoMock.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.ReservationPaymentDetail, bool>>>()))
                .Returns(reservationsList.Where(y => y.UID == 40000).SelectMany(y => y.ReservationPaymentDetails).AsQueryable());
            #endregion

            // arrange
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                reservationData = this.GetReservationDataFromDatabase("Orbitz-CJ1SVI", detached: true, unitOfWork: unitOfWork);
                reservationData.guest.UID = 0;
                reservationData.reservationPaymentDetail.CVV = EncryptCreditCards( //we are here - null exception
                            new ListCreditCardRequest
                            {
                                CreditCards = new List<string> { "111" }
                            })
                            .CreditCards.FirstOrDefault();

                resBuilder = new ReservationDataBuilder(null).WithExistingReservation(reservationData);
                resNumberExpected = reservationData.reservationDetail.Number;
                builder = new Operations.Helper.SearchBuilder(Container, reservationData.reservationDetail.Property_UID);
                groupRule = new GroupRule()
                {
                    RuleType = RuleType.Pull,
                    BusinessRules = BusinessRules.GenerateReservationNumber
                };
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: reservationData.reservationDetail.Channel_UID.GetValueOrDefault(), resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                var afterReservationData = GetReservationDataFromDatabase(reservation.UID);
                ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, afterReservationData);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ReservationAlreadyExistsException))]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_ExistingReservationWithChilds()
        {
            long errorCode = 0;

            ReservationDataBuilder resBuilder = null;
            SearchBuilder builder = null;
            string resNumberExpected = null;
            var transactionId = Guid.NewGuid().ToString();
            GroupRule groupRule = null;
            ReservationData reservationData = null;

            #region MockRepo
            MockReservationRepoFirstOrDefault();
            _reservationPaymentDetailGenericRepoMock.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.ReservationPaymentDetail, bool>>>()))
                .Returns(reservationsList.Where(y => y.UID == 40000).SelectMany(y => y.ReservationPaymentDetails).AsQueryable());
            #endregion

            // arrange
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                reservationData = this.GetReservationDataFromDatabase("Orbitz-CJ1SVI", detached: true, unitOfWork: unitOfWork);
                reservationData.guest.UID = 0;

                // insert 2 childs
                var child1 = new ReservationRoomChild
                {
                    Age = 2,
                    ChildNo = 1,
                    ReservationRoom_UID = reservationData.reservationRooms[0].UID,
                };
                var child2 = new ReservationRoomChild
                {
                    Age = 7,
                    ChildNo = 2,
                    ReservationRoom_UID = reservationData.reservationRooms[0].UID,
                };


                var reservationRoomChildRepo = this.RepositoryFactory.GetRepository<ReservationRoomChild>(unitOfWork);
                reservationRoomChildRepo.Add(child1);
                reservationRoomChildRepo.Add(child2);

                unitOfWork.Save();

                // insert 2 childs
                resBuilder = new ReservationDataBuilder(null).WithExistingReservation(reservationData).WithChildren((int)reservationData.reservationRooms[0].UID, 2, new List<int> { 5, 10 });
                resNumberExpected = reservationData.reservationDetail.Number;
                builder = new Operations.Helper.SearchBuilder(Container, reservationData.reservationDetail.Property_UID);
                groupRule = new GroupRule()
                {
                    RuleType = RuleType.Pull,
                    BusinessRules = BusinessRules.GenerateReservationNumber
                };
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: true, withCancellationPolicies: false, channelUid: reservationData.reservationDetail.Channel_UID.GetValueOrDefault(), resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                var afterReservationData = GetReservationDataFromDatabase(reservation.UID, unitOfWork: unitOfWork);
                // assert
                ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, afterReservationData);
            }
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_NoPaymentDetails()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_ChildsWithAges()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var ages = new List<int>() { 11, 3 };
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1).WithCreditCardPayment().WithChildren(1, 2, ages);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: true, ages: ages, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_ChildsWithNullAges()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            List<int> ages = null;
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1).WithCreditCardPayment().WithChildren(1, 2, null);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, hasAges: false, ages: ages, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_TwoRooms()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1).WithRoom(2, 2).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Push,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_TwoRoomsDifferentDates()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32).WithNewGuest()
                .WithRoom(1, 1, new DateTime(2013, 01, 02), new DateTime(2013, 01, 10))
                .WithRoom(2, 3, new DateTime(2013, 01, 14), new DateTime(2013, 01, 19))
                .WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule,
                numberOfRooms: 2);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
        }

        [TestMethod]
        [TestCategory("CancelReservation_Channels")]
        public void TestCancelReservation_ChannelCancellationRoomWithReasonTypeAndComment()
        {
            #region ARRANGE
            long propertyUID = 1635;
            long channel_UID = 32; //Booking

            domainReservation.Reservation res = reservationsList.FirstOrDefault(x => x.Channel_UID == channel_UID && x.NumberOfRooms == 2 && x.Property_UID == propertyUID);
            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
                rr.ReservationRoomExtras.Add(new ReservationRoomExtra
                {
                    Extra_UID = 1833,
                    Qty = 1,
                    Total_Price = rr.ReservationRoomsExtrasSum.Value,
                    Total_VAT = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }
            #endregion

            #region Mock Repo
            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, 66242);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);
            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = res.Number,
                ReservationRoomNo = res.ReservationRooms.First().ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = 3,
                CancelReservationComments = "Invalid Credit Card",
                RuleType = OB.Reservation.BL.Constants.RuleType.Push
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(res.UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, databaseData.reservationDetail.Status);
            Assert.AreEqual((decimal)15, databaseData.reservationDetail.RoomsExtras);
            Assert.AreEqual((decimal)341, databaseData.reservationDetail.RoomsPriceSum);
            Assert.AreEqual((decimal)356, databaseData.reservationDetail.RoomsTotalAmount);
            Assert.AreEqual((decimal)356, databaseData.reservationDetail.TotalAmount);
            Assert.AreEqual(3, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual("Invalid Credit Card", databaseData.reservationDetail.CancelReservationComments);
            Assert.AreEqual(32, databaseData.reservationDetail.Channel_UID);

            Assert.AreEqual(2, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);
            Assert.AreEqual((decimal)45, databaseData.reservationRooms[0].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)660, databaseData.reservationRooms[0].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)705, databaseData.reservationRooms[0].ReservationRoomsTotalAmount);

            Assert.AreEqual(1, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
            Assert.AreEqual((decimal)15, databaseData.reservationRooms[1].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)341, databaseData.reservationRooms[1].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)356, databaseData.reservationRooms[1].ReservationRoomsTotalAmount);
        }

        [TestMethod]
        [TestCategory("CancelReservation_Channels")]
        public void TestCancelReservation_ChannelCancelReservation()
        {
            #region ARRANGE
            long propertyUID = 1635;
            long channel_UID = 32; //Booking
            domainReservation.Reservation res = reservationsList.FirstOrDefault(x => x.Channel_UID == channel_UID && x.NumberOfRooms == 2 && x.Property_UID == propertyUID);

            foreach (ReservationRoom resroom in res.ReservationRooms)
            {
                resroom.DateFrom = DateTime.Now.Date;
                resroom.IsCancellationAllowed = true;
                resroom.CancellationValue = null;
                resroom.CancellationPolicyDays = 0;
                resroom.CancellationCosts = true;
                resroom.CancellationNrNights = null;
            }
            #endregion

            #region Mock Repo
            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, 66242);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);
            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = res.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty,
                RuleType = Reservation.BL.Constants.RuleType.Push
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(res.UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, databaseData.reservationDetail.Status);
            Assert.AreEqual((decimal)60, databaseData.reservationDetail.RoomsExtras);
            Assert.AreEqual((decimal)1001, databaseData.reservationDetail.RoomsPriceSum);
            Assert.AreEqual((decimal)1061, databaseData.reservationDetail.RoomsTotalAmount);
            Assert.AreEqual((decimal)1061, databaseData.reservationDetail.TotalAmount);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, databaseData.reservationDetail.CancelReservationComments);
            Assert.AreEqual(32, databaseData.reservationDetail.Channel_UID);

            Assert.AreEqual(2, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);
            Assert.AreEqual((decimal)45, databaseData.reservationRooms[0].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)660, databaseData.reservationRooms[0].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)705, databaseData.reservationRooms[0].ReservationRoomsTotalAmount);

            Assert.AreEqual(2, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
            Assert.AreEqual((decimal)15, databaseData.reservationRooms[1].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)341, databaseData.reservationRooms[1].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)356, databaseData.reservationRooms[1].ReservationRoomsTotalAmount);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_Channels_SaveAdditionalData_SaveHistory_SendLog()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            Assert.IsNotNull(reservation);
            Assert.AreEqual(resNumberExpected, reservation.Number);
            Assert.AreEqual(resBuilder.ExpectedData.reservationDetail.Channel_UID, reservation.Channel_UID);
            _resAddDataRepoMock.Verify(x => x.Add(It.IsAny<ReservationsAdditionalData>()), Times.Once);
            _ReservationHistoryRepoMock.Verify(x => x.Add(It.IsAny<ReservationsHistory>()), Times.Once);
            _eventSystemManagerPocoMock.Verify(x => x.SendMessage(It.IsAny<OB.Events.Contracts.ICustomNotification>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Channels")]
        public void TestInsertReservation_PullChannel_SaveAdditionalData_SaveHistory_SendLog()
        {
            long errorCode = 0;

            // arrange
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(32, 1, resNumber: resNumberExpected).WithNewGuest()
                .WithRoom(1, 1, cancelationAllowed: true).WithCreditCardPayment();
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 32, resNumber: resNumberExpected, groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, GetReservationDataFromDatabase(reservation.UID));
            _resAddDataRepoMock.Verify(x => x.Add(It.IsAny<ReservationsAdditionalData>()), Times.Once);
            _ReservationHistoryRepoMock.Verify(x => x.Add(It.IsAny<ReservationsHistory>()), Times.Once);
            _eventSystemManagerPocoMock.Verify(x => x.SendMessage(It.IsAny<OB.Events.Contracts.ICustomNotification>()), Times.Once);
        }

        #endregion

        #region OPERATORS
        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void BilledWithPaymentTypeNotDefinied()
        {
            long errorCode = 0;

            // arrange
            //Prepara the ChannelProperty
            BL.Contracts.Data.Channels.ChannelsProperty channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.OperatorBillingType = 3;
            channel.OperatorCreditLimit = 300;
            channel.OperatorCreditUsed = 120;

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true, channelOperatorType: 2);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
        }
        [Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        public void BilledWithActiveCreditLimitAndValidPaymentTypeAndApprovedDaily()
        {
            long errorCode = 0;

            // arrange
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.IsOperatorsCreditLimit = true;
            channel.OperatorBillingType = 1;
            channel.OperatorCreditLimit = 120;
            channel.OperatorCreditUsed = null;
            channel.PrePaymentCreditUsed = 230;

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            // Add RateChannel
            var rateChannel = new RatesChannel
            {
                UID = 1,
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };
            ratesChannelList.Add(rateChannel);
            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 4,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            channel = chPropsList.FirstOrDefault(x => x.UID == channel.UID);

            Assert.AreEqual(110, channel.OperatorCreditUsed);
            Assert.AreEqual(230, channel.PrePaymentCreditUsed);
        }

        [Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        public void BilledWithActiveCreditLimitAndValidPaymentTypeAndApprovedDailyExtras()
        {
            long errorCode = 0;

            // arrange
            IUnitOfWork unitOfWork = null;
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.OperatorBillingType = 2;
            channel.OperatorCreditLimit = 300;
            channel.OperatorCreditUsed = 120;
            channel.IsOperatorsCreditLimit = true;

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            // Add RateChannel
            var rateChannel = new RatesChannel
            {
                UID = 1,
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };
            ratesChannelList.Add(rateChannel);

            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 4,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert       
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID, unitOfWork: unitOfWork);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            var channelUid = channel.Channel_UID;
            channel = chPropsList.Where(x => x.UID == channel.UID).FirstOrDefault();

            Assert.AreEqual(230, channel.OperatorCreditUsed);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void BilledWithActiveCreditLimitAndValidPaymentTypeUnaproved()
        {
            long errorCode = 0;

            // arrange
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);

            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.OperatorBillingType = 3;
            channel.OperatorCreditLimit = 300;
            channel.OperatorCreditUsed = 120;


            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            // Add RateChannel
            var rateChannel = new RatesChannel
            {
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };

            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 4,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule, channelOperatorType: 2);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
        }

        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        public void BilledWithInactiveCreditLimitAndValidPaymentTypeAndApproved()
        {
            long errorCode = 0;

            // arrange
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.IsOperatorsCreditLimit = false;
            channel.OperatorBillingType = 1;
            channel.OperatorCreditLimit = 120;
            channel.OperatorCreditUsed = null;

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            resBuilder.InputData.reservationRooms.ForEach(x =>
            {
                x.CommissionType = 1;
            });

            // Add RateChannel
            var rateChannel = new RatesChannel
            {
                UID = 1,
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };
            ratesChannelList.Add(rateChannel);
            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 4,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule, hndCredit: false);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            var channelUid = channel.Channel_UID;
            channel = chPropsList.Where(x => x.UID == channel.UID).FirstOrDefault();

            Assert.AreEqual(null, channel.OperatorCreditUsed);
        }
        [Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        [DeploymentItem("./DL")]
        public void BilledWithActiveCreditLimitAndValidPaymentTypeAndApprovedButExceededLimit()
        {
            long errorCode = 0;

            // arrange
            IUnitOfWork unitOfWork = null;
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.IsOperatorsCreditLimit = true;
            channel.OperatorBillingType = 1;
            channel.OperatorCreditLimit = 100;
            channel.OperatorCreditUsed = null;

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            resBuilder.ExpectedData.reservationDetail.InternalNotes = System.Environment.NewLine
                        + string.Format(Resources.Resources.lblCreditLimitReached, "4 Cantos", ((decimal)channel.OperatorCreditLimit).ToString("0"));
            resBuilder.ExpectedData.reservationDetail.InternalNotesHistory = string.Format(Resources.Resources.lblCreditLimitReached,
                                "4 Cantos", channel.OperatorCreditLimit);

            // Add RateChannel
            var rateChannel = new RatesChannel
            {
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };
            ratesChannelList.Add(rateChannel);

            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 4,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Mock propertyQueryResult
            var propertyEventQueryResultList = new List<PropertyEventQR1>() {
                new PropertyEventQR1()
                {
                    UID = 4566,
                    ReservationOption = null
                }
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule, propertyEventQueryResultList: propertyEventQueryResultList);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            using (unitOfWork = SessionFactory.GetUnitOfWork())
            {
                channel = chPropsList.Where(x => x.UID == channel.UID).FirstOrDefault();

                Assert.AreEqual(110, channel.OperatorCreditUsed);
                Assert.IsTrue(databaseData.reservationDetail.InternalNotesHistory.Contains(resBuilder.ExpectedData.reservationDetail.InternalNotesHistory));


                //var propertyQueueRepo = RepositoryFactory.GetRepository<PropertyQueue>(unitOfWork);
                //var proactive = propertyQueueRepo.FirstOrDefault(x => x.TaskType_UID == 80 && x.Property_UID == 1263 && x.MailBody.Contains("operator"));

                //Assert.IsNotNull(proactive);
            }
        }
        [Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        public void PrePaymentWithActiveCreditLimitAndValidPaymentTypeUnaproved()
        {
            long errorCode = 0;

            // arrange
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.OperatorBillingType = 3;
            channel.OperatorCreditLimit = 300;
            channel.OperatorCreditUsed = 120;

            channel.IsActivePrePaymentCredit = true;
            channel.PrePaymentCreditLimit = 300;
            channel.PrePaymentCreditUsed = 120;

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(7);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            // Add RateChannel
            var rateChannel = new RatesChannel
            {
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };
            ratesChannelList.Add(rateChannel);
            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 7,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert       
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            channel = chPropsList.Where(x => x.UID == channel.UID).FirstOrDefault();

            Assert.AreEqual(300, channel.OperatorCreditLimit);
            Assert.AreEqual(120, channel.OperatorCreditUsed);
            Assert.AreEqual(230, channel.PrePaymentCreditUsed);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        public void PrePaymentWithInactiveCreditLimitAndValidPaymentType()
        {
            long errorCode = 0;

            // arrange
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.IsActivePrePaymentCredit = false;
            channel.OperatorBillingType = 3;
            channel.OperatorCreditLimit = 300;
            channel.OperatorCreditUsed = 120;
            channel.PrePaymentCreditUsed = null;

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(7);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            // Add RateChannel
            var rateChannel = new RatesChannel
            {
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };
            ratesChannelList.Add(rateChannel);
            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 7,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule, hndCredit: false);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert       
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            channel = chPropsList.Where(x => x.UID == channel.UID).FirstOrDefault();

            Assert.AreEqual(300, channel.OperatorCreditLimit);
            Assert.AreEqual(120, channel.OperatorCreditUsed);
            Assert.AreEqual(null, channel.PrePaymentCreditUsed);
        }

        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        [ExpectedException(typeof(BusinessLayerException))]
        public void PrePaymentWithPaymentTypeNotDefinied()
        {
            long errorCode = 0;

            // arrange
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.OperatorBillingType = 3;
            channel.OperatorCreditLimit = 300;
            channel.OperatorCreditUsed = 120;

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule, channelOperatorType: 2);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
        }

        [Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        public void TestInsertReservation_Operator_Credit()
        {
            long errorCode = 0;

            // arrange
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.OperatorBillingType = 4;
            channel.OperatorCreditLimit = 120;
            channel.OperatorCreditUsed = null;
            channel.IsOperatorsCreditLimit = true;

            var rateChannel = new RatesChannel
            {
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };
            ratesChannelList.Add(rateChannel);
            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 4,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            channel = chPropsList.FirstOrDefault(x => x.UID == channel.UID);
            Assert.AreEqual(110, channel.OperatorCreditUsed);
        }

        [Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("InsertReservation_Operators")]
        public void TestInsertReservation_Operator_CreditWithDifferentPaymentType()
        {
            long errorCode = 0;

            // arrange
            ChannelsProperty channel = null;

            channel = chPropsList.FirstOrDefault(cp => cp.Property_UID == 1635);
            channel.Channel_UID = 80;
            channel.Property_UID = 1263;
            channel.IsActive = true;
            channel.IsDeleted = false;
            channel.IsOperatorsCreditLimit = true;
            channel.OperatorBillingType = 5;
            channel.OperatorCreditLimit = 300;
            channel.OperatorCreditUsed = 120;

            // Add RateChannel
            var rateChannel = new RatesChannel
            {
                Channel_UID = 80,
                Rate_UID = 4639,
                IsDeleted = false
            };
            ratesChannelList.Add(rateChannel);
            ratesChannelsPaymentMethodsList.Add(new RatesChannelsPaymentMethod
            {
                PaymentMethod_UID = 4,
                RateChannel_UID = rateChannel.UID
            });

            var groupRule = new GroupRule()
            {
                BusinessRules = BusinessRules.GenerateReservationNumber
            };

            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(80, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true).WithBilledTypePayment(4);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 80, resNumber: resNumberExpected, validate: true,
                groupRule: groupRule);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;

            // assert                        
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            channel = chPropsList.FirstOrDefault(x => x.UID == channel.UID);
            Assert.AreEqual(230, channel.OperatorCreditUsed);
        }


        //TODO: ES: I was finishing this test
        //We need to insert the reservation in the list and the associated objets to make the test work
        [TestMethod]
        [TestCategory("CancelReservation_Operators")]
        public void TestCancelReservation_CreditOperator()
        {
            #region ARRANGE
            long propertyUID = 1635;
            long channel_UID = 80; //4 Cantos HoteisNet Pull

            //Get and change the reservation
            domainReservation.Reservation res = reservationsList.FirstOrDefault(x => x.Channel_UID == channel_UID && x.NumberOfRooms == 2 && x.Property_UID == propertyUID);
            res.IsPaid = false;
            res.Status = 1;
            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.Status = 1;
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
                rr.ReservationRoomExtras.Add(new ReservationRoomExtra
                {
                    Extra_UID = 1833,
                    Qty = 1,
                    Total_Price = rr.ReservationRoomsExtrasSum.Value,
                    Total_VAT = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }

            //Get and change the ChannelProperty
            var channelsProperties = chPropsList.FirstOrDefault(x => x.Property_UID == propertyUID && x.Channel_UID == channel_UID);
            channelsProperties.OperatorBillingType = 0;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = true;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 3000;
            channelsProperties.IsActivePrePaymentCredit = false;
            channelsProperties.PrePaymentCreditLimit = 500;
            channelsProperties.PrePaymentCreditUsed = 300;
            #endregion

            #region Mock Repo
            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, 66242);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);
            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = res.Number,
                ReservationRoomNo = res.ReservationRooms.First().ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(res.UID);

            var cp = chPropsList.FirstOrDefault(x => x.Property_UID == propertyUID && x.Channel_UID == channel_UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, databaseData.reservationDetail.Status);
            Assert.AreEqual((decimal)72, databaseData.reservationDetail.RoomsExtras);
            Assert.AreEqual((decimal)2091, databaseData.reservationDetail.RoomsPriceSum);
            Assert.AreEqual((decimal)2163, databaseData.reservationDetail.RoomsTotalAmount);
            Assert.AreEqual((decimal)2163, databaseData.reservationDetail.TotalAmount);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, databaseData.reservationDetail.CancelReservationComments);
            Assert.AreEqual(80, databaseData.reservationDetail.Channel_UID);

            Assert.AreEqual(2, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);
            Assert.AreEqual((decimal)45, databaseData.reservationRooms[0].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)660, databaseData.reservationRooms[0].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)705, databaseData.reservationRooms[0].ReservationRoomsTotalAmount);

            Assert.AreEqual(1, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
            Assert.AreEqual((decimal)15, databaseData.reservationRooms[1].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)341, databaseData.reservationRooms[1].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)356, databaseData.reservationRooms[1].ReservationRoomsTotalAmount);

            Assert.AreEqual(true, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)3000, cp.OperatorCreditUsed);
            Assert.AreEqual(false, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)500, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)300, cp.PrePaymentCreditUsed);
        }

        [TestMethod]
        [TestCategory("CancelReservation_Operators")]
        public void TestCancelReservation_PrePaymentCreditOperator()
        {
            #region ARRANGE
            long propertyUID = 1635;
            long channel_UID = 80; //4 Cantos HoteisNet Pull
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var res = reservationsList.First(x => x.Channel_UID == channel_UID && x.NumberOfRooms == 2 && x.Property_UID == propertyUID);

            res.IsPaid = false;
            res.Status = 1;
            res.PaymentMethodType_UID = 7;

            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.Status = 1;
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
                rr.ReservationRoomExtras.Add(new ReservationRoomExtra
                {
                    Extra_UID = 1833,
                    Qty = 1,
                    Total_Price = rr.ReservationRoomsExtrasSum.Value,
                    Total_VAT = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }

            var channelsProperties = chPropsList.First(x => x.Property_UID == propertyUID && x.Channel_UID == channel_UID);

            channelsProperties.OperatorBillingType = 0;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = false;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 3000;
            channelsProperties.IsActivePrePaymentCredit = true;
            channelsProperties.PrePaymentCreditLimit = 1200;
            channelsProperties.PrePaymentCreditUsed = 3500;

            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = res.Number,
                ReservationRoomNo = res.ReservationRooms.First().ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(res.UID);

            var cp = channelPropsList.FirstOrDefault(x => x.Property_UID == propertyUID && x.Channel_UID == channel_UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, databaseData.reservationDetail.Status);
            Assert.AreEqual((decimal)72, databaseData.reservationDetail.RoomsExtras);
            Assert.AreEqual((decimal)2091, databaseData.reservationDetail.RoomsPriceSum);
            Assert.AreEqual((decimal)2163, databaseData.reservationDetail.RoomsTotalAmount);
            Assert.AreEqual((decimal)2163, databaseData.reservationDetail.TotalAmount);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, databaseData.reservationDetail.CancelReservationComments);
            Assert.AreEqual(80, databaseData.reservationDetail.Channel_UID);

            Assert.AreEqual(2, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);
            Assert.AreEqual((decimal)45, databaseData.reservationRooms[0].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)660, databaseData.reservationRooms[0].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)705, databaseData.reservationRooms[0].ReservationRoomsTotalAmount);

            Assert.AreEqual(1, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
            Assert.AreEqual((decimal)15, databaseData.reservationRooms[1].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)341, databaseData.reservationRooms[1].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)356, databaseData.reservationRooms[1].ReservationRoomsTotalAmount);

            Assert.AreEqual(false, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)3000, cp.OperatorCreditUsed);
            Assert.AreEqual(true, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)1200, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)2366, cp.PrePaymentCreditUsed);
        }
        #endregion

        #region PMS

        [TestMethod]
        [TestCategory("InsertReservation_PMS")]
        public void ValidateUpdateReservationAllotmentAndInventory_PMSInsertReservation_IgnoreAvailabilityInInventory_False_NormalBehaviour()
        {
            bool ignoreAvailability = false;
            bool isUpdateReservation = false;
            bool validate = true;
            bool update = true;
            string correlationId = "correlationId";
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.PMS,
                BusinessRules = BusinessRules.IgnoreAvailability
            };
            var parametersPerDay = new List<Internal.BusinessObjects.UpdateAllotmentAndInventoryDayParameters>
            {
                new Internal.BusinessObjects.UpdateAllotmentAndInventoryDayParameters
                {
                    AddQty = 1,
                    Day = DateTime.Today,
                    RateAvailabilityType = 3, //Inventory
                    RateRoomDetailUID = 1,
                    RoomTypeUID = 1
                }
            };
            var inventories = new List<Inventory>
            {
                new Inventory
                {
                    QtyRoomOccupied = 1,
                    Date = DateTime.Today,
                    QtyContractedAllotmentOccupied = 1,
                    QtyContractedAllotmentTotal = 1,
                    RoomQuantity = 1
                }
            };

            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>()))
                           .Returns(inventories);

            var result = ReservationManagerPOCO.ValidateUpdateReservationAllotmentAndInventory(groupRule, parametersPerDay, validate, update, correlationId, isUpdateReservation, ignoreAvailability);

            // assert
            Assert.IsNotNull(result);
            // If the result list is the same as mock, it means that the normal behaviour is expected, as the sql was executed
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Date, DateTime.Today);
            Assert.AreEqual(result[0].QtyRoomOccupied, 1);
            Assert.AreEqual(result[0].QtyContractedAllotmentOccupied, 1);
            Assert.AreEqual(result[0].QtyContractedAllotmentTotal, 1);
            Assert.AreEqual(result[0].RoomQuantity, 1);
        }

        [TestMethod]
        [TestCategory("InsertReservation_PMS")]
        public void ValidateUpdateReservationAllotmentAndInventory_PMSInsertReservation_IgnoreAvailabilityInAllotment_False_NormalBehaviour()
        {
            bool ignoreAvailability = false;
            bool isUpdateReservation = false;
            bool validate = true;
            bool update = true;
            string correlationId = "correlationId";
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.PMS,
                BusinessRules = BusinessRules.IgnoreAvailability
            };
            var parametersPerDay = new List<Internal.BusinessObjects.UpdateAllotmentAndInventoryDayParameters>
            {
                new Internal.BusinessObjects.UpdateAllotmentAndInventoryDayParameters
                {
                    AddQty = 1,
                    Day = DateTime.Today,
                    RateAvailabilityType = 1, //Allotment
                    RateRoomDetailUID = 1,
                    RoomTypeUID = 1
                }
            };
            var inventories = new List<Inventory>
            {
                new Inventory
                {
                    QtyRoomOccupied = 1,
                    Date = DateTime.Today,
                    QtyContractedAllotmentOccupied = 1,
                    QtyContractedAllotmentTotal = 1,
                    RoomQuantity = 1
                }
            };

            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>()))
                           .Returns(inventories);

            var result = ReservationManagerPOCO.ValidateUpdateReservationAllotmentAndInventory(groupRule, parametersPerDay, validate, update, correlationId, isUpdateReservation, ignoreAvailability);

            // assert
            Assert.IsNotNull(result);
            // If the result list is the same as mock, it means that the normal behaviour is expected, as the sql was executed
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Date, DateTime.Today);
            Assert.AreEqual(result[0].QtyRoomOccupied, 1);
            Assert.AreEqual(result[0].QtyContractedAllotmentOccupied, 1);
            Assert.AreEqual(result[0].QtyContractedAllotmentTotal, 1);
            Assert.AreEqual(result[0].RoomQuantity, 1);
        }

        [TestMethod]
        [TestCategory("InsertReservation_PMS")]
        public void ValidateUpdateReservationAllotmentAndInventory_PMSInsertReservation_IgnoreAvailability_True_DoNotUpdateInventory()
        {
            bool ignoreAvailability = true;
            bool isUpdateReservation = false;
            bool validate = true;
            bool update = true;
            string correlationId = "correlationId";
            var groupRule = new GroupRule()
            {
                RuleType = RuleType.PMS,
                BusinessRules = BusinessRules.IgnoreAvailability
            };
            var parametersPerDay = new List<Internal.BusinessObjects.UpdateAllotmentAndInventoryDayParameters>
            {
                new Internal.BusinessObjects.UpdateAllotmentAndInventoryDayParameters
                {
                    AddQty = 1,
                    Day = DateTime.Today,
                    RateAvailabilityType = 1, //Allotment
                    RateRoomDetailUID = 1,
                    RoomTypeUID = 1
                }
            };
            var inventories = new List<Inventory>
            {
                new Inventory
                {
                    QtyRoomOccupied = 1,
                    Date = DateTime.Today,
                    QtyContractedAllotmentOccupied = 1,
                    QtyContractedAllotmentTotal = 1,
                    RoomQuantity = 1
                }
            };

            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>()))
                           .Returns(inventories);

            var result = ReservationManagerPOCO.ValidateUpdateReservationAllotmentAndInventory(groupRule, parametersPerDay, validate, update, correlationId, isUpdateReservation, ignoreAvailability);

            // assert
            Assert.IsNotNull(result);
            // If the result list is 0, it means that the ignore inventory flag worked.
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        [TestCategory("InsertReservation_PMS")]
        public void ValidateUpdateReservationAllotmentAndInventory_PMSInsertReservation_GroupRuleNull()
        {
            bool IgnoreAvailability = true;
            bool isUpdateReservation = false;
            bool validate = true;
            bool update = true;
            string correlationId = "correlationId";
            GroupRule groupRule = null; // To simulate channels reservations
            var parametersPerDay = new List<Internal.BusinessObjects.UpdateAllotmentAndInventoryDayParameters>
            {
                new Internal.BusinessObjects.UpdateAllotmentAndInventoryDayParameters
                {
                    AddQty = 1,
                    Day = DateTime.Today,
                    RateAvailabilityType = 1, //Allotment
                    RateRoomDetailUID = 1,
                    RoomTypeUID = 1
                }
            };
            var inventories = new List<Inventory>
            {
                new Inventory
                {
                    QtyRoomOccupied = 1,
                    Date = DateTime.Today,
                    QtyContractedAllotmentOccupied = 1,
                    QtyContractedAllotmentTotal = 1,
                    RoomQuantity = 1
                }
            };

            _sqlManagerMock.Setup(x => x.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>()))
                           .Returns(inventories);

            var result = ReservationManagerPOCO.ValidateUpdateReservationAllotmentAndInventory(groupRule, parametersPerDay, validate, update, correlationId, isUpdateReservation, IgnoreAvailability);

            // assert
            Assert.IsNotNull(result);
            // If the result list is the same as mock, it means that the normal behaviour is expected, as the sql was executed
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Date, DateTime.Today);
            Assert.AreEqual(result[0].QtyRoomOccupied, 1);
            Assert.AreEqual(result[0].QtyContractedAllotmentOccupied, 1);
            Assert.AreEqual(result[0].QtyContractedAllotmentTotal, 1);
            Assert.AreEqual(result[0].RoomQuantity, 1);
        }
        #endregion PMS

        #region CANCEL RESERVATION

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_WithPORules()
        {
            #region ARRANGE
            long propertyUID = 1635;
            long channel_UID = 80; //4 Cantos HoteisNet Pull

            //Get and change the reservation
            domainReservation.Reservation res = reservationsList.FirstOrDefault(x => x.UID == 66242);
            res.IsPaid = false;
            res.Status = 1;
            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.Status = 1;
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
                rr.ReservationRoomExtras.Add(new ReservationRoomExtra
                {
                    Extra_UID = 1833,
                    Qty = 1,
                    Total_Price = rr.ReservationRoomsExtrasSum.Value,
                    Total_VAT = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }
            #endregion

            #region Mock Repo
            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, 66242);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);
            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = res.Number,
                ReservationRoomNo = res.ReservationRooms.First().ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty,
                RuleType = Reservation.BL.Constants.RuleType.Pull
            };

            var result = ReservationManagerPOCO.CancelReservation(request);


            var domainResAdditional = reservationAddicionaldataList.First();
            var resAdditionalData = Newtonsoft.Json.JsonConvert.DeserializeObject<OB.Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData>(domainResAdditional.ReservationAdditionalDataJSON);
            var externalSellingRes = resAdditionalData.ExternalSellingReservationInformationByRule.FirstOrDefault();
            var externalSellingRoomBooked = resAdditionalData.ReservationRoomList.Last().ExternalSellingInformationByRule.LastOrDefault();

            // ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, result.Status);
            Assert.AreEqual(4, result.ReservationStatus);

            Assert.IsNotNull(resAdditionalData);
            Assert.IsNotNull(externalSellingRes);
            Assert.IsNotNull(externalSellingRoomBooked);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_ChannelCancellationRoom()
        {
            #region ARRANGE
            OB.Domain.Reservations.Reservation reservation = null;
            List<ReservationRoom> reservationRooms = null;
            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long channel_UID = 32; //Booking

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, 66242);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservationPOCO = Container.Resolve<IReservationManagerPOCO>();

            Random random = new Random();

            reservation = reservationsList.Where(x => x.Channel_UID == channel_UID && x.NumberOfRooms == 2 && x.Property_UID == propertyUID).Take(1).SingleOrDefault();
            reservationRooms = reservation.ReservationRooms.ToList();
            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
                rr.ReservationRoomExtras.Add(new ReservationRoomExtra
                {
                    Extra_UID = 1833,
                    Qty = 1,
                    Total_Price = rr.ReservationRoomsExtrasSum.Value,
                    Total_VAT = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }

            #endregion

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

            var resCancellationFirstRoom = reservationsList.Where(x => x.UID == reservation.UID).SingleOrDefault();
            var resRoomsCancellationFirstRoom = resCancellationFirstRoom.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual((decimal)15, resCancellationFirstRoom.RoomsExtras);
            Assert.AreEqual((decimal)341, resCancellationFirstRoom.RoomsPriceSum);
            Assert.AreEqual((decimal)356, resCancellationFirstRoom.RoomsTotalAmount);
            Assert.AreEqual((decimal)356, resCancellationFirstRoom.TotalAmount);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);
            Assert.AreEqual(32, resCancellationFirstRoom.Channel_UID);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);
            Assert.AreEqual((decimal)45, resRoomsCancellationFirstRoom[0].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)660, resRoomsCancellationFirstRoom[0].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)705, resRoomsCancellationFirstRoom[0].ReservationRoomsTotalAmount);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
            Assert.AreEqual((decimal)15, resRoomsCancellationFirstRoom[1].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)341, resRoomsCancellationFirstRoom[1].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)356, resRoomsCancellationFirstRoom[1].ReservationRoomsTotalAmount);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_BECancellationRoom()
        {
            // arrange
            long propertyUID = 1635;
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var res = reservationsList.Single(x => x.UID == reservation_UID);
            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            // act

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = res.Number,
                ReservationRoomNo = res.ReservationRooms.First().ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, databaseData.reservationDetail.Status);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, databaseData.reservationDetail.CancelReservationComments);

            Assert.AreEqual(2, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);

            Assert.AreEqual(1, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_BECancelReservation()
        {
            #region ARRANGE
            long propertyUID = 1635;
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var res = reservationsList.Single(x => x.UID == reservation_UID);
            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.DateFrom = DateTime.Now.Date;
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }
            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = res.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, databaseData.reservationDetail.Status);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, databaseData.reservationDetail.CancelReservationComments);
            //Assert.AreEqual(1, databaseData.reservationDetail.Channel_UID);

            Assert.AreEqual(2, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);

            Assert.AreEqual(2, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancelledPendingReservation()
        {
            // arrange
            long propertyUID = 1635;
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var res = reservationsList.Single(x => x.UID == reservation_UID);
            res.Status = (int)Constants.ReservationStatus.Pending;
            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            // act
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = res.Number,
                ReservationRoomNo = string.Join(",", res.ReservationRooms.Select(x => x.ReservationRoomNo)),
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(8, databaseData.reservationDetail.Status);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, databaseData.reservationDetail.CancelReservationComments);
            //Assert.AreEqual(1, databaseData.reservationDetail.Channel_UID);

            Assert.AreEqual(8, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);
            //Assert.AreEqual((decimal)0, databaseData.reservationRooms[0].ReservationRoomsExtrasSum);
            //Assert.AreEqual((decimal)110, databaseData.reservationRooms[0].ReservationRoomsPriceSum);
            //Assert.AreEqual((decimal)110, databaseData.reservationRooms[0].ReservationRoomsTotalAmount);

            Assert.AreEqual(8, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_PoliciesNotAllowedCancellation()
        {
            #region ARRANGE
            long propertyUID = 1635;
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var res = reservationsList.Single(x => x.UID == reservation_UID);

            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.DateFrom = DateTime.Now.Date;
                rr.CancellationPolicy = "struff";
                rr.IsCancellationAllowed = false;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }
            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = res.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);

            //// ASSERT
            Assert.AreEqual(-10, result.Result);
            Assert.AreEqual(1, databaseData.reservationDetail.Status);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationComments);
            //Assert.AreEqual(1, databaseData.reservationDetail.Channel_UID);

            Assert.AreEqual(1, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(false, databaseData.reservationRooms[0].IsCancellationAllowed);
            //Assert.AreEqual((decimal)0, databaseData.reservationRooms[0].ReservationRoomsExtrasSum);
            //Assert.AreEqual((decimal)110, databaseData.reservationRooms[0].ReservationRoomsPriceSum);
            //Assert.AreEqual((decimal)110, databaseData.reservationRooms[0].ReservationRoomsTotalAmount);

            Assert.AreEqual(1, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(false, databaseData.reservationRooms[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_BECancelReservationFillCancellationDate()
        {
            #region ARRANGE
            long propertyUID = 1635;
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var res = reservationsList.Single(x => x.UID == reservation_UID);
            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.DateFrom = DateTime.Now.Date;
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }
            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationUID = res.UID,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, databaseData.reservationDetail.Status);

            Assert.AreEqual(2, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);
            Assert.AreEqual(DateTime.UtcNow.Date, databaseData.reservationRooms[0].CancellationDate.Value.Date);

            Assert.AreEqual(2, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
            Assert.AreEqual(DateTime.UtcNow.Date, databaseData.reservationRooms[1].CancellationDate.Value.Date);
        }


        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_BECancellationRoomWithReasonTypeAndComment()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            Random random = new Random();

            var reservation = reservationsList.Single(x => x.UID == reservation_UID);
            var reservationRooms = reservation.ReservationRooms.Where(x => x.Reservation_UID == reservation.UID).ToList();
            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion


            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = 3,
                CancelReservationComments = "Invalid Credit Card"
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservationRooms;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(3, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual("Invalid Credit Card", resCancellationFirstRoom.CancelReservationComments);
            //Assert.AreEqual(1, resCancellationFirstRoom.Channel_UID);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_BECancellationRoomWithPaymentGateway()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            Random random = new Random();

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
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
            Assert.AreEqual(1, result.Result); //error - transaction invalid consult maxipago
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_BECancelReservationWithPaymentGateway()
        {
            #region ARRANGE
            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservationPOCO = Container.Resolve<IReservationManagerPOCO>();

            Random random = new Random();

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
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
            Assert.AreEqual(1, result.Result); //error - transaction invalid consult maxipago
            Assert.AreEqual(2, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestCancelReservation_BECancelReservation_WithInterestRate(Constants.PaymentGateway gateway)
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 67242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Single(x => x.UID == reservation_UID);
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.PaymentGatewayTransactionStatusCode = "1";
            reservation.PaymentGatewayName = gateway.ToString();
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

            //Mock PaymentGateway's
            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>> pgtMock = new Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>>();
                    RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Payments.PaymentGatewayTransaction>(It.IsAny<IUnitOfWork>())).
                        Returns(pgtMock.Object);

                    adyenMock.Setup(x => x.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    pgtMock.Setup(x => x.Any(It.IsAny<Expression<Func<OB.Domain.Payments.PaymentGatewayTransaction, bool>>>()))
                        .Returns(false);

                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true
                    });

                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral> pgMock = new Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral>();
                    pgMock.Setup(x => x.IsTheSameDay(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(false);


                    maxiPagoMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Refund(It.IsAny<payUContracts.RefundRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }


            //Mock Parcelation
            var first = bePartialPaymentCcMethodList.First(r => r.PropertyId == propertyUID);
            reservation.InterestRate = first.InterestRate;
            reservation.IsPartialPayment = true;
            reservation.ReservationPartialPaymentDetails.ForEach(r =>
            {
                r.InterestRate = first.InterestRate;
                r.InstallmentNo = first.Parcel;
            });
            MockReservationHelperPockMethods(true);

            var result = ReservationManagerPOCO.CancelReservation(request);


            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            decimal reservationValue = reservation.TotalAmount.Value * (1 + reservation.InterestRate.Value / 100);
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Void(It.Is<VoidRequest>(pgReq => pgReq.Amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Refund(It.Is<payUContracts.RefundRequest>(requestR => requestR.Amount == null)));
                    break;
            }
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestCancelReservation_BECancelReservation_WithInterestRate_SkipInterestRateCalculation(Constants.PaymentGateway gateway)
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 67242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Single(x => x.UID == reservation_UID);
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.PaymentGatewayTransactionStatusCode = "1";
            reservation.PaymentGatewayName = gateway.ToString();
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
                CancelReservationComments = string.Empty,
                SkipInterestCalculation = true,
            };

            //Mock PaymentGateway's
            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>> pgtMock = new Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>>();
                    RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Payments.PaymentGatewayTransaction>(It.IsAny<IUnitOfWork>())).
                        Returns(pgtMock.Object);

                    adyenMock.Setup(x => x.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    pgtMock.Setup(x => x.Any(It.IsAny<Expression<Func<OB.Domain.Payments.PaymentGatewayTransaction, bool>>>()))
                        .Returns(false);

                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true
                    });

                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral> pgMock = new Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral>();
                    pgMock.Setup(x => x.IsTheSameDay(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(false);


                    maxiPagoMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;

                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Refund(It.IsAny<payUContracts.RefundRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }


            //Mock Parcelation
            var first = bePartialPaymentCcMethodList.First(r => r.PropertyId == 1635);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            MockReservationHelperPockMethods(true);

            var result = ReservationManagerPOCO.CancelReservation(request);


            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            decimal reservationValue = reservation.TotalAmount.Value;
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Void(It.Is<VoidRequest>(pgReq => pgReq.Amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Refund(It.Is<payUContracts.RefundRequest>(requestR => requestR.Amount == null)));
                    break;

            }
        }

        //#region PULL
        ///// <summary>
        ///// 
        ///// </summary>
        ////[TestMethod]
        ////[TestCategory("CancelReservationTest")]
        ////[DeploymentItem("./Databases_WithData", "TestCancelReservation_CancellationRoomTypePull")]
        ////public void TestCancelReservation_CancellationRoomTypePull()
        ////{
        ////    #region ARRANGE
        ////    ProturEntities context = new ProturEntities();
        ////    BookingEngineService bookingEngineService = new BookingEngineService();
        ////    Random random = new Random();
        ////    long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
        ////    long channel_UID = 80; //4 Cantos HoteisNet Pull

        ////    Reservation reservation = context.Reservations.Where(x => x.Channel_UID == channel_UID && x.NumberOfRooms == 2 && x.Property_UID == propertyUID).Take(1).SingleOrDefault();
        ////    reservation.Status = 1;
        ////    var reservationRooms = context.ReservationRooms.Where(x => x.Reservation_UID == reservation.UID).ToList();
        ////    foreach (ReservationRoom rr in reservationRooms)
        ////    {
        ////        rr.Status = 1;
        ////    }

        ////    OperatorType notop = new OperatorType() { Name = "Not Operator", Code = 0 };
        ////    OperatorType op = new OperatorType() { Name = "Operator", Code = 1 };
        ////    OperatorType hnet = new OperatorType() { Name = "HoteisNet", Code = 2 };
        ////    context.OperatorTypes.AddObject(notop);
        ////    context.OperatorTypes.AddObject(op);
        ////    context.OperatorTypes.AddObject(hnet);

        ////    ChannelType push = new ChannelType() { Name = "PUSH", Code = 0 };
        ////    ChannelType gds = new ChannelType() { Name = "GDS", Code = 1 };
        ////    ChannelType pull = new ChannelType() { Name = "PULL", Code = 2 };
        ////    context.ChannelTypes.AddObject(push);
        ////    context.ChannelTypes.AddObject(gds);
        ////    context.ChannelTypes.AddObject(pull);

        ////    context.SaveChanges();
        ////    #endregion

        ////    //// ACT
        ////    int result = bookingEngineService.CancelReservation(
        ////        propertyUID,
        ////        reservation.Number,
        ////        reservationRooms[0].ReservationRoomNo,
        ////        (int)Constants.BookingEngineUserType.Guest,
        ////        null,
        ////        string.Empty);

        ////    context.Dispose();
        ////    ProturEntities newContext = new ProturEntities();

        ////    Reservation resCancellationFirstRoom = newContext.Reservations.Where(x => x.UID == reservation.UID).SingleOrDefault();
        ////    var resRoomsCancellationFirstRoom = newContext.ReservationRooms.Where(x => x.Reservation_UID == reservation.UID).ToList();

        ////    //var opt = newContext.OperatorTypes.ToList();
        ////    //var ch = newContext.ChannelTypes.ToList();

        ////    // ASSERT
        ////    Assert.AreEqual(-10, result);
        ////    Assert.AreEqual(1, resCancellationFirstRoom.Status);
        ////    Assert.AreEqual(72, resCancellationFirstRoom.RoomsExtras);
        ////    Assert.AreEqual(2091, resCancellationFirstRoom.RoomsPriceSum);
        ////    Assert.AreEqual(2163, resCancellationFirstRoom.RoomsTotalAmount);
        ////    Assert.AreEqual(2163, resCancellationFirstRoom.TotalAmount);
        ////    Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
        ////    Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);
        ////    Assert.AreEqual(80, resCancellationFirstRoom.Channel_UID);

        ////    Assert.AreEqual(1, resRoomsCancellationFirstRoom[0].Status);
        ////    Assert.AreEqual(null, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);
        ////    Assert.AreEqual(28, resRoomsCancellationFirstRoom[0].ReservationRoomsExtrasSum);
        ////    Assert.AreEqual(1106, resRoomsCancellationFirstRoom[0].ReservationRoomsPriceSum);
        ////    Assert.AreEqual(1134, resRoomsCancellationFirstRoom[0].ReservationRoomsTotalAmount);
        ////    Assert.AreEqual(null, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

        ////    Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
        ////    Assert.AreEqual(null, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        ////    Assert.AreEqual(44, resRoomsCancellationFirstRoom[1].ReservationRoomsExtrasSum);
        ////    Assert.AreEqual(985, resRoomsCancellationFirstRoom[1].ReservationRoomsPriceSum);
        ////    Assert.AreEqual(1029, resRoomsCancellationFirstRoom[1].ReservationRoomsTotalAmount);
        ////    Assert.AreEqual(null, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        ////}


        ///// <summary>
        ///// 
        ///// </summary>
        ////[TestMethod]
        ////[TestCategory("CancelReservationTest")]
        ////[DeploymentItem("./Databases_WithData", "TestCancelReservation_CancelReservationTypePull")]
        ////public void TestCancelReservation_CancelReservationTypePull()
        ////{
        ////    #region ARRANGE
        ////    ProturEntities context = new ProturEntities();
        ////    BookingEngineService bookingEngineService = new BookingEngineService();
        ////    Random random = new Random();
        ////    long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
        ////    long channel_UID = 80; //4 Cantos HoteisNet Pull

        ////    Reservation reservation = context.Reservations.Where(x => x.Channel_UID == channel_UID && x.NumberOfRooms == 2 && x.Property_UID == propertyUID).Take(1).SingleOrDefault();
        ////    reservation.Status = 1;
        ////    var reservationRooms = context.ReservationRooms.Where(x => x.Reservation_UID == reservation.UID).ToList();
        ////    foreach (ReservationRoom rr in reservationRooms)
        ////    {
        ////        rr.Status = 1;
        ////    }

        ////    OperatorType notop = new OperatorType() { Name = "Not Operator", Code = 0 };
        ////    OperatorType op = new OperatorType() { Name = "Operator", Code = 1 };
        ////    OperatorType hnet = new OperatorType() { Name = "HoteisNet", Code = 2 };
        ////    context.OperatorTypes.AddObject(notop);
        ////    context.OperatorTypes.AddObject(op);
        ////    context.OperatorTypes.AddObject(hnet);

        ////    ChannelType push = new ChannelType() { Name = "PUSH", Code = 0 };
        ////    ChannelType gds = new ChannelType() { Name = "GDS", Code = 1 };
        ////    ChannelType pull = new ChannelType() { Name = "PULL", Code = 2 };
        ////    context.ChannelTypes.AddObject(push);
        ////    context.ChannelTypes.AddObject(gds);
        ////    context.ChannelTypes.AddObject(pull);

        ////    context.SaveChanges();
        ////    #endregion

        ////    //// ACT
        ////    int result = bookingEngineService.CancelReservationByUID(
        ////     propertyUID,
        ////     reservation.UID,
        ////     (int)Constants.BookingEngineUserType.Guest,
        ////     null,
        ////     string.Empty);


        ////    context.Dispose();
        ////    ProturEntities newContext = new ProturEntities();

        ////    Reservation resCancellationFirstRoom = newContext.Reservations.Where(x => x.UID == reservation.UID).SingleOrDefault();
        ////    var resRoomsCancellationFirstRoom = newContext.ReservationRooms.Where(x => x.Reservation_UID == reservation.UID).ToList();

        ////    // ASSERT
        ////    Assert.AreEqual(-10, result);
        ////    Assert.AreEqual(1, resCancellationFirstRoom.Status);
        ////    Assert.AreEqual(72, resCancellationFirstRoom.RoomsExtras);
        ////    Assert.AreEqual(2091, resCancellationFirstRoom.RoomsPriceSum);
        ////    Assert.AreEqual(2163, resCancellationFirstRoom.RoomsTotalAmount);
        ////    Assert.AreEqual(2163, resCancellationFirstRoom.TotalAmount);
        ////    Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
        ////    Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);
        ////    Assert.AreEqual(80, resCancellationFirstRoom.Channel_UID);

        ////    Assert.AreEqual(1, resRoomsCancellationFirstRoom[0].Status);
        ////    Assert.AreEqual(null, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);
        ////    Assert.AreEqual(28, resRoomsCancellationFirstRoom[0].ReservationRoomsExtrasSum);
        ////    Assert.AreEqual(1106, resRoomsCancellationFirstRoom[0].ReservationRoomsPriceSum);
        ////    Assert.AreEqual(1134, resRoomsCancellationFirstRoom[0].ReservationRoomsTotalAmount);
        ////    Assert.AreEqual(null, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

        ////    Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
        ////    Assert.AreEqual(null, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        ////    Assert.AreEqual(44, resRoomsCancellationFirstRoom[1].ReservationRoomsExtrasSum);
        ////    Assert.AreEqual(985, resRoomsCancellationFirstRoom[1].ReservationRoomsPriceSum);
        ////    Assert.AreEqual(1029, resRoomsCancellationFirstRoom[1].ReservationRoomsTotalAmount);
        ////    Assert.AreEqual(null, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        ////}
        //#endregion

        ///// <summary>
        ///// 
        ///// </summary>
        //[Ignore]//ReDo test after release and don't know if it covers the travelAgent path, as chick norris maybe is not travelAgent
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CreditTravelAgent()
        {
            #region ARRANGE
            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;
            long tpi_UID = 61; // chuck norris, maybe is not travelAgent

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            TPIProperty tpiProperty = null;

            tpi = tpisList.Where(x => x.UID == tpi_UID).SingleOrDefault();
            tpi.Property_UID = propertyUID;

            tpiProperty = new TPIProperty();
            tpiProperty.TPI_UID = tpi.UID;
            tpiProperty.Property_UID = tpi.Property_UID.Value;
            tpiProperty.CreditLimit = 120;
            tpiProperty.CreditUsed = 1326;
            tpiPropertyList.Add(tpiProperty);

            //Prepare the reservation
            //var resNumberExpected = "RES000024-" + propertyUID;
            //var builder = new Operations.Helper.SearchBuilder(Container, propertyUID);
            //var resBuilder = new ReservationDataBuilder(1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
            //    .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithBilledTypePayment(4).WithTPI(tpi);
            //var transactionId = Guid.NewGuid().ToString();
            //resBuilder.InputData.guest.UID = 1;

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Take(1).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = false;
            reservation.TotalAmount = 800;
            reservation.RoomsTotalAmount = 800;
            reservation.RoomsPriceSum = 800;
            reservation.PaymentMethodType_UID = 4;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.TravelAgent,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);


            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT

            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual((decimal)60, resCancellationFirstRoom.RoomsExtras);
            Assert.AreEqual((decimal)140, resCancellationFirstRoom.RoomsPriceSum);
            Assert.AreEqual((decimal)140, resCancellationFirstRoom.RoomsTotalAmount);
            Assert.AreEqual((decimal)140, resCancellationFirstRoom.TotalAmount);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);
            Assert.AreEqual(32, resCancellationFirstRoom.Channel_UID);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);
            Assert.AreEqual((decimal)45, resRoomsCancellationFirstRoom[0].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)660, resRoomsCancellationFirstRoom[0].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)705, resRoomsCancellationFirstRoom[0].ReservationRoomsTotalAmount);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
            Assert.AreEqual((decimal)15, resRoomsCancellationFirstRoom[1].ReservationRoomsExtrasSum);
            Assert.AreEqual((decimal)341, resRoomsCancellationFirstRoom[1].ReservationRoomsPriceSum);
            Assert.AreEqual((decimal)356, resRoomsCancellationFirstRoom[1].ReservationRoomsTotalAmount);

            Assert.AreEqual((decimal)1326, tpiProperty.CreditUsed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CreditTravelAgentDirect()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            Channel agenciesDirect = null;
            var reservationPOCO = Container.Resolve<IReservationManagerPOCO>();

            Random random = new Random();

            agenciesDirect = channelsList.FirstOrDefault(c => c.ChannelCode == "DAGEN");

            var channelsProperties = new ChannelsProperty();
            channelsProperties.Channel_UID = agenciesDirect.UID;
            channelsProperties.Property_UID = propertyUID;
            channelsProperties.IsActive = true;
            channelsProperties.RateModel_UID = 1;
            channelsProperties.Value = 15;
            channelsProperties.OperatorBillingType = 1;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = true;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 2000;
            channelsProperties.IsActivePrePaymentCredit = false;
            channelsProperties.PrePaymentCreditLimit = 500;
            channelsProperties.PrePaymentCreditUsed = 300;
            chPropsList.Add(channelsProperties);

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            reservation.IsPaid = false;
            reservation.Channel_UID = agenciesDirect.UID;
            reservation.PaymentMethodType_UID = 4;
            reservation.PaymentGatewayName = null;

            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.TravelAgent,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            var cp = channelsProperties;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual(true, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)2000, cp.OperatorCreditUsed);
            Assert.AreEqual(false, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)500, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)300, cp.PrePaymentCreditUsed);
        }


        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CreditCorporatesDirect()
        {
            #region ARRANGE
            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var corporateDirect = channelsList.First(c => c.ChannelCode == "DCORP");

            ChannelsProperty channelsProperties = new ChannelsProperty();

            channelsProperties.Channel_UID = corporateDirect.UID;
            channelsProperties.Property_UID = propertyUID;
            channelsProperties.IsActive = true;
            channelsProperties.RateModel_UID = 2;
            channelsProperties.Value = 15;
            channelsProperties.OperatorBillingType = 1;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = true;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 2000;
            channelsProperties.IsActivePrePaymentCredit = false;
            channelsProperties.PrePaymentCreditLimit = 500;
            channelsProperties.PrePaymentCreditUsed = 300;
            chPropsList.Add(channelsProperties);

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Take(1).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = false;
            reservation.Channel_UID = corporateDirect.UID;
            reservation.PaymentMethodType_UID = 4;
            reservation.PaymentGatewayName = null;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
                rr.ReservationRoomExtras.Add(new ReservationRoomExtra
                {
                    Extra_UID = 1833,
                    Qty = 1,
                    Total_Price = rr.ReservationRoomsExtrasSum.Value,
                    Total_VAT = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }


            #endregion

            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Corporate,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            var cp = channelsProperties;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);
            Assert.AreEqual(corporateDirect.UID, resCancellationFirstRoom.Channel_UID);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual(true, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)2000, cp.OperatorCreditUsed);
            Assert.AreEqual(false, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)500, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)300, cp.PrePaymentCreditUsed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_PrePaymentCreditTravelAgentDirect()
        {
            #region ARRANGE
            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var agenciesDirect = channelsList.First(c => c.ChannelCode == "DAGEN");

            ChannelsProperty channelsProperties = new ChannelsProperty();
            channelsProperties.Channel_UID = agenciesDirect.UID;
            channelsProperties.Property_UID = propertyUID;
            channelsProperties.IsActive = true;
            channelsProperties.RateModel_UID = 1;
            channelsProperties.Value = 15;
            channelsProperties.OperatorBillingType = 1;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = false;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 2000;
            channelsProperties.IsActivePrePaymentCredit = true;
            channelsProperties.PrePaymentCreditLimit = 1300;
            channelsProperties.PrePaymentCreditUsed = 2832;
            chPropsList.Add(channelsProperties);

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Take(1).SingleOrDefault();
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = false;
            reservation.Channel_UID = agenciesDirect.UID;
            reservation.PaymentMethodType_UID = 7;
            reservation.PaymentGatewayName = null;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion


            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.TravelAgent,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();
            var cp = channelsProperties;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);
            Assert.AreEqual(agenciesDirect.UID, resCancellationFirstRoom.Channel_UID);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual(false, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)2000, cp.OperatorCreditUsed);
            Assert.AreEqual(true, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)1300, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)2832, cp.PrePaymentCreditUsed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_PrePaymentCreditCorporatesDirect()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var corporateDirect = channelsList.First(c => c.ChannelCode == "DCORP");

            ChannelsProperty channelsProperties = new ChannelsProperty();
            channelsProperties.Channel_UID = corporateDirect.UID;
            channelsProperties.Property_UID = propertyUID;
            channelsProperties.IsActive = true;
            channelsProperties.RateModel_UID = 1;
            channelsProperties.Value = 15;
            channelsProperties.OperatorBillingType = 1;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = false;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 2000;
            channelsProperties.IsActivePrePaymentCredit = true;
            channelsProperties.PrePaymentCreditLimit = 1300;
            channelsProperties.PrePaymentCreditUsed = 2832;
            chPropsList.Add(channelsProperties);

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = false;
            reservation.Channel_UID = corporateDirect.UID;
            reservation.PaymentMethodType_UID = 7;
            reservation.PaymentGatewayName = null;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion


            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.TravelAgent,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();
            var cp = channelsProperties;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);
            Assert.AreEqual(corporateDirect.UID, resCancellationFirstRoom.Channel_UID);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual(false, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)2000, cp.OperatorCreditUsed);
            Assert.AreEqual(true, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)1300, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)2832, cp.PrePaymentCreditUsed);

        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CreditTravelAgent_ReservationAlreadyPaid()
        {
            #region ARRANGE
            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long tpi_UID = 47; //FL Travel Services
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            Random random = new Random();

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = true;
            reservation.TotalAmount = 800;
            reservation.RoomsTotalAmount = 800;
            reservation.RoomsPriceSum = 800;
            reservation.PaymentMethodType_UID = 4;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            var tpip = new TPIProperty
            {
                UID = 4,
                TPI_UID = tpi_UID,
                Property_UID = propertyUID,
                CreditLimit = null,
                CreditUsed = 1326
            };

            tpiPropertyList.Add(tpip);


            #endregion


            //// ACT
            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.TravelAgent,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            TPIProperty tpiProperty = tpip;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual((decimal)1326, tpiProperty.CreditUsed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CreditOperator_ReservationAlreadyPaid()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long channel_UID = 80; //4 Cantos HoteisNet Pull
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = true;
            reservation.Status = 1;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.Status = 1;
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
                rr.ReservationRoomExtras.Add(new ReservationRoomExtra
                {
                    Extra_UID = 1833,
                    Qty = 1,
                    Total_Price = rr.ReservationRoomsExtrasSum.Value,
                    Total_VAT = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }

            var channelsProperties = chPropsList.Where(x => x.Property_UID == propertyUID && x.Channel_UID == channel_UID).SingleOrDefault();
            channelsProperties.OperatorBillingType = 0;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = true;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 3000;
            channelsProperties.IsActivePrePaymentCredit = false;
            channelsProperties.PrePaymentCreditLimit = 500;
            channelsProperties.PrePaymentCreditUsed = 300;

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

            var cp = channelsProperties;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual(true, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)3000, cp.OperatorCreditUsed);
            Assert.AreEqual(false, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)500, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)300, cp.PrePaymentCreditUsed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CreditTravelAgentDirect_ReservationAlreadyPaid()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var agenciesDirect = channelsList.First(c => c.ChannelCode == "DAGEN");

            ChannelsProperty channelsProperties = new ChannelsProperty();
            channelsProperties.Channel_UID = agenciesDirect.UID;
            channelsProperties.Property_UID = propertyUID;
            channelsProperties.IsActive = true;
            channelsProperties.RateModel_UID = 1;
            channelsProperties.Value = 15;
            channelsProperties.OperatorBillingType = 1;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = true;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 2000;
            channelsProperties.IsActivePrePaymentCredit = false;
            channelsProperties.PrePaymentCreditLimit = 500;
            channelsProperties.PrePaymentCreditUsed = 300;
            chPropsList.Add(channelsProperties);

            var reservation = reservationsList.First(x => x.UID == reservation_UID);
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = true;
            reservation.Channel_UID = agenciesDirect.UID;
            reservation.PaymentMethodType_UID = 4;
            reservation.PaymentGatewayName = null;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.TravelAgent,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            var cp = channelsProperties;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);
            Assert.AreEqual(agenciesDirect.UID, resCancellationFirstRoom.Channel_UID);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual(true, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)2000, cp.OperatorCreditUsed);
            Assert.AreEqual(false, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)500, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)300, cp.PrePaymentCreditUsed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Ignore]//ReDo test after release
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CreditCorporatesDirect_ReservationAlreadyPaid()
        {
            #region ARRANGE
            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            Random random = new Random();

            var corporateDirect = channelsList.First(c => c.ChannelCode == "DCORP");

            ChannelsProperty channelsProperties = new ChannelsProperty();

            channelsProperties.Channel_UID = corporateDirect.UID;
            channelsProperties.Property_UID = propertyUID;
            channelsProperties.IsActive = true;
            channelsProperties.RateModel_UID = 2;
            channelsProperties.Value = 15;
            channelsProperties.OperatorBillingType = 1;
            channelsProperties.IsActive = true;
            channelsProperties.IsOperatorsCreditLimit = true;
            channelsProperties.OperatorCreditLimit = 1000;
            channelsProperties.OperatorCreditUsed = 2000;
            channelsProperties.IsActivePrePaymentCredit = false;
            channelsProperties.PrePaymentCreditLimit = 500;
            channelsProperties.PrePaymentCreditUsed = 300;
            chPropsList.Add(channelsProperties);

            var reservation = reservationsList.First(x => x.UID == reservation_UID);
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = true;
            reservation.Channel_UID = corporateDirect.UID;
            reservation.PaymentMethodType_UID = 4;
            reservation.PaymentGatewayName = null;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Corporate,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            var cp = channelsProperties;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual(true, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)2000, cp.OperatorCreditUsed);
            Assert.AreEqual(false, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)500, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)300, cp.PrePaymentCreditUsed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_PrePaymentCreditOperator_ReservationAlreadyPaid()
        {
            #region ARRANGE
            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long channel_UID = 80; //4 Cantos HoteisNet Pull
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.First(x => x.UID == reservation_UID);
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.IsPaid = true;
            reservation.Status = 1;
            reservation.PaymentMethodType_UID = 7;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.Status = 1;
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 0;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
                rr.ReservationRoomExtras.Add(new ReservationRoomExtra
                {
                    Extra_UID = 1833,
                    Qty = 1,
                    Total_Price = rr.ReservationRoomsExtrasSum.Value,
                    Total_VAT = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }

            var channelsProperties = new ChannelsProperty
            {
                Property_UID = propertyUID,
                Channel_UID = channel_UID,
                OperatorBillingType = 0,
                IsActive = true,
                IsOperatorsCreditLimit = false,
                OperatorCreditLimit = 1000,
                OperatorCreditUsed = 3000,
                IsActivePrePaymentCredit = true,
                PrePaymentCreditLimit = 1200,
                PrePaymentCreditUsed = 3500
            };

            chPropsList.Add(channelsProperties);

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

            var cp = channelsProperties;

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);

            Assert.AreEqual(false, cp.IsOperatorsCreditLimit);
            Assert.AreEqual((decimal)1000, cp.OperatorCreditLimit);
            Assert.AreEqual((decimal)3000, cp.OperatorCreditUsed);
            Assert.AreEqual(true, cp.IsActivePrePaymentCredit);
            Assert.AreEqual((decimal)1200, cp.PrePaymentCreditLimit);
            Assert.AreEqual((decimal)3500, cp.PrePaymentCreditUsed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_BECancellationEachRooms()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

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

            if (SessionFactory.CurrentUnitOfWork != null && !SessionFactory.CurrentUnitOfWork.IsDisposed)
                SessionFactory.CurrentUnitOfWork.Dispose();

            request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[1].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result2 = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(1, result2.Result);
            Assert.AreEqual(2, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_PoliciesAllowedCancelDaysPriorToArrivalDate()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            var reservationPOCO = Container.Resolve<IReservationManagerPOCO>();

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.Date = DateTime.Now.AddDays(3).Date;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.DateFrom = DateTime.Now.AddDays(3).Date;
                rr.DateTo = DateTime.Now.AddDays(5).Date;
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 2;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

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
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);

            Assert.AreEqual(2, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_PoliciesNotAllowedCancelDaysPriorToArrivalDate()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            reservation.Date = DateTime.Now.AddDays(1).Date;

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.DateFrom = DateTime.Now.AddDays(1).Date;
                rr.DateTo = DateTime.Now.AddDays(2).Date;
                rr.CancellationPolicy = "struff";
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = 2;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
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
            Assert.AreEqual(-10, result.Result);
            Assert.AreEqual(1, resCancellationFirstRoom.Status);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationReason_UID);
            Assert.AreEqual(null, resCancellationFirstRoom.CancelReservationComments);

            Assert.AreEqual(1, resRoomsCancellationFirstRoom[0].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[0].IsCancellationAllowed);
            Assert.AreEqual(1, resRoomsCancellationFirstRoom[1].Status);
            Assert.AreEqual(true, resRoomsCancellationFirstRoom[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_ChangeOccupancyInReservation_CancellationEachRooms()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

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

            var reservation1 = reservation.Clone();

            if (SessionFactory.CurrentUnitOfWork != null && !SessionFactory.CurrentUnitOfWork.IsDisposed)
                SessionFactory.CurrentUnitOfWork.Dispose();

            request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[1].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result2 = ReservationManagerPOCO.CancelReservation(request);

            var reservation2 = reservation;
            var resRoomsCancellationFirstRoom = reservation2.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(1, result2.Result);
            Assert.AreEqual(4, reservation1.Status);
            Assert.AreEqual(2, reservation2.Status);
            Assert.AreEqual(null, reservation1.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation1.CancelReservationComments);
            Assert.AreEqual(2, reservation1.Adults);
            Assert.AreEqual(1, reservation1.Children);
            Assert.AreEqual(2, reservation2.Adults);
            Assert.AreEqual(1, reservation2.Children);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_ChangeNumberOfRooms_According_to_cancelled_rooms()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

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

            var reservation1 = reservation.Clone();

            if (SessionFactory.CurrentUnitOfWork != null && !SessionFactory.CurrentUnitOfWork.IsDisposed)
                SessionFactory.CurrentUnitOfWork.Dispose();

            request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[1].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result2 = ReservationManagerPOCO.CancelReservation(request);

            var reservation2 = reservation;
            var resRoomsCancellationFirstRoom = reservation2.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(1, result2.Result);
            Assert.AreEqual(4, reservation1.Status);
            Assert.AreEqual(2, reservation2.Status);
            Assert.AreEqual(null, reservation1.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation1.CancelReservationComments);
            Assert.AreEqual(2, reservation1.Adults);
            Assert.AreEqual(1, reservation1.Children);
            Assert.AreEqual(2, reservation2.Adults);
            Assert.AreEqual(1, reservation2.Children);
            Assert.AreEqual(1, reservation1.NumberOfRooms);
            Assert.AreEqual(1, reservation2.NumberOfRooms);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_FullCancel_CannotChangeNumberOfRooms()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(2, reservation.NumberOfRooms);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancelOneRoom()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

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

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(2, reservation.Adults);
            Assert.AreEqual(1, reservation.Children);
            Assert.AreEqual(1, reservation.NumberOfRooms);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancelOneRoom_WithNullNumberOfRooms()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            reservation.NumberOfRooms = null;
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

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

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(2, reservation.Adults);
            Assert.AreEqual(1, reservation.Children);
            Assert.AreEqual(1, reservation.NumberOfRooms);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_ChangeOccupancyInReservation_CancellationReservation()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(5, reservation.Adults);
            Assert.AreEqual(2, reservation.Children);

        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_WrongOccupancyInReservation_CancellationOneRoom()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66243;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

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
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(3, reservation.Adults);
            Assert.AreEqual(2, reservation.Children);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_WrongOccupancyInReservation_CancellationReservation()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66243;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(2, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(1, reservation.Adults);
            Assert.AreEqual(1, reservation.Children);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_OneRoomWithoutOccupancy_CancellationOneRoom()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66244;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[1].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(3, reservation.Adults);
            Assert.AreEqual(1, reservation.Children);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancellationOneRoom_OneRoomWithoutOccupancy()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66244;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[2].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(5, reservation.Adults);
            Assert.AreEqual(2, reservation.Children);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancellationOneRoom_ReservationWithoutChildrenOccupancy()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66245;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[2].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            var resCancellationFirstRoom = reservation;
            var resRoomsCancellationFirstRoom = reservation.ReservationRooms.ToList();

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(5, reservation.Adults);
            Assert.AreEqual(null, reservation.Children);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_ActiveNotAllowCancelReservation()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            MockOBBeSettingsRepo(true);

            MockOBPropertyRepo();

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty,
                RuleType = Reservation.BL.Constants.RuleType.BE,
                TemplateCancelRequest = new contractsReservations.TemplateCancelRequest
                {
                    Comments = "test",
                    GuestEmail = "teste@omnibees.com",
                    GuestName = "test1",
                    GuestPhone = "2023284244",
                    PropertyName = "wqdiwqdwqod"
                }
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, result.Status);
            Assert.AreEqual(0, result.Result);
            Assert.IsNull(result.ReservationStatus);
            Assert.AreEqual(false, result.Costs.Any());

        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancellationOneRoom_ActiveNotAllowCancelReservation()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            MockOBBeSettingsRepo(true);

            MockOBPropertyRepo();

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty,
                RuleType = Reservation.BL.Constants.RuleType.BE,
                TemplateCancelRequest = new contractsReservations.TemplateCancelRequest
                {
                    Comments = "test",
                    GuestEmail = "teste@omnibees.com",
                    GuestName = "test1",
                    GuestPhone = "2023284244",
                    PropertyName = "wqdiwqdwqod"
                }
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            //// ASSERT
            Assert.AreEqual(Reservation.BL.Contracts.Responses.Status.Success, result.Status);
            Assert.AreEqual(0, result.Result);
            Assert.IsNull(result.ReservationStatus);
            Assert.AreEqual(false, result.Costs.Any());
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancellationOneRoom_ActiveNotAllowCancelReservation_And_EmptyTemplate()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            MockOBBeSettingsRepo(true);

            MockOBPropertyRepo();

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty,
                RuleType = Reservation.BL.Constants.RuleType.BE
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(2, reservation.Adults);
            Assert.AreEqual(1, reservation.Children);
            Assert.AreEqual(1, reservation.NumberOfRooms);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancellationOneRoom_ActiveNotAllowCancelReservation_Without_ProactiveActionsForHotel()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            MockOBBeSettingsRepo(true);

            proactiveActionsInDataBase = new List<Contracts.Data.ProactiveActions.ProactiveAction>(); // empty list
            MockOBPropertyRepo();

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty,
                RuleType = Reservation.BL.Constants.RuleType.BE,
                TemplateCancelRequest = new contractsReservations.TemplateCancelRequest
                {
                    Comments = "test",
                    GuestEmail = "teste@omnibees.com",
                    GuestName = "test1",
                    GuestPhone = "2023284244",
                    PropertyName = "wqdiwqdwqod"
                }
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(2, reservation.Adults);
            Assert.AreEqual(1, reservation.Children);
            Assert.AreEqual(1, reservation.NumberOfRooms);
        }

        [TestMethod]
        [TestCategory("CancelReservation")]
        public void TestCancelReservation_CancellationOneRoom_Inactive_NotAllowCancelReservation()
        {
            #region ARRANGE

            long propertyUID = 1635; //_Hotel Newcity Demo FL - Base Currency EUR
            long reservation_UID = 66242;

            Random random = new Random();

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            resBuilder = resBuilder.WithCancelationPolicy(false, 0, false, 0);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            MockOBBeSettingsRepo(null);

            MockOBPropertyRepo();

            #endregion

            var reservation = reservationsList.Where(x => x.UID == reservation_UID).Single();
            var reservationRooms = reservation.ReservationRooms.ToList();

            foreach (ReservationRoom rr in reservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            #endregion

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = reservation.Number,
                ReservationRoomNo = reservationRooms[0].ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty,
                RuleType = Reservation.BL.Constants.RuleType.BE
            };

            var result = ReservationManagerPOCO.CancelReservation(request);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, reservation.Status);
            Assert.AreEqual(null, reservation.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, reservation.CancelReservationComments);
            Assert.AreEqual(2, reservation.Adults);
            Assert.AreEqual(1, reservation.Children);
            Assert.AreEqual(1, reservation.NumberOfRooms);
        }

        #endregion

        #region UPDATE RESERVATION
        ///// <summary>
        ///// it should create a new guest and associate with the reservation
        ///// [REFACTOR]Rooms not changed but sent will remove all rooms and add back all rooms received on request
        ///// </summary>
        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_UpdateAllFields_WithNewGuest()
        {
            #region ARRANGE

            long reservation_UID = 66242;

            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);


            builder.WithNewGuest()
                //.WithRoom(1, 1)
                .WithCreditCardPayment();

            // Change the Reservation UID
            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.ExpectedData.reservationDetail.UID = reservation_UID;

            // Change the Reservation Property_UID
            builder.InputData.reservationDetail.Property_UID = 1635;
            builder.ExpectedData.reservationDetail.Property_UID = 1635;

            // Change Reservation Number
            builder.InputData.reservationDetail.Number = "Orbitz-7G7XPR";
            builder.ExpectedData.reservationDetail.Number = "Orbitz-7G7XPR";

            // Change Guest Property Uid
            builder.InputData.guest.Property_UID = 1635;
            builder.ExpectedData.guest.Property_UID = 1635;


            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = 32;
            builder.ExpectedData.reservationDetail.Channel_UID = 32;

            builder.InputData.reservationDetail.ModifyDate = DateTime.Now.Date;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;

            builder.ExpectedData.reservationRooms.FirstOrDefault().ReservationRoomNo = "1";
            builder.ExpectedData.reservationRooms.ForEach(x =>
            {
                x.IsCancellationAllowed = false;
                x.CommissionType = 1;
            });
            #endregion

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            ReservationAssert.AssertAllReservationObjects(builder.ExpectedData, databaseData);
        }

        /// <summary>
        /// it should update the existing guest
        /// </summary>
        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_UpdateAllFields_WithExistingGuest()
        {
            #region ARRANGE

            long reservation_UID = 66242;

            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            builder
            .WithNewGuest()
            //.WithRoom(1, 1)
            .WithCreditCardPayment();

            // Change the Reservation UID
            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.ExpectedData.reservationDetail.UID = reservation_UID;

            // Change the Reservation Property_UID
            builder.InputData.reservationDetail.Property_UID = 1635;
            builder.ExpectedData.reservationDetail.Property_UID = 1635;

            // Change Reservation Number
            builder.InputData.reservationDetail.Number = "Orbitz-7G7XPR";
            builder.ExpectedData.reservationDetail.Number = "Orbitz-7G7XPR";

            // Change Guest Property Uid
            builder.InputData.guest.Property_UID = 1635;
            builder.ExpectedData.guest.Property_UID = 1635;

            // Map to Existing Guest
            var guestUID = guestsList.First().UID;

            builder.InputData.guest.UID = guestUID;
            builder.ExpectedData.guest.UID = guestUID;


            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = 32;
            builder.ExpectedData.reservationDetail.Channel_UID = 32;

            builder.InputData.reservationDetail.ModifyDate = DateTime.Now.Date;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;

            builder.ExpectedData.reservationRooms.FirstOrDefault().ReservationRoomNo = "1";
            builder.ExpectedData.reservationRooms.ForEach(x =>
            {
                x.IsCancellationAllowed = false;
                x.CommissionType = 1;
            });

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            ReservationAssert.AssertAllReservationObjects(builder.ExpectedData, databaseData);
        }

        /// <summary>
        /// it should update the existing guest
        /// </summary>
        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_UpdateAllFields_WithExistingCreditCard_TryToUpdateToNULL_It_Should_Not_Change()
        {
            #region ARRANGE

            long reservation_UID = 66242;
            long property_UID = 1635;
            long channel_UID = 32;

            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            builder
                .WithNewGuest()
                //.WithRoom(1, 1)
                .WithCreditCardPayment();

            // Change the Reservation UID
            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.ExpectedData.reservationDetail.UID = reservation_UID;

            // Change the Reservation Property_UID
            builder.InputData.reservationDetail.Property_UID = property_UID;
            builder.ExpectedData.reservationDetail.Property_UID = property_UID;

            // Change Reservation Number
            builder.InputData.reservationDetail.Number = "Orbitz-7G7XPR";
            builder.ExpectedData.reservationDetail.Number = "Orbitz-7G7XPR";

            // Change Guest Property Uid
            builder.InputData.guest.Property_UID = property_UID;
            builder.ExpectedData.guest.Property_UID = property_UID;

            // Map to Existing Guest
            builder.InputData.guest.UID = guestsList.First().UID;
            builder.ExpectedData.guest.UID = guestsList.First().UID;


            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = channel_UID;
            builder.ExpectedData.reservationDetail.Channel_UID = channel_UID;

            builder.InputData.reservationDetail.ModifyDate = DateTime.Now.Date;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;

            builder.ExpectedData.reservationRooms.FirstOrDefault().ReservationRoomNo = "1";
            builder.ExpectedData.reservationRooms.ForEach(x =>
            {
                x.IsCancellationAllowed = false;
                x.CommissionType = 1;
            });

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion


            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            ReservationAssert.AssertAllReservationObjects(builder.ExpectedData, databaseData);
        }

        /// <summary>
        /// it should update the reservation with partial payments
        /// </summary>
        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_UpdateAllFields_WithPartialPayments()
        {
            #region ARRANGE
            long reservation_UID = 66242;
            long property_UID = 1635;
            long channel_UID = 32;

            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;
            BEPartialPaymentCCMethod partialPayment;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            partialPayment = new BEPartialPaymentCCMethod
            {
                Parcel = 8,
                InterestRate = 0,
                Property_UID = property_UID,
                PaymentMethods_UID = 1
            };

            partialPaymentCCMethodList.Add(partialPayment);


            builder
                .WithNewGuest()
                .WithPartialPayment(partialPayment, 10)
                //.WithRoom(1, 1)
                .WithCreditCardPayment();

            // Change the Reservation UID
            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.ExpectedData.reservationDetail.UID = reservation_UID;

            // Change the Reservation Property_UID
            builder.InputData.reservationDetail.Property_UID = property_UID;
            builder.ExpectedData.reservationDetail.Property_UID = property_UID;

            // Change Reservation Number
            builder.InputData.reservationDetail.Number = "Orbitz-7G7XPR";
            builder.ExpectedData.reservationDetail.Number = "Orbitz-7G7XPR";

            // Change Guest Property Uid
            builder.InputData.guest.Property_UID = property_UID;
            builder.ExpectedData.guest.Property_UID = property_UID;

            // Map to Existing Guest
            var guestUID = guestsList.First().UID;

            builder.InputData.guest.UID = guestUID;
            builder.ExpectedData.guest.UID = guestUID;


            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = channel_UID;
            builder.ExpectedData.reservationDetail.Channel_UID = channel_UID;

            builder.InputData.reservationDetail.ModifyDate = DateTime.Now.Date;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;

            builder.ExpectedData.reservationRooms.First().ReservationRoomNo = "1";
            builder.ExpectedData.reservationRooms.ForEach(x =>
            {
                x.IsCancellationAllowed = false;
                x.CommissionType = 1;
            });

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);

            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            ReservationAssert.AssertAllReservationObjects(builder.ExpectedData, databaseData);
        }

        /// <summary>
        /// it should update the existing guest matched with username and not the id
        /// </summary>
        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_UpdateAllFields_WithExistingGuest_WithUsername()
        {
            #region ARRANGE

            long reservation_UID = 66242;
            long property_UID = 1635;
            long channel_UID = 32;

            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            builder
                .WithNewGuest()
                //.WithRoom(1, 1)
                .WithCreditCardPayment();

            // Change the Reservation UID
            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.ExpectedData.reservationDetail.UID = reservation_UID;

            // Change the Reservation Property_UID
            builder.InputData.reservationDetail.Property_UID = property_UID;
            builder.ExpectedData.reservationDetail.Property_UID = property_UID;

            // Change Reservation Number
            builder.InputData.reservationDetail.Number = "Orbitz-7G7XPR";
            builder.ExpectedData.reservationDetail.Number = "Orbitz-7G7XPR";

            // Change Guest Property Uid
            builder.InputData.guest.Property_UID = property_UID;
            builder.ExpectedData.guest.Property_UID = property_UID;

            // Map to Existing Guest (284528)

            var guest = guestsList.First(q => q.UID == builder.InputData.reservationDetail.Guest_UID);
            guest.Email = "tony.santos@visualforma.pt";

            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = channel_UID;
            builder.ExpectedData.reservationDetail.Channel_UID = channel_UID;

            builder.InputData.reservationDetail.ModifyDate = DateTime.Now.Date;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;

            builder.ExpectedData.reservationRooms.FirstOrDefault().ReservationRoomNo = "1";
            builder.ExpectedData.reservationRooms.ForEach(x =>
            {
                x.IsCancellationAllowed = false;
                x.CommissionType = 1;
            });

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);

            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            ReservationAssert.AssertAllReservationObjects(builder.ExpectedData, databaseData);
        }

        /// <summary>
        /// It inserts a new reservation
        /// don't understand the point of this test, if channel 32 gives the error mentioned in Falling Cause
        /// </summary>
        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_WithExistingReservation_DifferentChannel()
        {
            #region ARRANGE

            long reservation_UID = 66242;
            long property_UID = 1635;
            long channel_UID = 80;

            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            builder
                .WithNewGuest()
                //.WithRoom(1, 1)
                .WithCreditCardPayment();

            // Change the Reservation UID
            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.ExpectedData.reservationDetail.UID = reservation_UID;

            // Change the Reservation Property_UID
            builder.InputData.reservationDetail.Property_UID = property_UID;
            builder.ExpectedData.reservationDetail.Property_UID = property_UID;

            // Change Reservation Number
            builder.InputData.reservationDetail.Number = "Booking-7G7XPR";
            builder.ExpectedData.reservationDetail.Number = "Booking-7G7XPR";

            builder.ExpectedData.reservationDetail.InternalNotesHistory = null;

            // Change Guest Property Uid
            builder.InputData.guest.Property_UID = property_UID;
            builder.ExpectedData.guest.Property_UID = property_UID;

            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = channel_UID;
            builder.ExpectedData.reservationDetail.Channel_UID = channel_UID;

            builder.InputData.reservationDetail.ModifyDate = DateTime.Now.Date;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;

            builder.ExpectedData.reservationRooms.FirstOrDefault().ReservationRoomNo = "1";
            builder.ExpectedData.reservationRooms.ForEach(x =>
            {
                x.IsCancellationAllowed = false;
                x.CommissionType = 1;
            });

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            ReservationAssert.AssertAllReservationObjects(builder.ExpectedData, databaseData);
        }

        /// <summary>
        /// It inserts a new reservation
        /// [FAILING CAUSE] is failing because PMSReservationNumber is being removed from repository reservation object
        /// </summary>
        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_WithExistingReservation_FromPMS()
        {
            #region ARRANGE

            long reservation_UID = 66242;
            long property_UID = 1635;
            long channel_UID = 32;

            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            builder
                .WithNewGuest()
                //.WithRoom(1, 1)
                .WithCreditCardPayment();

            reservationsList.First(x => x.UID == reservation_UID).PmsRservationNumber = "lalala";

            // Change the Reservation UID
            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.ExpectedData.reservationDetail.UID = reservation_UID;

            // Change the Reservation Property_UID
            builder.InputData.reservationDetail.Property_UID = property_UID;
            builder.ExpectedData.reservationDetail.Property_UID = property_UID;

            // Change Reservation Number
            builder.InputData.reservationDetail.Number = "Booking-7G7XPR";
            builder.ExpectedData.reservationDetail.Number = "Booking-7G7XPR";

            builder.ExpectedData.reservationDetail.InternalNotesHistory = null;

            builder.InputData.reservationDetail.PmsRservationNumber = "lalala";
            builder.ExpectedData.reservationDetail.PmsRservationNumber = "lalala";

            // Change Guest Property Uid
            builder.InputData.guest.Property_UID = property_UID;
            builder.ExpectedData.guest.Property_UID = property_UID;

            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = channel_UID;
            builder.ExpectedData.reservationDetail.Channel_UID = channel_UID;

            builder.InputData.reservationDetail.ModifyDate = DateTime.Now.Date;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;

            builder.ExpectedData.reservationRooms.FirstOrDefault().ReservationRoomNo = "1";
            builder.ExpectedData.reservationRooms.ForEach(x =>
            {
                x.IsCancellationAllowed = false;
                x.CommissionType = 1;
                x.PmsRservationNumber = "lalala";
            });

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            ReservationAssert.AssertAllReservationObjects(builder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_TestIsPaid_ReservationFilterAlsoPaid()
        {
            //Arrange
            long reservation_UID = 66242;
            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            int totalRecords = -1;
            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            var request = new Reservation.BL.Contracts.Requests.UpdateReservationIsPaidRequest
            {
                ReservationId = 66242
            };

            _reservationsRepoMock.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>> expr) =>
                {
                    return reservationsList.AsQueryable().Where(expr);
                });
            _reservationsFilterRepoMock.Setup(x => x.FindReservationFilterByCriteria(It.IsAny<ListReservationFilterCriteria>(), out totalRecords, false)).Returns(reservationFilterList.Where(x => x.UID == 66242));

            _channelPropertiesRepoMock.Setup(x => x.ListChannel(It.IsAny<ListChannelRequest>()))
                .Returns((ListChannelRequest req) =>
                {
                    if (req.ChannelUIDs != null && req.ChannelUIDs.Any())
                        return channelsList.Where(y => req.ChannelUIDs.Contains(y.UID)).ToList();

                    return null;
                });
            //Act
            var result = ReservationManagerPOCO.UpdateReservationIsPaid(request);

            //Assert
            Assert.IsTrue((bool)reservationsList.FirstOrDefault(x => x.UID == 66242).IsPaid);
            Assert.IsTrue((bool)reservationFilterList.FirstOrDefault(x => x.UID == 66242).IsPaid);

        }

        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_TestIsPaidBulk_ReservationFilterAlsoPaid()
        {
            //Arrange
            long reservation_UID = 66242;
            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            int totalRecords = -1;
            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            var request = new Reservation.BL.Contracts.Requests.UpdateReservationIsPaidBulkRequest
            {
                ReservationNumbers = new List<string> { "635290249597293545" }
            };

            _reservationsRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationCriteria>()))
                .Returns(reservationsList.Where(x => x.UID == 66242));

            _reservationsRepoMock.Setup(x => x.GetQuery(It.IsAny<System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<domainReservation.Reservation, bool>> expr) =>
                {
                    return reservationsList.AsQueryable().Where(expr);
                });

            _reservationsFilterRepoMock.Setup(x => x.FindReservationFilterByCriteria(It.IsAny<ListReservationFilterCriteria>(), out totalRecords, false)).Returns(reservationFilterList.Where(x => x.UID == 66242));

            _reservationsFilterRepoMock.Setup(x => x.FindByCriteria(It.IsAny<ListReservationFilterCriteria>(), out totalRecords, false)).Returns(reservationFilterList.Where(x => x.UID == 66242).Select(x => x.UID).ToList());

            _channelPropertiesRepoMock.Setup(x => x.ListChannel(It.IsAny<ListChannelRequest>()))
                .Returns((ListChannelRequest req) =>
                {
                    if (req.ChannelIds != null && req.ChannelIds.Any())
                        return channelsList.Where(y => req.ChannelIds.Contains(y.UID)).ToList();

                    return null;
                });

            _paymentMethodTypeRepoMock.Setup(x =>
                    x.ListPaymentMethodTypes(It.IsAny<Contracts.Requests.ListPaymentMethodTypesRequest>()))
                .Returns((ListPaymentMethodTypesRequest req) =>
                {
                    if (!req.UIDs.Any())
                        return null;

                    return paymentTypesList.Where(y => req.UIDs.Contains(y.UID)).ToList();
                });
            //Act
            var result = ReservationManagerPOCO.UpdateReservationIsPaidBulk(request);

            //Assert
            Assert.IsTrue((bool)reservationsList.FirstOrDefault(x => x.UID == 66242).IsPaid);
            Assert.IsTrue((bool)reservationFilterList.FirstOrDefault(x => x.UID == 66242).IsPaid);

        }

        /// <summary>
        /// It accepts the reservation but creates a new reservation with a new reservation number
        /// [FAILING CAUSE] is failing as GetReservationFromDatabase is not returning extras
        /// </summary>
        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_WithExistingReservation_BookingEngine()
        {
            #region ARRANGE

            long reservation_UID = 66242;
            long property_UID = 1635;
            long channel_UID = 32;

            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            builder
                .WithNewGuest()
                //.WithRoom(1, 1)
                .WithCreditCardPayment();

            // Change the Reservation UID
            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.ExpectedData.reservationDetail.UID = reservation_UID;

            // Change the Reservation Property_UID
            builder.InputData.reservationDetail.Property_UID = property_UID;
            builder.ExpectedData.reservationDetail.Property_UID = property_UID;

            // Change Reservation Number
            builder.InputData.reservationDetail.Number = "RES000322-" + property_UID;
            builder.ExpectedData.reservationDetail.Number = "RES000322-" + property_UID;

            // Change Guest Property Uid
            builder.InputData.guest.Property_UID = property_UID;
            builder.ExpectedData.guest.Property_UID = property_UID;

            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = channel_UID;
            builder.ExpectedData.reservationDetail.Channel_UID = channel_UID;

            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;

            builder.ExpectedData.reservationRooms.FirstOrDefault().ReservationRoomNo = "RES000322-" + property_UID + "/1";
            builder.ExpectedData.reservationRooms.ForEach(x =>
            {
                x.IsCancellationAllowed = false;
                x.CommissionType = 1;
            });

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            builder.ExpectedData.reservationDetail.ModifyDate = databaseData.reservationDetail.ModifyDate;

            ReservationAssert.AssertAllReservationObjects(builder.ExpectedData, databaseData);
        }


        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_WithChannelReservationRoomId()
        {
            #region ARRANGE
            FillReservationAdditionalData();

            long reservation_UID = 66242;
            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            _resAddDataRepoMock.Setup(x => x.GetQuery())
                .Returns(reservationAddicionaldataList.AsQueryable());
            _reservationHelperMock.Setup(x => x.GetReservationAdditionalData((int)reservation_UID)).Returns(new ReservationsAdditionalData());

            builder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {

                ReservationRoomList = new List<contractsReservations.ReservationRoomAdditionalData>()
               {
                   new contractsReservations.ReservationRoomAdditionalData()
                   {
                     ChannelReservationRoomId = "TESTE179595"
                   }
               },
            };

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            builder.ExpectedData.reservationDetail.ModifyDate = databaseData.reservationDetail.ModifyDate;

            contractsReservations.ReservationsAdditionalData addiData = Newtonsoft.Json.JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(databaseData.reservationsAdditionalData.ReservationAdditionalDataJSON);
            IEnumerable<string> resDbAdditionalData = addiData.ReservationRoomList.Where(x => x.ReservationRoom_UID == 179595).Select(y => y.ChannelReservationRoomId);
            var builderAddData = builder.InputData.reservationsAdditionalData.ReservationRoomList.First().ChannelReservationRoomId;
            Assert.AreEqual(builderAddData, resDbAdditionalData.First());
        }


        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_WithOutAdditionalData()
        {
            #region ARRANGE
            FillReservationAdditionalData();
            FillRoomTypesMock();

            long reservation_UID = 66242;
            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible

            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            _resAddDataRepoMock.Setup(x => x.GetQuery())
                .Returns(reservationAddicionaldataList.AsQueryable());

            _reservationHelperMock.Setup(x => x.GetReservationAdditionalData((int)reservation_UID)).Returns(reservationAddicionaldataList.Single(x => x.Reservation_UID == reservation_UID));


            builder.InputData.reservationRooms = new List<ReservationRoom>
            {
               new ReservationRoom()
               {
                   RoomType_UID = 3709,
                   DepositPolicy = "DeposityTEST"
               },

            };

            builder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {

                ReservationRoomList = new List<contractsReservations.ReservationRoomAdditionalData>()
               {
                   new contractsReservations.ReservationRoomAdditionalData()
                   {
                     ReservationRoom_UID = 1,

                   }
               },
            };

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            contractsReservations.ReservationsAdditionalData domainAddiData = Newtonsoft.Json.JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(databaseData.reservationsAdditionalData.ReservationAdditionalDataJSON);

            Assert.AreEqual(databaseData.reservationRooms.Select(x => x.UID).First(), domainAddiData.ReservationRoomList.Select(x => x.ReservationRoom_UID).First());
            Assert.AreEqual("DeposityTEST", domainAddiData.ReservationRoomList.Select(x => x.DepositPolicy).First());


        }

        [TestMethod]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_WithAdditionalData()
        {
            #region ARRANGE
            FillReservationAdditionalData();
            FillRoomTypesMock();

            long reservation_UID = 66242;
            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible

            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            _resAddDataRepoMock.Setup(x => x.GetQuery())
                .Returns(reservationAddicionaldataList.AsQueryable());

            _reservationHelperMock.Setup(x => x.GetReservationAdditionalData((int)reservation_UID)).Returns(reservationAddicionaldataList.Single(x => x.Reservation_UID == reservation_UID));


            builder.InputData.reservationRooms = new List<ReservationRoom>
            {
               new ReservationRoom()
               {
                   RoomType_UID = 3709,
               },

            };

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            contractsReservations.ReservationsAdditionalData domainAddiData = Newtonsoft.Json.JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(databaseData.reservationsAdditionalData.ReservationAdditionalDataJSON);

            Assert.AreEqual(databaseData.reservationRooms.Select(x => x.UID).First(), domainAddiData.ReservationRoomList.Select(x => x.ReservationRoom_UID).First());


        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow(false)]
        [DataRow(true)]
        [TestCategory("UpdateReservation")]
        public void TestUpdateReservation_WithAdditionalDataBookerIsGenius(bool? bookerIsGenius)
        {
            #region ARRANGE
            FillReservationAdditionalData();
            FillRoomTypesMock();

            long reservation_UID = 66242;
            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;

            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible

            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            _resAddDataRepoMock.Setup(x => x.GetQuery())
                .Returns(reservationAddicionaldataList.AsQueryable());

            _reservationHelperMock.Setup(x => x.GetReservationAdditionalData((int)reservation_UID)).Returns(reservationAddicionaldataList.Single(x => x.Reservation_UID == reservation_UID));


            builder.InputData.reservationRooms = new List<ReservationRoom>
            {
               new ReservationRoom()
               {
                   RoomType_UID = 3709,
               },

            };

            builder.InputData.reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData
            {
                BookerIsGenius = bookerIsGenius,
            };

            MockMultipleBasedOnBuilders(searchBuilder, builder, transactionId, inventories: null, addTranlations: false);
            #endregion

            // ACT
            long result = this.UpdateReservation(builder);

            // ASSERT
            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);
            Assert.IsNotNull(databaseData);
            Assert.IsNotNull(databaseData.reservationsAdditionalData);
            Assert.IsNotNull(databaseData.reservationsAdditionalData.ReservationAdditionalDataJSON);

            contractsReservations.ReservationsAdditionalData domainAddiData = Newtonsoft.Json.JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(databaseData.reservationsAdditionalData.ReservationAdditionalDataJSON);
            Assert.AreEqual(bookerIsGenius, domainAddiData.BookerIsGenius);
        }


        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservation_WithPaymentGatewayTEST_BrasPag_Authorize()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            var tmpTpi = tpi.Clone();
            tmpTpi.Property_UID = 1263;
            tmpTpi.IsCompany = true;
            tpisList.Remove(tpi);
            tpisList.Add(tmpTpi);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithTPI(tpi);
            var transactionId = Guid.NewGuid().ToString();
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

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservation_WithPaymentGatewayTEST_BrasPag_WithCaptureDelay()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            var tmpTpi = tpi.Clone();
            tmpTpi.Property_UID = 1263;
            tmpTpi.IsCompany = true;
            tpisList.Remove(tpi);
            tpisList.Add(tmpTpi);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithTPI(tpi);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock
                .Setup(x => x.GetActivePaymentGatewayConfiguration(
                    It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration { UID = 1, GatewayCode = "300", CountryIsoCode = "132131", PaymentAuthorizationTypeId = 2 });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservation_WithPaymentGatewayTEST_BrasPag_WithAuthorizeAndThenCapture()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            var tmpTpi = tpi.Clone();
            tmpTpi.Property_UID = 1263;
            tmpTpi.IsCompany = true;
            tpisList.Remove(tpi);
            tpisList.Add(tmpTpi);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithTPI(tpi);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;
            resBuilder.InputData.reservationDetail.PaymentGatewayTransactionID = "38d06f69-e555-4cb3-8f27-34d2938d1f99";

            _iOBPPropertyRepoMock
                .Setup(x => x.GetActivePaymentGatewayConfiguration(
                    It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration { UID = 1, GatewayCode = "300", CountryIsoCode = "132131" });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
            _brasPag.Setup(x => x.Capture(It.IsAny<CaptureRequest>())).Returns(paymentResult);
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestInsertReservation_WithPaymentGateway_OnRequest()
        {
            long errorCode = 0;

            // arrange
            //Prepare the tpi
            OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi = null;
            tpi = tpisList.Where(x => x.UID == 61).SingleOrDefault();
            var tmpTpi = tpi.Clone();
            tmpTpi.Property_UID = 1263;
            tmpTpi.IsCompany = true;
            tpisList.Remove(tpi);
            tpisList.Add(tmpTpi);

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true).WithNewGuest()
                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false).WithCreditCardPayment().WithTPI(tpi);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock
                .Setup(x => x.GetActivePaymentGatewayConfiguration(
                    It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration { UID = 1, GatewayCode = "300", CountryIsoCode = "132131", PaymentAuthorizationTypeId = 2 });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

            //mock OnRequest in the gateway response message
            paymentResult.IsOnReview = true;

            //Mock response
            _brasPag.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //Check if the reservation is inserted on ConnectorEventQueue
            _obRatesRepoMock.Verify(x => x.ConnectorEventQueueInsert(It.IsAny<ConnectorEventQueueInsertRequest>()), Times.Never());

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";
            resBuilder.ExpectedData.reservationDetail.IsOnRequest = true;
            resBuilder.ExpectedData.reservationDetail.Status = 5;

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            Assert.AreEqual(5, databaseData.reservationDetail.Status, "Expected status OnRequest");
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        public void TestCancelReservation_WithPaymentGateway_WithVoid()
        {
            // arrange
            long propertyUID = 1635;
            long reservation_UID = 66242;

            #region supposed initializer

            ReservationDataBuilder resBuilder = null;
            SearchBuilder searchBuilder = null;
            var transactionId = Guid.NewGuid().ToString();

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out searchBuilder, out resBuilder, (int)reservation_UID);

            MockMultipleBasedOnBuilders(searchBuilder, resBuilder, transactionId, inventories: null, addTranlations: false);

            #endregion

            var res = reservationsList.Single(x => x.UID == reservation_UID);
            foreach (ReservationRoom rr in res.ReservationRooms)
            {
                rr.IsCancellationAllowed = true;
                rr.CancellationValue = null;
                rr.CancellationPolicyDays = null;
                rr.CancellationCosts = true;
                rr.CancellationNrNights = null;
            }

            // act

            var request = new OB.Reservation.BL.Contracts.Requests.CancelReservationRequest
            {
                PropertyUID = propertyUID,
                ReservationNo = res.Number,
                ReservationRoomNo = res.ReservationRooms.First().ReservationRoomNo,
                UserType = (int)Constants.BookingEngineUserType.Guest,
                CancelReservationReason_UID = null,
                CancelReservationComments = string.Empty
            };

            res.PaymentGatewayName = "BrasPag";
            res.PaymentGatewayTransactionID = "38d06f69-e555-4cb3-8f27-34d2938d1f99";

            _iOBPPropertyRepoMock
                .Setup(x => x.GetActivePaymentGatewayConfigurationReduced(
                    It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration { UID = 1, GatewayCode = "300", CountryIsoCode = "132131" });

            _brasPag.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(paymentResult);
            _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(_brasPag.Object);

            var result = ReservationManagerPOCO.CancelReservation(request);

            ReservationData databaseData = GetReservationDataFromDatabase(reservation_UID);

            //// ASSERT
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(4, databaseData.reservationDetail.Status);
            Assert.AreEqual(null, databaseData.reservationDetail.CancelReservationReason_UID);
            Assert.AreEqual(string.Empty, databaseData.reservationDetail.CancelReservationComments);

            Assert.AreEqual(2, databaseData.reservationRooms[0].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[0].IsCancellationAllowed);

            Assert.AreEqual(1, databaseData.reservationRooms[1].Status);
            Assert.AreEqual(true, databaseData.reservationRooms[1].IsCancellationAllowed);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_InterestsRate(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

            _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<ListCityDataRequest>())).Returns(new List<OB.BL.Contracts.Data.General.CityData>{
                        new OB.BL.Contracts.Data.General.CityData
                        {
                            UID = 1,
                            name = "medellin",
                            stateName = "medellin"
                        }
                    });


            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value * (1 + BEPPCCM.InterestRate.Value / 100);

            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Authorize(It.IsAny<string>(),
                                                    It.Is<decimal>(value => value == reservationValue),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Verify(m => m.Authorize(It.Is<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(request => request.TotalAmount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Authorize(It.Is<AuthorizeRequest>(request => request.TotalValue == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(m => m.Authorize(It.IsAny<string>(), It.Is<decimal>(totalValue => totalValue == reservationValue), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Charge(It.Is<payUContracts.ChargeRequest>(request => request.CreatePayment.Amount == reservationValue)));
                    break;
            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [ExpectedException(typeof(PaymentGatewayException))]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_InterestsRate_WithMinimumPartialPaymentNotMet(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null && r.PartialPaymentMinimumValue > 999);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;

            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_InterestsRate_WithMinimumPriceNotSet(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null && r.PartialPaymentMinimumValue == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

            _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });

            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value * (1 + BEPPCCM.InterestRate.Value / 100);
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Authorize(It.IsAny<string>(),
                                                    It.Is<decimal>(value => value == reservationValue),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Verify(m => m.Authorize(It.Is<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(request => request.TotalAmount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Authorize(It.Is<AuthorizeRequest>(request => request.TotalValue == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(m => m.Authorize(It.IsAny<string>(), It.Is<decimal>(totalValue => totalValue == reservationValue), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Charge(It.Is<payUContracts.ChargeRequest>(request => request.CreatePayment.Amount == reservationValue)));
                    break;
            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [ExpectedException(typeof(PaymentGatewayException))]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_InterestsRate_WithOBAPINotWorking(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null && r.PartialPaymentMinimumValue == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }


            _OBBePartialPaymentCcMethodRepository.Setup(m => m.ListBePartialPaymentCcMethods(It.IsAny<ListBePartialPaymentCcMethodRequest>()))
                                                   .Returns(
                                                       new ListBePartialPaymentCcMethodResponse
                                                       {
                                                           Status = Status.Fail
                                                       });

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [ExpectedException(typeof(PaymentGatewayException))]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_InterestsRate_InterestRateIsNull(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;
            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null && r.InterestRate == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_SkipInterestsRateCalculation(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);

                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;

            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, skipInterestCalculation: true);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value;
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Authorize(It.IsAny<string>(),
                                                    It.Is<decimal>(value => value == reservationValue),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Verify(m => m.Authorize(It.Is<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(request => request.TotalAmount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Authorize(It.Is<AuthorizeRequest>(request => request.TotalValue == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(m => m.Authorize(It.IsAny<string>(), It.Is<decimal>(totalValue => totalValue == reservationValue), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Charge(It.Is<payUContracts.ChargeRequest>(request => request.CreatePayment.Amount == reservationValue)));
                    break;
            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [ExpectedException(typeof(PaymentGatewayException))]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_ReservationInterestRateDifferentFromConfigured(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate.Value + (decimal)0.5,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });

            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, skipInterestCalculation: false);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value;
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Authorize(It.IsAny<string>(),
                                                    It.Is<decimal>(value => value == reservationValue),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Verify(m => m.Authorize(It.Is<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(request => request.TotalAmount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Authorize(It.Is<AuthorizeRequest>(request => request.TotalValue == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(m => m.Authorize(It.IsAny<string>(), It.Is<decimal>(totalValue => totalValue == reservationValue), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Charge(It.Is<payUContracts.ChargeRequest>(request => request.CreatePayment.Amount == reservationValue)));
                    break;
            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [ExpectedException(typeof(PaymentGatewayException))]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_NoParcelationSetInContract(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate.Value,
                Parcel = first.Parcel + 1,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });


            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;

            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, skipInterestCalculation: false);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value;
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Authorize(It.IsAny<string>(),
                                                    It.Is<decimal>(value => value == reservationValue),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Verify(m => m.Authorize(It.Is<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(request => request.TotalAmount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Authorize(It.Is<AuthorizeRequest>(request => request.TotalValue == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(m => m.Authorize(It.IsAny<string>(), It.Is<decimal>(totalValue => totalValue == reservationValue), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Charge(It.Is<payUContracts.ChargeRequest>(request => request.CreatePayment.Amount == reservationValue)));
                    break;

            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [ExpectedException(typeof(PaymentGatewayException))]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_ReservationParcelsDifferentFromConfigured(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate.Value,
                Parcel = first.Parcel + 1,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });
            _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });

            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, skipInterestCalculation: false);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = reservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            // assert
            ReservationData databaseData = GetReservationDataFromDatabase(reservation.UID);
            ReservationAssert.AssertAllReservationObjects(resBuilder.ExpectedData, databaseData);

            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value;
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Authorize(It.IsAny<string>(),
                                                    It.Is<decimal>(value => value == reservationValue),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Verify(m => m.Authorize(It.Is<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(request => request.TotalAmount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Authorize(It.Is<AuthorizeRequest>(request => request.TotalValue == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(m => m.Authorize(It.IsAny<string>(), It.Is<decimal>(totalValue => totalValue == reservationValue), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Charge(It.Is<payUContracts.ChargeRequest>(request => request.CreatePayment.Amount == reservationValue)));
                    break;
            }
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [ExpectedException(typeof(PaymentGatewayException))]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_MinimumPriceNotMet(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate.Value,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });


            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;

            }

            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value;
            _OBBeSettingsRepository.Setup(m => m.ListBeSettings(It.IsAny<ListBeSettingsRequest>()))
                                                   .Returns((ListBeSettingsRequest request) =>
                                                   {
                                                       var result = new List<Contracts.Data.BE.BESettings>
                                                        {
                                                            new Contracts.Data.BE.BESettings
                                                            {
                                                                PartialPaymentMinimumAllowed = (long)(reservationValue + 1),
                                                                AllowPartialPayment = true,
                                                            }
                                                        };

                                                       return new ListBeSettingsResponse
                                                       {
                                                           Status = Status.Success,
                                                           Errors = new List<Error>(),
                                                           Warnings = new List<Warning>(),
                                                           RequestGuid = request.RequestGuid,
                                                           RequestId = request.RequestId,
                                                           Result = result,
                                                           TotalRecords = result.Count
                                                       };
                                                   });

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, skipInterestCalculation: false);
        }

        [TestMethod]
        [TestCategory("PaymentGateway")]
        [ExpectedException(typeof(PaymentGatewayException))]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertReservation_WithPaymentGateway_With_PropertyNotAllowingPartialPayment(Constants.PaymentGateway gateway)
        {
            long errorCode = 0;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate.Value,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = "RES000024-1263";
            var builder = new Operations.Helper.SearchBuilder(Container, 1263);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            var transactionId = Guid.NewGuid().ToString();
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });


            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);

                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
            }

            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value;
            _OBBeSettingsRepository.Setup(m => m.ListBeSettings(It.IsAny<ListBeSettingsRequest>()))
                                                   .Returns((ListBeSettingsRequest request) =>
                                                   {
                                                       var result = new List<Contracts.Data.BE.BESettings>
                                                        {
                                                            new Contracts.Data.BE.BESettings
                                                            {
                                                                PartialPaymentMinimumAllowed = 0,
                                                                AllowPartialPayment = false,
                                                            }
                                                        };

                                                       return new ListBeSettingsResponse
                                                       {
                                                           Status = Status.Success,
                                                           Errors = new List<Error>(),
                                                           Warnings = new List<Warning>(),
                                                           RequestGuid = request.RequestGuid,
                                                           RequestId = request.RequestId,
                                                           Result = result,
                                                           TotalRecords = result.Count
                                                       };
                                                   });

            //Do the reservation
            var reservation = InsertReservation(out errorCode, builder, resBuilder, transactionId, propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected, skipInterestCalculation: false);
        }

        [TestMethod]
        [TestCategory("UpdateReservation")]
        [DataTestMethod]
        //[DataRow(Constants.PaymentGateway.Adyen)]
        //[DataRow(Constants.PaymentGateway.BPag)]
        //[DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestUpdateReservation_WithPaymentGateway_With_InterestsRate(Constants.PaymentGateway gateway)
        {
            #region ARRANGE

            long reservation_UID = 67242;
            long property_UID = 1635;
            long channel_UID = 32;

            //Get the builders to make the mock possible
            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            var reservation = reservationsList.Single(r => r.UID == reservation_UID);
            reservation.PaymentGatewayTransactionStatusCode = "1";
            reservation.PaymentGatewayName = gateway.ToString();
            foreach (var details in reservation.ReservationPaymentDetails)
                details.PaymentGatewayTokenizationIsActive = true;

            foreach (ReservationRoom resroom in reservation.ReservationRooms)
            {
                resroom.CancellationPolicyDays = 0;
                resroom.IsCancellationAllowed = true;
                resroom.CancellationValue = null;
                resroom.CancellationPolicyDays = null;
                resroom.CancellationCosts = true;
                resroom.CancellationNrNights = null;
            }

            //Mock Parcelation
            var first = bePartialPaymentCcMethodList.First(r => r.PropertyId == property_UID);
            reservation.InterestRate = first.InterestRate;
            reservation.IsPartialPayment = true;
            reservation.ReservationPartialPaymentDetails.ForEach(r =>
            {
                r.InterestRate = first.InterestRate;
                r.InstallmentNo = first.Parcel;
            });
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            builder
                .WithNewGuest()
                .WithCreditCardPayment()
                .WithCreditCardPayment()
                .WithPartialPayment(BEPPCCM, 20);

            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.InputData.reservationDetail.Property_UID = property_UID;
            builder.InputData.reservationDetail.Number = "RES000322-" + property_UID;
            builder.InputData.guest.Property_UID = property_UID;

            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = channel_UID;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;
            MockMultipleBasedOnBuilders(searchBuilder, builder, Guid.NewGuid().ToString(), inventories: null, addTranlations: false);
            #endregion

            // ACT
            var request = UpdateReservationRequest(builder);
            request.Reservation.PaymentGatewayTransactionID = "teste";
            request.Reservation.PaymentGatewayName = gateway.ToString();
            request.ReservationPaymentDetail = new contractsReservations.ReservationPaymentDetail
            {
                PaymentGatewayTokenizationIsActive = true,
            };

            //Mock PaymentGateway's
            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>> pgtMock = new Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>>();
                    RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Payments.PaymentGatewayTransaction>(It.IsAny<IUnitOfWork>())).
                        Returns(pgtMock.Object);

                    adyenMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    pgtMock.Setup(x => x.Any(It.IsAny<Expression<Func<OB.Domain.Payments.PaymentGatewayTransaction, bool>>>()))
                        .Returns(false);

                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true
                    });

                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral> pgMock = new Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral>();
                    pgMock.Setup(x => x.IsTheSameDay(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(false);


                    maxiPagoMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Refund(It.IsAny<payUContracts.RefundRequest>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true
                    });
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }

            UpdateReservation(request);

            decimal reservationValue = reservation.TotalAmount.Value * (1 + BEPPCCM.InterestRate.Value / 100);
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Void(It.Is<VoidRequest>(pgReq => pgReq.Amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Refund(It.Is<payUContracts.RefundRequest>(requestR => requestR.Amount == reservationValue)));
                    break;

            }
        }

        [TestMethod]
        [TestCategory("UpdateReservation")]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestUpdateReservation_WithPaymentGateway_With_SkipInterestRateCalculation(Constants.PaymentGateway gateway)
        {
            #region ARRANGE

            long reservation_UID = 67242;
            long property_UID = 1635;
            long channel_UID = 32;

            //Get the builders to make the mock possible
            ReservationDataBuilder builder = null;
            SearchBuilder searchBuilder = null;
            GetBuildersUsingReservation(out searchBuilder, out builder, (int)reservation_UID);

            var reservation = reservationsList.Single(r => r.UID == reservation_UID);
            reservation.PaymentGatewayTransactionStatusCode = "1";
            reservation.PaymentGatewayName = gateway.ToString();
            foreach (var details in reservation.ReservationPaymentDetails)
                details.PaymentGatewayTokenizationIsActive = true;

            foreach (ReservationRoom resroom in reservation.ReservationRooms)
            {
                resroom.CancellationPolicyDays = 0;
                resroom.IsCancellationAllowed = true;
                resroom.CancellationValue = null;
                resroom.CancellationPolicyDays = null;
                resroom.CancellationCosts = true;
                resroom.CancellationNrNights = null;
            }

            //Mock Parcelation
            var first = bePartialPaymentCcMethodList.First(r => r.PropertyId == 1635);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            builder
                .WithNewGuest()
                .WithCreditCardPayment()
                .WithCreditCardPayment()
                .WithPartialPayment(BEPPCCM, 20);

            builder.InputData.reservationDetail.UID = reservation_UID;
            builder.InputData.reservationDetail.Property_UID = property_UID;
            builder.InputData.reservationDetail.Number = "RES000322-" + property_UID;
            builder.InputData.guest.Property_UID = property_UID;

            // Change Reservation Channel
            builder.InputData.reservationDetail.Channel_UID = channel_UID;
            builder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date;
            MockMultipleBasedOnBuilders(searchBuilder, builder, Guid.NewGuid().ToString(), inventories: null, addTranlations: false);
            #endregion

            // ACT
            var request = UpdateReservationRequest(builder);
            request.Reservation.PaymentGatewayTransactionID = "teste";
            request.Reservation.PaymentGatewayName = gateway.ToString();
            request.SkipInterestCalculation = true;
            request.ReservationPaymentDetail = new contractsReservations.ReservationPaymentDetail
            {
                PaymentGatewayTokenizationIsActive = true,
            };

            //Mock PaymentGateway's
            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>> pgtMock = new Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>>();
                    RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Payments.PaymentGatewayTransaction>(It.IsAny<IUnitOfWork>())).
                        Returns(pgtMock.Object);

                    adyenMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    pgtMock.Setup(x => x.Any(It.IsAny<Expression<Func<OB.Domain.Payments.PaymentGatewayTransaction, bool>>>()))
                        .Returns(false);

                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true
                    });

                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral> pgMock = new Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral>();
                    pgMock.Setup(x => x.IsTheSameDay(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(false);


                    maxiPagoMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;

                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;

            }

            UpdateReservation(request);

            decimal reservationValue = reservation.TotalAmount.Value;
            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Void(It.Is<VoidRequest>(pgReq => pgReq.Amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Refund(It.Is<payUContracts.RefundRequest>(requestR => requestR.Amount == reservationValue)));
                    break;
            }
        }

        [TestMethod]
        [TestCategory("UpdateReservation")]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertAndUpdateReservation_WithPaymentGateway_InterestRate(Constants.PaymentGateway gateway)
        {
            #region InsertReservation
            //Mock PaymentGateway's
            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();

            long property_UID = 1263;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = $"RES000024-{property_UID}";
            var builder = new Operations.Helper.SearchBuilder(Container, property_UID);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true, dayOffset: 1)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });


            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
            }

            //Do the reservation
            var contractReservation = InsertReservation(out long errorCode, builder, resBuilder, Guid.NewGuid().ToString(), propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = contractReservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";

            #endregion

            ReservationData databaseData = GetReservationDataFromDatabase(contractReservation.UID);
            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value * (1 + BEPPCCM.InterestRate.Value / 100);

            //Clean Reference Loops
            reservationsList.ForEach(r =>
            {
                foreach (var paymentDetail in r.ReservationPaymentDetails)
                    paymentDetail.Reservation = null;

                foreach (var partialDetail in r.ReservationPartialPaymentDetails)
                    partialDetail.Reservation = null;

                foreach (var room in r.ReservationRooms)
                {
                    room.Reservation = null;
                    foreach (var rrc in room.ReservationRoomChilds)
                        rrc.ReservationRoom = null;

                    foreach (var rrd in room.ReservationRoomDetails)
                        rrd.ReservationRoom = null;

                    foreach (var rre in room.ReservationRoomExtras)
                        rre.ReservationRoom = null;
                }
            });

            #region ModifyReservation
            long channel_UID = 32;

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out SearchBuilder searchBuilder, out ReservationDataBuilder reservationBuilder, (int)contractReservation.UID);

            var domainReservation = reservationsList.Single(r => r.UID == contractReservation.UID);

            domainReservation.PaymentGatewayTransactionStatusCode = "1";
            domainReservation.PaymentGatewayName = gateway.ToString();
            foreach (var details in domainReservation.ReservationPaymentDetails)
                details.PaymentGatewayTokenizationIsActive = true;

            foreach (ReservationRoom resroom in domainReservation.ReservationRooms)
            {
                resroom.CancellationPolicyDays = 0;
                resroom.IsCancellationAllowed = true;
                resroom.CancellationValue = null;
                resroom.CancellationPolicyDays = null;
                resroom.CancellationCosts = true;
                resroom.CancellationNrNights = null;
            }

            reservationBuilder.WithNewGuest().WithCreditCardPayment().WithPartialPayment(BEPPCCM, 20);

            reservationBuilder.InputData.reservationDetail.UID = domainReservation.UID;
            reservationBuilder.InputData.reservationDetail.Property_UID = property_UID;
            reservationBuilder.InputData.reservationDetail.Number = "RES000322-" + property_UID;
            reservationBuilder.InputData.guest.Property_UID = property_UID;

            // Change Reservation Channel
            reservationBuilder.InputData.reservationDetail.Channel_UID = channel_UID;
            reservationBuilder.ExpectedData.reservationDetail.ModifyDate = DateTime.Now.Date - new TimeSpan(1, 0, 0, 0);
            MockMultipleBasedOnBuilders(searchBuilder, reservationBuilder, Guid.NewGuid().ToString(), inventories: null, addTranlations: false);
            domainReservation.ReservationRooms.Clear();
            domainReservation.Date = DateTime.Now.Date + new TimeSpan(3, 0, 0, 0);

            // ACT
            var request = UpdateReservationRequest(reservationBuilder);
            request.Reservation.PaymentGatewayTransactionID = "teste";
            request.Reservation.PaymentGatewayName = gateway.ToString();
            request.ReservationPaymentDetail = new contractsReservations.ReservationPaymentDetail
            {
                PaymentGatewayTokenizationIsActive = true,
            };

            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>> pgtMock = new Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>>();
                    RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Payments.PaymentGatewayTransaction>(It.IsAny<IUnitOfWork>())).
                        Returns(pgtMock.Object);

                    adyenMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    pgtMock.Setup(x => x.Any(It.IsAny<Expression<Func<OB.Domain.Payments.PaymentGatewayTransaction, bool>>>())).Returns(false);

                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns((VoidRequest voidRequest) =>
                    {
                        return new PaymentMessageResult
                        {
                            IsTransactionValid = true
                        };
                    });

                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral> pgMock = new Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral>();
                    pgMock.Setup(x => x.IsTheSameDay(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(false);


                    maxiPagoMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns((string a, string b, decimal ammount) =>
                    {
                        return new PaymentMessageResult
                        {
                            IsTransactionValid = true,
                        };
                    });

                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;

                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Refund(It.IsAny<payUContracts.RefundRequest>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true
                    });
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }

            UpdateReservation(request);
            #endregion

            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Void(It.Is<VoidRequest>(pgReq => pgReq.Amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Charge(It.Is<payUContracts.ChargeRequest>(requestR => requestR.CreatePayment.Amount == reservationValue)));
                    break;
            }
        }


        [TestMethod]
        [TestCategory("UpdateReservation")]
        [DataTestMethod]
        [DataRow(Constants.PaymentGateway.Adyen)]
        [DataRow(Constants.PaymentGateway.BPag)]
        [DataRow(Constants.PaymentGateway.MaxiPago)]
        [DataRow(Constants.PaymentGateway.BrasPag)]
        [DataRow(Constants.PaymentGateway.PayU)]
        public void TestInsertAndUpdateReservation_WithPaymentGateway_InterestRate_ModifiedSettingsInbetween(Constants.PaymentGateway gateway)
        {
            #region InsertReservation
            //Mock PaymentGateway's
            Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen> adyenMock = new Mock<PaymentGatewaysLibrary.AdyenGateway.IAdyen>();
            Mock<PaymentGatewaysLibrary.BPagGateway.IBPag> bPagMock = new Mock<PaymentGatewaysLibrary.BPagGateway.IBPag>();
            Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag> brasPagMock = new Mock<PaymentGatewaysLibrary.BrasPagGateway.IBrasPag>();
            Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago> maxiPagoMock = new Mock<PaymentGatewaysLibrary.MaxiPagoGateway.IMaxiPago>();
            Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia> payUMock = new Mock<PaymentGatewaysLibrary.PayU.Services.Interface.IPayUColombia>();

            long property_UID = 1263;

            var first = bePartialPaymentCcMethodList.First(r => r.PaymentMethodsId != null && r.PaymentMethodTypeId == null);
            BEPartialPaymentCCMethod BEPPCCM = new BEPartialPaymentCCMethod
            {
                InterestRate = first.InterestRate,
                Parcel = first.Parcel,
                PartialPaymentMinimumValue = first.PartialPaymentMinimumValue,
                PaymentMethodTypeId = first.PaymentMethodTypeId,
                PaymentMethods_UID = first.PaymentMethodsId,
                Property_UID = first.PropertyId,
                UID = first.Id
            };

            //Prepare the reservation
            var resNumberExpected = $"RES000024-{property_UID}";
            var builder = new Operations.Helper.SearchBuilder(Container, property_UID);
            var resBuilder = new ReservationDataBuilder(1, 1, resNumber: resNumberExpected, ignoreChangeResNumberIfBE: true, dayOffset: 1)
                                .WithNewGuest()
                                .WithRoom(1, 1, ignoreBEResNumber: true, cancelationAllowed: false)
                                .WithCreditCardPayment()
                                .WithPartialPayment(BEPPCCM, 20);
            resBuilder.InputData.guest.UID = 1;

            _iOBPPropertyRepoMock.Setup(x => x.GetActivePaymentGatewayConfiguration(It.IsAny<ListActivePaymentGatewayConfigurationRequest>()))
                .Returns(new PaymentGatewayConfiguration
                {
                    UID = 1,
                    GatewayCode = ((int)gateway).ToString(),
                    CountryIsoCode = "132131",
                    PaymentAuthorizationTypeId = 2
                });

            _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Setup(x => x.Authorize(It.IsAny<string>(),
                                                    It.IsAny<decimal>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<int?>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<short>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<DateTime>(),
                                                    It.IsAny<DateTime>()))
                        .Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(x => x.Authorize(It.IsAny<PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);

                    //These mocks are not important for the test, but we will have to return data
                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(new List<OB.BL.Contracts.Data.Properties.PropertyLight>{
                        new OB.BL.Contracts.Data.Properties.PropertyLight
                        {
                            Country_UID = 1,
                        }
                    });
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Authorize(It.IsAny<AuthorizeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Charge(It.IsAny<payUContracts.ChargeRequest>())).Returns(paymentResult);
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);

                    _iOBPPropertyRepoMock.Setup(x => x.ListCountries(It.IsAny<ListCountryRequest>())).Returns(new List<OB.BL.Contracts.Data.General.Country>{
                        new OB.BL.Contracts.Data.General.Country
                        {
                            UID = 1,
                            CountryCode = "1",
                        }
                    });

                    _iOBPPropertyRepoMock.Setup(x => x.ListCitiesDataTranslated(It.IsAny<Contracts.Requests.ListCityDataRequest>())).Returns(new List<BL.Contracts.Data.General.CityData>{
                        new BL.Contracts.Data.General.CityData
                        {
                            asciiname = "teste"
                        }
                    });
                    break;
            }

            //Do the reservation
            var contractReservation = InsertReservation(out long errorCode, builder, resBuilder, Guid.NewGuid().ToString(), propUid: 1263,
                withChildsRoom: false, withCancellationPolicies: false, channelUid: 1, resNumber: resNumberExpected);

            //The ModifyDate is set during the insertion process, so, must be equal
            resBuilder.ExpectedData.reservationDetail.ModifyDate = contractReservation.ModifyDate;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionStatusCode = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayName = "BrasPag";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayOrderID = "id:b3315347-5771-4a64-ab0b-54dc89b6cc4e;auxN:;authC:";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayProcessorName = "TEST SIMULATOR";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionID = "b3315347-5771-4a64-ab0b-54dc89b6cc4e";
            resBuilder.ExpectedData.reservationDetail.PaymentAmountCaptured = 0;
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayAutoGeneratedUID = "1";
            resBuilder.ExpectedData.reservationDetail.PaymentGatewayTransactionMessage = "1 - ";
            #endregion

            ReservationData databaseData = GetReservationDataFromDatabase(contractReservation.UID);
            decimal reservationValue = resBuilder.InputData.reservationDetail.TotalAmount.Value * (1 + BEPPCCM.InterestRate.Value / 100);

            //Clean Reference Loops
            reservationsList.ForEach(r =>
            {
                foreach (var paymentDetail in r.ReservationPaymentDetails)
                    paymentDetail.Reservation = null;

                foreach (var partialDetail in r.ReservationPartialPaymentDetails)
                    partialDetail.Reservation = null;

                foreach (var room in r.ReservationRooms)
                {
                    room.Reservation = null;
                    foreach (var rrc in room.ReservationRoomChilds)
                        rrc.ReservationRoom = null;

                    foreach (var rrd in room.ReservationRoomDetails)
                        rrd.ReservationRoom = null;

                    foreach (var rre in room.ReservationRoomExtras)
                        rre.ReservationRoom = null;
                }
            });

            //Modify PartialPaymentSettings
            first.InterestRate += (decimal)0.5;

            #region ModifyReservation
            long channel_UID = 32;

            //Get the builders to make the mock possible
            GetBuildersUsingReservation(out SearchBuilder searchBuilder, out ReservationDataBuilder reservationBuilder, (int)contractReservation.UID);

            var domainReservation = reservationsList.Single(r => r.UID == contractReservation.UID);

            domainReservation.PaymentGatewayTransactionStatusCode = "1";
            domainReservation.PaymentGatewayName = gateway.ToString();
            foreach (var details in domainReservation.ReservationPaymentDetails)
                details.PaymentGatewayTokenizationIsActive = true;

            foreach (ReservationRoom resroom in domainReservation.ReservationRooms)
            {
                resroom.CancellationPolicyDays = 0;
                resroom.IsCancellationAllowed = true;
                resroom.CancellationValue = null;
                resroom.CancellationPolicyDays = null;
                resroom.CancellationCosts = true;
                resroom.CancellationNrNights = null;
            }

            reservationBuilder.WithNewGuest().WithCreditCardPayment().WithPartialPayment(BEPPCCM, 20);

            reservationBuilder.InputData.reservationDetail.UID = domainReservation.UID;
            reservationBuilder.InputData.reservationDetail.Property_UID = property_UID;
            reservationBuilder.InputData.reservationDetail.Number = "RES000322-" + property_UID;
            reservationBuilder.InputData.guest.Property_UID = property_UID;

            // Change Reservation Channel
            reservationBuilder.InputData.reservationDetail.Channel_UID = channel_UID;
            MockMultipleBasedOnBuilders(searchBuilder, reservationBuilder, Guid.NewGuid().ToString(), inventories: null, addTranlations: false);
            domainReservation.Date = DateTime.UtcNow - new TimeSpan(3, 0, 0, 0);

            // ACT
            var request = UpdateReservationRequest(reservationBuilder);
            request.Reservation.PaymentGatewayTransactionID = "teste";
            request.Reservation.PaymentGatewayName = gateway.ToString();
            request.ReservationPaymentDetail = new contractsReservations.ReservationPaymentDetail
            {
                PaymentGatewayTokenizationIsActive = true,
            };

            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>> pgtMock = new Mock<IRepository<OB.Domain.Payments.PaymentGatewayTransaction>>();
                    RepositoryFactoryMock.Setup(x => x.GetRepository<OB.Domain.Payments.PaymentGatewayTransaction>(It.IsAny<IUnitOfWork>())).
                        Returns(pgtMock.Object);

                    adyenMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    pgtMock.Setup(x => x.Any(It.IsAny<Expression<Func<OB.Domain.Payments.PaymentGatewayTransaction, bool>>>())).Returns(false);

                    _paymentGatewayFactory.Setup(x => x.Adyen(It.IsAny<PaymentGatewayConfig>())).Returns(adyenMock.Object);
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });

                    _paymentGatewayFactory.Setup(x => x.BPag(It.IsAny<PaymentGatewayConfig>())).Returns(bPagMock.Object);
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Setup(x => x.Void(It.IsAny<VoidRequest>())).Returns((VoidRequest voidRequest) =>
                    {
                        return new PaymentMessageResult
                        {
                            IsTransactionValid = true
                        };
                    });

                    _paymentGatewayFactory.Setup(x => x.BrasPag(It.IsAny<PaymentGatewayConfig>())).Returns(brasPagMock.Object);
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral> pgMock = new Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral>();
                    pgMock.Setup(x => x.IsTheSameDay(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(false);


                    maxiPagoMock.Setup(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns((string a, string b, decimal ammount) =>
                    {
                        return new PaymentMessageResult
                        {
                            IsTransactionValid = true,
                        };
                    });

                    _paymentGatewayFactory.Setup(x => x.MaxiPago(It.IsAny<PaymentGatewayConfig>())).Returns(maxiPagoMock.Object);
                    break;

                case Constants.PaymentGateway.PayU:
                    payUMock.Setup(x => x.Refund(It.IsAny<payUContracts.RefundRequest>())).Returns(new PaymentMessageResult
                    {
                        IsTransactionValid = true,
                    });
                    _paymentGatewayFactory.Setup(x => x.PayUColombia(It.IsAny<PaymentGatewayConfig>())).Returns(payUMock.Object);
                    break;
            }

            UpdateReservation(request);
            #endregion

            switch (gateway)
            {
                case Constants.PaymentGateway.Adyen:
                    adyenMock.Verify(m => m.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue), It.IsAny<string>()));
                    break;
                case Constants.PaymentGateway.BPag:
                    bPagMock.Setup(m => m.VoidOrRefund(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.BrasPag:
                    brasPagMock.Verify(m => m.Void(It.Is<VoidRequest>(pgReq => pgReq.Amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.MaxiPago:
                    maxiPagoMock.Verify(x => x.Refund(It.IsAny<string>(), It.IsAny<string>(), It.Is<decimal>(amount => amount == reservationValue)));
                    break;
                case Constants.PaymentGateway.PayU:
                    payUMock.Verify(m => m.Refund(It.Is<payUContracts.RefundRequest>(requestR => requestR.Amount == reservationValue)));
                    break;

            }
        }
        #endregion

        #region Reservation Operations Using Mock DB (for integration tests)
        protected ReservationData GetReservationDataFromDatabase(string reservationNumber, bool detached = false, IUnitOfWork unitOfWork = null)
        {
            ReservationData result = null;
            domainReservation.Reservation res = null;
            IUnitOfWork localUnitOfWork = null;
            try
            {
                localUnitOfWork = unitOfWork != null ? unitOfWork : this.SessionFactory.GetUnitOfWork();

                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(localUnitOfWork);

                res = reservationRepo.FirstOrDefault(x => x.Number == reservationNumber);

                if (res != null)
                    result = GetReservationDataFromDatabase(res.UID, detached, localUnitOfWork);
            }
            finally
            {
                if (unitOfWork == null)
                    localUnitOfWork.Dispose();
            }
            return result;
        }

        protected ReservationData GetReservationDataFromDatabase(long reservationUID, bool detached = false, IUnitOfWork unitOfWork = null, domainReservation.Reservation res = null)
        {
            //Kills the current Thread UnitOfWOrk
            if (unitOfWork == null && this.SessionFactory.CurrentUnitOfWork != null && !this.SessionFactory.CurrentUnitOfWork.IsDisposed)
                this.SessionFactory.CurrentUnitOfWork.Dispose();

            ReservationData databaseData = new ReservationData();
            IUnitOfWork localUnitOfWork = null;
            try
            {
                localUnitOfWork = unitOfWork == null ? this.SessionFactory.GetUnitOfWork() : unitOfWork;

                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(localUnitOfWork);
                var crmRepo = this.RepositoryFactory.GetOBCRMRepository();
                var reservationPaymentDetailRepo = this.RepositoryFactory.GetRepository<ReservationPaymentDetail>(localUnitOfWork);
                var reservationPartialPaymentDetailRepo = this.RepositoryFactory.GetRepository<ReservationPartialPaymentDetail>(localUnitOfWork);
                //var propertyQueueRepo = this.RepositoryFactory.GetRepository<PropertyQueue>(localUnitOfWork);
                var reservationRoomRepo = this.RepositoryFactory.GetRepository<ReservationRoom>(localUnitOfWork);
                var reservationRoomDetailRepo = this.RepositoryFactory.GetRepository<ReservationRoomDetail>(localUnitOfWork);
                var reservationRoomExtraRepo = this.RepositoryFactory.GetRepository<ReservationRoomExtra>(localUnitOfWork);
                var reservationRoomChildRepo = this.RepositoryFactory.GetRepository<ReservationRoomChild>(localUnitOfWork);
                var reservationRomExtraScheduleRepo = this.RepositoryFactory.GetRepository<ReservationRoomExtrasSchedule>(localUnitOfWork);
                var reservationsAdditionalData = this.RepositoryFactory.GetRepository<ReservationsAdditionalData>(localUnitOfWork);

                // get objects from database
                databaseData.reservationDetail = (res == null ?
                            reservationRepo.GetQuery(x => x.UID == reservationUID)
                            .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)))
                            .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies))
                            .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras))
                            .FirstOrDefault()
                            : res);

                var guestUID = databaseData.reservationDetail.Guest_UID;
                var guest = crmRepo.ListGuestsByLightCriteria(new ListGuestLightRequest() { UIDs = new List<long>() { guestUID } }).FirstOrDefault();
                databaseData.guest = guest != null ? OtherConverter.Convert(guest) : null;

                databaseData.reservationRooms = databaseData.reservationDetail.ReservationRooms.ToList();
                databaseData.reservationRoomDetails = databaseData.reservationDetail.ReservationRooms.SelectMany(rr => rr.ReservationRoomDetails).ToList();
                databaseData.guestActivityObj = databaseData.guest != null && databaseData.guest.GuestActivities != null ? databaseData.guest.GuestActivities : new List<OB.Reservation.BL.Contracts.Data.CRM.GuestActivity>();
                databaseData.reservationRoomExtras = databaseData.reservationDetail.ReservationRooms.SelectMany(rr => rr.ReservationRoomExtras).ToList();
                databaseData.reservationRoomChild = databaseData.reservationDetail.ReservationRooms.SelectMany(rr => rr.ReservationRoomChilds).ToList();
                databaseData.reservationPaymentDetail = reservationPaymentDetailRepo.GetQuery(rpd => rpd.Reservation_UID == reservationUID).FirstOrDefault();
                databaseData.reservationPartialPaymentDetail = databaseData.reservationDetail.ReservationPartialPaymentDetails != null ? databaseData.reservationDetail.ReservationPartialPaymentDetails.ToList() : null;
                databaseData.reservationExtraSchedule = databaseData.reservationRoomExtras.SelectMany(rre => rre.ReservationRoomExtrasSchedules).ToList();
                databaseData.reservationsAdditionalData = reservationsAdditionalData.GetQuery(x => x.Reservation_UID == reservationUID).FirstOrDefault();

                if (detached)
                {
                    reservationRepo.Detach(databaseData.reservationDetail);
                    reservationPaymentDetailRepo.Detach(databaseData.reservationPaymentDetail);
                }
                return databaseData;
            }
            finally
            {
                if (unitOfWork == null)
                    localUnitOfWork.Dispose();
            }
        }

        protected long InsertReservation(ReservationDataBuilder state, bool handleCancelationCosts = true, bool handleDepositCosts = true, bool validate = true, bool validateAllotment = true)
        {
            return state.InsertReservation(state, Container, ReservationManagerPOCO, handleCancelationCosts, handleDepositCosts, validate, validateAllotment);
        }

        protected Reservation.BL.Contracts.Requests.UpdateReservationRequest UpdateReservationRequest(ReservationDataBuilder builder)
        {
            var res = builder.InputData.reservationDetail != null ? DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail) : null;
            res.ReservationAdditionalData = builder.InputData.reservationsAdditionalData;

            return new Reservation.BL.Contracts.Requests.UpdateReservationRequest
            {
                Guest = builder.InputData.guest != null ? new Reservation.BL.Contracts.Data.CRM.Guest
                {

                    FirstName = builder.InputData.guest.FirstName,
                    LastName = builder.InputData.guest.LastName,
                    Email = builder.InputData.guest.Email,
                    Phone = builder.InputData.guest.Phone,
                    IDCardNumber = builder.InputData.guest.IDCardNumber,
                    Country_UID = builder.InputData.guest.Country_UID,
                    State_UID = builder.InputData.guest.State_UID,
                    City = builder.InputData.guest.City,
                    Address1 = builder.InputData.guest.Address1,
                    Address2 = builder.InputData.guest.Address2,
                    PostalCode = builder.InputData.guest.PostalCode
                } : null,
                Reservation = res,
                GuestActivities = builder.InputData.guestActivity,
                ReservationRooms = builder.InputData.reservationRooms != null ? builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                ReservationRoomDetails = builder.InputData.reservationRoomDetails != null ? builder.InputData.reservationRoomDetails.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                ReservationRoomExtras = builder.InputData.reservationRoomExtras != null ? builder.InputData.reservationRoomExtras.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                ReservationRoomChilds = builder.InputData.reservationRoomChild != null ? builder.InputData.reservationRoomChild.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                ReservationPaymentDetail = builder.InputData.reservationPaymentDetail != null ? DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationPaymentDetail) : null,
                ReservationExtraSchedules = builder.InputData.reservationExtraSchedule != null ? builder.InputData.reservationExtraSchedule.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                RuleType = null
            };
        }

        protected long UpdateReservation(Reservation.BL.Contracts.Requests.UpdateReservationRequest request)
        {
            // call the method
            ReservationManagerPOCO = Container.Resolve<IReservationManagerPOCO>();

            long result = -1;
            var resetEvet = new ManualResetEvent(false);
            Exception threadException = null;

            var task = new Thread(new ThreadStart(() =>
            {
                try
                {
                    // call the method
                    ReservationManagerPOCO = Container.Resolve<IReservationManagerPOCO>();
                    result = ReservationManagerPOCO.UpdateReservation(request).Result;
                    ReservationManagerPOCO.WaitForAllBackgroundWorkers();
                }
                catch (Exception e)
                {
                    threadException = e;
                }
                finally
                {
                    resetEvet.Set();
                }

            }));
            task.Start();
            resetEvet.WaitOne();
            if (threadException != null)
                ExceptionDispatchInfo.Capture(threadException).Throw();
            return result;
        }

        protected long UpdateReservation(ReservationDataBuilder builder)
        {
            // call the method
            ReservationManagerPOCO = Container.Resolve<IReservationManagerPOCO>();

            long result = -1;
            var resetEvet = new ManualResetEvent(false);
            Exception threadException = null;

            var res = builder.InputData.reservationDetail != null ? DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationDetail) : null;
            res.ReservationAdditionalData = builder.InputData.reservationsAdditionalData;

            var task = new Thread(new ThreadStart(() =>
            {
                try
                {
                    // call the method
                    ReservationManagerPOCO = Container.Resolve<IReservationManagerPOCO>();

                    result = ReservationManagerPOCO.UpdateReservation(
                        new Reservation.BL.Contracts.Requests.UpdateReservationRequest
                        {
                            Guest = builder.InputData.guest != null ? new Reservation.BL.Contracts.Data.CRM.Guest
                            {

                                FirstName = builder.InputData.guest.FirstName,
                                LastName = builder.InputData.guest.LastName,
                                Email = builder.InputData.guest.Email,
                                Phone = builder.InputData.guest.Phone,
                                IDCardNumber = builder.InputData.guest.IDCardNumber,
                                Country_UID = builder.InputData.guest.Country_UID,
                                State_UID = builder.InputData.guest.State_UID,
                                City = builder.InputData.guest.City,
                                Address1 = builder.InputData.guest.Address1,
                                Address2 = builder.InputData.guest.Address2,
                                PostalCode = builder.InputData.guest.PostalCode
                            } : null,
                            Reservation = res,
                            GuestActivities = builder.InputData.guestActivity,
                            ReservationRooms = builder.InputData.reservationRooms != null ? builder.InputData.reservationRooms.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                            ReservationRoomDetails = builder.InputData.reservationRoomDetails != null ? builder.InputData.reservationRoomDetails.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                            ReservationRoomExtras = builder.InputData.reservationRoomExtras != null ? builder.InputData.reservationRoomExtras.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                            ReservationRoomChilds = builder.InputData.reservationRoomChild != null ? builder.InputData.reservationRoomChild.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                            ReservationPaymentDetail = builder.InputData.reservationPaymentDetail != null ? DomainToBusinessObjectTypeConverter.Convert(builder.InputData.reservationPaymentDetail) : null,
                            ReservationExtraSchedules = builder.InputData.reservationExtraSchedule != null ? builder.InputData.reservationExtraSchedule.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : null,
                            RuleType = null
                        }).Result;

                    ReservationManagerPOCO.WaitForAllBackgroundWorkers();
                }
                catch (Exception e)
                {
                    threadException = e;
                }
                finally
                {
                    resetEvet.Set();
                }

            }));
            task.Start();
            resetEvet.WaitOne();
            if (threadException != null)
                ExceptionDispatchInfo.Capture(threadException).Throw();

            //return task.Result;
            return result;
        }
        #endregion

        #region Aux Methods for Reservations Operations (Unit Tests)
        protected contractsReservations.Reservation InsertReservation(out long errorCode, SearchBuilder searchBuilder, ReservationDataBuilder resBuilder, string transactionId = "",
            long propUid = 1806, bool withChildsRoom = true, bool withCancellationPolicies = true, long channelUid = 1, int numberOfAdults = 1, string resNumber = null,
            bool withNoRooms = false, bool emptyGuest = false, bool resIsEmpty = false, List<Inventory> inventories = null, PromotionalCode promoCode = null,
            long guestUid = 1, bool addTranslations = false, bool handleCancelationCosts = false, Incentive incentive = null, GroupRule groupRule = null, bool hasAges = true,
            List<int> ages = null, int numberOfRooms = 1, List<PropertyEventQR1> propertyEventQueryResultList = null, bool defineNewGuestUid = true, bool validate = false,
            bool? hndCredit = null, bool validateAllot = false, bool onRequestEnable = false, bool skipInterestCalculation = false, int channelOperatorType = 0, int availabilityType = 0, bool ignoreAvailability = false)
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

            var result = resBuilder.InsertReservation(resBuilder, Container, ReservationManagerPOCO, handleCancelationCosts, true, validate, validateAllot, false, transactionId, groupRule, skipInterestCalculation, ignoreAvailability: ignoreAvailability);

            if (result < 0)
            {
                RemoveIfInserted(resBuilder.ExpectedData.reservationDetail.Number);
                errorCode = result;
                return null;
            }

            var response = resBuilder.GetReservation(ReservationManagerPOCO, result);
            return response.Result.FirstOrDefault();
        }
        #endregion

        #region Aux Methods
        protected string GetCurrencySymbol(long currencyUid)
        {
            switch (currencyUid)
            {
                case 34:
                    return "EUR";
                case 16:
                    return "BRL";
                default:
                    return string.Empty;
            }
        }

        protected int GetAdultPriceTest(RateRoomDetail rrd, int numAds)
        {
            switch (numAds)
            {
                case 1:
                    return (int)rrd.Adult_1.GetValueOrDefault();
                case 2:
                    return (int)rrd.Adult_2.GetValueOrDefault();
                case 3:
                    return (int)rrd.Adult_3.GetValueOrDefault();
            }

            return 0;
        }
        protected int GetChildPriceTest(RateRoomDetail rrd, int numChs, int freeChilds = 0, bool countAsAdult = false)
        {
            if (countAsAdult)
                return 0;

            numChs -= freeChilds;

            switch (numChs)
            {
                case 1:
                    return (int)rrd.Child_1.GetValueOrDefault();
                case 2:
                    return (int)rrd.Child_2.GetValueOrDefault();
                case 3:
                    return (int)rrd.Child_3.GetValueOrDefault();
            }

            return 0;
        }

        public static Contracts.Data.CRM.ThirdPartyIntermediaryLight ConvertToLigth(OB.BL.Operations.Test.Domain.CRM.ThirdPartyIntermediary tpi)
        {
            return new ThirdPartyIntermediaryLight()
            {
                IsActive = tpi.IsActive.GetValueOrDefault(),
                IsDeleted = tpi.IsDeleted.GetValueOrDefault(),
                Name = tpi.Name,
                TravelAgent_UID = tpi.TravelAgent_UID,
                UID = tpi.UID
            };
        }

        #region Aux Credit
        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedValue"></param>
        /// <returns></returns>
        protected string DecryptCreditCard(string encryptedValue)
        {
            // Kill the Exception
            if (String.IsNullOrEmpty(encryptedValue)) return encryptedValue;

            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(GetEncryptionRsaKey());

                    var resultBytes = Convert.FromBase64String(encryptedValue);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);

                    return ShakeCreditCard(Encoding.UTF8.GetString(decryptedBytes));
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ccNumber"></param>
        /// <returns></returns>
        protected string ShakeCreditCard(string ccNumber)
        {
            // Kill the Exception
            if (String.IsNullOrEmpty(ccNumber)) return ccNumber;

            var lenght = ccNumber.Length;
            if (lenght < 4) return ccNumber;

            var lastFourDigits = ccNumber.Substring(lenght - 4);
            var initialDigits = ccNumber.Substring(0, lenght - 4);

            // Invert the Order
            char[] charArray = lastFourDigits.ToCharArray();
            Array.Reverse(charArray);
            lastFourDigits = new string(charArray);

            // Return the transformed number
            return initialDigits + lastFourDigits;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected string GetEncryptionRsaKey()
        {
            return "<RSAKeyValue><Modulus>49BStnomTkKFC1ERxGA/MhfVuwwNW8JcO52FKXFWfpb2m8a/f0Q3l7FRz6pC8Hs65+BOIwUN/7RxGV+PzIQ4ZwVwEuY7GBQonjXTP3D20yUnXZA/NtGmxWYJlVnD8VUZLQdkKs3hBAfKnWjyOgnGoq7CIFyJ9KKOxRh39hQ+7TE=</Modulus><Exponent>AQAB</Exponent><P>+hwRM4IsoIyrwSlbl01L57gZk41RHSYkcZwnsrjodAc/0KSCPm4JhKn8CKc9Xi8Rjq5ekJ0xSbS9VPAmJrwQrQ==</P><Q>6S3VBl3OF3rVnwg9Cg0bc+FGIfHgbZcCw1gV/k2awmS6GKpQHZ7Dr5UnH+QRXSJ1IQ3PJ6rieEeUsGhZxiarFQ==</Q><DP>ZMj0oYX+R8AH4jGxR9oNEVYdcFkM66sYGnPrh1h9y2u0anYwScn7qer5td72msJq180qLCo711CuztBq/0bfjQ==</DP><DQ>YQ7Zv8el9DIF3ydfuOJRzf8z4Qc8AoG7/bGZnfuRcl7Y81FY/atLCrfLzENzUs/37yU/V+SSVbx90Jvu2kLYLQ==</DQ><InverseQ>aq95HffRtKIVEpCEjyolZxm8ovf5SpfT2dkXzbxd7hIPNuM9mNWu897q9KKv3EyEY36dAryiIetVMhFGK0HzuQ==</InverseQ><D>Dyk6j/VSHkw0AXxMM+b53bITYcbcDrrBG6CQj6EA0hzm3Zgc/3HBR2GgIbNhkBKLaYoWeSMpetZ93mPrNH+qJyTeKIPx7mLGu14GS40uikfYIMhRlihRfNskYfiTXsRA35GFdyTgLCPo+OJXVxqaFZpjwXGoBri7Vlgz9E+LRak=</D></RSAKeyValue>";
        }

        protected ListCreditCardResponse EncryptCreditCards(ListCreditCardRequest request)
        {
            var response = new ListCreditCardResponse();

            try
            {
                response.CreditCards = request.CreditCards.Select(x => EncryptCreditCard(x)).ToList();
                response.Succeed();
            }
            catch (Exception ex)
            {
                response.Errors.Add(new Error("EncryptCreditCards Failed", 0, "Could not encrypt card number. Please, check if card number is already encrypted"));
                response.Errors.Add(new Error(ex));
                response.Failed();
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ccNumber"></param>
        /// <returns></returns>
        protected string EncryptCreditCard(string ccNumber)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(GetEncryptionRsaKey());

                    var valueToEncrypt = Encoding.UTF8.GetBytes(ShakeCreditCard(ccNumber));
                    var encryptedData = rsa.Encrypt(valueToEncrypt, true);

                    return Convert.ToBase64String(encryptedData);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
        #endregion

        protected void AddTranslateToMockOtherPolicy(OtherPolicy otherPolicy, Domain.OtherPoliciesLanguage policyTranslate)
        {
            var tmp = otherPolicy.Clone();
            tmp.TranslatedName = policyTranslate.Name;
            tmp.TranslatedDescription = policyTranslate.Description;
            otherPolicyList.Remove(otherPolicy);
            otherPolicyList.Add(tmp);
        }
        protected void AddTranslateToMockCancellationPolicy(CancellationPolicy cancellationPolicy, CancellationPoliciesLanguage policyTranslate)
        {
            var tmp = cancellationPolicy.Clone();
            tmp.TranslatedName = policyTranslate.Name;
            tmp.TranslatedDescription = policyTranslate.Description;
            cancellationPolicyList.Remove(cancellationPolicy);
            cancellationPolicyList.Add(tmp);
        }
        protected void AddTranslateToMockDepositPolicy(DepositPolicy depositPolicy, DepositPoliciesLanguage policyTranslate)
        {
            var tmp = depositPolicy.Clone();
            tmp.TranslatedName = policyTranslate.Name;
            tmp.TranslatedDescription = policyTranslate.Description;
            depositPolicyList.Remove(depositPolicy);
            depositPolicyList.Add(tmp);
        }

        protected void RemoveIfInserted(string number)
        {
            if (reservationsList.Select(x => x.Number).Contains(number))
            {
                var resToRemove = reservationsList.Where(x => x.Number == number).SingleOrDefault();
                reservationsList.Remove(resToRemove);
            }
        }

        #region Aux Remove
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
        #endregion

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

        #region  LIST CANCEL RESERVATION REASONS

        [TestMethod]
        [TestCategory("ListCancelReservationReasons")]
        public void ListCancelReservationReasons_Simple_Success()
        {
            // Arrange
            MockCancelReservationReasonRepo();
            var request = new ListCancelReservationReasonRequest();

            // Act
            var response = _reservationManagerPOCO.ListCancelReservationReasons(request);

            // Assert
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(0, response.Warnings.Count);

            Assert.AreEqual(3, response.Result.Count);
            Assert.AreEqual("name 1", response.Result[0].Name);
            Assert.AreEqual("name 2", response.Result[1].Name);
            Assert.AreEqual("name 3", response.Result[2].Name);
        }

        [TestMethod]
        [TestCategory("ListCancelReservationReasons")]
        public void ListCancelReservationReasons_LanguageUID_TranslatedResulst()
        {
            // Arrange
            MockCancelReservationReasonRepo();
            var request = new ListCancelReservationReasonRequest { LanguageUID = 1 };

            // Act
            var response = _reservationManagerPOCO.ListCancelReservationReasons(request);

            // Assert
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(0, response.Errors.Count);
            Assert.AreEqual(0, response.Warnings.Count);

            Assert.AreEqual(3, response.Result.Count);
            Assert.AreEqual("translated name 11", response.Result[0].Name);
            Assert.AreEqual("name 2", response.Result[1].Name);
            Assert.AreEqual("name 3", response.Result[2].Name);
        }

        #endregion

        #region CheckAvailability Tests

        [TestMethod]
        [TestCategory("ReservationManagerPOCO.CheckRemoteAvailability")]
        public void ReservationManagerPOCO_CheckRemoteAvailability_Success()
        {
            // Arrange
            MockExternalSystemsRepo();
            var reservation = new contractsReservations.Reservation
            {
                UID = 1,
                Property_UID = 1
            };

            string requestId = "requestId";
            var response = Container.Resolve<ReservationManagerPOCO>();

            // Act

            var result = response.CheckRemoteAvailability(requestId, reservation);

            // Assert
            Assert.AreEqual(ES.API.Contracts.PreCheckStatus.IsAllowed, result.PreCheckStatus);
            Assert.AreEqual(requestId, result.MessageIdentifierToken);
            Assert.IsNotNull(result.ReservationExternalIdentifier);
            //throw
        }

        [TestMethod]
        [TestCategory("ReservationManagerPOCO.CheckRemoteAvailability")]
        public void ReservationManagerPOCO_CheckRemoteAvailability_Fail()
        {
            // Arrange
            MockExternalSystemsRepo();
            contractsReservations.Reservation reservation = null;
            string requestId = "requestId";
            var response = Container.Resolve<ReservationManagerPOCO>();

            // Act

            var result = response.CheckRemoteAvailability(requestId, reservation);

            // Assert
            Assert.AreEqual(ES.API.Contracts.PreCheckStatus.IsNotAllowed, result.PreCheckStatus);
            Assert.AreEqual(requestId, result.MessageIdentifierToken);
            Assert.IsNull(result.ReservationExternalIdentifier);
            //throw
        }

        [TestMethod]
        [TestCategory("ReservationManagerPOCO.CheckIfPropertyNeedsCheckAvailability")]
        public void ReservationManagerPOCO_CheckIfPropertyNeedsCheckAvailability()
        {
            // Arrange

            MockPMSRepo();
            MockObListPropertyRepo();

            string requestId = "requestId";
            long propertyId = 1;
            var response = Container.Resolve<ReservationManagerPOCO>();

            // Act

            var result = response.CheckIfPropertyNeedsToCheckAvailability(propertyId, requestId);

            // Assert
            Assert.IsTrue(result);

            //throw
        }

        [TestMethod]
        [TestCategory("ReservationManagerPOCO.MapPmsNumbers")]
        public void ReservationManagerPOCO_MapPmsNumbers_Success()
        {
            // Arrange
            var reservation = new domainReservation.Reservation
            {
                PmsRservationNumber = "ResNumber",
                ReservationRooms = new List<ReservationRoom>
                {
                    new ReservationRoom { PmsRservationNumber = "ResRoomNumber1", UID = 1 },
                    new ReservationRoom { PmsRservationNumber = "ResRoomNumber2", UID = 2 },
                }
            };

            var reservationExternalIdentifier = new ES.API.Contracts.Reservations.ReservationExternalIdentifier
            {
                ExternalNumber = "PmsResNumber",
                ReservationRoomExternalIdentifiers = new List<ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier>
                {
                    new ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier { ExternalNumber = "PmsResRoomNumber1", InternalId = 1},
                    new ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier { ExternalNumber = "PmsResRoomNumber2", InternalId = 2},
                }
            };
            var response = Container.Resolve<ReservationManagerPOCO>();

            // Act

            var result = response.MapPmsNumbers(reservation, reservationExternalIdentifier);

            // Assert
            Assert.AreEqual(reservationExternalIdentifier.ExternalNumber, result.PmsRservationNumber);
            Assert.AreEqual(reservationExternalIdentifier.ReservationRoomExternalIdentifiers[0].ExternalNumber, result.ReservationRooms.FirstOrDefault().PmsRservationNumber);
            Assert.AreEqual(reservationExternalIdentifier.ReservationRoomExternalIdentifiers[1].ExternalNumber, result.ReservationRooms.LastOrDefault().PmsRservationNumber);

            //throw
        }

        [TestMethod]
        [TestCategory("ReservationManagerPOCO.InsertNewExternalNumbersHistory")]
        public void ReservationManagerPOCO_InsertNewExternalNumbersHistory_Success()
        {
            // Arrange
            var reservation = new domainReservation.Reservation
            {
                PmsRservationNumber = "ResNumber",
                ReservationRooms = new List<ReservationRoom>
                {
                    new ReservationRoom { PmsRservationNumber = "ResRoomNumber1", UID = 1, DateFrom = DateTime.Today.AddDays(10), Status = 1},
                    new ReservationRoom { PmsRservationNumber = "ResRoomNumber2", UID = 2, DateFrom = DateTime.Today, Status = 4 },
                }
            };

            var reservationExternalIdentifier = new ES.API.Contracts.Reservations.ReservationExternalIdentifier
            {
                ExternalNumber = "PmsResNumber",
                ClientId = 1,
                InternalId = 1,
                IsByReservationRoom = false,
                PmsId = 1,
                ChannelNumber = "ChannelNumber - QueParaJáNãoServeParaNada",
                ReservationRoomExternalIdentifiers = new List<ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier>
                {
                    new ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier { ExternalNumber = "PmsResRoomNumber1", InternalId = 1},
                    new ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier { ExternalNumber = "PmsResRoomNumber2", InternalId = 2},
                }
            };
            var response = Container.Resolve<ReservationManagerPOCO>();

            // Act

            var result = response.MapPmsNumbers(reservation, reservationExternalIdentifier);

            // Assert
            Assert.AreEqual(reservationExternalIdentifier.ExternalNumber, result.PmsRservationNumber);
            Assert.AreEqual(reservationExternalIdentifier.ReservationRoomExternalIdentifiers[0].ExternalNumber, result.ReservationRooms.FirstOrDefault().PmsRservationNumber);
            Assert.AreEqual(reservationExternalIdentifier.ReservationRoomExternalIdentifiers[1].ExternalNumber, result.ReservationRooms.LastOrDefault().PmsRservationNumber);

            //throw
        }

        [ExpectedException(typeof(BusinessLayerException))]
        [TestMethod]
        [TestCategory("ReservationManagerPOCO.InsertNewExternalNumbersHistory")]
        public void ReservationManagerPOCO_ValidateCheckAvailabilityResponse_Success()
        {
            // Arrange
            var checkAvailResponse = new ES.API.Contracts.Response.CheckRemoteAvailabilityResponse
            {
                PreCheckStatus = ES.API.Contracts.PreCheckStatus.IsNotAllowed,
                Errors = new List<ES.API.Contracts.Error>
                {
                    new ES.API.Contracts.Error
                    {
                        Code = ES.API.Contracts.ErrorCode.InvalidPriceDefined,
                    }
                }
            };

            var response = Container.Resolve<ReservationManagerPOCO>();

            // Act

            response.ValidateCheckAvailabilityResponse(checkAvailResponse);

            // Assert

            //throw
        }

        [TestMethod]
        [TestCategory("ReservationManagerPOCO.CheckIfThereAreAnyActiveServiceInProperty")]
        public void ReservationManagerPOCO_CheckIfReservationProcessServiceIsActiveInProperty_Success()
        {
            // Arrange
            MockPMSRepo();

            var propertyId = 1;
            var requestId = "RequestId";

            var response = Container.Resolve<ReservationManagerPOCO>();

            // Act

            var result = response.CheckIfPropertyHasBookingServiceActive(propertyId, requestId);

            // Assert

            Assert.IsTrue(result);

            //throw
        }

        [TestMethod]
        [TestCategory("ReservationManagerPOCO.CheckIfThereAreAnyActiveServiceInProperty")]
        public void ReservationManagerPOCO_CheckIfReservationProcessServiceIsActiveInProperty_Fail()
        {
            // Arrange
            MockPMSRepo();

            var propertyId = 0;
            var requestId = "RequestId";

            var response = Container.Resolve<ReservationManagerPOCO>();

            // Act

            var result = response.CheckIfPropertyHasBookingServiceActive(propertyId, requestId);

            // Assert

            Assert.IsFalse(result);

            //throw
        }

        #endregion CheckAvailability Tests


        #region SaveReservationExternalIdentifier

        [TestMethod]
        [TestCategory("SaveReservationExternalIdentifier")]
        public void SaveReservationExternalIdentifier_ValidateRequest_NullRequest_Return1Error()
        {
            // Arrange
            var resManagerPoco = Container.Resolve<ReservationManagerPOCO>();
            OB.Reservation.BL.Contracts.Requests.SaveReservationExternalIdentifierRequest request = null;

            // Act
            var response = resManagerPoco.ValidateRequest(request);

            // Assert
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(0, response.Warnings.Count);
            Assert.AreEqual(1, response.Errors.Count);
            Assert.AreEqual("Request object instance is expected", response.Errors.FirstOrDefault().Description);
            Assert.AreEqual(2, response.Errors.FirstOrDefault().ErrorCode);
            Assert.AreEqual("InvalidRequest", response.Errors.FirstOrDefault().ErrorType);
        }

        [TestMethod]
        [TestCategory("SaveReservationExternalIdentifier")]
        public void SaveReservationExternalIdentifier_ValidateRequest_EmptyRequest_Return4Errors()
        {
            // Arrange
            var resManagerPoco = Container.Resolve<ReservationManagerPOCO>();
            var request = new OB.Reservation.BL.Contracts.Requests.SaveReservationExternalIdentifierRequest();

            // Act
            var response = resManagerPoco.ValidateRequest(request);

            // Assert
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, response.Status);
            Assert.AreEqual(0, response.Warnings.Count);
            Assert.AreEqual(4, response.Errors.Count);
            Assert.AreEqual(199, response.Errors[0].ErrorCode);
            Assert.AreEqual("InvalidRequest", response.Errors[0].ErrorType);
            Assert.AreEqual("SaveReservationExternalIdentifierRequest - PropertyId is null or empty", response.Errors[0].Description);
            Assert.AreEqual(199, response.Errors[1].ErrorCode);
            Assert.AreEqual("InvalidRequest", response.Errors[1].ErrorType);
            Assert.AreEqual("SaveReservationExternalIdentifierRequest - ClientId is null or empty", response.Errors[1].Description);
            Assert.AreEqual(199, response.Errors[2].ErrorCode);
            Assert.AreEqual("InvalidRequest", response.Errors[2].ErrorType);
            Assert.AreEqual("SaveReservationExternalIdentifierRequest - PmsId is null or empty", response.Errors[2].Description);
            Assert.AreEqual(199, response.Errors[3].ErrorCode);
            Assert.AreEqual("InvalidRequest", response.Errors[3].ErrorType);
            Assert.AreEqual("SaveReservationExternalIdentifierRequest - ReservationExternalIdentifier is null or empty", response.Errors[3].Description);
        }

        #endregion SaveReservationExternalIdentifier

        #region UpdateInternalNotes
        [TestMethod]
        [TestCategory("InsertInReservationInternalNotes")]
        public void InsertInReservationInternalNotes_UpdateInternalNotes_Success()
        {
            // Arrange
            var resManagerPoco = Container.Resolve<ReservationManagerPOCO>();
            var request = new InsertInReservationInternalNotesRequest
            {
                NotesToApped = "teste",
                ReservationNumber = "Orbitz-CJ1SVI"
            };

            _reservationsRepoMock.Setup(x => x.FindReservationByNumber(It.IsAny<string>()))
                .Returns(reservationsList.Where(x => x.Number == request.ReservationNumber).ToList());

            var domainReservation = reservationsList.Where(x => x.Number == request.ReservationNumber).ToList().FirstOrDefault();

            // Act
            var response = resManagerPoco.InsertInReservationInternalNotes(request);

            // Assert
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, response.Status);
            Assert.AreEqual(0, response.Warnings.Count);
            Assert.AreEqual(0, response.Errors.Count);

            Assert.AreEqual(request.NotesToApped, domainReservation.InternalNotes);
        }
        #endregion
    }
}

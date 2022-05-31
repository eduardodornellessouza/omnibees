using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.DL.Common.Interfaces;
using OB.BL.Operations.Interfaces;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System;
using OB.BL.Operations.Internal.BusinessObjects;
using System.Linq;
using OB.Api.Core;
using OB.BL.Contracts.Data.Rates;
using OB.DL.Common.Repositories.Interfaces;
using OB.BL.Operations.Test.Helper;
using OB.BL.Contracts.Requests;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.BL.Operations.Test.POCOS
{
    // Unit tests for ReservationHelperPOCO_PromotionalCodes.cs file
    [TestClass]
    public class ReservationHelperPOCO_PromotionalCodesUnitTest : UnitBaseTest
    {
        private IReservationHelperPOCO reservationHelperPOCO;
        private Mock<IOBPromotionalCodeRepository> promoCodePOCOMock;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var repoFactoryMock = new Mock<IRepositoryFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            promoCodePOCOMock = new Mock<IOBPromotionalCodeRepository>();
            repoFactoryMock.Setup(x => x.GetOBPromotionalCodeRepository()).Returns(promoCodePOCOMock.Object);

            this.Container = this.Container.RegisterInstance(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance(repoFactoryMock.Object);
            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            this.reservationHelperPOCO = this.Container.Resolve<IReservationHelperPOCO>();
        }

        private ValidPromocodeParameters ActValidatePromocodeForReservation(PromotionalCodeBuilder builder)
        {
            promoCodePOCOMock.Setup(x => x.ListPromotionalCodeForValidRates(It.IsAny<ListPromotionalCodeForValidRatesRequest>()))
                .Returns(builder.InputData.ValidateMockOutput);

            return reservationHelperPOCO.ValidatePromocodeForReservation(builder.InputData.ValidateInput);
        }

        #region Asserts

        private void Assert_ResultNotNull(ValidPromocodeParameters result)
        {
            Assert.IsNotNull(result, "Result shouldn't be null.");
        }

        private void Assert_PromocodeNotNull(ValidPromocodeParameters result)
        {
            Assert.IsNotNull(result.PromoCodeObj, "Promotional Code shouldn't be null.");
        }

        private void Assert_RejectReservation(ValidPromocodeParameters result)
        {
            Assert.IsTrue(result.RejectReservation, "RejectReservation should be true.");
        }

        private void Assert_AcceptReservation(ValidPromocodeParameters result)
        {
            Assert.IsFalse(result.RejectReservation, "RejectReservation should be false.");
        }

        private void Assert_InactivePromocode(ValidPromocodeParameters result)
        {
            Assert.IsFalse(result.PromoCodeObj.IsValid, "Promotional Code should be inactive.");
        }

        private void Assert_ActivePromocode(ValidPromocodeParameters result)
        {
            Assert.IsTrue(result.PromoCodeObj.IsValid, "Promotional Code should be active.");
        }

        private void Assert_ContainsReservationRoom(ValidPromocodeParameters result, long expectedRateUID, DateTime expectedCheckin, DateTime expectedCheckout)
        {
            Assert.AreEqual(1, result.ReservationRoomsPeriods.Count(x => x.RateUID == expectedRateUID && x.CheckIn == expectedCheckin && x.CheckOut == expectedCheckout),
                string.Format("Expect ReservationRoom with RateUID={0}, Checkin='{1}', Checkout='{2}'", expectedRateUID, expectedCheckin, expectedCheckout));
        }

        private void Assert_ContainsNewDiscountDay(ValidPromocodeParameters result, DateTime expectedDate)
        {
            Assert.AreEqual(1, result.NewDaysToApplyDiscount.Count(x => x == expectedDate), "Should apply discount to day: " + expectedDate.ToShortDateString());
        }

        private void Assert_ReservationRoomsCount(ValidPromocodeParameters result, int expectedCount)
        {
            Assert.AreEqual(expectedCount, result.ReservationRoomsPeriods.Count, "Result should contains " + expectedCount + " ReservationRoomsPeriods.");
        }

        private void Assert_NewDaysToApplyDiscountCount(ValidPromocodeParameters result, int expectedCount)
        {
            Assert.AreEqual(expectedCount, result.NewDaysToApplyDiscount.Count, "Result should contains " + expectedCount + " NewDaysToApplyDiscount.");
        }

        private void Assert_LimitType1(ValidPromocodeParameters result, PromotionalCode expected)
        {
            Assert.IsTrue(result.PromoCodeObj.ActiveLimits ?? false, "Promotional Code must have active limits.");
            Assert.AreEqual(1, result.PromoCodeObj.LimitType, "Promotional Code Limit Type should be 1.");
            Assert.AreEqual(expected.MaxReservations, result.PromoCodeObj.MaxReservations, "Promotional Code Max reservations should be " + expected.MaxReservations);
            Assert.AreEqual(expected.ReservationsCompleted, result.PromoCodeObj.ReservationsCompleted, "Promotional Code Max reservations should be " + expected.ReservationsCompleted);
        }

        private void Assert_LimitType2(ValidPromocodeParameters result, PromotionalCode expected)
        {
            Assert.IsTrue(result.PromoCodeObj.ActiveLimits ?? false, "Promotional Code must have active limits.");
            Assert.AreEqual(2, result.PromoCodeObj.LimitType, "Promotional Code Limit Type should be 1.");

            Assert.AreEqual(expected.LimitWeekDays.Count, result.PromoCodeObj.LimitWeekDays.Count, "Promotional Code Maximum Reservations per day count should be " + expected.LimitWeekDays.Count);
            foreach (var item in expected.LimitWeekDays)
            {
                Assert.AreEqual(item.Value, result.PromoCodeObj.LimitWeekDays[item.Key],
                    string.Format("Maximum Reservation for {0} shoul be {1}", (DayOfWeek)item.Key, item.Value));
            }

            Assert.AreEqual(expected.PromotionalCodesByDays.Count, result.PromoCodeObj.PromotionalCodesByDays.Count, "Promotional Code reservations completed should be " + expected.PromotionalCodesByDays.Count);
            foreach (var item in expected.PromotionalCodesByDays)
            {
                Assert.AreEqual(item.ReservationsCompleted, result.PromoCodeObj.PromotionalCodesByDays.First(x => x.WeekDay == item.WeekDay).ReservationsCompleted,
                    string.Format("Reservation Completed for {0} shoul be {1}", (DayOfWeek)item.WeekDay, item.ReservationsCompleted));
            }
        }

        #endregion

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Inactive_ExclusiveAnotherPromo()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.InactivePromocode();
            builder.WithoutValidRates();

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Inactive_NotExclusiveAnotherPromo()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.InactivePromocode();
            builder.WithRoomInsidePeriod(false);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_NoAssociatedRates()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithRoomInsidePeriod(false);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_NotExclusive_ExclusiveForAnotherPromo()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithoutValidRates();

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        #region Rates Are Exclusive for Promotional Code

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Exclusive_PeriodContainsStay()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Exclusive_StayContainsPeriod()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.WithRoomCheckinLessThanValidFromCheckoutGreaterThanValidTo(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20509029, new DateTime(2050, 9, 29), new DateTime(2050, 11, 2));
            Assert_NewDaysToApplyDiscountCount(result, 30);
            for (var date = new DateTime(2050, 10, 01); date <= new DateTime(2050, 10, 30); date = date.AddDays(1))
                Assert_ContainsNewDiscountDay(result, date);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Exclusive_PeriodContainsPartialStay()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.WithRoomCheckinOutsideCheckoutInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 2050930, new DateTime(2050, 9, 30), new DateTime(2050, 10, 4));
            Assert_NewDaysToApplyDiscountCount(result, 3);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 1));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 2));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 3));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Exclusive_CheckinEqualsValidTo()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.WithRoomCheckinEqualsValidTo(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501030, new DateTime(2050, 10, 30), new DateTime(2050, 11, 2));
            Assert_NewDaysToApplyDiscountCount(result, 1);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 30));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Exclusive_CheckoutEqualsValidFrom()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.WithRoomCheckoutEqualsValidFrom(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Exclusive_StayOutOfPeriod()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.WithRoomOutsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType1_MaxEqualsCompleted_Insert()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.WithRoomInsidePeriod(true);
            builder.ActiveLimit1MaxEqualsCompleted();

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType1_MaxEqualsCompleted_Modify()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit1MaxEqualsCompleted();
            builder.WithRoomInsidePeriod(true);
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 9));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 10));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 11));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 12));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 13));

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_LimitType1(result, builder.InputData.ValidateMockOutput.PromotionalCode);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType1_MaxLessThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit1MaxLessThanCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType1_MaxGreaterThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit1MaxGreaterThanCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_LimitType1(result, builder.InputData.ValidateMockOutput.PromotionalCode);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType1_TwoDifferentRooms()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit1MaxEqualsCompleted();
            builder.WithRoomInsidePeriod(true);
            builder.WithRoomCheckinEqualsValidTo(true);
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 30));

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 2);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_ContainsReservationRoom(result, 20501030, new DateTime(2050, 10, 30), new DateTime(2050, 11, 2));
            Assert_NewDaysToApplyDiscountCount(result, 5);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 30));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType2_MaxEqualsCompleted_Insert()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit2MaxEqualsCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType2_MaxEqualsCompleted_Modify()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit2MaxEqualsCompleted();
            builder.WithRoomInsidePeriod(true);
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 10));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 11));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 12));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 13));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 14));

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_LimitType2(result, builder.InputData.ValidateMockOutput.PromotionalCode);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType2_MaxLessThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit2MaxLessThanCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType2_MaxGreaterThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit2MaxGreaterThanCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_LimitType2(result, builder.InputData.ValidateMockOutput.PromotionalCode);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType2_Room1MaxLowerThanCompleted_Room2MaxGreaterThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit2MaxGreaterThanCompleted();
            builder.InputData.ValidateMockOutput.PromotionalCode.LimitWeekDays[0] = 9;
            builder.WithRoomCheckinEqualsValidTo(true);
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_Exclusive_LimitType2_OneDayWithouLimit()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ExclusivePromocode();
            builder.ActiveLimit2MaxGreaterThanCompleted();
            builder.InputData.ValidateMockOutput.PromotionalCode.LimitWeekDays.Remove(1); // Remove monday
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_InactivePromocode(result);
            Assert_RejectReservation(result);
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        #endregion Rates Are Exclusive for Reservation Promotional Code

        #region Rates Are Not Exclusive for all Promotional Codes

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_NotExclusiveForAllPromos_PeriodContainsStay()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_NotExclusiveForAllPromos_StayContainsPeriod()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithRoomCheckinLessThanValidFromCheckoutGreaterThanValidTo(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20509029, new DateTime(2050, 9, 29), new DateTime(2050, 11, 2));
            Assert_NewDaysToApplyDiscountCount(result, 30);
            for (var date = new DateTime(2050, 10, 01); date <= new DateTime(2050, 10, 30); date = date.AddDays(1))
                Assert_ContainsNewDiscountDay(result, date);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_NotExclusiveForAllPromos_PeriodContainsPartialStay()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithRoomCheckinOutsideCheckoutInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 2050930, new DateTime(2050, 9, 30), new DateTime(2050, 10, 4));
            Assert_NewDaysToApplyDiscountCount(result, 3);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 1));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 2));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 3));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_NotExclusiveForAllPromos_CheckinEqualsValidTo()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithRoomCheckinEqualsValidTo(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501030, new DateTime(2050, 10, 30), new DateTime(2050, 11, 2));
            Assert_NewDaysToApplyDiscountCount(result, 1);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 30));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_NotExclusiveForAllPromos_CheckoutEqualsValidFrom()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithRoomCheckoutEqualsValidFrom(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 2050928, new DateTime(2050, 9, 28), new DateTime(2050, 10, 1));
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_NotExclusiveForAllPromos_StayOutOfPeriod()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithRoomOutsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 205091, new DateTime(2050, 9, 1), new DateTime(2050, 9, 5));
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType1_MaxEqualsCompleted_Insert()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.WithRoomInsidePeriod(true);
            builder.ActiveLimit1MaxEqualsCompleted();

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType1_MaxEqualsCompleted_Modify()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit1MaxEqualsCompleted();
            builder.WithRoomInsidePeriod(true);
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 9));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 10));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 11));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 12));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 13));

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_LimitType1(result, builder.InputData.ValidateMockOutput.PromotionalCode);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType1_MaxLessThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit1MaxLessThanCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType1_MaxGreaterThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit1MaxGreaterThanCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_LimitType1(result, builder.InputData.ValidateMockOutput.PromotionalCode);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType1_TwoDifferentRooms()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit1MaxEqualsCompleted();
            builder.WithRoomInsidePeriod(true);
            builder.WithRoomCheckinEqualsValidTo(true);
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 30));

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 2);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_ContainsReservationRoom(result, 20501030, new DateTime(2050, 10, 30), new DateTime(2050, 11, 2));
            Assert_NewDaysToApplyDiscountCount(result, 5);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 30));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType2_MaxEqualsCompleted_Insert()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit2MaxEqualsCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType2_MaxEqualsCompleted_Modify()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit2MaxEqualsCompleted();
            builder.WithRoomInsidePeriod(true);
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 10));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 11));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 12));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 13));
            builder.InputData.ValidateInput.OldAppliedDiscountDays.Add(new DateTime(2050, 10, 14));

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_LimitType2(result, builder.InputData.ValidateMockOutput.PromotionalCode);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType2_MaxLessThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit2MaxLessThanCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType2_MaxGreaterThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit2MaxGreaterThanCompleted();
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_LimitType2(result, builder.InputData.ValidateMockOutput.PromotionalCode);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType2_Room1MaxLowerThanCompleted_Room2MaxGreaterThanCompleted()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit2MaxGreaterThanCompleted();
            builder.InputData.ValidateMockOutput.PromotionalCode.LimitWeekDays[0] = 9;
            builder.WithRoomCheckinEqualsValidTo(true);
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 2);
            Assert_ContainsReservationRoom(result, 20501030, new DateTime(2050, 10, 30), new DateTime(2050, 11, 2));
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 4);
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 10));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 11));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 12));
            Assert_ContainsNewDiscountDay(result, new DateTime(2050, 10, 13));
        }

        [TestMethod]
        [TestCategory("PromotionalCodeValidation")]
        public void ValidatePromocodeForReservation_Valid_NotExclusiveForAllPromos_LimitType2_OneDayWithouLimit()
        {
            // Build
            var builder = new PromotionalCodeBuilder();
            builder.ActiveLimit2MaxGreaterThanCompleted();
            builder.InputData.ValidateMockOutput.PromotionalCode.LimitWeekDays.Remove(1); // Remove monday
            builder.WithRoomInsidePeriod(true);

            // Act
            var result = ActValidatePromocodeForReservation(builder);

            // Asserts
            Assert_ResultNotNull(result);
            Assert_PromocodeNotNull(result);
            Assert_ActivePromocode(result);
            Assert_AcceptReservation(result);
            Assert_ReservationRoomsCount(result, 1);
            Assert_ContainsReservationRoom(result, 20501010, new DateTime(2050, 10, 10), new DateTime(2050, 10, 14));
            Assert_NewDaysToApplyDiscountCount(result, 0);
        }

        #endregion Rates Are NotExclusiveForAllPromos for Reservation Promotional Code
    }
}
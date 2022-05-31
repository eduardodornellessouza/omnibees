using System;
using System.Collections.Generic;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Contracts.Responses;
using OB.BL.Contracts.Data.Rates;

namespace OB.BL.Operations.Test.Helper
{
    class PromotionalCodeBuilder
    {
        public PromotionalCodeInputData InputData { get; set; }

        public PromotionalCodeBuilder()
        {
            InputData = new PromotionalCodeInputData();
        }

        public void ExclusivePromocode()
        {
            InputData.ValidateMockOutput.PromotionalCode.IsPromotionalCodeVisibleRate = true;
        }

        public void InactivePromocode()
        {
            InputData.ValidateMockOutput.PromotionalCode.IsValid = false;
        }

        public void WithoutValidRates()
        {
            InputData.ValidateInput.ReservationRooms.Add(new ReservationRoomStayPeriod()
            {
                RateUID = 20501031
            });
            InputData.ValidateMockOutput.ValidRateUIDs.Clear();
        }

        public void ActiveLimit1MaxLessThanCompleted()
        {
            InputData.ValidateMockOutput.PromotionalCode.ActiveLimits = true;
            InputData.ValidateMockOutput.PromotionalCode.LimitType = 1;
            InputData.ValidateMockOutput.PromotionalCode.MaxReservations = 9;
            InputData.ValidateMockOutput.PromotionalCode.ReservationsCompleted = 10;
        }

        public void ActiveLimit1MaxGreaterThanCompleted()
        {
            InputData.ValidateMockOutput.PromotionalCode.ActiveLimits = true;
            InputData.ValidateMockOutput.PromotionalCode.LimitType = 1;
            InputData.ValidateMockOutput.PromotionalCode.MaxReservations = 10;
            InputData.ValidateMockOutput.PromotionalCode.ReservationsCompleted = 9;
        }

        public void ActiveLimit1MaxEqualsCompleted()
        {
            InputData.ValidateMockOutput.PromotionalCode.ActiveLimits = true;
            InputData.ValidateMockOutput.PromotionalCode.LimitType = 1;
            InputData.ValidateMockOutput.PromotionalCode.MaxReservations = 10;
            InputData.ValidateMockOutput.PromotionalCode.ReservationsCompleted = 10;
        }

        public void ActiveLimit2MaxLessThanCompleted()
        {
            InputData.ValidateMockOutput.PromotionalCode.ActiveLimits = true;
            InputData.ValidateMockOutput.PromotionalCode.LimitType = 2;
            InputData.ValidateMockOutput.PromotionalCode.LimitWeekDays = new Dictionary<int, int>()
            {
                { 0, 9 },
                { 1, 9 },
                { 2, 9 },
                { 3, 9 },
                { 4, 9 },
                { 5, 9 },
                { 6, 9 }
            };
            for (DateTime date = new DateTime(2050, 10, 1); date < new DateTime(2050, 10, 30); date = date.AddDays(1))
            {
                InputData.ValidateMockOutput.PromotionalCode.PromotionalCodesByDays.Add(
                    new PromotionalCodesByDay()
                    {
                        Date = date,
                        WeekDay = (int)date.DayOfWeek,
                        ReservationsCompleted = 10,
                        PromotionalCode_UID = 1
                    });
            }
        }

        public void ActiveLimit2MaxGreaterThanCompleted()
        {
            InputData.ValidateMockOutput.PromotionalCode.ActiveLimits = true;
            InputData.ValidateMockOutput.PromotionalCode.LimitType = 2;
            InputData.ValidateMockOutput.PromotionalCode.LimitWeekDays = new Dictionary<int, int>()
            {
                { 0, 11 },
                { 1, 11 },
                { 2, 11 },
                { 3, 11 },
                { 4, 11 },
                { 5, 11 },
                { 6, 11 }
            };
            for (DateTime date = new DateTime(2050, 10, 1); date <= new DateTime(2050, 10, 30); date = date.AddDays(1))
            {
                InputData.ValidateMockOutput.PromotionalCode.PromotionalCodesByDays.Add(
                    new PromotionalCodesByDay()
                    {
                        Date = date,
                        WeekDay = (int)date.DayOfWeek,
                        ReservationsCompleted = 10,
                        PromotionalCode_UID = 1
                    });
            }
        }

        public void ActiveLimit2MaxEqualsCompleted()
        {
            InputData.ValidateMockOutput.PromotionalCode.ActiveLimits = true;
            InputData.ValidateMockOutput.PromotionalCode.LimitType = 2;
            InputData.ValidateMockOutput.PromotionalCode.LimitWeekDays = new Dictionary<int, int>()
            {
                { 0, 10 },
                { 1, 10 },
                { 2, 10 },
                { 3, 10 },
                { 4, 10 },
                { 5, 10 },
                { 6, 10 }
            };
            for (DateTime date = new DateTime(2050, 10, 1); date < new DateTime(2050, 10, 30); date = date.AddDays(1))
            {
                InputData.ValidateMockOutput.PromotionalCode.PromotionalCodesByDays.Add(
                    new PromotionalCodesByDay()
                    {
                        Date = date,
                        WeekDay = (int)date.DayOfWeek,
                        ReservationsCompleted = 10,
                        PromotionalCode_UID = 1
                    });
            }
        }

        public void WithRoomCheckinLessThanValidFromCheckoutGreaterThanValidTo(bool isAssociatedToPromocode)
        {
            InputData.ValidateInput.ReservationRooms.Add(new ReservationRoomStayPeriod()
            {
                CheckIn = new DateTime(2050, 9, 29),
                CheckOut = new DateTime(2050, 11, 2),
                RateUID = 20509029
            });
            InputData.ValidateMockOutput.ValidRateUIDs.Add(20509029, isAssociatedToPromocode);
        }

        public void WithRoomCheckinEqualsValidTo(bool isAssociatedToPromocode)
        {
            InputData.ValidateInput.ReservationRooms.Add(new ReservationRoomStayPeriod()
            {
                CheckIn = new DateTime(2050, 10, 30),
                CheckOut = new DateTime(2050, 11, 2),
                RateUID = 20501030
            });
            InputData.ValidateMockOutput.ValidRateUIDs.Add(20501030, isAssociatedToPromocode);
        }

        public void WithRoomCheckoutEqualsValidFrom(bool isAssociatedToPromocode)
        {
            InputData.ValidateInput.ReservationRooms.Add(new ReservationRoomStayPeriod()
            {
                CheckIn = new DateTime(2050, 9, 28),
                CheckOut = new DateTime(2050, 10, 1),
                RateUID = 2050928
            });
            InputData.ValidateMockOutput.ValidRateUIDs.Add(2050928, isAssociatedToPromocode);
        }

        public void WithRoomInsidePeriod(bool isAssociatedToPromocode)
        {
            InputData.ValidateInput.ReservationRooms.Add(new ReservationRoomStayPeriod()
            {
                CheckIn = new DateTime(2050, 10, 10),
                CheckOut = new DateTime(2050, 10, 14),
                RateUID = 20501010
            });
            InputData.ValidateMockOutput.ValidRateUIDs.Add(20501010, isAssociatedToPromocode);
        }

        public void WithRoomOutsidePeriod(bool isAssociatedToPromocode)
        {
            InputData.ValidateInput.ReservationRooms.Add(new ReservationRoomStayPeriod()
            {
                CheckIn = new DateTime(2050, 9, 1),
                CheckOut = new DateTime(2050, 9, 5),
                RateUID = 205091
            });
            InputData.ValidateMockOutput.ValidRateUIDs.Add(205091, isAssociatedToPromocode);
        }

        public void WithRoomCheckinOutsideCheckoutInsidePeriod(bool isAssociatedToPromocode)
        {
            InputData.ValidateInput.ReservationRooms.Add(new ReservationRoomStayPeriod()
            {
                CheckIn = new DateTime(2050, 9, 30),
                CheckOut = new DateTime(2050, 10, 4),
                RateUID = 2050930
            });
            InputData.ValidateMockOutput.ValidRateUIDs.Add(2050930, isAssociatedToPromocode);
        }
    }

    public class PromotionalCodeInputData
    {
        public ValidatePromocodeForReservationParameters ValidateInput { get; set; }
        public ListPromotionalCodeForValidRatesResponse ValidateMockOutput { get; set; }

        public PromotionalCodeInputData()
        {
            ValidateInput = new ValidatePromocodeForReservationParameters()
            {
                PromocodeUID = 1,
                PromoCode = "TestsPromoCode",
                ReservationRooms = new List<ReservationRoomStayPeriod>(),
                OldAppliedDiscountDays = new List<DateTime>()
            };
            ValidateMockOutput = new ListPromotionalCodeForValidRatesResponse()
            {
                PromotionalCode = new PromotionalCode()
                {
                    UID = 1,
                    Name = "Testing Validation Promocode",
                    Code = "TestsPromoCode",
                    IsDeleted = false,
                    IsValid = true,
                    IsPercentage = true,
                    DiscountValue = 10,
                    ValidFrom = new DateTime(2050, 10, 01),
                    ValidTo = new DateTime(2050, 10, 30),
                    LimitWeekDays = new Dictionary<int, int>(),
                    PromotionalCodesByDays = new List<PromotionalCodesByDay>()
                },
                ValidRateUIDs = new Dictionary<long, bool>(),
                DiscountValueByCurrency = new Dictionary<long, decimal?>(),
                Status = Status.Success
            };
        }
    }
}
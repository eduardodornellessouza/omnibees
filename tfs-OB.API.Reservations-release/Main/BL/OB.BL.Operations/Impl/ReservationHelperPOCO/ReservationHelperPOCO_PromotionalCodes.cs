using OB.BL.Operations.Internal.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using OBcontractsRates = OB.BL.Contracts.Data.Rates;
using contracts = OB.Reservation.BL.Contracts.Data.Reservations;
using OB.DL.Common.QueryResultObjects;
using OB.BL.Operations.Interfaces;
using domainReservations = OB.Domain.Reservations;

namespace OB.BL.Operations.Impl
{
    public partial class ReservationHelperPOCO : BusinessPOCOBase, IReservationHelperPOCO
    {
        private OB.Log.ILogger loggerPromotionalCode = OB.Log.LogsManager.CreateLogger("ReservationHelperPromocode");

        public ValidPromocodeParameters ValidatePromocodeForReservation(ValidatePromocodeForReservationParameters parameters)
        {
            var promoCodeRepo = RepositoryFactory.GetOBPromotionalCodeRepository();

            // Get only valid rates
            var validateRatesRequest = new OB.BL.Contracts.Requests.ListPromotionalCodeForValidRatesRequest()
            {
                DateFrom = parameters.ReservationRooms.Min(x => x.CheckIn),
                DateTo = parameters.ReservationRooms.Max(x => x.CheckOut),
                RateUIDs = parameters.ReservationRooms.Select(x => x.RateUID).Distinct().ToList(),
                PromotionalCodeUID = parameters.PromocodeUID,
                PromotionalCode = parameters.PromoCode,
                CurrencyUID = parameters.CurrencyUID
            };
            var promoCodeWithValidRatesResponse = promoCodeRepo.ListPromotionalCodeForValidRates(validateRatesRequest);

            if (promoCodeWithValidRatesResponse == null)
                return new ValidPromocodeParameters();

            // If there is no promotional code on reservation
            if (!parameters.PromocodeUID.HasValue && string.IsNullOrEmpty(parameters.PromoCode))
            {
                var rr = parameters.ReservationRooms ?? new List<ReservationRoomStayPeriod>();
                return new ValidPromocodeParameters()
                {
                    ReservationRoomsPeriods = rr,
                    RejectReservation = promoCodeWithValidRatesResponse.ValidRateUIDs.Distinct().Count() != rr.GroupBy(x => x.RateUID).Count()
                };
            }

            return ValidatePromocodeForReservation(parameters.ReservationRooms, promoCodeWithValidRatesResponse.PromotionalCode, promoCodeWithValidRatesResponse.ValidRateUIDs,
                parameters.OldAppliedDiscountDays ?? new List<DateTime>());
        }

        private ValidPromocodeParameters ValidatePromocodeForReservation(List<ReservationRoomStayPeriod> reservationRooms, OBcontractsRates.PromotionalCode promocodeObj, Dictionary<long, bool> validRateUIDs, List<DateTime> oldAppliedDiscountDays)
        {
            var result = new ValidPromocodeParameters()
            {
                PromoCodeObj = promocodeObj,
            };

            // Refuse Reservation
            if (reservationRooms == null || validRateUIDs == null || !validRateUIDs.Any())
            {
                result.RejectReservation = true;
                return result;
            }

            // Accept Reservation without discounts
            if (promocodeObj == null)
            {
                result.ReservationRoomsPeriods = reservationRooms;
                return result;
            }

            // If any rates are exclusive for another promocode
            if (validRateUIDs.Distinct().Count() != reservationRooms.GroupBy(x => x.RateUID).Count())
            {
                // Accept Reservation without discounts
                if (!promocodeObj.IsValid)
                {
                    result.ReservationRoomsPeriods = reservationRooms;
                    return result;
                }

                // Refuse Reservation
                if (promocodeObj.IsPromotionalCodeVisibleRate == true)
                {
                    result.RejectReservation = true;
                    return result;
                }
            }

            // Build object to validate maximum limits of promocode
            result.OldDaysAppliedDiscount = oldAppliedDiscountDays;
            result.ReservationRoomsPeriods = reservationRooms.Select(x =>
            new ReservationRoomStayPeriod(validRateUIDs[x.RateUID])
            {
                RateUID = x.RateUID,
                CheckIn = x.CheckIn,
                CheckOut = x.CheckOut
            }).ToList();

            // Validate limits of promocode
            if (result.ReservationRoomsPeriods.Any())
                result.RejectReservation = !ValidatePromocodeLimits(result);

            // Accept Reservation and apply discounts if have any discount to apply
            return result;
        }

        private bool ValidatePromocodeLimits(ValidPromocodeParameters parameters)
        {
            var promoCodeObj = parameters.PromoCodeObj;
            parameters.NewDaysToApplyDiscount = new List<DateTime>();

            if (promoCodeObj == null)
                return false;

            // If promocode is Deleted, Inactive or No valid dates are configured
            if (promoCodeObj.IsDeleted || !promoCodeObj.IsValid || !promoCodeObj.ValidFrom.HasValue || !promoCodeObj.ValidTo.HasValue)
            {
                promoCodeObj.IsValid = false;
                return parameters.ReservationRoomsPeriods != null && parameters.ReservationRoomsPeriods.Any();
            }

            // Load promocode by day configurations to validate inside foreach
            bool haveLimitByDay = promoCodeObj.ActiveLimits == true && promoCodeObj.LimitWeekDays != null && promoCodeObj.LimitWeekDays.Any();
            var weekDaysCompleted = new Dictionary<DateTime, int>();
            if (haveLimitByDay && promoCodeObj.PromotionalCodesByDays != null)
                weekDaysCompleted = promoCodeObj.PromotionalCodesByDays.GroupBy(x => x.Date).Where(x => x.Key != null).ToDictionary(k => k.Key.Value, v => v.First().ReservationsCompleted ?? 0);

            // For each Reservation Room
            bool exclusiveAndInvalid = false;
            foreach (var reservationRoom in parameters.ReservationRoomsPeriods.Where(x => x.IsAssociatedToPromocode))
            {
                bool limitAreExceeded = false;

                // If reservation room is out of valid promocode period
                if (reservationRoom.CheckIn.Date > promoCodeObj.ValidTo.Value || reservationRoom.CheckOut.Date <= promoCodeObj.ValidFrom.Value)
                {
                    //If rate is exclusive for promocode then promocode is invalid
                    if (promoCodeObj.IsPromotionalCodeVisibleRate == true)
                    {
                        exclusiveAndInvalid = true;
                        break;
                    }

                    continue; // Ignore discounts
                }

                // Get days inside discount
                DateTime dateFrom = reservationRoom.CheckIn.Date;
                DateTime dateTo = reservationRoom.CheckOut.Date;
                if (reservationRoom.CheckIn.Date < promoCodeObj.ValidFrom.Value.Date)
                    dateFrom = promoCodeObj.ValidFrom.Value.Date;
                if (reservationRoom.CheckOut.Date > promoCodeObj.ValidTo.Value.Date)
                    dateTo = promoCodeObj.ValidTo.Value.Date.AddDays(1);

                // If no limits are configured
                if (promoCodeObj.ActiveLimits != true)
                {
                    for (DateTime date = dateFrom; date < dateTo; date = date.AddDays(1))
                        parameters.NewDaysToApplyDiscount.Add(date);
                    continue;
                }

                // Different Limit Types
                switch (promoCodeObj.LimitType)
                {
                    // Limit by Max Reservations Count
                    case (int)Constants.PromoCodeLimitType.MaxReservations:
                        int maxReservations = promoCodeObj.MaxReservations ?? 0;
                        int reservationsCompleted = (promoCodeObj.ReservationsCompleted ?? 0);

                        // If PromotionalCode discounts were not applied before to this room
                        if (parameters.OldDaysAppliedDiscount.Count <= 0)
                            reservationsCompleted++;

                        if (maxReservations >= reservationsCompleted)
                            for (DateTime date = dateFrom; date < dateTo; date = date.AddDays(1))
                                parameters.NewDaysToApplyDiscount.Add(date);
                        else
                            limitAreExceeded = true;
                        break;

                    // Limit by Max Reservations Count Per Week Day
                    case (int)Constants.PromoCodeLimitType.MaxReservationsPerWeekDay:
                        if (haveLimitByDay)
                        {
                            var discountDays = new List<DateTime>();
                            for (DateTime date = dateFrom; date < dateTo; date = date.AddDays(1))
                            {
                                // If no limits are set for this day then doesn't apply discount for any day
                                int maxResByDay;
                                if (!promoCodeObj.LimitWeekDays.TryGetValue((int)date.DayOfWeek, out maxResByDay))
                                {
                                    limitAreExceeded = true;
                                    break;
                                }

                                int reservationsCompletedByDay;
                                weekDaysCompleted.TryGetValue(date, out reservationsCompletedByDay);

                                // If discount was not applied previously to this day
                                if (!parameters.OldDaysAppliedDiscount.Contains(date))
                                    reservationsCompletedByDay++;

                                if (maxResByDay >= reservationsCompletedByDay)
                                    discountDays.Add(date);
                                else
                                {
                                    limitAreExceeded = true;
                                    break;
                                }
                            }

                            if (!limitAreExceeded && discountDays.Any())
                                parameters.NewDaysToApplyDiscount.AddRange(discountDays);
                        }
                        break;
                }

                // If rate is exclusive for promocode and any limit exceeded then promocode is invalid
                if (limitAreExceeded && promoCodeObj.IsPromotionalCodeVisibleRate == true)
                {
                    exclusiveAndInvalid = true;
                    parameters.NewDaysToApplyDiscount.Clear();
                    break;
                }
            }

            if (exclusiveAndInvalid)
            {
                parameters.PromoCodeObj.IsValid = false;
                return false;
            }

            parameters.NewDaysToApplyDiscount = parameters.NewDaysToApplyDiscount.Distinct().ToList();
            return true;
        }

        public void UpdatePromoCodeReservationsCompleted(long promotionalCodeUID, IEnumerable<DateTime> oldDays = null, IEnumerable<DateTime> newDays = null)
        {
            if ((oldDays == null || !oldDays.Any()) && (newDays == null || !newDays.Any()))
                return;

            var promoCodeRepo = RepositoryFactory.GetOBPromotionalCodeRepository();
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);

            Dictionary<DateTime, int> changedPromocodeDays = null;

            // Get Promotional Code
            var promoCodeRequest = new OB.BL.Contracts.Requests.ListPromotionalCodeRequest()
            {
                PromoCodeIds = new List<long>() { promotionalCodeUID },
                PageSize = 1,
                PageIndex = 0
            };
            var promotionalCode = promoCodeRepo.ListPromotionalCode(promoCodeRequest).FirstOrDefault();
            if (promotionalCode == null)
            {
                loggerPromotionalCode.Warn(new Dictionary<string, object>() {
                    { "MethodName:", "UpdatePromoCodeReservationsCompleted" },
                    { "PromotionalCodeUID:", promotionalCodeUID } },
                    "This PromotionalCode with active limits doesn't exists.");
                return;
            }

            oldDays = oldDays == null ? new List<DateTime>() : oldDays.Distinct();
            newDays = newDays == null ? new List<DateTime>() : newDays.Distinct();

            if (!oldDays.Any() && newDays.Any())
                changedPromocodeDays = newDays.ToDictionary(k => k, v => 1);
            else if (oldDays.Any() && !newDays.Any())
                changedPromocodeDays = oldDays.ToDictionary(k => k, v => -1);
            else
            {
                var removedPromocodeDays = oldDays.Except(newDays).ToDictionary(k => k.Date, v => -1);
                var addedPromocodeDays = newDays.Except(oldDays).ToDictionary(k => k.Date, v => 1);
                changedPromocodeDays = removedPromocodeDays.Concat(addedPromocodeDays).ToDictionary(k => k.Key, v => v.Value);
            }

            string query = string.Empty;
            switch (promotionalCode.LimitType)
            {
                // Increment or decrement general ReservationCompleted
                case (int)Constants.PromoCodeLimitType.MaxReservations:
                    // If is modify then ignore reservation completed for type 1
                    if (changedPromocodeDays.ContainsValue(1) && changedPromocodeDays.ContainsValue(-1))
                        return;

                    int addCompleted = changedPromocodeDays.ContainsValue(1) ? 1 : -1;
                    promotionalCode.ReservationsCompleted = (promotionalCode.ReservationsCompleted ?? 0) + addCompleted;
                    query += string.Format("UPDATE PromotionalCodes SET ReservationsCompleted = {0} WHERE UID = {1}", promotionalCode.ReservationsCompleted, promotionalCode.UID);
                    break;

                // Increment or decrement ReservationsCompleted per day
                case (int)Constants.PromoCodeLimitType.MaxReservationsPerWeekDay:
                    UpsertPromotionalCodesByDays(promotionalCodeUID, changedPromocodeDays, ref query);
                    break;
            }

            if (!string.IsNullOrEmpty(query))
                sqlManager.ExecuteSql(query);
        }


        private static readonly string UPDATE_PROMOTIONALCODESBYDAY = @"UPDATE PromotionalCodesByDay SET ReservationsCompleted = {0}, IsDeleted = '{1}', ModifiedDate = '{2}' WHERE PromotionalCode_UID = {3} and Date = '{4}';";

        private static readonly string INSERT_PROMOTIONALCODESBYDAY = @"INSERT PromotionalCodesByDay (PromotionalCode_UID, WeekDay, Date, ReservationsCompleted, IsDeleted, ModifiedDate) VALUES ({0},{1},'{2}',{3},'{4}','{5}');";


        /// <summary>
        /// Update or Insert if doesn't exists into PromotionalCodesByDays. This method will increment or decrement reservation completed.
        /// Minimun value of ReservationCompleted is zero.
        /// </summary>
        /// <param name="promotionalCodeUID"></param>
        /// <param name="dateVsIncrementCompleted">
        /// Key - Date.
        /// Value - Adds the specified number to ReservationCompleted value. If specified number is negative then decrements the ReservationCompleted value.
        /// </param>
        private bool UpsertPromotionalCodesByDays(long promotionalCodeUID, Dictionary<DateTime, int> dateVsIncrementCompleted, ref string query)
        {
            if (dateVsIncrementCompleted == null || !dateVsIncrementCompleted.Any())
                return false;

            var promoCodeByDayRepo = RepositoryFactory.GetOBPromotionalCodeRepository();

            var dates = dateVsIncrementCompleted.Select(x => x.Key.Date).ToList();
            var utcNow = DateTime.UtcNow;

            // Update days
            var response = promoCodeByDayRepo.ListPromotionalCodesByDay(new OB.BL.Contracts.Requests.ListPromotionalCodesByDayRequest { PromotionalCodeUID = promotionalCodeUID, Dates = dates });
            var promoDaysToUpdate = new List<OBcontractsRates.PromotionalCodesByDay>();

            if (response.Status != Contracts.Responses.Status.Success)
            {
                var error = response.Errors.FirstOrDefault();
                var message = error != null ? $"OB.API {nameof(UpsertPromotionalCodesByDays)} - {error.ErrorCode} { error.Description}" : $"OB.API {nameof(UpsertPromotionalCodesByDays)}";
                throw new ApplicationException(message);
            }

            promoDaysToUpdate = response.Result;

            if (promoDaysToUpdate.Any())
            {
                foreach (var dayToUpdate in promoDaysToUpdate)
                {
                    if (!dayToUpdate.Date.HasValue)
                        continue;

                    int increment = dateVsIncrementCompleted[dayToUpdate.Date.Value];
                    if (increment == 0)
                        continue;

                    if (dayToUpdate.IsDeleted.HasValue && dayToUpdate.IsDeleted.Value)
                    {
                        loggerPromotionalCode.Warn(new Dictionary<string, object>() {
                            { "MethodName:", "UpsertPromotionalCodesByDays" },
                            { "PromocodeUID:", promotionalCodeUID },
                            { "Date:", dayToUpdate.Date } },
                            "This Updated PromotionalCodeByDay Row Was Deleted.");
                        dayToUpdate.IsDeleted = false;
                        dayToUpdate.ReservationsCompleted = 0;
                    }

                    dayToUpdate.ModifiedDate = utcNow;
                    dayToUpdate.ReservationsCompleted = (dayToUpdate.ReservationsCompleted ?? 0) + dateVsIncrementCompleted[dayToUpdate.Date.Value];
                    if (dayToUpdate.ReservationsCompleted < 0)
                        dayToUpdate.ReservationsCompleted = 0;

                    dates.Remove(dayToUpdate.Date.Value);

                    query += string.Format(UPDATE_PROMOTIONALCODESBYDAY, dayToUpdate.ReservationsCompleted, dayToUpdate.IsDeleted, 
                        dayToUpdate.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss"), promotionalCodeUID, dayToUpdate.Date.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }

            // Insert reamining
            if (dates.Any())
                foreach (var dayToInsert in dates)
                {
                    var resCompleted = dateVsIncrementCompleted[dayToInsert.Date];
                    if (resCompleted <= 0)
                    {
                        loggerPromotionalCode.Warn(new Dictionary<string, object>() {
                            { "MethodName:", "UpsertPromotionalCodesByDays" },
                            { "PromocodeUID:", promotionalCodeUID},
                            { "Date:", dayToInsert.Date } },
                            "The number of reservation completed to insert is less than zero.");
                        continue;
                    }

                    var promoCodeDayToInsert = new OB.BL.Contracts.Data.Rates.PromotionalCodesByDay()
                    {
                        PromotionalCode_UID = promotionalCodeUID,
                        WeekDay = (int)dayToInsert.Date.DayOfWeek,
                        Date = dayToInsert.Date,
                        ReservationsCompleted = resCompleted,
                        IsDeleted = false,
                        ModifiedDate = utcNow,
                    };

                    query += string.Format(INSERT_PROMOTIONALCODESBYDAY, promoCodeDayToInsert.PromotionalCode_UID, promoCodeDayToInsert.WeekDay, 
                        promoCodeDayToInsert.Date.Value.ToString("yyyy-MM-dd HH:mm:ss"), promoCodeDayToInsert.ReservationsCompleted, promoCodeDayToInsert.IsDeleted, promoCodeDayToInsert.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                }

            return true;
        }
    }
}
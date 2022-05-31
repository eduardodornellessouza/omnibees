using OB.BL.Operations.Extensions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Responses;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using static OB.Reservation.BL.Constants;
using domainReservations = OB.Domain.Reservations;
using OBcontractsRates = OB.BL.Contracts.Data.Rates;

namespace OB.BL.Operations.Impl
{
    public class ReservationPricesCalculationPOCO : BusinessPOCOBase, IReservationPricesCalculationPOCO
    {
        /// <summary>
        /// Calculate Prices
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="rateId"></param>
        /// <param name="roomTypeId"></param>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <param name="channelId"></param>
        /// <param name="baseCurrencyId"></param>
        /// <param name="adultCount"></param>
        /// <param name="childCount"></param>
        /// <param name="ages"></param>
        /// <param name="childTerms"></param>
        /// <param name="promotionalCodeId"></param>
        /// <param name="tpiId"></param>
        /// <param name="promotionalCode"></param>
        /// <param name="isModify"></param>
        /// <returns></returns>
        public List<OBcontractsRates.RateRoomDetailReservation> CalculateReservationRoomPrices(CalculateFinalPriceParameters parameters)
        {
            var rateBuyerGroupRepo = RepositoryFactory.GetOBRateBuyerGroupRepository();
            OBcontractsRates.RateBuyerGroup validBuyerGroup = null;

            if (parameters.ChildTerms == null || !parameters.ChildTerms.Any())
            {
                var childTermsRepo = RepositoryFactory.GetOBChildTermsRepository();
                var childTermsResponse = childTermsRepo.ListChildTerms(new OB.BL.Contracts.Requests.ListChildTermsRequest
                {
                    PropertyUIDs = new List<long>() { parameters.PropertyId },
                    IncludeChildTermCurrencies = true
                });

                if (childTermsResponse.Status == OB.BL.Contracts.Responses.Status.Success)
                    parameters.ChildTerms = childTermsResponse.Result;
            }

            // Get Incentives
            var incentivesList = GetIncentivesForReservationRoom(parameters.CheckIn, parameters.CheckOut, parameters.RateId, parameters.GroupRule);

            // Get RateRoomDetails
            var rrdList = GetRateRoomDetailsForReservationRoom(parameters.PropertyId,
                                                                        parameters.RateId,
                                                                        parameters.RoomTypeId,
                                                                        parameters.BaseCurrency,
                                                                        parameters.ChannelId,
                                                                        parameters.CheckIn,
                                                                        parameters.CheckOut,
                                                                        parameters.AdultCount,
                                                                        parameters.ChildCount,
                                                                        parameters.Ages,
                                                                        parameters.ChildTerms,
                                                                        parameters.RateModelId);

            if (!rrdList.Any())
                throw Errors.RateRoomDetailsAreNotSet.ToBusinessLayerException();

            // Get Buyer Group
            if (parameters.TpiId.HasValue)
            {
                validBuyerGroup = rateBuyerGroupRepo.GetRateBuyerGroup(parameters.RateId, parameters.TpiId.Value);
                if (validBuyerGroup != null && parameters.GroupRule != null)
                {
                    if (parameters.GroupRule?.BusinessRules.HasFlag(Domain.Reservations.BusinessRules.GDSBuyerGroup) == true &&
                        (validBuyerGroup.TPIType == (int)Constants.TPIType.TravelAgent
                        || validBuyerGroup.TPIType == (int)Constants.TPIType.TravelAgencyOffice))
                    {
                        parameters.TPIDiscountIsPercentage = validBuyerGroup.GDSValueIsPercentage;
                        parameters.TPIDiscountIsValueDecrease = validBuyerGroup.GDSValueIsDecrease;
                        parameters.TPIDiscountValue = validBuyerGroup.GDSValue;
                    }
                    else
                    {
                        parameters.TPIDiscountIsPercentage = validBuyerGroup.IsPercentage;
                        parameters.TPIDiscountIsValueDecrease = validBuyerGroup.IsValueDecrease;
                        parameters.TPIDiscountValue = validBuyerGroup.Value;
                    }
                }
            }

            parameters.RrdList = rrdList;
            parameters.Incentives = incentivesList;
            parameters.RateBuyer = validBuyerGroup;

            //Calculate Final Prices
            rrdList = CalculateFinalPrices(parameters);

            return rrdList;
        }

        /// <summary>
        /// Calculate final prices per day
        /// </summary>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <param name="ages"></param>
        /// <param name="rrdList">list of rate room details</param>
        /// <param name="promoCode"></param>
        /// <param name="incentives">list of valid incentives</param>
        /// <param name="childTerms">list of child terms to be applied</param>
        /// <param name="rateBuyer"></param>
        /// <param name="baseCurrency"></param>
        /// <param name="adultCount"></param>
        /// <param name="childCount"></param>
        /// <param name="ruleType"></param>
        /// <param name="exchangeRate"></param>
        /// <param name="isModify"></param>
        /// <returns>update list of rate room details</returns>
        private List<OBcontractsRates.RateRoomDetailReservation> CalculateFinalPrices(CalculateFinalPriceParameters parameters)
        {
            // Extra Bed Price
            if (parameters.GroupRule?.BusinessRules.HasFlag(domainReservations.BusinessRules.CalculateExtraBedPrice) == true)
                parameters.RrdList = CalculateExtraBedPrice(parameters.RrdList, parameters.AdultCount, parameters.ChildCount, parameters.Ages, parameters.BaseCurrency, parameters.ChildTerms);

            // Child Terms
            if (parameters.ChildTerms != null && parameters.ChildTerms.Any())
                parameters.RrdList = CalculateChildTerms(parameters.RrdList, parameters.AdultCount, parameters.ChildCount, parameters.Ages, parameters.BaseCurrency, parameters.ChildTerms);

            // Price Add on
            parameters.RrdList = CalculatePriceAddOn(parameters.RrdList);

            // Agencies
            parameters.RrdList = CalculatePriceRateBuyerGroup(parameters.RrdList, parameters.RateBuyer, parameters.GroupRule);

            // Incentives
            parameters.RrdList = CalculateIncentives(parameters.RrdList, parameters.Incentives, parameters.CheckIn, parameters.CheckOut, parameters.GroupRule);

            // PromoCodes
            parameters.RrdList = CalculatePricePromotionalCode(parameters.RrdList, parameters.ValidPromocodeParameters);

            // Loyalty Level
            parameters.RrdList = CalculatePriceWithLoyaltyLevel(parameters.RrdList, parameters.LoyaltyProgram, parameters.BaseCurrency);

            // RateModel
            parameters.RrdList = CalculatePriceModel(parameters.RrdList, parameters.GroupRule);

            // External Markup
            parameters.RrdList = CalculateExternalMarkup(parameters.RrdList, parameters.GroupRule, parameters.ExternalMarkupRule);

            // Convert To Property Currency
            foreach (var rrd in parameters.RrdList)
                rrd.FinalPrice = Math.Round(rrd.FinalPrice * parameters.ExchangeRate, 4, MidpointRounding.AwayFromZero);

            return parameters.RrdList;
        }

        /// <summary>
        /// Get Incentives matching the parameters
        /// </summary>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <param name="rateId"></param>
        /// <returns></returns>
        public List<OB.BL.Contracts.Data.Properties.Incentive> GetIncentivesForReservationRoom(DateTime checkIn, DateTime checkOut, long rateId, GroupRule groupRule)
        {
            var incentiveRepo = RepositoryFactory.GetOBIncentiveRepository();

            var incentives = new List<OB.BL.Contracts.Data.Properties.Incentive>();

            if (groupRule?.BusinessRules.HasFlag(BusinessRules.CalculateStayWindowWeekDays) == true && OB.DL.Common.Configuration.EnableNewOffers)
            {
                var response = incentiveRepo.ListIncentivesWithBookingAndStayPeriodsForReservationRoom(new OB.BL.Contracts.Requests.ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest
                {
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    RateIds = new List<long> { rateId }
                });
                if (response.Status == Contracts.Responses.Status.Success)
                    incentives = response.Result?.Where(x => x.Value.Count() > 1)
                        .SelectMany(x => x.Value.Where(j => j.StayWindowWeekDays != null)).ToList();
            }
            else
            {
                incentives = incentiveRepo.ListIncentivesForReservationRoom(new OB.BL.Contracts.Requests.ListIncentivesForReservationRoomRequest { CheckIn = checkIn, CheckOut = checkOut, RateId = rateId, FreeNights = true });
                incentives.AddRange(incentiveRepo.ListIncentivesForReservationRoom(new OB.BL.Contracts.Requests.ListIncentivesForReservationRoomRequest { CheckIn = checkIn, CheckOut = checkOut, RateId = rateId, FreeNights = false }));
            }

            return incentives;
        }

        /// <summary>
        /// Get Rate room detail matching the parameters
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="rateId"></param>
        /// <param name="roomTypeId"></param>
        /// <param name="reservationBaseCurrency"></param>
        /// <param name="channelId"></param>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <param name="adultCount"></param>
        /// <param name="childCount"></param>
        /// <param name="ages"></param>
        /// <param name="childTerms"></param>
        /// <param name="rateModelId"></param>
        /// <returns></returns>
        public List<OBcontractsRates.RateRoomDetailReservation> GetRateRoomDetailsForReservationRoom(long propertyId, long rateId, long roomTypeId, long reservationBaseCurrency, long channelId,
            DateTime checkIn, DateTime checkOut, int adultCount, int childCount, List<int> ages, List<OB.BL.Contracts.Data.Properties.ChildTerm> childTerms, int? rateModelId)
        {
            var rrdRepo = RepositoryFactory.GetOBRateRoomDetailsForReservationRoomRepository();

            var childTermsForSearch = GetGuestCountAfterApplyChildTerms(reservationBaseCurrency, childTerms, childCount, ages);
            var realOccupancy = CalculateRealOccupancy(childTermsForSearch, adultCount);

            var result = rrdRepo.ListRateRoomDetailForReservationRoom(new OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest
            {
                CheckIn = checkIn,
                CheckOut = checkOut,
                PropertyId = propertyId,
                RateId = rateId,
                RoomtypeId = roomTypeId,
                AdultCount = realOccupancy.AdultCount,
                ChildCount = realOccupancy.ChildCountForPrice,
                FreeChildCount = realOccupancy.FreeChildCount,
                ChannelId = channelId,
                RateModelUID = rateModelId
            });

            return result;
        }

        /// <summary>
        /// Calculate Real Occupancy
        /// </summary>
        /// <param name="childTerms"></param>
        /// <param name="adultCount"></param>
        /// <returns></returns>
        private OccupancyAfterChildTerms CalculateRealOccupancy(List<ChildTermsOccupancy> childTerms, int adultCount)
        {
            var result = new OccupancyAfterChildTerms { AdultCount = adultCount };

            if (childTerms == null || !childTerms.Any()) return result;

            result.AdultCount = adultCount + childTerms.Where(c => c.PriceType == ChildPriceType.Adult).Sum(c => c.NumberOfChilds);
            result.ChildCountForPrice = childTerms.Where(c => c.PriceType == ChildPriceType.Child).Sum(c => c.NumberOfChilds);
            result.ChildCountForOccupancy = childTerms.Where(c => c.IsAccountableForOccupancy).Sum(c => c.NumberOfChilds);
            result.FreeChildCount = childTerms.Where(c => c.PriceType == ChildPriceType.Free).Sum(c => c.NumberOfChilds);

            return result;
        }

        List<OBcontractsRates.RateRoomDetailReservation> CalculateExtraBedPrice(List<OBcontractsRates.RateRoomDetailReservation> rrdList, int adultCount, int childCount, List<int> ages,
            long? reservationBaseCurrency, List<OB.BL.Contracts.Data.Properties.ChildTerm> childTerms)
        {
            var childTermsForSearch = GetGuestCountAfterApplyChildTerms(reservationBaseCurrency, childTerms, childCount, ages);
            var realOccupancy = CalculateRealOccupancy(childTermsForSearch, adultCount);

            if (!rrdList.All(x => x.AcceptsExtraBed && x.AdultMaxOccupancy == realOccupancy.AdultCount))
                return rrdList;

            rrdList.ForEach(x => x.AdultPrice += x.ExtraBedPrice);

            return rrdList;
        }

        /// <summary>
        /// Update rrdList child prices based on parameters
        /// </summary>
        /// <param name="ages"></param>
        /// <param name="reservationBaseCurrency"></param>
        /// <param name="rrdList"></param>
        /// <param name="childTerms"></param>
        /// <param name="adultCount"></param>
        /// <param name="childCount"></param>
        /// <returns></returns>
        private List<OBcontractsRates.RateRoomDetailReservation> CalculateChildTerms(List<OBcontractsRates.RateRoomDetailReservation> rrdList, int adultCount, int childCount, List<int> ages,
            long? reservationBaseCurrency, List<OB.BL.Contracts.Data.Properties.ChildTerm> childTerms)
        {
            if (childTerms != null && childTerms.Any())
            {
                var childTermsForSearch = GetGuestCountAfterApplyChildTerms(reservationBaseCurrency, childTerms, childCount, ages);
                var realOccupancy = CalculateRealOccupancy(childTermsForSearch, adultCount);

                foreach (var rrd in rrdList)
                {
                    // Update Childs that count for price based on room mx free childs
                    var childCountForPriceCopy = realOccupancy.ChildCountForPrice;
                    List<ChildTermsOccupancy> childTermsCopy = childTermsForSearch.Clone();
                    if (childTermsForSearch.Any(x => x.PriceType == ChildPriceType.Free))
                    {
                        var freeChildsToRemove = childTermsCopy.Where(x => x.PriceType == ChildPriceType.Free).Sum(x => x.NumberOfChilds) - (rrd.MaxFreeChilds ?? int.MaxValue);
                        if (freeChildsToRemove > 0)
                        {
                            childCountForPriceCopy = childCountForPriceCopy + freeChildsToRemove;

                            childTermsCopy.Add(new ChildTermsOccupancy
                            {
                                NumberOfChilds = freeChildsToRemove,
                                PriceType = ChildPriceType.Child
                            });
                        }
                    }

                    decimal childPricePerOccupancy = rrd.ChildPrice;
                    rrd.ChildPrice = 0;
                    decimal childPricePerChild = childCountForPriceCopy > 0 ? childPricePerOccupancy / childCountForPriceCopy : 0;

                    foreach (var childTerm in childTermsCopy)
                    {
                        switch (childTerm.PriceType)
                        {
                            case (ChildPriceType.Free): // free doesnt count
                                rrd.ChildPrice += 0;
                                break;

                            case (ChildPriceType.Adult): // adult is included on adults price
                                rrd.ChildPrice += 0;
                                break;

                            default:
                                // add variations if exists
                                var childPriceVariation = childTerm.PriceVariations.FirstOrDefault(p => p.Currency_UID == reservationBaseCurrency);

                                if (childPriceVariation != null && childPriceVariation.PriceVariation > 0)
                                {
                                    decimal priceVariation = childPriceVariation.IsValueDecrease ? -(childPriceVariation.PriceVariation) : childPriceVariation.PriceVariation;

                                    if (childPriceVariation.IsPriceVariationPerc)
                                        rrd.ChildPrice += (childPricePerChild + ((childPricePerChild * priceVariation) / (decimal)100.0)) * childTerm.NumberOfChilds;
                                    else
                                        rrd.ChildPrice += (childPricePerChild + priceVariation) * childTerm.NumberOfChilds;
                                }
                                else
                                    rrd.ChildPrice += childPricePerChild * childTerm.NumberOfChilds;
                                break;
                        }
                    }
                }
            }

            return rrdList;
        }

        /// <summary>
        /// Calculate Price AddOn
        /// </summary>
        /// <param name="rrdList"></param>
        /// <returns></returns>
        private List<OBcontractsRates.RateRoomDetailReservation> CalculatePriceAddOn(List<OBcontractsRates.RateRoomDetailReservation> rrdList)
        {
            foreach (var rrd in rrdList)
            {
                decimal price = rrd.AdultPrice + rrd.ChildPrice;

                var priceAddOnValueFixed = (rrd.PriceAddOnValue ?? 0);
                if (rrd.PriceAddOnIsPercentage != null && rrd.PriceAddOnIsValueDecrease != null && rrd.PriceAddOnValue != null)
                {
                    if (rrd.PriceAddOnIsPercentage == true)
                        priceAddOnValueFixed = price * (priceAddOnValueFixed / 100);

                    if (rrd.PriceAddOnIsValueDecrease == true)
                        priceAddOnValueFixed = priceAddOnValueFixed * -1;

                    rrd.PriceAddOnValue = priceAddOnValueFixed;
                    price = price + (decimal)rrd.PriceAddOnValue;
                }

                rrd.PriceAfterAddOn = price;
                rrd.FinalPrice = price;
            }

            return rrdList;
        }

        /// <summary>
        /// Calculate buyer group price
        /// </summary>
        /// <param name="rrdList"></param>
        /// <param name="buyerGroup"></param>
        /// <param name="ruleType"></param>
        /// <returns></returns>
        private List<OBcontractsRates.RateRoomDetailReservation> CalculatePriceRateBuyerGroup(List<OBcontractsRates.RateRoomDetailReservation> rrdList, OBcontractsRates.RateBuyerGroup buyerGroup,
            domainReservations.GroupRule groupRule)
        {
            foreach (var rrd in rrdList)
            {
                var price = rrd.PriceAfterAddOn;

                if (buyerGroup == null || buyerGroup.Value == null)
                {
                    rrd.PriceAfterBuyerGroups = price;
                    continue;
                }

                decimal buyerValue;
                bool isPercentage;
                bool isValueDecrease;
                if (groupRule.BusinessRules.HasFlag(Domain.Reservations.BusinessRules.GDSBuyerGroup) &&
                        (buyerGroup.TPIType == (int)Constants.TPIType.TravelAgent
                        || buyerGroup.TPIType == (int)Constants.TPIType.TravelAgencyOffice))
                {
                    isPercentage = buyerGroup.GDSValueIsPercentage;
                    isValueDecrease = buyerGroup.GDSValueIsDecrease;
                    buyerValue = buyerGroup.GDSValue ?? 0;
                }
                else
                {
                    isPercentage = buyerGroup.IsPercentage;
                    isValueDecrease = buyerGroup.IsValueDecrease;
                    buyerValue = buyerGroup.Value ?? 0;
                }

                if (isPercentage)
                    buyerValue = price * (buyerValue / 100M);

                if (isValueDecrease)
                    buyerValue = buyerValue * -1M;

                price = price + buyerValue;



                rrd.PriceAfterBuyerGroups = price;
                rrd.FinalPrice = price;
            }

            return rrdList;
        }

        public List<OBcontractsRates.RateRoomDetailReservation> CalculatePriceWithLoyaltyLevel(List<OBcontractsRates.RateRoomDetailReservation> rrdList,
            OB.BL.Contracts.Data.CRM.LoyaltyProgram loyaltyProgram, long baseCurrencyId)
        {
            if (loyaltyProgram == null || rrdList.Count <= 0)
            {
                rrdList.ForEach(x => x.PriceAfterLoyaltyLevel = x.PriceAfterPromoCodes);
                return rrdList;
            }

            var rrdFirst = rrdList.First();

            var loyaltyLevel = loyaltyProgram.LoyaltyLevels.FirstOrDefault();
            var loyaltyRatesIds = loyaltyLevel == null ? Enumerable.Empty<long>() : loyaltyLevel.RateLoyaltyLevels.Select(x => x.Rate_UID);

            if (loyaltyLevel == null || loyaltyLevel.DiscountValue <= 0 || !loyaltyRatesIds.Contains(rrdFirst.Rate_UID))
            {
                rrdList.ForEach(x => x.PriceAfterLoyaltyLevel = x.PriceAfterPromoCodes);
                return rrdList;
            }

            if (loyaltyLevel.IsPercentage)
            {
                foreach (var rateRoomDetail in rrdList)
                {
                    rateRoomDetail.PriceAfterLoyaltyLevel = rateRoomDetail.PriceAfterPromoCodes - ((rateRoomDetail.PriceAfterPromoCodes * loyaltyLevel.DiscountValue) / 100);
                    rateRoomDetail.FinalPrice = rateRoomDetail.PriceAfterLoyaltyLevel;
                }
            }
            else
            {
                decimal discountPerDay = 0;
                var freeDays = rrdList.Where(x => x.PriceAfterPromoCodes <= 0).Count();
                var daysToApplyDiscount = rrdList.Count - freeDays;
                if (daysToApplyDiscount > 0)
                {
                    if (loyaltyProgram.DefaultCurrency.UID == baseCurrencyId)
                        discountPerDay = loyaltyLevel.DiscountValue;
                    else
                    {
                        var loyaltyCurrency = loyaltyLevel.LoyaltyLevelsCurrencies.FirstOrDefault(x => x.Currency_UID == baseCurrencyId);
                        if (loyaltyCurrency == null)
                            discountPerDay = 0;
                        else
                            discountPerDay = loyaltyCurrency.Value;
                    }

                    discountPerDay = discountPerDay / daysToApplyDiscount;

                    foreach (var rateRoomDetail in rrdList)
                    {
                        var price = rateRoomDetail.PriceAfterPromoCodes - discountPerDay;
                        rateRoomDetail.PriceAfterLoyaltyLevel = price < 0 ? 0 : price;
                        rateRoomDetail.FinalPrice = rateRoomDetail.PriceAfterLoyaltyLevel;
                    }
                }
            }

            return rrdList;
        }

        /// <summary>
        /// Calculate incentives discounts
        /// </summary>
        /// <param name="rrdList"></param>
        /// <param name="incentives"></param>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <returns></returns>
        private List<OBcontractsRates.RateRoomDetailReservation> CalculateIncentives(List<OBcontractsRates.RateRoomDetailReservation> rrdList, List<OB.BL.Contracts.Data.Properties.Incentive> incentives, DateTime checkIn, DateTime checkOut, GroupRule groupRule)
        {
            var workedIncentives = incentives;

            // Non Cumulative
            var nonCumulativeIncentives = new List<OB.BL.Contracts.Data.Properties.Incentive>();
            decimal totalNonCumulative = 0;
            var tempGrouppedIncentives = workedIncentives.FindAll(p => p.IsCumulative != true).GroupBy(p => p.UID).OrderBy(x => x.Key);

            foreach (var items in tempGrouppedIncentives)
            {
                var auxNonCumulativeIncentives = CalculateIncentives(false, checkIn, checkOut, rrdList, items.ToList(), groupRule);
                var auxTotal = auxNonCumulativeIncentives.Sum(p => p.TotalDiscounted);

                if (totalNonCumulative >= auxTotal) continue;

                nonCumulativeIncentives = auxNonCumulativeIncentives;
                totalNonCumulative = auxTotal;
            }

            // Cumulative
            var cumulativeIncentives = CalculateIncentives(true, checkIn, checkOut, rrdList, workedIncentives.FindAll(p => p.IsCumulative == true).ToList(), groupRule);
            decimal totalCumulative = 0;
            if (cumulativeIncentives.Count > 0)
                cumulativeIncentives.ForEach(p => totalCumulative += p.TotalDiscounted);

            // no incentives and no discount to apply
            if (totalNonCumulative <= 0 && totalCumulative <= 0)
            {
                foreach (var item in rrdList)
                {
                    item.PriceAfterIncentives = item.PriceAfterBuyerGroups;
                    item.FinalPrice = item.PriceAfterIncentives;
                }

                return rrdList;
            }

            var isNonCumulativeBest = totalNonCumulative >= totalCumulative;
            for (int i = 0; i < rrdList.Count; i++)
            {
                var best = new OB.BL.Contracts.Data.Properties.Incentive();
                var appliedIncentives = new List<OB.BL.Contracts.Data.Properties.Incentive>();
                if (isNonCumulativeBest)
                {
                    //Best incentive is non cumulative
                    decimal dayNonCumulative = 0;
                    foreach (var item in nonCumulativeIncentives)
                    {
                        if (dayNonCumulative < item.DayDiscount[i])
                            best = item;

                        dayNonCumulative = item.DayDiscount[i];
                    }

                    if (best.TotalDiscounted > 0)
                        appliedIncentives.Add(best);
                }
                else
                {
                    //Best incentive are the cumulatives
                    best = new OB.BL.Contracts.Data.Properties.Incentive();
                    //if (!best.DayDiscount.Any()) - not needed
                    best.DayDiscount = new List<decimal>();  //added
                    while (best.DayDiscount.Count < rrdList.Count)
                        best.DayDiscount.Add(0);

                    foreach (var item in cumulativeIncentives)
                    {
                        best.TotalDiscounted += item.DayDiscount[i];
                        best.DayDiscount[i] += item.DayDiscount[i];

                        if (item.DayDiscount[i] > 0)
                            appliedIncentives.Add(item);
                    }

                }

                if (best.DayDiscount != null && best.DayDiscount.Any())  //added verification
                {
                    rrdList[i].PriceAfterIncentives = rrdList[i].PriceAfterBuyerGroups - best.DayDiscount[i];
                    rrdList[i].FinalPrice = rrdList[i].PriceAfterIncentives;
                }
                else
                {
                    rrdList[i].PriceAfterIncentives = rrdList[i].PriceAfterBuyerGroups;
                    rrdList[i].FinalPrice = rrdList[i].PriceAfterIncentives;
                }

                rrdList[i].AppliedIncentives = appliedIncentives;
            }

            return rrdList;
        }

        private List<OB.BL.Contracts.Data.Properties.Incentive> CalculateIncentives(bool useCumulativeRules, DateTime checkIn, DateTime checkOut, List<OBcontractsRates.RateRoomDetailReservation> rrList, List<OB.BL.Contracts.Data.Properties.Incentive> incentivesList, GroupRule groupRule)
        {
            DateTime riDateFrom;
            DateTime riDateTo;
            decimal priceDiscounted;
            decimal price;

            foreach (var rrd in rrList)
            {
                price = rrd.PriceAfterBuyerGroups;

                foreach (var item in incentivesList)
                {
                    priceDiscounted = 0;

                    var dayOfWeekChecked = false;
                    if (groupRule.BusinessRules.HasFlag(BusinessRules.CalculateStayWindowWeekDays)
                        && OB.DL.Common.Configuration.EnableNewOffers
                        && (item.StayWindowWeekDays?.TryGetValue(rrd.Date.DayOfWeek, out dayOfWeekChecked) == false || !dayOfWeekChecked))
                    {
                        ApplyDiscounts(item, priceDiscounted, useCumulativeRules, ref price);
                        continue;
                    }

                    riDateFrom = (item.IncentiveFrom ?? rrd.Date).Date;
                    riDateTo = (item.IncentiveTo ?? rrd.Date).Date;

                    if (riDateFrom <= rrd.Date.Date && rrd.Date.Date <= riDateTo)
                    {
                        //Early bookings
                        if (item.IncentiveType_UID == 1)
                            priceDiscounted = price * ((decimal)(item.DiscountPercentage / 100.00));

                        //Last minute
                        else if (item.IncentiveType_UID == 2)
                            priceDiscounted = price * ((decimal)(item.DiscountPercentage / 100.00));

                        //Free Nights
                        else if (item.IncentiveType_UID == 3)
                        {
                            if (item.IsFreeDaysAtBegin == true)
                            {
                                if (rrd.Date >= checkIn && rrd.Date < checkIn.AddDays(item.FreeDays ?? 0))
                                    priceDiscounted = price;
                            }
                            else
                            {
                                if (rrd.Date < checkOut && rrd.Date >= checkOut.AddDays(-1 * (item.FreeDays ?? 0)))
                                    priceDiscounted = price;
                            }
                        }
                        // Discount
                        else if (item.IncentiveType_UID == 4)
                        {
                            priceDiscounted = price * ((decimal)(item.DiscountPercentage / 100.00));
                        }
                        // Stay Discount
                        else if (item.IncentiveType_UID == 5)
                        {
                            priceDiscounted = price * ((decimal)(item.DiscountPercentage / 100.00));
                        }
                        else
                        {
                            priceDiscounted = 0;
                        }
                    }

                    ApplyDiscounts(item, priceDiscounted, useCumulativeRules, ref price);
                }
            }

            return incentivesList;
        }

        private void ApplyDiscounts(OB.BL.Contracts.Data.Properties.Incentive item, decimal priceDiscounted, bool useCumulativeRules, ref decimal price)
        {
            item.DayDiscount.Add(priceDiscounted);
            item.TotalDiscounted += priceDiscounted;

            if (useCumulativeRules)
                price = price - priceDiscounted;
        }

        /// <summary>
        /// Calculate Rate Model prices
        /// </summary>
        /// <param name="rrdList"></param>
        /// <returns></returns>
        private List<OBcontractsRates.RateRoomDetailReservation> CalculatePriceModel(List<OBcontractsRates.RateRoomDetailReservation> rrdList, domainReservations.GroupRule groupRule)
        {
            if (!groupRule.BusinessRules.HasFlag(domainReservations.BusinessRules.CalculatePriceModel))
            {
                rrdList.ForEach(x => x.PriceAfterRateModel = x.PriceAfterLoyaltyLevel);
                return rrdList;
            }

            foreach (var rrd in rrdList)
            {
                var price = rrd.PriceAfterLoyaltyLevel;

                if(rrd.OperatorType == (int)OperatorsType.OperatorsHoteisNet && rrd.RateModelValue == null)
                    throw Errors.CommissionValueNotDefined.ToBusinessLayerException();

                var value = rrd.RateModelValue ?? 0;

                if (rrd.IsPackage)
                {
                    rrd.IsMarkup = true;
                    rrd.IsCommission = false;
                }

                if (rrd.PriceModel)
                {
                    //Retail: Markup RP x (1-0,markup)
                    if (rrd.IsMarkup)
                        price = price * (1 - (value / 100));
                }
                else
                {
                    //Net Price: Commision
                    if (rrd.IsCommission)
                        price = price / (1 - (value / 100));
                }

                rrd.PriceAfterRateModel = decimal.Round(price, 2);
                rrd.FinalPrice = rrd.PriceAfterRateModel;
            }

            return rrdList;
        }

        public decimal CalculateExternalCommission(decimal commission, long currencyUID, long? reservationBaseCurrency, long? propertyUID, decimal? totalAmount)
        {
            var reservationHelperPOCO = Resolve<IReservationHelperPOCO>();
            decimal commissionValue = 0;

            if (commission == 0 || !totalAmount.HasValue)
                return commissionValue;

            long baseCurrency = 0;
            if (propertyUID.HasValue)
            {
                var currencyRepo = RepositoryFactory.GetOBCurrencyRepository();
                var currency = currencyRepo.ListPropertyBaseCurrencyByPropertyUID(new OB.BL.Contracts.Requests.ListPropertyBaseCurrencyByPropertyUIDRequest() { IsActive = true, IsDemo = null, PropertyUIDs = new List<long> { propertyUID.Value } });
                if (currency != null && currency.ContainsKey(propertyUID.Value))
                    baseCurrency = currency[propertyUID.Value].UID;
            }
            else if (reservationBaseCurrency.HasValue)
                baseCurrency = reservationBaseCurrency.Value;

            if (baseCurrency > 0)
            {
                var exchangeRate = currencyUID > 0 ? reservationHelperPOCO.GetExchangeRateBetweenCurrencies(baseCurrency, currencyUID) : 1;
                var commissionBaseCurrency = ((totalAmount ?? 0) * (commission / 100M));
                commissionValue = commissionBaseCurrency * exchangeRate;
            }

            return commissionValue;
        }

        public List<OBcontractsRates.RateRoomDetailReservation> CalculateExternalMarkup(List<OBcontractsRates.RateRoomDetailReservation> rrdList, domainReservations.GroupRule groupRule, SellRule markup)
        {
            if (!groupRule.BusinessRules.HasFlag(domainReservations.BusinessRules.PullTpiReservationCalculation) || markup == null)
            {
                rrdList.ForEach(x => x.PriceAfterExternalMarkup = x.PriceAfterRateModel);
                return rrdList;
            }

            var first = rrdList.First();
            if (first.IsMarkup && markup.RatesTypeTarget == PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Comm)
                Errors.MarkupRulesAreInvalidForThisRate.ToBusinessLayerException();

            if (first.IsCommission && markup.RatesTypeTarget == PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget.Net)
                Errors.MarkupRulesAreInvalidForThisRate.ToBusinessLayerException();


            foreach (var rrd in rrdList)
            {
                decimal markupValue = 0;
                var priceModelMarkup = rrd.RateModelValue ?? 0;
                switch (markup.MarkupType)
                {
                    case PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalApplianceType.Up:
                        {
                            markupValue = priceModelMarkup + markup.Markup;
                            break;
                        }
                    case PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalApplianceType.Down:
                        {
                            markupValue = priceModelMarkup - markup.Markup;
                            if (markupValue < 0) markupValue = 0;
                            break;
                        }
                    case PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalApplianceType.Define:
                        {
                            markupValue = markup.Markup;
                            break;
                        }
                    default:
                        {
                            markupValue = 0;
                            break;
                        }
                }

                //Markup type base on channel pricemodel
                if (rrd.IsMarkup || rrd.IsPackage)
                {
                    rrd.PriceAfterExternalMarkup = (rrd.PriceAfterRateModel + markup.MarkupCurrencyValue) * (1 + (markupValue / 100M));
                    rrd.PriceAfterExternalTaxes = ((rrd.PriceAfterRateModel + markup.MarkupCurrencyValue +
                                                    markup.TaxCurrencyValue) * (1 + (markupValue / 100M))) *
                                                  (1 + (markup.Tax / 100M));
                }
                else
                    rrd.PriceAfterExternalMarkup = rrd.PriceAfterRateModel;


                rrd.FinalPrice = rrd.PriceAfterExternalTaxes > 0 ? rrd.PriceAfterExternalTaxes : rrd.PriceAfterExternalMarkup;
            }

            return rrdList;
        }

        /// <summary>
        /// Method that count the childrens accordingly with child terms
        /// </summary>
        /// <param name="reservationBaseCurrencyId"></param>
        /// <param name="propertyChildTerms"></param>
        /// <param name="nChilds"></param>
        /// <param name="ages"></param>
        /// <returns></returns>
        public List<ChildTermsOccupancy> GetGuestCountAfterApplyChildTerms(long? reservationBaseCurrencyId, List<OB.BL.Contracts.Data.Properties.ChildTerm> propertyChildTerms, int? nChilds, List<int> ages)
        {
            if (nChilds == null)
                nChilds = 0;

            var resultDictionary = new Dictionary<long, ChildTermsOccupancy>();

            try
            {
                if (nChilds > 0 && ages != null)
                {
                    if (propertyChildTerms?.Any() == true)
                    {
                        // Run all children ages
                        foreach (var age in ages)
                        {
                            // Check if age is included in children policies
                            foreach (OB.BL.Contracts.Data.Properties.ChildTerm ct in propertyChildTerms)
                            {
                                if (age >= ct.AgeFrom && age <= ct.AgeTo)
                                {
                                    // child inside policy
                                    var childTermsForSearch = new ChildTermsOccupancy();
                                    if (!resultDictionary.TryGetValue(ct.UID, out childTermsForSearch))
                                    {
                                        childTermsForSearch = new ChildTermsOccupancy();
                                        childTermsForSearch.IsPercentage = ct.IsPercentage ?? false;
                                        resultDictionary.Add(ct.UID, childTermsForSearch);
                                    }

                                    childTermsForSearch.NumberOfChilds++;

                                    // child is adult, we remove from child count and add it to adult count
                                    if (ct.CountsAsAdult)
                                    {
                                        childTermsForSearch.PriceType = ChildPriceType.Adult;
                                        childTermsForSearch.IsAccountableForOccupancy = true;
                                    }
                                    else
                                    {
                                        // child is free, remove from child count and add to free count
                                        if (ct.IsFree.HasValue && ct.IsFree.Value)
                                            childTermsForSearch.PriceType = ChildPriceType.Free;
                                        else
                                        {
                                            childTermsForSearch.PriceType = ChildPriceType.Child;

                                            // has price variation
                                            if (ct.IsActivePriceVariation)
                                            {
                                                // add others
                                                var currentChildTermCurrencies = ct.ChildTermsCurrencies;
                                                foreach (var entry in currentChildTermCurrencies)
                                                {
                                                    childTermsForSearch.PriceVariations.Add(new ChildPriceVariation
                                                    {
                                                        Currency_UID = entry.Currency_UID,
                                                        IsPriceVariationPerc = ct.IsPercentage ?? false,
                                                        PriceVariation = (ct.IsPercentage.HasValue && ct.IsPercentage.Value) ? ct.Value ?? 0 : entry.Value ?? 0,
                                                        IsValueDecrease = ct.IsValueDecrease ?? false
                                                    });
                                                }

                                                // add base
                                                if (currentChildTermCurrencies.All(x => x.Currency_UID != reservationBaseCurrencyId))
                                                    childTermsForSearch.PriceVariations.Add(new ChildPriceVariation
                                                    {
                                                        Currency_UID = reservationBaseCurrencyId,
                                                        IsPriceVariationPerc = ct.IsPercentage ?? false,
                                                        PriceVariation = ct.Value ?? 0,
                                                        IsValueDecrease = ct.IsValueDecrease ?? false
                                                    });
                                            }
                                        }

                                        //child doesnt count for occupancy, we remove it from child count
                                        if (!(ct.IsCountForOccupancy.HasValue && ct.IsCountForOccupancy.Value))
                                            childTermsForSearch.IsAccountableForOccupancy = false;
                                        else
                                            childTermsForSearch.IsAccountableForOccupancy = true;
                                    }
                                    // stop if a policy is matched
                                    break;
                                }
                                else
                                {
                                    if (ct == propertyChildTerms.Last())
                                    {
                                        // child not inside policy 
                                        throw Errors.InvalidChildrenAges.ToBusinessLayerException();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        throw Errors.InvalidChildrenAges.ToBusinessLayerException();
                    }
                }

                return resultDictionary.Values.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Calculate promotional code discount.
        /// </summary>
        /// <param name="rrdList"></param>
        /// <param name="promoCodeInfo"></param>
        /// <returns></returns>
        private List<OBcontractsRates.RateRoomDetailReservation> CalculatePricePromotionalCode(List<OBcontractsRates.RateRoomDetailReservation> rrdList, ValidPromocodeParameters promoCodeInfo)
        {
            bool applyDiscountsToRate = promoCodeInfo != null && promoCodeInfo.ReservationRoomsPeriods != null &&
                promoCodeInfo.ReservationRoomsPeriods.Any(x => x.IsAssociatedToPromocode && x.RateUID == rrdList.Select(rrd => rrd.Rate_UID).First());

            // Keep normal prices
            if (!applyDiscountsToRate || promoCodeInfo.PromoCodeObj == null || promoCodeInfo.NewDaysToApplyDiscount == null
                || !promoCodeInfo.NewDaysToApplyDiscount.Any())
            {
                foreach (var rrd in rrdList)
                    rrd.PriceAfterPromoCodes = rrd.PriceAfterIncentives;

                return rrdList;
            }

            var appliedPromotionalCodes = new List<AppliedPromotionalCodeQR1>();
            var promocodeObj = promoCodeInfo.PromoCodeObj;
            if (promocodeObj.IsPercentage)
            {
                decimal discountValuePercentage = (promocodeObj.DiscountValue ?? 0) / 100;
                foreach (var rrd in rrdList)
                {
                    decimal price = rrd.PriceAfterIncentives;
                    bool isDiscountDay = promoCodeInfo.NewDaysToApplyDiscount.Contains(rrd.Date);

                    if (price > 0 && isDiscountDay)
                    {
                        var discountValue = price * discountValuePercentage;
                        price -= discountValue;
                        rrd.AppliedPromotionalCode = new OB.BL.Contracts.Data.Reservations.ReservationRoomDetailsAppliedPromotionalCode()
                        {
                            PromotionalCode_UID = promocodeObj.UID,
                            ReservationRoomDetail_UID = rrd.UID ?? 0,
                            Date = rrd.Date.Date,
                            DiscountValue = discountValue,
                            DiscountPercentage = promocodeObj.DiscountValue ?? 0
                        };
                    }

                    rrd.PriceAfterPromoCodes = price;
                    rrd.FinalPrice = price;
                }
            }
            else
            {
                decimal discountPerDay = 0;
                var freeDays = rrdList.Where(x => x.PriceAfterIncentives <= 0).Count();
                var daysToApplyDiscount = rrdList.Count - freeDays;
                if (daysToApplyDiscount > 0)
                    discountPerDay = (promocodeObj.DiscountValue ?? 0) / daysToApplyDiscount;

                foreach (var rrd in rrdList)
                {
                    decimal price = rrd.PriceAfterIncentives;
                    bool isDiscountDay = promoCodeInfo.NewDaysToApplyDiscount.Contains(rrd.Date);

                    if (price > 0 && isDiscountDay)
                    {
                        price -= discountPerDay;
                        rrd.AppliedPromotionalCode = new OB.BL.Contracts.Data.Reservations.ReservationRoomDetailsAppliedPromotionalCode()
                        {
                            PromotionalCode_UID = promocodeObj.UID,
                            ReservationRoomDetail_UID = rrd.UID ?? 0,
                            Date = rrd.Date.Date,
                            DiscountValue = discountPerDay
                        };
                    }

                    rrd.PriceAfterPromoCodes = price;
                    rrd.FinalPrice = price;
                }
            }

            return rrdList;
        }
    }
}

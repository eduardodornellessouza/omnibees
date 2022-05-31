using System;
using System.Collections.Generic;
using System.Linq;
using OB.Services.IntegrationTests.Helpers;
using Ploeh.SemanticComparison;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.BL.Operations.Test.Helper;
using OB.DL.Common.Interfaces;
using Microsoft.Practices.Unity;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Operations.Test.Domain.Rates;
using OB.BL.Contracts.Data.Channels;
using OB.BL.Operations.Test.Domain;

namespace OB.BL.Operations.Helper
{
    //Atention: Some methods are commented and not removed yet because can be necessary in the future.
    //          The accesses to some repositories are commented and not removed yet because it can be necessary in the future.
    public class SearchBuilder : BaseBuilder
    {
        public SearchInputData InputData { get; set; }
        //public List<List<BookingEngineRateRoomSearch>> ExpectedData { get; set; }

        //Added
        public List<RateBuyerGroup> BuyerGroups { get; set; }
        public List<PromotionalCodeRate> PromocodeRatesList { get; set; }
        public List<PromotionalCodesCurrency> PromocodesCurrenciesList { get; set; }
        public List<RatesIncentive> RatesIncentiveList {get; set;}

        // arrange
        public SearchBuilder(IUnityContainer container, long propertyId = 1806)
            : base(container)
        {
            // initialize
            InputData = new SearchInputData
            {
                Rates = new List<Rate>(),
                RoomTypes = new List<RoomType>(),
                RateRooms = new List<RateRoom>(),
                RateRoomDetails = new List<RateRoomDetail>(),
                SearchParameter = new List<SearchParameters>(),
                PriceVariations = new List<PriceVariation>(),
                RateChannels = new List<RatesChannel>(),
                PropertyChannels = new List<ChannelsProperty>(),
                RateChannelsPaymentMethods = new List<RatesChannelsPaymentMethod>(),
                CancellationPolicies = new List<CancellationPolicy>(),
                Incentives = new List<Incentive>(),
                BlockedChannelsListUID = new List<long>(),
                ChildTerms = new List<ChildTerm>(),
                PropertyId = propertyId,
                RoomQuantity = 1,
                AdultPrices = this.CreatePrices(1, 100),
                ChildPrices = this.CreatePrices(1, 100),
                Allotment = 1,
                AllotmentUsed = 0,
                GroupCode = string.Empty,
                IsUserLoggedIn = true,
                LanguageId = 1
            };

            BuyerGroups = new List<RateBuyerGroup>();
            PromocodeRatesList = new List<PromotionalCodeRate>();
            PromocodesCurrenciesList = new List<PromotionalCodesCurrency>();
            RatesIncentiveList = new List<RatesIncentive>();
        }
        #region Builder Methods

        /// <summary>
        /// With the default search parameters.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder WithDefaultSearchParameters()
        {
            this.AddSearchParameters(new SearchParameters
            {
                AdultCount = 1,
                ChildCount = 0,
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(1)
            });

            return this;
        }

        /// <summary>
        /// With  room.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="AcceptsChildren">The accepts children.</param>
        /// <param name="AcceptsExtraBed">The accepts extra bed.</param>
        /// <param name="AdultMaxOccupancy">The adult maximum occupancy.</param>
        /// <param name="AdultMinOccupancy">The adult minimum occupancy.</param>
        /// <param name="ChildMaxOccupancy">The child maximum occupancy.</param>
        /// <param name="ChildMinOccupancy">The child minimum occupancy.</param>
        /// <param name="MaxOccupancy">The maximum occupancy.</param>
        /// <returns></returns>
        public SearchBuilder AddRoom(string name = "", bool? AcceptsChildren = false, bool? AcceptsExtraBed = false, int? AdultMaxOccupancy = 1,
                    int? AdultMinOccupancy = 1, int? ChildMaxOccupancy = 1, int? ChildMinOccupancy = 0, int? MaxOccupancy = 2, int? maxFreeChild = null, long? UID = null)
        {
            // initialize
            if (string.IsNullOrEmpty(name))
                name = Guid.NewGuid().ToString();

            InputData.RoomTypes.Add(CreateRoomType(name, AcceptsChildren, AcceptsExtraBed, AdultMaxOccupancy,
                    AdultMinOccupancy, ChildMaxOccupancy, ChildMinOccupancy, MaxOccupancy, maxFreeChild, UID));

            return this;
        }

        ///// <summary>
        ///// Adds the rates.
        ///// </summary>
        ///// <param name="numberOfRates">The number of rates.</param>
        ///// <returns></returns>
        //public SearchBuilder AddRates(int numberOfRates)
        //{
        //    for (int i = 0; i < numberOfRates; i++)
        //    {
        //        this.AddRate();
        //    }
        //    return this;
        //}

        /// <summary>
        /// With the  rate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public SearchBuilder AddRate(string name = "", DateTime? beginSale = null, DateTime? endSale = null, bool priceModel = false, long? UID = null)
        {
            // initialize
            if (string.IsNullOrEmpty(name))
                name = Guid.NewGuid().ToString();

            InputData.Rates.Add(CreateRate(name, null, null, false, false, beginSale, endSale, priceModel, UID));

            return this;
        }

        ///// <summary>
        ///// Adds the derived rate.
        ///// </summary>
        ///// <param name="parentId">The parent identifier.</param>
        ///// <param name="priceVariation">The price variation.</param>
        ///// <param name="isPriceVariationDecrease">if set to <c>true</c> [is price variation decrease].</param>
        ///// <param name="isPercentage">if set to <c>true</c> [is percentage].</param>
        ///// <param name="name">The name.</param>
        ///// <returns></returns>
        //public SearchBuilder AddDerivedRate(long parentId, decimal priceVariation, bool isPriceVariationDecrease,
        //                bool isPercentage = false, string name = "", DateTime? beginSale = null, DateTime? endSale = null)
        //{
        //    // initialize
        //    if (string.IsNullOrEmpty(name))
        //        name = Guid.NewGuid().ToString();

        //    InputData.Rates.Add(CreateRate(name, parentId, priceVariation, isPriceVariationDecrease, isPercentage, beginSale, endSale));

        //    return this;
        //}

        ///// <summary>
        ///// With  rate room.
        ///// </summary>
        ///// <param name="rateId">The rate identifier.</param>
        ///// <param name="roomId">The room identifier.</param>
        ///// <param name="allotment">The allotment.</param>
        ///// <returns></returns>
        //public SearchBuilder AddRateRoomCustom(long rateId, long roomId, int? allotment = 10)
        //{
        //    // initialize
        //    InputData.RateRooms.Add(CreateRateRoom(rateId, roomId, allotment));
        //    return this;
        //}

        /// <summary>
        /// Adds all rate rooms.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder AddRateRoomsAll()
        {
            // initialize
            foreach (var rate in InputData.Rates)
            {
                foreach (var room in InputData.RoomTypes)
                {
                    InputData.RateRooms.Add(CreateRateRoom(rate.UID, room.UID, 10));
                }
            }
            return this;
        }

        private int ChannelPropertyCount = 0;
        /// <summary>
        /// Adds property channels.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder AddPropertyChannels(List<long> channelIds, bool isActive = true, bool isDeleted = false,
            bool isOperatorsCreditLimit = true, decimal operatorCreditLimit = 1000, decimal operatorCreditUsed = 0, bool handleCredit = false,
            bool blockPaymentType = false)
        {
            // initialize
            //var channelRepo = RepositoryFactory.GetChannelsRepository(UnitOfWork);
            //var channelConfigRepo = RepositoryFactory.GetRepository<ChannelConfiguration>(UnitOfWork);
            //var propertyChannelsRepo = RepositoryFactory.GetChannelPropertyRepository(UnitOfWork);
            var channelsAvailable = new List<Channel>
            {
                new Channel
                {
                    UID = 1,
                    Name = "teste123"
                },
                new Channel
                {
                    UID = 80,
                    Name = "4 Cantos"
                },
                new Channel
                {
                    UID = 85,
                    Name = "Agaxtur"
                },
                new Channel
                {
                    UID = 247,
                    Name = "CVC"
                }
            };
            var channels = channelsAvailable.AsQueryable().Where(x => channelIds.Contains(x.UID));

            foreach (var channel in channelIds)
            {
                var rateChannel = new ChannelsProperty
                {
                    UID = ++ChannelPropertyCount,
                    Channel_UID = channel,
                    Property_UID = InputData.PropertyId,
                    IsDeleted = isDeleted,
                    IsActive = isActive,
                    OperatorCreditLimit = operatorCreditLimit,
                    IsOperatorsCreditLimit = isOperatorsCreditLimit,
                    OperatorCreditUsed = operatorCreditUsed,
                    OperatorBillingType = blockPaymentType ? 3 : 1
                };
                //propertyChannelsRepo.Add(rateChannel);

                if (handleCredit)
                {
                    var tmpChannel = channels.FirstOrDefault(x => x.UID == channel);
                    if (tmpChannel != null)
                    {
                        var channelConfig = new ChannelConfiguration
                        {
                            HandleCredit = handleCredit,
                        };
                        //channelConfigRepo.Add();
                        tmpChannel.ChannelConfiguration = channelConfig;
                        //channelRepo.AttachAsModified(tmpChannel);
                    }
                }

                InputData.PropertyChannels.Add(rateChannel);
            }

            //UnitOfWork.Save();

            return this;
        }

        private int TpisCount = 0;
        /// <summary>
        /// Adds property channels.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder AddTPI(long propertyId, OB.BL.Constants.TPIType tpiType, long propUid = 1806)
        {
            //var tpiRepo = RepositoryFactory.GetThirdPartyIntermediaryRepository(UnitOfWork);
            var tpi = new Test.Domain.CRM.ThirdPartyIntermediary
            {
                UID = ++TpisCount,
                Property_UID = propUid,
                Name = string.Empty,
                UserName = string.Empty,
                UserPassword = string.Empty,
                Country_UID = 150,
                Language_UID = 1,
                Currency_UID = 34,
                Address1 = string.Empty,
                City = string.Empty,
                PostalCode = string.Empty,
                Phone = string.Empty,
                CreateDate = DateTime.Now,
                TPIType = Convert.ToByte((int)tpiType)
            };

            //tpiRepo.Add(tpi);
            InputData.Tpi = tpi;

            UnitOfWork.Save();
            return this;
        }

        private int ChildTermCount = 0;
        /// <summary>
        /// Adds property channels.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder AddChiltTerm(string name = "", int ageFrom = 0, int ageTo = 1, bool countAsAdult = false, decimal? value = null,
            bool? isPercentage = null, bool? isValuePerNight = null, bool isFree = false, bool isCountForOccupancy = true, bool isValueDecrease = false,
            bool isActivePriceVariation = false, List<ChildTermsCurrency> currencies = null)
        {
            // initialize
            //var childTermRepo = RepositoryFactory.GetRepository<ChildTerm>(UnitOfWork);
            var childTerm = new ChildTerm
            {
                UID = ++ChildTermCount,
                Name = name,
                AgeFrom = ageFrom,
                AgeTo = ageTo,
                CountsAsAdult = countAsAdult,
                Value = value,
                IsPercentage = isPercentage,
                IsValuePerNight = isValuePerNight,
                IsFree = isFree,
                IsCountForOccupancy = isCountForOccupancy,
                IsValueDecrease = isValueDecrease,
                IsActivePriceVariation = isActivePriceVariation,
                ChildTermsCurrencies = currencies != null ? currencies.ToList() : null,
                Property_UID = this.InputData.PropertyId
            };

            //childTermRepo.Add(childTerm);
            //UnitOfWork.Save();

            InputData.ChildTerms.Add(childTerm);

            return this;
        }

        private int BuyerGroupCount = 0;
        /// <summary>
        /// Adds property channels.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder AddRateBuyerGroup(long rateId, long tpiId, bool isPercentage = false, bool isValueDecrease = false,
            decimal? value = null, decimal? gdsValue = null, bool gdsValueIsPercentage = false, bool gdsValueIsDecrease = false)
        {
            // initialize
            //var repo = RepositoryFactory.GetRateBuyerGroupRepository(UnitOfWork);
            var rateBuyerGroup = new RateBuyerGroup
            {
                UID = ++BuyerGroupCount,
                Rate_UID = rateId,
                TPI_UID = tpiId,
                IsPercentage = isPercentage,
                IsValueDecrease = isValueDecrease,
                Value = value,
                GDSValue = gdsValue,
                GDSValueIsDecrease = gdsValueIsDecrease,
                GDSValueIsPercentage = gdsValueIsPercentage,
                TPIType = 1
            };

            BuyerGroups.Add(rateBuyerGroup);

            //repo.Add(rateBuyerGroup);
            //UnitOfWork.Save();
            return this;
        }

        private int RateChannelCount = 0;
        /// <summary>
        /// Adds all rate channels.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder AddRateChannelsAll(List<long> channelIds, int rateModelId = 1, decimal priceAddonValue = 0, bool priceAddonIsPercentage = false,
            bool priceAddonIsValueDecrease = false, bool rateModelIsPercentage = true, decimal? package = null,
            decimal? markup = null, decimal? commission = null, decimal? value = null)
        {
            // initialize
            //var rateChannelsRepo = RepositoryFactory.GetRateChannelRepository(UnitOfWork);
            foreach (var rate in InputData.Rates)
            {
                foreach (var channel in channelIds)
                {
                    var rateChannel = new RatesChannel
                    {
                        UID = ++RateChannelCount,
                        Channel_UID = channel,
                        Rate_UID = rate.UID,
                        IsDeleted = false,
                        RateModel_UID = rateModelId,
                        PriceAddOnValue = priceAddonValue,
                        PriceAddOnIsPercentage = priceAddonIsPercentage,
                        PriceAddOnIsValueDecrease = priceAddonIsValueDecrease,
                        IsPercentage = rateModelIsPercentage,
                        Package = package,
                        Markup = markup,
                        Commission = commission,
                        Value = value
                    };
                    //rateChannelsRepo.Add(rateChannel);

                    InputData.RateChannels.Add(rateChannel);
                }
            }

            //UnitOfWork.Save();
            return this;
        }

        private int CancellationPolicyCount = 0;
        /// <summary>
        /// Adds all rate channels.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder AddRateCancellationPolicy(long rateId, string cancellationPolicyName = "", string cancellationPolicyDescription = "",
            bool cancellationCosts = false, int days = 0, bool isCancellationAllowed = true, int paymentModel = 1,
            List<Period> cancelationPolicyPeriods = null)
        {
            // initialize
            //var cancelationPolicyRepo = RepositoryFactory.GetCancellationPolicyRepository(UnitOfWork);

            var policy = new CancellationPolicy
            {
                UID = ++CancellationPolicyCount,
                CancellationCosts = cancellationCosts,
                Days = days,
                IsCancellationAllowed = isCancellationAllowed,
                PaymentModel = paymentModel,
                Name = cancellationPolicyName,
                Description = cancellationPolicyDescription
            };

            return this;
        }

        /// <summary>
        /// Adds all rate channels.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder AddRateChannelsPaymentMethodsAll(List<long> rateChannelIds, List<long> paymentTypes)
        {
            // initialize
            //var rateChannelsRepo = RepositoryFactory.GetRepository<RatesChannelsPaymentMethod>(UnitOfWork);
            foreach (var rate in InputData.Rates)
            {
                foreach (var rateChannel in rateChannelIds)
                {
                    foreach (var payment in paymentTypes)
                    {
                        var rateChannelPayment = new RatesChannelsPaymentMethod
                        {
                            PaymentMethod_UID = payment,
                            RateChannel_UID = rateChannel
                        };

                        //rateChannelsRepo.Add(rateChannelPayment);
                        InputData.RateChannelsPaymentMethods.Add(rateChannelPayment);
                    }
                }
            }

            //UnitOfWork.Save();
            return this;
        }

        /// <summary>
        /// With the  search parameters.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public SearchBuilder AddSearchParameters(SearchParameters search)
        {
            // initialize
            InputData.SearchParameter.Add(search);
            return this;
        }

        /// <summary>
        /// With channels blocked.
        /// </summary>
        /// <returns></returns>
        public SearchBuilder WithBlockedChannelsListUID(List<long> blockedChannels)
        {
            // initialize
            InputData.BlockedChannelsListUID = blockedChannels;
            InputData.IsBookingEngineBlocked = blockedChannels.Contains(1);
            return this;
        }

        ///// <summary>
        ///// With booking engine blocked.
        ///// </summary>
        ///// <returns></returns>
        //public SearchBuilder WithBookingEngineBlocked()
        //{
        //    InputData.BlockedChannelsListUID.Add(1);
        //    InputData.IsBookingEngineBlocked = true;
        //    return this;
        //}

        /// <summary>
        /// With restriction minimum lenght of stay
        /// </summary>
        /// <returns></returns>
        public SearchBuilder WithMinLenghtOfStay(int days)
        {
            // initialize
            InputData.MinimumLengthOfStay = days;
            return this;
        }

        /// <summary>
        /// With restriction maximum lenght of stay
        /// </summary>
        /// <returns></returns>
        public SearchBuilder WithMaxLenghtOfStay(int days)
        {
            // initialize
            InputData.MaximumLengthOfStay = days;
            return this;
        }

        /// <summary>
        /// With restriction StayTrought
        /// </summary>
        /// <returns></returns>
        public SearchBuilder WithStayTrought(int days)
        {
            // initialize
            InputData.StayThrough = days;
            return this;
        }

        /// <summary>
        /// With restriction Release Days
        /// </summary>
        /// <returns></returns>
        public SearchBuilder WithReleaseDays(int days)
        {
            // initialize
            InputData.ReleaseDays = days;
            return this;
        }

        /// <summary>
        /// With restriction Close on Arrival
        /// </summary>
        /// <returns></returns>
        public SearchBuilder WithCloseOnArrival(bool close)
        {
            // initialize
            InputData.ClosedOnArrival = close;
            return this;
        }

        /// <summary>
        /// With restriction Close on Departure
        /// </summary>
        /// <returns></returns>
        public SearchBuilder WithCloseOnDeparture(bool close)
        {
            // initialize
            InputData.ClosedOnDeparture = close;
            return this;
        }

        /// <summary>
        /// With  allotment.
        /// </summary>
        /// <param name="allotment">The allotment.</param>
        /// <returns></returns>
        public SearchBuilder WithAllotment(int? allotment)
        {
            // initialize
            InputData.Allotment = allotment;
            return this;
        }

        ///// <summary>
        ///// With  allotment.
        ///// </summary>
        ///// <param name="allotment">The allotment.</param>
        ///// <returns></returns>
        //public SearchBuilder WithAllotmentUsed(int? allotment)
        //{
        //    // initialize
        //    InputData.AllotmentUsed = allotment;
        //    return this;
        //}

        /// <summary>
        /// With  adult prices.
        /// </summary>
        /// <param name="nAdults">The n adults.</param>
        /// <param name="price">The price.</param>
        /// <param name="variationForEachAdult">The variation for each adult.</param>
        /// <param name="isVariationDecrease">if set to <c>true</c> [is variation decrease].</param>
        /// <returns></returns>
        public SearchBuilder WithAdultPrices(int nAdults, decimal price, decimal variationForEachAdult = 0, bool isVariationDecrease = false)
        {
            // initialize
            InputData.AdultPrices.Clear();
            InputData.AdultPrices = CreatePrices(nAdults, price, variationForEachAdult, isVariationDecrease);
            return this;
        }

        /// <summary>
        /// With  child prices.
        /// </summary>
        /// <param name="nChildren">The n adults.</param>
        /// <param name="price">The price.</param>
        /// <param name="variationForEachAdult">The variation for each adult.</param>
        /// <param name="isVariationDecrease">if set to <c>true</c> [is variation decrease].</param>
        /// <returns></returns>
        public SearchBuilder WithChildPrices(int nChildren, decimal price, decimal variationForEachAdult = 0, bool isVariationDecrease = false)
        {
            // initialize
            InputData.ChildPrices.Clear();
            InputData.ChildPrices = CreatePrices(nChildren, price, variationForEachAdult, isVariationDecrease);
            return this;
        }

        ///// <summary>
        ///// Adds the price variation.
        ///// </summary>
        ///// <param name="rateId">The rate identifier.</param>
        ///// <param name="priceVariationIsPercentage">if set to <c>true</c> [price variation is percentage].</param>
        ///// <param name="priceVariationIsDecrease">if set to <c>true</c> [price variation is decrease].</param>
        ///// <param name="priceVariationValue">The price variation value.</param>
        ///// <returns></returns>
        //public SearchBuilder AddPriceVariation(long rateId, bool priceVariationIsPercentage, bool priceVariationIsDecrease, decimal priceVariationValue)
        //{
        //    // initialize
        //    InputData.PriceVariations.Add(new PriceVariation
        //    {
        //        RateId = rateId,
        //        PriceVariationValue = priceVariationValue,
        //        PriceVariationIsPercentage = priceVariationIsPercentage,
        //        PriceVariationIsDecreased = priceVariationIsDecrease
        //    });

        //    return this;
        //}

        private int PromotionalCodeCount = 0;
        /// <summary>
        /// With promo code.
        /// </summary>
        /// <param name="promoCode">The promo code.</param>
        /// <param name="rateId">The rate identifier.</param>
        /// <returns></returns>
        public SearchBuilder AddPromoCode(long propertyId, long rateId, string code, DateTime dateFrom, DateTime dateTo,
            decimal? discountValue, bool isPercentage = false, int maxReservations = 100, int reservationCompleted = 0)
        {
            // initialize
            //var repo = RepositoryFactory.GetPromotionalCodeRepository(UnitOfWork);
            var promo = new PromotionalCode
            {
                UID = ++PromotionalCodeCount,
                //Property_UID = 1806,
                Code = code,
                DiscountValue = discountValue,
                IsPercentage = isPercentage,
                IsValid = true,
                Name = "",
                MaxReservations = maxReservations,
                ReservationsCompleted = reservationCompleted,
                IsDeleted = false,
                ValidFrom = dateFrom,
                ValidTo = dateTo,
                ActiveLimits = maxReservations > 0,
                LimitType = 1
            };

            //repo.Add(promo);
            InputData.PromoCode = promo;

            //UnitOfWork.Save();
            CreateRatePromoCode(promo, rateId);
            return this;
        }

        private int IncentiveCount = 0;
        /// <summary>
        /// With promo code.
        /// </summary>
        /// <param name="promoCode">The promo code.</param>
        /// <param name="rateId">The rate identifier.</param>
        /// <returns></returns>
        public SearchBuilder AddIncentive(long propertyId, long rateId, int days, int discountPercentage, int? freeDays,
            long incentiveTypeId, bool isFreeDaysAtBegin, bool isCumulative,int minDays = 0,int maxDays = 0, bool isBetweenNights = false,
            bool includeIncentiveWithBwToSimulateSpResults = false, Dictionary<DayOfWeek, bool> stayWindowWeekDays = null)
        {
            // initialize
            var incentive = new Incentive
            {
                UID = ++IncentiveCount,
                Property_UID = propertyId,
                Rate_UID = rateId,
                Days = days,
                DiscountPercentage = discountPercentage,
                FreeDays = freeDays,
                IncentiveType_UID = incentiveTypeId,
                IsFreeDaysAtBegin = isFreeDaysAtBegin,
                Name = string.Empty,
                IsDeleted = false,
                IsCumulative = isCumulative,
                MinDays = minDays,
                MaxDays = maxDays,
                IsBetweenNights = isBetweenNights,
                DayDiscount = new List<decimal>() { },
                StayWindowWeekDays = stayWindowWeekDays ?? new Dictionary<DayOfWeek, bool> {
                    {DayOfWeek.Monday, true },
                    {DayOfWeek.Tuesday, true },
                    {DayOfWeek.Wednesday, true },
                    {DayOfWeek.Thursday, true },
                    {DayOfWeek.Friday, true },
                    {DayOfWeek.Saturday, true },
                    {DayOfWeek.Sunday, true }
                }
            };

            if (stayWindowWeekDays == null || includeIncentiveWithBwToSimulateSpResults)
            {
                var inc = incentive.Clone();
                inc.StayWindowWeekDays = null;
                InputData.Incentives.Add(inc);
            }

            InputData.Incentives.Add(incentive);

            CreateRateIncentive(incentive, rateId, isCumulative);
            return this;
        }

        public SearchBuilder AddIncentive(long propertyId, long rateId, OB.Domain.Reservations.ReservationRoomDetailsAppliedIncentive appliedIncentive)
        {
            // initialize
            var incentive = new Incentive
            {
                UID = appliedIncentive.Incentive_UID,
                Property_UID = propertyId,
                Rate_UID = rateId,
                Days = appliedIncentive.Days.GetValueOrDefault(),
                DiscountPercentage = appliedIncentive.DiscountPercentage.GetValueOrDefault(),
                FreeDays = appliedIncentive.FreeDays.GetValueOrDefault(),
                IsFreeDaysAtBegin = appliedIncentive.IsFreeDaysAtBegin,
                Name = appliedIncentive.Name,
                IsDeleted = false,
                DayDiscount = new List<decimal>() { },
                StayWindowWeekDays = new Dictionary<DayOfWeek, bool> {
                    {DayOfWeek.Monday, true },
                    {DayOfWeek.Tuesday, true },
                    {DayOfWeek.Wednesday, true },
                    {DayOfWeek.Thursday, true },
                    {DayOfWeek.Friday, true },
                    {DayOfWeek.Saturday, true },
                    {DayOfWeek.Sunday, true }
                }
            };

            InputData.Incentives.Add(incentive);

            CreateRateIncentive(incentive, rateId, incentive.IsCumulative.GetValueOrDefault());
            return this;
        }

        private int PromoCodeCurrenciesCount = 0;
        /// <summary>
        /// With promo code.
        /// </summary>
        /// <param name="promoCode">The promo code.</param>
        /// <param name="rateId">The rate identifier.</param>
        /// <returns></returns>
        public SearchBuilder AddPromoCodeCurrencies(long promocodeId, long currencyId, decimal? value = null)
        {
            // initialize
            //var repo = RepositoryFactory.GetRepository<PromotionalCodesCurrency>(UnitOfWork);
            var promo = new PromotionalCodesCurrency
            {
                UID = ++PromoCodeCurrenciesCount,
                Currency_UID = currencyId,
                PromotionalCode_UID = promocodeId,
                Value = value
            };

            //repo.Add(promo);
            //UnitOfWork.Save();

            PromocodesCurrenciesList.Add(promo);

            return this;
        }

        //#endregion

        //#region Create Database Records Helpers

        /// <summary>
        /// Creates rate room details.
        /// </summary>
        public void CreateRateRoomDetails()
        {
            var datesList = new List<DateTime>();
            //var rateRoomDetailsRepo = RepositoryFactory.GetRateRoomDetailRepository(UnitOfWork);
            foreach (var item in InputData.SearchParameter)
            {
                for (DateTime i = item.CheckIn; i <= item.CheckOut; i = i.AddDays(1))
                {
                    datesList.Add(i);
                }
            }
            datesList = datesList.Distinct().ToList();

            if (datesList.Any())
            {
                for (DateTime i = datesList.Min(); i <= datesList.Max(); i = i.AddDays(1))
                {
                    foreach (var item in InputData.RateRooms)
                    {
                        var tmp = MapRateRoomDetail(i, item);
                        //rateRoomDetailsRepo.Add(tmp);
                        InputData.RateRoomDetails.Add(tmp);
                    }
                }
            }
            //UnitOfWork.Save();
        }

        /// <summary>
        /// Creates rate room details.
        /// </summary>
        public void CreateRateRoomDetails(DateTime dateFrom, DateTime dateTo, decimal priceAddon)
        {
            //var rateRoomDetailsRepo = RepositoryFactory.GetRateRoomDetailRepository(UnitOfWork);

            for (DateTime i = dateFrom; i <= dateTo; i = i.AddDays(1))
            {
                foreach (var item in InputData.RateRooms)
                {
                    var tmp = MapRateRoomDetail(i, item, priceAddon);
                    //rateRoomDetailsRepo.Add(tmp);
                    InputData.RateRoomDetails.Add(tmp);
                }
            }
            //UnitOfWork.Save();
        }

        ///// <summary>
        ///// Creates rate channels.
        ///// </summary>
        //public void CreateRateChannels()
        //{
        //    var rateChannelsRepo = RepositoryFactory.GetRateChannelRepository(UnitOfWork);
        //    foreach (var item in InputData.Rates)
        //    {
        //        var tmpChannel = new RatesChannel
        //        {
        //            IsDeleted = false,
        //            Channel_UID = 1,
        //            Rate_UID = item.UID,
        //            CreatedDate = DateTime.Now
        //        };

        //        var variation = InputData.PriceVariations.FirstOrDefault(x => x.RateId == item.UID);
        //        if (variation != null)
        //        {
        //            tmpChannel.PriceAddOnIsPercentage = variation.PriceVariationIsPercentage;
        //            tmpChannel.PriceAddOnIsValueDecrease = variation.PriceVariationIsDecreased;
        //            tmpChannel.PriceAddOnValue = variation.PriceVariationValue;
        //        }

        //        rateChannelsRepo.Add(tmpChannel);
        //    }

        //    UnitOfWork.Save();
        //}

        ///// <summary>
        ///// Creates rate rooms.
        ///// </summary>
        //public void CreateRateRooms()
        //{
        //    var rateRoomsRepo = RepositoryFactory.GetRateRoomRepository(UnitOfWork);
        //    foreach (var item in InputData.Rates)
        //    {
        //        foreach (var room in InputData.RoomTypes)
        //        {
        //            rateRoomsRepo.Add(CreateRateRoom(item.UID, room.UID, 100));
        //        }
        //    }

        //    UnitOfWork.Save();
        //}

        ///// <summary>
        ///// Compares the search criteria.
        ///// </summary>
        ///// <returns></returns>
        //public bool CompareSearchCriteria()
        //{
        //    bool isUniqueCriteria = true;
        //    if (InputData.SearchParameter.Count > 1)
        //    {
        //        KellermanSoftware.CompareNetObjects.CompareObjects comparator = new KellermanSoftware.CompareNetObjects.CompareObjects();
        //        SearchParameters PreviousRoom = InputData.SearchParameter[0];
        //        int i = 1;
        //        do
        //        {
        //            SearchParameters Room = InputData.SearchParameter[i++];
        //            isUniqueCriteria = isUniqueCriteria && comparator.Compare(Room, PreviousRoom);
        //            PreviousRoom = Room;
        //        }
        //        while (InputData.SearchParameter.Count > i);
        //    }

        //    return isUniqueCriteria;
        //}

        private int RateCount = 0;
        /// <summary>
        /// Creates the rate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parentRateId">The parent rate identifier.</param>
        /// <param name="priceVariation">The price variation.</param>
        /// <param name="isPriceVariationDecrease">if set to <c>true</c> [is price variation decrease].</param>
        /// <param name="isPercentage">if set to <c>true</c> [is percentage].</param>
        /// <returns></returns>
        private Rate CreateRate(string name, long? parentRateId = null, decimal? priceVariation = null,
            bool isPriceVariationDecrease = false, bool isPercentage = false, DateTime? beginSale = null, DateTime? endSale = null,
            bool priceModel = false, long? UID = null)
        {
            //var ratesRepo = RepositoryFactory.GetRateRepository(UnitOfWork);

            var rate = new Rate();
            rate.Name = name;
            rate.Property_UID = InputData.PropertyId;
            rate.Currency_UID = 34;
            //rate.RateCategory_UID = 9;
            //rate.CreatedDate = DateTime.Now;
            //rate.IsActive = true;
            //rate.IsDeleted = false;
            rate.BeginSale = beginSale;
            rate.EndSale = endSale;

            // Derivada
            rate.Rate_UID = parentRateId;
            rate.IsValueDecrease = isPriceVariationDecrease;
            rate.IsPriceDerived = parentRateId > 0;
            rate.IsPercentage = isPercentage;
            rate.Value = priceVariation;
            rate.PriceModel = priceModel;

            rate.UID = UID ?? (++RateCount);

            //ratesRepo.Add(rate);
            //UnitOfWork.Save();

            return rate;
        }

        private int CountRooms = 3709;
        /// <summary>
        /// Creates the type of the room.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="AcceptsChildren">The accepts children.</param>
        /// <param name="AcceptsExtraBed">The accepts extra bed.</param>
        /// <param name="AdultMaxOccupancy">The adult maximum occupancy.</param>
        /// <param name="AdultMinOccupancy">The adult minimum occupancy.</param>
        /// <param name="ChildMaxOccupancy">The child maximum occupancy.</param>
        /// <param name="ChildMinOccupancy">The child minimum occupancy.</param>
        /// <param name="MaxOccupancy">The maximum occupancy.</param>
        /// <returns></returns>
        private RoomType CreateRoomType(string name, bool? AcceptsChildren = null, bool? AcceptsExtraBed = null, int? AdultMaxOccupancy = null,
                    int? AdultMinOccupancy = null, int? ChildMaxOccupancy = null, int? ChildMinOccupancy = null, int? MaxOccupancy = null, int? maxFreeChild = null, long? UID = null)
        {
            //var roomRepo = RepositoryFactory.GetRoomTypesRepository(UnitOfWork);
            var room = new RoomType
            {
                UID = UID ?? CountRooms,
                Name = name,
                Property_UID = InputData.PropertyId,
                AcceptsChildren = AcceptsChildren,
                AcceptsExtraBed = AcceptsExtraBed,
                AdultMaxOccupancy = AdultMaxOccupancy,
                AdultMinOccupancy = AdultMinOccupancy,
                ChildMaxOccupancy = ChildMaxOccupancy,
                ChildMinOccupancy = ChildMinOccupancy,
                MaxFreeChild = maxFreeChild,
                IsDeleted = false,
                MaxOccupancy = MaxOccupancy,
                CreatedDate = DateTime.Now,
                Qty = 100
            };

            //roomRepo.Add(room);
            //UnitOfWork.Save();

            return room;
        }

        private int RateRoomCount = 4639; //0;
        /// <summary>
        /// Creates the rate room.
        /// </summary>
        /// <param name="rateId">The rate identifier.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="allotment">The allotment.</param>
        /// <returns></returns>
        private RateRoom CreateRateRoom(long rateId, long roomId, int? allotment)
        {
            //var rateRoomRepo = RepositoryFactory.GetRateRoomRepository(UnitOfWork);
            var rate = new RateRoom
            {
                UID = RateRoomCount,  //++RateRoomCount
                IsDeleted = false,
                Rate_UID = rateId,
                RoomType_UID = roomId,
                CreatedDate = DateTime.Now,
                Allotment = allotment,
                Price = -1,
            };

            //rateRoomRepo.Add(rate);
            //UnitOfWork.Save();

            return rate;
        }

        private int RatePromoCodeCount = 0;
        /// <summary>
        /// Creates the promo code.
        /// </summary>
        /// <param name="promoCode">The promo code.</param>
        /// <param name="rateId">The rate identifier.</param>
        /// <returns></returns>
        private void CreateRatePromoCode(PromotionalCode promoCode, long rateId)
        {
            //var promocodeRatesRepo = RepositoryFactory.GetRepository<PromotionalCodeRate>(UnitOfWork);

            // Add promocode to rate
            PromocodeRatesList.Add(new PromotionalCodeRate
            {
                UID = ++RatePromoCodeCount,
                PromotionalCode_UID = promoCode.UID,
                Rate_UID = rateId,
            });

            //UnitOfWork.Save();
        }

        private int RatesIncentiveCount = 0;
        /// <summary>
        /// Creates the rate incentive.
        /// </summary>
        /// <param name="incentive">The promo code.</param>
        /// <param name="rateId">The rate identifier.</param>
        /// <returns></returns>
        private void CreateRateIncentive(Incentive incentive, long rateId, bool isCumulative)
        {
            //var repo = RepositoryFactory.GetRepository<RatesIncentive>(UnitOfWork);

            // Add promocode to rate
            RatesIncentiveList.Add(new RatesIncentive
            {
                UID = ++RatesIncentiveCount,
                Incentive_UID = incentive.UID,
                Rate_UID = rateId,
                IsCumulative = isCumulative,
                DateFrom = DateTime.Now.Date,
                DateTo = DateTime.Now.AddDays(10),
                IsAvailableForDiferentPeriods = false
            });

            //UnitOfWork.Save();
        }

        //#endregion

        //#region Map Helpers

        /// <summary>
        /// Creates the prices.
        /// </summary>
        /// <param name="nAdults">The n adults.</param>
        /// <param name="price">The price.</param>
        /// <param name="variationForEachAdult">The variation for each adult.</param>
        /// <param name="isVariationDecrease">if set to <c>true</c> [is variation decrease].</param>
        /// <returns></returns>
        private List<decimal?> CreatePrices(int nAdults, decimal price, decimal variationForEachAdult = 0, bool isVariationDecrease = false)
        {
            var Prices = new List<decimal?>();
            //Prices.Add(price);
            for (int i = 1; i <= 20; i++)
            {
                if (i <= nAdults)
                {
                    if (variationForEachAdult > 0 && isVariationDecrease)
                    {
                        Prices.Add(price - variationForEachAdult);
                        price -= variationForEachAdult;
                    }
                    else if (variationForEachAdult > 0 && !isVariationDecrease)
                    {
                        Prices.Add(price + variationForEachAdult);
                        price += variationForEachAdult;
                    }
                    else
                        Prices.Add(price);
                }
                else
                    Prices.Add(null);
            }

            return Prices;
        }

        private int RoomDetailCount = 0;
        /// <summary>
        /// Maps the rate room detail.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private RateRoomDetail MapRateRoomDetail(DateTime i, RateRoom item, decimal priceAddon = 0)
        {
            var tmp = new RateRoomDetail();
            tmp.UID = ++RoomDetailCount;
            tmp.Adult_1 = InputData.AdultPrices[0] + priceAddon;
            tmp.Adult_2 = InputData.AdultPrices[1] + priceAddon;
            tmp.Adult_3 = InputData.AdultPrices[2] + priceAddon;
            tmp.Adult_4 = InputData.AdultPrices[3] + priceAddon;
            tmp.Adult_5 = InputData.AdultPrices[4] + priceAddon;
            tmp.Adult_6 = InputData.AdultPrices[5] + priceAddon;
            tmp.Adult_7 = InputData.AdultPrices[6] + priceAddon;
            tmp.Adult_8 = InputData.AdultPrices[7] + priceAddon;
            tmp.Adult_9 = InputData.AdultPrices[8] + priceAddon;
            tmp.Adult_10 = InputData.AdultPrices[9] + priceAddon;
            tmp.Adult_11 = InputData.AdultPrices[10] + priceAddon;
            tmp.Adult_12 = InputData.AdultPrices[11] + priceAddon;
            tmp.Adult_13 = InputData.AdultPrices[12] + priceAddon;
            tmp.Adult_14 = InputData.AdultPrices[13] + priceAddon;
            tmp.Adult_15 = InputData.AdultPrices[14] + priceAddon;
            tmp.Adult_16 = InputData.AdultPrices[15] + priceAddon;
            tmp.Adult_17 = InputData.AdultPrices[16] + priceAddon;
            tmp.Adult_18 = InputData.AdultPrices[17] + priceAddon;
            tmp.Adult_19 = InputData.AdultPrices[18] + priceAddon;
            tmp.Adult_20 = InputData.AdultPrices[19] + priceAddon;

            tmp.Child_1 = InputData.ChildPrices[0] + priceAddon;
            tmp.Child_2 = InputData.ChildPrices[1] + priceAddon;
            tmp.Child_3 = InputData.ChildPrices[2] + priceAddon;
            tmp.Child_4 = InputData.ChildPrices[3] + priceAddon;
            tmp.Child_5 = InputData.ChildPrices[4] + priceAddon;
            tmp.Child_6 = InputData.ChildPrices[5] + priceAddon;
            tmp.Child_7 = InputData.ChildPrices[6] + priceAddon;
            tmp.Child_8 = InputData.ChildPrices[7] + priceAddon;
            tmp.Child_9 = InputData.ChildPrices[8] + priceAddon;
            tmp.Child_10 = InputData.ChildPrices[9] + priceAddon;
            tmp.Child_11 = InputData.ChildPrices[10] + priceAddon;
            tmp.Child_12 = InputData.ChildPrices[11] + priceAddon;
            tmp.Child_13 = InputData.ChildPrices[12] + priceAddon;
            tmp.Child_14 = InputData.ChildPrices[13] + priceAddon;
            tmp.Child_15 = InputData.ChildPrices[14] + priceAddon;
            tmp.Child_16 = InputData.ChildPrices[15] + priceAddon;
            tmp.Child_17 = InputData.ChildPrices[16] + priceAddon;
            tmp.Child_18 = InputData.ChildPrices[17] + priceAddon;
            tmp.Child_19 = InputData.ChildPrices[18] + priceAddon;
            tmp.Child_20 = InputData.ChildPrices[19] + priceAddon;

            tmp.Allotment = InputData.Allotment;
            tmp.AllotmentUsed = InputData.AllotmentUsed;
            tmp.BlockedChannelsListUID = string.Join(",", InputData.BlockedChannelsListUID);
            tmp.isBookingEngineBlocked = InputData.IsBookingEngineBlocked;
            tmp.ChannelsListUID = InputData.IsBookingEngineBlocked ? string.Empty : "1";

            // Restrictions
            tmp.ClosedOnArrival = InputData.ClosedOnArrival;
            tmp.ClosedOnDeparture = InputData.ClosedOnDeparture;
            tmp.MaximumLengthOfStay = InputData.MaximumLengthOfStay;
            tmp.MinimumLengthOfStay = InputData.MinimumLengthOfStay;
            tmp.ReleaseDays = InputData.ReleaseDays;
            tmp.StayThrough = InputData.StayThrough;

            tmp.RateRoom_UID = item.UID;
            tmp.Date = i.Date;

            return tmp;
        }

        ///// <summary>
        ///// Creates the expected result object.
        ///// </summary>
        //public void CreateExpectedResult()
        //{
        //    ExpectedData = new List<List<BookingEngineRateRoomSearch>>();
        //    if (this.CompareSearchCriteria())
        //    {
        //        var list = new List<BookingEngineRateRoomSearch>();
        //        for (DateTime date = InputData.SearchParameter.FirstOrDefault().CheckIn; date < InputData.SearchParameter.FirstOrDefault().CheckOut;
        //                    date = date.AddDays(1))
        //        {
        //            foreach (var item in InputData.RateRooms)
        //            {
        //                var room = InputData.RoomTypes.FirstOrDefault(x => x.UID == item.RoomType_UID);
        //                var rate = InputData.Rates.FirstOrDefault(x => x.UID == item.Rate_UID);

        //                var tmp = MapBookingEngineRateRoomSearch(item, room, rate, InputData.SearchParameter.FirstOrDefault(), date);
        //                list.Add(tmp);
        //            }
        //        }

        //        ExpectedData.Add(list);
        //    }
        //    else
        //    {
        //        foreach (var parameters in InputData.SearchParameter)
        //        {
        //            var list = new List<BookingEngineRateRoomSearch>();

        //            for (DateTime date = parameters.CheckIn; date <= parameters.CheckOut; date = date.AddDays(1))
        //            {
        //                foreach (var item in InputData.RateRooms)
        //                {
        //                    var room = InputData.RoomTypes.FirstOrDefault(x => x.UID == item.RoomType_UID);
        //                    var rate = InputData.Rates.FirstOrDefault(x => x.UID == item.Rate_UID);

        //                    var tmp = MapBookingEngineRateRoomSearch(item, room, rate, parameters, date);
        //                    list.Add(tmp);
        //                }
        //            }

        //            ExpectedData.Add(list);
        //        }
        //    }
        //}

        //public void AssertSearch(List<List<BookingEngineRateRoomSearch>> result)
        //{
        //    // Assert numero de pesquisas
        //    Assert.AreEqual(result.Count, ExpectedData.Count, "Numero de pesquisas(quantidade de quartos) diferentes");

        //    for (int i = 0; i < result.Count; i++)
        //    {
        //        Assert.AreEqual(result[i].Count, ExpectedData[i].Count, "Numero de rateroomdetails diferentes");
        //    }

        //    for (int i = 0; i < result.Count; i++)
        //    {
        //        var expected = ExpectedData[i];
        //        var returned = result[i];
        //        for (int j = 0; j < returned.Count; j++)
        //        {
        //            var returnedItem = returned[j];
        //            var expectedItem = expected.FirstOrDefault(x => x.RateRoom_UID == returnedItem.RateRoom_UID && x.Date.Date == returnedItem.Date.Date);
        //            Assert.IsNotNull(expectedItem);

        //            var comparer = new Likeness<BookingEngineRateRoomSearch, BookingEngineRateRoomSearch>(returnedItem);
        //            comparer = comparer
        //            .Without(x => x.address)
        //            .Without(x => x.AppliedIncentives)
        //            .Without(x => x.AvailableOnRequest)
        //            .Without(x => x.BindingId)
        //            .Without(x => x.CancelationDays)
        //            .Without(x => x.ClassinficationStars)
        //            .Without(x => x.ClosedOnArrival)
        //            .Without(x => x.ClosedOnDeparture)
        //            .Without(x => x.ExtraBedPrice)
        //            .Without(x => x.Extras)
        //            .Without(x => x.fax)
        //            .Without(x => x.FreeDays)
        //            .Without(x => x.GetGeneralServicesAmenities)
        //            .Without(x => x.GroupCode_UID)
        //            .Without(x => x.Incentive_UID)
        //            .Without(x => x.IsAvailableToTPI)
        //            .Without(x => x.isBookingEngineBlocked)
        //            .Without(x => x.IsCancelationAllowed)
        //            .Without(x => x.IsDiscounted)
        //            .Without(x => x.IsExclusiveForGroupCode)
        //            .Without(x => x.IsExclusiveForPromoCode)
        //            .Without(x => x.IsFreeDaysAtBeginning)
        //            .Without(x => x.IsOccupancy)
        //            .Without(x => x.MaxAdult)
        //            .Without(x => x.MaxChild)
        //            .Without(x => x.MaximumLengthOfStay)
        //            .Without(x => x.MinimumLengthOfStay)
        //            .Without(x => x.OldAdultPrice)
        //            .Without(x => x.OldChildPrice)
        //            .Without(x => x.OrderIndex)
        //            .Without(x => x.PackageId)
        //            .Without(x => x.phone)
        //            .Without(x => x.PriceForOrder)
        //            .Without(x => x.PromotionalCode_UID)
        //            .Without(x => x.Property_ImageUID)
        //            .Without(x => x.Property_UID)
        //            .Without(x => x.PropertyAvailableOnRequest)
        //            .Without(x => x.PropertyDescription)
        //            .Without(x => x.PropertyName)
        //            .Without(x => x.RateCategory_UID)
        //            .Without(x => x.RateName)
        //            .Without(x => x.Rateorder)
        //            .Without(x => x.RateRoomDetail_UID)
        //            .Without(x => x.ReleaseDays)
        //            .Without(x => x.Restrictions)
        //            .Without(x => x.RoomNo)
        //            .Without(x => x.RoomTypeImage_UID)
        //            .Without(x => x.RoomTypeName)
        //            .Without(x => x.StayThrough)
        //            .Without(x => x.TotalTax)
        //            .Without(x => x.UID);

        //            comparer.ShouldEqual(expectedItem);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Maps the booking engine rate room search with multiple search criteria.
        ///// </summary>
        ///// <param name="item">The item.</param>
        ///// <param name="room">The room.</param>
        ///// <param name="rate">The rate.</param>
        ///// <param name="parameters">The search parameters.</param>
        ///// <param name="date">The date.</param>
        ///// <returns></returns>
        //public BookingEngineRateRoomSearch MapBookingEngineRateRoomSearch(RateRoom item, RoomType room, Rate rate,
        //    SearchParameters parameters, DateTime date)
        //{
        //    var tmp = new BookingEngineRateRoomSearch();
        //    tmp.AcceptsExtraBed = room.AcceptsExtraBed.HasValue ? room.AcceptsExtraBed.Value : false;

        //    if (parameters.AdultCount > 0)
        //        tmp.AdultPrice = InputData.AdultPrices[parameters.AdultCount - 1] ?? 0;

        //    tmp.AllotmentAvailable = (InputData.Allotment - InputData.AllotmentUsed) ?? 0;

        //    if (parameters.ChildCount > 0)
        //        tmp.ChildPrice = InputData.ChildPrices[parameters.ChildCount - 1] ?? 0;

        //    tmp.Rate_UID = item.Rate_UID;
        //    tmp.RoomType_UID = item.RoomType_UID;
        //    tmp.RateRoom_UID = item.UID;
        //    tmp.Date = date;

        //    // Add Price Variation
        //    if (InputData.PriceVariations.Count > 0)
        //    {
        //        var variation = InputData.PriceVariations.FirstOrDefault(x => x.RateId == item.Rate_UID);
        //        if (variation != null)
        //        {
        //            // Add X Percentage to value
        //            if (variation.PriceVariationIsPercentage && !variation.PriceVariationIsDecreased)
        //                tmp.AdultPrice = tmp.AdultPrice + ((variation.PriceVariationValue / 100) * tmp.AdultPrice);

        //            // Subtract X percentage of value
        //            if (variation.PriceVariationIsPercentage && variation.PriceVariationIsDecreased)
        //                tmp.AdultPrice = tmp.AdultPrice - ((variation.PriceVariationValue / 100) * tmp.AdultPrice);

        //            // Add X value to Value
        //            if (!variation.PriceVariationIsPercentage && !variation.PriceVariationIsDecreased)
        //                tmp.AdultPrice = tmp.AdultPrice + variation.PriceVariationValue;

        //            // Subtract X value to Value
        //            if (!variation.PriceVariationIsPercentage && variation.PriceVariationIsDecreased)
        //                tmp.AdultPrice = tmp.AdultPrice - variation.PriceVariationValue;
        //        }
        //    }

        //    return tmp;
        //}

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Ploeh.SemanticComparison;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Collections;
using OB.Domain.Rates;
using Microsoft.Practices.Unity;
using OB.BL.Operations.Test.Helper;
using OB.Domain.General;

namespace OB.BL.Operations.Helper
{
    public class UpdateRatesBuilder : BaseBuilder
    {
        public UpdateRatesObject Data { get; set; }
        public List<RateRoomDetail> ExpectedData { get; set; }
        //public List<RateRoomDetail> BeforeUpdateExpectedData { get; set; }
        public Dictionary<string, int?> OldAllotment { get; set; }
        private SearchBuilder searchBuilder { get; set; }

        // arrange
        public UpdateRatesBuilder(IUnityContainer container, long propertyId = 1635) : base(container)
        {
            // initialize
            Data = new UpdateRatesObject
            {
                Rates = new List<Rate>(),
                RoomAndParameters = new List<RoomParameters>(),
                RateRooms = new List<RateRoom>(),
                RateChannels = new List<RatesChannel>(),
                channelList = new List<long>(),
                Periods = new List<Period>(),
                PropertyId = 1635,
                UserId = 70,
                CloseChannelsWeekdays = new List<bool> { true, true, true, true, true, true, true },
                ClosedOnArrivalWeekdays = new List<bool> { true, true, true, true, true, true, true },
                ClosedOnDepartureWeekdays = new List<bool> { true, true, true, true, true, true, true },
                IsInsert = true,
                IsPriceChanged = true,
                IsAllotmentChanged = true,
                CorrelationId = Guid.NewGuid()
            };
            ExpectedData = new List<RateRoomDetail>();
            //BeforeUpdateExpectedData = new List<RateRoomDetail>();
            OldAllotment = new Dictionary<string, int?>();
            searchBuilder = new SearchBuilder(container, propertyId);
        }

        #region Builder Methods

        public UpdateRatesBuilder AddRates(int numberOfRates)
        {
            Data.Rates.Clear();
            Data.Rates.AddRange(searchBuilder.AddRates(numberOfRates).InputData.Rates);
            return this;
        }

        public UpdateRatesBuilder AddDerivedRate(long parentId, decimal priceVariation, bool isPriceVariationDecrease,
            bool isPercentage, string name = "")
        {
            Data.Rates.Clear();
            Data.Rates.AddRange(searchBuilder.AddDerivedRate(parentId, priceVariation, isPriceVariationDecrease,
                            isPercentage, name).InputData.Rates);
            return this;
        }

        public UpdateRatesBuilder AddRate(string name = "")
        {
            Data.Rates.Clear();
            Data.Rates.AddRange(searchBuilder.AddRate(name).InputData.Rates);
            return this;
        }

        public UpdateRatesBuilder AddRoom(int AdultMaxOccupancy = 1, int AdultMinOccupancy = 1, int ChildMaxOccupancy = 0,
                                            int ChildMinOccupancy = 0, int? MaxOccupancy = 2, bool AcceptsExtraBed = false,
                                            bool? AcceptsChildren = false)
        {
            var roomParameters = new RoomParameters
            {
                Room =
                    searchBuilder.AddRoom(
                        string.Empty,
                        AcceptsChildren,
                        AcceptsExtraBed,
                        AdultMaxOccupancy,
                        AdultMinOccupancy,
                        ChildMaxOccupancy,
                        ChildMinOccupancy,
                        MaxOccupancy).InputData.RoomTypes.Last(),
            };

            Data.RoomAndParameters.Add(roomParameters);
            return this;
        }

        public UpdateRatesBuilder AddRoomPrices(RoomParameters roomParameters, List<decimal> adultPrices, List<decimal> childrenPrices, int allotment = -1,
                        decimal? extraBedPrice = 0, List<bool> weekDays = null)
        {
            var roomParametersTmp = Data.RoomAndParameters.Find(x => x.Equals(roomParameters));
            if (roomParametersTmp.RoomPrices == null)
                roomParametersTmp.RoomPrices = new List<RoomPrices>();

            if (childrenPrices == null) childrenPrices = new List<decimal>();

            if (weekDays == null)
                weekDays = new List<bool> { true, true, true, true, true, true, true };

            // Check if weekdays already exists
            foreach (var item in roomParametersTmp.RoomPrices)
                Assert.IsFalse(item.WeekDays.SequenceEqual(weekDays), "Weekdays are repeated.");

            var roomPrices = new RoomPrices
            {
                AdultsPriceList = adultPrices,
                ChildrenPriceList = childrenPrices,
                Allotment = allotment,
                ExtraBedPrice = extraBedPrice,
                WeekDays = weekDays,
                NoOfAdultsList = new List<int>(),
                NoOfChildrenList = new List<int>()
            };

            roomParametersTmp.RoomPrices.Add(roomPrices);

            // Add Adults
            this.CreatePersons(true, roomParametersTmp.Room.AdultMinOccupancy.Value, roomParametersTmp.Room.AdultMaxOccupancy.Value, adultPrices, ref roomPrices);

            // Add Children
            this.CreatePersons(false, roomParametersTmp.Room.ChildMinOccupancy.Value, roomParametersTmp.Room.ChildMaxOccupancy.Value, childrenPrices, ref roomPrices);

            return this;
        }

        public UpdateRatesBuilder AddRateRooms()
        {
            Data.RateRooms.Clear();
            Data.RateRooms.AddRange(searchBuilder.AddRateRoomsAll().InputData.RateRooms);
            return this;
        }

        public UpdateRatesBuilder AddRateChannels(List<long> channelIds)
        {
            Data.RateChannels.Clear();
            Data.RateChannels.AddRange(searchBuilder.AddRateChannelsAll(channelIds).InputData.RateChannels);
            return this;
        }

        public UpdateRatesBuilder AddPeriod(DateTime startDate, DateTime endDate)
        {
            Data.Periods.Add(new Period
            {
                DateFrom = startDate,
                DateTo = endDate
            });
            return this;
        }

        public UpdateRatesBuilder WithMinDaysRestriction(int days)
        {
            Data.MinimumLengthOfStay = days;
            Data.IsMinDaysChanged = true;
            searchBuilder.InputData.MinimumLengthOfStay = days;
            return this;
        }

        public UpdateRatesBuilder WithMaxDaysRestriction(int days)
        {
            Data.MaximumLengthOfStay = days;
            Data.IsMaxDaysChanged = true;
            searchBuilder.InputData.MaximumLengthOfStay = days;
            return this;
        }

        public UpdateRatesBuilder WithStayThroughRestriction(int days)
        {
            Data.StayThrough = days;
            Data.IsStayThroughChanged = true;
            searchBuilder.InputData.StayThrough = days;
            return this;
        }

        public UpdateRatesBuilder WithReleaseDaysRestriction(int days)
        {
            Data.ReleaseDays = days;
            Data.IsReleaseDaysChanged = true;
            searchBuilder.InputData.ReleaseDays = days;
            return this;
        }

        public UpdateRatesBuilder WithClosedOnArrivalRestriction(bool isClosed, List<bool> weekdays = null)
        {
            Data.ClosedOnArrival = isClosed;
            Data.ClosedOnArrivalWeekdays = weekdays ?? new List<bool> { true, true, true, true, true, true, true };
            Data.IsClosedOnArrivalChanged = true;
            //searchBuilder.InputData.ClosedOnArrival = isClosed;
            return this;
        }

        public UpdateRatesBuilder WithClosedOnDepartureRestriction(bool isClosed, List<bool> weekdays = null)
        {
            Data.ClosedOnDeparture = isClosed;
            Data.ClosedOnDepartureWeekdays = weekdays ?? new List<bool> { true, true, true, true, true, true, true };
            Data.IsClosedOnDepartureChanged = true;
            searchBuilder.InputData.ClosedOnDeparture = isClosed;
            return this;
        }

        public UpdateRatesBuilder WithClosedSales(List<long> channelsList, List<bool> weekdays = null)
        {
            Data.isCloseSales = true;
            Data.CloseChannelsWeekdays = weekdays ?? new List<bool> { true, true, true, true, true, true, true };
            Data.channelList = channelsList;
            Data.IsStoppedSaleChanged = true;
            return this;
        }

        public UpdateRatesBuilder WithOpenSales(List<long> channelsList)
        {
            Data.isCloseSales = false;
            Data.channelList = channelsList;
            Data.IsStoppedSaleChanged = true;
            return this;
        }

        public UpdateRatesBuilder ChangeRoomPrices(RoomParameters roomParameters, RoomPrices roomPrices, List<decimal> adultPrices, List<decimal> childrenPrices,
            int allotment = -1, decimal? extraBedPrice = 0, List<bool> weekDays = null)
        {
            var roomParametersTmp = Data.RoomAndParameters.Find(x => x.Equals(roomParameters));
            if (roomParametersTmp.RoomPrices == null)
                roomParametersTmp.RoomPrices = new List<RoomPrices>();
            //else
            //    roomParametersTmp.RoomPrices.Clear();

            if (childrenPrices == null) childrenPrices = new List<decimal>();

            if (weekDays == null)
                weekDays = new List<bool> { true, true, true, true, true, true, true };

            // Check if weekdays already exists
            foreach (var item in roomParametersTmp.RoomPrices.Except(new List<RoomPrices>() { roomPrices }))
                Assert.IsFalse(item.WeekDays.SequenceEqual(weekDays), "Weekdays are repeated.");

            var tmpRoomPrices = roomParametersTmp.RoomPrices.Find(x => x.Equals(roomPrices));
            if (tmpRoomPrices != null)
            {
                tmpRoomPrices.AdultsPriceList = adultPrices;
                tmpRoomPrices.ChildrenPriceList = childrenPrices;
                tmpRoomPrices.Allotment = allotment;
                tmpRoomPrices.ExtraBedPrice = extraBedPrice;
                tmpRoomPrices.WeekDays = weekDays;
                tmpRoomPrices.NoOfAdultsList = new List<int>();
                tmpRoomPrices.NoOfChildrenList = new List<int>();
                tmpRoomPrices.IsPriceVariation = false;
            }
            else
            {
                var roomPricesNew = new RoomPrices
                {
                    AdultsPriceList = adultPrices,
                    ChildrenPriceList = childrenPrices,
                    Allotment = allotment,
                    ExtraBedPrice = extraBedPrice,
                    WeekDays = weekDays,
                    NoOfAdultsList = new List<int>(),
                    NoOfChildrenList = new List<int>(),
                    IsPriceVariation = false
                };

                roomParametersTmp.RoomPrices.Add(roomPricesNew);
                roomPrices = roomPricesNew;
            }

            // Add Adults
            this.CreatePersons(true, roomParametersTmp.Room.AdultMinOccupancy.Value, roomParametersTmp.Room.AdultMaxOccupancy.Value, adultPrices, ref roomPrices);

            // Add Children
            this.CreatePersons(false, roomParametersTmp.Room.ChildMinOccupancy.Value, roomParametersTmp.Room.ChildMaxOccupancy.Value, childrenPrices, ref roomPrices);

            return this;
        }

        public UpdateRatesBuilder ChangeRoomPricesWithVariation(RoomParameters roomParameters, RoomPrices roomPrices, List<PriceVariationValues> adultVariation,
                                    List<PriceVariationValues> childrenVariation, int allotment = -1, PriceVariationValues extraBedVariation = null, List<bool> weekDays = null)
        {
            var roomParametersTmp = Data.RoomAndParameters.Find(x => x.Equals(roomParameters));
            if (roomParametersTmp.RoomPrices == null)
                roomParametersTmp.RoomPrices = new List<RoomPrices>();
            //else
            //    roomParametersTmp.RoomPrices.Clear();

            if (adultVariation == null) adultVariation = new List<PriceVariationValues>();
            if (childrenVariation == null) childrenVariation = new List<PriceVariationValues>();
            if (extraBedVariation == null) extraBedVariation = new PriceVariationValues();

            if (weekDays == null)
                weekDays = new List<bool> { true, true, true, true, true, true, true };

            // Check if weekdays already exists
            foreach (var item in roomParametersTmp.RoomPrices.Except(new List<RoomPrices>() { roomPrices }))
                Assert.IsFalse(item.WeekDays.SequenceEqual(weekDays), "Weekdays are repeated.");

            // Adult Price Variation
            var adultPrices = new List<decimal>();
            var adultIsDecreased = new List<bool>();
            var adultIsPercentage = new List<bool>();
            foreach (var item in adultVariation)
            {
                adultPrices.Add(item.Value);
                adultIsDecreased.Add(item.IsValueDecreased);
                adultIsPercentage.Add(item.IsPercentage);
            }

            var childrenPrices = new List<decimal>();
            var childrenIsDecreased = new List<bool>();
            var childrenIsPercentage = new List<bool>();
            foreach (var item in childrenVariation)
            {
                childrenPrices.Add(item.Value);
                childrenIsDecreased.Add(item.IsValueDecreased);
                childrenIsPercentage.Add(item.IsPercentage);
            }

            var tmpRoomPrices = roomParametersTmp.RoomPrices.Find(x => x.Equals(roomPrices));
            if (tmpRoomPrices != null)
            {
                tmpRoomPrices.AdultsPriceVariationList = adultPrices;
                tmpRoomPrices.AdultPriceVariationIsPercentage = adultIsPercentage;
                tmpRoomPrices.AdultPriceVariationIsValueDecrease = adultIsDecreased;
                tmpRoomPrices.ChildrenPriceVariationList = childrenPrices;
                tmpRoomPrices.ChildPriceVariationIsPercentage = childrenIsPercentage;
                tmpRoomPrices.ChildPriceVariationIsValueDecrease = childrenIsDecreased;
                tmpRoomPrices.Allotment = allotment;
                tmpRoomPrices.ExtraBedVariationValue = extraBedVariation.Value;
                tmpRoomPrices.ExtraBedVariationIsPercentage = extraBedVariation.IsPercentage;
                tmpRoomPrices.ExtraBedVariationIsValueDecrease = extraBedVariation.IsValueDecreased;
                tmpRoomPrices.WeekDays = weekDays;
                tmpRoomPrices.NoOfAdultsList = new List<int>();
                tmpRoomPrices.NoOfChildrenList = new List<int>();
                tmpRoomPrices.IsPriceVariation = true;
            }
            else
            {
                var roomPricesNew = new RoomPrices
                {
                    AdultsPriceVariationList = adultPrices,
                    AdultPriceVariationIsPercentage = adultIsPercentage,
                    AdultPriceVariationIsValueDecrease = adultIsDecreased,
                    ChildrenPriceVariationList = childrenPrices,
                    ChildPriceVariationIsPercentage = childrenIsPercentage,
                    ChildPriceVariationIsValueDecrease = childrenIsDecreased,
                    Allotment = allotment,
                    ExtraBedVariationValue = extraBedVariation.Value,
                    ExtraBedVariationIsPercentage = extraBedVariation.IsPercentage,
                    ExtraBedVariationIsValueDecrease = extraBedVariation.IsValueDecreased,
                    WeekDays = weekDays,
                    NoOfAdultsList = new List<int>(),
                    NoOfChildrenList = new List<int>(),
                    IsPriceVariation = true
                };

                roomParametersTmp.RoomPrices.Add(roomPricesNew);
                roomPrices = roomPricesNew;
            }

            // Add Adults
            this.CreatePersons(true, roomParametersTmp.Room.AdultMinOccupancy.Value, roomParametersTmp.Room.AdultMaxOccupancy.Value, adultPrices, ref roomPrices);

            // Add Children
            this.CreatePersons(false, roomParametersTmp.Room.ChildMinOccupancy.Value, roomParametersTmp.Room.ChildMaxOccupancy.Value, childrenPrices, ref roomPrices);

            return this;
        }

        public UpdateRatesBuilder ChangeAllotment(RoomParameters roomParameters, int allotment)
        {
            var roomParametersTmp = Data.RoomAndParameters.Find(x => x.Equals(roomParameters));
            Assert.IsTrue(roomParametersTmp.RoomPrices.Any(), "Não existem quartos inseridos");

            foreach (var item in roomParametersTmp.RoomPrices)
            {
                item.Allotment = allotment;
            }

            return this;
        }

        public UpdateRatesBuilder ChangeExtraBedPrice(RoomParameters roomParameters, decimal extraBedPrice)
        {
            var roomParametersTmp = Data.RoomAndParameters.Find(x => x.Equals(roomParameters));
            Assert.IsTrue(roomParametersTmp.RoomPrices.Any(), "Não existem quartos inseridos");

            foreach (var item in roomParametersTmp.RoomPrices)
            {
                item.ExtraBedPrice = extraBedPrice;
            }

            return this;
        }

        #endregion

        #region Create Expected Data

        Dictionary<string, List<decimal>> parentInsertedPricesDictionary = new Dictionary<string, List<decimal>>();
        Dictionary<string, List<decimal>> parentUpdatedPricesDictionary = new Dictionary<string, List<decimal>>();
        Dictionary<string, List<decimal>> childrenInsertedPricesDictionary = new Dictionary<string, List<decimal>>();
        Dictionary<string, List<decimal>> childrenUpdatedPricesDictionary = new Dictionary<string, List<decimal>>();
        //Dictionary<string, List<decimal>> adultPricesBeforeVariationDictionary = new Dictionary<string, List<decimal>>();
        //Dictionary<string, List<decimal>> childrenPricesBeforeVariationDictionary = new Dictionary<string, List<decimal>>();
        Dictionary<string, decimal?> extraBedBeforeVariationDictionary = new Dictionary<string, decimal?>();
        Dictionary<string, List<DateTime>> daysNotValidDictionary = new Dictionary<string, List<DateTime>>();

        /// <summary>
        /// Creates the expected data.
        /// </summary>
        public void CreateExpectedData()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en");
            foreach (var item in Data.Periods)
            {
                for (var i = item.DateFrom; i <= item.DateTo; i = i.AddDays(1))
                {
                    for (var index = 0; index < this.Data.RateRooms.Count; index++)
                    {
                        var rateRoom = this.Data.RateRooms[index];
                        //var tmp = this.MapRateRoomDetail(i, rateRoom, index);
                        ExpectedData.AddRange(this.MapRateRoomDetail(i, rateRoom, index));
                    }
                }
            }
        }

        #endregion

        #region Get Data from Database

        /// <summary>
        /// Gets the rate room details.
        /// </summary>
        /// <returns></returns>
        public List<RateRoomDetail> GetRateRoomDetails()
        {
            var rateRoomDetailsRepo = RepositoryFactory.GetRateRoomDetailRepository(UnitOfWork);
            var rateRoomsIds = Data.RateRooms.Select(x => x.UID);
            return rateRoomDetailsRepo.GetQuery(x => rateRoomsIds.Contains(x.RateRoom_UID)).ToList();
        }

        /// <summary>
        /// Sets the rate room details blockedchannels field to null.
        /// </summary>
        /// <returns></returns>
        public void SetRateRoomDetailsBlockedChannelFieldToNull()
        {
            var rateRoomDetailsRepo = RepositoryFactory.GetRateRoomDetailRepository(UnitOfWork);

            var rateRoomsIds = Data.RateRooms.Select(x => x.UID);
            var tmp = rateRoomDetailsRepo.GetQuery(x => rateRoomsIds.Contains(x.RateRoom_UID)).ToList();

            foreach (var item in tmp)
            {
                item.BlockedChannelsListUID = null;
                rateRoomDetailsRepo.AttachAsModified(item);
            }

            UnitOfWork.Save();
        }

        /// <summary>
        /// Ge AsyncTasks By CorrelationId
        /// </summary>
        /// <returns></returns>
        public List<AsyncTask> GetAsyncTasks()
        {
            var asyncTaskRepo = RepositoryFactory.GetRepository<AsyncTask>(UnitOfWork);
            var guid = Data.CorrelationId.ToString();
            return asyncTaskRepo.GetQuery(x => x.correlationUID == guid).ToList();
        }

        #endregion

        #region Map Helpers

        public List<RateRoomDetail> MapRateRoomDetail(DateTime date, RateRoom item, int index)
        {
            var list = new List<RateRoomDetail>();
            // Get Room
            var room = Data.RoomAndParameters.FirstOrDefault(x => x.Room.UID == item.RoomType_UID);
            if (room == null) return null;

            // Derived Rate Variation
            var rate = Data.Rates.FirstOrDefault(x => x.UID == item.Rate_UID);
            var priceVariationDerived = rate.Value;
            var isPriceVariationDecreasedDerived = rate.IsValueDecrease;
            var isPercentageDerived = rate.IsPercentage;
            var isDerived = rate.IsPriceDerived;

            var j = 0;
            foreach (var roomPrices in room.RoomPrices)
            {
                // Validate weekdays - if is insert and weekday is false don't insert
                var dayIsValid = true;

                // don't update prices if weekday is false
                if (!roomPrices.WeekDays[(int)date.DayOfWeek])
                    dayIsValid = false;

                if (!dayIsValid && Data.IsInsert)
                {
                    if (!daysNotValidDictionary.Any(x => x.Key == rate.UID + ";" + room.Room.UID + ";" + j))
                        daysNotValidDictionary.Add(rate.UID + ";" + room.Room.UID + ";" + j, new List<DateTime>());

                    daysNotValidDictionary[rate.UID + ";" + room.Room.UID + ";" + j].Add(date);
                    j++;
                    continue;
                }
                else if (!Data.IsInsert && !dayIsValid
                    && daysNotValidDictionary.Any(x => x.Key == rate.UID + ";" + room.Room.UID + ";" + j)
                    && daysNotValidDictionary[rate.UID + ";" + room.Room.UID + ";" + j].Contains(date))
                //&& !this.parentInsertedPricesDictionary.Any(x => x.Key == rate.UID + ";" + room.Room.UID + ";" + j))
                {
                    j++;
                    continue;
                }

                // Validate allotment - If allotment is not defined and is an insert don't do nothing, go to next day
                if (roomPrices.Allotment == -1 && Data.IsInsert)
                    continue;

                // If is update without allotment and day not exist, go to next day
                else if (roomPrices.Allotment == -1 && !Data.IsInsert
                    && daysNotValidDictionary.Any(x => x.Key == rate.UID + ";" + room.Room.UID + ";" + j)
                    && daysNotValidDictionary[rate.UID + ";" + room.Room.UID + ";" + j].Contains(date))
                    continue;

                var adultPricesTmp = new List<decimal>();
                var childrenPricesTmp = new List<decimal>();
                var adultPrices = new List<decimal>();
                var childrenPrices = new List<decimal>();
                var priceVariation = new Nullable<decimal>();
                var isPriceVariationDecreased = false;
                var isPercentage = false;
                var isVariation = false;

                var tmp = new RateRoomDetail();

                if (isDerived)
                {
                    if (dayIsValid && !Data.IsInsert)
                    {
                        //if (daysNotValidDictionary.Any(x => x.Key == rate.Rate_UID + ";" + room.Room.UID + ";" + j))
                        //{
                        //    adultPrices = this.parentInsertedPricesDictionary[rate.Rate_UID + ";" + room.Room.UID + ";" + j];
                        //    childrenPrices = this.childrenInsertedPricesDictionary[rate.Rate_UID + ";" + room.Room.UID + ";" + j];
                        //}
                        //else
                        //{
                            adultPrices = this.parentUpdatedPricesDictionary[rate.Rate_UID + ";" + room.Room.UID + ";" + j];
                            childrenPrices = this.childrenUpdatedPricesDictionary[rate.Rate_UID + ";" + room.Room.UID + ";" + j];
                        //}
                    }
                    else
                    {
                        adultPrices = this.parentInsertedPricesDictionary[rate.Rate_UID + ";" + room.Room.UID + ";" + j];
                        childrenPrices = this.childrenInsertedPricesDictionary[rate.Rate_UID + ";" + room.Room.UID + ";" + j];
                    }

                    //childrenPrices = roomPrices.ChildrenPriceList;
                    //childrenPrices = this.childrenInsertedPricesDictionary[rate.Rate_UID + ";" + room.Room.UID + ";" + j];
                    priceVariation = priceVariationDerived;
                    isPriceVariationDecreased = isPriceVariationDecreasedDerived;
                    isPercentage = isPercentageDerived;
                    isVariation = true;
                }
                else if (roomPrices.IsPriceVariation)
                {
                    //adultPrices = this.adultPricesBeforeVariationDictionary[rate.UID + ";" + room.Room.UID + ";" + j];
                    //childrenPrices = this.childrenPricesBeforeVariationDictionary[rate.UID + ";" + room.Room.UID + ";" + j];

                    adultPrices = this.parentInsertedPricesDictionary[rate.UID + ";" + room.Room.UID + ";" + j];
                    childrenPrices = this.childrenInsertedPricesDictionary[rate.UID + ";" + room.Room.UID + ";" + j];
                    isVariation = true;
                }
                else
                {
                    if (!dayIsValid && !Data.IsInsert)
                    {
                        adultPrices = this.parentInsertedPricesDictionary[rate.UID + ";" + room.Room.UID + ";" + j];
                        childrenPrices = this.childrenInsertedPricesDictionary[rate.UID + ";" + room.Room.UID + ";" + j];
                        //childrenPrices = roomPrices.ChildrenPriceList;
                    }
                    else
                    {
                        adultPrices = roomPrices.AdultsPriceList;
                        childrenPrices = roomPrices.ChildrenPriceList;
                    }
                }

                // Insert Prices

                #region Set Adult Price

                for (var i = 0; i < roomPrices.NoOfAdultsList.Count; i++)
                {
                    if (roomPrices.IsPriceVariation && !isDerived)
                    {
                        priceVariation = roomPrices.AdultsPriceVariationList[i];
                        isPriceVariationDecreased = roomPrices.AdultPriceVariationIsValueDecrease[i];
                        isPercentage = roomPrices.AdultPriceVariationIsPercentage[i];
                    }

                    var adultNumber = roomPrices.NoOfAdultsList[i];
                    switch (adultNumber)
                    {
                        case 1:
                            tmp.Adult_1 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_1.HasValue) adultPricesTmp.Add(tmp.Adult_1.Value);
                            break;
                        case 2:
                            tmp.Adult_2 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_2.HasValue) adultPricesTmp.Add(tmp.Adult_2.Value);
                            break;
                        case 3:
                            tmp.Adult_3 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_3.HasValue) adultPricesTmp.Add(tmp.Adult_3.Value);
                            break;
                        case 4:
                            tmp.Adult_4 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_4.HasValue) adultPricesTmp.Add(tmp.Adult_4.Value);
                            break;
                        case 5:
                            tmp.Adult_5 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_5.HasValue) adultPricesTmp.Add(tmp.Adult_5.Value);
                            break;
                        case 6:
                            tmp.Adult_6 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_6.HasValue) adultPricesTmp.Add(tmp.Adult_6.Value);
                            break;
                        case 7:
                            tmp.Adult_7 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_7.HasValue) adultPricesTmp.Add(tmp.Adult_7.Value);
                            break;
                        case 8:
                            tmp.Adult_8 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_8.HasValue) adultPricesTmp.Add(tmp.Adult_8.Value);
                            break;
                        case 9:
                            tmp.Adult_9 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_9.HasValue) adultPricesTmp.Add(tmp.Adult_9.Value);
                            break;
                        case 10:
                            tmp.Adult_10 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_10.HasValue) adultPricesTmp.Add(tmp.Adult_10.Value);
                            break;
                        case 11:
                            tmp.Adult_11 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_11.HasValue) adultPricesTmp.Add(tmp.Adult_11.Value);
                            break;
                        case 12:
                            tmp.Adult_12 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_12.HasValue) adultPricesTmp.Add(tmp.Adult_12.Value);
                            break;
                        case 13:
                            tmp.Adult_13 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_13.HasValue) adultPricesTmp.Add(tmp.Adult_13.Value);
                            break;
                        case 14:
                            tmp.Adult_14 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_14.HasValue) adultPricesTmp.Add(tmp.Adult_14.Value);
                            break;
                        case 15:
                            tmp.Adult_15 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_15.HasValue) adultPricesTmp.Add(tmp.Adult_15.Value);
                            break;
                        case 16:
                            tmp.Adult_16 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_16.HasValue) adultPricesTmp.Add(tmp.Adult_16.Value);
                            break;
                        case 17:
                            tmp.Adult_17 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_17.HasValue) adultPricesTmp.Add(tmp.Adult_17.Value);
                            break;
                        case 18:
                            tmp.Adult_18 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_18.HasValue) adultPricesTmp.Add(tmp.Adult_18.Value);
                            break;
                        case 19:
                            tmp.Adult_19 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_19.HasValue) adultPricesTmp.Add(tmp.Adult_19.Value);
                            break;
                        case 20:
                            tmp.Adult_20 = CalculatePrice(adultPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid);
                            if (tmp.Adult_20.HasValue) adultPricesTmp.Add(tmp.Adult_20.Value);
                            break;
                    }
                }

                #endregion

                #region Set Children Price

                for (var i = 0; i < roomPrices.NoOfChildrenList.Count; i++)
                {
                    if (roomPrices.IsPriceVariation)
                    {
                        priceVariation = roomPrices.ChildrenPriceVariationList[i];
                        isPriceVariationDecreased = roomPrices.ChildPriceVariationIsValueDecrease[i];
                        isPercentage = roomPrices.ChildPriceVariationIsPercentage[i];
                    }
                    var adultNumber = roomPrices.NoOfChildrenList[i];
                    switch (adultNumber)
                    {
                        case 1:
                            tmp.Child_1 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased,
                                        isPercentage, dayIsValid, true);
                            if (tmp.Child_1.HasValue) childrenPricesTmp.Add(tmp.Child_1.Value);
                            break;
                        case 2:
                            tmp.Child_2 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_2.HasValue) childrenPricesTmp.Add(tmp.Child_2.Value);
                            break;
                        case 3:
                            tmp.Child_3 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_3.HasValue) childrenPricesTmp.Add(tmp.Child_3.Value);
                            break;
                        case 4:
                            tmp.Child_4 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_4.HasValue) childrenPricesTmp.Add(tmp.Child_4.Value);
                            break;
                        case 5:
                            tmp.Child_5 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_5.HasValue) childrenPricesTmp.Add(tmp.Child_5.Value);
                            break;
                        case 6:
                            tmp.Child_6 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_6.HasValue) childrenPricesTmp.Add(tmp.Child_6.Value);
                            break;
                        case 7:
                            tmp.Child_7 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_7.HasValue) childrenPricesTmp.Add(tmp.Child_7.Value);
                            break;
                        case 8:
                            tmp.Child_8 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_8.HasValue) childrenPricesTmp.Add(tmp.Child_8.Value);
                            break;
                        case 9:
                            tmp.Child_9 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_9.HasValue) childrenPricesTmp.Add(tmp.Child_9.Value);
                            break;
                        case 10:
                            tmp.Child_10 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_10.HasValue) childrenPricesTmp.Add(tmp.Child_10.Value);
                            break;
                        case 11:
                            tmp.Child_11 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_11.HasValue) childrenPricesTmp.Add(tmp.Child_11.Value);
                            break;
                        case 12:
                            tmp.Child_12 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_12.HasValue) childrenPricesTmp.Add(tmp.Child_12.Value);
                            break;
                        case 13:
                            tmp.Child_13 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_13.HasValue) childrenPricesTmp.Add(tmp.Child_13.Value);
                            break;
                        case 14:
                            tmp.Child_14 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_14.HasValue) childrenPricesTmp.Add(tmp.Child_14.Value);
                            break;
                        case 15:
                            tmp.Child_15 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_15.HasValue) childrenPricesTmp.Add(tmp.Child_15.Value);
                            break;
                        case 16:
                            tmp.Child_16 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_16.HasValue) childrenPricesTmp.Add(tmp.Child_16.Value);
                            break;
                        case 17:
                            tmp.Child_17 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_17.HasValue) childrenPricesTmp.Add(tmp.Child_17.Value);
                            break;
                        case 18:
                            tmp.Child_18 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_18.HasValue) childrenPricesTmp.Add(tmp.Child_18.Value);
                            break;
                        case 19:
                            tmp.Child_19 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_19.HasValue) childrenPricesTmp.Add(tmp.Child_19.Value);
                            break;
                        case 20:
                            tmp.Child_20 = CalculatePrice(childrenPrices[i], isDerived, isVariation, priceVariation, isPriceVariationDecreased, isPercentage, dayIsValid, true);
                            if (tmp.Child_20.HasValue) childrenPricesTmp.Add(tmp.Child_20.Value);
                            break;
                    }
                }

                #endregion

                #region ALLOTMENT

                if (!dayIsValid)
                    tmp.Allotment = OldAllotment[rate.UID + ";" + room.Room.UID + ";" + j];
                else
                {
                    if (!isDerived)
                        tmp.Allotment = roomPrices.Allotment == -1 && OldAllotment[rate.UID + ";" + room.Room.UID + ";" + j].HasValue ?
                                        OldAllotment[rate.UID + ";" + room.Room.UID + ";" + j] : roomPrices.Allotment;
                    else
                        tmp.Allotment = Data.SendAllRates ? roomPrices.Allotment :
                            OldAllotment[rate.Rate_UID + ";" + room.Room.UID + ";" + j] ?? roomPrices.Allotment;
                }

                tmp.AllotmentUsed = 0;

                #endregion

                #region RESTRICTIONS

                // Only Parent Rates
                if (!isDerived || Data.SendAllRates)
                {
                    // Restrictions
                    tmp.ClosedOnArrival = !Data.ClosedOnArrivalWeekdays[(int)date.DayOfWeek] ? null : Data.ClosedOnArrival;
                    tmp.ClosedOnDeparture = !Data.ClosedOnDepartureWeekdays[(int)date.DayOfWeek] ? null : Data.ClosedOnDeparture;
                    tmp.MaximumLengthOfStay = Data.MaximumLengthOfStay;
                    tmp.MinimumLengthOfStay = Data.MinimumLengthOfStay;
                    tmp.ReleaseDays = Data.ReleaseDays;
                    tmp.StayThrough = Data.StayThrough;

                    var realBLockedChannels = Data.RateChannels.Where(x => x.Rate_UID == item.Rate_UID)
                            .Select(x => x.Channel_UID).Intersect(Data.channelList).ToArray();

                    var blockedChannelsArray = Data.isCloseSales == true && Data.IsStoppedSaleChanged && Data.channelList.Any()
                                                ? realBLockedChannels
                                                : new long[Data.channelList.Count];

                    if (!Data.IsInsert)
                        Array.Reverse(blockedChannelsArray);

                    // Close/Open Sales
                    if (Data.CloseChannelsWeekdays[(int)date.DayOfWeek])
                        tmp.BlockedChannelsListUID = !Data.IsInsert && blockedChannelsArray.Any() ? string.Join(",", blockedChannelsArray) + "," : string.Join(",", blockedChannelsArray);
                    else
                        tmp.BlockedChannelsListUID = string.Empty;

                    tmp.isBookingEngineBlocked = (Data.isCloseSales == true && Data.IsStoppedSaleChanged && Data.channelList.Any()
                                                    && Data.channelList.Contains(1));
                }
                else
                {
                    tmp.BlockedChannelsListUID = string.Empty;
                    tmp.isBookingEngineBlocked = false;
                }

                #endregion

                tmp.ChannelsListUID = string.Join(",", Data.RateChannels.Select(x => x.Channel_UID).Distinct());
                tmp.Price = -1;

                if (!roomPrices.IsPriceVariation && dayIsValid)
                {
                    if (room.Room.AcceptsExtraBed.HasValue && !room.Room.AcceptsExtraBed.Value && !Data.IsInsert && !Data.IsAllotmentChanged)
                        tmp.ExtraBedPrice = -1;
                    else
                        tmp.ExtraBedPrice = roomPrices.ExtraBedPrice;
                }
                else
                {
                    var extraBedPriceToUpdate = this.extraBedBeforeVariationDictionary[item.Rate_UID + ";" + room.Room.UID + ";" + j];
                    tmp.ExtraBedPrice = CalculatePrice(extraBedPriceToUpdate ?? 0, isDerived, isVariation, roomPrices.ExtraBedVariationValue,
                                                        roomPrices.ExtraBedVariationIsValueDecrease,
                                                        roomPrices.ExtraBedVariationIsPercentage, dayIsValid);
                }

                tmp.RateRoom_UID = item.UID;
                tmp.Date = date;

                #region TEMPORARY VALUES

                // Save prices to use on derived rates
                // Adults
                if (!parentInsertedPricesDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j) && Data.IsInsert)
                    parentInsertedPricesDictionary.Add(item.Rate_UID + ";" + room.Room.UID + ";" + j, adultPricesTmp);
                else if (!parentUpdatedPricesDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j) && !Data.IsInsert)
                    parentUpdatedPricesDictionary.Add(item.Rate_UID + ";" + room.Room.UID + ";" + j, adultPricesTmp);
                else if (parentUpdatedPricesDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j) && !Data.IsInsert)
                    parentUpdatedPricesDictionary[item.Rate_UID + ";" + room.Room.UID + ";" + j] = adultPricesTmp;

                // Childrens
                if (!childrenInsertedPricesDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j) && Data.IsInsert)
                    childrenInsertedPricesDictionary.Add(item.Rate_UID + ";" + room.Room.UID + ";" + j, childrenPricesTmp);
                else if (!childrenUpdatedPricesDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j) && !Data.IsInsert)
                    childrenUpdatedPricesDictionary.Add(item.Rate_UID + ";" + room.Room.UID + ";" + j, childrenPricesTmp);
                else if (childrenUpdatedPricesDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j) && !Data.IsInsert)
                    childrenUpdatedPricesDictionary[item.Rate_UID + ";" + room.Room.UID + ";" + j] = childrenPricesTmp;

                // Save prices to use on variation rates
                //if (!adultPricesBeforeVariationDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j))
                //    adultPricesBeforeVariationDictionary.Add(item.Rate_UID + ";" + room.Room.UID + ";" + j, adultPricesTmp);

                //// Save prices to use on variation rates
                //if (!childrenPricesBeforeVariationDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j))
                //    childrenPricesBeforeVariationDictionary.Add(item.Rate_UID + ";" + room.Room.UID + ";" + j, childrenPricesTmp);

                // Save prices to use on variation rates
                if (!extraBedBeforeVariationDictionary.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j))
                    extraBedBeforeVariationDictionary.Add(item.Rate_UID + ";" + room.Room.UID + ";" + j, roomPrices.ExtraBedPrice);

                if (!OldAllotment.Any(x => x.Key == item.Rate_UID + ";" + room.Room.UID + ";" + j))
                    OldAllotment.Add(item.Rate_UID + ";" + room.Room.UID + ";" + j, roomPrices.Allotment);

                #endregion

                j++;
                list.Add(tmp);
            }

            return list;
        }

        private decimal CalculatePrice(decimal price, bool isDerived, bool isVariation, decimal? value, bool isPriceVariationDecreased,
                bool isPercentage, bool updatePrices, bool isChildren = false)
        {
            if (!updatePrices && !isDerived) return price;
            if ((isVariation && !value.HasValue) || !isVariation) return price;

            if (isChildren && isDerived) return price;

            // Value Increased && Valor
            if (!isPriceVariationDecreased && !isPercentage) price = Math.Round(price + value.Value, 2, MidpointRounding.ToEven);

            // Value Decreased && Valor
            else if (isPriceVariationDecreased && !isPercentage) price = Math.Round(price - value.Value, 2, MidpointRounding.ToEven);

            // Value Increased && Percentage
            else if (!isPriceVariationDecreased && isPercentage) price = Math.Round((price * ((value.Value / 100)) + price), 2, MidpointRounding.ToEven);

            // Value Decreased && Percentage
            else price = Math.Round(price - (price * (value.Value / 100)), 2, MidpointRounding.ToEven);

            return price < 0 ? 0 : price;
        }

        /// <summary>
        /// Creates the persons.
        /// </summary>
        /// <param name="isAdult">if set to <c>true</c> [is adult].</param>
        /// <param name="startPerson">The start person.</param>
        /// <param name="endPerson">The end person.</param>
        /// <param name="prices">The prices.</param>
        /// <param name="roomParameters">The room parameters.</param>
        private void CreatePersons(bool isAdult, int startPerson, int endPerson, List<decimal> prices, ref RoomPrices roomPrices)
        {
            Assert.IsFalse(startPerson > endPerson, "startPerson must be greater than endPerson");
            if (endPerson == 0 || startPerson == 0 || prices == null || !prices.Any()) return;

            // Fill number of persons
            int personCount = 0;
            for (var i = startPerson; i <= endPerson; i++)
            {
                if (isAdult)
                    roomPrices.NoOfAdultsList.Add(i);
                else
                    roomPrices.NoOfChildrenList.Add(i);

                personCount++;
            }

            Assert.IsFalse(personCount != prices.Count(), "Number of prices does not match number of persons");
            //CreatePriceList(isAdult, personCount, prices, ref roomPrices);
        }

        #endregion

        #region Assert Methods

        /// <summary>
        /// Asserts the update.
        /// </summary>
        public void AssertUpdate()
        {
            var rateRoomDetails = this.GetRateRoomDetails();
            Assert.IsTrue(ExpectedData.Count == rateRoomDetails.Count, "Number of RateRoomDetails Don't Match");

            foreach (var rateRoomDetail in rateRoomDetails)
            {
                var expectedData =
                    this.ExpectedData.FirstOrDefault(
                        x => x.RateRoom_UID == rateRoomDetail.RateRoom_UID && x.Date == rateRoomDetail.Date);

                //if (!rateRoomDetail.ClosedOnDeparture.HasValue)
                //    rateRoomDetail.ClosedOnDeparture = false;

                //if (!expectedData.ClosedOnDeparture.HasValue)
                //    expectedData.ClosedOnDeparture = false;

                var comparer = new Likeness<RateRoomDetail, RateRoomDetail>(rateRoomDetail);
                comparer =
                    comparer.Without(x => x.UID)
                        .Without(x => x.CreatedDate)
                        .Without(x => x.CreateBy)
                        .Without(x => x.ModifyBy)
                        .Without(x => x.ModifyDate)
                        .Without(x => x.correlationID)
                        .Without(x => x.IsOccupancy)
                        .Without(x => x.Revision)
                        .Without(x => x.CancellationPolicy)
                        .Without(x => x.RateChannelUpdates)
                        .Without(x => x.RateRoom)
                        //.Without(x => x.ReservationRoomDetails)
                        .Without(x => x.IsDifferentThanRate)
                        //.Without(x => x.Price)
                        .Without(x => x.BlockedChannelsListUID)
                        .Without(x => x.IsDifferentThanRate)
                        .Without(x => x.Property_UID);

                ICollection expectedChannels = new List<string>();
                ICollection dbChannels = new List<string>();


                if (expectedData.BlockedChannelsListUID != null)
                    expectedChannels = expectedData.BlockedChannelsListUID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (rateRoomDetail.BlockedChannelsListUID != null)
                    dbChannels = rateRoomDetail.BlockedChannelsListUID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();


                CollectionAssert.AreEquivalent(expectedChannels, dbChannels, "BlockedChannelsListUID don't match, expected '{0}', actual '{1}'", expectedData.BlockedChannelsListUID, rateRoomDetail.BlockedChannelsListUID);


                //Debug.WriteLine("Date {0}", rateRoomDetail.Date);
                //Debug.WriteLine("RateRoomId {0}", rateRoomDetail.UID);
                //for (int i = 1; i <= 20; i++)
                //{
                //    Debug.WriteLine("Adult_{0} - {1}", i, rateRoomDetail.GetType().GetProperty("Adult_" + i).GetValue(rateRoomDetail));
                //    Debug.WriteLine("Expected Adult_{0} - {1}", i, expectedData.GetType().GetProperty("Adult_" + i).GetValue(expectedData));
                //}
                comparer.ShouldEqual(expectedData);
            }

            // Validate AsyncTasks
            var asyncTasks = GetAsyncTasks();
            Assert.IsTrue(asyncTasks.Count > 0, "AsyncTaks Assert");
        }

        #endregion
    }
}

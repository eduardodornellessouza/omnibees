using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OB.BL.Operations.Helper
{
    public class UpdateRatesObject
    {
        public bool SendAllRates { get; set; }
        public List<Rate> Rates { get; set; }
        public List<RoomParameters> RoomAndParameters { get; set; }
        public List<Period> Periods { get; set; }
        public long PropertyId { get; set; }
        public long UserId { get; set; }
        public List<RateRoom> RateRooms { get; set; }
        //public List<RatesChannel> RateChannels { get; set; }
        public List<long> channelList { get; set; }
        public List<bool> CloseChannelsWeekdays { get; set; }

        // Restrictions
        public int? MinimumLengthOfStay { get; set; }
        public int? MaximumLengthOfStay { get; set; }
        public int? StayThrough { get; set; }
        public int? ReleaseDays { get; set; }
        public bool? ClosedOnArrival { get; set; }
        public List<bool> ClosedOnArrivalWeekdays { get; set; }
        public bool? ClosedOnDeparture { get; set; }
        public List<bool> ClosedOnDepartureWeekdays { get; set; }

        // Bool Changes
        public bool IsMinDaysChanged { get; set; }
        public bool IsMaxDaysChanged { get; set; }
        public bool IsStayThroughChanged { get; set; }
        public bool IsReleaseDaysChanged { get; set; }
        public bool IsClosedOnArrivalChanged { get; set; }
        public bool IsClosedOnDepartureChanged { get; set; }
        public bool? isCloseSales { get; set; }
        public bool IsStoppedSaleChanged { get; set; }
        public bool IsPriceChanged { get; set; }
        public bool IsAllotmentChanged { get; set; }

        public bool IsInsert { get; set; }
        public Guid CorrelationId { get; set; }
    }

    public class RoomParameters
    {
        public RoomType Room { get; set; }
        public List<RoomPrices> RoomPrices { get; set; }
    }

    public class RoomPrices
    {
        public List<int> NoOfAdultsList { get; set; }
        public List<decimal> AdultsPriceList { get; set; }
        public List<int> NoOfChildrenList { get; set; }
        public List<decimal> ChildrenPriceList { get; set; }
        public decimal? ExtraBedPrice { get; set; }
        public int? Allotment { get; set; }
        public List<bool> WeekDays { get; set; }
        public bool IsPriceVariation { get; set; }

        public List<decimal> AdultsPriceVariationList { get; set; }
        public List<bool> AdultPriceVariationIsValueDecrease { get; set; }
        public List<bool> AdultPriceVariationIsPercentage { get; set; }

        public List<decimal> ChildrenPriceVariationList { get; set; }
        public List<bool> ChildPriceVariationIsValueDecrease { get; set; }
        public List<bool> ChildPriceVariationIsPercentage { get; set; }

        public decimal ExtraBedVariationValue { get; set; }
        public bool ExtraBedVariationIsValueDecrease { get; set; }
        public bool ExtraBedVariationIsPercentage { get; set; }
    }

    public class PriceVariationValues
    {
        public decimal Value { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsValueDecreased { get; set; }
    }

    public class Period
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}

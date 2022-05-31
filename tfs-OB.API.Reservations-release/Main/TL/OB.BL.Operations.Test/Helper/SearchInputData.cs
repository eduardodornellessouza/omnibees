using OB.BL.Contracts.Data.Channels;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Operations.Test.Domain.CRM;
using OB.BL.Operations.Test.Domain.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Operations.Test.Domain;

namespace OB.Services.IntegrationTests.Helpers
{
    public class SearchInputData
    {
        public long PropertyId { get; set; }
        public long LanguageId { get; set; }
        public int RoomQuantity { get; set; }
        public BL.Contracts.Data.Rates.PromotionalCode PromoCode { get; set; }
        public PromotionalCodeRate PromoCodeRate { get; set; }
        public long BeUserId { get; set; }
        public int BeUserType { get; set; }
        public string GroupCode { get; set; }
        public List<Rate> Rates { get; set; }
        public List<RoomType> RoomTypes { get; set; }
        public List<BL.Contracts.Data.Rates.RateRoom> RateRooms { get; set; }
        public List<RatesChannel> RateChannels { get; set; }
        public List<RatesChannelsPaymentMethod> RateChannelsPaymentMethods { get; set; }
        public List<ChannelsProperty> PropertyChannels { get; set; }
        public long PackageId { get; set; }
        public int? Allotment { get; set; }
        public int? AllotmentUsed { get; set; }
        public bool IsBookingEngineBlocked { get; set; }
        public List<long> BlockedChannelsListUID { get; set; }
        public List<SearchParameters> SearchParameter { get; set; }

        // Restrictions
        public bool ClosedOnArrival { get; set; }
        public bool ClosedOnDeparture { get; set; }
        public int? MaximumLengthOfStay { get; set; }
        public int? MinimumLengthOfStay { get; set; }
        public int? ReleaseDays { get; set; }
        public int? StayThrough { get; set; }

        public bool IsUserLoggedIn { get; set; }
        public List<decimal?> AdultPrices { get; set; }
        public List<decimal?> ChildPrices { get; set; }

        // Price Variation
        public List<PriceVariation> PriceVariations { get; set; }

        public List<BL.Contracts.Data.Rates.RateRoomDetail> RateRoomDetails { get; set; }

        public List<CancellationPolicy> CancellationPolicies { get; set; }

        public List<ChildTerm> ChildTerms { get; set; }

        public ThirdPartyIntermediary Tpi { get; set; }

        public List<Incentive> Incentives { get; set; }

        public List<RatesIncentive> RatesIncentive { get; set; }
    }

    public class PriceVariation
    {
        public long RateId { get; set; }
        public bool PriceVariationIsPercentage { get; set; }
        public bool PriceVariationIsDecreased { get; set; }
        public decimal PriceVariationValue { get; set; }
    }
}

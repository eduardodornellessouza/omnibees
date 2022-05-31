using System;
using System.Collections.Generic;
namespace OB.DL.Common.QueryResultObjects
{
    public class RateRoomDetailQR1
    {
        public RateRoomDetailQR1()
        {
            AppliedIncentives = new List<IncentiveQR1>();
        }

        public long Channel_UID { get; set; }
        public long Property_UID { get; set; }
        public long Rate_UID { get; set; }
        public long RoomType_UID { get; set; }
        public System.DateTime Date { get; set; }
        public bool PriceModel { get; set; }
        public decimal AdultPrice { get; set; }
        public decimal ChildPrice { get; set; }
        public Nullable<decimal> RateModelValue { get; set; }
        public Nullable<bool> RateModelIsPercentage { get; set; }
        public bool IsCommission { get; set; }
        public bool IsPackage { get; set; }
        public bool IsMarkup { get; set; }
        public Nullable<bool> PriceAddOnIsPercentage { get; set; }
        public Nullable<bool> PriceAddOnIsValueDecrease { get; set; }
        public Nullable<decimal> PriceAddOnValue { get; set; }
        public Nullable<int> MaxFreeChilds { get; set; }

        public Nullable<long> UID { get; set; }
        public Nullable<int> RoomQty { get; set; }
        public Nullable<int> Adults { get; set; }
        public Nullable<int> Childs { get; set; }
        public Nullable<int> DateRangeCount { get; set; }
        public string PaymentMethods_UID { get; set; }
        public Nullable<int> Allotment { get; set; }
        public Nullable<int> AllotmentUsed { get; set; }
        public Nullable<int> MinimumLengthOfStay { get; set; }
        public Nullable<int> StayThrough { get; set; }
        public Nullable<int> ReleaseDays { get; set; }
        public Nullable<int> MaximumLengthOfStay { get; set; }
        public Nullable<bool> ClosedOnArrival { get; set; }
        public Nullable<bool> ClosedOnDeparture { get; set; }
        public Nullable<bool> IsAvailableToTPI { get; set; }

        public decimal PriceAfterAddOn { get; set; }
        public decimal PriceAfterPromoCodes { get; set; }
        public decimal PriceAfterLoyaltyLevel { get; set; }
        public decimal PriceAfterIncentives { get; set; }
        public decimal PriceAfterBuyerGroups { get; set; }
        public decimal PriceAfterRateModel { get; set; }
        public decimal PriceAfterExternalMarkup { get; set; }
        public decimal FinalPrice { get; set; }
        public int RPH { get; set; }
        public int CurrencyId { get; set; }

        public long? TpiUid { get; set; }
        public string Name { get; set; }

        //Applied to first line only
        public List<IncentiveQR1> AppliedIncentives { get; set; }
        public AppliedPromotionalCodeQR1 AppliedPromotionalCode { get; set; }
    }
}
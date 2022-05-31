using OB.Reservation.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateRoomDetailReservation : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? MinimumLengthOfStay { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? StayThrough { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? ReleaseDays { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? MaximumLengthOfStay { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? ClosedOnArrival { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? ClosedOnDeparture { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsAvailableToTPI { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PriceAfterAddOn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PriceAfterPromoCodes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? AllotmentUsed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PriceAfterLoyaltyLevel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PriceAfterBuyerGroups { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PriceAfterRateModel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PriceAfterExternalMarkup { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PriceAfterExternalTaxes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal FinalPrice { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RPH { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int CurrencyId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? TpiUid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PriceAfterIncentives { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? Allotment { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PaymentMethods_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? DateRangeCount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Channel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Rate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RoomType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Date { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool PriceModel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal AdultPrice { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ChildPrice { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? RateModelValue { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? RateModelIsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsCommission { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPackage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsMarkup { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? PriceAddOnIsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? PriceAddOnIsValueDecrease { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? PriceAddOnValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? MaxFreeChilds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? RoomQty { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? Adults { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? Childs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Incentive> AppliedIncentives { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ReservationRoomDetailsAppliedPromotionalCode AppliedPromotionalCode { get; set; }
    }
}

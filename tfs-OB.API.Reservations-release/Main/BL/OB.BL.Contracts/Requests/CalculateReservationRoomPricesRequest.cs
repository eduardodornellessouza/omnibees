using OB.Reservation.BL.Contracts.Data;
using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.PortalOperadoras;
using OB.Reservation.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class CalculateReservationRoomPricesRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckIn { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckOut { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long BaseCurrency { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int AdultCount { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int ChildCount { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<int> Ages { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<RateRoomDetailReservation> RrdList { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Incentive> Incentives { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ChildTerm> ChildTerms { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public RateBuyerGroup RateBuyer { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ExchangeRate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsModify { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RateId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RoomTypeId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? TpiId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? RateModelId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ChannelId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? TPIDiscountIsPercentage { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? TPIDiscountIsValueDecrease { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? TPIDiscountValue { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public LoyaltyProgram LoyaltyProgram { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long LoyaltyLevelBaseCurrencyId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public SellRule ExternalMarkupRule { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ValidPromocodeParameters ValidPromocodeParameters { get; set; }
    }
}

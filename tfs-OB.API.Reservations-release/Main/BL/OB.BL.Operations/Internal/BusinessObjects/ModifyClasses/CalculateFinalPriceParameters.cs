using OB.DL.Common.QueryResultObjects;
using OB.BL.Contracts.Data.Rates;
using OB.BL.Contracts.Data.Properties;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using OB.Reservation.BL.Contracts.Data.CRM;
using PO.BL.Contracts.Data.OperatorMarkupCommission;

namespace OB.BL.Operations.Internal.BusinessObjects.ModifyClasses
{
    public class CalculateFinalPriceParameters
    {
        public CalculateFinalPriceParameters()
        {
            IsModify = true;
        }

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public long BaseCurrency { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public List<int> Ages { get; set; }
        public List<RateRoomDetailReservation> RrdList { get; set; }
        public List<OB.BL.Contracts.Data.Properties.Incentive> Incentives { get; set; }
        public List<ChildTerm> ChildTerms { get; set; }
        public OB.BL.Contracts.Data.Rates.RateBuyerGroup RateBuyer { get; set; }
        public decimal ExchangeRate { get; set; }
        public bool IsModify { get; set; }
        public GroupRule GroupRule { get; set; }
        public long PropertyId { get; set; }
        public long RateId { get; set; }
        public long RoomTypeId { get; set; }
        public long? TpiId { get; set; }
        public int? RateModelId { get; set; }
        public long ChannelId { get; set; }
        public bool? TPIDiscountIsPercentage { get; set; }
        public bool? TPIDiscountIsValueDecrease { get; set; }
        public decimal? TPIDiscountValue { get; set; }
        public OB.BL.Contracts.Data.CRM.LoyaltyProgram LoyaltyProgram { get; set; }
        public long LoyaltyLevelBaseCurrencyId { get; set; }
        public SellRule ExternalMarkupRule { get; set; }
        public ValidPromocodeParameters ValidPromocodeParameters { get; set; }
    }
}

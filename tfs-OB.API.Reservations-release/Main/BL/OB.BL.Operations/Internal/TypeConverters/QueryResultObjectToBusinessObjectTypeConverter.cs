using OB.DL.Common.QueryResultObjects;
using System.Linq;
using contractsGeneral = OB.Reservation.BL.Contracts.Data.General;
using contractsOBProperties = OB.BL.Contracts.Data.Properties;
using contractsOBRates = OB.BL.Contracts.Data.Rates;
using contractsRates = OB.Reservation.BL.Contracts.Data.Rates;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using OBContractsReservations = OB.BL.Contracts.Data.Reservations;



namespace OB.BL.Operations.Internal.TypeConverters
{
    /// <summary>
    /// The purpose of this class is exclusively to convert classes from the QueryResultObjects repository namespace 
    /// into BusinessObjects/Data Transfer Objects or classes from the Contracts namespace.
    /// </summary>
    public class QueryResultObjectToBusinessObjectTypeConverter
    {
        #region ReservationHistory

        public static contractsReservations.ReservationHistory Convert(ReservationHistoryQR1 obj)
        {
            var newObj = new contractsReservations.ReservationHistory();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(ReservationHistoryQR1 obj, contractsReservations.ReservationHistory objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.ReservationUID = obj.ReservationUID;
            objDestination.ReservationNumber = obj.ReservationNumber;
            objDestination.Channel = obj.Channel;
            objDestination.StatusUID = obj.StatusUID;
            objDestination.Status = obj.Status;
            objDestination.UserName = obj.UserName;
            objDestination.Message = obj.Message;
            objDestination.ChangedDate = obj.ChangedDate;
        }
        #endregion Reservation

        #region CancelationPolicy

        public static OB.Reservation.BL.Contracts.Data.Rates.CancellationPolicy Convert(CancellationPolicyQR1 obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.Rates.CancellationPolicy();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(CancellationPolicyQR1 obj, OB.Reservation.BL.Contracts.Data.Rates.CancellationPolicy objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.CancelPolicyName;
            objDestination.Description = obj.CancellationPolicy_Description;
            objDestination.TranslatedName = obj.TranslatedCancelPolicyName;
            objDestination.TranslatedDescription = obj.TranslatedCancellationPolicy_Description;
            objDestination.Days = obj.CancellationDays;
            objDestination.IsCancellationAllowed = obj.IsCancellationAllowed;
            objDestination.CancellationCosts = obj.CancellationCosts;
            objDestination.Value = obj.Value;
            objDestination.PaymentModel = obj.PaymentModel;
            objDestination.NrNights = obj.NrNights;
        }

        #endregion

        #region TaxPolicy

        public static OB.Reservation.BL.Contracts.Data.Rates.TaxPolicy Convert(TaxPolicyQR1 obj)
        {
            var newObj = new OB.Reservation.BL.Contracts.Data.Rates.TaxPolicy();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(TaxPolicyQR1 obj, OB.Reservation.BL.Contracts.Data.Rates.TaxPolicy objDestination)
        {
            objDestination.Description = obj.TaxDescription;
            objDestination.IsPercentage = (bool)obj.TaxIsPercentage;
            objDestination.IsPerNight = obj.IsPerNight;
            objDestination.IsPerPerson = obj.IsPerPerson;
            objDestination.Name = obj.TaxName;
            objDestination.UID = (long)obj.TaxId;
            objDestination.Value = obj.TaxDefaultValue;
        }

        #endregion

        #region BillingTypes

        public static contractsGeneral.BillingType Convert(ExtrasBillingTypeQR1 obj)
        {
            var newObj = new contractsGeneral.BillingType();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(ExtrasBillingTypeQR1 obj, contractsGeneral.BillingType objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.Name;
            objDestination.Type = obj.Type;
        }

        #endregion BillingTypes

        #region PropertyWithReservationsForChannelOrTPI
        public static contractsReservations.PropertyWithReservationsForChannelOrTPI Convert(PropertyWithReservationsForChannelOrTPIQR1 obj)
        {
            var newObj = new contractsReservations.PropertyWithReservationsForChannelOrTPI();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(PropertyWithReservationsForChannelOrTPIQR1 obj, contractsReservations.PropertyWithReservationsForChannelOrTPI objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.Name;
        }
        #endregion

        #region OTHER POLICY

        public static contractsRates.OtherPolicy Convert(OtherPolicyQR1 obj)
        {
            var newObj = new contractsRates.OtherPolicy();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OtherPolicyQR1 obj, contractsRates.OtherPolicy objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.OtherPolicy_Name = obj.Name;
            objDestination.OtherPolicy_Description = obj.Description;
            objDestination.TranslatedName = obj.TranslatedName;
            objDestination.TranslatedDescription = obj.TranslatedDescription;
        }

        #endregion OTHER POLICY

        #region IncentiveQR1
        public static contractsOBProperties.Incentive Convert(IncentiveQR1 obj)
        {
            var newObj = new contractsOBProperties.Incentive();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(IncentiveQR1 obj, contractsOBProperties.Incentive objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Rate_UID = obj.Rate_UID;
            objDestination.IncentiveType_UID = obj.IncentiveType_UID;
            objDestination.DiscountPercentage = obj.DiscountPercentage;
            objDestination.FreeDays = obj.FreeDays;
            objDestination.Days = obj.Days;
            objDestination.IsFreeDaysAtBegin = obj.IsFreeDaysAtBegin;
            objDestination.IncentiveFrom = obj.IncentiveFrom;
            objDestination.IncentiveTo = obj.IncentiveTo;
            objDestination.IsCumulative = obj.IsCumulative;
            objDestination.Name = obj.IncentiveName;
            objDestination.TotalDiscounted = obj.TotalDiscounted;
            objDestination.DayDiscount = obj.DayDiscount;

        }
        #endregion IncentiveQR1

        #region RateRoomDetailQR1
        public static contractsOBRates.RateRoomDetailReservation Convert(RateRoomDetailQR1 obj)
        {
            var newObj = new contractsOBRates.RateRoomDetailReservation();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(RateRoomDetailQR1 obj, contractsOBRates.RateRoomDetailReservation objDestination)
        {
            objDestination.Channel_UID = obj.Channel_UID;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.RoomType_UID = obj.RoomType_UID;
            objDestination.Date = obj.Date;
            objDestination.PriceModel = obj.PriceModel;
            objDestination.AdultPrice = obj.AdultPrice;
            objDestination.ChildPrice = obj.ChildPrice;
            objDestination.RateModelValue = obj.RateModelValue;
            objDestination.RateModelIsPercentage = obj.RateModelIsPercentage;
            objDestination.IsCommission = obj.IsCommission;
            objDestination.IsPackage = obj.IsPackage;
            objDestination.IsMarkup = obj.IsMarkup;
            objDestination.PriceAddOnIsPercentage = obj.PriceAddOnIsPercentage;
            objDestination.PriceAddOnIsValueDecrease = obj.PriceAddOnIsValueDecrease;
            objDestination.PriceAddOnValue = obj.PriceAddOnValue;
            objDestination.MaxFreeChilds = obj.MaxFreeChilds;
            objDestination.UID = obj.UID;
            objDestination.RoomQty = obj.RoomQty;
            objDestination.Adults = obj.Adults;
            objDestination.Childs = obj.Childs;
            objDestination.DateRangeCount = obj.DateRangeCount;
            objDestination.PaymentMethods_UID = obj.PaymentMethods_UID;
            objDestination.Allotment = obj.Allotment;
            objDestination.AllotmentUsed = obj.AllotmentUsed;
            objDestination.MinimumLengthOfStay = obj.MinimumLengthOfStay;
            objDestination.StayThrough = obj.StayThrough;
            objDestination.ReleaseDays = obj.ReleaseDays;
            objDestination.MaximumLengthOfStay = obj.MaximumLengthOfStay;
            objDestination.ClosedOnArrival = obj.ClosedOnArrival;
            objDestination.ClosedOnDeparture = obj.ClosedOnDeparture;
            objDestination.IsAvailableToTPI = obj.IsAvailableToTPI;
            objDestination.PriceAfterAddOn = obj.PriceAfterAddOn;
            objDestination.PriceAfterPromoCodes = obj.PriceAfterPromoCodes;
            objDestination.PriceAfterLoyaltyLevel = obj.PriceAfterLoyaltyLevel;
            objDestination.PriceAfterIncentives = obj.PriceAfterIncentives;
            objDestination.PriceAfterBuyerGroups = obj.PriceAfterBuyerGroups;
            objDestination.PriceAfterRateModel = obj.PriceAfterRateModel;
            objDestination.PriceAfterExternalMarkup = obj.PriceAfterExternalMarkup;
            objDestination.FinalPrice = obj.FinalPrice;
            objDestination.RPH = obj.RPH;
            objDestination.CurrencyId = obj.CurrencyId;
            objDestination.TpiUid = obj.TpiUid;
            objDestination.Name = obj.Name;
            objDestination.AppliedIncentives = obj.AppliedIncentives.Select(x => Convert(x)).ToList();

        }
        #endregion RateRoomDetailQR1

        #region ReservationLookups
        public static ReservationLookups Convert(OBContractsReservations.ReservationLookups obj)
        {
            var newObj = new ReservationLookups();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OBContractsReservations.ReservationLookups obj, ReservationLookups objDestination)
        {
            objDestination.GuestsLookup = obj.GuestsLookup;
            objDestination.ExtrasLookup = obj.ExtrasLookup;
            objDestination.ExtrasBillingTypeLookup = obj.ExtrasBillingTypeLookup;
            objDestination.RoomTypeLookup = obj.RoomTypeLookup;
            objDestination.RatesLookup = obj.RatesLookup;
            objDestination.RateCategoriesLookup = obj.RateCategoriesLookup;
            objDestination.OtaCodesLookup = obj.OtaCodesLookup;
            objDestination.TaxPoliciesLookup = obj.TaxPoliciesLookup;
            objDestination.PromotionalCodesLookup = obj.PromotionalCodesLookup;
            objDestination.GroupCodesLookup = obj.GroupCodesLookup;
            objDestination.IncentivesLookup = obj.IncentivesLookup;
            objDestination.GuestActivitiesLookup = obj.GuestActivitiesLookup;
            objDestination.BESpecialRequestsLookup = obj.BESpecialRequestsLookup;
            objDestination.TransferLocationsLookup = obj.TransferLocationsLookup;
            if (obj.ReservationBaseCurrencyLookup != null)
                objDestination.ReservationBaseCurrencyLookup = obj.ReservationBaseCurrencyLookup.ToDictionary(x => x.Key, x => OtherConverter.Convert(x.Value));
            if (obj.ReservationCurrencyLookup != null)
                objDestination.ReservationCurrencyLookup = obj.ReservationCurrencyLookup.ToDictionary(x => x.Key, x => OtherConverter.Convert(x.Value));
            objDestination.ChannelLookup = obj.ChannelLookup.ToDictionary(x => x.Key, x => OtherConverter.Convert(x.Value));
            objDestination.ChannelOperatorLookup = obj.ChannelOperatorLookup.ToDictionary(x => x.Key, x => OtherConverter.Convert(x.Value));
            objDestination.ReservationPropertyBaseCurrencyLookup = obj.ReservationPropertyBaseCurrencyLookup.ToDictionary(x => x.Key, x => OtherConverter.Convert(x.Value));
            objDestination.CorporateAndTravelAgentLookup = obj.CorporateAndTravelAgentLookup;
            objDestination.CompanyNameLookup = obj.CompanyNameLookup;
            objDestination.PaymentMethodLookup = obj.PaymentMethodLookup;
            objDestination.PaymentMethodTypesLookup = obj.PaymentMethodTypesLookup.ToDictionary(x => x.Key, x => OtherConverter.Convert(x.Value));
            objDestination.OnRequestDecisionUserLookup = obj.OnRequestDecisionUserLookup.ToDictionary(x => x.Key, x => OtherConverter.Convert(x.Value));
            objDestination.ReferralSourcesLookup = obj.ReferralSourcesLookup;
            objDestination.ExternalSourceLookup = obj.ExternalSourceLookup;
            objDestination.CommissionTypeNamesLookup = obj.CommissionTypeNamesLookup;
            objDestination.CountryNameLookup = obj.CountryNameLookup;
            objDestination.StateNameLookup = obj.StateNameLookup;
            objDestination.TPICommissionLookup = obj.TPICommissionLookup.ToDictionary(x => x.Key, x => OtherConverter.Convert(x.Value));
            objDestination.CountryLookup = obj.CountryLookup;
            objDestination.PropertyCountryLookup = obj.PropertyCountryLookup;
        }

        #endregion ReservationLookups

        #region ReservationBEOverview
        public static Reservation.BL.Contracts.Data.Reservations.ReservationBEOverview Convert(ReservationBEOverviewQR1 obj)
        {
            if (obj == null)
                return null;

            var newObj = new Reservation.BL.Contracts.Data.Reservations.ReservationBEOverview();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(ReservationBEOverviewQR1 obj, Reservation.BL.Contracts.Data.Reservations.ReservationBEOverview objDestination)
        {
            objDestination.CommissionValue = obj.CommissionValue;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.Nights = obj.Nights;
            objDestination.PropertyBaseCurrencyExchangeRate = obj.PropertyBaseCurrencyExchangeRate;
            objDestination.PropertyBaseCurrency_UID = obj.PropertyBaseCurrency_UID;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.ReservationBaseCurrency_UID = obj.ReservationBaseCurrency_UID;
            objDestination.ReservationCommission = obj.ReservationCommission;
            objDestination.ReservationCurrencyExchangeRate = obj.ReservationCurrencyExchangeRate;
            objDestination.ReservationCurrency_UID = obj.ReservationCurrency_UID;
            objDestination.ReservationRoomNo = obj.ReservationRoomNo;
            objDestination.ReservationRoomStatus = obj.ReservationRoomStatus;
            objDestination.ReservationRoom_UID = obj.ReservationRoom_UID;
            objDestination.ReservationStatus = obj.ReservationStatus;
            objDestination.ReservationTotal = obj.ReservationTotal;
            objDestination.Reservation_UID = obj.Reservation_UID;
        }
        #endregion
    }
}

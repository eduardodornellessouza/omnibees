using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Rates;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Data.Channels;
using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Payments;
using OB.Reservation.BL.Contracts.Data.VisualStates;
using System;
using System.Collections.Generic;
using OBCRM = OB.BL.Contracts.Data.CRM;
using OBGeneral = OB.BL.Contracts.Data.General;

namespace OB.DL.Common.QueryResultObjects
{
    public class ReservationLookups
    {
        public ReservationLookups()
        {
            ExtrasLookup = new Dictionary<long, Extra>();
            TaxPoliciesLookup = new Dictionary<long, TaxPolicy>();
            RatesLookup = new Dictionary<long, Rate>();
            RateCategoriesLookup = new Dictionary<long, RateCategory>();
            OtaCodesLookup = new Dictionary<long, OtaCode>();
            PromotionalCodesLookup = new Dictionary<long, PromotionalCode>();
            GuestsLookup = new Dictionary<long, OBCRM.Guest>();
            RoomTypeLookup = new Dictionary<long, RoomType>();
            ReservationsAdditionalData = new Dictionary<long, ReservationsAdditionalData>();
            GroupCodesLookup = new Dictionary<long, GroupCode>();
            IncentivesLookup = new Dictionary<long, Incentive>();
            ExtrasBillingTypeLookup = new Dictionary<long, List<ExtrasBillingType>>();
            GuestActivitiesLookup = new Dictionary<long, List<string>>();
            BESpecialRequestsLookup = new Dictionary<long, string>();
            TransferLocationsLookup = new Dictionary<long, TransferLocation>();
            ReservationStatusNameLookup = new Dictionary<long, string>();
            ChannelLookup = new Dictionary<long, Channel>();
            ChannelOperatorLookup = new Dictionary<long, ChannelOperator>();
            ReservationPropertyBaseCurrencyLookup = new Dictionary<long, Currency>();
            ReservationReadStatusLookup = new Dictionary<long, ReservationReadStatus>();
            CompanyNameLookup = new Dictionary<long, string>();
            PaymentMethodLookup = new Dictionary<long, string>();
            PaymentMethodTypesLookup = new Dictionary<long, PaymentMethodType>();
            OnRequestDecisionUserLookup = new Dictionary<long, User>();
            ReferralSourcesLookup = new Dictionary<long, string>();
            ExternalSourceLookup = new Dictionary<long, string>();
            CommissionTypeNamesLookup = new Dictionary<long, string>();
            CountryNameLookup = new Dictionary<long, string>();
            StateNameLookup = new Dictionary<long, string>();
            CorporateAndTravelAgentLookup = new Dictionary<long, Tuple<string, string, long>>();
            ReservationBaseCurrencyLookup = new Dictionary<long, Currency>();
            ReservationCurrencyLookup = new Dictionary<long, Currency>();
            TransactionsLookup = new Dictionary<long, string>();
            TPICommissionLookup = new Dictionary<long, TPICommission>();
            CountryLookup = new Dictionary<long, OBGeneral.Country>();
            PropertyCountryLookup = new Dictionary<long, long>();
        }

        /// <summary>
        /// Dictionary with the Extra definition by Extra UID.
        /// </summary>
        public Dictionary<long, Extra> ExtrasLookup { get; set; }

        /// <summary>
        /// Dictionary with the Extra Billing Types definition by Extra UID.
        /// </summary>
        public Dictionary<long, List<ExtrasBillingType>> ExtrasBillingTypeLookup { get; set; }

        /// <summary>
        /// Dictionary with the TaxPolicy definition by TaxPolicy UID.
        /// </summary>
        public Dictionary<long, TaxPolicy> TaxPoliciesLookup { get; set; }

        /// <summary>
        /// Dictionary with the Rate definition by Rate UID
        /// </summary>
        public Dictionary<long, Rate> RatesLookup { get; set; }

        /// <summary>
        /// Dictionary with the RateCategory definition by RateCategory UID
        /// </summary>
        public Dictionary<long, RateCategory> RateCategoriesLookup { get; set; }

        /// <summary>
        /// Dictionary with the OtaCode definition by OtaCode UID
        /// </summary>
        public Dictionary<long, OtaCode> OtaCodesLookup { get; set; }

        /// <summary>
        /// Dictionary with the PromotionalCodes definition by Promotional UID
        /// </summary>
        public Dictionary<long, PromotionalCode> PromotionalCodesLookup { get; set; }

        /// <summary>
        /// List with the Rate UIDs that are packages
        /// </summary>
        //public List<long> RatePackageUIDs { get; set; }

        /// <summary>
        /// Dictionary with the Guests by UID.
        /// </summary>
        public Dictionary<long, OBCRM.Guest> GuestsLookup { get; set; }

        /// <summary>
        /// Dictionary with the RoomType by UID.
        /// </summary>
        public Dictionary<long, RoomType> RoomTypeLookup { get; set; }

        /// <summary>
        /// Dictionary with the Reservation Additional Data by UID.
        /// </summary>
        public Dictionary<long, ReservationsAdditionalData> ReservationsAdditionalData { get; set; }

        /// <summary>
        /// Dictionary with the Group Code Data by UID.
        /// </summary>
        public Dictionary<long, GroupCode> GroupCodesLookup { get; set; }

        /// <summary>
        /// Dictionary with the Incentives Data by UID.
        /// </summary>
        public Dictionary<long, Incentive> IncentivesLookup { get; set; }

        /// <summary>
        /// Dictionary with the TransactionId Data by UID.
        /// </summary>
        public Dictionary<long, string> TransactionsLookup { get; set; }


        /// <summary>
        /// Dictionary with the GuestActivities by GuestUID.
        /// </summary>
        public Dictionary<long, List<string>> GuestActivitiesLookup { get; set; }


        /// <summary>
        /// Dictionary with the BESpecialRequests.
        /// </summary>
        public Dictionary<long, string> BESpecialRequestsLookup { get; set; }


        /// <summary>
        /// Dictionary with the TransferLocations.
        /// </summary>
        public Dictionary<long, TransferLocation> TransferLocationsLookup { get; set; }

        public Dictionary<long, Currency> ReservationBaseCurrencyLookup { get; set; }

        public Dictionary<long, Currency> ReservationCurrencyLookup { get; set; }

        public Dictionary<long, string> ReservationStatusNameLookup { get; set; }

        public Dictionary<long, Channel> ChannelLookup { get; set; }

        public Dictionary<long, ChannelOperator> ChannelOperatorLookup { get; set; }

        public Dictionary<long, Currency> ReservationPropertyBaseCurrencyLookup { get; set; }

        public Dictionary<long, ReservationReadStatus> ReservationReadStatusLookup { get; set; }

        /// <summary>
        /// Dictionary with the Corporate name and travel agent name and TPI language UID.
        /// the key is corporate uid.
        /// Item1 - Corporate name.
        /// Item2 - Travel Agent name.
        /// Item3 - TPI Language_UID.
        /// </summary>
        public Dictionary<long, Tuple<string, string, long>> CorporateAndTravelAgentLookup { get; set; }

        public Dictionary<long, string> CompanyNameLookup { get; set; }

        public Dictionary<long, string> PaymentMethodLookup { get; set; }

        public Dictionary<long, PaymentMethodType> PaymentMethodTypesLookup { get; set; }

        public Dictionary<long, User> OnRequestDecisionUserLookup { get; set; }

        public Dictionary<long, string> ReferralSourcesLookup { get; set; }

        public Dictionary<long, string> ExternalSourceLookup { get; set; }

        public Dictionary<long, string> CommissionTypeNamesLookup { get; set; }

        public Dictionary<long, string> CountryNameLookup { get; set; }

        public Dictionary<long, string> StateNameLookup { get; set; }

        /// <summary>
        /// Dictionary with the TPI Commissions.
        /// </summary>        
        public Dictionary<long, TPICommission> TPICommissionLookup { get; set; }

        /// <summary>
        /// Dictionary with the property ID vs Country ID.
        /// </summary>
        public Dictionary<long, long> PropertyCountryLookup { get; set; }

        /// <summary>
        /// Dictionary with the country info.
        /// </summary>
        public Dictionary<long, OBGeneral.Country> CountryLookup { get; set; }
    }
}
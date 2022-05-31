using System;
using System.Collections.Generic;

namespace OB.DL.Common
{
    /// <summary>
    /// Request class to be used in operations that search for a specific Reservation or set of Reservations.
    /// </summary>
    public class ListReservationCriteria
    {
        public ListReservationCriteria()
        {
            ReservationUIDs = new List<long>();
            ReservationNumbers = new List<string>();
        }

        /// <summary>
        /// List of Reservation UIDs to look for.
        /// </summary>
        public List<long> ReservationUIDs { get; set; }

        /// <summary>
        /// PropertyUIDs used to look for the Reservation.
        /// </summary>
        public List<long> PropertyUIDs { get; set; }

        /// <summary>
        /// ChannelUIDs used to look for the Reservation.
        /// </summary>
        public List<long> ChannelUIDs { get; set; }

        /// <summary>
        /// List of Reservation Numbers, e.g. RES000234/01, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        public List<string> ReservationNumbers { get; set; }

        /// <summary>
        /// List of Tpi Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        public List<long> TpiIds { get; set; }

        /// <summary>
        /// List of Partner Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        public List<int> PartnerIds { get; set; }

        /// <summary>
        /// List of Partner Reservation Numbers, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        public List<string> PartnerReservationNumbers { get; set; }

        /// <summary>
        /// List of Reservation Status Codes to look for.
        /// </summary>
        public List<long> ReservationStatusCodes { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made after the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made before the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// CheckIn Date
        /// </summary>
        public DateTime? CheckIn { get; set; }

        /// <summary>
        /// CheckOut Date
        /// </summary>
        public DateTime? CheckOut { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRooms child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationRooms { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomDetails child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationRoomDetails { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomChilds child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationRoomChilds { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomExtras child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationRoomExtras { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the Extras child collection of the found Reservations.
        /// </summary>
        public bool IncludeExtras { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the Extras Billing Types child collection of the found Reservations.
        /// </summary>
        public bool IncludeExtrasBillingTypes { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomExtrasSchedules child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationRoomExtrasSchedules { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomExtrasAvailableDates child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationRoomExtrasAvailableDates { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomTaxPolicies child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationRoomTaxPolicies { get; set; }

        /// <summary>
        /// Gets or sets the flag for including the ReservationPaymentDetail reference of the found Reservations.
        /// </summary>
        public bool IncludeReservationPaymentDetail { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationPartialPaymentDetails child collection of the found Reservations.
        /// </summary>
        public bool IncludeReservationPartialPaymentDetails { get; set; }

        /// <summary>
        /// Gets or sets the flag for including the ReservationRooms child collection of the found Reservations.
        /// </summary>
        public bool IncludeGuests { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the TaxPolicies lookup collection of the found Reservations.

        public bool IncludeTaxPolicies { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the Rates lookup collection of the found Reservations -> ReservationRooms.
        /// </summary>
        public bool IncludeRates { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the RateCategories lookup collection of the found ReservationRooms -> Rates.
        /// </summary>
        public bool IncludeRateCategories { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the OtaCodes lookup collection of the found Rates -> RateCategories.
        /// </summary>
        public bool IncludeOtaCodes { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the RoomTypes lookup collection of the found Reservations -> ReservationRooms.
        /// </summary>
        public bool IncludeRoomTypes { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the PromotionalCodes lookup collection of the found Reservations -> ReservationRooms -> Rates -> PromotionalCodes.
        /// </summary>
        public bool IncludePromotionalCodes { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the PromotionalCodePackages lookup collection of the found Reservations -> ReservationRooms -> Rates -> PromotionalCodePackages ->.
        /// </summary>
        public bool IncludeRatePackages { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the RateIncentives lookup collection of the found Reservations -> ReservationRooms -> Rates -> RateIncentives
        /// </summary>
        public bool IncludeReservationRoomDetailsAppliedIncentives { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomDetailsAppliedPromotionalCode child collection of the found Reservations -> ReservationRooms -> ReservationRoomDetails -> ReservationRoomDetailsAppliedPromotionalCode.
        /// </summary>        
        public bool IncludeReservationRoomDetailsAppliedPromotionalCode { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationAdditionalData lookup collection of the found Reservations -> ReservationRooms -> Rates -> PromotionalCodePackages ->.
        /// </summary>
        public bool IncludeReservationAddicionalData { get; set; }

        /// <summary>
        /// Return lookups in this GuestActivities
        /// </summary>
        public bool IncludeGuestActivities { get; set; }

        /// <summary>
        /// Return lookups in this SpecialRequests
        /// </summary>
        public bool IncludeBESpecialRequests { get; set; }

        /// <summary>
        /// Return lookups in this TransferLocation
        /// </summary>
        public bool IncludeTransferLocation { get; set; }

        /// <summary>
        /// Return lookups with reservation base currency object
        /// </summary>
        public bool IncludeReservationBaseCurrency { get; set; }

        /// <summary>
        /// Filter reservation by status
        /// </summary>
        public List<long> ReservationStatus { get; set; }

        /// <summary>
        /// Return lookups in this language
        /// </summary>
        public long? LanguageUID { get; set; }

        /// <summary>
        /// Return lookups in this GroupCodes
        /// </summary>
        public bool IncludeGroupCodes { get; set; }

        /// <summary>
        /// Return lookups in this Incentives
        /// </summary>
        public bool IncludeIncentives { get; set; }

        /// <summary>
        /// Filter by Modified From Date
        /// </summary>
        public DateTime? ModifiedFrom { get; set; }

        /// <summary>
        /// Filter by Modified To Date
        /// </summary>
        public DateTime? ModifiedTo { get; set; }

        /// <summary>
        /// Filter by Guest Name
        /// </summary>
        //public string GuestName { get; set; }

        public List<OB.DL.Common.Filter.FilterByInfo> Filters { get; set; }

        public List<OB.DL.Common.Filter.SortByInfo> Orders { get; set; }

        public Kendo.DynamicLinq.Filter NestedFilters { get; set; }

        public bool IncludeCancelationCosts { get; set; }

        public bool IncludeReservationStatusName { get; set; }

        public bool IncludeChannel { get; set; }

        public bool IncludeChannelOperator { get; set; }

        public bool IncludePropertyBaseCurrency { get; set; }

        public bool IncludeReservationReadStatus { get; set; }

        public bool IncludeTPIName { get; set; }
        public bool IncludeTPILanguageUID { get; set; }

        public bool IncludeCompanyName { get; set; }

        public bool IncludeReservationCurrency { get; set; }
        public bool IncludePaymentMethodType { get; set; }

        public bool IncludeBillingCountryName { get; set; }
        public bool IncludeBillingStateName { get; set; }
        public bool IncludeOnRequestDecisionUser { get; set; }
        public bool IncludeReferralSource { get; set; }
        public bool IncludeExternalSource { get; set; }

        public bool IncludeCommissionTypeName { get; set; }

        public bool IncludeGuestCountryName { get; set; }
        public bool IncludeGuestStateName { get; set; }
        public bool IncludeGuestPrefixName { get; set; }
        public bool IncludeTPICommissions { get; set; }
        public bool IncludeReservationRoomIncentivePeriods { get; set; }

        public bool IncludePropertyCountry { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a specific Reservation or set of Reservations.
    /// </summary>
    [DataContract]
    public class ListReservationRequest : GridPagedRequest
    {
        public ListReservationRequest()
        {
            ReservationNumbers = new List<string>();
            IncludeGuests = true;
        }

        /// <summary>
        /// List of Reservation UIDs to look for.
        /// </summary>
        [DataMember]
        public List<long> ReservationUIDs { get; set; }

        /// <summary>
        /// PropertyUIDs used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        /// <summary>
        /// PropertyNames used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<string> PropertyNames { get; set; }

        /// <summary>
        /// ChannelUIDs used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<long> ChannelUIDs { get; set; }

        /// <summary>
        /// ExternalChannelUIDs used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<long> ExternalChannelIds { get; set; }

        /// <summary>
        /// ExternalTPIUIDs used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<long> ExternalTpiIds { get; set; }

        /// <summary>
        /// ExternalNames used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<string> ExternalNames { get; set; }


        /// <summary>
        /// ChannelNames used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<string> ChannelNames { get; set; }

        /// <summary>
        /// List of Reservation Numbers, e.g. RES000234/01, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        [DataMember]
        public List<string> ReservationNumbers { get; set; }

        /// <summary>
        /// List of Guest Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        [DataMember]
        public List<long> GuestIds { get; set; }

        /// <summary>
        /// List of Tpi Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        [DataMember]
        public List<long> TpiIds { get; set; }

        /// <summary>
        /// TPINames used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<string> TPINames { get; set; }

        /// <summary>
        /// List of Partner Ids, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        [DataMember]
        public List<int> PartnerIds { get; set; }

        /// <summary>
        /// List of Partner Reservation Numbers, that are used to look for reservations for the
        /// given property and channel pair.
        /// </summary>
        [DataMember]
        public List<string> PartnerReservationNumbers { get; set; }

        /// <summary>
        /// List of Reservation Status Codes to look for.
        /// </summary>
        [DataMember]
        public List<long> ReservationStatusCodes { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made after the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        [DataMember]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made before the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        [DataMember]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made after the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        [DataMember]
        public DateTime? ModifiedFrom { get; set; }

        /// <summary>
        /// Date (UTC) criteria to search for Reservations made before the given date (including the given day).
        /// Example: To Get all the Reservations between 06-07-2014 10:10 and 08-07-2014 23:30 you could use DateFrom:06-07-2014 , DateTo:08-07-2014
        /// The DateTime should be in UTC form.
        /// </summary>
        [DataMember]
        public DateTime? ModifiedTo { get; set; }

        /// <summary>
        /// CheckIn Date
        /// </summary>
        [DataMember]
        public DateTime? CheckIn { get; set; }

        /// <summary>
        /// CheckInTo Date to filter on the ManageReservation grid
        /// </summary>
        [DataMember]
        public DateTime? CheckInTo { get; set; }

        /// <summary>
        /// CheckOut Date
        /// </summary>
        [DataMember]
        public DateTime? CheckOut { get; set; }

        /// <summary>
        /// CheckOutFrom Date to filter on the ManageReservation grid
        /// </summary>
        [DataMember]
        public DateTime? CheckOutFrom { get; set; }

        /// <summary>
        /// List of Employee Ids to look for.
        /// </summary>
        [DataMember]
        public List<long> EmployeeIds { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRooms child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRooms { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomDetails child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomDetails { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomChilds child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomChilds { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomExtras child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomExtras { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the Extras child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeExtras { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the Extras Billing Types child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeExtrasBillingTypes { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomExtrasSchedules child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomExtrasSchedules { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomExtrasAvailableDates child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomExtrasAvailableDates { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomTaxPolicies child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomTaxPolicies { get; set; }

        /// <summary>
        /// if set to <c>true</c> and creditCardToken != null then set CardNumber = Token.
        /// </summary>
        [DataMember]
        public bool UnifyCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the flag for including the ReservationPaymentDetail reference of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationPaymentDetail { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationPartialPaymentDetails child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeReservationPartialPaymentDetails { get; set; }

        /// <summary>
        /// Gets or sets the flag for including the ReservationRooms child collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeGuests { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the TaxPolicies lookup collection of the found Reservations.
        /// </summary>
        [DataMember]
        public bool IncludeTaxPolicies { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the Rates lookup collection of the found Reservations -> ReservationRooms.
        /// </summary>
        [DataMember]
        public bool IncludeRates { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the RateCategories lookup collection of the found ReservationRooms -> Rates.
        /// </summary>
        [DataMember]
        public bool IncludeRateCategories { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the OtaCodes lookup collection of the found Rates -> RateCategories.
        /// </summary>
        [DataMember]
        public bool IncludeOtaCodes { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the RoomTypes lookup collection of the found Reservations -> ReservationRooms.
        /// </summary>
        [DataMember]
        public bool IncludeRoomTypes { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the PromotionalCodes lookup collection of the found Reservations -> ReservationRooms -> Rates -> PromotionalCodes.
        /// </summary>
        [DataMember]
        public bool IncludePromotionalCodes { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the RateIncentives lookup collection of the found Reservations -> ReservationRooms -> Rates -> RateIncentives
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomDetailsAppliedIncentives { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationRoomDetailsAppliedPromotionalCode child collection of the found Reservations -> ReservationRooms -> ReservationRoomDetails -> ReservationRoomDetailsAppliedPromotionalCode.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomDetailsAppliedPromotionalCode { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the ReservationAdditionalData lookup collection of the found Reservations -> ReservationRooms -> Rates -> PromotionalCodePackages ->.
        /// </summary>
        [DataMember]
        public bool IncludeReservationAddicionalData { get; set; }

        [DataMember]
        public bool IncludeReservationBaseCurrency { get; set; }

        [DataMember]
        public bool IncludeReservationStatusName { get; set; }

        /// <summary>
        /// Gets or sets the flag for including and initializing the RateIncentives lookup collection of the found Reservations -> ReservationRooms -> Rates -> RateIncentives
        /// </summary>
        [DataMember]
        public List<long> ReservationStatus { get; set; }

        /// <summary>
        /// GuestName used to look for the Reservation.
        /// </summary>
        [DataMember]
        public string GuestName { get; set; }

        /// <summary>
        /// NumberOfNights used to look for the Reservation.
        /// </summary>
        [DataMember]
        public int? NumberOfNights { get; set; }

        /// <summary>
        /// NumberOfAdults used to look for the Reservation.
        /// </summary>
        [DataMember]
        public int? NumberOfAdults { get; set; }

        /// <summary>
        /// NumberOfChildren used to look for the Reservation.
        /// </summary>
        [DataMember]
        public int? NumberOfChildren { get; set; }

        /// <summary>
        /// NumberOfRooms used to look for the Reservation.
        /// </summary>
        [DataMember]
        public int? NumberOfRooms { get; set; }

        /// <summary>
        /// List of PaymentType Ids, that are used to look for reservations for the
        /// </summary>
        [DataMember]
        public List<long> PaymentTypeIds { get; set; }

        /// <summary>
        /// TotalAmount used to look for the Reservation.
        /// </summary>
        [DataMember]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// ExternalTotalAmount used to look for the Reservation.
        /// </summary>
        [DataMember]
        public decimal? ExternalTotalAmount { get; set; }

        /// <summary>
        /// ExternalCommissionValue used to look for the Reservation.
        /// </summary>
        [DataMember]
        public decimal? ExternalCommissionValue { get; set; }

        /// <summary>
        /// ExternalIsPaid used to look for the Reservation.
        /// </summary>
        [DataMember]
        public bool? ExternalIsPaid { get; set; }

        /// <summary>
        /// IsPaid used to look for the Reservation.
        /// </summary>
        [DataMember]
        public bool? IsPaid { get; set; }

        /// <summary>
        /// ReservationDate used to look for the Reservation.
        /// </summary>
        [DataMember]
        public DateTime? ReservationDate { get; set; }

        /// <summary>
        /// PO_TPI_PCC used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<string> PO_TPI_PCC { get; set; }

        /// <summary>
        /// PO_TPI_Name used to look for the Reservation.
        /// </summary>
        [DataMember]
        public List<string> PO_TPI_Name { get; set; }

        /// <summary>
        /// Payment type billed with applies deposit cost
        /// </summary>
        [DataMember]
        public bool? ApplyDepositPolicy { get; set; }

        /// <summary>
        /// CreatedDate used to look for the Reservation.
        /// </summary>
        [DataMember]
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// IsOnRequest used to look for the Reservation.
        /// </summary>
        [DataMember]
        public bool? IsOnRequest { get; set; }

        /// <summary>
        /// IsReaded used to look for the Reservation.
        /// </summary>
        [DataMember]
        public bool? IsReaded { get; set; }

        /// <summary>
        /// Return lookups in this GroupCodes
        /// </summary>
        [DataMember]
        public bool IncludeGroupCodes { get; set; }


        /// <summary>
        /// Return lookups in this Incentives
        /// </summary>
        [DataMember]
        public bool IncludeIncentives { get; set; }


        /// <summary>
        /// Return Reservation Resume Info with Check-In, Check-Out, Number Of Guests, Nr of Nights, etc...
        /// </summary>
        [DataMember]
        public bool IncludeReservationResumeInfo { get; set; }


        /// <summary>
        /// Return lookups in this GuestActivities
        /// </summary>
        [DataMember]
        public bool IncludeGuestActivities { get; set; }


        /// <summary>
        /// Return lookups in this SpecialRequests
        /// </summary>
        [DataMember]
        public bool IncludeBESpecialRequests { get; set; }


        /// <summary>
        /// Return lookups in this TransferLocation
        /// </summary>
        [DataMember]
        public bool IncludeTransferLocation { get; set; }

        /// <summary>
        /// Return lookups in this TransferLocation
        /// </summary>
        [DataMember]
        public bool IncludeCancelationCosts { get; set; }

        /// <summary>
        /// Return lookups in this ReservationReadStatus
        /// </summary>
        [DataMember]
        public bool IncludeReservationReadStatus { get; set; }

        /// <summary>
        /// Return lookups in this channel
        /// </summary>
        [DataMember]
        public bool IncludeChannel { get; set; }

        /// <summary>
        /// Return lookups in this channel operator
        /// </summary>
        [DataMember]
        public bool IncludeChannelOperator { get; set; }

        /// <summary>
        /// Return lookups in this property base currency
        /// </summary>
        [DataMember]
        public bool IncludePropertyBaseCurrency { get; set; }

        /// <summary>
        /// Return lookups in this tpi name
        /// </summary>
        [DataMember]
        public bool IncludeTPIName { get; set; }

        /// <summary>
        /// Return lookups in this Tpi language
        /// </summary>
        [DataMember]
        public bool IncludeTPILanguageUID { get; set; }

        /// <summary>
        /// Return lookups in this company name
        /// </summary>
        [DataMember]
        public bool IncludeCompanyName { get; set; }

        /// <summary>
        /// Return lookups in this reservation currency
        /// </summary>
        [DataMember]
        public bool IncludeReservationCurrency { get; set; }

        /// <summary>
        /// Return lookups in this payment method type
        /// </summary>
        [DataMember]
        public bool IncludePaymentMethodType { get; set; }

        /// <summary>
        /// Return lookups in this billing country name
        /// </summary>
        [DataMember]
        public bool IncludeBillingCountryName { get; set; }

        /// <summary>
        /// Return lookups in this billing state name
        /// </summary>
        [DataMember]
        public bool IncludeBillingStateName { get; set; }

        /// <summary>
        /// Return lookups in this onrequest decision user
        /// </summary>
        [DataMember]
        public bool IncludeOnRequestDecisionUser { get; set; }

        /// <summary>
        /// Return lookups in this referral source
        /// </summary>
        [DataMember]
        public bool IncludeReferralSource { get; set; }

        /// <summary>
        /// Return lookups in this external source
        /// </summary>
        [DataMember]
        public bool IncludeExternalSource { get; set; }

        /// <summary>
        /// Return lookups in this commission type name
        /// </summary>
        [DataMember]
        public bool IncludeCommissionTypeName { get; set; }

        /// <summary>
        /// Return lookups in this guest country name
        /// </summary>
        [DataMember]
        public bool IncludeGuestCountryName { get; set; }

        /// <summary>
        /// Return lookups in this guest state name
        /// </summary>
        [DataMember]
        public bool IncludeGuestStateName { get; set; }

        /// <summary>
        /// Return lookups in this guest prefix name
        /// </summary>
        [DataMember]
        public bool IncludeGuestPrefixName { get; set; }

        /// <summary>
        /// Return lookups in this guest prefix name
        /// </summary>
        [DataMember]
        public bool IncludeTPICommissions { get; set; }

        /// <summary>
        /// Return the Payment Gatewas Transactions fields used when user is seing the details for a reservation.
        /// </summary>
        [DataMember]
        public bool IncludePaymentGatewayTransactions { get; set; }

        /// <summary>
        /// Return the all the incentives with his periods for the reservation rooms.
        /// </summary>
        [DataMember]
        public bool IncludeReservationRoomIncentivePeriods { get; set; }

        /// <summary>
        /// Set <c>true</c> to return country code of the property.
        /// </summary>
        [DataMember]
        public bool IncludePropertyCountryCode { get; set; }

        /// <summary>
        /// Filters by BigPullAuthRequestor_UID. 
        /// This UID represents the UID of the Pull user that create the reservation.
        /// </summary>
        [DataMember]
        public List<long> BigPullAuthRequestorUIDs { get; set; }

        /// <summary>
        /// Filters by BigPullAuthOwner_UID.
        /// The Big Pull Authentication User reservation owner.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> BigPullAuthOwnerUIDs { get; set; }
    }
}
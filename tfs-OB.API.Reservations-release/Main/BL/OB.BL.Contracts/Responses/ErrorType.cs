using System.ComponentModel.DataAnnotations;
namespace OB.Reservation.BL.Contracts.Responses
{
    public class ErrorType
    {
        public static readonly string InstanceNotFoundForUID = "InstanceNotFoundForUID";


        public static readonly string InvalidCreditCard = "InvalidCreditCard";


        public static readonly string InvalidRequest = "InvalidRequest";


        public static readonly string InvalidUserID = "InvalidUserID";

        public static readonly string PaymentGatewayConnection = "PaymentGatewayInvalidConfiguration";
        public static readonly string PaymentGatewayError = "PaymentGatewayError";
        public static readonly string PaymentWithoutEnoughFundsError = "PaymentWithoutEnoughFundsError";

        public static readonly string UpdateRatesError = "UpdateRatesError";
        public static readonly string CreateRatesError = "CreateRatesError";

        public static readonly string UpdateRateRoomDetailsError = "UpdateRateRoomDetailsError";

        public static readonly string UpdateReservationsError = "UpdateReservationsError";
        public static readonly string CreateReservationsError = "CreateReservationsError";
        public static readonly string CancelReservationsError = "CancelReservationsError";

        public static readonly string GenerateLinksForLoyaltyLevelToBERegisterError = "GenerateLinksForLoyaltyLevelToBERegisterError";
        public static readonly string RemoveGeneratedLinkForLoyaltyLevelToBERegisterError = "RemoveGeneratedLinkForLoyaltyLevelToBERegisterError";

        public static readonly string BigPullAuthenticationInsert_NullRequest_Error = "BigPullAuthenticationInsert_NullRequest_Error";
        public static readonly string BigPullAuthenticationInsert_InvalidRequest_Error = "BigPullAuthenticationInsert_InvalidRequest_Error";
        public static readonly string BigPullAuthenticationInsertError = "BigPullAuthenticationInsertError";
        public static readonly string BigPullAuthenticationInsert_UsernameAlreadyExists_Error = "BigPullAuthenticationInsert_UsernameAlreadyExists_Error";
        public static readonly string BigPullAuthentication_NullRequest_UpdateError = "BigPullAuthentication_NullRequest_UpdateError";
        public static readonly string BigPullAuthenticationUpdate_InvalidRequest_Error = "BigPullAuthenticationUpdate_InvalidRequest_Error";
        public static readonly string BigPullAuthenticationUpdateError = "BigPullAuthenticationUpdateError";
        public static readonly string BigPullAuthenticationUpdate_UsernameAlreadyExists_Error = "BigPullAuthenticationUpdate_UsernameAlreadyExists_Error";
        public static readonly string BigPullAuthenticationUpdate_WrongOldPassword_Error = "BigPullAuthenticationUpdate_WrongOldPassword_Error";

        //#region Reservation Validation ErrorTypes

        //public static readonly string AllotmentNotAvailable = "AllotmentNotAvailable";
        //public static readonly string InvalidPropertyChannel = "InvalidPropertyChannel";
        //public static readonly string InvalidRateChannel = "InvalidRateChannel";
        //public static readonly string InvalidPropertyChannelMapping = "InvalidPropertyChannelMapping";
        //public static readonly string InvalidAgency = "InvalidAgency";
        //public static readonly string InvalidCompanyCode = "InvalidCompanyCode";
        //public static readonly string DateRangeDaysError = "DateRangeDaysError";
        //public static readonly string InvalidCurrency = "InvalidCurrency";
        //public static readonly string InvalidBoardType = "InvalidBoardType";
        //public static readonly string OccupancyNotAvailable = "OccupancyNotAvailable";
        //public static readonly string MaxOccupancyExceeded = "MaxOccupancyExceeded";
        //public static readonly string MaxAdultsExceeded = "MaxAdultsExceeded";
        //public static readonly string MaxChildrenExceeded = "MaxChildsExceeded";
        //public static readonly string ClosedDayRestrictionError = "ClosedDayRestrictionError";
        //public static readonly string MinimumLengthOfStayRestrictionError = "MinimumLengthOfStayRestrictionError";
        //public static readonly string MaximumLengthOfStayRestrictionError = "MaximumLengthOfStayRestrictionError";
        //public static readonly string StayTroughtRestrictionError = "StayTroughtRestrictionError";
        //public static readonly string ReleaseDaysRestrictionError = "ReleaseDaysRestrictionError";
        //public static readonly string ClosedOnArrivalRestrictionError = "ClosedOnArrivalRestrictionError";
        //public static readonly string ClosedOnDepartureRestrictionError = "ClosedOnDepartureRestrictionError";
        //public static readonly string RateIsNotForSale = "RateIsNotForSale";
        //public static readonly string InvalidPaymentMethod = "InvalidPaymentMethod";
        //public static readonly string InvalidGroupCode = "InvalidGroupCode";
        //public static readonly string RateIsExclusiveForGroupCodes = "RateIsExclusiveForGroupCodes";
        //public static readonly string RateIsExclusiveForPromoCodes = "RateIsExclusiveForPromoCodes";
        //public static readonly string ReservationDoesNotExist = "ReservationDoesNotExist";
        //public static readonly string ReservationIsOnRequest = "ReservationIsOnRequest";
        //public static readonly string InvalidRoom = "InvalidRoom";
        //public static readonly string InvalidUpdate = "InvalidUpdate";
        //public static readonly string CancelationCostsAppliedError = "CancelationCostsAppliedError";
        //public static readonly string GuaranteeError = "ValidateGuaranteeError";
        //public static readonly string ChildTermsError = "ChildTermsError";
        //public static readonly string ChildrenAgesMissing = "ChildrenAgesMissing";
        //public static readonly string ReservationError = "ReservationError";
        //public static readonly string InvalidReservationBaseCurrency = "InvalidReservationBaseCurrency";
        //public static readonly string InvalidRate = "InvalidRate";
        //public static readonly string InvalidRoomType = "InvalidRoomType";
        //public static readonly string InvalidCheckIn = "InvalidCheckIn";
        //public static readonly string InvalidCheckOut = "InvalidCheckOut";
        //public static readonly string RuleTypeIsRequired = "RuleTypeIsRequired";
        //public static readonly string CancellationPolicyHasChangeError = "CancellationPolicyHasChangeError";
        //public static readonly string DepositPolicyHasChangeError = "DepositPolicyHasChangeError";
        //public static readonly string ReservationIsAlreadyCancelledError = "ReservationIsAlreadyCancelledError";
        //public static readonly string RateRoomDetailsAreNotSet = "RateRoomDetailsAreNotSet";
        //public static readonly string InvalidReservationTransaction = "InvalidReservationTransaction";

        //#endregion
    }

    /// <summary>
    /// Adicionar novos erros sempre no fim e incluir comentário com data de atualização
    /// </summary>
    public enum Errors
    {
        [Display(Description = "Something is not working well, please refer to support with the Request Id: {0}")]
        DefaultError = -1,

        [Display(Description = "The parameter '{0}' is required")]
        RequiredParameter = -2,

        [Display(Description = "The parameter '{0}' is invalid")]
        InvalidParameter = -3,

        [Display(Description = ("The parameter '{0}' exceeded the max length: {1}."))]
        ParameterMaxLengthExceeded = -4,

        [Display(Description = ("The parameter {0} cannot be after than the current date."))]
        InvalidDateAfterCurrentDate = -21,

        [Display(Description = ("The parameter {0} cannot be before than the current date."))]
        InvalidDateBeforeCurrentDate = -22,

        [Display(Description = ("The start date {0} cannot be greater than the end date {1}."))]
        StartDateExceedsEndDate = -30,

        #region RESERVATION ERRORS

        [Display(Description = "Alloment Not Available")]
        AllotmentNotAvailable = -500,

        [Display(Description = "Invalid Property Channel")]
        InvalidPropertyChannel = -501,

        [Display(Description = "Invalid Rate Channel")]
        InvalidRateChannel = -502,

        [Display(Description = "Invalid Property Channel Mapping")]
        InvalidPropertyChannelMapping = -503,

        [Display(Description = "Invalid Agency")]
        InvalidAgency = -504,

        [Display(Description = "Invalid Company Code")]
        InvalidCompanyCode = -505,

        [Display(Description = "Missig days in date range Error")]
        DateRangeDaysError = -506,

        [Display(Description = "Wrong currency Error")]
        InvalidCurrency = -507,

        [Display(Description = "Invalid BoardType")]
        InvalidBoardType = -509,

        [Display(Description = "Occupancy Not Available")]
        OccupancyNotAvailable = -510,

        [Display(Description = "Max Occupancy Exceeded")]
        MaxOccupancyExceeded = -511,

        [Display(Description = "Max Adults Exceeded")]
        MaxAdultsExceeded = -512,

        [Display(Description = "Max Children Exceeded")]
        MaxChildrenExceeded = -513,

        [Display(Description = "Closed Day Restriction Error")]
        ClosedDayRestrictionError = -514,

        [Display(Description = "Minimum Length Of Stay Restriction Error")]
        MinimumLengthOfStayRestrictionError = -515,

        [Display(Description = "Maximum Length Of Stay Restriction Error")]
        MaximumLengthOfStayRestrictionError = -516,

        [Display(Description = "Stay Trought Restriction Error")]
        StayTroughtRestrictionError = -517,

        [Display(Description = "Release Days Restriction Error")]
        ReleaseDaysRestrictionError = -518,

        [Display(Description = "Closed On Arrival Restriction Error")]
        ClosedOnArrivalRestrictionError = -519,

        [Display(Description = "Closed On Departure Restriction Error")]
        ClosedOnDepartureRestrictionError = -520,

        [Display(Description = "Rate Is Not For Sale")]
        RateIsNotForSale = -523,

        [Display(Description = "Invalid Payment Method")]
        InvalidPaymentMethod = -524,

        [Display(Description = "Invalid Group Code")]
        InvalidGroupCode = -526,

        [Display(Description = "Rate Is Exclusive For GroupCodes")]
        RateIsExclusiveForGroupCodes = -527,

        [Display(Description = "Rate Is Exclusive For PromoCodes")]
        RateIsExclusiveForPromoCodes = -528,

        [Display(Description = "Reservation Does Not Exist")]
        ReservationDoesNotExist = -529,

        [Display(Description = "Reservation Is OnRequest")]
        ReservationIsOnRequest = -560,

        [Display(Description = "Invalid RoomType Error")]
        InvalidRoom = -542,

        //[Display(Description = "Invalid Update")]
        //InvalidUpdate = "InvalidUpdate",

        [Display(Description = "Cancelation Costs Applied Error")]
        CancelationCostsAppliedError = -544,

        [Display(Description = "Guarantee type is not selected in OB")]
        GuaranteeTypeNotSelectedError = -4102,

        [Display(Description = "Invalid guarantee type information")]
        GuaranteeTypeInformationError = -4101,

        [Display(Description = "ChildTerms Error")]
        ChildTermsError = -546,

        [Display(Description = "Children Ages Missing")]
        ChildrenAgesMissing = -534,

        [Display(Description = "Invalid Children Ages")]
        InvalidChildrenAges = -533,

        [Display(Description = "Reservation Error")]
        ReservationError = -4104,

        [Display(Description = "Invalid Rate")]
        InvalidRate = -535,

        [Display(Description = "Invalid CheckIn")]
        InvalidCheckIn = -536,

        [Display(Description = "Invalid CheckOut")]
        InvalidCheckOut = -537,

        [Display(Description = "RuleType Is Required")]
        RuleTypeIsRequired = -539,

        [Display(Description = "Cancellation Policy Has Changed Error")]
        CancellationPolicyHasChangeError = -538,

        [Display(Description = "Cancel Reservation Not Allowed Error")]
        CancellationReservationNotAllowedError = -10,

        [Display(Description = "Deposit Policy Has Changed Error")]
        DepositPolicyHasChangeError = -540,

        [Display(Description = "Reservation Is Already Cancelled Error")]
        ReservationIsAlreadyCancelledError = -541,

        [Display(Description = "RoomNo \"{0}\" Is Already Cancelled Error")]
        RoomIsAlreadyCancelledError = -543,

        [Display(Description = "Channel UID \"{0}\" is not valid or Channel doesn't exist")]
        InvalidChannel = -547,

        [Display(Description = "Invalid Reservation Transaction")]
        InvalidReservationTransaction = -548,

        [Display(Description = "Invalid Reservation Transaction Status")]
        InvalidReservationTransactionStatus = -549,

        [Display(Description = "Invalid Reservation Request")]
        InvalidReservationRequest = -550,

        [Display(Description = "Tpi Invalid Payment")]
        TpiInvalidPayment = -3002,

        [Display(Description = "Operator Invalid Payment")]
        OperatorInvalidPayment = -551,

        [Display(Description = "Invalid Reservation Status")]
        InvalidReservationStatus = -552,

        [Display(Description = "Deposit Costs Applied Error")]
        DepositCostsAppliedError = -553,

        [Display(Description = "Transaction Type Is Required")]
        TransactionTypeIsRequired = -554,

        [Display(Description = "Transaction Action Is Required")]
        TransactionActionIsRequired = -555,

        [Display(Description = "Reservation Number Doesn't Match Transaction Id")]
        InvalidReservationNumber = -556,

        [Display(Description = "RateRoomDetails Are Not Set")]
        RateRoomDetailsAreNotSet = -557,

        [Display(Description = "Error processing payment")]
        ErrorProcessingPayment = -545,

        [Display(Description = "Error processing ignore pending")]
        ErrorProcessingIgnorePending = -562,

        [Display(Description = "Room Is Cancelled Error")]
        RoomIsCancelledError = -561,

        [Display(Description = "Operation was interrupted by another (Concurrency lock)")]
        OperationWasInterrupted = -558,

        [Display(Description = "Transaction Is Cancelled")]
        TransactionIsCancelled = -559,

        [Display(Description = "ChannelId Is Required")]
        ChannelIdIsRequired = -563,

        [Display(Description = "Reservations OnRequest Are Not Allowed")]
        ReservationsOnRequestAreNotAllowed = -565,

        [Display(Description = "Property Must Have Currency Defined")]
        PropertyMustHaveCurrencyDefined = -566,

        [Display(Description = "Property Id Is Required")]
        PropertyIdIsRequired = -577,

        [Display(Description = "Property Currency Is Required")]
        PropertyCurrencyIsRequired = -578,

        [Display(Description = "Reservation Prices Are Invalid")]
        ReservationPricesAreInvalid = -579,

        [Display(Description = "Markup Rules Are Invalid For This Rate")]
        MarkupRulesAreInvalidForThisRate = -580,

        [Display(Description = "Promotional Code Is Invalid For This Reservation.")]
        InvalidPromocodeForReservation = -581,

        [Display(Description = "Value not defined for selected CommissionType")]
        CommissionValueNotDefined = -582,

        [Display(Description = "Invalid Rate AvailabilityType")]
        InvalidRateAvailabilityType = -590,

        [Display(Description = "The close sales action triggered by Availability Control fails.")]
        AvailabilityControlCloseSalesFailed = -600,

        [Display(Description = "CheckOut cannot be bigger than CheckIn.")]
        CheckOutBiggerThanCheckIn = -601,

        [Display(Description = "The hotel does not have availability for this reservation")]
        InvalidCheckAvailability = -610,

        [Display(Description = "ReservationExternalIdentifier is null or empty")]
        InvalidReservationExternalIdentifier = -611,

        [Display(Description = "Reservation could not be accepted in External System")]
        InvalidReservationFromExternalSource = -612,

        [Display(Description = "Incentives Applied Error")]
        IncentivesAppliedError = -613,

        [Display(Description = "Partial payments are disabled for the current Payment Method Type")]
        PaymentMethodNotSupportPartialPayments = -614,

        #region LOYALTY

        [Display(Description = "GuestLoyaltyLevel Is Diferent From Registered")]
        GuestLoyaltyLevelIsDiferentFromRegistered = -567,

        [Display(Description = "GuestLoyaltyLevel Number Of Reservations Excedded")]
        GuestLoyaltyLevelNumberOfReservationsExcedded = -568,

        [Display(Description = "GuestLoyaltyLevel Number Of Reservation Room Nights Excedded")]
        GuestLoyaltyLevelNumberOfReservationRoomNightExcedded = -569,

        [Display(Description = "GuestLoyaltyLevel Total Reservations Value Excedded")]
        GuestLoyaltyLevelTotalReservationValueExcedded = -570,

        [Display(Description = "GuestLoyaltyLevel CheckIn And CheckOut Must Be Defined")]
        GuestLoyaltyLevelCheckInAndCheckOutMustBeDefined = -571,

        [Display(Description = "GuestLoyaltyLevel Reservation Base Currency Is Required")]
        GuestLoyaltyLevelReservationBaseCurrencyIsRequired = -572,

        [Display(Description = "GuestLoyaltyLevel Loyalty Level Base Currency Is Required")]
        GuestLoyaltyLevelLoyaltyLevelBaseCurrencyIsRequired = -573,

        [Display(Description = "Guest Id Is Required")]
        GuestIdIsRequired = -574,

        [Display(Description = "Loyalty Level Id Is Required")]
        LoyaltyLevelIdIsRequired = -575,

        [Display(Description = "Reservation Room Is Required")]
        ReservationRoomIsRequired = -576,

        #endregion

        #endregion

        #region USER ERRORS

        [Display(Description = "Invalid User")]
        InvalidUser = -700,

        #endregion

        #region PayPal ERRORS

        [Display(Description = "Paypal - Property Configurations Not Defined")]
        InvalidPayPalConfiguration = -800,

        [Display(Description = "Paypal - Failed Cancelling Recurring Payment")]
        FailedCancellingRecurringPayment = -801,

        [Display(Description = "Paypal - Failed Refund Captured Payment")]
        FailedRefundCapturedPayment = -802,

        [Display(Description = "Paypal - Failed Capture Payment")]
        FailedCapturedPayment = -803,

        [Display(Description = "Paypal - DoExpressCheckout Error")]
        FailedDoExpressCheckout = -804,

        #endregion PayPal ERRORS

        //Updated Date: 29-11-2018
        [Display(Description = ("No request sent"))]
        NoRequest = -6,

        #region Braspag Errors

        [Display(Description = "Guest Phone has special characters.")]
        InvalidGuestPhoneFormat = -400,

        [Display(Description = "Guest City has special characters or numbers.")]
        InvalidGuestCityFormat = -401,

        #endregion

        DbUpdateConcurrencyException = -1000,
    }
}
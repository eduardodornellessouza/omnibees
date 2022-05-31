
using OB.Api.Core;

namespace OB.Domain.Reservations
{
    using System;
    using System.Collections.Generic;
    using OB.Domain;

    public partial class GroupRule : DomainObject
    {
    	public static readonly DomainScope DomainScope = DomainScopes.Reservations;
    	public static readonly string PROP_NAME_GROUPTYPE = "GroupType";
    	public static readonly string PROP_NAME_BUSINESSRULES = "BusinessRules";

        public RuleType RuleType { get; set; }
        public BusinessRules BusinessRules { get; set; }
    }

    /// <summary>
    /// Business Rules Group Types
    /// </summary>
    public enum RuleType
    {
        Push = 0,
        GDS = 1,
        Pull = 2,
        PMS = 3,
        BE = 4,
        Omnibees = 5,
        PortalOperadoras = 6,
        BEAPI = 7
    }

    /// <summary>
    /// Business Rules
    /// </summary>
    [Flags]
    public enum BusinessRules
    {
        #region INSERT/MODIFY RESERVATION RULES

        // Validations
        ValidateCancelationCosts = 1 << 0,
        ValidateGuarantee = 1 << 1,
        ValidateRestrictions = 1 << 2,
        ValidateAllotment = 1 << 4,
        ValidateBookingWindow = 1 << 30,
        ValidateReservation = 1 << 31,
        ValidateRateChannelPartialPayments = 1 << 32,

        // Behavior
        HandleCancelationPolicy = 1 << 5,
        ForceDefaultCancellationPolicy = 1 << 6,
        HandleDepositPolicy = 1 << 7,
        HandlePaymentGateway = 1 << 8,
        UseReservationTransactions = 1 << 9,
        ValidateDepositCosts = 1 << 10,
        LoyaltyDiscount = 1 << 11,
        CalculatePriceModel = 1 << 12,
        GenerateReservationNumber = 1 << 13,
        EncryptCreditCard = 1 << 14,
        ConvertValuesToPropertyCurrency = 1 << 15,
        PullTpiReservationCalculation = 1 << 16,
        ReturnSellingPrices = 1 << 17,
        ApplyNewReservationFilter = 1 << 18,
        ConvertValuesToRateCurrency = 1 << 19,
        BEReservationCalculation = 1 << 20,
        GDSBuyerGroup = 1 << 21,
        PriceCalculationAbsoluteTolerance = 1 << 22,
        CalculateExtraBedPrice = 1 << 23,
        IsToPreCheckAvailability = 1 << 24,
        IgnoreAvailability = 1 << 25,
        /// <summary>
        /// Ignores the concatenation of DepositPolicy.Name with DepositPolicy.Description.
        /// </summary>
        IgnoreDepositPolicyConcat = 1 << 26,
        /// <summary>
        /// Ignores the validations of PaymentMethodTypes. If this flag is set than all payments are accepted.
        /// </summary>
        IgnorePaymentMethodTypeValidations = 1 << 27,

        ConvertValuesFromClientToRates = 1 << 28,
        CalculateStayWindowWeekDays = 1 << 29

        #endregion
    }
}

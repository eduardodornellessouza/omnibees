namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using static Constants;

    [DataContract]
    public class ReservationsAdditionalData : ContractBase
    {
        public ReservationsAdditionalData()
        {
            ReservationRoomList = new List<ReservationRoomAdditionalData>();
            ExternalSellingReservationInformationByRule = new List<ExternalSellingReservationInformation>();
        }

        /// <summary>
        /// Contains PSD2 data related to European Union directive with the same name. It might be used by payment gateways to validate payments.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public PSD2 PSD2 { get; set; }

        /// <summary>
        /// Contains PaymentGateway Details
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PaymentGatewayDetail> PaymentGatewayDetails { get; set; }

        /// <summary>
        /// ISO 3166-3
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string MarketSourceIsoCountry { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationDomain { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BookingEngineTemplate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsDirectReservation { get; set; }

        #region Partner
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> ChannelPartnerID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PartnerName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PartnerReservationNumber { get; set; }
        #endregion

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public OB.Reservation.BL.Constants.TPIType TPIType { get; set; }

        #region Agency

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyPCC { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyAddress { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyCountry { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyVATNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyIATA { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyPhone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyCity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyEmail { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AgencyCountryISO { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long AgencyCountryUID { get; set; }

        #endregion

        #region Company
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyPCC { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyAddress { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyCountry { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyVATNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyIATA { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyPhone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyCity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyEmail { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanyCountryISO { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CompanyCountryUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CompanySubsidiary { get; set; }

        #endregion

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public OB.Reservation.BL.Contracts.Data.CRM.LoyaltyLevel LoyaltyLevel { get; set; }

        #region Reservation Rooms List

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ReservationRoomAdditionalData> ReservationRoomList { get; set; }

        #endregion

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? ExternalTpiId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? ExternalChannelId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ExternalName { get; set; }

        /// <summary>
        /// External Selling Reservation Information By Rule
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ExternalSellingReservationInformation> ExternalSellingReservationInformationByRule { get; set; }

        /// <summary>
        /// When it's true, the reservation won't be sent for the PMS, because it has already been sent by CVC.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool AlreadySentToPMS { get; set; }

        /// <summary>
        /// External source id for ReservationsAdditionalData for consulting purposes.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long SourceId { get; set; }

        /// <summary>
        /// Antifraud PayU cookie from browser
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Cookie { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? BookerIsGenius { get; set; }

        /// <summary>
        /// Set <c>true</c> if it is a relaxed reservation from Pull. 
        /// This type of reservation is a non-restrictive reservation. Pull can do an update without validations.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsRelaxedReservation { get; set; }

        /// <summary>
        /// <c>True</c> if payment method type of reservation is not configured in rate (RateChannelsPaymentMethods).
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool PaymentMethodTypeNotConfiguredInRate { get; set; }
        
        /// <summary>
        /// The Big Pull Authentication User that made the reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> BigPullAuthRequestor_UID { get; set; }

        /// <summary>
        /// The Big Pull Authentication User reservation owner.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> BigPullAuthOwner_UID { get; set; }

        /// <summary>
        /// <c>True</c> if reservation is from remark (recovered reservation from lost reservations).
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsRemarkReservation { get; set; }

        /// <summary>
        /// Information the new user to recover password later.
        /// This object is only filled if the user is new.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public RecoverUserPassword NewUserRecoverPasswordInfo { get; set; }

        /// <summary>
        /// trv_reference for communications with Trivago
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TrivagoReference { get; set; }

        /// <summary>
        /// locale for communications with Trivago
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TrivagoLocale { get; set; }
    }

    public class PaymentGatewayDetail
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Installments { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AuthorizationCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Nsu { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Tid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool Authentication3DS { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ProviderReturnMessage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CaptureDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CancellationDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? Amount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Currency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Provider { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Status { get; set; }
    }

    public class ExternalSellingReservationInformation
    {
        /// <summary>
        /// KeeperUID
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long KeeperUID { get; set; }

        /// <summary>
        /// KeeperType
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public PO_KeeperType KeeperType { get; set; }

        /// <summary>
        /// Reservation Total Amount
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Rooms price with Extras/Taxes
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal RoomsTotalAmount { get; set; }

        /// <summary>
        /// Rooms price without Extras/Taxes
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal RoomsPriceSum { get; set; }

        /// <summary>
        /// Total Taxes Amount
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? TotalTax { get; set; }

        /// <summary>
        /// Total PO Taxes Amount
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? TotalPOTax { get; set; }

        /// <summary>
        /// Total Taxes Amount
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? RoomsTax { get; set; }

        /// <summary>
        /// true if reservation as already been paid, false otherwise
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsPaid { get; set; }

        /// <summary>
        /// Total Commission
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal TotalCommission { get; set; }


        #region Configurations

        /// <summary>
        /// Currency UID
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CurrencyUID { get; set; }

        /// <summary>
        /// Currency Symbol
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencySymbol { get; set; }

        /// <summary>
        /// ExchangeRate
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Markup
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal Markup { get; set; }

        /// <summary>
        /// 1-Defenir;2-Diminuir;3-Aumentar
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int MarkupType { get; set; }

        /// <summary>
        /// Markup value type (% or $)
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool MarkupIsPercentage { get; set; }

        /// <summary>
        /// Commission
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal Commission { get; set; }

        /// <summary>
        /// 1-Defenir;2-Diminuir;3-Aumentar
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int CommissionType { get; set; }

        /// <summary>
        /// Commission value type (% or $)
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool CommissionIsPercentage { get; set; }

        /// <summary>
        /// Tax
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal Tax { get; set; }

        /// <summary>
        /// Tax value type (% or $)
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool TaxIsPercentage { get; set; }

        #endregion Configurations

    }

    /// <summary>
    /// This class contains the information about the new user created in a InsertReservation.
    /// </summary>
    [DataContract]
    public class RecoverUserPassword
    {
        /// <summary>
        /// - Guest = 1;
        /// - TravelAgent = 2;
        /// - Corporate = 3;
        /// - Employee = 4;
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int UserType { get; set; }
        
        /// <summary>
        /// The email of created user
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Email { get; set; }
        
        /// <summary>
        /// The user language Id
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long LanguageId { get; set; }
    }

    public class PSD2
    {
        public T3DS T3DS { get; set; }
        public CardHolder CardHolder { get; set; }
    }

    public class T3DS
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CardSecurityCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Channel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CardNumberCollection { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ExemptionCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string MandateType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string OriginalPaymentRef { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string MerchantName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string T3DSAuthValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string TAVV { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ECI { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string DirectoryServerTrxID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AuthenticationIssues { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal PurchaseAmount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PurchaseCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string VerificationResult { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AuthenticationResult { get; set; }
    }

    public class CardHolder
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string FirstName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LastName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Address Address { get; set; }
    }

    public class Address
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AddressLine1 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AddressLine2 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AddressLine3 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public City City { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public StateProv StateProv { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PostalCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Country Country { get; set; }
    }

    public class City
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
    }

    public class StateProv
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
    }

    public class Country
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Code { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
    }
}

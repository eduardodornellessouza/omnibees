using System;
using System.Collections.Generic;

namespace OB.DL.Common.QueryResultObjects
{
    /// <summary>
    /// Information About the Reservation
    /// For Information About the ReservationRoom Please Use the Property - ReservationRooms
    /// </summary>
    ///
    [Serializable]
    public partial class ReservationDetailQR1
    {
        public long Guest_UID { get; set; }

        public string ReservationNumber { get; set; }

        public long Status { get; set; }

        public Nullable<decimal> TotalAmount { get; set; }

        public string TotalAmountFormated { get; set; }

        public Nullable<decimal> BaseTotalAmount { get; set; }

        public string BaseTotalAmountFormated { get; set; }

        public string BaseCurrentySymbol { get; set; }

        public long? BaseCurrency_UID { get; set; }

        public Nullable<int> TotalAdults { get; set; }

        public Nullable<int> TotalChildren { get; set; }

        public string PromoCodeName { get; set; }

        public string GroupCodeName { get; set; }

        public long PropertyID { get; set; }

        public string BESpecialRequests1_Name { get; set; }

        public string BESpecialRequests2_Name { get; set; }

        public string BESpecialRequests3_Name { get; set; }

        public string BESpecialRequests4_Name { get; set; }

        public string TransferLocation_Name { get; set; }

        public Nullable<decimal> TransferLocation_Price { get; set; }

        public Nullable<TimeSpan> TransferTime { get; set; }

        public string TransferOther { get; set; }

        public string CancellationPoliciy { get; set; }

        public string DepositPoliciy { get; set; }

        public string OtherPoliciy { get; set; }

        public string BillingAddress1 { get; set; }

        public string BillingAddress2 { get; set; }

        public string BillingContactName { get; set; }

        public string BillingPostalCode { get; set; }

        public string BillingCity { get; set; }

        public Nullable<long> BillingCountry_UID { get; set; }

        public string BillingCountry_Name { get; set; }

        public long? BillingStateUID { get; set; }

        public string BillingStateName { get; set; }

        public string BillingPhone { get; set; }

        public string BillingEmail { get; set; }

        public Nullable<decimal> Tax { get; set; }

        public string ChannelAffiliateName { get; set; }

        public Nullable<long> PaymentMethod_UID { get; set; }

        public string PaymentMethod_Name { get; set; }

        //OtherLoyaltyCardType_UID
        public Nullable<long> OtherLoyaltyCardType_UID { get; set; }

        //OtherLoyaltyCardNumber
        public string OtherLoyaltyCardNumber { get; set; }

        //LoyaltyCardNumber
        public string LoyaltyCardNumber { get; set; }

        // OtherLoyaltyCardType
        public string OtherLoyaltyCardTypeName { get; set; }

        public string CurrencyName { get; set; }

        public string CurrencySymbol { get; set; }

        public Nullable<DateTime> CurrencyExchangeRateDate { get; set; }

        public Nullable<decimal> ReservationCurrencyExchangeRate { get; set; }

        public Nullable<long> ReservationLanguage_UID { get; set; }

        public Nullable<long> ReservationCurrency_UID { get; set; }

        public Nullable<int> ReservationNumberOfRooms { get; set; }

        public Nullable<decimal> ReservationTotalTax { get; set; }

        public string ReservationTotalTaxFormated { get; set; }

        public Nullable<decimal> ReservationRoomsTotalTax { get; set; }

        public string ReservationRoomsTotalTaxFormated { get; set; }

        public Nullable<decimal> ReservationExtrasTotalPrice { get; set; }

        public string ReservationExtrasTotalPriceFormated { get; set; }

        public Nullable<decimal> ReservationRoomsTotalPrice { get; set; }

        public string ReservationRoomsTotalPriceFormated { get; set; }

        public string Prefix { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string PostalCode { get; set; }

        public string PhoneNo { get; set; }

        public string CountryName { get; set; }

        public string StateName { get; set; }

        public long? StateUID { get; set; }

        public Nullable<DateTime> Birthday { get; set; }

        public string City { get; set; }

        public string Email { get; set; }

        public string FacebookUser { get; set; }

        public string TwitterUser { get; set; }

        public string RoomguestName { get; set; }

        // public long RoomType_UID { get; set; }

        public string Activity_Name { get; set; }

        public string Notes { get; set; }

        public string CCType { get; set; }

        public string CardHolderName { get; set; }

        public string CardNumber { get; set; }

        public string CVV { get; set; }

        public Nullable<DateTime> ExpirationDate { get; set; }

        /// <summary>
        /// List Of the rooms Of the reservation
        /// </summary>
        public List<ReservationRoomQR1> ReservationRooms { get; set; }

        public List<OtherPolicyQR1> HotelPolicies { get; set; }

        public List<GuestActivityQR1> GuestActivities { get; set; }

        public long? ChannelUID { get; set; }

        public string ChannelName { get; set; }

        //public string ChannelAffiliateName { get; set; }
        //public string PromoCodeName { get; set; }

        public string IDCardNumber { get; set; }

        public string PaymentTypeName { get; set; }

        public int PaymentTypeCode { get; set; }

        public string BankDepositDescription { get; set; }

        public long ReservationId { get; set; }

        public Nullable<bool> IsPartialPayment { get; set; }

        public Nullable<int> NoOfInstallment { get; set; }

        public Nullable<decimal> InstallmentAmount { get; set; }

        public Nullable<decimal> InterestRate { get; set; }

        public string PartialPaymentFormatted { get; set; }

        public string InternalNotes { get; set; }

        public string InternalNotesHistory { get; set; }

        // US 3833
        public bool? UseDifferentBillingInfo { get; set; }

        public string BillingTaxCardNumber { get; set; }

        public long? Tpi_UID { get; set; }

        public long? Salesman_UID { get; set; }

        public long? Employee_UID { get; set; }

        public DateTime ReservationDate { get; set; }

        public bool? IsOnRequest { get; set; }

        public string PmsRservationNumber { get; set; }

        public bool? IsMobile { get; set; }

        public string CompanyName { get; set; }

        public long? Company_UID { get; set; }

        public long? TPICompany_UID { get; set; }

        public decimal PropertyBaseCurrencyExchangeRate { get; set; }
    }
}
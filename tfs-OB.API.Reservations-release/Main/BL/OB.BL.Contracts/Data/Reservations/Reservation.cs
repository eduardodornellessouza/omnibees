using OB.Reservation.BL.Contracts.Attributes;
using OB.Reservation.BL.Contracts.Data.Channels;
using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Payments;
using OB.Reservation.BL.Contracts.Data.VisualStates;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    [ContainsCC]
    public class Reservation : ContractBase
    {
        public Reservation()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual long Guest_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string Number { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> Channel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Channel Channel { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual ChannelOperator ChannelOperator { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GDSSource { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual System.DateTime Date { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> TotalAmount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual int Adults { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<int> Children { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual long Status { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ContainsCC]
        public virtual string Notes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ContainsCC]
        public virtual string InternalNotes { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string IPAddress { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> TPI_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> PromotionalCode_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> ChannelProperties_RateModel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> ChannelProperties_Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<bool> ChannelProperties_IsPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> InvoicesDetail_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.DateTime> ModifyDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> ModifyBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> BESpecialRequests1_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> BESpecialRequests2_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> BESpecialRequests3_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> BESpecialRequests4_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> TransferLocation_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.TimeSpan> TransferTime { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string TransferOther { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<int> CancellationPolicyDays { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string OtherPolicy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string DepositPolicy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string CancellationPolicy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingAddress1 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingAddress2 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingContactName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingPostalCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingCity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> BillingCountry_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingCountry_Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingPhone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> Tax { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ChannelAffiliateName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> PaymentMethodType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual PaymentMethodType PaymentMethodType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<int> CancelReservationReason_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string CancelReservationComments { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> OtherLoyaltyCardType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string OtherLoyaltyCardNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string LoyaltyCardNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> ReservationCurrency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> ReservationBaseCurrency_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> ReservationCurrencyExchangeRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.DateTime> ReservationCurrencyExchangeRateDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> ReservationLanguageUsed_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> TotalTax { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<int> NumberOfRooms { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> RoomsTax { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> RoomsExtras { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> RoomsPriceSum { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> RoomsTotalAmount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> GroupCode_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PmsRservationNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<bool> IsOnRequest { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> OnRequestDecisionUser { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.DateTime> OnRequestDecisionDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> BillingState_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingState_Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingEmail { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BankDepositDescription { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<bool> IsPartialPayment { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> InstallmentAmount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<int> NoOfInstallment { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> InterestRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Guest Guest { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestFirstName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestLastName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestEmail { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestPhone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestIDCardNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> GuestCountry_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestCountryName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> GuestState_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestStateName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestCity { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestAddress1 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestAddress2 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestPostalCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestMobilePhone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string GuestState { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<bool> UseDifferentBillingInfo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BillingTaxCardNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayTransactionStatusCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayTransactionStatus { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayProcessorName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayTransactionID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayTransactionEnviroment { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayTransactionType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> PaymentAmountCaptured { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayOrderID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.DateTime> PaymentGatewayTransactionDateTime { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayAutoGeneratedUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PaymentGatewayTransactionMessage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<bool> IsMobile { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<bool> IsPaid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> IsPaidDecisionUser { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.DateTime> IsPaidDecisionDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> Salesman_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> SalesmanCommission { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<bool> SalesmanIsCommissionPercentage { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string InternalNotesHistory { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> Company_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> Employee_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> TPICompany_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> PaymentGatewayTransactionUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> ReservationBaseCurrencyExchangeRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReservationBaseCurrencySymbol { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<decimal> PropertyBaseCurrencyExchangeRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string PropertyBaseCurrencySymbol { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> ReferralSourceId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReferralSourceURL
        {
            get => referralSourceURL;
            set => referralSourceURL = value?.Substring(0, Math.Min(value.Length, 800));
        }
        private string referralSourceURL;


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual IList<ReservationRoom> ReservationRooms { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual ReservationPaymentDetail ReservationPaymentDetail { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual IList<ReservationPartialPaymentDetail> ReservationPartialPaymentDetails { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual ReservationsAdditionalData ReservationAdditionalData { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Rates.PromotionalCode PromotionalCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Rates.GroupCode GroupCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Currency ReservationBaseCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Currency ReservationCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Currency PropertyCurrency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReservationStatusName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual ReservationReadStatus ReservationReadStatus { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual List<string> GuestActivities { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BESpecialRequest1 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BESpecialRequest2 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BESpecialRequest3 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string BESpecialRequest4 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string TransferLocation { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual decimal? TransferLocationPrice { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string TransactionId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual long? ExternalSource_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string CampaignName { get; set; }


        // Reservation Resume Info
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.DateTime> Resume_CheckIn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<System.DateTime> Resume_CheckOut { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<int> Resume_NrNights { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<int> Resume_TotalNrAdults { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<int> Resume_TotalNrChilds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReservationCorporateName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReservationTPIName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual long ReservationTPILanguageUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReservationCompanyName{ get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual User OnRequestDecisionGuest { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReferralSourceName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string ExternalSourceName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual TPICommission TPICommission { get; set; }

        /// <summary>
        /// The Country code of the Property.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PropertyCountryCode { get; set; }
    }
}
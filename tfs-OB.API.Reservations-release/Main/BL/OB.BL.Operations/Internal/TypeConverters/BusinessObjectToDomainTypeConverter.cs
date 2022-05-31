using OB.Reservation.BL.Contracts.Data.Reservations;
using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using contractsPayments = OB.Reservation.BL.Contracts.Data.Payments;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainPayment = OB.Domain.Payments;
using domainReservations = OB.Domain.Reservations;


namespace OB.BL.Operations.Internal.TypeConverters
{
    public static class BusinessObjectToDomainTypeConverter
    {

        #region Reservation

        public static domainReservations.Reservation Convert(contractsReservations.Reservation obj,
            ListReservationRequest request = null)
        {
            var newObj = new domainReservations.Reservation();

            Map(obj, newObj, request: request);

            return newObj;
        }

        public static void Map(contractsReservations.Reservation obj, domainReservations.Reservation objDestination, IEnumerable<string> excludeProperties = null,
            ListReservationRequest request = null)
        {
            if (excludeProperties == null)
                excludeProperties = Enumerable.Empty<string>();

            if (obj.UID > 0 && !excludeProperties.Contains(domainReservations.Reservation.PROP_NAME_UID))
                objDestination.UID = obj.UID;

            if (!excludeProperties.Contains(domainReservations.Reservation.PROP_NAME_GUEST_UID))
                objDestination.Guest_UID = obj.Guest_UID;
            objDestination.Number = obj.Number;
            objDestination.Channel_UID = obj.Channel_UID;
            objDestination.GDSSource = obj.GDSSource;
            objDestination.Date = obj.Date;
            objDestination.TotalAmount = obj.TotalAmount;
            objDestination.Adults = obj.Adults;
            objDestination.Children = obj.Children;
            objDestination.Status = obj.Status;
            objDestination.Notes = obj.Notes;
            objDestination.ExternalSource_UID = obj.ExternalSource_UID;
            objDestination.CampaignName = obj.CampaignName;

            if (!excludeProperties.Contains(domainReservations.Reservation.PROP_NAME_INTERNALNOTES))
                objDestination.InternalNotes = obj.InternalNotes;

            objDestination.IPAddress = obj.IPAddress;
            objDestination.TPI_UID = !obj.TPI_UID.HasValue || obj.TPI_UID <= 0 ? null : obj.TPI_UID;
            objDestination.PromotionalCode_UID = obj.PromotionalCode_UID;
            objDestination.ChannelProperties_RateModel_UID = obj.ChannelProperties_RateModel_UID;
            objDestination.ChannelProperties_Value = obj.ChannelProperties_Value;
            objDestination.ChannelProperties_IsPercentage = obj.ChannelProperties_IsPercentage;
            objDestination.InvoicesDetail_UID = obj.InvoicesDetail_UID;
            objDestination.Property_UID = obj.Property_UID;

            if (!excludeProperties.Contains(domainReservations.Reservation.PROP_NAME_CREATEDDATE))
                objDestination.CreatedDate = obj.CreatedDate;
            if (!excludeProperties.Contains(domainReservations.Reservation.PROP_NAME_CREATEBY))
                objDestination.CreateBy = obj.CreateBy;

            if (!excludeProperties.Contains(domainReservations.Reservation.PROP_NAME_MODIFYDATE))
                objDestination.ModifyDate = obj.ModifyDate;
            if (!excludeProperties.Contains(domainReservations.Reservation.PROP_NAME_MODIFYBY))
                objDestination.ModifyBy = obj.ModifyBy;


            objDestination.BESpecialRequests1_UID = obj.BESpecialRequests1_UID;
            objDestination.BESpecialRequests2_UID = obj.BESpecialRequests2_UID;
            objDestination.BESpecialRequests3_UID = obj.BESpecialRequests3_UID;
            objDestination.BESpecialRequests4_UID = obj.BESpecialRequests4_UID;
            objDestination.TransferLocation_UID = obj.TransferLocation_UID;
            objDestination.TransferTime = obj.TransferTime;
            objDestination.TransferOther = obj.TransferOther;
            objDestination.CancellationPolicyDays = obj.CancellationPolicyDays;
            objDestination.OtherPolicy = obj.OtherPolicy;
            objDestination.DepositPolicy = obj.DepositPolicy;
            objDestination.CancellationPolicy = obj.CancellationPolicy;
            objDestination.BillingAddress1 = obj.BillingAddress1;
            objDestination.BillingAddress2 = obj.BillingAddress2;
            objDestination.BillingContactName = obj.BillingContactName;
            objDestination.BillingPostalCode = obj.BillingPostalCode;
            objDestination.BillingCity = obj.BillingCity;
            objDestination.BillingCountry_UID = obj.BillingCountry_UID;
            objDestination.BillingPhone = obj.BillingPhone;
            objDestination.Tax = obj.Tax;
            objDestination.ChannelAffiliateName = obj.ChannelAffiliateName;
            objDestination.PaymentMethodType_UID = obj.PaymentMethodType_UID;
            objDestination.CancelReservationReason_UID = obj.CancelReservationReason_UID;
            objDestination.CancelReservationComments = obj.CancelReservationComments;
            objDestination.OtherLoyaltyCardType_UID = obj.OtherLoyaltyCardType_UID;
            objDestination.OtherLoyaltyCardNumber = obj.OtherLoyaltyCardNumber;
            objDestination.LoyaltyCardNumber = obj.LoyaltyCardNumber;
            objDestination.ReservationCurrency_UID = obj.ReservationCurrency_UID;
            objDestination.ReservationBaseCurrency_UID = obj.ReservationBaseCurrency_UID;
            objDestination.ReservationCurrencyExchangeRate = obj.ReservationCurrencyExchangeRate;
            objDestination.ReservationCurrencyExchangeRateDate = obj.ReservationCurrencyExchangeRateDate;
            objDestination.ReservationLanguageUsed_UID = obj.ReservationLanguageUsed_UID;
            objDestination.TotalTax = obj.TotalTax;
            objDestination.NumberOfRooms = obj.NumberOfRooms;
            objDestination.RoomsTax = obj.RoomsTax;
            objDestination.RoomsExtras = obj.RoomsExtras;
            objDestination.RoomsPriceSum = obj.RoomsPriceSum;
            objDestination.RoomsTotalAmount = obj.RoomsTotalAmount;
            objDestination.GroupCode_UID = obj.GroupCode_UID;
            objDestination.PmsRservationNumber = String.IsNullOrEmpty(obj.PmsRservationNumber) ? objDestination.PmsRservationNumber : obj.PmsRservationNumber;
            objDestination.IsOnRequest = obj.IsOnRequest;
            objDestination.OnRequestDecisionUser = obj.OnRequestDecisionUser;
            objDestination.OnRequestDecisionDate = obj.OnRequestDecisionDate;
            objDestination.BillingState_UID = obj.BillingState_UID;
            objDestination.BillingEmail = obj.BillingEmail;
            objDestination.BankDepositDescription = obj.BankDepositDescription;
            objDestination.IsPartialPayment = obj.IsPartialPayment;
            objDestination.InstallmentAmount = obj.InstallmentAmount;
            objDestination.NoOfInstallment = obj.NoOfInstallment;
            objDestination.InterestRate = obj.InterestRate;
            objDestination.GuestFirstName = obj.GuestFirstName;
            objDestination.GuestLastName = obj.GuestLastName;
            objDestination.GuestEmail = obj.GuestEmail;
            objDestination.GuestPhone = obj.GuestPhone;
            objDestination.GuestIDCardNumber = obj.GuestIDCardNumber;
            objDestination.GuestCountry_UID = obj.GuestCountry_UID;
            objDestination.GuestState_UID = obj.GuestState_UID;
            objDestination.GuestCity = obj.GuestCity;
            objDestination.GuestAddress1 = obj.GuestAddress1;
            objDestination.GuestAddress2 = obj.GuestAddress2;
            objDestination.GuestPostalCode = obj.GuestPostalCode;
            objDestination.UseDifferentBillingInfo = obj.UseDifferentBillingInfo;
            objDestination.BillingTaxCardNumber = obj.BillingTaxCardNumber;
            objDestination.PaymentGatewayTransactionStatusCode = obj.PaymentGatewayTransactionStatusCode;
            objDestination.PaymentGatewayName = obj.PaymentGatewayName;
            objDestination.PaymentGatewayProcessorName = obj.PaymentGatewayProcessorName;
            objDestination.PaymentGatewayTransactionID = obj.PaymentGatewayTransactionID;
            objDestination.PaymentGatewayTransactionUID = obj.PaymentGatewayTransactionUID;
            objDestination.PaymentAmountCaptured = obj.PaymentAmountCaptured;
            objDestination.PaymentGatewayOrderID = obj.PaymentGatewayOrderID;
            objDestination.PaymentGatewayTransactionDateTime = obj.PaymentGatewayTransactionDateTime;
            objDestination.PaymentGatewayAutoGeneratedUID = obj.PaymentGatewayAutoGeneratedUID;
            objDestination.PaymentGatewayTransactionMessage = obj.PaymentGatewayTransactionMessage;
            objDestination.IsMobile = obj.IsMobile;
            objDestination.IsPaid = obj.IsPaid;
            objDestination.IsPaidDecisionUser = obj.IsPaidDecisionUser;
            objDestination.IsPaidDecisionDate = obj.IsPaidDecisionDate;
            objDestination.Salesman_UID = obj.Salesman_UID;
            objDestination.SalesmanCommission = obj.SalesmanCommission;
            objDestination.SalesmanIsCommissionPercentage = obj.SalesmanIsCommissionPercentage;

            if (!excludeProperties.Contains(domainReservations.Reservation.PROP_NAME_INTERNALNOTESHISTORY))
                objDestination.InternalNotesHistory = obj.InternalNotesHistory;

            objDestination.Company_UID = obj.Company_UID;
            objDestination.Employee_UID = obj.Employee_UID;
            objDestination.TPICompany_UID = obj.TPICompany_UID;
            objDestination.PropertyBaseCurrencyExchangeRate = obj.PropertyBaseCurrencyExchangeRate;
            objDestination.ReferralSourceId = obj.ReferralSourceId;
            objDestination.ReferralSourceURL = obj.ReferralSourceURL;

            if (obj.ReservationRooms != null && (request != null && (request.IncludeReservationRooms || request.IncludeReservationRoomExtras || request.IncludeExtras
                || request.IncludeReservationRoomDetails || request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeReservationRoomDetailsAppliedPromotionalCode || request.IncludeIncentives
                || request.IncludeReservationRoomChilds || request.IncludeReservationRoomTaxPolicies || request.IncludeReservationRoomExtrasSchedules)))
                objDestination.ReservationRooms = new List<domainReservations.ReservationRoom>(obj.ReservationRooms.Select(x => Convert(x, request)));

            if (obj.ReservationPaymentDetail != null && request != null && request.IncludeReservationPaymentDetail)
                objDestination.ReservationPaymentDetails = new List<domainReservations.ReservationPaymentDetail>() { Convert(obj.ReservationPaymentDetail) };

            if (obj.ReservationPartialPaymentDetails != null && request != null && request.IncludeReservationPartialPaymentDetails)
                objDestination.ReservationPartialPaymentDetails = new List<domainReservations.ReservationPartialPaymentDetail>(obj.ReservationPartialPaymentDetails.Select(x => Convert(x)));

        }

        #endregion   Reservation

        #region ReservationRoom

        public static domainReservations.ReservationRoom Convert(contractsReservations.ReservationRoom obj, ListReservationRequest request = null)
        {
            var newObj = new domainReservations.ReservationRoom();

            Map(obj, newObj, request: request);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationRoom obj, domainReservations.ReservationRoom objDestination, IEnumerable<string> excludeProperties = null,
            ListReservationRequest request = null)
        {
            if (excludeProperties == null)
                excludeProperties = Enumerable.Empty<string>();

            if (obj.UID > 0 && !excludeProperties.Contains(domainReservations.ReservationRoom.PROP_NAME_UID))
                objDestination.UID = obj.UID;


            objDestination.Reservation_UID = obj.Reservation_UID;
            objDestination.RoomType_UID = obj.RoomType_UID;
            objDestination.GuestName = obj.GuestName;
            objDestination.GuestEmail = obj.GuestEmail;
            objDestination.SmokingPreferences = obj.SmokingPreferences;
            objDestination.DateFrom = obj.DateFrom.HasValue ? obj.DateFrom.Value.Date : obj.DateFrom;
            objDestination.DateTo = obj.DateTo.HasValue ? obj.DateTo.Value.Date : obj.DateTo;
            objDestination.AdultCount = obj.AdultCount;
            objDestination.ChildCount = obj.ChildCount;
            objDestination.TotalTax = obj.TotalTax;
            objDestination.Package_UID = obj.Package_UID;
            objDestination.CancellationPolicy = obj.CancellationPolicy;
            objDestination.DepositPolicy = obj.DepositPolicy;
            objDestination.OtherPolicy = obj.OtherPolicy;
            objDestination.CancellationPolicyDays = obj.CancellationPolicyDays;
            objDestination.ReservationRoomNo = obj.ReservationRoomNo;
            objDestination.Status = obj.Status;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.RoomName = obj.RoomName;
            objDestination.Rate_UID = obj.Rate_UID;
            objDestination.IsCanceledByChannels = obj.IsCanceledByChannels;
            objDestination.ReservationRoomsPriceSum = obj.ReservationRoomsPriceSum;
            objDestination.ReservationRoomsExtrasSum = obj.ReservationRoomsExtrasSum;
            objDestination.ReservationRoomsTotalAmount = obj.ReservationRoomsTotalAmount;
            objDestination.ArrivalTime = obj.ArrivalTime;
            objDestination.PmsRservationNumber = obj.PmsRservationNumber;
            objDestination.TPIDiscountIsPercentage = obj.TPIDiscountIsPercentage;
            objDestination.TPIDiscountIsValueDecrease = obj.TPIDiscountIsValueDecrease;
            objDestination.TPIDiscountValue = obj.TPIDiscountValue;
            objDestination.IsCancellationAllowed = obj.IsCancellationAllowed;
            objDestination.CancellationCosts = obj.CancellationCosts;
            objDestination.CancellationValue = obj.CancellationValue;
            objDestination.CancellationPaymentModel = obj.CancellationPaymentModel;
            objDestination.CancellationNrNights = obj.CancellationNrNights;
            objDestination.CommissionType = obj.CommissionType;
            objDestination.CommissionValue = obj.CommissionValue;
            objDestination.CancellationDate = obj.CancellationDate;
            objDestination.LoyaltyLevel_UID = obj.LoyaltyLevel_UID;
            objDestination.LoyaltyLevelName = obj.LoyaltyLevelName;

            if (obj.ReservationRoomTaxPolicies != null)
                objDestination.ReservationRoomTaxPolicies = new List<domainReservations.ReservationRoomTaxPolicy>(obj.ReservationRoomTaxPolicies.Select(x => Convert(x)));

            if (obj.ReservationRoomChilds != null && request != null && request.IncludeReservationRoomChilds)
                objDestination.ReservationRoomChilds = new List<domainReservations.ReservationRoomChild>(obj.ReservationRoomChilds.Select(x => Convert(x)));

            if (obj.ReservationRoomDetails != null && (request != null && (request.IncludeReservationRoomDetails || request.IncludeReservationRoomDetailsAppliedIncentives
                || request.IncludeReservationRoomDetailsAppliedPromotionalCode || request.IncludeIncentives)))
                objDestination.ReservationRoomDetails = new List<domainReservations.ReservationRoomDetail>(obj.ReservationRoomDetails.Select(x => Convert(x,
                    request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeIncentives, request.IncludeReservationRoomDetailsAppliedPromotionalCode)));

            if (obj.ReservationRoomExtras != null && (request != null && (request.IncludeReservationRoomExtras || request.IncludeReservationRoomExtrasSchedules || request.IncludeExtras)))
                objDestination.ReservationRoomExtras = new List<domainReservations.ReservationRoomExtra>(obj.ReservationRoomExtras.Select(x => Convert(x, request.IncludeReservationRoomExtrasSchedules)));
        }

        #endregion ReservationRoom       

        #region ReservationRoomTaxPolicy
        public static domainReservations.ReservationRoomTaxPolicy Convert(contractsReservations.ReservationRoomTaxPolicy obj)
        {
            var newObj = new domainReservations.ReservationRoomTaxPolicy();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(ReservationRoomTaxPolicy obj, domainReservations.ReservationRoomTaxPolicy objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.BillingType = obj.BillingType;
            objDestination.ReservationRoom_UID = obj.ReservationRoom_UID;
            objDestination.TaxCalculatedValue = obj.TaxCalculatedValue;
            objDestination.TaxDefaultValue = obj.TaxDefaultValue;
            objDestination.TaxDescription = obj.TaxDescription;

            objDestination.TaxId = obj.TaxId;
            objDestination.TaxIsPercentage = obj.TaxIsPercentage;
            objDestination.TaxName = obj.TaxName;
        }
        #endregion ReservationRoomTaxPolicy

        #region ReservationRoomDetail

        public static domainReservations.ReservationRoomDetail Convert(contractsReservations.ReservationRoomDetail obj, bool includeReservationRoomDetailAppliedIncentives = false, bool includeReservationRoomDetailAppliedPromotionalCodes = false)
        {
            var newObj = new domainReservations.ReservationRoomDetail();

            Map(obj, newObj, null, includeReservationRoomDetailAppliedIncentives, includeReservationRoomDetailAppliedPromotionalCodes);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationRoomDetail obj, domainReservations.ReservationRoomDetail objDestination, IEnumerable<string> excludeProperties = null,
            bool includeReservationRoomDetailAppliedIncentives = false, bool includeReservationRoomDetailAppliedPromotionalCodes = false)
        {
            if (excludeProperties == null)
                excludeProperties = Enumerable.Empty<string>();

            if (obj.UID > 0 && !excludeProperties.Contains(domainReservations.ReservationRoomDetail.PROP_NAME_UID))
                objDestination.UID = obj.UID;


            objDestination.RateRoomDetails_UID = obj.RateRoomDetails_UID;
            objDestination.Price = obj.Price;
            objDestination.ReservationRoom_UID = obj.ReservationRoom_UID;
            objDestination.AdultPrice = obj.AdultPrice;
            objDestination.ChildPrice = obj.ChildPrice;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.Date = obj.Date;
            objDestination.Rate_UID = obj.Rate_UID;

            if (obj.ReservationRoomDetailsAppliedIncentives != null && includeReservationRoomDetailAppliedIncentives)
                objDestination.ReservationRoomDetailsAppliedIncentives = new List<domainReservations.ReservationRoomDetailsAppliedIncentive>(obj.ReservationRoomDetailsAppliedIncentives
                    .Select(x => Convert(x)));

            if (obj.ReservationRoomDetailsAppliedPromotionalCode != null && includeReservationRoomDetailAppliedPromotionalCodes)
                objDestination.ReservationRoomDetailsAppliedPromotionalCodes = new List<domainReservations.ReservationRoomDetailsAppliedPromotionalCode>
                {
                    Convert(obj.ReservationRoomDetailsAppliedPromotionalCode)
                };
        }

        #endregion ReservationRoomDetail

        #region ReservationRoomDetailsAppliedIncentive

        public static domainReservations.ReservationRoomDetailsAppliedIncentive Convert(contractsReservations.ReservationRoomDetailsAppliedIncentive obj)
        {
            var newObj = new domainReservations.ReservationRoomDetailsAppliedIncentive();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationRoomDetailsAppliedIncentive obj, domainReservations.ReservationRoomDetailsAppliedIncentive objDestination, IEnumerable<string> excludeProperties = null)
        {
            if (excludeProperties == null)
                excludeProperties = Enumerable.Empty<string>();

            objDestination.UID = obj.UID;
            objDestination.ReservationRoomDetails_UID = obj.ReservationRoomDetails_UID;
            objDestination.Name = obj.Name;
            objDestination.Days = obj.Days;
            objDestination.FreeDays = obj.FreeDays;
            objDestination.DiscountPercentage = obj.DiscountPercentage;
            objDestination.IsFreeDaysAtBegin = obj.IsFreeDaysAtBegin;
            objDestination.Incentive_UID = obj.Incentive_UID;
            objDestination.DiscountValue = obj.DiscountValue;
        }
        #endregion ReservationRoomDetail

        #region ReservationRoomDetailsAppliedPromotionalCode

        public static domainReservations.ReservationRoomDetailsAppliedPromotionalCode Convert(contractsReservations.ReservationRoomDetailsAppliedPromotionalCode obj)
        {
            var newObj = new domainReservations.ReservationRoomDetailsAppliedPromotionalCode();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationRoomDetailsAppliedPromotionalCode obj, domainReservations.ReservationRoomDetailsAppliedPromotionalCode objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.ReservationRoomDetail_UID = obj.ReservationRoomDetail_UID;
            objDestination.PromotionalCode_UID = obj.PromotionalCode_UID;
            objDestination.Date = obj.Date;
            objDestination.DiscountValue = obj.DiscountValue;
            objDestination.DiscountPercentage = obj.DiscountPercentage;
        }

        #endregion ReservationRoomDetailsAppliedPromotionalCode

        #region ReservationRoomChild

        public static domainReservations.ReservationRoomChild Convert(contractsReservations.ReservationRoomChild obj)
        {
            var newObj = new domainReservations.ReservationRoomChild();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationRoomChild obj, domainReservations.ReservationRoomChild objDestination, IEnumerable<string> excludeProperties = null)
        {
            if (excludeProperties == null)
                excludeProperties = Enumerable.Empty<string>();

            if (obj.UID > 0 && !excludeProperties.Contains(domainReservations.ReservationRoomChild.PROP_NAME_UID))
                objDestination.UID = obj.UID;

            objDestination.Age = obj.Age;
            objDestination.ChildNo = obj.ChildNo;
        }

        #endregion ReservationRoomChild

        #region ReservationPaymentDetail

        public static domainReservations.ReservationPaymentDetail Convert(contractsReservations.ReservationPaymentDetail obj)
        {
            var newObj = new domainReservations.ReservationPaymentDetail();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationPaymentDetail obj, domainReservations.ReservationPaymentDetail objDestination, IEnumerable<string> excludeProperties = null)
        {
            if (excludeProperties == null)
                excludeProperties = Enumerable.Empty<string>();


            if (obj.UID > 0 && !excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_UID))
                objDestination.UID = obj.UID;

            if (obj.PaymentMethod_UID > 0 && !excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_PAYMENTMETHOD_UID))
                objDestination.PaymentMethod_UID = obj.PaymentMethod_UID;

            if (obj.Reservation_UID > 0 && !excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_RESERVATION_UID))
                objDestination.Reservation_UID = obj.Reservation_UID;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_AMOUNT))
                objDestination.Amount = obj.Amount;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_CURRENCY_UID))
                objDestination.Currency_UID = obj.Currency_UID;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_CVV))
                objDestination.CVV = obj.CVV;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_CARDNAME))
                objDestination.CardName = obj.CardName;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_CARDNUMBER))
                objDestination.CardNumber = obj.CardNumber;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_EXPIRATIONDATE))
                objDestination.ExpirationDate = obj.ExpirationDate;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_CREATEDDATE))
                objDestination.CreatedDate = obj.CreatedDate;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_MODIFIEDDATE))
                objDestination.ModifiedDate = obj.ModifiedDate;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_ACTIVATIONDATE))
                objDestination.ActivationDate = obj.ActivationDate;

            if (!excludeProperties.Contains(domainReservations.ReservationPaymentDetail.PROP_NAME_HASHCODE))
                objDestination.HashCode = obj.HashCode;
        }

        #endregion ReservationPaymentDetail

        #region RESERVATIONPARTIALPAYMENTDETAILS

        public static domainReservations.ReservationPartialPaymentDetail Convert(contractsReservations.ReservationPartialPaymentDetail obj)
        {
            var newObj = new domainReservations.ReservationPartialPaymentDetail();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationPartialPaymentDetail obj, domainReservations.ReservationPartialPaymentDetail objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Reservation_UID = obj.Reservation_UID;
            objDestination.InstallmentNo = obj.InstallmentNo;
            objDestination.InterestRate = obj.InterestRate;
            objDestination.Amount = obj.Amount;
            objDestination.IsPaid = obj.IsPaid;
            objDestination.ModifiedDate = obj.ModifiedDate;
        }

        #endregion RESERVATIONPARTIALPAYMENTDETAILS

        #region ReservationRoomExtra

        public static domainReservations.ReservationRoomExtra Convert(contractsReservations.ReservationRoomExtra obj, bool includeReservationRoomExtrasSchedules = false)
        {
            var newObj = new domainReservations.ReservationRoomExtra();

            Map(obj, newObj, null, includeReservationRoomExtrasSchedules);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationRoomExtra obj, domainReservations.ReservationRoomExtra objDestination, IEnumerable<string> excludeProperties = null,
            bool includeReservationRoomExtrasSchedules = false)
        {
            if (excludeProperties == null)
                excludeProperties = Enumerable.Empty<string>();

            if (obj.UID > 0 && !excludeProperties.Contains(domainReservations.ReservationRoomExtra.PROP_NAME_UID))
                objDestination.UID = obj.UID;

            objDestination.Extra_UID = obj.Extra_UID;
            objDestination.ExtraIncluded = obj.ExtraIncluded;
            objDestination.Qty = obj.Qty;
            objDestination.Total_Price = obj.Total_Price;
            objDestination.Total_VAT = obj.Total_VAT;

            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;

            if (includeReservationRoomExtrasSchedules)
            {
                if (obj.ReservationRoomExtrasAvailableDates != null)
                    objDestination.ReservationRoomExtrasAvailableDates = new List<domainReservations.ReservationRoomExtrasAvailableDate>(obj.ReservationRoomExtrasAvailableDates.Select(x => Convert(x)));

                if (obj.ReservationRoomExtrasSchedules != null)
                    objDestination.ReservationRoomExtrasSchedules = new List<domainReservations.ReservationRoomExtrasSchedule>(obj.ReservationRoomExtrasSchedules.Select(x => Convert(x)));
            }
        }

        #endregion ReservationRoomExtra

        #region ReservationRoomExtrasSchedule
        public static domainReservations.ReservationRoomExtrasSchedule Convert(contractsReservations.ReservationRoomExtrasSchedule obj)
        {
            var newObj = new domainReservations.ReservationRoomExtrasSchedule();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationRoomExtrasSchedule obj, domainReservations.ReservationRoomExtrasSchedule objDestination, IEnumerable<string> excludeProperties = null)
        {
            if (excludeProperties == null)
                excludeProperties = Enumerable.Empty<string>();

            if (obj.UID > 0 && !excludeProperties.Contains(domainReservations.ReservationRoomExtrasSchedule.PROP_NAME_UID))
                objDestination.UID = obj.UID;

            objDestination.Date = obj.Date;

            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;


        }
        #endregion ReservationRoomExtrasSchedule

        #region RESERVATIONROOMEXTRASAVAILABLEDATE

        public static domainReservations.ReservationRoomExtrasAvailableDate Convert(contractsReservations.ReservationRoomExtrasAvailableDate obj)
        {
            var newObj = new domainReservations.ReservationRoomExtrasAvailableDate();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsReservations.ReservationRoomExtrasAvailableDate obj, domainReservations.ReservationRoomExtrasAvailableDate objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.ReservationRoomExtra_UID = obj.ReservationRoomExtra_UID;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
        }

        #endregion

        #region LostReservation
        public static domainReservations.LostReservation Convert(contractsReservations.LostReservation obj)
        {
            var newObj = new domainReservations.LostReservation();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(contractsReservations.LostReservation obj, domainReservations.LostReservation objDestination)
        {
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.GuestEmail = obj.GuestEmail;
            objDestination.GuestName = obj.GuestName;
            objDestination.NumberOfRooms = obj.NumberOfRooms;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.ReservationTotal = obj.ReservationTotal;

            if (obj.TotalReservation != decimal.Zero)
            {
                objDestination.TotalReservation = obj.TotalReservation;
            }
            else
            {
                decimal parsedDecimal;

                if (decimal.TryParse(obj.ReservationTotal, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.InvariantCulture, out parsedDecimal))
                {
                    objDestination.TotalReservation = parsedDecimal;
                }
            }

            objDestination.CreatedDate = obj.CreatedDate;
        }
        #endregion

        #region LostReservationDetailMapping
        public static domainReservations.LostReservationDetail Convert(contractsReservations.LostReservationDetail obj)
        {
            var newObj = new domainReservations.LostReservationDetail();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(contractsReservations.LostReservationDetail obj, domainReservations.LostReservationDetail objDestination)
        {
            objDestination.Total = obj.Total;
            objDestination.RoomsTotalAmountOnlyRates = obj.RoomsTotalAmountOnlyRates;
            objDestination.CurrencyUID = obj.CurrencyUID;
            objDestination.CurrencyName = obj.CurrencyName;
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.PropertyUID = obj.PropertyUID;
            objDestination.Comments = obj.Comments;
            objDestination.Request1 = obj.Request1;
            objDestination.Request2 = obj.Request2;
            objDestination.Request3 = obj.Request3;
            objDestination.Request4 = obj.Request4;
            objDestination.Rooms = obj.Rooms.Select(x => BusinessObjectToDomainTypeConverter.Convert(x)).ToList();
            objDestination.Guest = BusinessObjectToDomainTypeConverter.Convert(obj.Guest);
            objDestination.IPAddress = obj.IPAddress;
            objDestination.CreatedDate = obj.CreatedDate;
        }
        #endregion

        #region LostReservationGuest
        public static domainReservations.LostReservationGuest Convert(contractsReservations.LostReservationGuest obj)
        {
            var newObj = new domainReservations.LostReservationGuest();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(contractsReservations.LostReservationGuest obj, domainReservations.LostReservationGuest objDestination)
        {
            objDestination.FirstName = obj.FirstName;
            objDestination.LastName = obj.LastName;
            objDestination.Address = obj.Address;
            objDestination.Email = obj.Email;
            objDestination.PhoneNumber = obj.PhoneNumber;
            objDestination.PostalCode = obj.PostalCode;
            objDestination.VATNumber = obj.VATNumber;
            objDestination.CityName = obj.CityName;
            objDestination.CityUID = obj.CityUID;
            objDestination.StateName = obj.StateName;
            objDestination.StateUID = obj.StateUID;
            objDestination.CountryName = obj.CountryName;
            objDestination.CountryUID = obj.CountryUID;
            objDestination.Birthday = obj.Birthday;
            objDestination.IpAddress = obj.IpAddress;
        }
        #endregion

        #region LostReservationDetailMapping
        public static domainReservations.LostReservationRoom Convert(contractsReservations.LostReservationRoom obj)
        {
            var newObj = new domainReservations.LostReservationRoom();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(contractsReservations.LostReservationRoom obj, domainReservations.LostReservationRoom objDestination)
        {
            objDestination.Adults = obj.Adults;
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.Childs = obj.Childs;
            objDestination.ChildsAges = obj.ChildsAges;
            objDestination.CurrencyUID = obj.CurrencyUID;
            objDestination.CurrencyName = obj.CurrencyName;
            objDestination.GuestName = obj.GuestName;
            objDestination.Name = obj.Name;
            objDestination.RateUID = obj.RateUID;
            objDestination.RateName = obj.RateName;
            objDestination.Nights = obj.Nights;
            objDestination.RateTotal = obj.RateTotal;
            objDestination.TotalAmountOnlyRate = obj.TotalAmountOnlyRate;
            objDestination.UID = obj.UID;
            objDestination.Extras = obj.Extras.Select(x => BusinessObjectToDomainTypeConverter.Convert(x)).ToList();
            objDestination.Incentives = obj.Incentives.Select(x => BusinessObjectToDomainTypeConverter.Convert(x)).ToList();
        }
        #endregion

        #region LostReservationTableLine

        public static domainReservations.LostReservationTableLine Convert(contractsReservations.LostReservationTableLine obj)
        {
            var newObj = new domainReservations.LostReservationTableLine();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(contractsReservations.LostReservationTableLine obj, domainReservations.LostReservationTableLine objDestination)
        {
            objDestination.Name = obj.Name;
            objDestination.Value = obj.Value;
            objDestination.CurrencyName = obj.CurrencyName;
        }

        #endregion

        #region PaymentGatewayTransaction
        public static domainPayment.PaymentGatewayTransaction Convert(contractsPayments.PaymentGatewayTransaction obj, bool isUpdate = false)
        {
            var newObj = new domainPayment.PaymentGatewayTransaction();
            Map(obj, newObj, isUpdate);
            return newObj;
        }

        public static void Map(contractsPayments.PaymentGatewayTransaction obj, domainPayment.PaymentGatewayTransaction objDestination, bool isUpdate)
        {
            objDestination.UID = obj.UID;
            objDestination.XmlMessage = obj.XmlMessage;
            objDestination.TransactionType = obj.TransactionType;
            objDestination.TransactionStatus = obj.TransactionStatus;
            objDestination.TransactionRequest = obj.TransactionRequest;
            objDestination.TransactionMessage = obj.TransactionMessage;
            objDestination.TransactionID = obj.TransactionID;
            objDestination.TransactionErrorMessage = obj.TransactionErrorMessage;
            objDestination.TransactionEnviroment = obj.TransactionEnviroment;
            objDestination.TransactionCode = obj.TransactionCode;
            objDestination.ServerDate = obj.ServerDate;
            objDestination.RefundTransactionID = obj.RefundTransactionID;
            if (!isUpdate)
                objDestination.Property = obj.Property;
            objDestination.PaypalProfileID = obj.PaypalProfileID;
            objDestination.PaymentGatewayOrderID = obj.PaymentGatewayOrderID;
            objDestination.PaymentGatewayName = obj.PaymentGatewayName;
            objDestination.PaymentGatewayAutoGeneratedUID = obj.PaymentGatewayAutoGeneratedUID;
            objDestination.OrderType = obj.OrderType;
            objDestination.OrderCaptured = obj.OrderCaptured;
            objDestination.GuestInformation = obj.GuestInformation;
        }
        #endregion

        #region PaymentGatewayTransactionDetails
        public static domainPayment.PaymentGatewayTransactionsDetail Convert(contractsPayments.PaymentGatewayTransactionsDetail obj)
        {
            var newObj = new domainPayment.PaymentGatewayTransactionsDetail();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(contractsPayments.PaymentGatewayTransactionsDetail obj, domainPayment.PaymentGatewayTransactionsDetail objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.ResponseJson = obj.ResponseJson;
            objDestination.ResponseCode = obj.ResponseCode;
            objDestination.RequestType = obj.RequestType;
            objDestination.RequestJson = obj.RequestJson;
            objDestination.RefundTransactionID = obj.RefundTransactionID;
            objDestination.PaymentGatewayTransactionId = obj.PaymentGatewayTransactionId;
        }
        #endregion

        #region ReservationGuest
        public static domainReservations.Reservation Convert(contractsReservations.ReservationGuest obj)
        {
            var newObj = new domainReservations.Reservation();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(contractsReservations.ReservationGuest obj, domainReservations.Reservation objDestination)
        {
            objDestination.GuestFirstName = obj.FirstName;
            objDestination.GuestLastName = obj.LastName;
            objDestination.GuestEmail = obj.Email;
            objDestination.GuestPhone = obj.Phone;
            objDestination.GuestIDCardNumber = obj.IdCardNumber;
            objDestination.GuestCountry_UID = obj.CountryId;
            objDestination.GuestState_UID = obj.StateId;
            objDestination.GuestCity = obj.City;
            objDestination.GuestAddress1 = obj.Address1;
            objDestination.GuestAddress2 = obj.Address2;
            objDestination.GuestPostalCode = obj.PostalCode;
        }
        #endregion

        #region BillingInfo
        public static domainReservations.Reservation Convert(contractsReservations.BillingInfo obj)
        {
            var newObj = new domainReservations.Reservation();
            Map(obj, newObj);
            return newObj;
        }

        public static void Map(contractsReservations.BillingInfo obj, domainReservations.Reservation objDestination)
        {
            objDestination.BillingAddress1 = obj.Address1;
            objDestination.BillingAddress2 = obj.Address2;
            objDestination.BillingContactName = obj.ContactName;
            objDestination.BillingPostalCode = obj.PostalCode;
            objDestination.BillingCity = obj.City;
            objDestination.BillingCountry_UID = obj.CountryId;
            objDestination.BillingPhone = obj.Phone;
            objDestination.BillingState_UID = obj.StateId;
            objDestination.BillingEmail = obj.Email;
            objDestination.BillingTaxCardNumber = obj.TaxCardNumber;
        }
        #endregion
    }

}
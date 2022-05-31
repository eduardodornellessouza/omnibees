using OB.BL.Contracts.Data.Reservations;
using OB.Domain.Payments;
using OB.Events.Contracts.Messages;
using OB.Reservation.BL.Contracts.Data.BaseLogDetails;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using contractsPayments = OB.Reservation.BL.Contracts.Data.Payments;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using contractsVisualState = OB.Reservation.BL.Contracts.Data.VisualStates;
using domainReservations = OB.Domain.Reservations;

namespace OB.BL.Operations.Internal.TypeConverters
{
    public class DomainToBusinessObjectTypeConverter
    {
        #region Reservation

        public static contractsReservations.Reservation Convert(domainReservations.Reservation obj,
            ListReservationRequest request = null, List<OB.Reservation.BL.Contracts.Data.Payments.PaymentGatewayTransaction> paymentGatewayTransactions = null)
        {
            if (request == null)
                request = new ListReservationRequest();

            if (paymentGatewayTransactions == null)
                paymentGatewayTransactions = new List<OB.Reservation.BL.Contracts.Data.Payments.PaymentGatewayTransaction>();

            var newObj = new contractsReservations.Reservation();

            Map(obj, newObj, request, paymentGatewayTransactions);

            return newObj;
        }

        public static void Map(domainReservations.Reservation obj, contractsReservations.Reservation objDestination, ListReservationRequest request, List<OB.Reservation.BL.Contracts.Data.Payments.PaymentGatewayTransaction> paymentGatewayTransactions)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

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
            objDestination.InternalNotes = obj.InternalNotes;
            objDestination.IPAddress = obj.IPAddress;
            objDestination.TPI_UID = obj.TPI_UID;
            objDestination.PromotionalCode_UID = obj.PromotionalCode_UID;
            objDestination.ChannelProperties_RateModel_UID = obj.ChannelProperties_RateModel_UID;
            objDestination.ChannelProperties_Value = obj.ChannelProperties_Value;
            objDestination.ChannelProperties_IsPercentage = obj.ChannelProperties_IsPercentage;
            objDestination.InvoicesDetail_UID = obj.InvoicesDetail_UID;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.CreateBy = obj.CreateBy;
            objDestination.ModifyDate = obj.ModifyDate;
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
            objDestination.PmsRservationNumber = obj.PmsRservationNumber;
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
            objDestination.ExternalSource_UID = obj.ExternalSource_UID;
            objDestination.CampaignName = obj.CampaignName;

            if (request != null && request.IncludeGuests)
            {
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
            }
            objDestination.ReferralSourceId = obj.ReferralSourceId;
            objDestination.ReferralSourceURL = obj.ReferralSourceURL;

            objDestination.UseDifferentBillingInfo = obj.UseDifferentBillingInfo;
            objDestination.BillingTaxCardNumber = obj.BillingTaxCardNumber;
            objDestination.PaymentGatewayTransactionStatusCode = obj.PaymentGatewayTransactionStatusCode;
            objDestination.PaymentGatewayTransactionStatus = paymentGatewayTransactions.FirstOrDefault()?.TransactionStatus;
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
            objDestination.InternalNotesHistory = obj.InternalNotesHistory;
            objDestination.Company_UID = obj.Company_UID;
            objDestination.Employee_UID = obj.Employee_UID;
            objDestination.TPICompany_UID = obj.TPICompany_UID;
            objDestination.PropertyBaseCurrencyExchangeRate = obj.PropertyBaseCurrencyExchangeRate;

            if (request != null && (request.IncludeReservationRooms || request.IncludeReservationRoomExtras || request.IncludeExtras
                || request.IncludeReservationRoomDetails || request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeReservationRoomDetailsAppliedPromotionalCode || request.IncludeIncentives
                || request.IncludeReservationRoomChilds || request.IncludeReservationRoomTaxPolicies || request.IncludeReservationRoomExtrasSchedules || request.IncludeReservationRoomExtrasAvailableDates
                || request.IncludeReservationRoomIncentivePeriods))
                objDestination.ReservationRooms = new ObservableCollection<contractsReservations.ReservationRoom>(obj.ReservationRooms.Select(x => Convert(x, request)));

            if (request != null && request.IncludeReservationPartialPaymentDetails)
                objDestination.ReservationPartialPaymentDetails = new ObservableCollection<contractsReservations.ReservationPartialPaymentDetail>(obj.ReservationPartialPaymentDetails.Select(Convert));

            if (request != null && request.IncludeReservationPaymentDetail)
                objDestination.ReservationPaymentDetail = (obj.ReservationPaymentDetails.Any() ? Convert(obj.ReservationPaymentDetails.FirstOrDefault(), request.UnifyCardNumber) : null);

            if (request != null && !paymentGatewayTransactions.Count().Equals(0) && request.IncludePaymentGatewayTransactions)
            {
                objDestination.PaymentGatewayTransactionEnviroment = paymentGatewayTransactions.FirstOrDefault(x => x.PaymentGatewayAutoGeneratedUID.Equals(obj.Number))?.TransactionEnviroment;
                objDestination.PaymentGatewayTransactionType = paymentGatewayTransactions.FirstOrDefault(x => x.PaymentGatewayAutoGeneratedUID.Equals(obj.Number))?.TransactionType;
                if (string.IsNullOrEmpty(objDestination.PaymentGatewayName))
                {
                    objDestination.PaymentGatewayName = paymentGatewayTransactions.FirstOrDefault(x => x.PaymentGatewayAutoGeneratedUID.Equals(obj.Number))?.PaymentGatewayName;
                }
                if (string.IsNullOrEmpty(objDestination.PaymentGatewayTransactionStatusCode))
                {
                    objDestination.PaymentGatewayTransactionStatusCode = paymentGatewayTransactions.FirstOrDefault(x => x.PaymentGatewayAutoGeneratedUID.Equals(obj.Number))?.TransactionStatus;
                }
            }
        }

        #endregion Reservation

        #region ReservationLight

        public static contractsReservations.ReservationLight ConvertToLight(domainReservations.Reservation obj)
        {
            var newObj = new contractsReservations.ReservationLight();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.Reservation obj, contractsReservations.ReservationLight objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.Number = obj.Number;
            objDestination.Channel_UID = obj.Channel_UID;
            objDestination.GDSSource = obj.GDSSource;
            objDestination.Date = obj.Date;
            objDestination.TotalAmount = obj.TotalAmount;
            objDestination.Adults = obj.Adults;
            objDestination.Children = obj.Children;
            objDestination.Status = obj.Status;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifyDate = obj.ModifyDate;
            objDestination.Tax = obj.Tax;
            objDestination.ChannelAffiliateName = obj.ChannelAffiliateName;
            objDestination.ReservationCurrency_UID = obj.ReservationCurrency_UID;
            objDestination.ReservationBaseCurrency_UID = obj.ReservationBaseCurrency_UID;
            objDestination.ReservationCurrencyExchangeRate = obj.ReservationCurrencyExchangeRate;
            objDestination.ReservationCurrencyExchangeRateDate = obj.ReservationCurrencyExchangeRateDate;
            objDestination.TotalTax = obj.TotalTax;
            objDestination.NumberOfRooms = obj.NumberOfRooms;
            objDestination.RoomsTax = obj.RoomsTax;
            objDestination.RoomsExtras = obj.RoomsExtras;
            objDestination.RoomsPriceSum = obj.RoomsPriceSum;
            objDestination.RoomsTotalAmount = obj.RoomsTotalAmount;
            objDestination.GroupCode_UID = obj.GroupCode_UID;
            objDestination.PmsRservationNumber = obj.PmsRservationNumber;
            objDestination.IsOnRequest = obj.IsOnRequest;
            objDestination.GuestFirstName = obj.GuestFirstName;
            objDestination.GuestLastName = obj.GuestLastName;
            objDestination.PropertyBaseCurrencyExchangeRate = obj.PropertyBaseCurrencyExchangeRate;

            if (obj.ReservationRooms != null)
            {
                objDestination.ReservationRooms = new ObservableCollection<contractsReservations.ReservationRoomLight>(obj.ReservationRooms.Select(ConvertToLight));
            }
        }

        #endregion Reservation

        #region ReservationHistory

        public static contractsReservations.ReservationHistory Convert(domainReservations.ReservationsHistory obj)
        {
            var newObj = new contractsReservations.ReservationHistory();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationsHistory obj, contractsReservations.ReservationHistory objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.ReservationUID = obj.ReservationUID;
            objDestination.ReservationNumber = obj.ReservationNumber;
            objDestination.Channel = obj.Channel;
            objDestination.Status = obj.Status;
            objDestination.UserName = obj.UserName;
        }
        #endregion Reservation

        #region BaseGridLineDetail

        public static void Map(domainReservations.BaseGridLineDetail obj, BaseGridLineDetail objDestination)
        {
            objDestination.Action = obj.Action;
            objDestination.SubActions = obj.SubActions;
            objDestination.LogId = obj.LogId;
            objDestination.GridLineId = obj.GridLineId;
            objDestination.CreatedByUsername = obj.CreatedByUsername;
            objDestination.CreatedBy = obj.CreatedBy;
            objDestination.PropertyId = obj.PropertyId;
            objDestination.CreatedDate = obj.CreatedDate;
        }

        #endregion

        #region ReservationGridLineDetail

        public static ReservationGridLineDetail Convert(domainReservations.ReservationGridLineDetail obj)
        {
            var newObj = new ReservationGridLineDetail();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationGridLineDetail obj, ReservationGridLineDetail objDestination)
        {
            // Map BaseLogDetail Properties
            Map(obj, (BaseGridLineDetail)objDestination);

            objDestination.ReservationNumber = obj.ReservationNumber;
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.GuestName = obj.GuestName;
            objDestination.ChannelName = obj.ChannelName;
            objDestination.ReservationStatus = obj.ReservationStatus;
            objDestination.ReservationTotal = obj.ReservationTotal;
        }

        #endregion ReservationGridLineDetail

        #region RESERVATIONROOMEXTRASAVAILABLEDATE

        public static contractsReservations.ReservationRoomExtrasAvailableDate Convert(domainReservations.ReservationRoomExtrasAvailableDate obj)
        {
            var newObj = new contractsReservations.ReservationRoomExtrasAvailableDate();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomExtrasAvailableDate obj, contractsReservations.ReservationRoomExtrasAvailableDate objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.ReservationRoomExtra_UID = obj.ReservationRoomExtra_UID;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
        }

        #endregion

        #region LostReservation
        public static contractsReservations.LostReservation Convert(domainReservations.LostReservation obj)
        {
            var newObj = new contractsReservations.LostReservation();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.LostReservation obj, contractsReservations.LostReservation objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.CouchBaseId = obj.CouchBaseId;
            objDestination.GuestEmail = obj.GuestEmail;
            objDestination.GuestName = obj.GuestName;
            objDestination.NumberOfRooms = obj.NumberOfRooms;
            objDestination.ReservationTotal = obj.ReservationTotal;
            objDestination.TotalReservation = obj.TotalReservation.HasValue ? obj.TotalReservation.Value : decimal.Parse(obj.ReservationTotal);
            objDestination.Property_UID = obj.Property_UID;
            objDestination.CreatedDate = obj.CreatedDate.Value;
        }
        #endregion

        #region LostReservationDetail
        public static contractsReservations.LostReservationDetail Convert(domainReservations.LostReservationDetail obj)
        {
            var newObj = new contractsReservations.LostReservationDetail();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.LostReservationDetail obj, contractsReservations.LostReservationDetail objDestination)
        {
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.CurrencyUID = obj.CurrencyUID;
            objDestination.Total = obj.Total;
            objDestination.RoomsTotalAmountOnlyRates = obj.RoomsTotalAmountOnlyRates;
            objDestination.Guest = Convert(obj.Guest);
            objDestination.Rooms = obj.Rooms.Select(Convert).ToList();
            objDestination.Comments = obj.Comments;
            objDestination.Request1 = obj.Request1;
            objDestination.Request2 = obj.Request2;
            objDestination.Request3 = obj.Request3;
            objDestination.Request4 = obj.Request4;
            objDestination.CurrencyName = obj.CurrencyName;
            objDestination.PropertyUID = obj.PropertyUID;
            objDestination.IPAddress = obj.IPAddress;
            objDestination.CreatedDate = obj.CreatedDate;
        }
        #endregion

        #region LostReservationGuest
        public static contractsReservations.LostReservationGuest Convert(domainReservations.LostReservationGuest obj)
        {
            var newObj = new contractsReservations.LostReservationGuest();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.LostReservationGuest obj, contractsReservations.LostReservationGuest objDestination)
        {
            objDestination.Address = obj.Address;
            objDestination.CityName = obj.CityName;
            objDestination.CityUID = obj.CityUID;
            objDestination.CountryName = obj.CountryName;
            objDestination.CountryUID = obj.CountryUID;
            objDestination.Email = obj.Email;
            objDestination.FirstName = obj.FirstName;
            objDestination.LastName = obj.LastName;
            objDestination.PhoneNumber = obj.PhoneNumber;
            objDestination.PostalCode = obj.PostalCode;
            objDestination.StateName = obj.StateName;
            objDestination.StateUID = obj.StateUID;
            objDestination.VATNumber = obj.VATNumber;
            objDestination.Birthday = obj.Birthday;
            objDestination.IpAddress = obj.IpAddress;
        }
        #endregion

        #region LostReservationRoom
        public static contractsReservations.LostReservationRoom Convert(domainReservations.LostReservationRoom obj)
        {
            var newObj = new contractsReservations.LostReservationRoom();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.LostReservationRoom obj, contractsReservations.LostReservationRoom objDestination)
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
            objDestination.Nights = obj.Nights;
            objDestination.RateUID = obj.RateUID;
            objDestination.RateName = obj.RateName;
            objDestination.RateTotal = obj.RateTotal;
            objDestination.TotalAmountOnlyRate = obj.TotalAmountOnlyRate;
            objDestination.UID = obj.UID;
            objDestination.Extras = obj.Extras.Select(Convert).ToList();
            objDestination.Incentives = obj.Incentives.Select(Convert).ToList();
        }
        #endregion

        #region LostReservationTableLine
        public static contractsReservations.LostReservationTableLine Convert(domainReservations.LostReservationTableLine obj)
        {
            var newObj = new contractsReservations.LostReservationTableLine();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.LostReservationTableLine obj, contractsReservations.LostReservationTableLine objDestination)
        {
            objDestination.Name = obj.Name;
            objDestination.Value = obj.Value;
            objDestination.CurrencyName = obj.CurrencyName;
        }
        #endregion

        #region ReservationRoom
        public static contractsReservations.ReservationRoom Convert(domainReservations.ReservationRoom obj, ListReservationRequest request = null)
        {
            var newObj = new contractsReservations.ReservationRoom();

            Map(obj, newObj, request);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoom obj, contractsReservations.ReservationRoom objDestination, ListReservationRequest request)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.Reservation_UID = obj.Reservation_UID;
            objDestination.RoomType_UID = obj.RoomType_UID;
            objDestination.GuestName = obj.GuestName;
            objDestination.GuestEmail = obj.GuestEmail;
            objDestination.SmokingPreferences = obj.SmokingPreferences;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
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

            if (request != null && request.IncludeReservationRoomChilds)
                objDestination.ReservationRoomChilds = new ObservableCollection<contractsReservations.ReservationRoomChild>(obj.ReservationRoomChilds.Select(Convert));

            if (request != null && (request.IncludeReservationRoomDetails || request.IncludeCancelationCosts || request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeReservationRoomDetailsAppliedPromotionalCode || request.IncludeIncentives || request.IncludeReservationRoomIncentivePeriods))
                objDestination.ReservationRoomDetails = new ObservableCollection<contractsReservations.ReservationRoomDetail>(obj.ReservationRoomDetails.Select(x => Convert(x,
                    request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeIncentives || request.IncludeReservationRoomIncentivePeriods, request.IncludeReservationRoomDetailsAppliedPromotionalCode)));

            if (request != null && (request.IncludeReservationRoomExtras || request.IncludeReservationRoomExtrasSchedules || request.IncludeReservationRoomExtrasAvailableDates || request.IncludeExtras))
                objDestination.ReservationRoomExtras = new ObservableCollection<contractsReservations.ReservationRoomExtra>(obj.ReservationRoomExtras.Select(x => Convert(x, request.IncludeReservationRoomExtrasSchedules, request.IncludeReservationRoomExtrasAvailableDates)));

            if (request != null && (request.IncludeReservationRoomTaxPolicies || request.IncludeTaxPolicies))
                objDestination.ReservationRoomTaxPolicies = new ObservableCollection<contractsReservations.ReservationRoomTaxPolicy>(obj.ReservationRoomTaxPolicies.Select(Convert));

        }
        #endregion ReservationRoom

        #region ReservationRoomLight
        public static contractsReservations.ReservationRoomLight ConvertToLight(domainReservations.ReservationRoom obj)
        {
            var newObj = new contractsReservations.ReservationRoomLight();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoom obj, contractsReservations.ReservationRoomLight objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.Reservation_UID = obj.Reservation_UID;
            objDestination.RoomType_UID = obj.RoomType_UID;
            objDestination.GuestName = obj.GuestName;
            objDestination.DateFrom = obj.DateFrom;
            objDestination.DateTo = obj.DateTo;
            objDestination.AdultCount = obj.AdultCount;
            objDestination.ChildCount = obj.ChildCount;
            objDestination.TotalTax = obj.TotalTax;
            objDestination.Status = obj.Status;
            objDestination.RoomName = obj.RoomName;
            objDestination.Rate_UID = obj.Rate_UID;
            objDestination.ReservationRoomsPriceSum = obj.ReservationRoomsPriceSum;
            objDestination.ReservationRoomsExtrasSum = obj.ReservationRoomsExtrasSum;
            objDestination.ReservationRoomsTotalAmount = obj.ReservationRoomsTotalAmount;
            objDestination.PmsRservationNumber = obj.PmsRservationNumber;
            objDestination.CommissionType = obj.CommissionType;
            objDestination.CommissionValue = obj.CommissionValue;
        }
        #endregion ReservationRoomLight

        #region ReservationRoomDetail
        public static contractsReservations.ReservationRoomDetail Convert(domainReservations.ReservationRoomDetail obj, bool includeReservationRoomDetailAppliedIncentives = true, bool includeReservationRoomDetailAppliedPromotionalCode = true)
        {
            var newObj = new contractsReservations.ReservationRoomDetail();

            Map(obj, newObj, includeReservationRoomDetailAppliedIncentives, includeReservationRoomDetailAppliedPromotionalCode);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomDetail obj, contractsReservations.ReservationRoomDetail objDestination, bool includeReservationRoomDetailAppliedIncentives = false, bool includeReservationRoomDetailAppliedPromotionalCode = true)
        {
            if (obj.UID > 0)
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

            if (includeReservationRoomDetailAppliedIncentives && obj.ReservationRoomDetailsAppliedIncentives != null)
                objDestination.ReservationRoomDetailsAppliedIncentives = new ObservableCollection<contractsReservations.ReservationRoomDetailsAppliedIncentive>(obj.ReservationRoomDetailsAppliedIncentives.Select(Convert));

            if (includeReservationRoomDetailAppliedPromotionalCode && obj.ReservationRoomDetailsAppliedPromotionalCodes != null)
                objDestination.ReservationRoomDetailsAppliedPromotionalCode = obj.ReservationRoomDetailsAppliedPromotionalCodes.Select(Convert).FirstOrDefault();
        }

        #endregion ReservationRoomDetail

        #region ReservationRoomDetailsAppliedIncentive
        public static contractsReservations.ReservationRoomDetailsAppliedIncentive Convert(domainReservations.ReservationRoomDetailsAppliedIncentive obj)
        {
            var newObj = new contractsReservations.ReservationRoomDetailsAppliedIncentive();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomDetailsAppliedIncentive obj, contractsReservations.ReservationRoomDetailsAppliedIncentive objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

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

        #endregion ReservationRoomExtrasSchedule

        #region ReservationRoomDetailsAppliedPromotionalCode
        public static contractsReservations.ReservationRoomDetailsAppliedPromotionalCode Convert(domainReservations.ReservationRoomDetailsAppliedPromotionalCode obj)
        {
            var newObj = new contractsReservations.ReservationRoomDetailsAppliedPromotionalCode();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomDetailsAppliedPromotionalCode obj, contractsReservations.ReservationRoomDetailsAppliedPromotionalCode objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.ReservationRoomDetail_UID = obj.ReservationRoomDetail_UID;
            objDestination.PromotionalCode_UID = obj.PromotionalCode_UID;
            objDestination.Date = obj.Date;
            objDestination.DiscountValue = obj.DiscountValue;
            objDestination.DiscountPercentage = obj.DiscountPercentage;
        }
        #endregion PromotionalCodesByDay

        #region ReservationRoomExtra
        public static contractsReservations.ReservationRoomExtra Convert(domainReservations.ReservationRoomExtra obj, bool includeReservationRoomExtrasSchedules = false, bool includeReservationRoomExtrasAvailableDates = false)
        {
            var newObj = new contractsReservations.ReservationRoomExtra();

            Map(obj, newObj, includeReservationRoomExtrasSchedules, includeReservationRoomExtrasAvailableDates);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomExtra obj, contractsReservations.ReservationRoomExtra objDestination, bool includeReservationRoomExtrasSchedules = false, bool includeReservationRoomExtrasAvailableDates = false)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;


            objDestination.Extra_UID = obj.Extra_UID;
            objDestination.ExtraIncluded = obj.ExtraIncluded;
            objDestination.ReservationRoom_UID = obj.ReservationRoom_UID;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.Qty = obj.Qty;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.Total_Price = obj.Total_Price;
            objDestination.Total_VAT = obj.Total_VAT;

            if (includeReservationRoomExtrasSchedules)
                objDestination.ReservationRoomExtrasSchedules = obj.ReservationRoomExtrasSchedules != null && obj.ReservationRoomExtrasSchedules.Any() ? obj.ReservationRoomExtrasSchedules.Select(Convert).ToList() : new List<contractsReservations.ReservationRoomExtrasSchedule>();

            if (includeReservationRoomExtrasAvailableDates)
                objDestination.ReservationRoomExtrasAvailableDates = obj.ReservationRoomExtrasAvailableDates?.Select(Convert).ToList() ?? new List<contractsReservations.ReservationRoomExtrasAvailableDate>();
        }

        #endregion ReservationRoomExtra

        #region ReservationRoomExtrasSchedule
        public static contractsReservations.ReservationRoomExtrasSchedule Convert(domainReservations.ReservationRoomExtrasSchedule obj)
        {
            var newObj = new contractsReservations.ReservationRoomExtrasSchedule();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomExtrasSchedule obj, contractsReservations.ReservationRoomExtrasSchedule objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.Date = obj.Date;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.ReservationRoomExtra_UID = obj.ReservationRoomExtra_UID;
        }

        #endregion ReservationRoomExtrasSchedule

        #region ReservationRoomChild
        public static contractsReservations.ReservationRoomChild Convert(domainReservations.ReservationRoomChild obj)
        {
            var newObj = new contractsReservations.ReservationRoomChild();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomChild obj, contractsReservations.ReservationRoomChild objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;


            objDestination.Age = obj.Age;
            objDestination.ChildNo = obj.ChildNo;
            objDestination.ReservationRoom_UID = obj.ReservationRoom_UID;

        }

        #endregion ReservationRoomExtra

        #region ReservationPaymentDetail
        public static contractsReservations.ReservationPaymentDetail Convert(domainReservations.ReservationPaymentDetail obj, bool unifyCardNumber = false)
        {
            var newObj = new contractsReservations.ReservationPaymentDetail();

            Map(obj, newObj, unifyCardNumber);

            return newObj;
        }

        /// <summary>
        /// Maps the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="objDestination">The object destination.</param>
        /// <param name="unifyCardNumber">if set to <c>true</c> and token != null then changes the CardNumber to Token.</param>
        public static void Map(domainReservations.ReservationPaymentDetail obj, contractsReservations.ReservationPaymentDetail objDestination, bool unifyCardNumber = false)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;


            objDestination.UID = obj.UID;
            objDestination.PaymentMethod_UID = obj.PaymentMethod_UID;
            objDestination.Reservation_UID = obj.Reservation_UID;
            objDestination.Amount = obj.Amount;
            objDestination.Currency_UID = obj.Currency_UID;
            objDestination.CVV = obj.CVV;
            objDestination.CardName = obj.CardName;
            objDestination.CardNumber = obj.CardNumber;
            objDestination.ExpirationDate = obj.ExpirationDate;
            objDestination.ActivationDate = obj.ActivationDate;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.OBTokenizationIsActive = obj.OBTokenizationIsActive;
            objDestination.PaymentGatewayTokenizationIsActive = obj.PaymentGatewayTokenizationIsActive;
            objDestination.CreditCardToken = obj.CreditCardToken;
            objDestination.HashCode = obj.HashCode;
            objDestination.VCNReservationId = obj.VCNReservationId;
            objDestination.VCNToken = obj.VCNToken;

            if (unifyCardNumber && !string.IsNullOrWhiteSpace(obj.CreditCardToken))
                objDestination.CardNumber = obj.CreditCardToken;
        }
        #endregion ReservationPaymentDetail

        #region ReservationPartialPaymentDetail
        public static contractsReservations.ReservationPartialPaymentDetail Convert(domainReservations.ReservationPartialPaymentDetail obj)
        {
            var newObj = new contractsReservations.ReservationPartialPaymentDetail();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationPartialPaymentDetail obj, contractsReservations.ReservationPartialPaymentDetail objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;


            objDestination.UID = obj.UID;
            objDestination.Amount = obj.Amount;
            objDestination.Reservation_UID = obj.Reservation_UID;
            objDestination.InstallmentNo = obj.InstallmentNo;
            objDestination.InterestRate = obj.InterestRate;
            objDestination.IsPaid = obj.IsPaid;

            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.ModifiedDate = obj.ModifiedDate;


        }
        #endregion ReservationPartialPaymentDetail

        #region ReservationRoomTaxPolicy
        public static contractsReservations.ReservationRoomTaxPolicy Convert(domainReservations.ReservationRoomTaxPolicy obj)
        {
            var newObj = new contractsReservations.ReservationRoomTaxPolicy();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomTaxPolicy obj, contractsReservations.ReservationRoomTaxPolicy objDestination)
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

        #region NotificationBase
        public static Notification Convert(domainReservations.NotificationBase obj)
        {
            var newObj = new Notification();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.NotificationBase obj, Notification objDestination)
        {
            objDestination.Action = obj.Action;
            objDestination.AssociatedEntities = obj.AssociatedEntities;
            objDestination.SubActions = obj.SubActions;
            objDestination.Operation = obj.Operation;

            objDestination.CreatedBy = obj.CreatedBy;
            objDestination.CreatedByName = obj.CreatedByName;
            objDestination.CreatedDate = obj.CreatedDate;

            objDestination.PropertyUID = obj.PropertyUID;
            objDestination.Description = obj.Description;
            objDestination.EntityDeltas = obj.EntityDeltas;
        }
        #endregion

        #region ReservationReadStatus
        public static contractsVisualState.ReservationReadStatus Convert(domainReservations.VisualState obj)
        {
            var newObj = new contractsVisualState.ReservationReadStatus();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.VisualState obj, contractsVisualState.ReservationReadStatus objDestination)
        {
            contractsVisualState.ReservationReadStatus reservationReadStatus = null;
            try
            {
                reservationReadStatus = obj.JSONData.FromJSON<contractsVisualState.ReservationReadStatus>();
            }
            catch { }

            if (reservationReadStatus != null)
            {
                long.TryParse(obj.LookupKey_1, out var reservationId);

                objDestination.Read = reservationReadStatus.Read;
                objDestination.Date = reservationReadStatus.Date;
                objDestination.UserId = reservationReadStatus.UserId;
                objDestination.ReservationId = reservationId;
            }
        }

        #endregion

        #region PaymentGatewayTransaction
        public static contractsPayments.PaymentGatewayTransaction Convert(PaymentGatewayTransaction obj)
        {
            if (obj == null)
                return null;

            var newObj = new contractsPayments.PaymentGatewayTransaction();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(PaymentGatewayTransaction obj, contractsPayments.PaymentGatewayTransaction objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.PaymentGatewayName = obj.PaymentGatewayName;
            objDestination.Property = obj.Property;
            objDestination.PaymentGatewayOrderID = obj.PaymentGatewayOrderID;
            objDestination.PaymentGatewayAutoGeneratedUID = obj.PaymentGatewayAutoGeneratedUID;
            objDestination.OrderCaptured = obj.OrderCaptured;
            objDestination.OrderType = obj.OrderType;
            objDestination.TransactionID = obj.TransactionID;
            objDestination.TransactionCode = obj.TransactionCode;
            objDestination.TransactionStatus = obj.TransactionStatus;
            objDestination.TransactionMessage = obj.TransactionMessage;
            objDestination.TransactionErrorMessage = obj.TransactionErrorMessage;
            objDestination.XmlMessage = obj.XmlMessage;
            objDestination.TransactionType = obj.TransactionType;
            objDestination.TransactionEnviroment = obj.TransactionEnviroment;
            objDestination.ServerDate = obj.ServerDate;
            objDestination.GuestInformation = obj.GuestInformation;
            objDestination.RefundTransactionID = obj.RefundTransactionID;
            objDestination.PaypalProfileID = obj.PaypalProfileID;
            objDestination.TransactionRequest = obj.TransactionRequest;
        }

        #endregion

        #region CancelReservationReason

        public static contractsReservations.CancelReservationReason Convert(domainReservations.CancelReservationReason obj)
        {
            var newObj = new contractsReservations.CancelReservationReason();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.CancelReservationReason obj, contractsReservations.CancelReservationReason objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.Name;
        }

        #endregion

        #region ReservationFilter

        public static contractsReservations.ReservationFilter Convert(domainReservations.ReservationFilter obj, bool includeReservationRoomFilter = false)
        {
            var newObj = new contractsReservations.ReservationFilter();

            Map(obj, newObj, includeReservationRoomFilter);

            return newObj;
        }

        public static void Map(domainReservations.ReservationFilter obj, contractsReservations.ReservationFilter objDestination, bool includeReservationRoomFilter = false)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.UID = obj.UID;
            objDestination.CreatedDate = obj.CreatedDate;
            objDestination.PropertyUid = obj.PropertyUid;
            objDestination.PropertyName = obj.PropertyName;
            objDestination.Number = obj.Number;
            objDestination.IsOnRequest = obj.IsOnRequest;
            objDestination.IsReaded = obj.IsReaded;
            objDestination.ModifiedDate = obj.ModifiedDate;
            objDestination.GuestName = obj.GuestName;
            objDestination.NumberOfNights = obj.NumberOfNights;
            objDestination.NumberOfAdults = obj.NumberOfAdults;
            objDestination.NumberOfChildren = obj.NumberOfChildren;
            objDestination.NumberOfRooms = obj.NumberOfRooms;
            objDestination.TPI_UID = obj.TPI_UID;
            objDestination.PaymentTypeUid = obj.PaymentTypeUid;
            objDestination.Status = obj.Status;
            objDestination.TotalAmount = obj.TotalAmount;
            objDestination.ExternalTotalAmount = obj.ExternalTotalAmount;
            objDestination.ExternalCommissionValue = obj.ExternalCommissionValue;
            objDestination.ExternalIsPaid = obj.ExternalIsPaid;
            objDestination.ChannelUid = obj.ChannelUid;
            objDestination.ChannelName = obj.ChannelName;
            objDestination.TPI_Name = obj.TPI_Name;
            objDestination.IsPaid = obj.IsPaid;
            objDestination.ReservationDate = obj.ReservationDate;
            objDestination.ReservationBaseCurrencyExchangeRate = obj.ReservationBaseCurrencyExchangeRate;
            objDestination.ReservationBaseCurrencySymbol = obj.ReservationBaseCurrencySymbol;
            objDestination.PropertyBaseCurrencyExchangeRate = obj.PropertyBaseCurrencyExchangeRate;
            objDestination.PropertyBaseCurrencySymbol = obj.PropertyBaseCurrencySymbol;
            objDestination.RepresentativeCurrencyExchangeRate = obj.RepresentativeCurrencyExchangeRate;
            objDestination.RepresentativeCurrencySymbol = obj.RepresentativeCurrencySymbol;
            objDestination.PartnerUid = obj.PartnerUid;
            objDestination.PartnerReservationNumber = obj.PartnerReservationNumber;
            objDestination.CreatedBy = obj.CreatedBy;
            objDestination.ModifiedBy = obj.ModifiedBy;
            objDestination.Guest_UID = obj.Guest_UID;
            objDestination.ExternalChannelUid = obj.ExternalChannelUid;
            objDestination.ExternalTPIUid = obj.ExternalTPIUid;
            objDestination.ExternalName = obj.ExternalName;
            objDestination.IsMobile = obj.IsMobile;
            objDestination.LoyaltyCardNumber = obj.LoyaltyCardNumber;

            if (includeReservationRoomFilter)
                objDestination.ReservationRoomFilters = obj.ReservationRoomFilters.Select(Convert).ToList();
        }

        #endregion ReservationFilter

        #region ReservationRoomFilter

        public static contractsReservations.ReservationRoomFilter Convert(domainReservations.ReservationRoomFilter obj)
        {
            var newObj = new contractsReservations.ReservationRoomFilter();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.ReservationRoomFilter obj, contractsReservations.ReservationRoomFilter objDestination)
        {
            if (obj.UID > 0)
                objDestination.UID = obj.UID;

            objDestination.UID = obj.UID;
            objDestination.ReservationRoomNo = obj.ReservationRoomNo;
            objDestination.GuestName = obj.GuestName;
            objDestination.ReservationId = obj.ReservationId;
            objDestination.Status = obj.Status;
            objDestination.GuestName = obj.GuestName;
            objDestination.DepositNumberOfNight = obj.DepositNumberOfNight;
            objDestination.DepositCost = obj.DepositCost;
            objDestination.ApplyDepositPolicy = obj.ApplyDepositPolicy;
            objDestination.CheckIn = obj.CheckIn;
            objDestination.CheckOut = obj.CheckOut;
            objDestination.CancellationCost = obj.CancellationCost;

        }

        #endregion ReservationRoomFilter

        #region TokenizedCreditCardsReadsPerMonth
        public static contractsReservations.TokenizedCreditCardsReadsPerMonth Convert(domainReservations.TokenizedCreditCardsReadsPerMonth obj)
        {
            if (obj == null)
                return null;

            var newObj = new contractsReservations.TokenizedCreditCardsReadsPerMonth();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(domainReservations.TokenizedCreditCardsReadsPerMonth obj, contractsReservations.TokenizedCreditCardsReadsPerMonth objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Property_UID = obj.Property_UID;
            objDestination.YearNr = obj.YearNr;
            objDestination.MonthNr = obj.MonthNr;
            objDestination.NrOfCreditCardReads = obj.NrOfCreditCardReads;
            objDestination.LastModifiedDate = obj.LastModifiedDate;
        }
        #endregion

        #region ReservationStatus

        /// <summary>
        /// Converts ReservationStatus form Domain to Business Object.
        /// WARNING: Before set langUIDToTranslate make sure you have included ReservationStatusLanguages.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="langUIDToTranslate">Before set this property make sure you have included ChannelLanguages.</param>
        /// <returns></returns>
        public static Reservation.BL.Contracts.Reservations.ReservationStatus Convert(domainReservations.ReservationStatus obj, long? langUIDToTranslate = null)
        {
            var newObj = new Reservation.BL.Contracts.Reservations.ReservationStatus();

            Map(obj, newObj, langUIDToTranslate);

            return newObj;
        }
        public static void Map(domainReservations.ReservationStatus obj, Reservation.BL.Contracts.Reservations.ReservationStatus objDestination, long? langUIDToTranslate = null)
        {
            objDestination.Id = obj.UID;
            objDestination.Name = obj.Name;

            if (langUIDToTranslate > 0 && obj.ReservationStatusLanguages != null && obj.ReservationStatusLanguages.Any())
            {
                var translation = obj.ReservationStatusLanguages.FirstOrDefault(x => x.Language_UID == langUIDToTranslate.Value);
                if (translation != null && !string.IsNullOrEmpty(translation.Name))
                {
                    objDestination.Name = translation.Name;
                }
            }
        }
        #endregion
    }
}

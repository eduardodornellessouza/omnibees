using System;
using System.Collections.Generic;
using System.Data;
//using OB.Reservation.BL.Contracts.Data.Rates;
using OB.Reservation.BL.Contracts.Requests;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.DL.Common.QueryResultObjects;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using domainReservations = OB.Domain.Reservations;
using OBcontractsRates = OB.BL.Contracts.Data.Rates;
//using OB.Domain.General;
//using OB.Reservation.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Data.Reservations;
using rateContracts = OB.BL.Contracts.Data.Rates;
using reservationContracts = OB.Reservation.BL.Contracts.Data.Reservations;
using OB.DL.Common.Interfaces;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using contractsGeneral = OB.Reservation.BL.Contracts.Data.General;
using contractsRates = OB.BL.Contracts.Data.Rates;
using OB.DL.Common.Repositories.Interfaces;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using OB.Api.Core;
using OB.DL.Common.Repositories.Interfaces.Rest;
using contractsCRMOB = OB.BL.Contracts.Data.CRM;


//using OB.Reservation.BL.Contracts.Data.CRM;

namespace OB.BL.Operations.Interfaces
{
    /// <summary>
    /// BusinessPOCO interface that exposes Admin operations.
    /// </summary>
    public interface IReservationHelperPOCO : IBusinessPOCOBase
    {
        #region POLICIES

        #region CANCELLATION POLICIES

        /// <summary>
        /// Calculate cancelation cost for each room of the reservation
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        List<ReservationRoomCancelationCost> GetCancelationCosts(OB.Reservation.BL.Contracts.Data.Reservations.Reservation reservation, bool convertToRateCurrency = true, DateTime? cancellationDate = null);

        /// <summary>
        /// Get Most restriction Cancelation Policy
        /// </summary>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <param name="rateId"></param>
        /// <param name="currencyId"></param>
        /// <param name="languageId"></param>
        /// <param name="rrdList">list of rateroomdetail, final price is used for the calculation</param>
        /// <returns></returns>
        rateContracts.CancellationPolicy GetMostRestrictiveCancelationPolicy(DateTime checkIn, DateTime checkOut, long? rateId, long? currencyId, long? languageId,
            List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList, bool forceDefaultPolicy = false);

        /// <summary>
        /// Set cancellation Policies to reservation room
        /// </summary>
        /// <param name="room"></param>
        /// <param name="rrdList">FinalPrice property is used for policies calculation</param>
        /// <param name="reservationBaseCurrency"></param>
        /// <param name="reservationLanguageUsed"></param>
        /// <param name="propertyBaseCurrencyExchangeRate"></param>
        /// <param name="forceDefaultCancellationpolicy"></param>
        void SetCancellationPolicies(OB.Domain.Reservations.ReservationRoom room, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList, long? reservationBaseCurrency, long? reservationLanguageUsed,
            decimal? propertyBaseCurrencyExchangeRate = 1, bool forceDefaultCancellationpolicy = false);

        /// <summary>
        /// Check if cancellation policy is diferent from original reservation
        /// </summary>
        /// <param name="room"></param>
        /// <param name="currentCancelationPolicy">current cancellation policy</param>
        /// <returns>false if equals, true otherwise</returns>
        bool CheckIfCancelationPolicyChanged(OB.Domain.Reservations.ReservationRoom room, rateContracts.CancellationPolicy currentCancelationPolicy);

        #endregion

        #region DEPOSIT POLICIES

        /// <summary>
        /// Calculate deposit cost for each room of the reservation
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        List<Internal.BusinessObjects.ReservationRoomDepositCost> GetDepositCosts(contractsReservations.Reservation reservation,
            OB.Domain.Reservations.ReservationsAdditionalData reservationAdditionalData, ReservationsAdditionalData contractAdditionalDate = null, DateTime? depositDate = null);

        /// <summary>
        /// Get Most restriction Deposit Policy
        /// </summary>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <param name="rateId"></param>
        /// <param name="currencyId"></param>
        /// <param name="languageId"></param>
        /// <param name="rrdList">list of rateroomdetail, final price is used for the calculation</param>
        /// <returns></returns>
        rateContracts.DepositPolicy GetMostRestrictiveDepositPolicy(DateTime checkIn, DateTime checkOut, long? rateId, long? currencyId, long? languageId, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList);

        /// <summary>
        /// Set deposit Policies to reservation room
        /// </summary>
        /// <param name="room"></param>
        /// <param name="rrdList">FinalPrice property is used for policies calculation</param>
        /// <param name="reservationAdditionalData"></param>
        /// <param name="reservationId"></param>
        /// <param name="reservationBaseCurrency"></param>
        /// <param name="reservationLanguageUsed"></param>
        /// <param name="propertyBaseCurrencyExchangeRate"></param>
        void SetDepositPolicies(OB.Domain.Reservations.ReservationRoom room, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList, ref ReservationsAdditionalData reservationAdditionalData,
            long reservationId, long? reservationBaseCurrency, long? reservationLanguageUsed, decimal? propertyBaseCurrencyExchangeRate = 1);

        /// <summary>
        /// Check if deposit policy is diferent from original reservation
        /// </summary>
        /// <param name="reservationRoomId"></param>
        /// <param name="reservationAdditionalData"></param>
        /// <param name="currentDepositPolicy">current deposit policy</param>
        /// <returns>false if equals, true otherwise</returns>
        bool CheckIfDepositPolicyChanged(long reservationRoomId, ReservationsAdditionalData reservationAdditionalData, rateContracts.DepositPolicy currentDepositPolicy);

        /// <summary>
        /// Compare 2 deposit policies excluding the following fields
        /// "Name", "Description", "Property_UID", "ModifiedDate", "IsDeleted", "UID",
        /// "TranslateName", "TranslatedDescription", "Percentage", "CreatedDate"
        /// </summary>
        /// <param name="currentPolicy"></param>
        /// <param name="newPolicy"></param>
        /// <returns></returns>
        bool CompareDepositPolicies(rateContracts.DepositPolicy currentPolicy, rateContracts.DepositPolicy newPolicy);

        #endregion

        #region OTHER POLICIES

        /// <summary>
        /// Set Other Policy in reservation room
        /// </summary>
        /// <param name="room"></param>
        /// <param name="rateId"></param>
        /// <param name="baseLanguage"></param>
        void SetOtherPolicy(OB.Domain.Reservations.ReservationRoom room, long? rateId, long? baseLanguage);

        contractsRates.OtherPolicy GetOtherPolicyByRateId(long? rateId, long? baseLanguage);

        #endregion

        List<contractsRates.TaxPolicy> GetTaxPoliciesByRateIds(List<long> rateIds, long? currencyId, long? languageId);

        #endregion

        OB.Domain.Reservations.ReservationsAdditionalData GetReservationAdditionalData(long reservationId);
        OB.Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData GetReservationAdditionalDataJsonObject(ref domainReservations.ReservationsAdditionalData reservationAdditionalData, long reservationUID);
        int GetReservationTransactionState(string transactionId, long channelId, out long reservationId, out long hangfireId);
        void SetIncludedExtras(OB.Domain.Reservations.ReservationRoom room, long languageUid);

        void SetReservationRoomDetails(domainReservations.ReservationRoom room,List<OBcontractsRates.RateRoomDetailReservation> rrds, out decimal total);
        void SetReservationRoomTaxPolicies(OB.Domain.Reservations.ReservationRoom room, List<contractsRates.TaxPolicy> taxPolicies, decimal exchangeRate, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList);
        void UpdateOperatorCreditUsed(long propertyId, long? channelId, long? paymentMethodTypeId, bool isOnRequest, decimal amount, out bool sendCreditLimitExcededEmail, out string channelName, out decimal creditLimit);
        void UpdateReservationStatus(long reservationId, Reservation.BL.Constants.ReservationStatus reservationStatus, string transactionId,
            Reservation.BL.Constants.ReservationTransactionStatus transactionStatus, long? userId, bool updateReservationHistory = true, string paymentGatewayOrderId = null);
        void UpdateTPICreditUsed(long propertyId, long? tpiId, long? paymentMethodTypeId, decimal amount, out bool sendCreditLimitExcededEmail);
        void DeleteReservationRooms(ICollection<OB.Domain.Reservations.ReservationRoom> rooms);
        void DeleteReservationRoomFilter(long reservationUID);

        /// <summary>
        /// Get Current Exchange Rate Between two currency from property
        /// </summary>
        /// <param name="baseCurrencyId"></param>
        /// <param name="currencyId"></param>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        decimal GetExchangeRateBetweenCurrenciesByPropertyId(long baseCurrencyId, long currencyId, long propertyId);

        /// <summary>
        /// Get exchange rate between two currencies
        /// </summary>
        /// <param name="baseCurrencyUid"></param>
        /// <param name="currencyUid"></param>
        /// <returns></returns>
        decimal GetExchangeRateBetweenCurrencies(long baseCurrencyUid, long currencyUid);

        /// <summary>
        /// Set Job To Ignore Reservation after an amount of time if it status remain pending
        /// </summary>
        /// <param name="request"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        long SetJobToIgnorePendingReservation(ReservationBaseRequest request, long jobId);

        /// <summary>
        /// delete Job To Ignore Reservation after an amount of time if it status remain pending
        /// </summary>
        /// <param name="jobId"></param>
        void DeleteHangfireJob(long jobId);

        bool UpdateReservationTransactionRetries(string transactionId, long channelId, out int retries);

        /// <summary>
        /// Update Reservation Transaction Status
        /// </summary>
        /// <returns></returns>
        void UpdateReservationTransactionStatus(string transactionId, long channelId, OB.Reservation.BL.Constants.ReservationTransactionStatus transactionStatus);

        /// <summary>
        /// Set Job To Unlock Reservation Transaction after an amount of time (Transaction was lock because excess of retries)
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="channelId"></param>
        /// <param name="transactionStatus"></param>
        void SetJobToUnlockReservationTransaction(string transactionId, long channelId, OB.Reservation.BL.Constants.ReservationTransactionStatus transactionStatus);//, long oldJobToDelete)


        /// <summary>
        /// Set Job To Cancel Reservation onrequest after an amount of time if it status remain pending
        /// </summary>
        /// <param name="request"></param>
        /// <param name="secondsToDelayJob"></param>
        /// <returns></returns>
        long SetJobToCancelReservationOnRequestWithDelay(IUnitOfWork unitOfWork, ReservationBaseRequest request, double secondsToDelayJob);

        void InsertReservationTransaction(string transactionId, long reservationId, string reservationNumber, long reservationStatus,
                                Constants.ReservationTransactionStatus transactionStatus, long channelId, long hangfireId, int retries);

        /// <summary>
        /// Get all guest reservationroom on a period of time
        /// </summary>
        /// <param name="guest_UID"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        List<GuestLoyaltyReservationQR1> GetGuestsReservationRoomsWithLoyaltyDiscount(long guest_UID, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Convert all reservation values to property currency
        /// </summary>
        /// <param name="newReservation"></param>
        /// <param name="existingReservation"></param>
        /// <param name="context"></param>
        /// <param name="reservationAdditionalData"></param>
        /// <returns></returns>
        domainReservations.Reservation ApplyCurrencyExchangeToReservationForConnectors(domainReservations.Reservation newReservation, domainReservations.Reservation existingReservation,
            ReservationDataContext context, ReservationsAdditionalData reservationAdditionalData = null, bool onlyApplyExchangeToAdditionalData = false,
            List<string> roomNoAdditionalDataToUpdate = null);

        void TreatPullTpiReservation(TreatPullTpiReservationParameters parameters);

        IEnumerable<SellRule> GetRulesFromPortal(long channelUID, long propertyUID,
            long? externalTpiId, long? externalChannelId, long? tpiId, long? currencyId, Guid? requestGuid = null);

        void MapSellingPrices(contractsReservations.Reservation reservation, bool includeReservationRooms, bool includeReservationRoomDetails);

        void TreatReservationFilter(TreatReservationFiltersParameters parameters, IUnitOfWork unitOfWork);
        void ModifyReservationFilter(contractsReservations.Reservation newReservation, contractsGeneral.ServiceName serviceName, contractsCRMOB.Guest guest = null);

        ReservationLookups GetReservationLookups(DL.Common.ListReservationCriteria request, IEnumerable<domainReservations.Reservation> result);

        bool ConvertAllValuesToRateCurrency(contractsReservations.Reservation reservationWithoutConvertions);

        void SetLanguageToReservation(contractsReservations.Reservation reservation, long property_UID, IOBPropertyRepository propsRepo, Guid? requestGuid = null);

        void SetLanguageToReservation(Domain.Reservations.Reservation reservation, long property_UID, IOBPropertyRepository propsRepo);

        #region PROMOTIONAL CODES

        ValidPromocodeParameters ValidatePromocodeForReservation(ValidatePromocodeForReservationParameters parameters);

        void TreatBeReservation(TreatBEReservationParameters parameters);

        void UpdatePromoCodeReservationsCompleted(long promotionalCodeUID, IEnumerable<DateTime> oldDays = null, IEnumerable<DateTime> newDays = null);

        #endregion PROMOTIONAL CODES

        void ValidatePricePerDay(List<OBcontractsRates.RateRoomDetailReservation> calculatedRRD, List<contractsReservations.ReservationRoomDetail> originalRRD,
            decimal failMargin, ReservationBaseRequest reservationRequest);

        void ValidateReservationRoomsPrices(contractsReservations.ReservationRoom calculatedRR, contractsReservations.ReservationRoom originalRR,
            decimal failMargin, ReservationBaseRequest reservationRequest);

        void ValidateReservationPrices(contractsReservations.Reservation calculatedRes, contractsReservations.Reservation originalRes,
            decimal failMargin, ReservationBaseRequest reservationRequest);

        decimal ValidateReservationRoomExtras(List<contractsReservations.ReservationRoomExtra> roomExtras, List<OBcontractsRates.Extra> extras, decimal propertyBaseCurrencyExchangeRate,
            int roomNightsCount, int roomAdultsCount, decimal failMargin, ReservationBaseRequest reservationRequest);

        void SetReservationRoomTaxPolicies(contractsReservations.ReservationRoom room,
            List<OBcontractsRates.TaxPolicy> taxPolicies, decimal exchangeRate, List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList);
    }
}

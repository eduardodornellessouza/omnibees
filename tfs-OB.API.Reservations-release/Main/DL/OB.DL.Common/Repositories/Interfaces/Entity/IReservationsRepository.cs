using OB.BL;
using OB.DL.Common.Criteria;
using OB.DL.Common.QueryResultObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using domain = OB.Domain.Reservations;
using domainReservation = OB.Domain.Reservations;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface IReservationsRepository : IRepository<domain.Reservation>
    {
        DbContext dbContext { get; set; }

        string GenerateReservationNumber(long propertyId);

        /// <summary>
        /// Call to the GetReservation detail SP
        /// </summary>
        /// <param name="reservationUID"></param>
        /// <param name="languageUID"></param>
        /// <param name="languageIso"></param>
        /// <returns></returns>
        ReservationDetailSearchQR1 GetReservationDetail(long reservationUID, long languageUID, string languageIso);

        ReservationDataContext GetReservationContext(string existingReservationNumber, long channelUID, long tpiUID, long companyUID, long propertyUID,
            long reservationUID, long currencyUID, long paymentMethodTypeUID, IEnumerable<long> rateUIDs, string guestFirstName,
            string guestLastName, string guestEmail, string guestUsername, long? languageId, Guid? requestGuid = null);

        domainReservation.Reservation FindByUIDEagerly(long uid);

        domainReservation.Reservation FindByUIDEagerly(long uid, long propertyUID);

        domainReservation.Reservation FindByReservationNumberAndChannelUID(string reservationNumber, long channelUID);

        domainReservation.Reservation FindByReservationNumberAndChannelUIDAndPropertyUID(string reservationNumber, long channelUID, long propertyUID);

        IEnumerable<long> FindReservationUIDSByRateRoomsAndDateOfModifOrStay(List<long> propertyUIDs, List<long> rateUIDs, DateTime? dateFrom, DateTime? dateTo, bool isDateFindModifications = false, bool isDateFindArrivals = false, bool isDateFindStays = false);

        IEnumerable<domainReservation.Reservation> FindReservationsLightByCriteria(out int totalRecords, List<long> UIDs, List<long> channelUIDs, List<long> propertyUIDs, DateTime? dateFrom, DateTime? dateTo, bool isDateFindModifications = false, bool isDateFindArrivals = false, bool isDateFindStays = false, bool includeReservationRooms = false, int pageIndex = -1, int pageSize = -1, bool returnTotal = false);

        IEnumerable<domainReservation.Reservation> FindByReservationUIDs(IEnumerable<long> ids,
         bool includeReservationRooms = false,
         bool includeReservationRoomChilds = false,
         bool includeReservationRoomDetails = false,
         bool includeReservationRoomDetailsIncentives = false,
         bool includeReservationRoomExtras = false,
         bool includeReservationPaymentDetails = false,
         bool includeReservationPartialPaymentDetails = false,
         bool includeReservationRoomTaxPolicies = false,
         int pageIndex = -1, int pageSize = -1);

        IEnumerable<domainReservation.Reservation> FindByCriteria(out int totalRecords, ListReservationCriteria request, int pageIndex = -1, int pageSize = -1, bool returnTotal = false);

        IEnumerable<domainReservation.Reservation> FindByCriteria(ListReservationCriteria request);

        /// <summary>
        /// Find reservations by criteria and by checkout
        /// </summary>
        /// <param name="totalRecords"></param>
        /// <param name="reservationUIDs"></param>
        /// <param name="propertyUIDs"></param>
        /// <param name="channelUIDs"></param>
        /// <param name="reservationNumbers"></param>
        /// <param name="reservationStatusCodes"></param>
        /// <param name="checkOutFrom"></param>
        /// <param name="checkOutTo"></param>
        /// <param name="includeReservationRooms"></param>
        /// <param name="includeReservationRoomChilds"></param>
        /// <param name="includeReservationRoomDetails"></param>
        /// <param name="includeReservationRoomDetailsIncentives"></param>
        /// <param name="includeReservationRoomExtras"></param>
        /// <param name="includeReservationPaymentDetails"></param>
        /// <param name="includeReservationPartialPaymentDetails"></param>
        /// <param name="includeReservationRoomTaxPolicies"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="returnTotal"></param>
        /// <returns></returns>
        IEnumerable<domainReservation.Reservation> FindByCheckOut(out int totalRecords, List<long> reservationUIDs, List<long> propertyUIDs, List<long> channelUIDs, List<string> reservationNumbers,
            List<long> reservationStatusCodes,
            DateTime? checkOutFrom, DateTime? checkOutTo,
            bool includeReservationRooms = false,
            bool includeReservationRoomChilds = false,
            bool includeReservationRoomDetails = false,
            bool includeReservationRoomDetailsIncentives = false,
            bool includeReservationRoomExtras = false,
            bool includeReservationPaymentDetails = false,
            bool includeReservationPartialPaymentDetails = false,
            bool includeReservationRoomTaxPolicies = false,
            int pageIndex = -1, int pageSize = -1,
            bool returnTotal = false);

        void SetSequenceReservationNumberRange(int interval);

        domainReservation.Reservation FindByNumberEagerly(string number);

        domainReservation.Reservation FindByNumberEagerly(string number, long propertyUID);

        IEnumerable<PropertyWithReservationsForChannelOrTPIQR1> FindPropertiesWithReservationsForChannelOrTPI(long channelUID, long? tpiUID, List<long> propertyUIDs);

        /// <summary>
        /// Get Current Reservation Transaction State
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="channelId"></param>
        /// <param name="reservationId"></param>
        /// <param name="hangfireId"></param>
        /// <returns></returns>
        int GetReservationTransactionState(string transactionId, long channelId, out long reservationId, out long hangfireId);

        /// <summary>
        /// Update reservation/reservationrooms to new status
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="reservationStatus"></param>
        /// <returns></returns>
        void UpdateReservationStatus(long reservationId, int reservationStatus, string transactionId, int transactionState, string reservationStatusName, long? userId, bool updateReservationHistory, string paymentGatewayOrderId);

        /// <summary>
        ///  Get Current Basic Reservation Info by TransactionID and PropertyUID
        /// </summary>
        /// <param name="propertyUID"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        ReservationBasicInfoForTransactionIdQR1 GetReservationBasicInfoForPaymentGatewayForTransaction(long propertyUID, string transactionId);

        /// <summary>
        ///  Get Current Basic ReservationTransactionStatus Info by ReservationUID
        /// </summary>
        /// <param name="reservationUID"></param>
        /// <returns></returns>
        ReservationTransactionStatusBasicInfoForReservationUidQR1 GetReservationTransactionStatusBasicInfoForReservationUID(long reservationUID);

        //Used in PO Representatives
        IEnumerable<PropertyWithReservationsForChannelOrTPIQR1> FindPropertiesWithReservationsForChannelsTpis(List<long> propertyUIDs);

        #region Aux

        bool FindAnyRoomTypesBy_ReservationUID_And_SystemEventCode(long reservationUID, string systemEventCode);

        IEnumerable<PEOccupancyAlertQR1> FindByRoomTypeUID_And_Code_And_PropertyUID_And_RoomTypeDate(long roomTypeUID,
                    long propertyUID, List<DateTime> roomTypeDates, string code);

        long GetBookingEngineChannelUID();


        OB.BL.Contracts.Responses.ListReservationsExternalSourceResponse ListReservationsExternalSource(OB.BL.Contracts.Requests.ListReservationsExternalSourceRequest request);

        List<domainReservation.ReservationsAdditionalData> FindReservationsAdditionalDataByReservationsUIDs(List<long> reservationIds);

        List<Item> FindReservationTransactionStatusByReservationsUIDs(List<long> reservationIds);

        int DeleteAllActivitiesForGuestUID(long guestUID);

        #endregion

        string GenerateReservationNumber(long propertyId, ref IDbTransaction scope);
        
        /// <summary>
        /// Inserts the reservation in transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="reservationNumber">The reservation number.</param>
        /// <param name="reservationStatus">The reservation status.</param>
        /// <param name="transactionStatus">The transaction status.</param>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="hangfireId">The hangfire identifier.</param>
        /// <param name="retries">The retries.</param>
        int InsertReservationTransaction(string transactionId, long reservationId, string reservationNumber, long reservationStatus, Constants.ReservationTransactionStatus transactionStatus, long channelId, long hangfireId, int retries);

        /// <summary>
        /// Get Current Exchange Rate Between two currency from property
        /// </summary>
        /// <param name="baseCurrencyId"></param>
        /// <param name="currencyId"></param>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        decimal GetExchangeRateBetweenCurrenciesByPropertyId(long baseCurrencyId, long currencyId, long propertyId);

        int UpdateRateRoomDetailAllotments(Dictionary<long, int> rateRoomDetailsVsAllotmentToAdd, bool validateAllotment, string correlationId = null);

        int InsertReservationAdditionalDataJson(long reservationId, string reservationAdditionalDataJson);
        int UpdateReservationAdditionalDataJson(long id, string reservationAdditionalDataJson);

        List<ReservationBEOverviewQR1> ListMyAccountReservationsOverview(long UserUID, int UserType, DateTime? DateFrom, DateTime? DateTo);

        int ValidateReservation(ValidateReservationCriteria criteria);

        int UpdateReservationTransactionStatus(string transactionId, long channelId, OB.Reservation.BL.Constants.ReservationTransactionStatus transactionStatus);

        /// <summary>
        /// Update or Insert if not exists the columns VCNReservationId and VCNToken.
        /// After the update the ModifiedDate of Reservations is updated too.
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="vcnReservationId"></param>
        /// <param name="vcnToken"></param>
        /// <returns>
        ///     <c>-1</c> if reservation not found. Nothing done.
        ///     <c>2</c> if updated with sucess. 
        ///     Otherwise something went wrong.
        /// </returns>
        int UpdateReservationVcn(UpdateReservationVcnCriteria criteria);

        long GetPropertyIdByReservationId(long reservationId);

        List<domainReservation.Reservation> FindReservationByNumber(string reservationNumber);
    }
}
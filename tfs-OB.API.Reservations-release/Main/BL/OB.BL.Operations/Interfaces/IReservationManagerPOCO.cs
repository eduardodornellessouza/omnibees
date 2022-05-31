using OB.BL.Operations.Internal.BusinessObjects;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OB.BL.Operations.Interfaces
{
    public interface IReservationManagerPOCO : IBusinessPOCOBase
    {

        #region RESTful Operations
        /// <summary>
        /// RESTful implementation of the InsertReservation operation.
        /// Inserts a Reservation into the Datastore given it's associated objects in the request.
        /// </summary>
        /// <param name="request">A InsertReservationRequest object that contains the required objects to create a Reservation.</param>
        /// <returns>An InsertReservationResponse that contains the UID of the created reservation and the status of the operation.</returns>
        InsertReservationResponse InsertReservation(InsertReservationRequest request);

        /// <summary>
        /// RESTful implementation of the UpdateReservation operation.
        /// Updates a Reservation in the Datastore given it's associated objects in the request.
        /// </summary>
        /// <param name="request">A UpdateReservationRequest object that contains the required objects to update a Reservation.</param>
        /// <returns>An UpdateReservationResponse that contains the result and status of the operation.</returns>
        UpdateReservationResponse UpdateReservation(UpdateReservationRequest request);

        /// <summary>
        /// RESTful implmentation of the CancelReservation operation.
        /// Cancels an existing Reservation Room in the Datastore given it's Reservation Number and Reservation Room number.
        /// </summary>
        /// <param name="request">A CancelReservationRequest object that contains the Reservation number and Reservation Room Number</param>
        /// <returns>A CancelReservationResponse object that contains the result and status of the operation.</returns>
        CancelReservationResponse CancelReservation(CancelReservationRequest request);

        /// <summary>
        /// RESTful implementation of the Search operation for Reservations given the Request criteria.
        /// </summary>
        /// <param name="request">A ListReservationRequest that contains the search criteria(PropertyUID, ChannelUID, Dates, Reservation numbers, etc)</param>
        /// <returns>A ListReservationResponse that contains the Reservation DTOs that match the given request criteria.</returns>
        ListReservationResponse ListReservations(ListReservationRequest request);

        /// <summary>
        /// RESTful implementation of the ListReservationUIDsByPropertyRateRoomsDateOfModifOrStay operation.
        /// This operation searchs for reservations given a specific request criteria, using property, rate/room, creation or modify date, checkin and checkout periods to search Reservations matching that criteria.
        /// </summary>
        /// <param name="request">A ListReservationUIDsByPropRateRoomAndDateOfModifOrStayRequest object containing the Search criteria</param>
        /// <returns>A ListReservationsUIDsResponse containing the UID's of matching Reservation objects</returns>
        ListReservationsUIDsResponse ListReservationUIDsByPropertyRateRoomsDateOfModifOrStay(ListReservationUIDsByPropRateDateOfModifOrStayRequest request);

        /// <summary>
        /// RESTful implementation of the ReservationsLight operation.
        /// This operation searchs for reservations given a specific request criteria, using uid(s), channel(s), property(s), creation or modify date, checkin and checkout periods to search Reservations matching that criteria.
        /// </summary>
        /// <param name="request">A ListReservationsLightRequest object containing the Search criteria</param>
        /// <returns>A ListReservationsLightResponse containing the UID's of matching ReservationLight objects</returns>
        ListReservationsLightResponse ListReservationsLight(ListReservationsLightRequest request);

        /// <summary>
        /// RESTful implementation of the ListReservationHistories operation.
        /// This operation searchs for reservations given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListReservationHistoryRequest object containing the Search criteria</param>
        /// <returns>A ListReservationHistoryResponse containing the List of matching ReservationHistory objects</returns>
        ListReservationHistoryResponse ListReservationHistories(ListReservationHistoryRequest request);

        /// <summary>
        /// RESTful implementation of the UpdatePMSReservationNumber operation.
        /// This operation updates the Reservation or ReservationRoom entities with the given PMSReservationNumbers in the request object.
        /// </summary>
        /// <param name="request">A UpdatePMSReservationNumberRequest object containing the mapping between Reservations/PMSNumbers and the ReservationRooms/PMSNumber</param>
        /// <returns>A UpdatePMSReservationNumberResponse containing the result status and/or list of found errors</returns>
        UpdatePMSReservationNumberResponse UpdatePMSReservationNumber(UpdatePMSReservationNumberRequest request);

        /// <summary>
        /// RESTful implementation of the SaveReservationExternalIdentifier operation.
        /// This operation updates the Reservation or ReservationRoom entities with the given PMSReservationNumbers in the request object.
        /// </summary>
        /// <param name="request">A SaveReservationExternalIdentifierRequest object containing the mapping between Reservations/PMSNumbers and the ReservationRooms/PMSNumber</param>
        /// <returns>A SaveReservationExternalIdentifierResponse containing the result status and/or list of found errors</returns>
        SaveReservationExternalIdentifierResponse SaveReservationExternalIdentifier(SaveReservationExternalIdentifierRequest request);

        /// <summary>
        /// RESTful implementation of the GetCancelationCosts operatio\n.
        /// This operation calculates cancelation costs of ReservationRooms.
        /// </summary>
        /// <param name="request">A GetCancelationCostsRequest object containing the Reservation with ReservationRooms and </param>
        /// <returns>A GetCancelationCostsResponse containing the result costs of each reservation room.</returns>
        GetCancelationCostsResponse GetCancelationCosts(GetCancelationCostsRequest request);

        GetDepositCostsResponse GetDepositCosts(GetDepositCostsRequest request);

        InsertInReservationInternalNotesResponse InsertInReservationInternalNotes(InsertInReservationInternalNotesRequest request);

        CalculateReservationRoomPricesResponse CalculateReservationRoomPrices(CalculateReservationRoomPricesRequest request);

        HaveGuestExceededLoyaltyDiscountResponse HaveGuestExceededLoyaltyDiscount(HaveGuestExceededLoyaltyDiscountRequest request);

        CalculateGuestPastReservationsValuesResponse CalculateGuestPastReservationsValues(CalculateGuestPastReservationsValuesRequest request);

        GetExchangeRatesBetweenCurrenciesResponse GetExchangeRatesBetweenCurrencies(GetExchangeRatesBetweenCurrenciesRequest request);

        ValidatePromocodeForReservationResponse ValidatePromocodeForReservation(ValidatePromocodeForReservationRequest request);

        #endregion RESTful Operations

        //TODO: I simplified this service to receive mostly the request object only. However, this service is for testing purposes only, so it should be removed
        //from the interface, but the testing methods still use groupRule and validate. Although groupRule can be mocked on the tests, validate can't. 
        //This implies changing the tests to use the InsertReservation service above or something else.
        ReservationResult InsertReservation(InsertReservationRequest request, GroupRule groupRule, bool validate = true);

        /// <summary>
        ///  Fetch Reservation Detail by ReservationId 
        /// </summary> 
        ReservationDetailQR1 GetFullReservationDetail(long ReservationId, long LanguageId, bool basecurrency);

        /// <summary>
        ///  Fetch Reservation Detail by ReservationId 
        /// </summary> 
        ReservationDetailQR1 GetReservationDetail(long ReservationId, long languageId);

        /// <summary>
        ///  Fetch Reservation Room Details
        /// </summary>
        /// <param name="ReservationUID"></param>
        List<ReservationRoomDetailQR1> GetReservationRoomDetail(long ReservationUID, long languageUID);

        /// <summary>
        ///  Fetch Reservation Room Details
        /// </summary>
        /// <param name="ReservationUID"></param>
        List<ReservationRoomQR1> GetReservationRoom(long ReservationUID, long languageUID, decimal PropertyBaseCurrencyExchangeRate);

        /// <summary>
        /// Fetch Reservation Room Extra
        /// </summary>
        /// <param name="ReservationRoomUID">The reservation room uid.</param>
        /// <param name="Language_UID">The language uid.</param>
        /// <returns></returns>
        List<ReservationRoomExtraQR1> GetReservationRoomExtraByRoomUID(long ReservationRoomUID, long Language_UID);

        /// <summary>
        /// Fetch Reservation Room Extra
        /// </summary>
        /// <param name="ReservationRoomUID">The reservation room uid.</param>
        /// <param name="Language_UID">The language uid.</param>
        /// <returns></returns>
        List<ReservationRoomIncentiveQR1> GetReservationRoomIncentivesByRoomUID(long ReservationRoomUID, long Language_UID);

        /// <summary>
        /// Get reservation Numbers
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        ListReservationNumbersResponse ListReservationNumbers(ListReservationNumbersRequest request);

        /// <summary>
        /// RESTful implementation of the Get Properties with Reservations for the given Channel_UID and TPI_UID.
        /// Gets a list of Properties UID's and Names's
        /// </summary>
        /// <param name="request">ListPropertiesWithReservationsForChannelOrTPIRequest instance</param>
        /// <returns>A ListPropertiesWithReservationsForChannelOrTPIResponse object that contains a list of Properties UID's and Names's</returns>
        ListPropertiesWithReservationsForChannelOrTPIResponse ListPropertiesWithReservationsForChannelOrTPI(ListPropertiesWithReservationsForChannelOrTPIRequest request);

        /// <summary>
        /// RESTful implementation of the Get Properties with Reservations for the given multiple Channels.
        /// Gets a list of Properties UID's and Names's
        /// </summary>
        /// <param name="request">ListPropertiesWithReservationsForChannelsRequest instance</param>
        /// <returns>A ListPropertiesWithReservationsForChannelOrTPIResponse object that contains a list of Properties UID's and Names's</returns>
        ListPropertiesWithReservationsForChannelOrTPIResponse ListPropertiesWithReservationsForChannelsTpis(ListPropertiesWithReservationsForChannelsRequest request);


        /// <summary>
        /// Export Reservations to and Ftp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        PagedResponseBase ExportReservationsToFtp(ExportReservationsToFtpRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        MarkReservationsAsViewedResponse MarkReservationsAsViewed(MarkReservationsAsViewedRequest request, bool IsDisposed = true);

        /// <summary>
        /// Validates and distribute reservation requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        ReservationBaseResponse ReservationCoordinator(ReservationBaseRequest request, Reservation.BL.Constants.ReservationAction action);


        /// <summary>
        /// Update reservation transaction status
        /// </summary>
        /// <param name="request"></param>
        void UpdateReservationTransactionStatus(UpdateReservationTransactionStatusRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        ListLostReservationsResponse ListLostReservations(ListLostReservationsRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        InsertLostReservationResponse InsertLostReservation(InsertLostReservationRequest request);

        /// <summary>
        /// RESTful implementation of the PaymentGatewayInfoAndModifyReservationOnRequestForTransactionID.
        /// This operation returns Info about the Reservation and ReservationTransactionStatus and Commits or Cancels the Reservation
        /// </summary>
        /// <param name="request">PaymentGatewayCommitOrCancelReservationOnRequestForTransactionIDRequest</param>
        /// <returns>PaymentGatewayCommitOrCancelReservationOnRequestForTransactionIDResponse</returns>
        PaymentGatewayInfoForTransactionIDResponse PaymentGatewayInfoAndModifyReservationOnRequestForTransactionID(PaymentGatewayInfoAndModifyReservationOnRequestForTransactionIDRequest request);

        /// <summary>
        /// Gets all instances of ReservationReadStatus given a search Criteria (ListMarkReservationsAsViewedRequest)  returning a ListMarkReservationsAsViewedResponse instance with all ReservationReadStatus instances
        /// that match the search criteria or none if not found any in the database that matches the given criteria..
        /// </summary>
        /// <param name="request">ListMarkReservationsAsViewedRequest instance</param>
        /// <returns>A ListMarkReservationsAsViewedResponse object that contains a collection of ReservationReadStatus instances.</returns>
        ListMarkReservationsAsViewedResponse ListMarkReservationsAsViewed(ListMarkReservationsAsViewedRequest request);


        /// <summary>
        /// RESTful implementation of the ListPaymentGatewayTransactions operation.
        /// This operation searchs for PaymentGatewayTransactions given a specific request criteria to search PaymentGatewayTransactions matching that criteria.
        /// </summary>
        /// <param name="request">A ListPaymentGatewayTransactionsRequest object containing the Search criteria</param>
        /// <returns>A ListPaymentGatewayTransactionsResponse containing the UID's of matching PaymentGatewayTransaction objects</returns>
        ListPaymentGatewayTransactionsResponse ListPaymentGatewayTransactions(ListPaymentGatewayTransactionsRequest request);

        /// <summary>
        /// RESTful implementation of the UpdateReservationIsPaid operation.
        /// This operation updates IsPaid of Reservation entities.
        /// </summary>
        /// <param name="request">A UpdateReservationIsPaidRequest object containing the criteria to update ispaid of reservation</param>
        /// <returns>A UpdateReservationIsPaidResponse containing the result status and/or list of found errors</returns>
        UpdateReservationIsPaidResponse UpdateReservationIsPaid(UpdateReservationIsPaidRequest request);

        /// <summary>
        /// RESTful implementation of the UpdateReservationIsPaid operation.
        /// This operation updates IsPaid of Reservation entities.
        /// </summary>
        /// <param name="request">A UpdateReservationIsPaidRequest object containing the criteria to update ispaid of reservation</param>
        /// <returns>A UpdateReservationIsPaidResponse containing the result status and/or list of found errors</returns>
        UpdateReservationIsPaidBulkResponse UpdateReservationIsPaidBulk(UpdateReservationIsPaidBulkRequest request);

        /// <summary>
        /// RESTful implementation of the UpdateReservationCancelReason operation.
        /// This operation update reservation cancel reason.
        /// </summary>
        /// <param name="request">A UpdateReservationCancelReasonRequest object containing the criteria to update reservation cancel reason</param>
        /// <returns>A UpdateReservationCancelReasonResponse containing the result reservation Id</returns>
        UpdateReservationCancelReasonResponse UpdateReservationCancelReason(UpdateReservationCancelReasonRequest request);

        /// <summary>
        /// RESTful implementation of the ListCancelReservationReasons operation.
        /// This operation list all Cancel Reservation Reasons - Cache
        /// </summary>
        /// <param name="request">A ListCancelReservationReasonRequest object containing the criteria to list cancel reservation reasons</param>
        /// <returns>A ListCancelReservationReasonResponse containing the results cancelreservationreason</returns>
        ListCancelReservationReasonResponse ListCancelReservationReasons(ListCancelReservationReasonRequest request);

        /// <summary>
        /// RESTful implementation of the ListReservationsFilter operation.
        /// This operation searchs for reservations given a specific request criteria.
        /// </summary>
        /// <param name="request">A FindReservationRequest object containing the Search criteria</param>
        /// <returns>A ListReservationResponse containing the List of matching Reservation objects</returns>
        ListReservationsFilterResponse ListReservationsFilter(OB.Reservation.BL.Contracts.Requests.ListReservationsFilterRequest request);

        /// <summary>
        /// RESTful implementation of the ListMyAccountReservationsOverview operation.
        /// This operation searchs for some data of reservations given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListMyAccountReservationsOverviewRequest object containing the Search criteria</param>
        /// <returns>A ListMyAccountReservationsOverviewResponse containing the List of matching Reservation objects</returns>
        ListMyAccountReservationsOverviewResponse ListMyAccountReservationsOverview(ListMyAccountReservationsOverviewRequest request);

        ApproveOrRefuseOnRequestReservationResponse ApproveOrRefuseOnRequestReservation(ApproveOrRefuseOnRequestReservationRequest request);

        ListReservationStatusesResponse ListReservationStatuses(ListReservationStatusesRequest request);

        Task<ValidateReservationRestricionsResponse> ValidateReservationRestrictions(ValidateReservationRestricionsRequest request);

        /// <summary>
        /// Updates the VCN (Virtual Card Number) information in reservation.
        /// </summary>
        /// <param name="request">The request with VCN information.</param>
        /// <returns>And empty response with the status or errors if exists.</returns>
        UpdateReservationVCNResponse UpdateReservationVCN(UpdateReservationVCNRequest request);

        List<Contracts.Data.Properties.Inventory> ValidateUpdateReservationAllotmentAndInventory(GroupRule groupRule, List<UpdateAllotmentAndInventoryDayParameters> parametersPerDay, bool validate,
                                                    bool update = true, string correlationId = null, bool isUpdateReservation = false, bool ignoreInventory = false);

        OB.BL.Contracts.Data.CRM.Guest InsertGuest(OB.BL.Contracts.Data.CRM.Guest objguest, List<long> guestActivityUIDS, OB.Reservation.BL.Contracts.Data.Reservations.Reservation objRes, 
            List<OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomExtra> objResRoomExtra);
    }
}
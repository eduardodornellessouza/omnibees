using OB.BL.Operations.Interfaces;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.REST.Services.Attributes;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace OB.REST.Services.Controllers
{
    /// <summary>
    /// OmniBees Reservation RESTful Service that provide operations for:
    /// <para/>InsertReservation
    /// <para/>UpdateReservation
    /// <para/>CancelReservation
    /// <para/>ListReservationHistories
    /// <para/>UpdatePMSReservationNumber
    /// <example>
    /// How to call InsertReservation in BSON:
    /// <c>
    /// var request = new InsertReservationRequest {.....};
    /// var client = new WebClient();
    /// client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");
    /// var ms = new MemoryStream();
    /// var serializer = new JsonSerializer();
    /// 
    /// var writer = new BsonWriter(ms);
    /// serializer.Serialize(writer, request);
    /// byte[] binaryResult = client.UploadData("http://machine/OB.REST.Services/api/Reservation/InsertReservation", "POST", ms.ToArray());
    /// var reader = new BsonReader(new MemoryStream(binaryResult));
    /// InsertReservationResponse response = serializer.Deserialize&lt;InsertReservationResponse&gt;(reader);
    ///  </c>
    /// </example>
    /// <example>
    /// How to fetch a reservation with UID=1723 in BSON:
    /// <c>
    /// var client = new WebClient();
    /// client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");
    /// var serializer = new JsonSerializer();
    /// 
    /// byte[] binaryResult = client.DownloadData("http://machine/OB.REST.Services/api/Reservation/Get?id=1723");
    /// var reader = new BsonReader(new MemoryStream(binaryResult));
    /// ListReservationResponse response = serializer.Deserialize&lt;ListReservationResponse&gt;(reader);
    ///  </c>
    /// </example>
    /// <example>
    /// How to fetch a reservation with Number=RES0001/01 for Property 1723 and channel 1 in BSON:
    /// <c>
    /// var request = new ListReservationRequest{ PropertyUID = 1723, ChannelUID = 1, .....};
    /// request.Numbers.Add("RES0001/01");
    /// var client = new WebClient();
    /// client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");
    /// var ms = new MemoryStream();
    /// var serializer = new JsonSerializer();
    /// 
    /// var writer = new BsonWriter(ms);
    /// serializer.Serialize(writer, request);
    /// byte[] binaryResult = client.UploadData("http://machine/OB.REST.Services/api/Reservation/ListReservations", "POST", ms.ToArray());
    /// var reader = new BsonReader(new MemoryStream(binaryResult));
    /// ListReservationResponse response = serializer.Deserialize&lt;ListReservationResponse&gt;(reader);
    /// var reservation = response.Result.First();
    ///  </c>
    /// </example>
    /// <example>
    /// How to get a list of ReservationHistories in BSON:
    /// <c>
    /// var request = new ListReservationHistoriesRequest{.....};
    /// var client = new WebClient();
    /// client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");
    /// var ms = new MemoryStream();
    /// var serializer = new JsonSerializer();
    /// 
    /// var writer = new BsonWriter(ms);
    /// serializer.Serialize(writer, request);
    /// byte[] binaryResult = client.UploadData("http://machine/OB.REST.Services/api/Reservation/ListReservationHistories", "POST", ms.ToArray());
    /// var reader = new BsonReader(new MemoryStream(binaryResult));
    /// ListReservationHistoriesResponse response = serializer.Deserialize&lt;ListReservationResponse&gt;(reader);
    ///  </c>
    /// </example>
    /// <example>
    /// var request = new UpdatePMSReservationNumberRequest
    /// {
    ///     PMSReservationNumberByReservationRoomUID = new Dictionary&lt;long, string&gt;
    ///     {
    ///         { 342876, "PMS-DFGER213"},
    ///         { 342878, "PMS-DFGER214"},
    ///     },
    ///     PMSReservationNumberByReservationUID = new Dictionary&lt;long, string&gt;
    ///     {
    ///         { 54894857, "PMS2-5468"},
    ///     }
    /// };
    ///  var client = new WebClient();
    ///  client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");
    ///  var ms = new MemoryStream();
    ///  var serializer = new JsonSerializer();
    ///   
    ///  var writer = new BsonWriter(ms);
    ///  serializer.Serialize(writer, request);
    ///  byte[] binaryResult = client.UploadData("http://machine/OB.REST.Services/api/Reservation/UpdatePMSReservationNumber", "POST", ms.ToArray());
    ///  var reader = new BsonReader(new MemoryStream(binaryResult));
    ///  UpdatePMSReservationNumberResponse response = serializer.Deserialize&lt;UpdatePMSReservationNumberResponse&gt;(reader);</example>
    /// </summary>    
    public class ReservationController : BaseController
    {
        private IReservationManagerPOCO _reservationManager;

        public ReservationController(IReservationManagerPOCO reservationManager)
        {
            _reservationManager = reservationManager;
        }

        /// <summary>
        /// REST Operation (POST) to create and insert a Reservation into the Database. The operation is executed inside a Transaction (ReadCommited).
        /// <para/>This operation never throws an exception. Check the Status and Errors response fields to verify if the exeuction failed due to an internal Exception.
        /// </summary>     
        /// <param name="request">The InsertReservation Request object that contains all the required objects 
        /// (Guest, Reservation , ReservationRooms, ReservationRoomDetails, ReservationRoomExtras, ReservationRoomChilds, ReservationPaymentDetails, ReservationPartialPaymentDetails) 
        /// to create and insert a Reservation into the database.</param>
        /// <returns>A InsertReservationResponse instance that contains the UID of the inserted Reservation if the Transaction completed successfully.         
        /// Insert reservation error code:
        /// <para/>INSERT GENERAL ERROR= 0 
        /// <para/>MaxiPagoGeneralError = -1 
        /// <para/>OBInternalError = -250 
        /// <para/>InvalidAllotment = -500 
        /// <para/>MaxiPagoAcquirerError = -1022 
        /// <para/>MaxiPagoParametersError = -1024 
        /// <para/>MaxiPagoMerchantCredentialError = -1025 
        /// <para/>MaxiPagoInternalError = -2048 
        /// <para/>TPI CREDIT LIMIT EXCEDED error= -3000 
        /// <para/>Operator CREDIT LIMIT EXCEDED error= -3001 
        /// <para/>Invalid Payment Method Type error= -3002 
        /// <para/>Invalid Credit Card error= -4000 
        /// <para/>Error must occur when CardNumber field is empty= -4100 
        /// <para/>Invalid guarantee type, DepositInformation is empty= -4101
        /// <para/>Guarantee type is not selected in OB= -4102
        /// <para/>Error must occur when PaymentMethodType_UID field is null= -4103
        /// <para/>ReservationError-Reservation detail must be provided= -4104
        /// <para/>
        /// </returns> 
        [ActionName("InsertReservation")]
        [AcceptVerbs("POST"/*, "PUT"*/)]
        //[Authorize(Roles="Admin")]
        public InsertReservationResponse InsertReservation(InsertReservationRequest request)
        {
            return _reservationManager.InsertReservation(request);
        }


        /// <summary>
        /// REST Operation (POST) to update an existing Reservation in the Database. The operation is executed inside a Transaction (ReadCommited).
        /// This operation never throws an exception. Check the Status and Errors response fields to verify if the exeuction failed due to an internal Exception.       
        /// </summary>
        /// <see cref="ResponseBase.Status"/>
        /// <param name="request">The UpdateReservation Request object that contains all the required objects 
        /// (Guest, Reservation , ReservationRooms, ReservationRoomDetails, ReservationRoomExtras, ReservationRoomChilds, ReservationPaymentDetails, ReservationPartialPaymentDetails) 
        /// to update the Reservation in the database.</param>
        /// <returns>A UpdateReservationResponse instance that contains the UID of the updated Reservation if the Transaction completed successfully. 
        /// Otherwise it's 0 and you need to check the Status and Errors fields.
        /// Update reservation error code:
        /// <para/>INSERT GENERAL ERROR= 0 
        /// <para/>Invalid Credit Card error= -4000 
        /// <para/>Error must occur when CardNumber field is empty= -4100 
        /// <para/>Invalid guarantee type, DepositInformation is empty= -4101
        /// <para/>Guarantee type is not selected in OB= -4102
        /// <para/>Error must occur when PaymentMethodType_UID field is null= -4103
        /// <para/>ReservationError-Reservation detail must be provided= -4104
        /// </returns>          
        [ActionName("UpdateReservation")]
        [AcceptVerbs("POST"/*, "PUT"*/)]
        //[Authorize(Roles="Admin")]
        public UpdateReservationResponse UpdateReservation([FromBody]UpdateReservationRequest request)
        {
            return _reservationManager.UpdateReservation(request);
        }

        /// <summary>
        /// REST Operation (POST) to Cancel an existing Reservation in the Database. The operation is executed inside a Transaction (ReadCommited).
        /// This operation never throws an exception. Check the Status and Errors response fields to verify if the exeuction failed due to an internal Exception.       
        /// </summary>
        /// <see cref="ResponseBase.Status"/>
        /// <param name="request">The CancelReservation Request object that contains all the required fields
        /// (PropertyUID, ReservationNo, ReservationRoomNo, UserType, CancelReservationReason_UID, CancelReservationComments)  
        /// to cancel the Reservation in the database.</param>
        /// <returns>A CancelReservationResponse instance that contains the result code of the operation:
        /// Cancel reservation error code:
        /// <para/>
        /// <para/>1 OK
        /// <para/>0 NOT OK (you need to check the Status and Errors response fields to verify if the execution failed due to an internal Exception)
        /// <para/>-100 rooms could not be return same day wait till 24:00 - abort ttansactions
        /// <para/>-200 transaction invalid consult maxipago - not abort transaction
        /// <para/>-199 cancelad in omnibees but not cancelleed in maxipago - not abort transaction
        /// <para/>
        /// </returns>
        [ActionName("CancelReservation")]
        [AcceptVerbs("POST")]
        //[Authorize(Roles="Admin")]
        public CancelReservationResponse CancelReservation([FromBody]CancelReservationRequest request)
        {
            return _reservationManager.CancelReservation(request);
        }


        /// <summary>
        /// Gets all instances of Reservation given a search Criteria (ListReservationRequest)  returning a ListReservationResponse instance with all Reservation instances
        /// that match the search criteria or none if not found any in the database that matches the given criteria..
        /// </summary>
        /// <param name="request">ListReservationRequest instance</param>
        /// <returns>A ListReservationResponse object that contains a collection of Reservation instances.</returns>
        //[Authorize(Roles="Admin")]
        [AcceptVerbs("POST")]
        //[Authorize]
        //[PrincipalPermission(SecurityAction.Demand, Role = "Occupation Levels Configuration 1;Add")]
        public ListReservationResponse ListReservations(ListReservationRequest request)
        {
            return _reservationManager.ListReservations(request);
        }

        /// <summary>
        /// Gets all the found ReservationHistory instances given their unique IDs (UID) returning a ListReservationHistoryResponse instance with 
        /// the Reservation instances.    
        /// </summary>
        /// <param name="request">A ListReservationHistoriesRequest that contains the UIDs to  search for. This should be passed in the Body of the Request</param>
        /// <returns>A ListReservationHistoriesResponse object, that contains a collection of ReservationHistories instances if successfull.</returns>
        [AcceptVerbs("POST")]
        //[Authorize(Roles="Admin")]
        public ListReservationHistoryResponse ListReservationHistories(ListReservationHistoryRequest request)
        {
            return _reservationManager.ListReservationHistories(request);
        }

        /// <summary>
        /// RESTful implementation of the UpdatePMSReservationNumber operation.
        /// This operation updates the Reservation or ReservationRoom entities with the given PMSReservationNumbers in the request object.
        /// </summary>
        /// <param name="request">A UpdatePMSReservationNumberRequest object containing the mapping between Reservations/PMSNumbers and the ReservationRooms/PMSNumber</param>
        /// <returns>A UpdatePMSReservationNumberResponse containing the result status and/or list of found errors</returns>
        [AcceptVerbs("POST")]
        //[Authorize(Roles="Admin")]
        public UpdatePMSReservationNumberResponse UpdatePMSReservationNumber(UpdatePMSReservationNumberRequest request)
        {
            return _reservationManager.UpdatePMSReservationNumber(request);
        }

        /// <summary>
        /// RESTful implementation of the SaveReservationExternalIdentifier operation.
        /// This operation updates the Reservation and/or ReservationRoom entities with the given PMSReservationNumbers in the request object.
        /// </summary>
        /// <remarks>
        /// <para>Possible erros: </para>
        /// <para>- -1 : Something is not working well, please refer to support with the Request Id: xpto.</para>
        /// <para>- -2 : The parameter '{0}' is required</para>
        /// <para>- -6 : No request sent.</para>
        /// <para>- -199 : SaveReservationExternalIdentifierRequest - PropertyId is null or empty</para>
        /// <para>- -199 : SaveReservationExternalIdentifierRequest - ClientId is null or empty</para>
        /// <para>- -199 : SaveReservationExternalIdentifierRequest - PmsId is null or empty</para>
        /// <para>- -199 : SaveReservationExternalIdentifierRequest - ReservationExternalIdentifier is null or empty</para>
        /// <para>Recomended Parameters: </para>
        /// <para>- RequestId</para>
        /// <para>Required parameters: </para>
        /// <para>- PropertyId </para> 
        /// <para>- PmsId </para>
        /// <para>- ClientId </para>
        /// <para>- ReservationExternalIdentifier </para>
        /// </remarks>
        /// <param name="request">A SaveReservationExternalIdentifier object containing the mapping between Reservation/PMSNumber and the ReservationRooms/PMSNumbers</param>
        /// <returns>A SaveReservationExternalIdentifierResponse containing the result status and/or list of found errors/warnings</returns>
        [AcceptVerbs("POST")]
        public SaveReservationExternalIdentifierResponse SaveReservationExternalIdentifier(SaveReservationExternalIdentifierRequest request)
        {
            return _reservationManager.SaveReservationExternalIdentifier(request);
        }

        /// <summary>
        /// RESTful implementation of the ListReservationUIDsByPropertyRateRoomsDateOfModifOrStay operation.
        /// This operation searchs for reservations given a specific request criteria, using property, rate/room, creation or modify date, checkin and checkout periods to search Reservations matching that criteria.
        /// </summary>
        /// <param name="request">A ListReservationUIDsByPropRateRoomAndDateOfModifOrStayRequest object containing the Search criteria</param>
        /// <returns>A ListReservationsUIDsResponse containing the UID's of matching Reservation objects</returns>
        [AcceptVerbs("POST")]
        public ListReservationsUIDsResponse ListReservationUIDsByPropertyRateRoomsDateOfModifOrStay(ListReservationUIDsByPropRateDateOfModifOrStayRequest request)
        {
            return _reservationManager.ListReservationUIDsByPropertyRateRoomsDateOfModifOrStay(request);
        }

        /// <summary>
        /// RESTful implementation of the ReservationsLight operation.
        /// This operation searchs for reservations given a specific request criteria, using uid(s), channel(s), property(s), creation or modify date, checkin and checkout periods to search Reservations matching that criteria.
        /// </summary>
        /// <param name="request">A ListReservationsLightRequest object containing the Search criteria</param>
        /// <returns>A ListReservationsLightResponse containing the UID's of matching ReservationLight objects</returns>
        [AcceptVerbs("POST")]
        [Obsolete("Use ListReservations instead. This property will be removed on OB version 0.9.49")]
        public ListReservationsLightResponse ListReservationsLight(ListReservationsLightRequest request)
        {
            return _reservationManager.ListReservationsLight(request);
        }


        /// <summary>
        /// Gets a list of reservations numbers to one or multiples properties
        /// </summary>
        /// <param name="request">ListReservationNumbersRequest instance</param>
        /// <returns>A ListReservationNumbersResponse object that contains reservation numbers for one or multiple properties</returns>
        //[Authorize(Roles="Admin")]
        [AcceptVerbs("POST")]
        public ListReservationNumbersResponse ListReservationNumbers(ListReservationNumbersRequest request)
        {
            return _reservationManager.ListReservationNumbers(request);
        }


        /// <summary>
        /// RESTful implementation of the Get Properties with Reservations for the given Channel_UID and TPI_UID.
        /// Gets a list of Properties UID's and Names's
        /// </summary>
        /// <param name="request">ListPropertiesWithReservationsForChannelOrTPIRequest instance</param>
        /// <returns>A ListPropertiesWithReservationsForChannelOrTPIResponse object that contains a list of Properties UID's and Names's</returns>
        //[Authorize(Roles="Admin")]
        [AcceptVerbs("POST")]
        public ListPropertiesWithReservationsForChannelOrTPIResponse ListPropertiesWithReservationsForChannelOrTPI(ListPropertiesWithReservationsForChannelOrTPIRequest request)
        {
            return _reservationManager.ListPropertiesWithReservationsForChannelOrTPI(request);
        }

        /// <summary>
        /// RESTful implementation of the Get Properties with Reservations for the given multiple Channels.
        /// Gets a list of Properties UID's and Names's
        /// </summary>
        /// <param name="request">ListPropertiesWithReservationsForChannelsRequest instance</param>
        /// <returns>A ListPropertiesWithReservationsForChannelOrTPIResponse object that contains a list of Properties UID's and Names's</returns>
        [AcceptVerbs("POST")]
        public ListPropertiesWithReservationsForChannelOrTPIResponse ListPropertiesWithReservationsForChannelsTpis(ListPropertiesWithReservationsForChannelsRequest request)
        {
            return _reservationManager.ListPropertiesWithReservationsForChannelsTpis(request);
        }


        /// <summary>
        /// Creates and exports reservations to a FTP Server (Perot).
        /// </summary>
        /// <param name="request">
        /// ExportReservationsToFtpRequest instance containing information to which system it should export (Currently only Perot).
        /// The instance should contain the information about which property is it affecting and a date interval to which the report is generated.
        /// </param>
        /// <returns>A PagedResponseBase object that contains the numbers of affected rows.</returns>
        //[Authorize(Roles="Admin")]
        [AcceptVerbs("POST")]
        public PagedResponseBase ExportReservationsToFtp(ExportReservationsToFtpRequest request)
        {
            return _reservationManager.ExportReservationsToFtp(request);
        }

        #region Reservation External Sources

        ///// <summary>
        ///// RESTful implementation of the ListReservationsExternalSources operation.
        ///// This operation searchs for ReservationsExternalSources entities given a specific request criteria.
        ///// </summary>
        ///// <param name="request">A ListReservationExternalSourcesRequest object containing the Search criteria</param>
        ///// <returns>A ListReservationsExternalSourcesResponse containing the List of matching ReservationsExternalSources objects</returns>
        //[AcceptVerbs("POST")]
        ////[Authorize]
        //public ListReservationExternalSourceResponse ListReservationsExternalSources(ListReservationExternalSourceRequest request)
        //{
        //    return _reservationManager.ListReservationsExternalSources(request);
        //}


        ///// <summary>
        ///// RESTful implementation of the CreateOrUpdateReservationsExternalSources operation.
        ///// This operation creates or Updates an GuestsExternalSource entities in ReservationsExternalSources table.
        ///// </summary>
        ///// <param name="request">A InsertOrUpdateReservationExternalSourcesRequest object containing the ReservationsExternalSources instance to create</param>
        ///// <returns>A ListGuestsExternalSourcesResponse that includes the errors, warnings or the Rate operation result of the creation</returns>
        //[AcceptVerbs("POST")]
        ////[Authorize]
        //public ListReservationExternalSourceResponse InsertOrUpdateReservationsExternalSources(InsertOrUpdateReservationExternalSourceRequest request)
        //{
        //    return _reservationManager.InsertOrUpdateReservationsExternalSources(request);
        //}
        #endregion

        /// <summary>
        /// Marks reservations as read.
        /// </summary>
        /// <param name="request">The request containing information about which reservations should be marked as read to which user.</param>
        /// <returns>A response containing the affected number of reservations.</returns>
        //[Authorize(Roles="Admin")]
        [AcceptVerbs("POST")]
        public MarkReservationsAsViewedResponse MarkReservationsAsViewed(MarkReservationsAsViewedRequest request)
        {
            return _reservationManager.MarkReservationsAsViewed(request, false);
        }

        /// <summary>
        /// Modify Reservation
        /// </summary>
        /// <param name="request">ModifyReservationRequest</param>
        /// <returns>
        /// <para>ModifyReservationResponse</para>
        /// <para>No allotment available Error = <b>-500</b></para> 
        /// <para>Channel is not on property Error = <b>-501</b></para> 
        /// <para>Channel is not on rate Error = <b>-502</b></para> 
        /// <para>Property channel mapping not has this combination Error = <b>-503</b></para> 
        /// <para>Invalid Agecy Error = <b>-504</b></para> 
        /// <para>Invalid company code Error = <b>-505</b></para> 
        /// <para>Missig days in date range Error = <b>-506</b></para> 
        /// <para>Worng currencie Error = <b>-507</b></para> 
        /// <para>Bortype type is different in request Error = <b>-509</b></para> 
        /// <para>Max Occupancy Exceeded in one or more rooms Error = <b>-511</b></para> 
        /// <para>Max Adult Exceeded in one or more rooms Error = <b>-512</b></para> 
        /// <para>Max child Exceeded in one or more rooms Error = <b>-513</b></para> 
        /// <para>One or more selected days are closed Error = <b>-514</b></para>
        /// <para>Minimum Length Of Stay Restriction Error = <b>-515</b></para>
        /// <para>Maximum Length Of Stay Restriction Error = <b>-516</b></para>
        /// <para>StayTrought Restriction Error = <b>-517</b></para>
        /// <para>Release days Restriction Error = <b>-518</b></para>
        /// <para>Close On Arrival Restriction Error = <b>-519</b></para>
        /// <para>Close On Departure Restriction Error = <b>-520</b></para>
        /// <para>Rate is not for selling Error = <b>-523</b></para>
        /// <para>PaymentMehtod not allowed Error = <b>-524</b></para>
        /// <para>Invalid Group code Error = <b>-526</b></para>
        /// <para>Rate is exculisive for groupcode Error = <b>-527</b></para>
        /// <para>Rate is exculisive for promo code Error = <b>-528</b></para>
        /// <para>Reservation don't exist Error = <b>-529</b></para> 
        /// <para>Reservation IsOnRequest Error = <b>-530</b></para> 
        /// <para>Provide age for all the children Error = <b>-534</b></para> 
        /// <para>Invalid Rate Error = <b>-535</b></para> 
        /// <para>Invalid CheckIn Error = <b>-536</b></para> 
        /// <para>Invalid CheckOut Error = <b>-537</b></para> 
        /// <para>Cancellation Policy Has Change Error = <b>-538</b></para> 
        /// <para>Rule Type is Required Error = <b>-539</b></para> 
        /// <para>Deposit Policy Has Change Error = <b>-540</b></para> 
        /// <para>One or more invalid roomtypes Error = <b>-542</b></para> 
        /// <para>Nothing to update Error = <b>-543</b></para> 
        /// <para>One or more Rooms have cancelation costs applied Error = <b>-544</b></para> 
        /// /// <para>Invalid Reservation Transaction Error = <b>-546</b></para> 
        /// </returns>
        [AcceptVerbs("POST")]
        public ModifyReservationResponse ModifyReservation(ModifyReservationRequest request)
        {
            var response = (ModifyReservationResponse)_reservationManager.ReservationCoordinator(request, Reservation.BL.Constants.ReservationAction.Modify);
            return response;
            //return _reservationManager.ModifyReservation(request);
        }

        /// <summary>
        /// Update Reservation Transaction Status
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [RequiresPermission("Admin")]
        public void UpdateReservationTransactionStatus(UpdateReservationTransactionStatusRequest request)
        {
            _reservationManager.UpdateReservationTransactionStatus(request);
        }

        /// <summary>
        /// List the Lost Reservations in BE
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ListLostReservationsResponse ListLostReservations(ListLostReservationsRequest request)
        {
            return _reservationManager.ListLostReservations(request);
        }

        /// <summary>
        /// Insert a Lost Reservation in the Database
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public InsertLostReservationResponse InsertLostReservation(InsertLostReservationRequest request)
        {
            return _reservationManager.InsertLostReservation(request);
        }


        /// <summary>
        /// RESTful implementation of the PaymentGatewayInfoAndModifyReservationOnRequestForTransactionID.
        /// This operation returns Info about the Reservation and ReservationTransactionStatus and Commits or Cancels the Reservation
        /// </summary>
        /// <param name="request">PaymentGatewayCommitOrCancelReservationOnRequestForTransactionIDRequest</param>
        /// <returns>PaymentGatewayCommitOrCancelReservationOnRequestForTransactionIDResponse</returns>
        [HttpPost]
        public PaymentGatewayInfoForTransactionIDResponse PaymentGatewayInfoAndModifyReservationOnRequestForTransactionID(PaymentGatewayInfoAndModifyReservationOnRequestForTransactionIDRequest request)
        {
            return _reservationManager.PaymentGatewayInfoAndModifyReservationOnRequestForTransactionID(request);
        }

        /// <summary>
        /// RESTful implementation of the GetCancelationCosts operation.
        /// This operation calculates cancelation costs of ReservationRooms.
        /// </summary>
        /// <param name="request">A GetCancelationCostsRequest object containing the Reservation with ReservationRooms and </param>
        /// <returns>A GetCancelationCostsResponse containing the result costs of each reservation room.</returns>
        [AcceptVerbs("POST")]
        public GetCancelationCostsResponse GetCancelationCosts(GetCancelationCostsRequest request)
        {
            return _reservationManager.GetCancelationCosts(request);
        }

        [AcceptVerbs("POST")]
        public GetDepositCostsResponse GetDepositCosts(GetDepositCostsRequest request)
        {
            return _reservationManager.GetDepositCosts(request);
        }

        [AcceptVerbs("POST")]
        public InsertInReservationInternalNotesResponse InsertInReservationInternalNotes(InsertInReservationInternalNotesRequest request)
        {
            return _reservationManager.InsertInReservationInternalNotes(request);
        }

        /// <summary>
        /// Gets all instances of ReservationReadStatus given a search Criteria (ListMarkReservationsAsViewedRequest)  returning a ListMarkReservationsAsViewedResponse instance with all ReservationReadStatus instances
        /// that match the search criteria or none if not found any in the database that matches the given criteria..
        /// </summary>
        /// <param name="request">ListMarkReservationsAsViewedRequest instance</param>
        /// <returns>A ListMarkReservationsAsViewedResponse object that contains a collection of ReservationReadStatus instances.</returns>
        [AcceptVerbs("POST")]
        [RequiresPermission("Admin")]
        public ListMarkReservationsAsViewedResponse ListMarkReservationsAsViewed(ListMarkReservationsAsViewedRequest request)
        {
            return _reservationManager.ListMarkReservationsAsViewed(request);
        }

        /// <summary>
        /// RESTful implementation of the ListPaymentGatewayTransactions operation.
        /// This operation searchs for PaymentGatewayTransactions given a specific request criteria to search PaymentGatewayTransactions matching that criteria.
        /// </summary>
        /// <param name="request">A ListPaymentGatewayTransactionsRequest object containing the Search criteria</param>
        /// <returns>A ListPaymentGatewayTransactionsResponse containing the UID's of matching PaymentGatewayTransaction objects</returns>
        [AcceptVerbs("POST")]
        [RequiresPermission("Admin")]
        public ListPaymentGatewayTransactionsResponse ListPaymentGatewayTransactions(ListPaymentGatewayTransactionsRequest request)
        {
            return _reservationManager.ListPaymentGatewayTransactions(request);
        }

        /// <summary>
        /// RESTful implementation of the UpdateReservationIsPaid operation.
        /// This operation updates IsPaid of Reservation entities.
        /// </summary>
        /// <param name="request">A UpdateReservationIsPaidRequest object containing the criteria to update ispaid of reservation</param>
        /// <returns>A UpdateReservationIsPaidResponse containing the result status and/or list of found errors</returns>
        [AcceptVerbs("POST")]
        [Authorize(Roles = "Admin")]
        public UpdateReservationIsPaidResponse UpdateReservationIsPaid(UpdateReservationIsPaidRequest request)
        {
            return _reservationManager.UpdateReservationIsPaid(request);
        }

        /// <summary>
        /// RESTful implementation of the UpdateReservationIsPaidBulk operation.
        /// This operation updates IsPaid of Reservations entities.
        /// </summary>
        /// <param name="request">A UpdateReservationIsPaidRequest object containing the criteria to update ispaid of reservations</param>
        /// <returns>A UpdateReservationIsPaidResponse containing the result status and/or list of found errors</returns>
        [AcceptVerbs("POST")]
        [Authorize(Roles = "Admin")]
        public UpdateReservationIsPaidBulkResponse UpdateReservationIsPaidBulk(UpdateReservationIsPaidBulkRequest request)
        {
            return _reservationManager.UpdateReservationIsPaidBulk(request);
        }

        /// <summary>
        /// RESTful implementation of the UpdateReservationCancelReason operation.
        /// This operation update reservation cancel reason.
        /// </summary>
        /// <param name="request">A UpdateReservationCancelReasonRequest object containing the criteria to update reservation cancel reason</param>
        /// <returns>A UpdateReservationCancelReasonResponse containing the result reservation Id</returns>
        [AcceptVerbs("POST")]
        [Authorize(Roles = "Admin")]
        public UpdateReservationCancelReasonResponse UpdateReservationCancelReason(UpdateReservationCancelReasonRequest request)
        {
            return _reservationManager.UpdateReservationCancelReason(request);
        }

        /// <summary>
        /// RESTful implementation of the ListCancelReservationReasons operation.
        /// This operation list all Cancel Reservation Reasons - Cache
        /// </summary>
        /// <param name="request">A ListCancelReservationReasonRequest object containing the criteria to list cancel reservation reasons</param>
        /// <returns>A ListCancelReservationReasonResponse containing the results cancelreservationreason</returns>
        [AcceptVerbs("POST")]
        public ListCancelReservationReasonResponse ListCancelReservationReasons(ListCancelReservationReasonRequest request)
        {
            return _reservationManager.ListCancelReservationReasons(request);
        }

        /// <summary>
        /// RESTful implementation of the ListReservationsFilter operation.
        /// This operation searchs for reservations given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListReservationsFilterRequest object containing the Search criteria</param>
        /// <returns>A ListReservationsFilterResponse containing the List of matching Reservation objects</returns>
        [AcceptVerbs("POST")]
        [Authorize(Roles = "Admin")]
        public ListReservationsFilterResponse ListReservationsFilter(OB.Reservation.BL.Contracts.Requests.ListReservationsFilterRequest request)
        {
            return _reservationManager.ListReservationsFilter(request);
        }

        [AcceptVerbs("POST")]
        // Omnibees Channels Needs this Service
        public GetExchangeRatesBetweenCurrenciesResponse GetExchangeRatesBetweenCurrencies(GetExchangeRatesBetweenCurrenciesRequest request)
        {
            return _reservationManager.GetExchangeRatesBetweenCurrencies(request);
        }

        [AcceptVerbs("POST")]
        public ValidatePromocodeForReservationResponse ValidatePromocodeForReservation(ValidatePromocodeForReservationRequest request)
        {
            return _reservationManager.ValidatePromocodeForReservation(request);
        }

        [AcceptVerbs("POST")]
        [RequiresPermission("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ListMyAccountReservationsOverviewResponse ListMyAccountReservationsOverview(ListMyAccountReservationsOverviewRequest request)
        {
            return _reservationManager.ListMyAccountReservationsOverview(request);
        }

        /// <summary>
        /// Approves or Refuses a On Request Reservation.
        /// </summary>
        /// <param name="request">The request containing the reservation Id, if it is approved or not, the user Id making the decision, the channel Id, channel Type and Transaction Id.</param>
        /// <returns>The response containing the status, errors and warnings of this operation.</returns>
        [AcceptVerbs("POST")]
        public ApproveOrRefuseOnRequestReservationResponse ApproveOrRefuseOnRequestReservation(ApproveOrRefuseOnRequestReservationRequest request)
        {
            return _reservationManager.ApproveOrRefuseOnRequestReservation(request);
        }

        /// <summary>
        /// Calculate Guest Past Reservations Values
        /// </summary>
        /// <param name="request">The request containing the Guest Id, PeriodicityLimitType, PeriodicityLimitValue and LoyaltyLevelBaseCurrency id.</param>
        /// <returns>The response containing the status, errors and warnings of this operation.</returns>
        [AcceptVerbs("POST")]
        public CalculateGuestPastReservationsValuesResponse CalculateGuestPastReservationsValues(CalculateGuestPastReservationsValuesRequest request)
        {
            return _reservationManager.CalculateGuestPastReservationsValues(request);
        }


        /// <summary>
        /// Have Guest Exceeded Loyalty Discount.
        /// </summary>
        /// <param name="request">The request containing the Guest Id.</param>
        /// <returns>The response containing the status, errors and warnings of this operation.</returns>
        [AcceptVerbs("POST")]
        public HaveGuestExceededLoyaltyDiscountResponse HaveGuestExceededLoyaltyDiscount(HaveGuestExceededLoyaltyDiscountRequest request)
        {
            return _reservationManager.HaveGuestExceededLoyaltyDiscount(request);
        }

        /// <summary>
        /// Calculate ReservationRoom Prices.
        /// </summary>
        /// <param name="request">The request containing the criteria to calculate reservationroom prices.</param>
        /// <returns>The response containing the status, errors and warnings of this operation.</returns>
        [AcceptVerbs("POST")]
        public CalculateReservationRoomPricesResponse CalculateReservationRoomPrices(CalculateReservationRoomPricesRequest request)
        {
            return _reservationManager.CalculateReservationRoomPrices(request);
        }

        /// <summary>
        /// Validate Reservation Restricions
        /// </summary>
        /// <param name="request">The request containing the criteria to check the reservation restrictions</param>
        /// <returns>The response containing the status, errors and warnings of this operation.</returns>
        [AcceptVerbs("POST")]
        public async Task<ValidateReservationRestricionsResponse> ValidateReservationRestrictions(ValidateReservationRestricionsRequest request)
        {
            return await _reservationManager.ValidateReservationRestrictions(request);
        }

        [AcceptVerbs("POST")]
        public ListReservationStatusesResponse ListReservationStatuses(ListReservationStatusesRequest request)
        {
            return _reservationManager.ListReservationStatuses(request);
        }

        /// <summary>
        /// Updates the VCN (Virtual Card Number) information in reservation.
        /// </summary>
        /// <param name="request">The request with VCN information.</param>
        /// <returns>And empty response with the status or errors if exists.</returns>
        [AcceptVerbs("POST")]
        public UpdateReservationVCNResponse UpdateReservationVCN(UpdateReservationVCNRequest request)
        {
            return _reservationManager.UpdateReservationVCN(request);
        }
    }
}
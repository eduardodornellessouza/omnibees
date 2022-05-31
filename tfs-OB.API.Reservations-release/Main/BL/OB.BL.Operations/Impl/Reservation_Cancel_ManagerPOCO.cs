using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Helper.Interfaces;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.TypeConverters;
using OB.DL.Common;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.Reservations;
using OB.Log.Messages;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Logs;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using contractsEvents = OB.Events.Contracts;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservations = OB.Domain.Reservations;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core;
using static OB.BL.Constants;
using System.Text;

namespace OB.BL.Operations.Impl
{
    public partial class ReservationManagerPOCO : BusinessPOCOBase, IReservationManagerPOCO
    {
        /// <summary>
        /// Gets the user of cancel reservation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="reservation">The reservation.</param>
        /// <param name="userUID">The user uid.</param>
        /// <param name="userName">Name of the user.</param>
        private void GetUserOfCancelReservation(CancelReservationRequest request, contractsReservations.Reservation reservation, out long userUID, out string userName, out bool isGuest)
        {
            userName = null;
            userUID = 0;
            isGuest = false;

            // Cancelled from Omnibees
            if (request.OBUserType > 0)
            {
                userUID = request.UserType;
                var userContract = RepositoryFactory.GetOBUserRepository().ListUsers(new Contracts.Requests.ListUserRequest { UIDs = new List<long> { request.UserType } }).FirstOrDefault();
                if (userContract != null)
                    userName = userContract.UserName;
            }
            else
            {
                // Cancelled from BE
                if (reservation.Channel_UID.HasValue && reservation.Channel_UID.Value == (int)Constants.BookingEngineChannelId)
                {
                    isGuest = true;
                    userUID = reservation.Guest_UID;
                    userName = reservation.Guest?.UserName;
                }

                // Chancelled from another Channel
                else
                {
                    userUID = Constants.AdminUserUID;
                    userName = Constants.AdminUserName;
                }
            }
        }

        /// <summary>
        /// RESTful implementation of the Cancel Reservation operation.
        /// This operation cancels a reservation
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CancelReservationResponse CancelReservation(CancelReservationRequest request)
        {
            Logger.Info("Start Canceling Reservation: UID: {0} / Number: {1} / GUID: {2}", request.ReservationUID, request.ReservationNo, request.RequestGuid);

            var response = new CancelReservationResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                GroupRule groupRule = null;
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var groupRulesRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);

                    // Comentado pois Iphone não Envia Channel Id. Assim que possivel colocar Iphone a enviar ChannelId para ativar.
                    //if (request.ReservationUID == null && request.ChannelId <= 0)
                    //    throw Errors.ChannelIdIsRequired.ToBusinessLayerException();

                    if (request.RuleType != null)
                        groupRule = groupRulesRepo.GetGroupRule(RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request));
                }

                if ((groupRule?.RuleType == domainReservations.RuleType.BE || groupRule?.RuleType == domainReservations.RuleType.BEAPI)
                    && DoCancelReservationRequest(request))
                    return new CancelReservationResponse { Status = Status.Success };

                response = CancelReservation(request, groupRule);

                response.Succeed();

            }
            catch (BusinessLayerException ex)
            {
                var log = new LogMessageBase
                {
                    ErrorCode = ex.ErrorCode.ToString(),
                    AppName = Configuration.AppName,
                    MethodName = nameof(CancelReservation),
                    RequestId = request.RequestId,
                    Environment = Configuration.Environment
                };

                //To contain new Paymentgateway erros logic.
                if (ex.Errors?.Any() == true)
                {
                    response.Errors.AddRange(ex.Errors);
                    log.Description = string.Join(";", response.Errors.Select(x => x.Description));
                }
                else
                {
                    var error = new OB.Reservation.BL.Contracts.Responses.Error(ex);
                    error.ErrorType = ex.ErrorType;
                    error.ErrorCode = ex.ErrorCode;

                    var hasDescription = Enum.TryParse(ex.ErrorCode.ToString(), out Errors description)
                        && !ex.ErrorCode.ToString().Equals(description.ToString());
                    log.Description = hasDescription ? EnumExtensions.GetDisplayAttribute(description) : string.Empty;

                    response.Errors.Add(error);
                }

                response.Failed();
                response.Result = ex.ErrorCode;
                Logger.Warn(log);
            }
            catch (DbUpdateConcurrencyException)
            {
                var ex = Errors.OperationWasInterrupted.ToBusinessLayerException();

                FailLogAndSetWarn(response, request, ex);
            }
            catch (OptimisticConcurrencyException e)
            {
                var ex = Errors.OperationWasInterrupted.ToBusinessLayerException(e);

                FailLogAndSetWarnWithRequest(response, request, ex);
            }
            catch (Exception ex)
            {
                var error = new OB.Reservation.BL.Contracts.Responses.Error(ex);

                response.Failed();
                response.Errors.Add(error);

                Logger.Error(new LogMessageBase
                {
                    ErrorCode = error.ErrorCode.ToString(),
                    AppName = Configuration.AppName,
                    MethodName = nameof(CancelReservation),
                    Description = error.Description,
                    RequestId = request.RequestId,
                    Environment = Configuration.Environment
                });
            }

            return response;
        }

        /// <summary>
        /// Check if Cancel Reservation Request is active
        /// Send email to hotel with reservation info
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private bool DoCancelReservationRequest(Reservation.BL.Contracts.Requests.CancelReservationRequest request)
        {
            // Check if hotel have Cancelation Request active
            var beStyleDetailsRepo = RepositoryFactory.GetOBBeSettingsRepository();
            var beStyleDetails = beStyleDetailsRepo.ListBeSettings(
                new Contracts.Requests.ListBeSettingsRequest
                {
                    PropertyIds = new List<long> { request.PropertyUID },
                    Fields = new HashSet<Contracts.Data.BE.BESettingsFields> { Contracts.Data.BE.BESettingsFields.NotAllowCancelReservation }
                });

            var logMessage = new LogMessageBase {
                MethodName = nameof(DoCancelReservationRequest),
                RequestId = request.RequestId
            };

            if (beStyleDetails.Status == Contracts.Responses.Status.Success
                && beStyleDetails.Result?.First()?.NotAllowCancelReservation == true
                && request.TemplateCancelRequest != null)
            {
                var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
                var proactiveActions = propertyRepo.ListProactiveActions(new Contracts.Requests.ListProactiveActionsRequest
                { PropertyId = request.PropertyUID, SystemEventIds = new List<long> { (long)SystemEvents.BookingCancelled } });

                var proactiveActionsForHotel = proactiveActions.Where(x => x.Emails?.Any() == true && !x.IsDeleted);

                if (!proactiveActionsForHotel.Any())
                {
                    logMessage.Description = "Don't have proactive actions for hotel.";
                    Logger.Debug(logMessage);
                    return false;
                }

                //build body email and send email to hotel
                var body = new StringBuilder();
                body.AppendLine($"Nome do Hotel: {request.TemplateCancelRequest.PropertyName}");
                body.AppendLine($"Numero de reserva: {request.ReservationNo}");
                body.AppendLine($"Nome do Hospede: {request.TemplateCancelRequest.GuestName}");
                body.AppendLine($"Email: {request.TemplateCancelRequest.GuestEmail}");
                body.AppendLine($"Telefone: {request.TemplateCancelRequest.GuestPhone}");
                body.AppendLine($"Observações: {request.TemplateCancelRequest.Comments}");

                var projectGeneral = Resolve<IProjectGeneral>();
                var sendResponse = projectGeneral.SendMail("notifications@omnibeesmail.com", "Omnibees",
                    string.Join(",", proactiveActionsForHotel.SelectMany(x => x.Emails).ToList()), string.Empty, string.Empty, "Solicitação de Cancelamento de Reservas",
                    body.ToString(), false, null, null, false);

                logMessage.Description = $"Was sent: {sendResponse}";
                Logger.Debug(logMessage);

                return sendResponse;
            }

            logMessage.Description = $"NotAllowCancelReservation: {beStyleDetails.Result?.First()?.NotAllowCancelReservation == true} or TemplateCancelRequest was not sent.";
            Logger.Debug(logMessage);
            return false;
        }

        private void FailLogAndSetWarn(CancelReservationResponse response, CancelReservationRequest request, BusinessLayerException ex)
        {
            var log = new LogMessageBase
            {
                ErrorCode = ex.ErrorCode.ToString(),
                AppName = Configuration.AppName,
                MethodName = nameof(CancelReservation),
                RequestId = request.RequestId,
                Environment = Configuration.Environment
            };

            var error = new OB.Reservation.BL.Contracts.Responses.Error(ex);
            error.ErrorType = ex.ErrorType;
            error.ErrorCode = ex.ErrorCode;

            var hasDescription = Enum.TryParse(ex.ErrorCode.ToString(), out Errors description)
                && !ex.ErrorCode.ToString().Equals(description.ToString());
            log.Description = hasDescription ? EnumExtensions.GetDisplayAttribute(description) : string.Empty;

            response.Errors.Add(error);

            response.Failed();
            response.Result = ex.ErrorCode;
            Logger.Warn(log);
        }
        
        private void FailLogAndSetWarnWithRequest(CancelReservationResponse response, CancelReservationRequest request, BusinessLayerException ex)
        {
            var log = new LogMessageBase
            {
                Code = ex.ErrorCode.ToString(),
                MethodName = nameof(CancelReservation),
                RequestId = request.RequestId
            };

            var error = new OB.Reservation.BL.Contracts.Responses.Error(ex);
            error.ErrorType = ex.ErrorType;
            error.ErrorCode = ex.ErrorCode;

            var hasDescription = Enum.TryParse(ex.ErrorCode.ToString(), out Errors description)
                && !ex.ErrorCode.ToString().Equals(description.ToString());
            log.Description = hasDescription ? EnumExtensions.GetDisplayAttribute(description) : string.Empty;

            response.Errors.Add(error);

            response.Failed();
            response.Result = ex.ErrorCode;
            Logger.Warn(ex, log, new LogEventPropertiesBase { Request = request });
        }

        /// <summary>
        /// Cancel Reservation Number
        /// </summary>
        /// <param name="PropertyUID"></param>boo
        /// <param name="UserName"></param>
        /// <param name="ReservationNo"></param>
        /// <returns></returns>
        private CancelReservationResponse CancelReservation(CancelReservationRequest request, GroupRule groupRule)
        {
            Contract.Ensures(Contract.Result<CancelReservationResponse>() != null);
            var response = new CancelReservationResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;
            domainReservations.Reservation reservation = null;
            List<string> ResRoomNo;
            var correlationId = Guid.NewGuid().ToString();

            // Cancel reservation error code
            //  1 OK
            //  0 NOT OK
            // -100 rooms could not be return same day wait till 24:00 - abort ttansactions
            // -200 transaction invalid consult maxipago - not abort transaction
            // -199 cancelad in omnibees but not cancelleed in maxipago - not abort trnsaction

            int resultCode = 1;
            long reservationId = 0;
            long hangfireId = 0;
            ReservationDataContext reservationContext = new ReservationDataContext();
            var reservationHelper = Resolve<IReservationHelperPOCO>();
            contractsReservations.Reservation reservationBeforeCancellation = null;
            bool transactionSucceeded = false;
            bool checkAvailability = false;
            bool ignoreAvailability = request.IgnoreAvailability;
            contractsReservations.Reservation contractReservation = null;

            using (var scope = TransactionManager.BeginTransactionScope(OB.Domain.Reservations.Reservation.DomainScope))
            {
                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                    #region REVERT RESERVATION IF IS IN PENDING STATE

                    if (!string.IsNullOrEmpty(request.TransactionId) && groupRule != null)
                    {
                        var status = reservationRepo.GetReservationTransactionState(request.TransactionId, request.ChannelId, out reservationId, out hangfireId);
                        if (status == (long)Constants.ReservationTransactionStatus.Pending)
                        {
                            IgnoreReservation(request.TransactionId, request.ChannelId, groupRule, hangfireId, request.SkipInterestCalculation);
                        }
                    }
                    #endregion REVERT RESERVATION IF IS IN PENDING STATE

                    #region GET RESERVATION

                    if (request.ReservationUID != null)
                        reservation = reservationRepo.FindByUIDEagerly(request.ReservationUID.Value);

                    else if (request.ChannelId > 0 && !string.IsNullOrEmpty(request.ReservationNo) && request.PropertyUID > 0)
                        reservation = reservationRepo.FindByReservationNumberAndChannelUIDAndPropertyUID(request.ReservationNo, request.ChannelId, request.PropertyUID);

                    else if (request.ChannelId > 0 && !string.IsNullOrEmpty(request.ReservationNo))
                        reservation = reservationRepo.FindByReservationNumberAndChannelUID(request.ReservationNo, request.ChannelId);

                    else
                        reservation = reservationRepo.FindByNumberEagerly(request.ReservationNo);

                    if (reservation == null)
                        throw Errors.ReservationDoesNotExist.ToBusinessLayerException();

                    //If the reservation has not language, we will get the property base language
                    if (!reservation.ReservationLanguageUsed_UID.HasValue)
                    {
                        var propRepo = RepositoryFactory.GetOBPropertyRepository();
                        reservationHelper.SetLanguageToReservation(reservation, reservation.Property_UID, propRepo);
                    }

                    if (reservation != null
                        && (reservation.Status == (long)Constants.ReservationStatus.Cancelled
                        || reservation.Status == (long)Constants.ReservationStatus.CancelledOnRequest))
                        throw Errors.ReservationIsAlreadyCancelledError.ToBusinessLayerException();

                    if (string.IsNullOrEmpty(request.ReservationRoomNo))
                        ResRoomNo = reservation.ReservationRooms.Select(x => x.ReservationRoomNo).ToList();
                    else
                    {
                        ResRoomNo = request.ReservationRoomNo.Split(',').ToList();

                        var alreadyCancelledRooms = reservation.ReservationRooms
                            .Where(x => ResRoomNo.Contains(x.ReservationRoomNo) &&
                                (x.Status == (int)Constants.ReservationStatus.Cancelled ||
                                 x.Status == (int)Constants.ReservationStatus.CancelledOnRequest ||
                                 x.Status == (int)Constants.ReservationStatus.CancelledPending));

                        if (alreadyCancelledRooms.Any())
                        {
                            var ex = Errors.RoomIsAlreadyCancelledError.ToBusinessLayerException(alreadyCancelledRooms.First().ReservationRoomNo);
                            ex.Errors = new List<Error>
                            {
                                new Error(ex)
                                {
                                    ErrorType = ex.ErrorType,
                                    ErrorCode = ex.ErrorCode,
                                },
                            };

                            foreach (var r in alreadyCancelledRooms.Skip(1))
                            {
                                ex.Errors.Add(new Error(Errors.RoomIsAlreadyCancelledError.ToBusinessLayerException(r.ReservationRoomNo))
                                {
                                    ErrorType = ex.ErrorType,
                                    ErrorCode = ex.ErrorCode,
                                });
                            }

                            throw ex;
                        }
                    }

                    #endregion

                    // Reservation object before cancellation (Used for LOG)
                    var converterParametersBefore = new OB.Reservation.BL.Contracts.Requests.ListReservationRequest
                    {
                        IncludeReservationRoomChilds = true,
                        IncludeReservationRoomDetails = true,
                        IncludeReservationRoomDetailsAppliedIncentives = true,
                        IncludeReservationRoomExtras = true,
                        IncludeReservationRooms = true,
                        IncludeReservationRoomTaxPolicies = true,
                        IncludeReservationPaymentDetail = true,
                        IncludeReservationPartialPaymentDetails = true,
                        IncludeReservationRoomExtrasSchedules = true,
                        IncludeRoomTypes = true,
                        IncludeRates = true,
                        IncludeExtras = true,
                        IncludeGuests = true,
                        IncludeIncentives = true
                    };
                    reservationBeforeCancellation = ConvertReservationToContractWithLookups(converterParametersBefore, new List<domainReservations.Reservation>() { reservation }).FirstOrDefault();

                    #region CANCELATION COSTS

                    if (request.GetCancelationCostsOnly)
                    {
                        var costs = reservationHelper.GetCancelationCosts(reservationBeforeCancellation);
                        response.Costs.AddRange(costs.Select(x => OtherConverter.Convert(x)));
                        response.Result = 0;
                        return response;
                    }

                    #endregion

                    #region GetReservationContext
                    reservationContext = reservationRepo.GetReservationContext(reservation.Number, reservation.Channel_UID ?? 0, reservation.TPI_UID ?? 0,
                                reservation.Company_UID ?? 0, reservation.Property_UID, reservation.UID, reservation.ReservationCurrency_UID ?? 0,
                                reservation.PaymentMethodType_UID ?? 0,
                                reservation.ReservationRooms.Select(x => x.Rate_UID.GetValueOrDefault()).Distinct(),
                                reservation.GuestFirstName, reservation.GuestLastName, reservation.GuestEmail, string.Empty, reservation.ReservationLanguageUsed_UID ?? 0);
                    #endregion

                    if (reservation != null)
                    {
                        var reservationPricesPOCO = Resolve<IReservationPricesCalculationPOCO>();
                        var reservationHistoryRepo = this.RepositoryFactory.GetReservationHistoryRepository(unitOfWork);

                        if (request.OBUserType == null && (groupRule == null || groupRule.RuleType != domainReservations.RuleType.Pull)
                            && reservation.IsOnRequest != true)
                        {
                            // verify policies
                            if (!CheckCancellationAllowedPerReservation(reservation))
                            {
                                response.Result = (int)Errors.CancellationReservationNotAllowedError;
                                response.Errors.Add(new Error
                                {
                                    ErrorType = Errors.CancellationReservationNotAllowedError.ToString(),
                                    ErrorCode = (int)Errors.CancellationReservationNotAllowedError,
                                    Description = EnumExtensions.GetDisplayAttribute(Errors.CancellationReservationNotAllowedError)
                                });
                                response.Failed();
                                return response;
                            }
                        }

                        reservationContext.ReservationUID = reservation.UID;
                        reservationContext.Guest_UID = reservation.Guest_UID;

                        // Get Previous States
                        bool hasReservationBeenBooked = false;
                        if (reservation.Status == (int)Constants.ReservationStatus.Pending)
                        {
                            var statusString = Constants.ReservationStatus.Booked.ToString();
                            if (reservationHistoryRepo.Any(x => x.Status == statusString && x.ReservationUID == reservation.UID))
                                hasReservationBeenBooked = true;
                        }

                        var roomUIDsToCancel = new List<long>();
                        decimal totalValue = 0;
                        //Cancel ReservationRoom
                        for (int index = 0; index < ResRoomNo.Count(); index++)
                        {
                            string RoomNo = ResRoomNo[index];
                            var room = reservation.ReservationRooms.FirstOrDefault(r => r.ReservationRoomNo == RoomNo && r.Status != (long)Constants.ReservationStatus.Cancelled);
                            if (room != null)
                            {
                                // Update the status of the Room
                                var previousStatus = room.Status;

                                //Sabre / Travelport
                                if (reservation.Status == (int)Constants.ReservationStatus.Pending) //&& ReservationNo.StartsWith("SBR") Isto é Necessário?
                                {
                                    if (hasReservationBeenBooked)
                                        room.Status = (int)Constants.ReservationStatus.Cancelled;
                                    else
                                        room.Status = (int)Constants.ReservationStatus.CancelledPending;
                                }
                                else if (room.Status == (int)Constants.ReservationStatus.OnRequestAccepted && groupRule.RuleType != domainReservations.RuleType.Omnibees)
                                    room.Status = (int)Constants.ReservationStatus.OnRequestChannelCancel;
                                else if (reservation.Channel_UID == 1)
                                    room.Status = room.Status == (int)Constants.ReservationStatus.BookingOnRequest && reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value ? (int)Constants.ReservationStatus.CancelledOnRequest : (int)Constants.ReservationStatus.Cancelled;
                                else
                                    room.Status = reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value ? (int)Constants.ReservationStatus.CancelledOnRequest : (int)Constants.ReservationStatus.Cancelled;

                                // Update Cancellation Date
                                if (previousStatus != 2 && room.Status == 2)
                                {
                                    roomUIDsToCancel.Add(room.UID);
                                    room.CancellationDate = DateTime.UtcNow;
                                }

                                totalValue += room.ReservationRoomsTotalAmount ?? 0;
                                room.ModifiedDate = DateTime.UtcNow;
                                //Insert cancel extra notification
                                InsertPropertyQueueOnCancelExtraNotificatoin(reservation, room.UID, request.PropertyUID);
                            }
                        }

                        //Update Reservation Status
                        decimal cancelationValue = -1;

                        var roomsNotCancelled = reservation.ReservationRooms.Count(r => r.Reservation_UID == reservation.UID &&
                                                    r.Status != (int)Constants.ReservationStatus.Cancelled && r.Status != (int)Constants.ReservationStatus.CancelledOnRequest
                                                    && r.Status != (int)Constants.ReservationStatus.OnRequestChannelCancel);

                        //Sabre / Travelport
                        if (reservation.Status == (int)Constants.ReservationStatus.Pending)
                        {
                            if (hasReservationBeenBooked)
                                reservation.Status = (int)Constants.ReservationStatus.Cancelled;
                            else
                                reservation.Status = (int)Constants.ReservationStatus.CancelledPending;
                        }
                        else if (reservation.Status == (int)Constants.ReservationStatus.OnRequestAccepted && groupRule.RuleType != domainReservations.RuleType.Omnibees)
                            reservation.Status = (int)Constants.ReservationStatus.OnRequestChannelCancel;
                        else if (reservation.Channel_UID == 1)
                            reservation.Status = roomsNotCancelled > 0 ? (long)Constants.ReservationStatus.Modified : reservation.Status == (int)Constants.ReservationStatus.BookingOnRequest && reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value ? (int)Constants.ReservationStatus.CancelledOnRequest : (int)Constants.ReservationStatus.Cancelled;
                        else
                            reservation.Status = roomsNotCancelled > 0 ? (long)Constants.ReservationStatus.Modified : reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value ? (long)Constants.ReservationStatus.CancelledOnRequest : (long)Constants.ReservationStatus.Cancelled;

                        if (reservation.Status != (long)Constants.ReservationStatus.Cancelled
                            && reservation.Status != (long)Constants.ReservationStatus.CancelledOnRequest
                            && reservation.Status != (long)Constants.ReservationStatus.CancelledPending
                            && reservation.Status != (long)Constants.ReservationStatus.OnRequestChannelCancel
                            && roomsNotCancelled > 0)
                        {
                            reservation.NumberOfRooms = roomsNotCancelled;
                        }

                        //Calculate Cancellation Room Amount
                        // if is not canceled => there is at least one room, if is canceled, the values in last room should not be decreased
                        contractsReservations.ReservationsAdditionalData reservationAdditionalDataJsonObj = null;
                        if ((reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value
                            && (reservation.Status != (long)Constants.ReservationStatus.CancelledOnRequest && reservation.Status != (long)Constants.ReservationStatus.OnRequestChannelCancel))
                            || ((!reservation.IsOnRequest.HasValue || (reservation.IsOnRequest.HasValue && !reservation.IsOnRequest.Value))
                                && reservation.Status != (long)Constants.ReservationStatus.Cancelled) || (roomsNotCancelled > 0 && ResRoomNo.Count() != roomsNotCancelled))
                        {

                            #region GET RESERVATIONADDITIONALDATA OBJECT
                            var reservationAdditionalData = reservationHelper.GetReservationAdditionalData(reservation.UID);
                            reservationAdditionalDataJsonObj = reservationHelper.GetReservationAdditionalDataJsonObject(ref reservationAdditionalData, reservation.UID);
                            #endregion

                            #region Calculate Cancellation Room Amount

                            Decimal totalAmount = 0;
                            Decimal roomPriceSum = 0;
                            Decimal roomTax = 0;
                            Decimal RommExtras = 0;
                            Decimal RoomTaxTmp = 0; // Valor total da tax do quarto a cancelar
                            int adults = 0;
                            int childrens = 0;

                            bool HasFlagPullTpiReservationCalculation = (groupRule != null
                                    && groupRule.BusinessRules.HasFlag(BusinessRules.PullTpiReservationCalculation)
                                    && reservationAdditionalDataJsonObj != null
                                    && reservationAdditionalDataJsonObj.ExternalSellingReservationInformationByRule != null
                                    && (reservationAdditionalDataJsonObj.ExternalSellingReservationInformationByRule.Any(j => j.Commission > 0 || j.Markup > 0)));

                            for (int index = 0; index < ResRoomNo.Count(); index++)
                            {
                                string RoomNo = ResRoomNo[index];

                                var reservationRoom = reservation.ReservationRooms.FirstOrDefault(rr => rr.ReservationRoomNo == RoomNo);

                                // Valor total da tax do quarto a cancelar
                                RoomTaxTmp = reservationRoom != null ? reservationRoom.TotalTax.GetValueOrDefault() : 0;

                                foreach (var reservationRoomDetail in reservationRoom.ReservationRoomDetails)
                                {
                                    totalAmount += reservationRoomDetail.Price;

                                    roomPriceSum += reservationRoomDetail.Price;
                                    roomTax += (RoomTaxTmp == 0 ? 0 : RoomTaxTmp);
                                }

                                //Room Extra Amount
                                foreach (domainReservations.ReservationRoomExtra objExt in reservationRoom.ReservationRoomExtras)
                                {
                                    totalAmount += objExt.Total_Price;
                                    RommExtras += objExt.Total_Price;
                                }

                                totalAmount = totalAmount + RoomTaxTmp;
                                adults += reservationRoom.AdultCount.GetValueOrDefault();
                                childrens += reservationRoom.ChildCount.GetValueOrDefault();

                                #region EXTERNAL SELLING TOTALS
                                if (HasFlagPullTpiReservationCalculation)
                                {
                                    var rrAdditionalData = reservationAdditionalDataJsonObj.ReservationRoomList.Where(x => x.ReservationRoomNo == RoomNo).FirstOrDefault();
                                    if (rrAdditionalData != null)
                                    {
                                        foreach (var externalRoomSellingInfo in rrAdditionalData.ExternalSellingInformationByRule)
                                        {
                                            var externalSellingRes = reservationAdditionalDataJsonObj.ExternalSellingReservationInformationByRule.Where(x => x.KeeperType == externalRoomSellingInfo.KeeperType).FirstOrDefault();
                                            if (externalSellingRes != null)
                                            {
                                                externalSellingRes.TotalAmount = externalSellingRes.TotalAmount - externalRoomSellingInfo.ReservationRoomsTotalAmount > 0 ? externalSellingRes.TotalAmount - externalRoomSellingInfo.ReservationRoomsTotalAmount : 0;
                                                externalSellingRes.RoomsPriceSum = externalSellingRes.RoomsPriceSum - externalRoomSellingInfo.ReservationRoomsPriceSum > 0 ? externalSellingRes.RoomsPriceSum - externalRoomSellingInfo.ReservationRoomsPriceSum : 0;
                                                externalSellingRes.RoomsTotalAmount = externalSellingRes.TotalAmount;
                                                externalSellingRes.TotalTax = externalSellingRes.TotalTax - externalRoomSellingInfo.TotalTax > 0 ? externalSellingRes.TotalTax - externalRoomSellingInfo.TotalTax : 0;
                                                externalSellingRes.TotalPOTax = externalSellingRes.TotalPOTax - externalRoomSellingInfo.PricesPerDay.Sum(x => x.Price) > 0 ? externalSellingRes.TotalPOTax - externalRoomSellingInfo.PricesPerDay.Sum(x => x.Price) : 0;
                                                if (externalSellingRes.CommissionIsPercentage)
                                                    externalSellingRes.TotalCommission = reservationPricesPOCO.CalculateExternalCommission(externalSellingRes.Commission, externalSellingRes.CurrencyUID, null, reservation.Property_UID, externalSellingRes.TotalAmount);
                                            }
                                        }
                                    }
                                }
                                #endregion

                            }
                            reservation.TotalAmount = reservation.TotalAmount - totalAmount > 0 ? reservation.TotalAmount - totalAmount : 0;
                            reservation.RoomsPriceSum = reservation.RoomsPriceSum - roomPriceSum > 0 ? reservation.RoomsPriceSum - roomPriceSum : 0;
                            reservation.RoomsTax = reservation.RoomsTax - roomTax > 0 ? reservation.RoomsTax - roomTax : 0;
                            reservation.TotalTax = (reservation.TotalTax.HasValue ? reservation.TotalTax : 0) - RoomTaxTmp;
                            reservation.RoomsExtras = (reservation.RoomsExtras.HasValue ? reservation.RoomsExtras : 0) - RommExtras;
                            reservation.RoomsTotalAmount = reservation.TotalAmount;
                            reservation.Adults = adults > 0 && (reservation.Adults - adults > 0)
                                ? (reservation.Adults - adults)
                                : GetOccupancyFromAvailableReservationRooms(reservation, x => x.AdultCount);
                            if (reservation.Children.HasValue || childrens > 0)
                                reservation.Children = childrens > 0 && (reservation.Children - childrens > 0)
                                    ? (reservation.Children - childrens)
                                    : GetOccupancyFromAvailableReservationRooms(reservation, x => x.ChildCount) ;

                            cancelationValue = totalAmount;

                            #endregion Calculate Cancellation Room Amount

                            #region SAVE RESERVATION ADDITIONAL DATA
                            if (HasFlagPullTpiReservationCalculation)
                            {
                                var additionalDataRepo = RepositoryFactory.GetRepository<ReservationsAdditionalData>(unitOfWork);
                                reservationAdditionalDataJsonObj.AlreadySentToPMS = request.AlreadySentToPMS;
                                reservationAdditionalData.ReservationAdditionalDataJSON = reservationAdditionalDataJsonObj.ToJSON();
                                if (reservationAdditionalData != null && reservationAdditionalData.UID > 0)
                                    additionalDataRepo.AttachAsModified(reservationAdditionalData);
                                else
                                    additionalDataRepo.Add(reservationAdditionalData);
                            }
                            #endregion

                        }

                        //Cancel Reservation Reason
                        reservation.CancelReservationReason_UID = request.CancelReservationReason_UID;
                        reservation.CancelReservationComments = request.CancelReservationComments;

                        reservation.ModifyDate = DateTime.UtcNow;



                        // Decrement reservations completed from ReservationRoomDetailsAppliedPromocode table
                        if (reservation.PromotionalCode_UID > 0 && roomUIDsToCancel.Any())
                        {
                            var discountDates = reservation.ReservationRooms.Where(rr => roomUIDsToCancel.Contains(rr.UID)).SelectMany(rr => rr.ReservationRoomDetails)
                                .SelectMany(rrd => rrd.ReservationRoomDetailsAppliedPromotionalCodes).Select(appliedPromo => appliedPromo.Date).Distinct().ToList();
                            reservationHelper.UpdatePromoCodeReservationsCompleted(reservation.PromotionalCode_UID.Value, oldDays: discountDates);
                        }

                        if ((reservationBeforeCancellation.Status == (int)Constants.ReservationStatus.Booked)
                            || (reservationBeforeCancellation.Status == (int)Constants.ReservationStatus.Modified)
                            || (reservationBeforeCancellation.Status == (int)Constants.ReservationStatus.OnRequestAccepted)
                            || (reservationBeforeCancellation.Status == (int)Constants.ReservationStatus.Pending))
                        {
                            var ratesRepo = RepositoryFactory.GetOBRateRepository();
                            var rateUIDs = reservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).Where(x => x.Rate_UID > 0).Select(x => x.Rate_UID.Value).Distinct().ToList();
                            var rateAvailabilityTypes = rateUIDs.Any() ?
                                ratesRepo.ListRatesAvailablityType(new OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest { RatesUIDs = rateUIDs }) : new Dictionary<long, int>();

                            // Builds parameters to Update Allotment and Inventory
                            var daysToDecrement = new List<UpdateAllotmentAndInventoryDayParameters>();

                            foreach (var groupByRateRoom in reservation.ReservationRooms.GroupBy(x => new { x.RoomType_UID, AvailabilityType = x.Rate_UID.HasValue && rateAvailabilityTypes.Any() ? rateAvailabilityTypes[x.Rate_UID.Value] : 0 }))
                            {
                                List<ReservationRoomDetail> rrds = new List<ReservationRoomDetail>();

                                //when cancel only one room
                                if (!String.IsNullOrEmpty(request.ReservationRoomNo))
                                {
                                    rrds = groupByRateRoom.Where(x => x.ReservationRoomNo == request.ReservationRoomNo).SelectMany(x => x.ReservationRoomDetails).ToList();
                                }
                                //when cancel all reservation
                                else
                                {
                                    var rrCancelled = reservationBeforeCancellation.ReservationRooms.Where(x => x.Status == (int)Constants.ReservationStatus.Cancelled).Select(x => x.ReservationRoomNo).ToList();
                                    var rrdsAll = groupByRateRoom.Select(x => x.ReservationRoomNo).ToList();
                                    var rrdsAvaliable = rrdsAll.Except(rrCancelled);

                                    rrds = groupByRateRoom.Where(x => rrdsAvaliable.Contains(x.ReservationRoomNo)).SelectMany(x => x.ReservationRoomDetails).ToList();
                                }

                                foreach (var decrementDay in rrds.GroupBy(x => x.Date.Date))
                                {
                                    var rrd = decrementDay.First();

                                    daysToDecrement.Add(new UpdateAllotmentAndInventoryDayParameters()
                                    {
                                        Day = decrementDay.Key,
                                        RoomTypeUID = groupByRateRoom.Key.RoomType_UID ?? 0,
                                        AddQty = -decrementDay.Count(),
                                        RateRoomDetailUID = rrd.RateRoomDetails_UID ?? 0,
                                        RateAvailabilityType = groupByRateRoom.Key.AvailabilityType
                                    });
                                }
                            }

                            if (!reservationContext.IsAvailabilityRestoreStopped)
                            {
                                // Updates Allotment and Inventory
                                var inventories = this.ValidateUpdateReservationAllotmentAndInventory(groupRule, daysToDecrement, false, true, correlationId, ignoreAvailability: ignoreAvailability);
                                if (reservationContext.Inventories == null)
                                    reservationContext.Inventories = new List<contractsProperties.Inventory>();
                                else
                                    reservationContext.Inventories.RemoveAll(x => reservation.ReservationRooms.Select(rr => rr.RoomType_UID).Contains(x.RoomType_UID));

                                reservationContext.Inventories.AddRange(inventories);
                            }
                        }

                        // decrement operator credit
                        this.DecrementOperatorCreditUsed(reservation, cancelationValue);

                        // decrement tpi credit
                        this.DecrementCreditUsed(reservation, false, false);

                        #region Convert Reservation
                        contractReservation = DomainToBusinessObjectTypeConverter.Convert(reservation, new ListReservationRequest
                        {
                            IncludeReservationRooms = true,
                            IncludeReservationPartialPaymentDetails = true,
                            IncludeReservationPaymentDetail = true,
                            IncludeReservationRoomDetails = true
                        });
                        contractReservation.ReservationAdditionalData = reservationAdditionalDataJsonObj;
                        #endregion Convert Reservation

                        #region CHECK AVAILABILITY
                        if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.IsToPreCheckAvailability))
                        {
                            checkAvailability = CheckAvailability(request.RequestId, reservation);
                        }

                        #endregion CHECK AVAILABILITY

                        #region RESERVATION FILTER
                        var reservationFilterRepo = this.RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                        var reservationFilter = reservationFilterRepo.FindByReservationUIDs(new List<long> { reservation.UID }).FirstOrDefault();

                        var parameters = new TreatReservationFiltersParameters()
                        {
                            NewReservation = contractReservation,
                            ReservationFilter = reservationFilter,
                            ServiceName = ServiceName.CancelReservation
                        };
                        reservationHelper.TreatReservationFilter(parameters, unitOfWork);

                        if (reservationFilter == null)
                            reservationFilterRepo.Add(parameters.ReservationFilter);
                        else
                            reservationFilterRepo.AttachAsModified(parameters.ReservationFilter);

                        unitOfWork.Save();
                        #endregion

                        ////Avoid secound request to cancel the reservation again
                        //if (CheckReservationCanceled(reservation.UID))
                        //{
                        //    response.Result = (int)Errors.ReservationIsAlreadyCancelledError;
                        //    response.Errors.Add(new Error
                        //    {
                        //        ErrorType = Errors.ReservationIsAlreadyCancelledError.ToString(),
                        //        ErrorCode = (int)Errors.ReservationIsAlreadyCancelledError,
                        //        Description = EnumExtensions.GetDisplayAttribute(Errors.ReservationIsAlreadyCancelledError)
                        //    });
                        //    response.Failed();

                        //    return response;
                        //}

                        //paymentgateway

                        #region PaymentGateway

                        if (!string.IsNullOrEmpty(reservation.PaymentGatewayName) && !request.AlreadyCancelledFromPaymentGateway)
                        {
                            if (reservationBeforeCancellation.Channel_UID == 1 && (reservation.PaymentGatewayName == "BPag" && (reservationBeforeCancellation.Status == (int)Constants.ReservationStatus.BookingOnRequest && (reservationBeforeCancellation.IsOnRequest.HasValue && reservationBeforeCancellation.IsOnRequest.Value == true))))
                            {
                                // No Need to Do Refund
                            }
                            else
                            {
                                resultCode = this.RefundPaymentGateway(reservation, (decimal)(totalValue / reservation.PropertyBaseCurrencyExchangeRate), request.SkipInterestCalculation, ResRoomNo.Count(), request.RequestId, handleWithGatewayException: true);

                                if (resultCode < 0 && resultCode != -200)
                                {
                                    response.Result = resultCode;
                                    return response;
                                }
                            }
                        }

                        // error maxipago
                        if (resultCode == -200)
                        {
                            resultCode = -200 + 1;// -199 cancelad in omnibees but not cancelleed in maxipago
                        }

                        #endregion PaymentGateway

                        unitOfWork.Save();
                    }

                    scope.Complete();

                    transactionSucceeded = true;
                }
            }

            if (reservation != null && transactionSucceeded)
            {
                CancelReservationLogHandler(request, contractReservation, reservationBeforeCancellation, reservationContext);

                QueueBackgroundWork(() =>
                {
                    if (hangfireId > 0)
                        reservationHelper.DeleteHangfireJob(hangfireId);

                    CancelReservationBackgroundWork(request, groupRule, reservation, correlationId, hangfireId, reservationBeforeCancellation, reservationContext, checkAvailability);
                });
            }

            response.ReservationStatus = reservation?.Status;
            response.Result = resultCode;
            return response;
        }

        private void CancelReservationBackgroundWork(CancelReservationRequest request, GroupRule groupRule, domainReservations.Reservation reservation, string correlationId,
            long hangfireId, contractsReservations.Reservation reservationBeforeCancellation, ReservationDataContext reservationContext, bool checkAvail = false)
        {
            var logger = new Log.DefaultLogger("ReservationBackgroundWork");
            logger.Debug("CancelReservationBackgroundWork: RequestGuid: {0} - ReservationUID {1}", request.RequestGuid, reservation.UID);

            contractsReservations.Reservation contractReservation = null;
            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationRoomDetailRepo = this.RepositoryFactory.GetRepository<domainReservations.ReservationRoomDetail>(unitOfWork);
                    var propertyPoco = RepositoryFactory.GetOBPropertyRepository();
                    contractReservation = new contractsReservations.Reservation();

                    #region convert reservation to contract
                    //the method GenerateListReservationRequest was created because the request have so many Includes
                    var converterParameters = GenerateListReservationRequest();

                    contractReservation = ConvertReservationToContractWithLookups(converterParameters, new List<domainReservations.Reservation>() { reservation }).FirstOrDefault();
                    #endregion

                    if (reservation.Status != (long)Constants.ReservationStatus.CancelledPending)
                    {
                        if (reservation.Status == (long)Constants.ReservationStatus.Modified)
                        {
                            this.InsertPropertyQueue(reservation.Property_UID, reservation.UID, reservation.Channel_UID, (long)Constants.SystemEventsCode.BookingChanged);
                        }
                        else
                        {
                            if (reservation.Status == (long)Constants.ReservationStatus.Cancelled)
                            {
                                this.InsertPropertyQueue(reservation.Property_UID, reservation.UID, reservation.Channel_UID, (long)Constants.SystemEventsCode.Bookingcancelled);
                            }
                            else if (reservation.Status == (long)Constants.ReservationStatus.CancelledOnRequest || reservation.Status == (long)Constants.ReservationStatus.OnRequestChannelCancel)
                            {
                                this.InsertPropertyQueue(reservation.Property_UID, reservation.UID, reservation.Channel_UID, (long)Constants.SystemEventsCode.CancelOnRequest);
                            }

                            this.CancelPropertyQueueEvent(reservation.Property_UID, (long)Constants.SystemEventsCode.PreStay, reservation.UID, "Reservation was Cancelled!!!");
                            this.CancelPropertyQueueEvent(reservation.Property_UID, (long)Constants.SystemEventsCode.PostStay, reservation.UID, "Reservation was Cancelled!!!");
                            this.CancelPropertyQueueEvent(reservation.Property_UID, (long)Constants.SystemEventsCode.ExtraNotificationEmail, reservation.UID, "Reservation was Cancelled!!!");
                        }


                        #region SEND ALLOTMENT FOR CHANNELS
                        if (!reservationContext.IsAvailabilityRestoreStopped)
                        {
                            SendAllotmentAndInventoryForChannels(contractReservation.Property_UID, contractReservation.Channel_UID ?? 0, null, null, contractReservation.ReservationRooms, ServiceName.CancelReservation, reservationContext.Inventories);
                        }

                        #endregion SEND ALLOTMENT FOR CHANNELS

                        //notify connectors async
                        //(reservation.Status == 1 || reservation.Status == 2 || reservation.Status == 4)
                        if  (( reservation.Status == (long)Constants.ReservationStatus.Booked
                            || reservation.Status == (long)Constants.ReservationStatus.Cancelled
                            || reservation.Status == (long)Constants.ReservationStatus.Modified)
                            && !request.AlreadySentToPMS)
                        {
                            if (reservation.ReservationRooms.Any(r => r.Status != (long)Constants.ReservationStatus.Cancelled))
                            {
                                this.NotifyConnectors("Reservation", reservation.UID, reservation.Property_UID, (int)Constants.ConnectorEventOperations.Update, null, checkAvail, requestId: request.RequestId);//2
                            }
                            else
                            {
                                this.NotifyConnectors("Reservation", reservation.UID, reservation.Property_UID, (int)Constants.ConnectorEventOperations.Cancel, null, checkAvail, requestId: request.RequestId);//3
                            }

                        }
                    }

                    #region ReservationHistory

                    long userUID;
                    string userName;
                    bool isGuest;
                    this.GetUserOfCancelReservation(request, contractReservation, out userUID, out userName, out isGuest);
                    string hostAdress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostName : string.Empty;

                    this.SaveReservationHistory(new ReservationLogRequest
                    {
                        ServiceName = OB.Reservation.BL.Contracts.Data.General.ServiceName.CancelReservation,
                        OldReservation = reservationBeforeCancellation,
                        Reservation = contractReservation,
                        //GuestActivity = objGuestActivity,
                        UserHostAddress = hostAdress,
                        UserIsGuest = isGuest,
                        UserID = userUID,
                        UserName = userName,
                        ReservationTransactionId = request.TransactionId,
                        ReservationTransactionState = OB.Reservation.BL.Constants.ReservationTransactionStatus.Commited,
                        GroupRule = groupRule != null ? (int)groupRule.RuleType : new Nullable<int>(),
                        BigPullAuthRequestor_UID = request.BigPullAuthRequestor_UID,
                        propertyName = reservationContext.PropertyName,
                        ChannelName = reservationContext.ChannelName
                    });

                    #endregion ReservationHistory

                    // Mark as UnRead
                    this.MarkReservationsAsViewed(new MarkReservationsAsViewedRequest
                    {
                        Reservation_UIDs = new List<long> { reservation.UID },
                        User_UID = 65,
                        NewValue = false,
                        RequestGuid = Guid.NewGuid()
                    });

                }

                // Set TripAdvisor Review
                if (reservationBeforeCancellation.ReservationRooms != null &&
                    reservationBeforeCancellation.ReservationRooms.Any() &&
                    reservationBeforeCancellation.ReservationRooms.FirstOrDefault().DateTo.HasValue &&
                    reservationBeforeCancellation.Channel_UID == (int)Constants.BookingEngineChannelId)
                {
                    SetTripAdvisorReview(reservation.Property_UID, reservation.UID, reservation.GuestEmail,
                                reservationBeforeCancellation.ReservationRooms.FirstOrDefault().DateTo.Value,
                                reservationBeforeCancellation.ReservationLanguageUsed_UID,
                                (int)Constants.TripAdvisorReviewAction.Delete, reservation.ReservationLanguageUsed_UID,
                                reservationBeforeCancellation.ReservationRooms.FirstOrDefault().DateFrom);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "CancelReservationBackgroundWork");

                Dictionary<string, object> arguments = new Dictionary<string, object>();
                arguments.Add("Reservation", contractReservation);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments, null,
                    "Ocorreram erros nas tarefas de background do CancelReservation");
                throw;
            }

        }

        private void CancelReservationLogHandler(CancelReservationRequest request, contractsReservations.Reservation contractReservation, contractsReservations.Reservation reservationBeforeCancellation, ReservationDataContext reservationDataContext)
        {
            long userUID;
            string userName;
            bool isGuest;
            this.GetUserOfCancelReservation(request, contractReservation, out userUID, out userName, out isGuest);

            this.NewLogReservation(OB.Reservation.BL.Contracts.Data.General.ServiceName.CancelReservation,
                userUID,
                userName,
                reservationDataContext.PropertyName,
                oldReservation: reservationBeforeCancellation,
                contractReservation,
                channelName: reservationDataContext.ChannelName,
                propertyBaseCurrencyId: reservationDataContext.PropertyBaseCurrency_UID,
                requestId: request.RequestId);
        }

        private int GetOccupancyFromAvailableReservationRooms(domainReservations.Reservation reservation, Func<ReservationRoom, int?> selector)
        {
            return reservation.ReservationRooms.Where(r => selector.Invoke(r).HasValue && r.Status != (int)Constants.ReservationStatus.Cancelled && r.Status != (int)Constants.ReservationStatus.CancelledOnRequest
                                                     && r.Status != (int)Constants.ReservationStatus.OnRequestChannelCancel).Sum(s => selector.Invoke(s).Value);
        }

        /// <summary>
        /// Check if there are any record on ReservationsHistory where the reservation has been canceled.
        /// </summary>
        /// <returns>True is the reservation has already been cancelled, false instead.</returns>
        private bool CheckReservationCanceled(long reservationId)
        {
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork(true))
            {
                var reservationHistoryRepo = this.RepositoryFactory.GetReservationHistoryRepository(unitOfWork);

                var statusString = Constants.ReservationStatus.Cancelled.ToString();
                if (reservationHistoryRepo.Any(x => x.Status == statusString && x.ReservationUID == reservationId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Insert into property queue for cancel Extra Notificatoin email while room cancellation.
        /// </summary>
        /// <param name="ReservationRoom_UID"></param>
        /// <param name="PropertyUID"></param>
        private void InsertPropertyQueueOnCancelExtraNotificatoin(domainReservations.Reservation reservation, long ReservationRoom_UID, long PropertyUID)
        {
            var extraRepo = this.RepositoryFactory.GetOBExtrasRepository();

            var reservationRoom = reservation.ReservationRooms.FirstOrDefault(x => x.UID == ReservationRoom_UID);

            if (reservationRoom == null)
                return;

            //Check if any extra having notification email then insert into property queue.
            List<ReservationRoomExtra> lstReservationRoomExtras = reservationRoom.ReservationRoomExtras.Where(rre => rre.Extra_UID > 0).ToList();

            if (lstReservationRoomExtras != null && lstReservationRoomExtras.Count > 0)
            {
                var reservationRoomExtraUIDs = lstReservationRoomExtras.Select(x => x.Extra_UID).ToList();

                List<OB.BL.Contracts.Data.Rates.Extra> extras = extraRepo.ListExtras(new Contracts.Requests.ListExtraRequest { UIDs = reservationRoomExtraUIDs }).Where(x => !string.IsNullOrEmpty(x.NotificationEmail)).ToList();

                foreach (ReservationRoomExtra objExtra in lstReservationRoomExtras)
                {
                    this.InsertExtraNotificationPropertyQueue(objExtra, OB.BL.Resources.BillingMandatoryFields.lblCancelledExtra, (int)Constants.SystemTemplatesCodes.CancelExtraNotificationEmail, (int)Constants.SystemEventsCode.ExtraNotificationEmail);
                }
            }
        }

        /// <summary>method to check if all the rooms are allowed to cancel</summary>
        public bool CheckCancellationAllowedPerReservation(domainReservations.Reservation reservation)
        {
            return CheckCancellationAllowedPerRoom(reservation).All(x => x);
        }

        public List<bool> CheckCancellationAllowedPerRoom(domainReservations.Reservation reservation)
        {
            var propRepo = this.RepositoryFactory.GetOBPropertyRepository();

            var rooms = reservation.ReservationRooms;
            DateTime now = DateTime.UtcNow;

            var result = Enumerable.Repeat(false, rooms.Count).ToList();

            for (int i = 0; i < rooms.Count; i++)
            {
                var resRoom = rooms.ElementAt(i);

                if (resRoom == null)
                    continue;

                now = propRepo.ConvertToPropertyTimeZone(new Contracts.Requests.ConvertToPropertyTimeZoneRequest
                {
                    PropertyId = reservation.Property_UID,
                    Dates = new List<DateTime> { now }
                }).FirstOrDefault();

                // has cancellation policy
                if (resRoom != null && !string.IsNullOrEmpty(resRoom.CancellationPolicy) && resRoom.IsCancellationAllowed.HasValue)
                {
                    int DaysLeft = resRoom.DateFrom.Value.Subtract(now).Days;

                    if (!resRoom.IsCancellationAllowed.Value) // no cancellation allowed
                        result[i] = false;
                    else
                        if (!(DaysLeft >= resRoom.CancellationPolicyDays))
                        result[i] = false; // as soon as we find a non cancellable room, we return false
                    else
                        result[i] = true;
                }
                else
                    result[i] = true; // no policy, cancelation is allowed
            }
            return result;
        }

        private OB.Reservation.BL.Contracts.Requests.ListReservationRequest GenerateListReservationRequest(long? reservationId = null)
        {
            var converterParameters = new OB.Reservation.BL.Contracts.Requests.ListReservationRequest
            {
                ReservationUIDs = reservationId != null ? new List<long> { (long)reservationId } : null,
                IncludeChannel = true,
                IncludeReservationReadStatus = true,
                IncludeReservationRooms = true,
                IncludeReservationAddicionalData = true,
                IncludeReservationStatusName = true,
                IncludeTPIName = true,
                IncludeRates = true,
                IncludePaymentMethodType = true,
                IncludeChannelOperator = true,
                IncludeRoomTypes = true,
                IncludeReservationResumeInfo = true,
                IncludeReferralSource = true,
                IncludeExternalSource = true,
                IncludeCompanyName = true,
                IncludeReservationCurrency = true,
                IncludeCommissionTypeName = true,
                IncludeTPICommissions = true,
                IncludeReservationRoomDetails = true,
                IncludeReservationRoomExtras = true,
                IncludeReservationRoomTaxPolicies = true,
                IncludeGuestCountryName = true,
                IncludeGuestStateName = true,
                IncludeGuestPrefixName = true,
                IncludeGuestActivities = true,
                IncludeGuests = true,
                IncludeTransferLocation = true,
                IncludePromotionalCodes = true,
                IncludeReservationRoomDetailsAppliedPromotionalCode = true,
                IncludeGroupCodes = true,
                IncludeBillingCountryName = true,
                IncludeBillingStateName = true,
                IncludePropertyBaseCurrency = true,
                IncludeReservationBaseCurrency = true,
                ReturnTotal = true,
                IncludeReservationRoomChilds = true,
                IncludeExtras = true,
                IncludeExtrasBillingTypes = true,
                IncludeReservationRoomExtrasSchedules = true,
                IncludeReservationRoomExtrasAvailableDates = true,
                IncludeTaxPolicies = true,
                IncludeReservationRoomDetailsAppliedIncentives = true,
                IncludeIncentives = true,
                IncludeBESpecialRequests = true,
                IncludeCancelationCosts = true,
                IncludeTPILanguageUID = true,
                IncludeOnRequestDecisionUser = true,
                IncludeReservationRoomIncentivePeriods = true,
            };

            return converterParameters;
        }

        private void CancelPropertyQueueEvent(long propertyID, long SystemEventCode, long taskTypeID, string cancelReason)
        {
            var propertyEventRepo = this.RepositoryFactory.GetOBPropertyEventsRepository();
            propertyEventRepo.CancelPropertyQueueEvent(new Contracts.Requests.CancelPropertyQueueEventRequest { PropertyId = propertyID, SystemEventCode = SystemEventCode, TaskTypeId = taskTypeID, ReasonCancelation = cancelReason });
        }

        private IEnumerable<ReservationRoomDetailRealAllotment> CancelRooms(OB.Domain.Reservations.Reservation reservation, List<contractsReservations.ReservationRoom> rooms, long Property_UID, string correlationId)
        {
            List<ReservationRoomDetailRealAllotment> result = new List<ReservationRoomDetailRealAllotment>();
            if (rooms != null && rooms.Count > 0)
            {
                var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

                foreach (var room in rooms)
                {
                    if (room.Status != 2)
                    {
                        room.Status = (int)Constants.ReservationStatus.Cancelled;
                        room.CancellationDate = DateTime.UtcNow;

                        //Insert cancel Extra notification email in property queue.
                        this.InsertPropertyQueueOnCancelExtraNotificatoin(reservation, room.UID, Property_UID);

                        DateTime ini = room.DateFrom.HasValue ? room.DateFrom.Value : new DateTime();
                        DateTime end = room.DateTo.HasValue ? room.DateTo.Value.AddDays(-1) : new DateTime();

                        //update inventory
                        if (ini != new DateTime() && end != new DateTime())
                        {
                            //get inventory
                            List<OB.BL.Contracts.Data.Properties.Inventory> inventory = propertyRepo.ListInventory(new Contracts.Requests.ListInventoryRequest
                            {
                                roomTypeIdsAndDateRange = new List<Contracts.Data.Properties.InventorySearch>
                            {
                                    new Contracts.Data.Properties.InventorySearch
                                    { RoomType_UID = room.RoomType_UID ?? 0,
                                        DateFrom = ini,
                                        DateTo = end }
                                }
                            });

                            foreach (OB.BL.Contracts.Data.Properties.Inventory inv in inventory)
                            {
                                inv.QtyRoomOccupied = inv.QtyRoomOccupied > 0 ? inv.QtyRoomOccupied - 1 : 0;
                            }
                        }

                        //decrememt AllotmentUsed in rateroomdetails
                        var allotments = DecrementRealAllotmentRateRoomDetailsByReservationRoom(room.UID);
                        result.AddRange(allotments);
                    }
                }
            }
            return result;
        }

        #region Cancel Reservation
        private contractsEvents.Portable.Infrastructure.PropertyDictionary LoggingCancelReservationRoom(contractsReservations.ReservationRoom reservationRoomBeforeCancellation, contractsReservations.ReservationRoom reservationRoomAfterCancellation, decimal cancellationCost)
        {
            if (reservationRoomBeforeCancellation == null)
                reservationRoomBeforeCancellation = new contractsReservations.ReservationRoom();
            if (reservationRoomAfterCancellation == null)
                reservationRoomAfterCancellation = new contractsReservations.ReservationRoom();

            var currencyRepo = this.RepositoryFactory.GetOBCurrencyRepository();

            string roomName = this.GetReservationRoomName(reservationRoomAfterCancellation);
            string rateName = reservationRoomAfterCancellation.Rate != null ? reservationRoomAfterCancellation.Rate.Name : null;

            // Get Rate Currency Symbol
            long rateCurrencyUID = (reservationRoomAfterCancellation.Rate != null) ? reservationRoomAfterCancellation.Rate.Currency_UID : -1;
            var rateCurrency = currencyRepo.ListCurrencies(new Contracts.Requests.ListCurrencyRequest { UIDs = new List<long> { rateCurrencyUID } }).FirstOrDefault();
            string rateCurrencySymbol = (rateCurrency != null) ? string.Format("{0}({1})", rateCurrency.CurrencySymbol, rateCurrency.Symbol) : null;

            // Get Cancellation cost symbol
            string cancellationCostSymbol = null;
            switch (reservationRoomAfterCancellation.CancellationPaymentModel)
            {
                case (int)Constants.CancellationPoliciesPaymentModel.FixedValue:
                    cancellationCostSymbol = rateCurrencySymbol;
                    break;
                case (int)Constants.CancellationPoliciesPaymentModel.Percentage:
                    cancellationCostSymbol = "%";
                    break;
            }


            var entityProperties = new contractsEvents.Portable.Infrastructure.PropertyDictionary();
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Name, roomName, roomName);
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.RateName, rateName, rateName);
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationDate, reservationRoomAfterCancellation.CancellationDate, reservationRoomBeforeCancellation.CancellationDate);
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationCosts, reservationRoomAfterCancellation.CancellationCosts, reservationRoomBeforeCancellation.CancellationCosts);
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationPolicyDays, reservationRoomAfterCancellation.CancellationPolicyDays, reservationRoomBeforeCancellation.CancellationPolicyDays);
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationNrNights, reservationRoomAfterCancellation.CancellationNrNights, reservationRoomBeforeCancellation.CancellationNrNights);
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationPolicy, reservationRoomAfterCancellation.CancellationPolicy, reservationRoomBeforeCancellation.CancellationPolicy);

            // Cancellation Value
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationValue,
                string.Format("{0:0.00} {1}", reservationRoomAfterCancellation.CancellationValue, cancellationCostSymbol),
                string.Format("{0:0.00} {1}", reservationRoomBeforeCancellation.CancellationValue, cancellationCostSymbol));

            // Calculated Cancellation Cost
            string roomCancellationCost = (rateCurrencySymbol != null) ? string.Format("{0:0.00} {1}", cancellationCost, rateCurrencySymbol) : cancellationCost.ToString("0.00");
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationCostsCalculated, roomCancellationCost, roomCancellationCost);

            // Room Status
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Status
                , Enum.GetName(typeof(Constants.ReservationStatus), reservationRoomAfterCancellation.Status)
                , Enum.GetName(typeof(Constants.ReservationStatus), reservationRoomBeforeCancellation.Status));

            // Checkin Date
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DateFrom
                , (reservationRoomAfterCancellation.DateFrom.HasValue) ? reservationRoomAfterCancellation.DateFrom.Value.Date : new Nullable<DateTime>()
                , (reservationRoomBeforeCancellation.DateFrom.HasValue) ? reservationRoomBeforeCancellation.DateFrom.Value.Date : new Nullable<DateTime>());

            // Checkout Date
            entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DateTo
                , (reservationRoomAfterCancellation.DateTo.HasValue) ? reservationRoomAfterCancellation.DateTo.Value.Date : new Nullable<DateTime>()
                , (reservationRoomBeforeCancellation.DateTo.HasValue) ? reservationRoomBeforeCancellation.DateTo.Value.Date : new Nullable<DateTime>());

            //entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ReservationRoomNo, reservationRoomAfterCancellation.ReservationRoomNo, reservationRoomBeforeCancellation.ReservationRoomNo); // Reservation Room Number
            //entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.GuestName, reservationRoomAfterCancellation.GuestName, reservationRoomBeforeCancellation.GuestName); // Guest Name
            //entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.AdultCount, reservationRoomAfterCancellation.AdultCount, reservationRoomBeforeCancellation.AdultCount); // Number of Adults
            //entityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ChildCount, reservationRoomAfterCancellation.ChildCount, reservationRoomBeforeCancellation.ChildCount); // Number of Children

            return entityProperties;
        }

        private List<contractsEvents.EntityDelta> LoggingCancelReservation(contractsReservations.Reservation reservationBeforeCancellation, contractsReservations.Reservation reservationAfterCancellation)
        {
            // Get all cancellation costs
            var reservationHelper = Resolve<IReservationHelperPOCO>();
            var cancellationCosts = reservationHelper.GetCancelationCosts(reservationBeforeCancellation);

            // CANCELLED ROOMS INFO-----------------------------------------------------------------
            var cancelledRoomsEntitiesDeltas = new List<contractsEvents.EntityDelta>();
            if (reservationBeforeCancellation.ReservationRooms != null && reservationAfterCancellation.ReservationRooms != null && reservationBeforeCancellation.ReservationRooms.Count == reservationAfterCancellation.ReservationRooms.Count)
                for (int iRoom = 0; iRoom < reservationAfterCancellation.ReservationRooms.Count; iRoom++)
                {
                    if (reservationBeforeCancellation.ReservationRooms[iRoom].Status != reservationAfterCancellation.ReservationRooms[iRoom].Status)
                    {
                        var cancelCost = cancellationCosts.FirstOrDefault(x => x.Number == reservationBeforeCancellation.ReservationRooms[iRoom].ReservationRoomNo);
                        decimal cost = (cancelCost != null) ? cancelCost.CancelationCosts : 0;

                        var cancelledRoomEntityDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.ReservationRooms)
                        {
                            EntityState = contractsEvents.EntityState.Modified,
                            EntityKey = reservationAfterCancellation.ReservationRooms[iRoom].UID,
                            EntityProperties = this.LoggingCancelReservationRoom(reservationBeforeCancellation.ReservationRooms[iRoom], reservationAfterCancellation.ReservationRooms[iRoom], cost)
                        };
                        cancelledRoomsEntitiesDeltas.Add(cancelledRoomEntityDelta);
                    }
                }

            return cancelledRoomsEntitiesDeltas;
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// RESTful implementation of the ListCancelReservationReasons operation.
        /// This operation list all Cancel Reservation Reasons - Cache
        /// </summary>
        /// <param name="request">A ListCancelReservationReasonRequest object containing the criteria to list cancel reservation reasons</param>
        /// <returns>A ListCancelReservationReasonResponse containing the results cancelreservationreason</returns>
        public ListCancelReservationReasonResponse ListCancelReservationReasons(ListCancelReservationReasonRequest request)
        {
            var response = new ListCancelReservationReasonResponse
            {
                RequestGuid = request.RequestGuid,
                RequestId = request.RequestId
            };

            try
            {
                List<CancelReservationReason> domainObjs;
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var repo = RepositoryFactory.GetCancelReservationReasonRepository(unitOfWork);
                    domainObjs = repo.GetAll().ToList();
                }

                if (request.LanguageUID > 0)
                {
                    foreach (var domainObj in domainObjs)
                    {
                        var language = domainObj.CancelReservationReasonLanguages.FirstOrDefault(x => x.Language_UID == request.LanguageUID);
                        if (language != null)
                            domainObj.Name = language.Name;
                    }
                }

                response.Result = domainObjs.Select(DomainToBusinessObjectTypeConverter.Convert).ToList();
                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                var error = new OB.Reservation.BL.Contracts.Responses.Error(ex);
                if (ex is BusinessLayerException exception)
                {
                    error.ErrorType = exception.ErrorType;
                    error.ErrorCode = exception.ErrorCode;
                }
                response.Errors.Add(error);
            }

            return response;
        }
    }
}

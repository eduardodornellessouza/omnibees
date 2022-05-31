using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Helper.Interfaces;
using OB.BL.Operations.Helper.Masking;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.BusinessObjects.ValidationRequests;
using OB.BL.Operations.Internal.TypeConverters;
using OB.DL.Common;
using OB.DL.Common.QueryResultObjects;
using OB.Domain;
using OB.Domain.Reservations;
using OB.Events.Contracts.Enums;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Logs;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static OB.Reservation.BL.Constants;
using contractsCRM = OB.Reservation.BL.Contracts.Data.CRM;
using contractsCRMOB = OB.BL.Contracts.Data.CRM;
using contractsEvents = OB.Events.Contracts;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using contractsRates = OB.BL.Contracts.Data.Rates;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservations = OB.Domain.Reservations;

namespace OB.BL.Operations.Impl
{
    public partial class ReservationManagerPOCO : BusinessPOCOBase, IReservationManagerPOCO
    {

        /// <summary>
        /// RESTful implementation of the InsertReservation operation.
        /// This operation inserts a reservation given its details.
        /// </summary>
        /// <param name="request">An InsertReservationRequest that contains all the Reservation details</param>
        /// <returns>An InsertReservationResponse that includes the errors, warnings or the insert reservation operation result.</returns>
        public InsertReservationResponse InsertReservation(InsertReservationRequest request)
        {
            request = request.MaskCC();
            var response = new InsertReservationResponse();
            try
            {
                Contract.Requires(request != null, "Request object instance is expected");
                response.RequestGuid = request.RequestGuid;
                response.RequestId = request.RequestId;

                GroupRule groupRule = null;

                if (!request.RuleType.HasValue)
                    throw Errors.RuleTypeIsRequired.ToBusinessLayerException();

                if (request.RuleType.HasValue)
                {
                    using (var unitOfWork = SessionFactory.GetUnitOfWork())
                    {
                        var rulesRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
                        groupRule = rulesRepo.GetGroupRule(RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request));
                    }
                }

                var result = InsertReservation(request, groupRule, true);

                response.Number = result.Number;
                response.Result = result.UID;
                response.ReservationStatus = result.ReservationStatus;
                response.Errors = result.Errors;
                response.Warnings = result.Warnings;

                response.Succeed();
            }
            catch (BusinessLayerException ex) // BusinessLayerException convert to known internal error 
            {
                response.Failed();
                //To contain new braspag erros logic.
                if (ex.Errors?.Any() == true)
                {
                    response.Errors.AddRange(ex.Errors);
                }
                else
                {
                    response.Errors.Add(new Error(ex.ErrorType, ex.ErrorCode, ex.Message));
                }
                response.Result = ex.ErrorCode;
                Logger.Warn(ex, "REQUEST:{0}", request.ToJSON());
            }
            catch (Exception ex) // Not expected Exception convert to Default internal error and refer RequestGuid 
            {
                response.Failed();
                response.Errors.Add(Errors.DefaultError.ToContractError(request.RequestGuid.ToString()));
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
            }
            return response;
        }

        public ReservationResult InsertReservation(InsertReservationRequest request, GroupRule groupRule, bool validate = true)
        {
            Stopwatch insertionWatch = new Stopwatch();
            insertionWatch.Start();

            bool usePaymentGateway = request.UsePaymentGateway ?? true;

            if (request.Reservation == null)
            {
                Logger.Error("Reservation detail must be provided");
                throw Errors.ReservationError.ToBusinessLayerException();
            }
            if (request.Reservation.UID != 0)
            {
                Logger.Error("Reservation to insert must be a new reservation. UID ({0}) is different from 0.", request.Reservation.UID);
                throw new ReservationAlreadyExistsException("Reservation to insert must be a new reservation!");
            }
            if (request.Guest == null)
            {
                Logger.Error("Guest cannot be null in a reservation");
                throw new BusinessLayerException("Guest cannot be null in a reservation");
            }

            var reservationResult = new ReservationResult();
            if (request.ReservationsAdditionalData == null)
                request.ReservationsAdditionalData = new contractsReservations.ReservationsAdditionalData();

            contractsCRMOB.Guest objguest = OtherConverter.Convert(request.Guest);

            RecoverUserPassword recoverUserPassword = new RecoverUserPassword();

            reservationResult.UID = 0;
            long jobId = 0;
            Task insertReservationPaymentOptionsTask = null;
            if (string.IsNullOrEmpty(request.TransactionId))
                request.TransactionId = Guid.NewGuid().ToString();

            var correlationId = Guid.NewGuid().ToString();
            var reservationValidator = Resolve<IReservationValidatorPOCO>();

            var reservationHelper = Resolve<IReservationHelperPOCO>();

            ReservationDataContext reservationContext = null;

            #region Verify and check the reservation language
            if (!request.Reservation.ReservationLanguageUsed_UID.HasValue)
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var propRepo = RepositoryFactory.GetOBPropertyRepository();
                    reservationHelper.SetLanguageToReservation(request.Reservation, request.Reservation.Property_UID, propRepo, request.RequestGuid);
                }
            }
            #endregion

            var throughputLimiterPOCO = this.Resolve<IThroughputLimiterManagerPOCO>();
            var throughputLimiter = throughputLimiterPOCO.GetThroughputLimiter(_INSERT_RESERVATION_THROUGHPUTLIMITER_KEY, _INSERT_RESERVATION_MAX_THREADS, _INSERT_RESERVATION_MAX_THREADS);

            domainReservations.Reservation domainReservation = null;
            contractsReservations.Reservation contractReservation = null;

            var checkAvailability = false;

            ReservationPaymentDetail reservationPaymentDetail = null;
            IEnumerable<ReservationPartialPaymentDetail> reservationPartialPaymentDetails = null;
            long? reservationAdditionalDataId = null;

            bool guestInserted = false;
            bool transactionSucceeded = false;
            bool sendTpiCreditLimitEmail = false;
            bool ignoreAvailability = request.IgnoreAvailability;
            bool invalidPaymentMethodType = false;

            try
            {                
                reservationValidator.InitialValidation(new InitialValidationRequest
                {
                    Reservation = request.Reservation,
                    ReservationRooms = request.ReservationRooms,
                    ReservationRoomDetails = request.ReservationRoomDetails,
                    RequestId = request.RequestId,
                    GroupRule = groupRule
                });

                throughputLimiter.Wait();
                try
                {
                    if (!guestInserted)
                    {
                        //Deals with waits for the Guest creation and update.
                        using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                        {
                            try
                            {
                                var reservationsRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                                var currencyUID = request.Reservation.ReservationBaseCurrency_UID ?? 1;
                                var rateIds = request.ReservationRooms != null && request.ReservationRooms.Any(x => x.Rate_UID.HasValue) ? request.ReservationRooms.Select(x => x.Rate_UID.GetValueOrDefault()).Distinct()
                                    : request.ReservationRoomDetails != null && request.ReservationRoomDetails.Any(x => x.Rate_UID.HasValue)
                                    ? request.ReservationRoomDetails.Select(x => x.Rate_UID.GetValueOrDefault()).Distinct() : new List<long>();
                                reservationContext = reservationsRepo.GetReservationContext(request.Reservation.Number,
                                                                                            request.Reservation.Channel_UID ?? 0,
                                                                                            request.Reservation.TPI_UID ?? 0,
                                                                                            request.Reservation.Company_UID ?? 0,
                                                                                            request.Reservation.Property_UID,
                                                                                            -1,
                                                                                            currencyUID,
                                                                                            request.Reservation.PaymentMethodType_UID ?? 0,
                                                                                            rateIds,
                                                                                            objguest.FirstName,
                                                                                            objguest.LastName,
                                                                                            objguest.Email,
                                                                                            objguest.UserName,
                                                                                            request.Reservation.ReservationLanguageUsed_UID,                                                                                            
                                                                                            requestGuid: request.RequestGuid);

                                reservationContext.ValidateAllotment = request.ValidateAllotment;

                                if (!reservationContext.IsChannelValid)
                                    throw Errors.InvalidChannel.ToBusinessLayerException((request.Reservation.Channel_UID ?? 0).ToString());

                                // Override IsOnRequest according the reservation status
                                if (!reservationContext.IsFromChannel || reservationContext.ChannelType != (long)Constants.ChannelType.Push)
                                    request.Reservation.IsOnRequest = request.Reservation.Status == (long)Constants.ReservationStatus.BookingOnRequest;

                                if (validate)
                                {
                                    var result = ValidateReservation(request.Reservation, request.ReservationPaymentDetail, request.ReservationRooms, reservationContext);
                                    invalidPaymentMethodType = (result == -3002);

                                    if (invalidPaymentMethodType)
                                    {
                                        bool ignorePaymentMethodTypeValidation = groupRule?.BusinessRules.HasFlag(BusinessRules.IgnorePaymentMethodTypeValidations) == true;
                                        if (ignorePaymentMethodTypeValidation)
                                            reservationResult.Warnings.Add(new Warning(nameof(Errors.InvalidPaymentMethod), (int)Errors.InvalidPaymentMethod, "Invalid Payment Method"));
                                        else
                                            throw Errors.OperatorInvalidPayment.ToBusinessLayerException();
                                    }
                                }

                                // Set PropertyBaseCurrencyExchangeRate to 1 if is null
                                request.Reservation.PropertyBaseCurrencyExchangeRate = request.Reservation.PropertyBaseCurrencyExchangeRate ?? 1;

                                objguest.Client_UID = reservationContext.Client_UID;
                                //Check whether Guest already exists
                                if (reservationContext.Guest_UID > 0)
                                {
                                    objguest.UID = reservationContext.Guest_UID.Value;
                                }


                                // Insert or update guest
                                if (objguest.UID > 0)
                                    UpdateGuest(objguest, request.GuestActivities, request.Reservation, request.ReservationRoomExtras);
                                else
                                {
                                    InsertGuest(objguest, request.GuestActivities, request.Reservation, request.ReservationRoomExtras);

                                    recoverUserPassword.PropertyId = objguest.PropertyId;
                                    recoverUserPassword.Email = objguest.Email;
                                    recoverUserPassword.LanguageId = objguest.LanguageId;
                                    recoverUserPassword.UserType = Constants.BookingEngineUserType.Guest;
                                    recoverUserPassword.ClientId = reservationContext.Client_UID;
                                }


                                unitOfWork.Save();

                                reservationContext.Guest_UID = objguest.UID;
                                guestInserted = true;
                            }
                            catch (BusinessLayerException ex)
                            {
                                Logger.Warn(ex);
                                throw;
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                                throw new BusinessLayerException(ex.Message, ex);
                            }
                        }
                    }

                    using (var scope = TransactionManager.BeginTransactionScope(OB.Domain.Reservations.Reservation.DomainScope))
                    {
                        using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                        {
                            contractsRates.PromotionalCode promotionalCode = null;
                            var reservationAdditionalDataRepo = RepositoryFactory.GetRepository<ReservationsAdditionalData>(unitOfWork);

                            #region PULL TPI RESERVATION

                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.PullTpiReservationCalculation))
                            {
                                if (request.Reservation.Company_UID.HasValue)
                                    request.Reservation.TPI_UID = request.Reservation.Company_UID;
                                if (request.Reservation.TPICompany_UID.HasValue)
                                    request.Reservation.TPI_UID = request.Reservation.TPICompany_UID;

                                var treatPullParameters = new TreatPullTpiReservationParameters
                                {
                                    AddicionalData = request.ReservationsAdditionalData,
                                    GroupRule = groupRule,
                                    Guest = objguest,
                                    Reservation = request.Reservation,
                                    ReservationContext = reservationContext,
                                    ReservationRoomChilds = request.ReservationRoomChilds,
                                    ReservationRoomDetails = request.ReservationRoomDetails,
                                    ReservationRoomExtras = request.ReservationRoomExtras,
                                    Rooms = request.ReservationRooms,
                                    IsInsert = true,
                                    Version = request.Version,
                                    PromotionalCode = promotionalCode,
                                    ReservationRequest = request,
                                    RequestGuid = request.RequestGuid,
                                    PaymentMethodTypeNotConfiguredInRate = invalidPaymentMethodType
                                };

                                reservationHelper.TreatPullTpiReservation(treatPullParameters);
                                promotionalCode = treatPullParameters.PromotionalCode;
                                request.ReservationRooms = treatPullParameters.Rooms; //objreservationroom lose the reference due to clone inside treatpulltpireservation 
                            }

                            #endregion

                            #region BE RESERVATION

                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.BEReservationCalculation))
                            {
                                if (request.Reservation.Company_UID.HasValue)
                                    request.Reservation.TPI_UID = request.Reservation.Company_UID;
                                if (request.Reservation.TPICompany_UID.HasValue)
                                    request.Reservation.TPI_UID = request.Reservation.TPICompany_UID;

                                // Forces this version to verify prices
                                if (request.Version == null)
                                    request.Version = new Reservation.BL.Contracts.Data.Version { Major = 0, Minor = 9, Patch = 45 };

                                var treatBeParams = new TreatBEReservationParameters
                                {
                                    GroupRule = groupRule,
                                    Guest = objguest,
                                    Reservation = request.Reservation,
                                    ReservationContext = reservationContext,
                                    ReservationRoomChilds = request.ReservationRoomChilds,
                                    ReservationRoomDetails = request.ReservationRoomDetails,
                                    ReservationRoomExtras = request.ReservationRoomExtras,
                                    Rooms = request.ReservationRooms,
                                    PromotionalCode = promotionalCode,
                                    ReservationRequest = request,
                                };
                                reservationHelper.TreatBeReservation(treatBeParams);
                                promotionalCode = treatBeParams.PromotionalCode;
                            }

                            #endregion

                            #region Validate LOYALTY LEVEL

                            var roomsToValidate = new List<OB.BL.Operations.Internal.BusinessObjects.ValidationRequests.LoyaltyRoomToValidate>();
                            if (request.ReservationRooms != null)
                            {
                                roomsToValidate = request.ReservationRooms.Where(x => x.LoyaltyLevel_UID.HasValue)
                                    .Select(
                                        x =>
                                            new OB.BL.Operations.Internal.BusinessObjects.ValidationRequests.
                                                LoyaltyRoomToValidate
                                            {
                                                CheckIn = x.DateFrom,
                                                CheckOut = x.DateTo,
                                                TotalAmount = x.ReservationRoomsPriceSum,
                                                LoyaltyLevel_UID = x.LoyaltyLevel_UID.Value
                                            }).ToList();
                            }

                            if (roomsToValidate.Count > 0)
                            {
                                var criteria = new LoyaltyLevelValidationCriteria
                                {
                                    Guest_UID = reservationContext.Guest_UID,
                                    IsForNightsRoomActive = reservationContext.IsForNightsRoomActive,
                                    IsForNightsRoomValue = reservationContext.IsForNightsRoomValue,
                                    IsForNumberOfReservationsActive =
                                        reservationContext.IsForNumberOfReservationsActive,
                                    IsForNumberOfReservationsValue =
                                        reservationContext.IsForNumberOfReservationsValue,
                                    IsForReservationActive = reservationContext.IsForReservationActive,
                                    IsForReservationRoomNightsValue =
                                        reservationContext.IsForReservationRoomNightsValue,
                                    IsForReservationValue = reservationContext.IsForReservationValue,
                                    IsForTotalReservationsActive = reservationContext.IsForTotalReservationsActive,
                                    IsForTotalReservationsValue = reservationContext.IsForTotalReservationsValue,
                                    IsLimitsForPeriodicityActive = reservationContext.IsLimitsForPeriodicityActive,
                                    LoyaltyLevel_UID = reservationContext.GuestLoyaltyLevel_UID,
                                    LoyaltyLevelLimitsPeriodicityValue =
                                        reservationContext.LoyaltyLevelLimitsPeriodicityValue,
                                    LoyaltyLevelLimitsPeriodicity_UID =
                                        reservationContext.LoyaltyLevelLimitsPeriodicity_UID,
                                    LoyaltyLevelBaseCurrency_UID = reservationContext.LoyaltyCurrency_Uid,
                                    RoomsToValidate = roomsToValidate,
                                    PropertyId = request.Reservation.Property_UID,
                                    PropertyCurrencyId = reservationContext.PropertyBaseCurrency_UID.Value
                                };

                                reservationValidator.ValidateGuestLoyaltyLevel(criteria);
                            }

                            #endregion Reservation Validator

                            // TODO: dmartins 2021-08-20 validar se o tipo de pagamento utilizado na reserva pode ser utilizado com parcelamento

                            List<Task> asyncTasks = new List<Task>();

                            //Insert Reservation Detail
                            this.InsertReservationDetail(reservationContext, out domainReservation, objguest.UID,
                                request.Reservation, request.ReservationRooms, groupRule);

                            //save salesman data comission
                            if (request.Reservation.TPI_UID.HasValue && request.Reservation.TPI_UID.Value > 0 && reservationContext.SalesmanCommission_UID > 0)
                            {
                                domainReservation.Salesman_UID = reservationContext.Salesman_UID;
                                domainReservation.SalesmanCommission = reservationContext.SalesmanBaseCommission;
                                domainReservation.SalesmanIsCommissionPercentage = reservationContext.SalesmanIsBaseCommissionPercentage;
                            }

                            reservationContext.ReservationUID = domainReservation.UID;

                            //Insert Reservation Room
                            Dictionary<ReservationRoom, bool> insertedRoomsAvailability = this.InsertReservationRooms(reservationContext, domainReservation, request.ReservationRooms,
                                request.ReservationRoomDetails, request.ReservationRoomExtras, request.ReservationRoomChilds, request.ReservationExtraSchedules,
                                request.Reservation.Status, request.HandleCancelationCost, request.HandleDepositCost,
                                request.ReservationsAdditionalData, correlationId, groupRule, ignoreAvailability: ignoreAvailability);

                            // Validates Status according Allotment and Inventory availability
                            if (!reservationContext.IsFromChannel || reservationContext.ChannelType != (long)Constants.ChannelType.Push)
                            {
                                // Some rooms are unavailable
                                if (!insertedRoomsAvailability.All(x => x.Value))
                                {
                                    // Some rooms are available and other rooms are unavailable
                                    if (insertedRoomsAvailability.GroupBy(x => x.Value).Count() > 1)
                                        throw new InvalidAllotmentException("Discrepancy between Rooms. There are AVAILABLE and UNAVAILABLE Rooms in this Reservation.");

                                    if (domainReservation.Status != (int)Constants.ReservationStatus.BookingOnRequest ||
                                        insertedRoomsAvailability.Any(x => x.Key.Status != (int)Constants.ReservationStatus.BookingOnRequest))
                                        throw new InvalidAllotmentException(reservationContext.IsOnRequestEnable ?
                                            "The Reservation Status and ReservationRooms Status must be BookingOnRequest to accept unavailable Rooms." :
                                            "Allotment not available!");

                                    if (!reservationContext.IsOnRequestEnable)
                                        throw new InvalidAllotmentException($"IsOnRequest is disable for Channel_UID = '{ reservationContext.Channel_UID }'.");
                                }

                                // All rooms are available
                                else if (domainReservation.Status <= 0 || domainReservation.Status == (long)Constants.ReservationStatus.BookingOnRequest)
                                {
                                    // Changes Reservation and ReservationRooms status to Booked
                                    domainReservation.IsOnRequest = false;
                                    domainReservation.Status = (long)Constants.ReservationStatus.Booked;
                                    request.Reservation.Status = (long)Constants.ReservationStatus.Booked;
                                    for (int i = 0; i < insertedRoomsAvailability.Count; i++)
                                    {
                                        insertedRoomsAvailability.ElementAt(i).Key.Status = (int)Constants.ReservationStatus.Booked;
                                        request.ReservationRooms.ElementAt(i).Status = (int)Constants.ReservationStatus.Booked;
                                    }
                                }
                            }

                            insertReservationPaymentOptionsTask = StartConcurrentWork(() =>
                            {
                                CreateReservationPaymentOptions(reservationContext,
                                    objguest,
                                    request,
                                    domainReservation,
                                    out reservationPaymentDetail,
                                    out reservationPartialPaymentDetails,
                                    groupRule,
                                    promotionalCode);
                            });
                            asyncTasks.Add(insertReservationPaymentOptionsTask);

                            // INCREMENT OPERATOR CREDIT USED
                            asyncTasks.Add(StartConcurrentWork(() =>
                            {
                                IncrementOperatorCreditUsed(domainReservation, request.ReservationPaymentDetail,
                                    domainReservation.TotalAmount, reservationContext);
                            }));

                            Task.WaitAll(asyncTasks.ToArray());

                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ConvertValuesToPropertyCurrency))
                                reservationHelper.ApplyCurrencyExchangeToReservationForConnectors(
                                    domainReservation, null, reservationContext, request.ReservationsAdditionalData);
                            //validate AND INCREMENT TPI Credit Limit
                            if (request.Reservation.PaymentMethodType_UID.HasValue && request.Reservation.TPI_UID.HasValue && request.Reservation.IsOnRequest != true
                                && reservationContext.ChannelHandleCredit.HasValue && !reservationContext.ChannelHandleCredit.Value)
                            {
                                sendTpiCreditLimitEmail = IncrementTPICreditUsed(request.Reservation.PaymentMethodType_UID.Value,
                                    request.Reservation.Property_UID, domainReservation.TotalAmount ?? 0,
                                    request.Reservation.TPI_UID.Value);
                            }

                            request.Reservation.ReservationRooms = request.ReservationRooms;
                            var errors = reservationValidator.ValidateReservation(request.Reservation,
                                new ValidateReservationRequest()
                                {
                                    ValidateGuarantee = request.ValidateGuarantee,
                                    GroupRule = groupRule
                                });

                            if (errors.Count > 0)
                            {
                                var error = errors.First();
                                throw new BusinessLayerException(error.Description, error.ErrorType, error.ErrorCode);
                            }

                            #region RESERVATION TRANSACTION STATUS AND HANGFIRE

                            Constants.ReservationTransactionStatus transactionStatus;
                            if (domainReservation.Status == (int)Constants.ReservationStatus.Booked)
                                transactionStatus = Constants.ReservationTransactionStatus.Commited;
                            else if (domainReservation.Status == (int)Constants.ReservationStatus.BookingOnRequest)
                                transactionStatus = Constants.ReservationTransactionStatus.CommitedOnRequest;
                            else
                                transactionStatus = Constants.ReservationTransactionStatus.Commited;

                            reservationHelper.InsertReservationTransaction(request.TransactionId, domainReservation.UID,
                                domainReservation.Number, domainReservation.Status,
                                transactionStatus, domainReservation.Channel_UID ?? 0, jobId, 0);

                            #endregion RESERVATION TRANSACTION STATUS AND HANGFIRE

                            #region CHECK AVAILABILITY

                            // If the property in the reservation needs to check availability (checkAvailability == true), the reservation can not be inserted in OB before knowing if the other channel manager has inventory available for the reservation!
                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.IsToPreCheckAvailability))
                            {
                                checkAvailability = CheckAvailability(request.RequestId, domainReservation);
                            }

                            #endregion CHECK AVAILABILITY

                            unitOfWork.Save();

                            // Fills ReservationAdditionalData with TPI information to be used in ReservationFilter
                            FillReservationAdditionDataTpiInfo(request.ReservationsAdditionalData, domainReservation);

                            #region Convert Reservation

                            contractReservation = DomainToBusinessObjectTypeConverter.Convert(domainReservation, new ListReservationRequest
                            {
                                IncludeReservationRooms = true,
                                IncludeReservationRoomDetails = true,
                                IncludeReservationPartialPaymentDetails = true,
                                IncludeReservationPaymentDetail = true
                            });
                            contractReservation.ReservationAdditionalData = request.ReservationsAdditionalData;

                            var reservationAddData = new ReservationsAdditionalData();
                            InsertReservationRoomAdditionalData(request.ReservationRooms, request.ReservationsAdditionalData, reservationContext, domainReservation, contractReservation, reservationAddData, recoverUserPassword);
                            var resAdditional = reservationAdditionalDataRepo.Add(reservationAddData);

                            #endregion Convert Reservation

                            #region RESERVATION FILTER

                            var reservationFilterRepo = this.RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                            var reservationFilter = new Domain.Reservations.ReservationFilter();
                            var parameters = new TreatReservationFiltersParameters()
                            {
                                NewReservation = contractReservation,
                                ReservationFilter = reservationFilter,
                                Guest = objguest,
                                ServiceName = ServiceName.InsertReservation
                            };
                            reservationHelper.TreatReservationFilter(parameters, unitOfWork);
                            reservationFilterRepo.Add(parameters.ReservationFilter);

                            #endregion

                            unitOfWork.Save();
                            reservationAdditionalDataId = resAdditional.UID;

                            #region HangFire

                            if (reservationContext.ChannelType == (int)Constants.ChannelType.Pull && domainReservation.Status == (int)Constants.ReservationStatus.BookingOnRequest)
                            {
                                var appRepo = RepositoryFactory.GetOBAppSettingRepository();
                                var secondsToDelayJob = double.Parse(appRepo.ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string>() { "CancelReservationOnRequestTimeToLive" } }).FirstOrDefault().Value);

                                jobId = reservationHelper.SetJobToCancelReservationOnRequestWithDelay(unitOfWork, new CancelReservationRequest
                                {
                                    ReservationUID = domainReservation.UID,
                                    OBUserType = 65,
                                    RuleType = OB.Reservation.BL.Constants.RuleType.Omnibees,
                                    ChannelId = domainReservation.Channel_UID ?? 0,
                                    TransactionId = request.TransactionId
                                }, secondsToDelayJob);
                            }
                            // Booking OnRequest from BE and BPag (Pending Payment Authorization)
                            else if (reservationContext.Channel_UID == 1 && (domainReservation.PaymentGatewayName == "BPag" && (domainReservation.Status == (int)Constants.ReservationStatus.BookingOnRequest && (domainReservation.IsOnRequest.HasValue && domainReservation.IsOnRequest.Value == true))))
                            {
                                jobId = reservationHelper.SetJobToCancelReservationOnRequestWithDelay(unitOfWork, new CancelReservationRequest
                                {
                                    ReservationUID = domainReservation.UID,
                                    OBUserType = 65,
                                    RuleType = OB.Reservation.BL.Constants.RuleType.Omnibees,
                                    ChannelId = domainReservation.Channel_UID ?? 0,
                                    TransactionId = request.TransactionId
                                }, 49 * 3600); // Allow at least 48hours for the BPag Bell Notification
                            }

                            #endregion

                            insertionWatch.Stop();
                            Logger.Info("Created Reservation {0} \tTook: {1}ms", reservationContext.ReservationUID, insertionWatch.ElapsedMilliseconds);

                            scope.Complete();

                            transactionSucceeded = true;
                        }
                    }
                }
                finally
                {
                    throughputLimiter.Release();
                }
            }
            catch (Exception ex)
            {
                transactionSucceeded = false;

                AggregateException aggregateEx = ex as AggregateException;
                if (aggregateEx != null)
                    ex = aggregateEx.GetBaseException();

                //Only send mail if it's not a PaymentGatewayException
                var paymentGatewayEx = ex as PaymentGatewayException;
                if (paymentGatewayEx == null ||
                    (paymentGatewayEx.ErrorType != Contracts.Responses.ErrorType.PaymentGatewayError
                    && paymentGatewayEx.ErrorType != Contracts.Responses.ErrorType.InvalidCreditCard))
                {
                    if (ex.IsDataLayerValidationException())
                        ex.LogDataLayerValidationErrors(Logger);

                    if ((ex is SqlException) || (ex is InvalidOperationException))
                    {
                        if (ex.InnerException != null)
                            Logger.Error(ex.InnerException);
                        else
                            Logger.Error(ex);
                    }

                    if (request.ReservationPaymentDetail != null)
                    {
                        request.ReservationPaymentDetail.CardNumber = null;
                        request.ReservationPaymentDetail.ExpirationDate = null;
                    }

                    Dictionary<string, object> arguments = new Dictionary<string, object>();
                    arguments.Add("objguest", objguest);
                    arguments.Add("objReservation", request.Reservation);
                    arguments.Add("objGuestActivity", request.GuestActivities);
                    arguments.Add("objReservationRoom", request.ReservationRooms);
                    arguments.Add("objReservationRoomDetail", request.ReservationRoomDetails);
                    arguments.Add("objReservationRoomExtras", request.ReservationRoomExtras);
                    arguments.Add("objReservationRoomChild", request.ReservationRoomChilds);
                    arguments.Add("objReservationPaymentDetail", request.ReservationPaymentDetail);
                    arguments.Add("objReservationExtraSchedule", request.ReservationExtraSchedules);
                    this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.Critical, arguments);
                }

                if (insertReservationPaymentOptionsTask != null)
                {
                    if (insertReservationPaymentOptionsTask.Status != TaskStatus.RanToCompletion && insertReservationPaymentOptionsTask.Status != TaskStatus.Faulted)
                        insertReservationPaymentOptionsTask.GetAwaiter().GetResult();

                    int result = 0;

                    if (!string.IsNullOrEmpty(request.Reservation.PaymentGatewayTransactionID) && request.Reservation.PaymentGatewayName != Constants.PaymentGateway.Paypal.ToString())
                        result = RefundPaymentGateway(domainReservation, (decimal)request.Reservation.TotalAmount, request.SkipInterestCalculation, request.ReservationRooms.Count);

                    if (result < 0 && request.Reservation.PaymentGatewayName == Constants.PaymentGateway.MaxiPago.ToString())
                    {
                        reservationResult.UID = -250; //internal error from omnibees side
                        return reservationResult;
                    }
                }

                if (ex is BusinessLayerException)
                    ExceptionDispatchInfo.Capture(ex).Throw();
                else throw;
            }

            //Insert Occupancy Alerts in PropertyQueue
            if (transactionSucceeded)
            {
                InsertReservationLogHandler(contractReservation, reservationContext, request.RequestId);

                QueueBackgroundWork(() =>
                {                    
                    InsertReservationAuxBackgroundWork(objguest, request, jobId, reservationContext, throughputLimiterPOCO, domainReservation, sendTpiCreditLimitEmail, recoverUserPassword, reservationAdditionalDataId: reservationAdditionalDataId, checkAvailability);
                });
            }

            reservationResult.UID = domainReservation.UID;
            reservationResult.Number = domainReservation.Number;
            reservationResult.ReservationStatus = (OB.Reservation.BL.Constants.ReservationStatus)(int)domainReservation.Status;
            return reservationResult;
        }
    

        void FillReservationAdditionDataTpiInfo(contractsReservations.ReservationsAdditionalData reservationsAdditionalData, domainReservations.Reservation domainReservation)
        {
            // Fill Tpi Information
            if (domainReservation.TPI_UID.HasValue)
            {
                var crmRepo = this.RepositoryFactory.GetOBCRMRepository();
                var tpi = crmRepo.GetTpiReservationAdditionalData(new Contracts.Requests.GetTpiReservationAdditionalDataRequest
                {
                    TpiUID = domainReservation.TPI_UID.Value
                });
                if (tpi != null)
                {
                    if (tpi.TpiType == (int)Constants.TPIType.Company)
                    {
                        reservationsAdditionalData.CompanyAddress = tpi.Address;
                        reservationsAdditionalData.CompanyCountry = tpi.Country;
                        reservationsAdditionalData.CompanyCurrency = tpi.Currency;

                        if (string.IsNullOrEmpty(reservationsAdditionalData.CompanyIATA))
                            reservationsAdditionalData.CompanyIATA = tpi.IATA;

                        reservationsAdditionalData.CompanyName = tpi.Name;
                        reservationsAdditionalData.CompanyPCC = tpi.CompanyCode;
                        reservationsAdditionalData.CompanyPhone = tpi.Phone;
                        reservationsAdditionalData.CompanyVATNumber = tpi.VATNumber;
                        reservationsAdditionalData.CompanyCountryUID = tpi.CountryUID;
                    }
                    else
                    {
                        reservationsAdditionalData.AgencyAddress = tpi.Address;
                        reservationsAdditionalData.AgencyCountry = tpi.Country;
                        reservationsAdditionalData.AgencyCurrency = tpi.Currency;

                        if (string.IsNullOrEmpty(reservationsAdditionalData.AgencyIATA))
                            reservationsAdditionalData.AgencyIATA = tpi.IATA;

                        reservationsAdditionalData.AgencyName = tpi.Name;
                        reservationsAdditionalData.AgencyPCC = tpi.PCC;
                        reservationsAdditionalData.AgencyPhone = tpi.Phone;
                        reservationsAdditionalData.AgencyVATNumber = tpi.VATNumber;
                        reservationsAdditionalData.AgencyCountryUID = tpi.CountryUID;
                    }

                    reservationsAdditionalData.TPIType = (OB.Reservation.BL.Constants.TPIType)tpi.TpiType;
                }
            }

            var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
            if (!String.IsNullOrEmpty(reservationsAdditionalData.CompanyCountryISO))
            {
                var result = propertyRepo.ListCountries(new Contracts.Requests.ListCountryRequest { CountryCodes = new List<string> { reservationsAdditionalData.CompanyCountryISO } }).FirstOrDefault();

                if (result != null)
                    reservationsAdditionalData.CompanyCountryUID = result.UID;
            }

            if (!String.IsNullOrEmpty(reservationsAdditionalData.AgencyCountryISO))
            {
                var result = propertyRepo.ListCountries(new Contracts.Requests.ListCountryRequest { CountryCodes = new List<string> { reservationsAdditionalData.AgencyCountryISO } }).FirstOrDefault();

                if (result != null)
                    reservationsAdditionalData.AgencyCountryUID = result.UID;
            }
        }        

        private void ReservationHistoryHandler(contractsReservations.Reservation contractReservation, InsertReservationRequest request, long jobId, long? reservationAdditionalDataId, ReservationDataContext reservationDataContext, contractsCRMOB.Guest objguest)
        {
            string hostAdress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostName : string.Empty;
            OB.Reservation.BL.Constants.ReservationTransactionStatus transactionStatus;

            if (contractReservation.Status == (int)Constants.ReservationStatus.Booked)
                transactionStatus = OB.Reservation.BL.Constants.ReservationTransactionStatus.Commited;
            else if (contractReservation.Status == (int)Constants.ReservationStatus.BookingOnRequest)
                transactionStatus = OB.Reservation.BL.Constants.ReservationTransactionStatus.CommitedOnRequest;
            else
                transactionStatus = OB.Reservation.BL.Constants.ReservationTransactionStatus.Commited;

            this.SaveReservationHistory(new ReservationLogRequest
            {
                ServiceName = OB.Reservation.BL.Contracts.Data.General.ServiceName.InsertReservation,
                Reservation = contractReservation,
                GuestActivity = request.GuestActivities,
                UserHostAddress = hostAdress,
                UserID = 0,
                ReservationTransactionId = request.TransactionId,
                ReservationTransactionState = transactionStatus,
                ReservationAdditionalDataUID = reservationAdditionalDataId,
                ReservationAdditionalData = request.ReservationsAdditionalData,
                HangfireId = jobId,
                BigPullAuthRequestor_UID = request.ReservationsAdditionalData.BigPullAuthRequestor_UID,
                ChannelName = reservationDataContext.ChannelName,
                propertyName = reservationDataContext.PropertyName,
                Guest = objguest
            });
        }

        private void InsertReservationLogHandler(contractsReservations.Reservation contractReservation, ReservationDataContext reservationDataContext, string requestId)
        {
            this.NewLogReservation(OB.Reservation.BL.Contracts.Data.General.ServiceName.InsertReservation,
                Constants.AdminUserUID, 
                Constants.AdminUserName,
                reservationDataContext.PropertyName,
                oldReservation: null, 
                contractReservation,
                channelName: reservationDataContext.ChannelName,
                propertyBaseCurrencyId: reservationDataContext.PropertyBaseCurrency_UID,
                requestId: requestId);
        }

        private void InsertReservationAuxBackgroundWork(contractsCRMOB.Guest objguest, InsertReservationRequest request, long jobId,
            ReservationDataContext reservationContext, IThroughputLimiterManagerPOCO throughputLimiterPOCO, domainReservations.Reservation domainReservation,
            bool sendTpiCreditLimitEmail, RecoverUserPassword recoverUserPassword, long? reservationAdditionalDataId, bool checkAvailability = false)
        {
            var objReservation = request.Reservation;
            var objReservationRoom = request.ReservationRooms;
            var reservationsAdditionalData = request.ReservationsAdditionalData;
            var transactionId = request.TransactionId;
            
            var logger = new Log.DefaultLogger("ReservationBackgroundWork");
            logger.Debug("Initiate InsertReservationAuxBackgroundWork: transactionId: {0} - ReservationUID: {1} - Jobid: {2} - RequestId: {3}", transactionId, domainReservation.UID, jobId, request.RequestId);
            
            try
            {
                if (sendTpiCreditLimitEmail)
                    InsertPropertyQueue(objReservation.Property_UID, (long)Constants.SystemEventsCode.CreditLimit, objReservation.TPI_UID ?? 0, false);

                //Insert Additional Data
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
                    
                    var converterParameters = GenerateListReservationRequest();
                    var contractReservation = ConvertReservationToContractWithLookups(converterParameters, new List<domainReservations.Reservation>() { domainReservation }).FirstOrDefault();

                    ReservationHistoryHandler(contractReservation, request, jobId, reservationAdditionalDataId, reservationContext, objguest);
                    #region Insert Reservation into PropertyQueue

                    if (objReservation.Status == (long)OB.Reservation.BL.Constants.ReservationStatus.BookingOnRequest)
                    {
                        InsertPropertyQueue(domainReservation.Property_UID, domainReservation.UID, domainReservation.Channel_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.OnRequestBooking, recoverUserPassword);
                    }
                    else
                    {
                        if (objReservation.Status == (long)OB.Reservation.BL.Constants.ReservationStatus.Booked)
                        {
                            var lang = propertyRepo.ListLanguages(new Contracts.Requests.ListLanguageRequest { UIDs = new List<long> { (long)domainReservation.ReservationLanguageUsed_UID } }).FirstOrDefault();
                            if (lang != null)
                            {
                                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang.Code);
                                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(lang.Code);
                            }

                            // SEND Reservation Room Extras Email
                            foreach (var reservationRoom in domainReservation.ReservationRooms)
                            {
                                foreach (var reservationRoomExtra in reservationRoom.ReservationRoomExtras)
                                {
                                    InsertExtraNotificationPropertyQueue(reservationRoomExtra, OB.BL.Resources.BillingMandatoryFields.lblNewExtra, (int)OB.Reservation.BL.Constants.SystemTemplatesCodes.ExtraNotificationEmail, (int)OB.Reservation.BL.Constants.SystemEventsCode.ExtraNotificationEmail);
                                }
                            }

                            InsertPropertyQueue(domainReservation.Property_UID, domainReservation.UID, domainReservation.Channel_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.NewBookingArrived, recoverUserPassword);

                            if (!reservationContext.IsFromChannel)
                            {
                                InsertPropertyQueue(domainReservation.Property_UID, domainReservation.UID, domainReservation.Channel_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.PreStay);
                                InsertPropertyQueue(domainReservation.Property_UID, domainReservation.UID, domainReservation.Channel_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.PostStay);
                            }
                        }

                        if (objReservation.Status == (long)OB.Reservation.BL.Constants.ReservationStatus.Modified)
                        {
                            InsertPropertyQueue(domainReservation.Property_UID, domainReservation.UID, domainReservation.Channel_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.BookingChanged);
                        }

                        if (objReservation.Status == (long)OB.Reservation.BL.Constants.ReservationStatus.Cancelled)
                        {
                            InsertPropertyQueue(domainReservation.Property_UID, domainReservation.UID, domainReservation.Channel_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.Bookingcancelled);
                            CancelPropertyQueueEvent(objReservation.Property_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.PreStay, domainReservation.UID, "Reservation was Cancelled!!!");
                            CancelPropertyQueueEvent(objReservation.Property_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.PostStay, domainReservation.UID, "Reservation was Cancelled!!!");
                            CancelPropertyQueueEvent(objReservation.Property_UID, (long)OB.Reservation.BL.Constants.SystemEventsCode.ExtraNotificationEmail, domainReservation.UID, "Reservation was Cancelled!!!");
                        }
                    }

                    #endregion Insert Reservation into PropertyQueue

                    //notify connectors async
                    if ((domainReservation.Status == (long)Constants.ReservationStatus.Booked ||
                        domainReservation.Status == (long)Constants.ReservationStatus.Cancelled ||
                        domainReservation.Status == (long)Constants.ReservationStatus.Modified) && !reservationsAdditionalData.AlreadySentToPMS && !checkAvailability)
                        this.NotifyConnectors("Reservation", reservationContext.ReservationUID, objReservation.Property_UID, (int)Constants.ConnectorEventOperations.Insert, null, checkAvailability, requestId: request.RequestId);//1

                    if (reservationContext.Inventories == null || reservationContext.Inventories.Count == 0)
                        return;

                    // Set Availability Control Email
                    this.InsOccupancyAlertsInPropertyQueue(domainReservation.UID, domainReservation.Property_UID, domainReservation.Channel_UID, true);

                    var throughputLimiterOccLevels = throughputLimiterPOCO.GetThroughputLimiter(_CLOSE_SALES_THROUGHPUTLIMITER_KEY,
                                _INSERT_RESERVATION_MAX_THREADS, _INSERT_RESERVATION_MAX_THREADS);

                    // US 762 OCC LEVELS
                    bool applyOccLevels = false;
                    if (ConfigurationManager.AppSettings["OccupancyLevelsEnabled"] != null && ConfigurationManager.AppSettings["OccupancyLevelsEnabled"].ToLowerInvariant() == "true")
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();

                        applyOccLevels = propertyRepo.ApplyOccupancyLevels(new Contracts.Requests.ApplyOccupancyLevelsRequest { InventoryToCheck = reservationContext.Inventories, PropertyUID = objReservation.Property_UID, ReservationUID = domainReservation.UID, Operation = "InsertReservation", GeneratedBy = 1 });
                        Logger.Debug(string.Format("-------- ApplyOccupancyLevels took: {0} ms", watch.ElapsedMilliseconds));
                        watch.Stop();
                    }

                    if (!applyOccLevels)
                    {
                        var stopWatch = new Stopwatch();
                        var random = new Random(DateTime.UtcNow.GetHashCode());

                        //TODO: number of retries should be configurable.
                        for (int retries = 0; retries < maxNumberOfRetriesForUpdateInventory; retries++)
                        {
                            // TODO: Review if it's really intended to give up on getting the semaphore after 1 minute.
                            //       Right now, this introduces a delay of up to 1 minute under contention, but it doesn't prevent contention.
                            var releaseSemaphore = throughputLimiterOccLevels.Wait(60000);
                            bool sucess = false;
                            try
                            {
                                stopWatch.Restart();

                                // Availabilty Control
                                this.CloseSales(contractReservation);

                                Logger.Debug(string.Format("-------- CloseSales retry:{0}   took: {1} ms", retries, stopWatch.ElapsedMilliseconds));
                                stopWatch.Stop();

                                sucess = true;
                            }
                            catch (Exception ex)
                            {
                                if ((retries + 1) < maxNumberOfRetriesForUpdateInventory)
                                {
                                    Logger.Error(ex, string.Format("A transaction deadlock or timeout occurred while processing CloseSales for reservation {0}. Attempt #{1}",
                                                                reservationContext.ReservationUID,
                                                                (retries + 1)));
                                    Thread.Sleep(random.Next(500, 3000));
                                }
                                else
                                {
                                    Logger.Error(ex, string.Format("A transaction deadlock or timeout occurred while processing CloseSales for reservation {0}. Given up after #{1} attempts",
                                                                reservationContext.ReservationUID,
                                                                (retries + 1)));
                                }
                            }
                            finally
                            {
                                if (releaseSemaphore)
                                    throughputLimiterOccLevels.Release();
                            }

                            if (sucess)
                                break;
                        }
                    }


                    #region SEND ALLOTMENT FOR CHANNELS

                    SendAllotmentAndInventoryForChannels(objReservation.Property_UID, objReservation.Channel_UID ?? 0, null, null, contractReservation.ReservationRooms, ServiceName.InsertReservation, reservationContext.Inventories);

                    #endregion SEND ALLOTMENT FOR CHANNELS

                }
                // Set TripAdvisor Review
                if (objReservationRoom != null && objReservationRoom.Any() && objReservationRoom.FirstOrDefault().DateTo.HasValue && objReservation.Channel_UID == (int)Constants.BookingEngineChannelId)
                {
                    SetTripAdvisorReview(domainReservation.Property_UID, domainReservation.UID, domainReservation.GuestEmail,
                                objReservationRoom.FirstOrDefault().DateTo.Value, objguest.Country_UID, (int)Constants.TripAdvisorReviewAction.Create, domainReservation.ReservationLanguageUsed_UID,
                                            objReservationRoom.FirstOrDefault().DateFrom);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, new Log.Messages.LogMessageBase {
                    MethodName = nameof(InsertReservationAuxBackgroundWork),
                    Description = "There were errors in the InsertReservation background thread",
                    RequestId = request.RequestId
                });

                Dictionary<string, object> arguments = new Dictionary<string, object>();
                arguments.Add("transactionId", transactionId);
                arguments.Add("Reservation", domainReservation);
                arguments.Add("Jobid", jobId);

                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments, null,
                    "Ocorreram erros nas tarefas de background do InsertReservation");
                throw;
            }

            logger.Debug("Finished InsertReservationAuxBackgroundWork: transactionId: {0} - ReservationUID: {1} - Jobid: {2} - RequestId: {3}", transactionId, domainReservation.UID, jobId, request.RequestId);
        }

        private void InsertReservationRoomAdditionalData(List<contractsReservations.ReservationRoom> objReservationRoom, contractsReservations.ReservationsAdditionalData reservationsAdditionalData,
            ReservationDataContext reservationContext, domainReservations.Reservation domainReservation, contractsReservations.Reservation contractReservation, ReservationsAdditionalData reservationAddData,
            RecoverUserPassword recoverUserPassword = null)
        {
            #region Convert Reservation
            contractReservation.ReservationAdditionalData = reservationsAdditionalData;

            #endregion Convert Reservation
            if (reservationsAdditionalData == null)
                reservationsAdditionalData = new contractsReservations.ReservationsAdditionalData();

            reservationAddData.Reservation_UID = domainReservation.UID;
            reservationAddData.IsFromNewInsert = reservationAddData.UID > 0;
            reservationAddData.ReservationDomain = !(string.IsNullOrEmpty(reservationsAdditionalData.ReservationDomain)) ? reservationsAdditionalData.ReservationDomain : null;
            reservationAddData.BookingEngineTemplate = !(string.IsNullOrEmpty(reservationsAdditionalData.BookingEngineTemplate)) ? reservationsAdditionalData.BookingEngineTemplate : null;
            reservationAddData.isDirectReservation = reservationsAdditionalData.IsDirectReservation ?? false;

            long? baseLanguage = reservationContext.PropertyBaseLanguage_UID.HasValue ? reservationContext.PropertyBaseLanguage_UID : default(long?);

            #region RESERVATION ADDITIONAL DATA

            if (objReservationRoom != null && objReservationRoom.Any())
            {
                string missedTranlation = "This information does not exist in your language...!";
                int index = 0;
                foreach (contractsReservations.ReservationRoom resRoom in objReservationRoom)
                {
                    var reservationRoom = domainReservation.ReservationRooms.ElementAt(index);
                    DepositPolicyQR1 depositPolicy;
                    CancellationPolicyQR1 cancellationPolicy;
                    OtherPolicyQR1 otherPolicy;

                    reservationContext.DepositPolicies.TryGetValue(resRoom.Rate_UID ?? 0, out depositPolicy);
                    reservationContext.CancellationPolicies.TryGetValue(resRoom.Rate_UID ?? 0, out cancellationPolicy);
                    reservationContext.OtherPolicies.TryGetValue(resRoom.Rate_UID ?? 0, out otherPolicy);

                    var cancellationPolicyContract = cancellationPolicy != null ? QueryResultObjectToBusinessObjectTypeConverter.Convert(cancellationPolicy) : null;
                    var otherPolicyContract = otherPolicy != null ? QueryResultObjectToBusinessObjectTypeConverter.Convert(otherPolicy) : null;

                    var isNew = false;
                    var resRoomAdditionalData = reservationsAdditionalData?.ReservationRoomList?.Where(x => x.ReservationRoom_UID == (index + 1)).FirstOrDefault() ?? null;
                    if (resRoomAdditionalData == null)
                    {
                        isNew = true;
                        resRoomAdditionalData = new contractsReservations.ReservationRoomAdditionalData();
                    }

                    resRoomAdditionalData.ReservationRoom_UID = reservationRoom.UID;
                    resRoomAdditionalData.ReservationRoomNo = reservationRoom.ReservationRoomNo;
                    resRoomAdditionalData.DepositPolicy_UID = resRoom.DepositPolicy_UID;
                    resRoomAdditionalData.IsDepositCostsAllowed = resRoom.IsDepositAllowed;
                    resRoomAdditionalData.DepositCosts = resRoom.DepositCosts;
                    resRoomAdditionalData.DepositDays = resRoom.DepositDays;
                    resRoomAdditionalData.DepositPolicy = resRoom.DepositPolicy;
                    resRoomAdditionalData.NrNights = resRoom.DepositNrNights;
                    resRoomAdditionalData.PaymentModel = resRoom.DepositPaymentModel;
                    resRoomAdditionalData.Value = resRoom.DepositPaymentModel != 3 ? resRoom.DepositValue : null;
                    resRoomAdditionalData.DepositInformation = resRoom.DepositInformation;
                    resRoomAdditionalData.DeposityGuaranteeType = resRoom.DeposityGuaranteeType;

                    #region DEPOSIT POLICY
                    if (depositPolicy != null)
                    {
                        if (baseLanguage == contractReservation.ReservationLanguageUsed_UID)
                            resRoomAdditionalData.DepositPolicyDescription = depositPolicy.DepositPolicyDescription;
                        else if (!string.IsNullOrEmpty(depositPolicy.TranslatedDepositPolicyDescription))
                            resRoomAdditionalData.DepositPolicyDescription = depositPolicy.TranslatedDepositPolicyDescription;
                        else
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.AppendLine(missedTranlation);
                            builder.AppendLine(depositPolicy.DepositPolicyDescription);
                            resRoomAdditionalData.DepositPolicyDescription = builder.ToString();
                        }
                    }

                    #endregion

                    #region CANCELLATION POLICY

                    if (cancellationPolicyContract != null)  //TODO VERIFY THIS!!!
                    {
                        if (string.IsNullOrEmpty(cancellationPolicyContract.TranslatedDescription) && baseLanguage != contractReservation.ReservationLanguageUsed_UID)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.AppendLine(missedTranlation);
                            builder.AppendLine(cancellationPolicyContract.Description);
                            cancellationPolicyContract.Description = builder.ToString();
                        }

                        if (string.IsNullOrEmpty(cancellationPolicyContract.TranslatedName) && baseLanguage != contractReservation.ReservationLanguageUsed_UID)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.AppendLine(missedTranlation);
                            builder.AppendLine(cancellationPolicyContract.Name);
                            cancellationPolicyContract.Name = builder.ToString();
                        }
                    }

                    resRoomAdditionalData.CancellationPolicy = cancellationPolicyContract;

                    #endregion

                    #region OTHER POLICY

                    if (otherPolicyContract != null)
                    {
                        if (string.IsNullOrEmpty(otherPolicyContract.TranslatedDescription) && baseLanguage != contractReservation.ReservationLanguageUsed_UID)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.AppendLine(missedTranlation);
                            builder.AppendLine(otherPolicyContract.OtherPolicy_Description);
                            otherPolicyContract.OtherPolicy_Description = builder.ToString();
                        }

                        if (string.IsNullOrEmpty(otherPolicyContract.TranslatedName) && baseLanguage != contractReservation.ReservationLanguageUsed_UID)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.AppendLine(missedTranlation);
                            builder.AppendLine(otherPolicyContract.OtherPolicy_Name);
                            otherPolicyContract.OtherPolicy_Name = builder.ToString();
                        }
                    }

                    resRoomAdditionalData.OtherPolicy = otherPolicyContract;

                    #endregion

                    if (resRoomAdditionalData != null && isNew)
                    {
                        if (reservationsAdditionalData?.ReservationRoomList == null)
                            reservationsAdditionalData.ReservationRoomList = new List<contractsReservations.ReservationRoomAdditionalData>();

                        reservationsAdditionalData.ReservationRoomList.Add(resRoomAdditionalData);
                    }

                    index++;
                }
            }

            #region TPI INFORMATION

            if (reservationsAdditionalData.ChannelPartnerID != null)
                reservationAddData.ChannelPartnerID = reservationsAdditionalData.ChannelPartnerID;

            if (!String.IsNullOrEmpty(reservationsAdditionalData.PartnerReservationNumber))
                reservationAddData.PartnerReservationNumber = reservationsAdditionalData.PartnerReservationNumber;

            #endregion

            #region LOYALTY LEVEL

            if (contractReservation.ReservationRooms.Any(x => x.LoyaltyLevel_UID != null))
            {
                reservationsAdditionalData.LoyaltyLevel = new contractsCRM.LoyaltyLevel();
                reservationsAdditionalData.LoyaltyLevel.UID = reservationContext.GuestLoyaltyLevel_UID.Value;
                reservationsAdditionalData.LoyaltyLevel.Description = reservationContext.LoyaltyLevelDescription;
                reservationsAdditionalData.LoyaltyLevel.Name = reservationContext.LoyaltyLevelName;

                if (reservationContext.LoyaltyLevelIsPercentage)
                    reservationsAdditionalData.LoyaltyLevel.DiscountValue = reservationContext.LoyaltyLevelDiscountValue;
                else
                    reservationsAdditionalData.LoyaltyLevel.DiscountValue = reservationContext.LoyaltyLevelDiscountValue * (domainReservation.PropertyBaseCurrencyExchangeRate ?? 1);

                reservationsAdditionalData.LoyaltyLevel.IsForNightsRoomActive = reservationContext.IsForNightsRoomActive;

                reservationsAdditionalData.LoyaltyLevel.IsForNightsRoomValue = reservationContext.IsForNightsRoomValue;
                reservationsAdditionalData.LoyaltyLevel.IsForNumberOfReservationsActive = reservationContext.IsForNumberOfReservationsActive;
                reservationsAdditionalData.LoyaltyLevel.IsForNumberOfReservationsValue = reservationContext.IsForNumberOfReservationsValue;
                reservationsAdditionalData.LoyaltyLevel.IsForReservationActive = reservationContext.IsForReservationActive;
                reservationsAdditionalData.LoyaltyLevel.IsForReservationRoomNightsValue = reservationContext.IsForReservationRoomNightsValue;
                reservationsAdditionalData.LoyaltyLevel.IsForReservationValue = reservationContext.IsForReservationValue;
                reservationsAdditionalData.LoyaltyLevel.IsForTotalReservationsActive = reservationContext.IsForTotalReservationsActive;
                reservationsAdditionalData.LoyaltyLevel.IsForTotalReservationsValue = reservationContext.IsForTotalReservationsValue;
                reservationsAdditionalData.LoyaltyLevel.IsLimitsForPeriodicityActive = reservationContext.IsLimitsForPeriodicityActive;

                reservationsAdditionalData.LoyaltyLevel.IsPercentage = reservationContext.LoyaltyLevelIsPercentage;
                reservationsAdditionalData.LoyaltyLevel.LoyaltyLevelLimitsPeriodicity_UID = reservationContext.LoyaltyLevelLimitsPeriodicity_UID;
                reservationsAdditionalData.LoyaltyLevel.LoyaltyLevelLimitsPeriodicityValue = reservationContext.LoyaltyLevelLimitsPeriodicityValue;
            }

            #endregion

            reservationAddData.BookerIsGenius = reservationsAdditionalData.BookerIsGenius;

            if (!string.IsNullOrWhiteSpace(recoverUserPassword?.Email))
            {
                reservationsAdditionalData.NewUserRecoverPasswordInfo = new contractsReservations.RecoverUserPassword
                {
                    Email = recoverUserPassword.Email,
                    LanguageId = recoverUserPassword.LanguageId,
                    UserType = (int)recoverUserPassword.UserType,
                };
            }

            reservationAddData.ReservationAdditionalDataJSON = Newtonsoft.Json.JsonConvert.SerializeObject(reservationsAdditionalData);
            contractReservation.ReservationAdditionalData = reservationsAdditionalData;

            reservationAddData.BigPullAuthRequestor_UID = reservationsAdditionalData.BigPullAuthRequestor_UID;
            reservationAddData.BigPullAuthOwner_UID = reservationsAdditionalData.BigPullAuthOwner_UID;

            #endregion
        }

        /// <summary>
        /// Set tripadvisor Review (Create/Update/Delete)
        /// </summary>
        /// <param name="propertyId">The property identifier.</param>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="recipientEmail">Guest Email</param>
        /// <param name="checkout">Required for Create and Update</param>
        /// <param name="countryId">The country identifier.</param>
        /// <param name="action">Constants.TripAdvisorReviewAction.(Create, Update, Delete)</param>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="checkin">The checkin.</param>
        protected virtual void SetTripAdvisorReview(long propertyId, long reservationId, string recipientEmail, DateTime checkout, long? countryId, int action, long? languageId, DateTime? checkin = null)
        {
            // Check if TripAdvisor Review is active
            var appRepo = this.RepositoryFactory.GetOBAppSettingRepository();
            var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

            var prop = appRepo.ListTripAdvisorConfiguration(new Contracts.Requests.ListTripAdvisorConfigRequest { PropertyUIDs = new List<long> { propertyId } }).FirstOrDefault();

            if (prop == null || !prop.IsCommentsActive) return;
            else
            {
                prop.CreatedDate = propertyRepo.ConvertToPropertyTimeZone(new Contracts.Requests.ConvertToPropertyTimeZoneRequest { PropertyId = propertyId, Dates = new List<DateTime> { prop.CreatedDate } }).First();
                prop.ModifiedDate = propertyRepo.ConvertToPropertyTimeZone(new Contracts.Requests.ConvertToPropertyTimeZoneRequest { PropertyId = propertyId, Dates = new List<DateTime> { prop.ModifiedDate } }).First();
            }

            var isTest = (ConfigurationManager.AppSettings["TripAdvisorTestEnable"] != null && ConfigurationManager.AppSettings["TripAdvisorTestEnable"].ToLowerInvariant() == "true") ? true : false;
            var countryCode = "US";
            var languageCode = "en_US";

            // Get Country Code
            if (countryId > 0)
            {
                var country = propertyRepo.ListCountries(new Contracts.Requests.ListCountryRequest { UIDs = new List<long> { countryId.HasValue ? countryId.Value : 0 } }).FirstOrDefault();
                if (country != null)
                    countryCode = country.CountryCode;
            }

            // Get language Code
            if (languageId > 0)
            {
                var lang = propertyRepo.ListLanguages(new Contracts.Requests.ListLanguageRequest { UIDs = new List<long> { languageId.HasValue ? languageId.Value : 0 } }).FirstOrDefault();
                if (lang != null)
                    languageCode = lang.Code.Substring(0, 2);

                if (languageCode == "en")
                    languageCode = "en_US";
            }

            switch (action)
            {
                case (int)Constants.TripAdvisorReviewAction.Create:
                    QueueBackgroundWork(async () =>
                    {
                        await appRepo.SendTripAdvisorReviewEmailAsync(new Contracts.Requests.TripAdvisorReviewAsyncRequest { IsTestEnable = isTest, PropertyId = propertyId, ReservationId = reservationId, RecipientEmail = recipientEmail, Checkout = checkout, CountryCode = countryCode, Checkin = checkin, LanguageCode = languageCode });
                    });
                    break;

                case (int)Constants.TripAdvisorReviewAction.Update:
                    QueueBackgroundWork(async () =>
                    {
                        await appRepo.AlterTripAdvisorReviewEmailAsync(new Contracts.Requests.TripAdvisorReviewAsyncRequest { IsTestEnable = isTest, ReservationId = reservationId, RecipientEmail = recipientEmail, Checkout = checkout, CountryCode = countryCode, Checkin = checkin, LanguageCode = languageCode });
                    });
                    break;

                case (int)Constants.TripAdvisorReviewAction.Delete:
                    QueueBackgroundWork(async () =>
                    {
                        await appRepo.EraseTripAdvisorReviewAsync(new Contracts.Requests.TripAdvisorReviewAsyncRequest { IsTestEnable = isTest, ReservationId = reservationId });
                    });
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        ///  Insert Reservation RoomDetail in ReservationRomm table
        /// </summary>
        /// <param name="objReservationroom"></param>
        /// <returns>A Dictionary which key is the created room and the value is a flag that indicates if room has Allotment and Inventory available.</returns>
        Dictionary<ReservationRoom, bool> InsertReservationRooms(ReservationDataContext reservationContext, domainReservations.Reservation res, List<contractsReservations.ReservationRoom> objReservationroom,
            List<contractsReservations.ReservationRoomDetail> objReservationRoomDetail, List<contractsReservations.ReservationRoomExtra> objReservationRoomExtras,
            List<contractsReservations.ReservationRoomChild> objReservationRoomChild, List<contractsReservations.ReservationRoomExtrasSchedule> extraSchedule,
            long ReservationStatus, bool handleCancelationCosts, bool handleDepositCosts, contractsReservations.ReservationsAdditionalData reservationsAdditionalData = null,
            string correlationId = null, GroupRule groupRule = null, bool concatDepositPolicyName = true, bool ignoreAvailability = false)
        {
            var roomIndexIsAvailable = new Dictionary<ReservationRoom, bool>();
            if (objReservationroom == null)
                return roomIndexIsAvailable;

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                var reservationsRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                var reservationRoomsRepo = this.RepositoryFactory.GetRepository<domainReservations.ReservationRoom>(unitOfWork);
                var rateBuyerGroupRepo = this.RepositoryFactory.GetOBRateBuyerGroupRepository();
                var crmRepo = this.RepositoryFactory.GetOBCRMRepository();
                var propertyRepo = RepositoryFactory.GetOBPropertyRepository();

                res = res == null ? reservationsRepo.Get(reservationContext.ReservationUID) : res;
                long? baseLanguage = reservationContext.PropertyBaseLanguage_UID.HasValue ? reservationContext.PropertyBaseLanguage_UID : default(long?);
                DateTime? checkInDate = objReservationroom.OrderBy(t => t.DateFrom).Select(t => t.DateFrom).FirstOrDefault();
                checkInDate = checkInDate.HasValue ? checkInDate.Value : DateTime.Now;
                // Get RateChannel Commission and Commission Type for every rate
                var commissions = new List<OB.BL.Contracts.Data.Rates.RateChannel>();
                decimal? tpiCommissions = null;
                bool isChannelCommission = true;

                //Get property availability type
                int propertyAvailabilityType = 0;
                var property = propertyRepo.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest { UIDs = new List<long> { res.Property_UID } }).FirstOrDefault();
                if (property != null)
                    propertyAvailabilityType = property.AvailabilityType;

                if (objReservationroom.Any(x => x.CommissionType == null || x.CommissionValue == null))
                {
                    if (res.Channel_UID != 1)
                    {
                        var rates = objReservationroom.Select(y => y.Rate_UID).Distinct();
                        commissions = rateBuyerGroupRepo.ListRatesChannels(new Contracts.Requests.ListRatesChannelsRequest
                        {
                            ExcludeDeleteds = true,
                            ChannelUIDs = new List<long> { res.Channel_UID.HasValue ? res.Channel_UID.Value : 0 },
                            RateUIDs = rates.Where(x => x.HasValue).Select(x => x.Value).ToList()
                        });
                    }
                    else
                    {
                        var tpi = crmRepo.ListTpiProperty(new Contracts.Requests.ListTPIPropertyRequest
                        {
                            TPI_Ids = new List<long> { res.TPI_UID.HasValue ? res.TPI_UID.Value : 0 },
                            PropertyIds = new List<long> { res.Property_UID },
                            IsActive = true,
                            ExcludeDeleteds = true
                        }).FirstOrDefault();

                        if (tpi != null)
                        {
                            tpiCommissions = tpi.Commission;
                            isChannelCommission = false;
                        }
                    }
                }

                string reservationNo = res.Number;
                reservationContext.Inventories = new List<contractsProperties.Inventory>();
                for (int i = 0; i < objReservationroom.Count; i++)
                {
                    contractsReservations.ReservationRoom rrDTO = objReservationroom[i];
                    long reservationRoomUID = rrDTO.UID;
                    domainReservations.ReservationRoom reservationRoom = null;

                    // Set commissions
                    if (isChannelCommission)
                    {
                        if (commissions.Any())
                        {
                            var tmpRc = commissions.FirstOrDefault(x => x.Rate_UID == rrDTO.Rate_UID);
                            if (tmpRc != null)
                            {
                                if (reservationContext.ChannelOperatorType != (int)OperatorsType.OperatorsHoteisNet)
                                {
                                    rrDTO.CommissionType = tmpRc.RateModel_UID;
                                    rrDTO.CommissionValue = tmpRc.Value;
                                }
                                else
                                {
                                    switch (rrDTO.CommissionType)
                                    {
                                        case 1:
                                            rrDTO.CommissionValue = tmpRc.Commission;
                                            break;
                                        case 2:
                                            rrDTO.CommissionValue = tmpRc.Markup;
                                            break;
                                        case 5:
                                            rrDTO.CommissionValue = tmpRc.Package;
                                            break;
                                        default:
                                            throw Errors.RequiredParameter.ToBusinessLayerException(nameof(rrDTO.CommissionType));
                                    }
                                }
                            }
                        }
                    }
                    else if (tpiCommissions != null)
                    {
                        rrDTO.CommissionType = 1;
                        rrDTO.CommissionValue = tpiCommissions;
                    }

                    if (objReservationRoomDetail != null && objReservationRoomDetail.Count > 0)
                    {
                        var existingReservationRoomDetails = objReservationRoomDetail.Where(t => t.ReservationRoom_UID == reservationRoomUID).ToList();
                        if (existingReservationRoomDetails.Count() > 0)
                        {
                            this.SetReservationRoomPolicyWithLanguageTranslation(baseLanguage, res.ReservationLanguageUsed_UID, ref rrDTO,
                                existingReservationRoomDetails, checkInDate.Value, res.ReservationBaseCurrency_UID, handleCancelationCosts, handleDepositCosts, res,
                            reservationsAdditionalData, reservationContext, concatDepositPolicyName);
                        }
                    }

                    //#ntelo # 1629
                    //check whether the reservation is from channels if so check for insert or update
                    if (!reservationContext.IsFromChannel || (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.GenerateReservationNumber)))
                    {
                        reservationRoom = new domainReservations.ReservationRoom();
                        reservationRoom.Reservation = res;
                        reservationRoom.Reservation_UID = res.UID;
                        res.ReservationRooms.Add(reservationRoom);
                        BusinessObjectToDomainTypeConverter.Map(rrDTO, reservationRoom, new string[] { domainReservations.ReservationRoom.PROP_NAME_UID });
                        reservationRoom.ReservationRoomNo = reservationNo + "/" + reservationRoomUID;

                        reservationRoom.CreatedDate = DateTime.UtcNow;
                        reservationRoomsRepo.Add(reservationRoom);
                    }
                    else
                    {
                        reservationRoom = reservationRoomsRepo.GetQuery(x => x.Reservation_UID == rrDTO.Reservation_UID && x.ReservationRoomNo == rrDTO.ReservationRoomNo)
                                            .FirstOrDefault();

                        if (reservationRoom != null)
                        {
                            BusinessObjectToDomainTypeConverter.Map(rrDTO, reservationRoom, new string[] { domainReservations.ReservationRoom.PROP_NAME_UID });
                            reservationRoom.ModifiedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            reservationRoom = new domainReservations.ReservationRoom();
                            reservationRoom.Reservation_UID = res.UID;
                            reservationRoom.Reservation = res;
                            res.ReservationRooms.Add(reservationRoom);

                            BusinessObjectToDomainTypeConverter.Map(rrDTO, reservationRoom, new string[] { domainReservations.ReservationRoom.PROP_NAME_UID });
                            reservationRoom.CreatedDate = DateTime.UtcNow;
                        }
                    }

                    if (objReservationroom.Count == 1)
                    {
                        reservationRoom.PmsRservationNumber = res.PmsRservationNumber;
                    }

                    reservationRoom.Reservation_UID = reservationContext.ReservationUID;

                    //Add TA/Coporate Discount details.
                    if (res.TPI_UID.HasValue && res.TPI_UID.Value > 0)
                    {
                        OB.BL.Contracts.Data.Rates.RateBuyerGroup objRBG = rateBuyerGroupRepo.GetRateBuyerGroup(rrDTO.Rate_UID ?? 0, res.TPI_UID ?? 0);

                        if (objRBG != null)
                        {
                            reservationRoom.TPIDiscountIsPercentage = objRBG.IsPercentage;
                            reservationRoom.TPIDiscountIsValueDecrease = objRBG.IsValueDecrease;
                            reservationRoom.TPIDiscountValue = objRBG.Value;
                        }
                    }

                    //Insert Reservation RoomDetail
                    bool roomIsAvailable = InsertReservationRoomDetail(reservationContext, reservationRoom, reservationRoomUID, objReservationRoomDetail,
                                                reservationRoom.RoomType_UID, ReservationStatus, groupRule, propertyAvailabilityType, false, correlationId, ignoreAvailability);
                    roomIndexIsAvailable.Add(reservationRoom, roomIsAvailable);

                    if (objReservationRoomExtras != null)
                    {
                        //Insert Reservation RoomExtras
                        InsertReservationRoomExtras(reservationRoom, reservationRoomUID, objReservationRoomExtras, extraSchedule, reservationRoom, false);
                    }

                    if (objReservationRoomChild != null)
                    {
                        //Insert Reservation RoomChild
                        InsertReservationRoomChild(reservationContext, reservationRoom, reservationRoomUID, objReservationRoomChild, false);
                    }
                }
            }

            return roomIndexIsAvailable;
        }


        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 5 April 2011
        /// Desc: Insert Reservation RoomDetail
        /// </summary>
        /// <param name="reservationContext">The reservation context.</param>
        /// <param name="reservationRoom">The reservation room.</param>
        /// <param name="reservationRoomUID">The reservation room uid.</param>
        /// <param name="objReservationRoomDetail">The object reservation room detail.</param>
        /// <param name="roomTypeId">The room type identifier.</param>
        /// <param name="reservationStatus">The reservation status.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="groupRule">The group rule.</param>
        /// <param name="save">if set to <c>true</c> [save].</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>
        ///   <c>true</c> if room has allotment and inventory available for all days.
        /// </returns>
        bool InsertReservationRoomDetail(ReservationDataContext reservationContext, domainReservations.ReservationRoom reservationRoom, long reservationRoomUID,
            List<contractsReservations.ReservationRoomDetail> objReservationRoomDetail, long? roomTypeId, long reservationStatus, GroupRule groupRule,
            int propertyAvailabilityType,
            bool save = true,
            string correlationId = null,
            bool ignoreAvailability = false)
        {
            bool roomIsAvailable = true;

            if (objReservationRoomDetail == null)
                return roomIsAvailable;

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork(domainReservations.ReservationRoomDetail.DomainScope, domainReservations.ReservationRoom.DomainScope))
            {
                var reservationRoomDetailRepo = this.RepositoryFactory.GetRepository<domainReservations.ReservationRoomDetail>(unitOfWork);
                List<contractsReservations.ReservationRoomDetail> objRoomDetail = objReservationRoomDetail.Where(t => t.ReservationRoom_UID == reservationRoomUID).ToList();
                IEnumerable<domainReservations.ReservationRoomDetail> domainRRDs = reservationContext.IsExistingReservation ? reservationRoom.ReservationRoomDetails.ToList() : Enumerable.Empty<domainReservations.ReservationRoomDetail>();
                reservationContext.ReservationRoomDetails = new List<domainReservations.ReservationRoomDetail>();
                var roomInventories = new List<contractsProperties.Inventory>();

                foreach (contractsReservations.ReservationRoomDetail obj in objRoomDetail)
                {
                    domainReservations.ReservationRoomDetail rrD = null;

                    //check if its from channel if so check for insert or update.
                    if (reservationContext.IsFromChannel)
                        rrD = domainRRDs.FirstOrDefault(x => x.Date.Year == obj.Date.Year && x.Date.Month == obj.Date.Month && x.Date.Day == obj.Date.Day);

                    //New Reservation for Room
                    if (rrD == null)
                    {
                        rrD = new domainReservations.ReservationRoomDetail();
                        rrD.ReservationRoom = reservationRoom;
                        reservationRoom.ReservationRoomDetails.Add(rrD);

                        obj.Rate_UID = obj.Rate_UID ?? reservationRoom.Rate_UID;

                        BusinessObjectToDomainTypeConverter.Map(obj, rrD, new string[] { domainReservations.ReservationRoomDetail.PROP_NAME_UID },
                            includeReservationRoomDetailAppliedIncentives: obj.ReservationRoomDetailsAppliedIncentives?.Any() == true,
                            includeReservationRoomDetailAppliedPromotionalCodes: obj.ReservationRoomDetailsAppliedPromotionalCode != null);

                        rrD.CreatedDate = DateTime.UtcNow;
                        reservationRoomDetailRepo.Add(rrD);

                        //update ntelo does not update inv if status==2 canceled
                        if (roomTypeId.HasValue && reservationRoom != null && reservationRoom.Status.HasValue && reservationRoom.Status != (long)OB.BL.Constants.ReservationStatus.Cancelled
                            && (obj.Rate_UID.HasValue || propertyAvailabilityType == (int)Contracts.Data.PropertyAvailabilityTypes.Inventory))
                        {
                            int rateAvailabilityType;
                            if (propertyAvailabilityType == (int)Contracts.Data.PropertyAvailabilityTypes.Inventory && !obj.Rate_UID.HasValue)
                            {
                                rateAvailabilityType = (int)Contracts.Data.RateAvailabilityTypes.Inventory;
                            }
                            else
                                reservationContext.RatesAvailabilityType.TryGetValue(obj.Rate_UID.Value, out rateAvailabilityType);

                            var inventory = this.CreateInventoryDetails(obj.Date, roomTypeId.Value, rateAvailabilityType);
                            roomInventories.Add(inventory);
                        }
                    }
                    else
                    {
                        rrD.Rate_UID = obj.Rate_UID ?? reservationRoom.Rate_UID;
                        rrD.ModifiedDate = rrD.ModifiedDate = DateTime.UtcNow;
                    }

                    reservationContext.ReservationRoomDetails.Add(rrD);
                }

                //Update RateRoomDetails AllotmentUsed
                //update ntelo does not update inv if status==2 canceled
                if (reservationRoom.Status.HasValue && reservationRoom.Status != (long)OB.BL.Constants.ReservationStatus.Cancelled)
                {
                    var parametersPerDay = new List<UpdateAllotmentAndInventoryDayParameters>();
                    var rrdDict = objRoomDetail.GroupBy(x => x.Date.Date).ToDictionary(k => k.Key, v => v.First());
                    foreach (var inventories in roomInventories.OrderBy(x => x.Date))
                    {
                        contractsReservations.ReservationRoomDetail rrd;
                        rrdDict.TryGetValue(inventories.Date.Date, out rrd);

                        var rrdId = rrd?.RateRoomDetails_UID ?? 0;
                        var rateId = reservationRoom.Rate_UID.HasValue ? reservationRoom.Rate_UID.Value : rrd?.Rate_UID ?? 0;

                        parametersPerDay.Add(new UpdateAllotmentAndInventoryDayParameters()
                        {
                            Day = inventories.Date.Date,
                            RateRoomDetailUID = rrdId,
                            RoomTypeUID = roomTypeId ?? 0,
                            RateAvailabilityType = reservationContext.RatesAvailabilityType.ContainsKey(rateId) ?
                                reservationContext.RatesAvailabilityType[rateId] : 0,
                            AddQty = 1
                        });
                    }

                    bool validate = groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ValidateAllotment);

                    // If reservation status is OnRequest then only validate if the room is available
                    if (reservationStatus == (long)OB.BL.Constants.ReservationStatus.BookingOnRequest && validate)
                    {
                        try
                        {
                            // If validate doesn't throw an exception then Room is available
                            ValidateUpdateReservationAllotmentAndInventory(groupRule, parametersPerDay, true, false, correlationId, ignoreAvailability: ignoreAvailability);
                        }
                        catch (InvalidAllotmentException)
                        {
                            roomIsAvailable = false;
                            if (!reservationContext.IsOnRequestEnable)
                                throw new InvalidAllotmentException($"IsOnRequest is disable for Channel_UID = '{ reservationContext.Channel_UID }'.");
                        }
                    }

                    if (roomIsAvailable)
                    {
                        try
                        {
                            // Try to update alloment and inventory
                            var updatedInventories = ValidateUpdateReservationAllotmentAndInventory(groupRule, parametersPerDay, validate, true, correlationId, ignoreAvailability: ignoreAvailability);

                            // Redefine inventories of ReservationContext to update Occupancy Levels later
                            reservationContext.Inventories.RemoveAll(x => x.RoomType_UID == (roomTypeId ?? 0));
                            reservationContext.Inventories.AddRange(updatedInventories);
                        }
                        catch (InvalidAllotmentException)
                        {
                            roomIsAvailable = false;
                        }
                    }
                }

                if (save)
                    unitOfWork.Save();
            }

            return roomIsAvailable;
        }

        public OB.BL.Contracts.Data.Properties.Inventory CreateInventoryDetails(DateTime dateTime, long roomTypeId, int rateAvailabilityType)
        {
            var inventory = new OB.BL.Contracts.Data.Properties.Inventory();
            inventory.QtyRoomOccupied = 1;
            inventory.RoomType_UID = roomTypeId;
            inventory.Date = dateTime.Date;

            if (rateAvailabilityType == (int)OB.BL.Contracts.Data.RateAvailabilityTypes.AllotmentInventoryImpact)
                inventory.QtyContractedAllotmentOccupied = 1;

            return inventory;
        }

        /// <summary>
        /// Insert Reservation Room Extras in ReservationRoomExtras table
        /// </summary>
        /// <param name="reservationRoom">The reservation room.</param>
        /// <param name="roomNo">The room no.</param>
        /// <param name="objReservationRoomExtra">The object reservation room extra.</param>
        /// <param name="objExtrasSchedule">The object extras schedule.</param>
        /// <param name="objResRoom">The object resource room.</param>
        /// <param name="save">if set to <c>true</c> [save].</param>
        /// <returns></returns>
        private int InsertReservationRoomExtras(domainReservations.ReservationRoom reservationRoom, long roomNo, List<contractsReservations.ReservationRoomExtra> objReservationRoomExtra, List<contractsReservations.ReservationRoomExtrasSchedule> objExtrasSchedule, domainReservations.ReservationRoom objResRoom, bool save = true)
        {
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork(ReservationRoom.DomainScope, ReservationRoomExtrasSchedule.DomainScope))
            {
                var extrasRepo = this.RepositoryFactory.GetOBExtrasRepository();
                var reservationRoomsRepo = this.RepositoryFactory.GetRepository<ReservationRoomExtra>(unitOfWork);
                var reservationRoomExtrasScheduleRepo = this.RepositoryFactory.GetRepository<ReservationRoomExtrasSchedule>(unitOfWork);

                List<contractsReservations.ReservationRoomExtra> objRoomExtra = objReservationRoomExtra.Where(t => t.UID == roomNo).ToList();
                foreach (contractsReservations.ReservationRoomExtra obj in objRoomExtra)
                {
                    if (obj.LinkedExtraByReservationRoomNumber != null)
                    {
                        if (string.IsNullOrWhiteSpace(reservationRoom?.ReservationRoomNo))
                            continue;

                        string number = reservationRoom?.ReservationRoomNo?.Split('/').Last();

                        if (string.IsNullOrWhiteSpace(number))
                            continue;

                        long.TryParse(number, out long roomNumber);

                        if (roomNumber <= 0)
                            continue;

                        if (!obj.LinkedExtraByReservationRoomNumber.Contains(roomNumber))
                            continue;
                    }

                    var newReservationRoomExtra = new ReservationRoomExtra();
                    BusinessObjectToDomainTypeConverter.Map(obj, newReservationRoomExtra, new string[] { ReservationRoomExtra.PROP_NAME_UID });
                    reservationRoom.ReservationRoomExtras.Add(newReservationRoomExtra);
                    newReservationRoomExtra.ReservationRoom = reservationRoom;
                    newReservationRoomExtra.CreatedDate = obj.CreatedDate = DateTime.UtcNow;

                    if (objResRoom != null && objResRoom.DateFrom.HasValue && objResRoom.DateTo.HasValue && objResRoom.Rate_UID.HasValue && obj.ExtraIncluded)
                    {
                        //ReservationRoomExtraAvailableDates.
                        List<OB.BL.Contracts.Data.Rates.RatesExtrasPeriod> lstrateExtraPeriods = extrasRepo.ListRatesExtrasPeriod(new Contracts.Requests.ListRatesExtrasPeriodRequest
                        {
                            DateFrom = objResRoom.DateFrom.Value,
                            DateTo = objResRoom.DateTo.Value,
                            RateUIDs = new List<long> { objResRoom.Rate_UID.Value },
                            ExtraUIDs = new List<long> { obj.Extra_UID }
                        });

                        if (lstrateExtraPeriods != null && lstrateExtraPeriods.Count > 0)
                        {
                            foreach (OB.BL.Contracts.Data.Rates.RatesExtrasPeriod item in lstrateExtraPeriods)
                            {
                                ReservationRoomExtrasAvailableDate objRREAD = new ReservationRoomExtrasAvailableDate();
                                objRREAD.DateFrom = item.DateFrom;
                                objRREAD.DateTo = item.DateTo;
                                newReservationRoomExtra.ReservationRoomExtrasAvailableDates.Add(objRREAD);
                            }
                        }
                    }

                    reservationRoomsRepo.Add(newReservationRoomExtra);

                    if (save)
                        unitOfWork.Save();

                    if (objExtrasSchedule != null)
                    {
                        List<contractsReservations.ReservationRoomExtrasSchedule> scheduleInfo = objExtrasSchedule.Where(t => t.UID == roomNo && t.ReservationRoomExtra_UID == obj.Extra_UID).ToList();

                        if (scheduleInfo != null && scheduleInfo.Count > 0)
                        {
                            foreach (contractsReservations.ReservationRoomExtrasSchedule extraSchedule in scheduleInfo)
                            {
                                var newReservationRoomExtraSchedule = new ReservationRoomExtrasSchedule();
                                newReservationRoomExtra.ReservationRoomExtrasSchedules.Add(newReservationRoomExtraSchedule);
                                newReservationRoomExtraSchedule.ReservationRoomExtra = newReservationRoomExtra;

                                BusinessObjectToDomainTypeConverter.Map(extraSchedule, newReservationRoomExtraSchedule, new string[] { ReservationRoomExtrasSchedule.PROP_NAME_UID });

                                newReservationRoomExtraSchedule.CreatedDate = DateTime.UtcNow;
                                newReservationRoomExtraSchedule.ModifiedDate = DateTime.UtcNow;
                                reservationRoomExtrasScheduleRepo.Add(newReservationRoomExtraSchedule);
                            }

                            if (save)
                                unitOfWork.Save();
                        }
                    }
                }
            }

            return 1;
        }

        /// <summary>
        /// Creates an email to notify the accountable people for the extras.
        /// Refactored to receive the Title, SystemTemplateCode and SystemEventCode.
        /// </summary>
        /// <param name="objExtra"></param>
        /// <param name="title"></param>
        /// <param name="systemTemplateCode"></param>
        /// <param name="systemEventCode"></param>
        private void InsertExtraNotificationPropertyQueue(domainReservations.ReservationRoomExtra objExtra, string title, int systemTemplateCode, int systemEventCode)
        {
            if (objExtra == null || objExtra.UID == 0)
                return;

            var extraRepo = this.RepositoryFactory.GetOBExtrasRepository();
            var propertyEventRepo = this.RepositoryFactory.GetOBPropertyEventsRepository();

            OB.BL.Contracts.Data.Rates.Extra extra = extraRepo.ListExtras(new Contracts.Requests.ListExtraRequest { UIDs = new List<long> { objExtra.Extra_UID } }).FirstOrDefault();

            if (extra != null && !string.IsNullOrEmpty(extra.NotificationEmail) && extra.Property_UID.HasValue)
            {
                string FromMail = ProjectGeneral.FromEmail;
                // INSERT OPTIMIZATION
                propertyEventRepo.PreparePropertyQueueToSendEmail(new Contracts.Requests.PreparePropertyQueueRequest
                {
                    PropertyUID = extra.Property_UID.Value,
                    MailBody = null,
                    MailFrom = FromMail,
                    MailTo = extra.NotificationEmail,
                    MailSubject = title,
                    TaskType_UID = objExtra.UID,
                    SystemTemplateCode = systemTemplateCode,
                    SystemEventCode = systemEventCode
                });
            }
        }

        /// <summary>
        ///  Insert ReservationRoomChild in ReservationRoomChild table
        /// </summary>
        /// <param name="reservationContext">The reservation context.</param>
        /// <param name="reservationRoom">The reservation room.</param>
        /// <param name="RoomNo">The room no.</param>
        /// <param name="objReservationRoomChild">The object reservation room child.</param>
        /// <param name="save">if set to <c>true</c> [save].</param>
        /// <returns></returns>
        private int InsertReservationRoomChild(ReservationDataContext reservationContext, domainReservations.ReservationRoom reservationRoom, long RoomNo, List<contractsReservations.ReservationRoomChild> objReservationRoomChild, bool save = true)
        {
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork(ReservationRoomChild.DomainScope))
            {
                var reservationRoomChildRepo = this.RepositoryFactory.GetRepository<ReservationRoomChild>(unitOfWork);
                //ntelo#1747
                List<contractsReservations.ReservationRoomChild> objRoomChild = objReservationRoomChild.Where(t => t.ReservationRoom_UID == RoomNo).ToList();

                foreach (contractsReservations.ReservationRoomChild obj in objRoomChild)
                {
                    ReservationRoomChild resC = null;
                    if (reservationContext.IsFromChannel)
                    {
                        resC = reservationRoom.ReservationRoomChilds.FirstOrDefault(x => x.ChildNo == obj.ChildNo);

                        if (resC != null)
                        {
                            resC.Age = obj.Age;
                        }
                    }

                    if (resC == null)
                    {
                        //TMOREIRA:Create domain object and map it from the DTO.
                        var newReservationRoomChild = new ReservationRoomChild();
                        reservationRoom.ReservationRoomChilds.Add(newReservationRoomChild);
                        newReservationRoomChild.ReservationRoom = reservationRoom;
                        BusinessObjectToDomainTypeConverter.Map(obj, newReservationRoomChild, new string[] { ReservationRoomChild.PROP_NAME_UID });
                        reservationRoomChildRepo.Add(newReservationRoomChild);
                    }
                }

                if (save)
                    unitOfWork.Save();
            }

            return 1;
        }

        private int InsertReservationDetail(ReservationDataContext reservationContext, out domainReservations.Reservation reservation, long GuestUID,
    contractsReservations.Reservation objReservationDetail, List<contractsReservations.ReservationRoom> objReservationRoom = null, GroupRule groupRule = null)
        {
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork(DomainScopes.Reservations))
            {
                var reservationsRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                reservation = null;
                bool isExisting = reservationContext.IsExistingReservation;

                if (isExisting)
                {
                    // email
                    ReservationAlreadyExistsException ex = new ReservationAlreadyExistsException("Reservation to insert must be a new reservation!");
                    Logger.Warn("Reservation to insert must be a new reservation. UID ({0}) is different from 0.", reservationContext.ReservationUID);
                    throw ex;
                }

                if (!reservationContext.IsFromChannel || (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.GenerateReservationNumber)))
                {
                    reservation = new domainReservations.Reservation();
                    BusinessObjectToDomainTypeConverter.Map(objReservationDetail, reservation);
                    reservation.UID = 0;

                    if (string.IsNullOrEmpty(objReservationDetail.Number))
                        reservation.Number = reservationsRepo.GenerateReservationNumber(objReservationDetail.Property_UID);
                    else
                        reservation.Number = objReservationDetail.Number;
                }
                else // Is from channel
                {
                    reservation = new domainReservations.Reservation();
                    BusinessObjectToDomainTypeConverter.Map(objReservationDetail, reservation);
                }

                reservation.Guest_UID = GuestUID;

                if (!((objReservationDetail.Status == (long)Constants.ReservationStatus.Modified || (objReservationDetail.UID > 0)) && isExisting))
                {
                    if (objReservationDetail.ReservationCurrencyExchangeRateDate == DateTime.MinValue)
                        objReservationDetail.ReservationCurrencyExchangeRateDate = DateTime.Now;
                }

                if (!isExisting)
                    reservationsRepo.Add(reservation);

                return UpdateReservationDetail(reservation, GuestUID, objReservationDetail);
            }
        }


        private int CreateReservationPaymentOptions(ReservationDataContext reservationContext,
                   contractsCRMOB.Guest objguest,
                   InsertReservationRequest request,
                   domainReservations.Reservation domainReservation,
                   out ReservationPaymentDetail reservationPaymentDetail,
                   out IEnumerable<ReservationPartialPaymentDetail> reservationPartialPaymentDetails,
                   GroupRule groupRule,
                   contractsRates.PromotionalCode promoCode = null,
                   string requestId = null)
        {
            reservationPaymentDetail = null;
            reservationPartialPaymentDetails = Enumerable.Empty<ReservationPartialPaymentDetail>();

            var securityRepo = this.RepositoryFactory.GetOBSecurityRepository();
            var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
            var securtityRepo = this.RepositoryFactory.GetOBSecurityRepository();
            var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

            // Gets property configuration to know if has tokenization active
            var propertySecConfig = propertyRepo.GetPropertySecurityConfiguration(new Contracts.Requests.ListPropertySecurityConfigurationRequest
            {
                PropertyUIDs = new List<long>
                {
                    request.Reservation.Property_UID
                }
            }).FirstOrDefault() ?? new contractsProperties.PropertySecurityConfiguration();
            bool paymentIsValid = true;

            // Make Payment with Payment Gateway
            if (request.Reservation.PaymentMethodType_UID.HasValue)
            {
                //Get Payment Method Type by Code.
                var objSelectedPaymentMethodType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest { UIDs = new List<long>() { request.Reservation.PaymentMethodType_UID.Value } }).FirstOrDefault();
                if (objSelectedPaymentMethodType != null)
                {
                    domainReservation.PaymentMethodType_UID = objSelectedPaymentMethodType.UID;
                    if ((request.ReservationPaymentDetail != null)
                        || (request.ReservationPaymentDetail != null && request.ReservationPaymentDetail.PaymentMethod_UID.HasValue && !string.IsNullOrEmpty(request.ReservationPaymentDetail.CardNumber)))
                    {
                        bool isValidReservationSourceToMakePayment = (!reservationContext.IsFromChannel || reservationContext.ChannelOperatorType != (int)Constants.OperatorsType.None)
                                && reservationContext.ChannelType != (int)Constants.ChannelType.Pull
                                && objSelectedPaymentMethodType.Code == (int)Constants.PaymentMethodTypesCode.CreditCard
                                && request.UsePaymentGateway.Value;

                        if (propertySecConfig.IsProtectedWithPaymentGateway == true || isValidReservationSourceToMakePayment)
                        {
                            string cardNumber = string.Empty;

                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.EncryptCreditCard))
                                cardNumber = request.ReservationPaymentDetail.CardNumber;
                            else
                            {
                                cardNumber = securityRepo.DecryptCreditCards(
                                    new Contracts.Requests.ListCreditCardRequest { CreditCards = new List<string> { request.ReservationPaymentDetail.CardNumber } })
                                    .FirstOrDefault();
                            }

                            if (!Helper.ValidationHelper.CardCheckDigit(cardNumber)
                                && !Helper.ValidationHelper.CardCheck(cardNumber, request.ReservationPaymentDetail.PaymentMethod_UID))
                            {
                                paymentIsValid = false;

                                // Abort Reservation if card isn't valid
                                if (isValidReservationSourceToMakePayment)
                                    throw new PaymentGatewayException("Error making payment on gateway- Error Code = " + -4000, Contracts.Responses.ErrorType.InvalidCreditCard, -4000);
                            }

                            else
                            {
                                var paymentReturn = this.MakePaymentWithPaymentGateway(objguest, request.ReservationPaymentDetail, domainReservation,
                                    request.SkipInterestCalculation,
                                    groupRule, promoCode, request.AntifraudDeviceFingerPrintId, request.RequestId, request.ReservationsAdditionalData?.Cookie);

                                if (paymentReturn < 0)
                                {
                                    paymentIsValid = false;

                                    // Abort Reservation if payment was not suceeded
                                    if (isValidReservationSourceToMakePayment)
                                        throw new PaymentGatewayException("Error making payment on gateway- Error Code = " + paymentReturn, Contracts.Responses.ErrorType.PaymentGatewayError, paymentReturn);
                                }
                            }
                        }
                    }
                }
            }

            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.EncryptCreditCard) && request.ReservationPaymentDetail != null)
            {
                var toEncrypt = new List<string>();
                List<string> encryptedList;

                if (!string.IsNullOrWhiteSpace(request.ReservationPaymentDetail.CardNumber))
                    toEncrypt.Add(request.ReservationPaymentDetail.CardNumber);

                if (!string.IsNullOrWhiteSpace(request.ReservationPaymentDetail.CVV))
                    toEncrypt.Add(request.ReservationPaymentDetail.CVV);

                if (toEncrypt.Count > 0)
                {
                    encryptedList = securtityRepo.EncryptCreditCards(new Contracts.Requests.ListCreditCardRequest { CreditCards = toEncrypt });

                    request.ReservationPaymentDetail.CardNumber = encryptedList.ElementAt(0);

                    if (!string.IsNullOrWhiteSpace(request.ReservationPaymentDetail.CVV))
                        request.ReservationPaymentDetail.CVV = encryptedList.ElementAt(1);
                }
            }

            //Insert Credit Card details
            //Insert ReservationPayment Detail
            reservationPaymentDetail = this.CreateReservationPaymentDetails(reservationContext, domainReservation, request.ReservationPaymentDetail, propertySecConfig, paymentIsValid);

            //map reservation contract
            request.Reservation.PaymentGatewayTransactionID = domainReservation.PaymentGatewayTransactionID;
            request.Reservation.PaymentGatewayName = domainReservation.PaymentGatewayName;
            request.Reservation.PaymentGatewayAutoGeneratedUID = domainReservation.PaymentGatewayAutoGeneratedUID;
            request.Reservation.PaymentGatewayOrderID = domainReservation.PaymentGatewayOrderID;

            //Created
            if (reservationPaymentDetail != null && reservationPaymentDetail.UID == 0)
            {
                domainReservation.ReservationPaymentDetails.Add(reservationPaymentDetail);
                reservationPaymentDetail.Reservation = domainReservation;
            }

            //check if partial payment option.
            if (!reservationContext.IsFromChannel && request.Reservation.IsPartialPayment.HasValue && request.Reservation.IsPartialPayment.Value && request.Reservation.NoOfInstallment.HasValue && request.Reservation.InstallmentAmount.HasValue)
            {
                //Insert ReservationPartialPayment Detail.
                reservationPartialPaymentDetails = this.CreateReservationPartialPaymentDetails(reservationContext.ReservationUID, request.Reservation.InterestRate, request.Reservation.InstallmentAmount, request.Reservation.NoOfInstallment.Value);

                foreach (var partialPayment in reservationPartialPaymentDetails)
                {
                    domainReservation.ReservationPartialPaymentDetails.Add(partialPayment);
                    partialPayment.Reservation = domainReservation;
                }
            }

            return 0;
        }

        private void InsertPropertyQueue(long propertyId, long reservationId, long? channelId, long systemEventCode, RecoverUserPassword recoverUserPassword, bool isOperator = false)
        {
            var propertyEventRepo = this.RepositoryFactory.GetOBPropertyEventsRepository();

            propertyEventRepo.InsertPropertyQueue(new Contracts.Requests.InsertPropertyQueueRequest
            {
                PropertyId = propertyId,
                ReservationId = reservationId,
                ChannelId = (channelId ?? 0),
                SystemEventCode = systemEventCode,
                IsOperator = isOperator,
                Email = recoverUserPassword != null ? recoverUserPassword.Email : string.Empty,
                LanguageId = recoverUserPassword != null ? recoverUserPassword.LanguageId : 0,
                IsNewGuestFromReservation = String.IsNullOrEmpty(recoverUserPassword.Email) ? false : true,
                ClientId = recoverUserPassword.ClientId
            });
        }

        private void InsertPropertyQueue(long propertyId, long reservationId, long? channelId, long systemEventCode, bool isOperator = false)
        {
            var propertyEventRepo = this.RepositoryFactory.GetOBPropertyEventsRepository();

            propertyEventRepo.InsertPropertyQueue(new Contracts.Requests.InsertPropertyQueueRequest
            {
                PropertyId = propertyId,
                ReservationId = reservationId,
                ChannelId = (channelId ?? 0),
                SystemEventCode = systemEventCode,
                IsOperator = isOperator,
            });
        }

        private void InsertPropertyQueue(long propertyUID, long systemEventCode, long taskTypeId, bool isOperator = false)
        {
            var propertyEventRepo = this.RepositoryFactory.GetOBPropertyEventsRepository();

            propertyEventRepo.InsertPropertyQueue(new Contracts.Requests.InsertPropertyQueueRequest
            {
                PropertyId = propertyUID,
                SystemEventCode = systemEventCode,
                TaskTypeId = taskTypeId,
                IsOperator = isOperator
            });
        }

        #region Insert Reservation
        private contractsEvents.EntityDelta LoggingInsertReservationAux(contractsReservations.Reservation reservation)
        {
            if (reservation == null)
                return new contractsEvents.EntityDelta();

            contractsEvents.EntityDelta entityDelta;
            var reservationInfoEntityProperties = new contractsEvents.Portable.Infrastructure.PropertyDictionary();

            // Created Date
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CreatedDate,
                reservation.CreatedDate);

            // Property UID
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PropertyUID,
                reservation.Property_UID);

            //ReservationBaseCurrency_UID
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ReservationBaseCurrency_UID,
                reservation.ReservationBaseCurrency_UID);

            //propertyName in notificationbase

            // ReservationNumber
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ReservationNumber,
                reservation.Number);

            // IsOnRequest
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsOnRequest,
                reservation.IsOnRequest);

            // IsReaded - always false in insert
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsReaded,
                false);

            // ModifiedDate
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ModifiedDate,
                reservation.ModifyDate);

            // GuestName
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.GuestName,
                string.Format("{0} {1}", reservation.GuestFirstName, reservation.GuestLastName));

            // NumberOfNights
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.NumberOfNights,
                reservation.Resume_NrNights);

            // AdultCount
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.AdultCount,
                reservation.Adults);

            // ChildCount
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ChildCount,
                reservation.Children);

            // NumberOfRooms
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.NumberOfRooms,
                reservation.NumberOfRooms);

            // TPI_UID
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.TPI_UID,
                reservation.TPI_UID);

            // PaymentType_UID
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PaymentType_UID,
                reservation.PaymentMethodType_UID);

            // Status
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Status,
                reservation.Status);

            // TotalReservationAmount
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.TotalReservationAmount,
                reservation.TotalAmount);


            if (reservation.ReservationAdditionalData != null)
            {
                //PO tpi pcc  * ver com o carlos *
                reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PO_PCC,
                    reservation.Company_UID.HasValue ? reservation.ReservationAdditionalData.CompanyPCC : reservation.ReservationAdditionalData.AgencyPCC);

                //PO tpi name * ver com o carlos *
                reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PO_TPIName,
                    reservation.Company_UID.HasValue ? reservation.ReservationAdditionalData.CompanyName : reservation.ReservationAdditionalData.AgencyName);

                //tpi name * ver com o carlos *
                reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.TPIName,
                    reservation.Company_UID.HasValue ? reservation.ReservationAdditionalData.CompanyName : reservation.ReservationAdditionalData.AgencyName);

                if (reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule != null && reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.Any())
                {
                    // ExternalTotalReservationAmount
                    reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ExternalTotalReservationAmount,
                        reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.Last().TotalAmount);

                    // ExternalCommissionValue
                    reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ExternalCommissionValue, reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.Last().TotalCommission); //falta commission no additionaldata

                    // ExternalIsPaid
                    reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ExternalIsPaid,
                    reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.Last().IsPaid);

                    // RepresentativeCurrencyExchangeRate
                    reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.RepresentativeCurrencyExchangeRate,
                        reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.Last().ExchangeRate);

                    // RepresentativeCurrencySymbol
                    reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CurrencySymbol,
                    reservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.Last().CurrencyUID);

                    //Partner UID
                    reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PartnerUid,
                    reservation.ReservationAdditionalData.ChannelPartnerID);

                    //Partner Reservation Number
                    reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PartnerReservationNumber,
                    reservation.ReservationAdditionalData.PartnerReservationNumber);

                }
            }

            // Channel_UID
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ChannelId,
            reservation.Channel_UID);

            //channel name
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ChannelName,
            reservation.Channel != null ? reservation.Channel.Name : string.Empty);

            //ispaid
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsPaid,
            reservation.IsPaid);

            // ReservationDate
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ReservationDate,
            reservation.Date);


            //reservation base exchange rate
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ReservationBaseExchangeRate,
            reservation.ReservationBaseCurrencyExchangeRate);

            //reservation base symbol
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ReservationBaseCurrencySymbol,
            reservation.ReservationBaseCurrencySymbol);

            //PropertyBaseCurrencyExchangeRate
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PropertyBaseCurrencyExchangeRate,
                reservation.PropertyBaseCurrencyExchangeRate);

            // CurrencySymbol - Property
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PropertyBaseCurrencySymbol,
            reservation.PropertyBaseCurrencySymbol);

            //Created by in notificationbase

            //Modified by
            reservationInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ModifiedBy,
            reservation.ModifyBy);

            // Create Reservation Delta
            var reservationInfoDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.Reservation)
            {
                EntityState = contractsEvents.EntityState.Created,
                EntityKey = reservation.UID,
                EntityType = EntityEnum.Reservation.ToString(),
                EntityProperties = reservationInfoEntityProperties
            };

            // Clean Room Delta
            entityDelta = contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(reservationInfoDelta);

            return entityDelta;
        }

        private List<contractsEvents.EntityDelta> LoggingInsertReservationRoom(contractsReservations.ReservationRoom reservationRoom)
        {
            if (reservationRoom == null)
                return new List<contractsEvents.EntityDelta>();

            var entityDeltas = new List<contractsEvents.EntityDelta>();
            var roomInfoEntityProperties = new contractsEvents.Portable.Infrastructure.PropertyDictionary();

            // Reservation UID
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Reservation_UID,
                reservationRoom.Reservation_UID);

            // Checkin Date
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DateFrom
                , (reservationRoom.DateFrom.HasValue) ? reservationRoom.DateFrom.Value.Date : new Nullable<DateTime>());

            // Checkout Date
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DateTo
                , (reservationRoom.DateTo.HasValue) ? reservationRoom.DateTo.Value.Date : new Nullable<DateTime>());

            //Aplly deposit policy
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ApplyDepositPolicy, reservationRoom.IsDepositAllowed);

            //Deposit Number Nights
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DepositNrNights, reservationRoom.DepositNrNights);

            //Deposit costs
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DepositCosts, reservationRoom.DepositCosts);

            // Create Room Delta
            var roomInfoDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.ReservationRooms)
            {
                EntityState = contractsEvents.EntityState.Created,
                EntityKey = reservationRoom.UID,
                EntityType = EntityEnum.ReservationRooms.ToString(),
                EntityProperties = roomInfoEntityProperties
            };

            // Clean Room Delta
            entityDeltas.Add(contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(roomInfoDelta));

            return entityDeltas;
        }

        private List<contractsEvents.EntityDelta> LoggingInsertReservation(contractsReservations.Reservation reservation)
        {
            var reservationEntitiesDeltas = new List<contractsEvents.EntityDelta>();

            //Reservation entity delta
            var reservationEntityDelta = this.LoggingInsertReservationAux(reservation);
            reservationEntitiesDeltas.Add(reservationEntityDelta);

            //ReservationRooms entity delta
            if (reservation.ReservationRooms != null)
            {
                foreach (var room in reservation.ReservationRooms)
                {
                    var roomEntityDelta = this.LoggingInsertReservationRoom(room);
                    reservationEntitiesDeltas.AddRange(roomEntityDelta);
                }
            }

            return reservationEntitiesDeltas;
        }
        #endregion

        public InsertLostReservationResponse InsertLostReservation(InsertLostReservationRequest request)
        {
            var response = new InsertLostReservationResponse(request);
            response.RequestId = request.RequestId;
            contractsReservations.LostReservation lostResevationToLog;

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var repository = RepositoryFactory.GetRepository<domainReservations.LostReservation>(unitOfWork);
                    var couchRepository = RepositoryFactory.GetRepository<domainReservations.LostReservationDetail>(unitOfWork);

                    domainReservations.LostReservation lostReservation;
                    lostReservation = BusinessObjectToDomainTypeConverter.Convert(request.LostReservation);
                    var lostReservationDetail = couchRepository.Add(BusinessObjectToDomainTypeConverter.Convert(request.LostReservationDetail));

                    lostReservation.CouchBaseId = lostReservationDetail.DocumentId;
                    repository.Add(lostReservation);

                    unitOfWork.Save();

                    lostResevationToLog = DomainToBusinessObjectTypeConverter.Convert(lostReservation);
                }

                response.Succeed();

                //Log
                var username = $"{request.LostReservationDetail?.Guest?.Email}";
                LogLostReservation(ServiceName.LostReservation, username, lostResevationToLog, request.RequestId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Errors.Add(new Error(ex));
                response.Failed();
            }

            return response;
        }
    }
}

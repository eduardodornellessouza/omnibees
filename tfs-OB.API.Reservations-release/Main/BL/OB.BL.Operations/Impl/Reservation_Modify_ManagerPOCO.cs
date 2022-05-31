using OB.Api.Core;
using OB.BL.Contracts.Data.PMS;
using OB.BL.Contracts.Requests;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Helper.Interfaces;
using OB.BL.Operations.Helper.Masking;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.BL.Operations.Internal.BusinessObjects.ValidationRequests;
using OB.BL.Operations.Internal.TypeConverters;
using OB.DL.Common;
using OB.DL.Common.QueryResultObjects;
using OB.Domain;
using OB.Domain.Reservations;
using OB.Events.Contracts.Enums;
using OB.Log.Messages;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Payments;
using OB.Reservation.BL.Contracts.Logs;
using OB.Reservation.BL.Contracts.Responses;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Web;
using static OB.Reservation.BL.Constants;
using contractsCRMOB = OB.BL.Contracts.Data.CRM;
using contractsEvents = OB.Events.Contracts;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using contractsRates = OB.BL.Contracts.Data.Rates;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservations = OB.Domain.Reservations;
using ListReservationRequest = OB.Reservation.BL.Contracts.Requests.ListReservationRequest;
using MarkReservationsAsViewedRequest = OB.Reservation.BL.Contracts.Requests.MarkReservationsAsViewedRequest;
using ModifyReservationRequest = OB.Reservation.BL.Contracts.Requests.ModifyReservationRequest;
using PMSHistoryRoomData = OB.Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData;
using ReservationBaseRequest = OB.Reservation.BL.Contracts.Requests.ReservationBaseRequest;
using UpdatePMSReservationNumberRequest = OB.Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest;
using UpdateReservationRequest = OB.Reservation.BL.Contracts.Requests.UpdateReservationRequest;
using UpdateReservationTransactionStatusRequest = OB.Reservation.BL.Contracts.Requests.UpdateReservationTransactionStatusRequest;

namespace OB.BL.Operations.Impl
{
    public partial class ReservationManagerPOCO : BusinessPOCOBase, IReservationManagerPOCO
    {
        public UpdateReservationResponse UpdateReservation(UpdateReservationRequest request)
        {
            var response = new UpdateReservationResponse();
            response.RequestId = request.RequestId;
            response.RequestGuid = request.RequestGuid;

            try
            {
                GroupRule groupRule = null;

                if (request.RuleType.HasValue)
                {
                    using (var unitOfWork = SessionFactory.GetUnitOfWork())
                    {
                        var rulesRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
                        groupRule = rulesRepo.GetGroupRule(RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request));
                    }
                }

                response.Result = UpdateReservation(request, groupRule);
                response.Succeed();
            }
            catch (BusinessLayerException ex) // BusinessLayerException convert to known internal error 
            {
                Logger.Warn(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new Error(ex.ErrorType, ex.ErrorCode, ex.Message));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new Error(ex));
            }
            return response;
        }

        /// <summary>
        /// RESTful implementation of the UpdatePMSReservationNumber operation.
        /// This operation updates the Reservation or ReservationRoom entities with the given PMSReservationNumbers in the request object.
        /// </summary>
        /// <param name="request">A UpdatePMSReservationNumberRequest object containing the mapping between Reservations/PMSNumbers and the ReservationRooms/PMSNumber</param>
        /// <returns>A UpdatePMSReservationNumberResponse containing the result status and/or list of found errors</returns>
        public UpdatePMSReservationNumberResponse UpdatePMSReservationNumber(UpdatePMSReservationNumberRequest request)
        {
            var response = new UpdatePMSReservationNumberResponse
            {
                RequestGuid = request.RequestGuid,
                RequestId = request.RequestId
            };

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");
                var createdDate = DateTime.UtcNow;

                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var pmsServicesRepo = this.RepositoryFactory.GetOBPMSRepository();
                    var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();
                    var reservationRoomRepo = this.RepositoryFactory.GetReservationRoomRepository(unitOfWork);
                    var reservationsRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                    

                    bool dirty = false;
                    bool dirtyReservationsPmsNumber = false;
                    bool dirtyReservationRoomsPmsNumber = false;
                    InsertPMSReservationsHistoryRequest insertPMSReservationsHistoryRequest = new InsertPMSReservationsHistoryRequest();
                    insertPMSReservationsHistoryRequest.PMSReservationsHistories = new List<PMSReservationsHistory>();

                    string pmsReservationNumber = string.Empty;
                    long propertyId = 0;
                    long clientId = 0;
                    long pmsUID = 0;

                    List<domainReservations.Reservation> reservations = null;
                    List<domainReservations.ReservationRoom> reservationRooms = null;

                    if (request.PMSReservationNumberByReservationUID != null &&
                        request.PMSReservationNumberByReservationUID.Count > 0)
                    {
                        var rUIDs = request.PMSReservationNumberByReservationUID.Select(x => x.Key);
                        reservations = reservationsRepo.GetQuery(x => rUIDs.Contains(x.UID)).ToList();
                        if (reservations == null || !reservations.Any())
                        {
                            response.Failed();
                            response.Errors.Add(new Error(ErrorType.InvalidRequest, (int)Errors.ReservationDoesNotExist, "There are no reservations that match the request"));
                            return response;
                        }

                        bool noReservationRooms = false;

                        if (request.PMSReservationNumberByReservationRoomUID.Count == 0)
                        {
                            propertyId = reservations.FirstOrDefault().Property_UID;
                            if (request.ClientId != null)
                            {
                                clientId = request.ClientId.Value;
                            }
                            else
                            {
                                var propertyList = propertyRepo.ListPropertiesLight(new ListPropertyRequest
                                {
                                    UIDs = new List<long> { propertyId }
                                });
                                if (propertyList != null)
                                    clientId = propertyList.SingleOrDefault().Client_UID;
                            }

                            if (request.PmsId != null)
                            {
                                pmsUID = request.PmsId.Value;
                            }
                            else
                            {
                                var pmsPropertyMapList =
                                    pmsServicesRepo.ListPMSServicesPropertyMappings(new ListPMSServicesPropertyMappingRequest
                                    {
                                        PropertyUIDs = new List<long> { propertyId },
                                        IsActive = true,
                                        IsDeleted = false
                                    }).Result;
                                long? pmsServiceUID = pmsPropertyMapList == null ? 0 : pmsPropertyMapList.FirstOrDefault().PMSService_UID;

                                var pmsList = pmsServicesRepo.ListPMSServices(new ListPMSServiceRequest
                                {
                                    PMSServiceUIDs = new List<long> { pmsServiceUID.Value }
                                }).Result;
                                if (pmsList != null)
                                    pmsUID = pmsList.FirstOrDefault().PMS_UID;
                            }
                            noReservationRooms = true;
                        }

                        var reservationUids = reservations.Select(r => r.UID);
                        reservationRooms = noReservationRooms ?
                            reservationRoomRepo.GetQuery(q => reservationUids.Contains(q.Reservation_UID)).ToList() : null;
                        
                        foreach (var reservation in reservations)
                        {
                            pmsReservationNumber = reservation.PmsRservationNumber = request.PMSReservationNumberByReservationUID[reservation.UID];
                            dirty = true;
                            dirtyReservationsPmsNumber = true;

                            if (noReservationRooms)
                            {
                                var rooms2 = reservationRooms.Where(r => r.Reservation_UID == reservation.UID).ToList();

                                PMSReservationsHistory pmsReservationsHistory = new PMSReservationsHistory();
                                pmsReservationsHistory.Reservation_UID = reservation.UID;
                                pmsReservationsHistory.PMSReservationNumber = pmsReservationNumber;
                                pmsReservationsHistory.Status = (int)reservation.Status;
                                pmsReservationsHistory.Date = createdDate;
                                pmsReservationsHistory.PMS_UID = pmsUID;
                                pmsReservationsHistory.Checkin = rooms2.Count > 0 ? (DateTime)rooms2.Min(q => q.DateFrom) : DateTime.UtcNow;
                                pmsReservationsHistory.Property_UID = propertyId;
                                pmsReservationsHistory.Client_UID = clientId;

                                insertPMSReservationsHistoryRequest.PMSReservationsHistories.Add(pmsReservationsHistory);
                            }
                        }
                    }

                    if (request.PMSReservationNumberByReservationRoomUID != null && request.PMSReservationNumberByReservationRoomUID.Count > 0)
                    {
                        var rrUIDs = request.PMSReservationNumberByReservationRoomUID.Select(x => x.Key);

                        reservationRooms = reservationRoomRepo.GetQuery(x => rrUIDs.Contains(x.UID)).ToList();
                        List<long> temp = reservationRooms.Select(x => x.Reservation_UID).Distinct().ToList();

                        reservations = reservationsRepo.GetQuery(x => temp.Contains(x.UID)).ToList();
                        if (reservations == null || !reservations.Any())
                        {
                            response.Failed();
                            response.Errors.Add(new Error(ErrorType.InvalidRequest, (int)Errors.ReservationDoesNotExist, "There are no reservations that match the request"));
                            return response;
                        }
                        propertyId = reservations.FirstOrDefault().Property_UID;
                        
                        if (request.ClientId != null)
                        {
                            clientId = request.ClientId.Value;
                        }
                        else
                        {
                            var propertyList = propertyRepo.ListPropertiesLight(new ListPropertyRequest
                            {
                                UIDs = new List<long> { propertyId }
                            });
                            clientId = propertyList != null ? propertyList.FirstOrDefault().Client_UID : 0;
                        }

                        if (request.PmsId != null)
                        {
                            pmsUID = request.PmsId.Value;
                        }
                        else
                        {
                            var pmsPropertyMapList = pmsServicesRepo.ListPMSServicesPropertyMappings(new ListPMSServicesPropertyMappingRequest
                            {
                                PropertyUIDs = new List<long> { propertyId },
                                IsActive = true,
                                IsDeleted = false
                            }).Result;
                            long? pmsServiceUID = pmsPropertyMapList != null ? pmsPropertyMapList.FirstOrDefault().PMSService_UID : 0;

                            var pmsList = pmsServicesRepo.ListPMSServices(new ListPMSServiceRequest
                            {
                                PMSServiceUIDs = new List<long> { pmsServiceUID.Value }
                            }).Result;
                            pmsUID = pmsList != null ? pmsList.FirstOrDefault().PMS_UID : 0;
                        }

                        List<Guid> insertedRoms = new List<Guid>();
                            
                        foreach (var reservationRoom in reservationRooms)
                        {
                            var pmsNumber = request.PMSReservationNumberByReservationRoomUID[reservationRoom.UID];
                            reservationRoom.PmsRservationNumber = pmsNumber.PMSNumber;
                                                       
                            if (!insertedRoms.Any(q => q == request.PMSReservationNumberByReservationRoomUID[reservationRoom.UID].Guid))
                            {
                                PMSReservationsHistory pmsReservationsHistory = new PMSReservationsHistory();
                                pmsReservationsHistory.Reservation_UID = reservationRoom.Reservation_UID;

                                if (!ValidReservationRoomsGuid(request.PMSReservationNumberByReservationRoomUID))
                                {
                                    pmsReservationsHistory.PMSReservationNumber = pmsNumber.PMSNumber;
                                    //pmsReservationsHistory.Status = (int)reservationRoom.Status;
                                }
                                else
                                {
                                    pmsReservationsHistory.PMSReservationNumber = string.IsNullOrWhiteSpace(pmsReservationNumber) ? "No PMS Number" : pmsReservationNumber;
                                    //pmsReservationsHistory.Status = (int)reservationRoom.Reservation.Status;
                                }
                                //NOTE: The Status is being written with the first reservation room status. We think this is incorrect cometimes, e.g:
                                // In case a reservation is being written as a unique transactions with several rooms, The status will be the status of
                                // the first room, which may be canceled, thus marking the whole reservation as "canceled".
                                // If someone can confirm this behaviour is incorrect, this line shoudl be removed and the two lines inside those ifs should
                                // also be removed. The unit tests will also need to change. Also, the checkin date should be investigated if is correct: 
                                // Checkin date may need to change to minimum reservationRoom "DateFrom" property, opposed to the first room "DateFrom".
                                pmsReservationsHistory.Status = (int)reservationRoom.Status;
                                pmsReservationsHistory.Date = createdDate;
                                pmsReservationsHistory.PMS_UID = pmsUID;
                                pmsReservationsHistory.Checkin = (DateTime)reservationRoom.DateFrom;
                                pmsReservationsHistory.Property_UID = propertyId;
                                pmsReservationsHistory.Client_UID = clientId;

                                insertPMSReservationsHistoryRequest.PMSReservationsHistories.Add(pmsReservationsHistory);

                                insertedRoms.Add(request.PMSReservationNumberByReservationRoomUID[reservationRoom.UID].Guid);
                            }
                            dirty = true;
                            dirtyReservationRoomsPmsNumber = true;
                        }
                    }

                    if (dirty)
                    {
                        unitOfWork.Save();
                     
                        UpdateExternalConfirmationNumber(reservations, reservationRooms, dirtyReservationsPmsNumber, dirtyReservationRoomsPmsNumber, pmsUID);

                    }


                    var responseInsertPMSHistory = pmsServicesRepo.InsertPMSReservationsHistory(insertPMSReservationsHistoryRequest);
                    if (responseInsertPMSHistory.Status == Contracts.Responses.Status.Success)
                        response.Succeed();
                    else
                        response.Failed();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.Warn(ex, new LogMessageBase
                {
                    RequestId = request.RequestId,
                    MethodName = nameof(UpdatePMSReservationNumber),
                    Code = nameof(Errors.DbUpdateConcurrencyException),
                });
                response.Failed();
                response.Errors.Add(new Error(ex));
            }
            catch (BusinessLayerException ex)
            { // BusinessLayerException convert to known internal error 
                Logger.Warn(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new Error(ex.ErrorType, ex.ErrorCode, ex.Message));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new Error(ex));
            }
            return response;
        }

        internal void UpdateExternalConfirmationNumber(List<domainReservations.Reservation> reservations, List<domainReservations.ReservationRoom> reservationRooms, bool dirtyReservationsPmsNumber, bool dirtyReservationRoomsPmsNumber, long pmsId)
        {
            string noPmsNumber = null;
            IEventSystemManagerPOCO eventSystemManager = Resolve<IEventSystemManagerPOCO>();
                       
            if (reservations == null || !reservations.Any())
                return;

            foreach (var reservation in reservations)
            {
                long? channelId = reservation.Channel_UID;
                long reservationId = reservation.UID;
                long propertyId = reservation.Property_UID;

                contractsEvents.Entities.Pms.PmsNumberSave pmsNumberSave = new contractsEvents.Entities.Pms.PmsNumberSave()
                {
                    ReservationNumber = reservation.Number,
                    ReservationId = reservationId,
                    PmsId = pmsId,
                    PropertyId = propertyId,
                    ChannelId = channelId.GetValueOrDefault(0),
                    ReservationRooms = new List<contractsEvents.Entities.Pms.ReservationRooms>(),
                    IsCancellation = reservation.Status == (int)Constants.ReservationStatus.Cancelled,
                };

                string reservationPmsNumber = noPmsNumber;

                if (dirtyReservationsPmsNumber)
                    reservationPmsNumber = reservation.PmsRservationNumber;
                else if (dirtyReservationRoomsPmsNumber && reservationRooms.Where(x => x.Reservation_UID == reservationId).GroupBy(x => x.PmsRservationNumber).Count() == 1)
                    reservationPmsNumber = reservationRooms.FirstOrDefault(x => x.Reservation_UID == reservationId).PmsRservationNumber;

                pmsNumberSave.PmsReservationNumber = reservationPmsNumber;

                if (reservationRooms != null && reservationRooms.Any(x => x.Reservation_UID == reservationId))
                {
                    foreach (var reservationRoom in reservationRooms.Where(x => x.Reservation_UID == reservationId))
                    {
                        string reservationRoomPmsNumber = noPmsNumber;
                        if (dirtyReservationRoomsPmsNumber)
                            reservationRoomPmsNumber = reservationRoom.PmsRservationNumber;
                        else if(!dirtyReservationRoomsPmsNumber && dirtyReservationsPmsNumber)
                            reservationRoomPmsNumber = reservation.PmsRservationNumber;

                        pmsNumberSave.ReservationRooms.Add(new contractsEvents.Entities.Pms.ReservationRooms
                        {
                            PropertyId = propertyId,
                            ReservationNumber = reservation?.Number,
                            PmsReservationNumber = reservationRoomPmsNumber,
                            ReservationRoomNo = reservationRoom.ReservationRoomNo,
                            IsCancellation = reservationRoom.Status == (int)Constants.ReservationStatus.Cancelled,
                        });
                    }
                }

                var notification = new contractsEvents.Messages.PmsNumberSaveNotification()
                {
                    EntityKey = reservation.UID,
                    RequestId = reservation?.Number,
                    Operation = contractsEvents.Operations.Create,
                    Deltas = new contractsEvents.DeltasList<contractsEvents.Entities.Pms.PmsNumberSave>()
                };

                notification.Deltas.Add(contractsEvents.EntityState.Created, reservation?.Number, pmsNumberSave);
                
                eventSystemManager.SendMessage(notification);

            }


        }

        public bool ValidReservationRoomsGuid(Dictionary<long, PMSHistoryRoomData> pmsNumberByReservationRoomUID)
        {
            bool result = false;

            List<Guid> tList = pmsNumberByReservationRoomUID.Values.Select(q => q.Guid).ToList();

            if (tList.Distinct().Count() == 1)
            {
                List<string> pmsNumberList = pmsNumberByReservationRoomUID.Values.Select(q => q.PMSNumber).ToList();
                if (pmsNumberList.Distinct().Count() != 1)
                {
                    result = true;
                }
            }

            return result;
        }



        private int UpdateReservationDetail(domainReservations.Reservation reservation, long GuestUID, contractsReservations.Reservation objReservationDetail)
        {
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork(DomainScopes.Reservations))
            {
                reservation.Guest_UID = GuestUID;

                bool isExisting = reservation.UID > 0;

                if (!isExisting)
                {
                    reservation.CreatedDate = DateTime.UtcNow;
                    reservation.ModifyDate = null;
                    reservation.InternalNotesHistory = DateTime.UtcNow + ";" + objReservationDetail.InternalNotes;
                }
                else
                {
                    reservation.ModifyDate = DateTime.UtcNow;

                    //This branch of code is only executed when the Caller of this Method is the UpdateReservation!!!!!
                    // US 8129 - Jorge Murta & Rafael Felix
                    if (reservation.InternalNotesHistory == null) reservation.InternalNotesHistory = String.Empty;

                    string internalNotesHistory =
                    (reservation.InternalNotesHistory == null ? String.Empty : reservation.InternalNotesHistory)
                    + "----------------------------------------------------;"
                    + DateTime.UtcNow + ";"
                    + objReservationDetail.InternalNotes;
                    reservation.InternalNotesHistory = internalNotesHistory;
                }

                reservation.BESpecialRequests1_UID = objReservationDetail.BESpecialRequests1_UID > 0 ? objReservationDetail.BESpecialRequests1_UID : null;
                reservation.BESpecialRequests2_UID = objReservationDetail.BESpecialRequests2_UID > 0 ? objReservationDetail.BESpecialRequests2_UID : null;
                reservation.BESpecialRequests3_UID = objReservationDetail.BESpecialRequests3_UID > 0 ? objReservationDetail.BESpecialRequests3_UID : null;
                reservation.BESpecialRequests4_UID = objReservationDetail.BESpecialRequests4_UID > 0 ? objReservationDetail.BESpecialRequests4_UID : null;
                reservation.TransferLocation_UID = objReservationDetail.TransferLocation_UID > 0 ? objReservationDetail.TransferLocation_UID : null;

                if (!isExisting)
                    unitOfWork.Save();
            }

            return 1;
        }

        [Obsolete("It is not used")]
        private int UpdateRateRoomDetails(List<long> rateRoomDetailUIDs, ref IDbTransaction scope, bool validateAllotment = false, bool decreaseAllotment = false, string correlationId = null)
        {
            int result = 0;
            if (rateRoomDetailUIDs.Count > 0)
                result = UpdateRateRoomDetailAllotments(rateRoomDetailUIDs.ToDictionary(k => k, v => decreaseAllotment ? -1 : 1), validateAllotment, correlationId);

            return result;
        }

        private long UpdateReservation(UpdateReservationRequest request, GroupRule groupRule)
        {
            request = request.MaskCC();
            if (request.Reservation == null || (request.Reservation.UID <= 0 && string.IsNullOrWhiteSpace(request.Reservation.Number)) || request.Reservation.Property_UID <= 0 || request.Reservation.Channel_UID <= 0)
            {
                string objReservation_UID = "0";
                string objReservation_Property_UID = "0";
                string objReservation_Channel_UID = "0";
                if (null != request.Reservation)
                {
                    objReservation_UID = request.Reservation.UID.ToString();
                    objReservation_Property_UID = request.Reservation.Property_UID.ToString();
                    objReservation_Channel_UID = request.Reservation.Channel_UID.HasValue ? request.Reservation.Channel_UID.ToString() : "0";
                }
                throw new Exception("objReservation has invalid values!"
                    + "\n objReservation_UID=" + objReservation_UID
                    + "\n objReservation_Property_UID=" + objReservation_Property_UID
                     + "\n objReservation_Channel_UID=" + objReservation_Channel_UID
                    );
            }

            var correlationId = Guid.NewGuid().ToString();
            List<ReservationRoom> oldReservationRooms = new List<ReservationRoom>();
            var reservationHelper = Resolve<IReservationHelperPOCO>();

            #region Verify and check the reservation language
            if (!request.Reservation.ReservationLanguageUsed_UID.HasValue)
            {
                var propRepo = RepositoryFactory.GetOBPropertyRepository();
                reservationHelper.SetLanguageToReservation(request.Reservation, request.Reservation.Property_UID, propRepo);
            }
            #endregion

            OB.Domain.Reservations.Reservation reservation = null;
            long oldReservationStatus = -1;
            ReservationPaymentDetail reservationPaymentDetail = null;
            IEnumerable<ReservationPartialPaymentDetail> reservationPartialPaymentDetails = Enumerable.Empty<ReservationPartialPaymentDetail>();
            ReservationDataContext reservationContext = null;
            List<ReservationRoomDetailRealAllotment> reservationRoomDetailsRealAllotment = new List<ReservationRoomDetailRealAllotment>();
            /* ----JG US 7863---- */
            List<ReservationRoomCustom> bacthList = new List<ReservationRoomCustom>();
            /* ------------------ */
            var currencyUID = request.Reservation.ReservationCurrency_UID ?? request.Reservation.ReservationBaseCurrency_UID ?? 1;
            // Set PropertyBaseCurrencyExchangeRate to 1 if is null
            request.Reservation.PropertyBaseCurrencyExchangeRate = request.Reservation.PropertyBaseCurrencyExchangeRate == null ? 1 : request.Reservation.PropertyBaseCurrencyExchangeRate;
            var throughputLimiterPOCO = this.Resolve<IThroughputLimiterManagerPOCO>();
            var throughputLimiter = throughputLimiterPOCO.GetThroughputLimiter(_UPDATE_RESERVATION_THROUGHPUTLIMITER_KEY, _UPDATE_RESERVATION_MAX_THREADS, _UPDATE_RESERVATION_MAX_THREADS);
            bool transactionSucceeded = false;
            bool ignoreAvailability = request.IgnoreAvailability;

            contractsCRMOB.Guest objguest = OtherConverter.Convert(request.Guest);

            contractsReservations.Reservation reservationBeforeUpdate = null;
            contractsReservations.Reservation reservationAfterUpdate = null;
            var converterParameters = new OB.Reservation.BL.Contracts.Requests.ListReservationRequest
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

            try
            {
                throughputLimiter.Wait();

                using (var scope = TransactionManager.BeginTransactionScope(OB.Domain.Reservations.Reservation.DomainScope))
                {
                    using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                    {
                        try
                        {
                            var reservationsRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                            var reservationRoomRepo = RepositoryFactory.GetRepository<ReservationRoom>(unitOfWork);
                            var reservationRoomDetailRepo = RepositoryFactory.GetRepository<ReservationRoomDetail>(unitOfWork);
                            var reservationRoomChildRepo = RepositoryFactory.GetRepository<ReservationRoomChild>(unitOfWork);
                            var reservationRoomExtraRepo = RepositoryFactory.GetRepository<ReservationRoomExtra>(unitOfWork);
                            var rrTaxPoliciesRepo = RepositoryFactory.GetRepository<ReservationRoomTaxPolicy>(unitOfWork);
                            var reservationRoomDetailAppliedIncentiveRepo = RepositoryFactory.GetRepository<ReservationRoomDetailsAppliedIncentive>(unitOfWork);
                            var resRoomExtraAvailableDatesRepo = RepositoryFactory.GetRepository<ReservationRoomExtrasAvailableDate>(unitOfWork);

                            var reservationValidator = this.Resolve<IReservationValidatorPOCO>();

                            if (request.Reservation.UID > 0)
                                reservation = reservationsRepo.FindByUIDEagerly(request.Reservation.UID);
                            else reservation = reservationsRepo.FindByReservationNumberAndChannelUID(request.Reservation.Number, request.Reservation.Channel_UID.GetValueOrDefault());

                            if (reservation == null)
                                throw Errors.ReservationDoesNotExist.ToBusinessLayerException();

                            // Contract for new log
                            reservationBeforeUpdate = ConvertReservationToContractWithLookups(converterParameters, new List<domainReservations.Reservation>() { reservation }).FirstOrDefault();

                            var existingReservationRooms = reservation.ReservationRooms.ToList();
                            oldReservationRooms = existingReservationRooms;
                            var existingReservationRoomDetails = reservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails);

                            var rateIds = request.ReservationRooms == null ?
                                    (existingReservationRooms.Any(x => x.Rate_UID.HasValue) ? existingReservationRooms.Select(x => x.Rate_UID.GetValueOrDefault()) : existingReservationRoomDetails != null && existingReservationRoomDetails.Any(x => x.Rate_UID.HasValue) ? existingReservationRoomDetails.Select(x => x.Rate_UID.GetValueOrDefault()) : new List<long>())
                                :
                                    (request.ReservationRooms.Any(x => x.Rate_UID.HasValue) ? request.ReservationRooms.Select(x => x.Rate_UID.GetValueOrDefault()).Distinct() : request.ReservationRoomDetails != null && request.ReservationRoomDetails.Any(x => x.Rate_UID.HasValue) ? request.ReservationRoomDetails.Select(x => x.Rate_UID.GetValueOrDefault()).Distinct() : new List<long>());

                            reservationContext = reservationsRepo.GetReservationContext(request.Reservation.Number,
                                                                                        request.Reservation.Channel_UID.GetValueOrDefault(),
                                                                                        request.Reservation.TPI_UID.GetValueOrDefault(),
                                                                                        request.Reservation.Company_UID.GetValueOrDefault(),
                                                                                        request.Reservation.Property_UID,
                                                                                        request.Reservation.UID,
                                                                                        currencyUID,//currencyUID,
                                                                                        request.Reservation.PaymentMethodType_UID ?? 0,
                                                                                        rateIds,
                                                                                        objguest.FirstName,
                                                                                        objguest.LastName,
                                                                                        objguest.Email,
                                                                                        objguest.UserName,
                                                                                        request.Reservation.ReservationLanguageUsed_UID);

                            // Map Reservation Object to Update
                            oldReservationStatus = reservation.Status;

                            BusinessObjectToDomainTypeConverter.Map(request.Reservation, reservation, new List<string>
                            {
                                OB.Domain.Reservations.Reservation.PROP_NAME_INTERNALNOTESHISTORY,
                                OB.Domain.Reservations.Reservation.PROP_NAME_CREATEDDATE,
                                OB.Domain.Reservations.Reservation.PROP_NAME_MODIFYDATE,
                            });

                            // set PaymentMethodType_UID to 1 if null
                            if (!request.Reservation.PaymentMethodType_UID.HasValue)
                                request.Reservation.PaymentMethodType_UID = 1;

                            //check gest as client uid
                            //Check whether Guest already exists
                            objguest.Client_UID = reservationContext.Client_UID;
                            //Check whether Guest already exists
                            if (reservationContext.Guest_UID > 0)
                            {
                                objguest.UID = reservationContext.Guest_UID.Value;
                            }

                            #region Insert or update guest
                            if (objguest.UID > 0)
                                UpdateGuest(objguest, request.GuestActivities, request.Reservation, request.ReservationRoomExtras);
                            else
                                InsertGuest(objguest, request.GuestActivities, request.Reservation, request.ReservationRoomExtras);
                            #endregion Insert or update guest

                            #region first delete all                    

                            if (request.ReservationRooms != null)
                            {
                                List<ReservationRoom> rr_del = reservation.ReservationRooms.ToList();
                                List<ReservationRoomDetail> rrd_del = reservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).ToList();
                                List<ReservationRoomChild> rrc_del = reservation.ReservationRooms.SelectMany(x => x.ReservationRoomChilds).ToList();
                                List<ReservationRoomExtra> rre_del = reservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList();
                                List<ReservationRoomTaxPolicy> rrt_del = reservation.ReservationRooms.SelectMany(x => x.ReservationRoomTaxPolicies).ToList();
                                List<ReservationRoomDetailsAppliedIncentive> rrdai_del = rrd_del.SelectMany(x => x.ReservationRoomDetailsAppliedIncentives).ToList();
                                List<ReservationRoomExtrasAvailableDate> rread_del = rre_del.SelectMany(x => x.ReservationRoomExtrasAvailableDates).ToList();
                                bool hasTaxPolicies = request.ReservationRooms.Where(x => x.ReservationRoomTaxPolicies != null).SelectMany(x => x.ReservationRoomTaxPolicies).Any();

                                foreach (ReservationRoom rr in rr_del)
                                {
                                    //decrement QtyRoomOccupied and JG US 7863
                                    if (rr.Status != (int)Constants.ReservationStatus.Cancelled && !string.IsNullOrEmpty(rr.PmsRservationNumber))
                                    {
                                        ReservationRoomCustom raux = new ReservationRoomCustom
                                        {
                                            UID = rr.UID,
                                            Reservation_UID = rr.Reservation_UID,
                                            ReservationRoomNo = rr.ReservationRoomNo,
                                            RoomName = rr.RoomName,
                                            RoomType_UID = rr.RoomType_UID == null ? 0 : (long)rr.RoomType_UID,
                                            DateFrom = rr.DateFrom,
                                            DateTo = rr.DateTo,
                                            SmokingPreferences = rr.SmokingPreferences,
                                            AdultCount = rr.AdultCount,
                                            ChildCount = rr.ChildCount,
                                            TotalTax = rr.TotalTax,
                                            Package_UID = rr.Package_UID,
                                            PmsReservationNumber = rr.PmsRservationNumber,
                                            guestName = rr.GuestName,
                                            ReservationNumber = rr.Reservation != null ? rr.Reservation.Number : reservation.Number,
                                            Status = true
                                        };

                                        if (rr_del.Count == 1 && String.IsNullOrEmpty(reservation.PmsRservationNumber))
                                        {
                                            reservation.PmsRservationNumber = rr.PmsRservationNumber;
                                        }

                                        bacthList.Add(raux);

                                        /* ------------------ */
                                    }

                                    if (request.ReservationRoomExtras != null)
                                    {
                                        List<ReservationRoomExtra> rre_list = rr.ReservationRoomExtras.ToList();
                                        foreach (ReservationRoomExtra rre in rre_list)
                                        {
                                            rre_del.Add(rre);
                                            rr.ReservationRoomExtras.Remove(rre);
                                            rre.ReservationRoom = null;
                                        }
                                    }

                                    if (request.ReservationRoomChilds != null)
                                    {
                                        List<ReservationRoomChild> rrc_list = rr.ReservationRoomChilds.ToList();
                                        foreach (ReservationRoomChild rrd2 in rrc_list)
                                        {
                                            rrc_del.Add(rrd2);

                                            rr.ReservationRoomChilds.Remove(rrd2);
                                            rrd2.ReservationRoom = null;
                                        }
                                    }

                                    if (hasTaxPolicies && rr?.ReservationRoomTaxPolicies.Count > 0)
                                        rrTaxPoliciesRepo.Delete(rr.ReservationRoomTaxPolicies);

                                    rr.Reservation = null;
                                    reservation.ReservationRooms.Remove(rr);
                                }

                                // Decrement Inventory and Alloment for old rooms
                                if (reservation == null || (reservation != null && reservation.Status != (int)Constants.ReservationStatus.Cancelled))
                                {
                                    var ratesRepo = RepositoryFactory.GetOBRateRepository();
                                    var decrementAllotmentAndInventoryParams = new List<UpdateAllotmentAndInventoryDayParameters>();
                                    var notCancelledRooms = reservationBeforeUpdate.ReservationRooms.Where(x => x.Status != (int)Constants.ReservationStatus.Cancelled).ToList();

                                    // Get availability type of reservation rooms rates
                                    var availabilityrequest = new OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest
                                    {
                                        RatesUIDs = notCancelledRooms.Where(x => x.Rate_UID.HasValue).Select(x => x.Rate_UID.Value).Distinct().ToList()
                                    };
                                    Dictionary<long, int> rateAvailabilityTypes = ratesRepo.ListRatesAvailablityType(availabilityrequest);

                                    // Group by RoomType and AvailabilityType
                                    foreach (var room in notCancelledRooms.GroupBy(x => new { x.RoomType_UID, AvailabilityType = x.Rate_UID.HasValue ? rateAvailabilityTypes[x.Rate_UID.Value] : 0 }))
                                    {
                                        var rrds = room.SelectMany(x => x.ReservationRoomDetails).ToList();
                                        foreach (var decrementDay in rrds.GroupBy(x => x.Date.Date))
                                        {
                                            var rrd = decrementDay.First();
                                            decrementAllotmentAndInventoryParams.Add(new UpdateAllotmentAndInventoryDayParameters()
                                            {
                                                Day = decrementDay.Key,
                                                RoomTypeUID = room.Key.RoomType_UID ?? 0,
                                                AddQty = -decrementDay.Count(),
                                                RateRoomDetailUID = rrd.RateRoomDetails_UID ?? 0,
                                                RateAvailabilityType = room.Key.AvailabilityType
                                            });
                                        }
                                    }

                                    // Updates Allotment and Inventory
                                    var inventories = ValidateUpdateReservationAllotmentAndInventory(groupRule, decrementAllotmentAndInventoryParams, false, true, correlationId, true, ignoreAvailability: ignoreAvailability);
                                    if (reservationContext.Inventories == null)
                                        reservationContext.Inventories = new List<contractsProperties.Inventory>();
                                    else
                                        reservationContext.Inventories.RemoveAll(x => notCancelledRooms.Select(rr => rr.RoomType_UID).Contains(x.RoomType_UID));
                                    reservationContext.Inventories.AddRange(inventories);

                                    // Add to a list for notify channels about real allotment in a background thread
                                    reservationRoomDetailsRealAllotment.AddRange(DecrementRealAllotmentRateRoomDetailsByReservationRoom(rrd_del));
                                }

                                foreach (ReservationRoomDetail rrd1 in rrd_del)
                                {
                                    if (rrd1.ReservationRoom != null)
                                        rrd1.ReservationRoom.ReservationRoomDetails.Remove(rrd1);
                                    rrd1.ReservationRoom = null;
                                }

                                reservationRoomRepo.Delete(rr_del);
                                reservationRoomChildRepo.Delete(rrc_del);
                                reservationRoomDetailRepo.Delete(rrd_del);
                                reservationRoomExtraRepo.Delete(rre_del);
                                reservationRoomDetailAppliedIncentiveRepo.Delete(rrdai_del);
                                resRoomExtraAvailableDatesRepo.Delete(rread_del);
                            }

                            unitOfWork.Save();

                            #endregion first delete all

                            // remove credit used to add later
                            this.DecrementOperatorCreditUsed(reservation);

                            //Insert Reservation Detail
                            this.UpdateReservationDetail(reservation, objguest.UID, request.Reservation);

                            // Pull cannot concat policy name with the policy description
                            bool ignoreDepositPolicyConcat = groupRule?.BusinessRules.HasFlag(BusinessRules.IgnoreDepositPolicyConcat) ?? false;

                            //Insert Reservation Room
                            var roomsIsAvailableList = this.InsertReservationRooms(reservationContext, reservation, request.ReservationRooms, request.ReservationRoomDetails, request.ReservationRoomExtras,
                                request.ReservationRoomChilds, request.ReservationExtraSchedules, request.Reservation.Status, request.HandleCancelationCost, request.HandleDepositCost, correlationId: correlationId, concatDepositPolicyName: !ignoreDepositPolicyConcat, ignoreAvailability: ignoreAvailability);

                            // Throw an exception if some room doesn't have availability
                            if (roomsIsAvailableList.ContainsValue(false))
                                throw Errors.AllotmentNotAvailable.ToBusinessLayerException();

                            List<Task> asyncTasks = new List<Task>();
                            //Insert Reservation Payment Option Detail
                            if (request.Reservation.PaymentMethodType_UID.HasValue)
                            {
                                asyncTasks.Add(StartConcurrentWork(() =>
                                {
                                    var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
                                    var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

                                    // Gets property configuration to know if has tokenization active
                                    var propertySecConfig = propertyRepo.GetPropertySecurityConfiguration(new Contracts.Requests.ListPropertySecurityConfigurationRequest { PropertyUIDs = new List<long> { request.Reservation.Property_UID } }).FirstOrDefault() ?? new Contracts.Data.Properties.PropertySecurityConfiguration();
                                    bool paymentIsValid = true;

                                    //Get Payment Method Type by Code.
                                    var objSelectedPaymentMethodType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest { UIDs = new List<long>() { request.Reservation.PaymentMethodType_UID.Value } }).FirstOrDefault();
                                    if (objSelectedPaymentMethodType != null)
                                    {
                                        request.Reservation.PaymentMethodType_UID = objSelectedPaymentMethodType.UID;
                                        if ((objSelectedPaymentMethodType.Code == (int)Constants.PaymentMethodTypesCode.CreditCard && request.ReservationPaymentDetail != null)
                                            || (request.ReservationPaymentDetail != null && request.ReservationPaymentDetail.PaymentMethod_UID.HasValue && !string.IsNullOrEmpty(request.ReservationPaymentDetail.CardNumber)))
                                        {
                                            // Refund old reservation amount
                                            if (!string.IsNullOrEmpty(reservationBeforeUpdate.PaymentGatewayTransactionID) && reservationBeforeUpdate.ReservationPaymentDetail != null && reservationBeforeUpdate.ReservationPaymentDetail.PaymentGatewayTokenizationIsActive)
                                                this.RefundPaymentGateway(BusinessObjectToDomainTypeConverter.Convert(reservationBeforeUpdate, new ListReservationRequest
                                                {
                                                    IncludeReservationPaymentDetail = true,
                                                }),
                                                    (decimal)(reservationBeforeUpdate.TotalAmount / reservationBeforeUpdate.PropertyBaseCurrencyExchangeRate),
                                                    request.SkipInterestCalculation,
                                                    reservationBeforeUpdate.ReservationRooms.Count);

                                            // Make payment with new amount if Property has PaymentGateway Tokenization active
                                            if (propertySecConfig.IsProtectedWithPaymentGateway == true)
                                            {
                                                var paymentReturn = this.MakePaymentWithPaymentGateway(objguest, request.ReservationPaymentDetail, reservation, request.SkipInterestCalculation);
                                                paymentIsValid = (paymentReturn >= 0);
                                            }
                                        }

                                        //Insert Payment details
                                        if (request.ReservationPaymentDetail != null)
                                        {
                                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.EncryptCreditCard) && request.ReservationPaymentDetail != null)
                                            {
                                                var securtityRepo = this.RepositoryFactory.GetOBSecurityRepository();

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
                                            //Insert ReservationPayment Detail
                                            reservationPaymentDetail = this.CreateReservationPaymentDetails(reservationContext, reservation, request.ReservationPaymentDetail, propertySecConfig, paymentIsValid);
                                        }

                                        //check if partial payment option.
                                        if (request.Reservation.IsPartialPayment.HasValue && request.Reservation.IsPartialPayment.Value && request.Reservation.NoOfInstallment.HasValue && request.Reservation.InstallmentAmount.HasValue)
                                        {
                                            //Insert ReservationPartialPayment Detail.
                                            reservationPartialPaymentDetails = this.CreateReservationPartialPaymentDetails(reservation.UID, request.Reservation.InterestRate, request.Reservation.InstallmentAmount, request.Reservation.NoOfInstallment.Value);
                                        }

                                        if (reservationPaymentDetail != null && reservationPaymentDetail.UID == 0)
                                        {
                                            reservation.ReservationPaymentDetails.Add(reservationPaymentDetail);
                                            reservationPaymentDetail.Reservation = reservation;
                                        }

                                        foreach (var partialPayment in reservationPartialPaymentDetails)
                                        {
                                            if (partialPayment.UID == 0)
                                            {
                                                reservation.ReservationPartialPaymentDetails.Add(partialPayment);
                                                partialPayment.Reservation = reservation;
                                            }
                                        }

                                    }
                                }));
                            }

                            // update credit
                            asyncTasks.Add(StartConcurrentWork(() =>
                            {
                                IncrementOperatorCreditUsed(reservation, request.ReservationPaymentDetail, request.Reservation.TotalAmount);
                            }));

                            Task.WaitAll(asyncTasks.ToArray());

                            //Cancel Rooms
                            if (request.ReservationRooms != null)
                            {
                                var rooms = request.ReservationRooms.Where(s => s.Status == (int)Constants.ReservationStatus.Cancelled).ToList();
                                if (rooms != null && rooms.Count > 0)
                                {
                                    var allotmentDetails = CancelRooms(reservation, rooms, request.Reservation.Property_UID, correlationId);
                                    reservationRoomDetailsRealAllotment.AddRange(allotmentDetails);
                                }
                            }

                            request.Reservation.ReservationRooms = request.ReservationRooms;
                            var errors = reservationValidator.ValidateReservation(request.Reservation, new ValidateReservationRequest()
                            {
                                ValidateGuarantee = request.ValidateGuarantee
                            });

                            if (errors.Count > 0)
                            {
                                var error = errors.First();
                                throw new BusinessLayerException(error.Description,
                                                error.ErrorType, error.ErrorCode);
                            }

                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ConvertValuesToPropertyCurrency))
                                reservationHelper.ApplyCurrencyExchangeToReservationForConnectors(reservation, null, reservationContext);

                            unitOfWork.Save();

                            #region RESERVATION FILTERS
                            reservationAfterUpdate = DomainToBusinessObjectTypeConverter.Convert(reservation, converterParameters);
                            reservationHelper.ModifyReservationFilter(reservationAfterUpdate, ServiceName.UpdateReservation, objguest);
                            #endregion RESERVATION FILTERS

                            scope.Complete();

                            transactionSucceeded = true;
                        }
                        finally
                        {
                            throughputLimiter.Release();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments);

                throw;
            }

            if (transactionSucceeded)
            {
                UpdateReservationLogHandler(reservationAfterUpdate, reservationBeforeUpdate, reservationContext, request.RequestId);

                QueueBackgroundWork(() =>
                {
                    UpdateReservationBackgroundWork(request.Reservation, request.GuestActivities, request.ReservationRooms, reservation,
                        oldReservationStatus, reservationContext,
                        bacthList, reservationBeforeUpdate, request.AlreadySentToPMS, request.BigPullAuthRequestor_UID);
                });
            }

            return reservationContext.ReservationUID;
        }

        private void UpdateReservationBackgroundWork(contractsReservations.Reservation objReservation, List<long> objGuestActivity, List<contractsReservations.ReservationRoom> objReservationRoom,
                                                    OB.Domain.Reservations.Reservation reservation, long oldReservationStatus, ReservationDataContext reservationContext, List<ReservationRoomCustom> bacthList,
                                                    contractsReservations.Reservation reservationBeforeUpdate, bool alreadySentToPMS, long? bigPullAuthId)
        {
            var logger = new Log.DefaultLogger("ReservationBackgroundWork");
            logger.Debug("UpdateReservationBackgroundWork: ReservationUID: {0}", objReservation.UID);

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var converterParameters = GenerateListReservationRequest();

                    var contractReservation = ConvertReservationToContractWithLookups(converterParameters, new List<domainReservations.Reservation>() { reservation }).FirstOrDefault();
                    var timeBefore = DateTime.Now.Ticks;

                    /* ----JG US 7863---- */
                    string serializedObjJson = Helper.JsonSerializer.SerializeToJson<ReservationRoomCustom>(bacthList);

                    //if (objReservation.Status == 2 || objReservation.Status == 4)
                    if ((contractReservation.Status == (long)Constants.ReservationStatus.Cancelled || contractReservation.Status == (long)Constants.ReservationStatus.Modified) && !alreadySentToPMS)
                        this.NotifyConnectors("Reservation", reservation.UID, objReservation.Property_UID, (int)Constants.ConnectorEventOperations.CancelRoom, serializedObjJson);//4

                    /* ------------------ */

                    var reservationAdditionalDataRepo = RepositoryFactory.GetRepository<ReservationsAdditionalData>(unitOfWork);
                    var reservationHelper = Resolve<IReservationHelperPOCO>();
                    var reservationAdditionalData = reservationHelper.GetReservationAdditionalData(reservation.UID);
                    if (reservationAdditionalData != null)
                    {
                        var contractAdditionalData = objReservation.ReservationAdditionalData;
                        if (contractAdditionalData != null)
                            contractAdditionalData.AlreadySentToPMS = alreadySentToPMS;
                        InsertReservationRoomAdditionalData(objReservationRoom, contractAdditionalData, reservationContext, reservation, contractReservation, reservationAdditionalData);
                    }
                    unitOfWork.Save();

                    //Insert Occupancy Alerts in PropertyQueue
                    this.InsOccupancyAlertsInPropertyQueue(reservation.UID, reservation.Property_UID, reservation.Channel_UID, true);

                    #region Insert Reservation into PropertyQueue

                    //sabre US 3948
                    if (objReservation.Status != (int)Constants.ReservationStatus.Pending)
                    {
                        // bug 12848 - new reservation
                        if (oldReservationStatus == (int)Constants.ReservationStatus.Pending && objReservation.Status == (int)Constants.ReservationStatus.Booked)
                            this.InsertPropertyQueue(reservation.Property_UID, reservation.UID, reservation.Channel_UID, (long)Constants.SystemEventsCode.NewBookingArrived);
                        else
                            this.InsertPropertyQueue(reservation.Property_UID, reservation.UID, reservation.Channel_UID, (long)Constants.SystemEventsCode.BookingChanged);

                        //change allotment if rooms are cancelled
                        if (objReservationRoom != null)
                        {
                            var rooms = objReservationRoom.Where(s => s.Status == (int)Constants.ReservationStatus.Cancelled).ToList();
                            if (rooms != null && rooms.Count > 0)
                            {
                                //send email
                                this.InsertPropertyQueue(reservation.Property_UID, reservation.UID, reservation.Channel_UID, (long)Constants.SystemEventsCode.Bookingcancelled);

                                this.CancelPropertyQueueEvent(objReservation.Property_UID, (long)Constants.SystemEventsCode.PreStay, objReservation.UID, "Reservation was Cancelled!!!");
                                this.CancelPropertyQueueEvent(objReservation.Property_UID, (long)Constants.SystemEventsCode.PostStay, objReservation.UID, "Reservation was Cancelled!!!");
                                this.CancelPropertyQueueEvent(objReservation.Property_UID, (long)Constants.SystemEventsCode.ExtraNotificationEmail, objReservation.UID, "Reservation was Cancelled!!!");
                            }
                        }
                    }

                    #endregion Insert Reservation into PropertyQueue


                    #region SEND ALLOTMENT FOR CHANNELS

                    SendAllotmentAndInventoryForChannels(contractReservation.Property_UID, contractReservation.Channel_UID ?? 0, null, reservationBeforeUpdate.ReservationRooms,
                        contractReservation.ReservationRooms, ServiceName.UpdateReservation, reservationContext.Inventories);

                    #endregion SEND ALLOTMENT FOR CHANNELS


                    //notify connectors async
                    //if (objReservation.Status == 1 || objReservation.Status == 2 || objReservation.Status == 4)
                    if ((contractReservation.Status == (long)Constants.ReservationStatus.Booked
                        || objReservation.Status == (long)Constants.ReservationStatus.Cancelled
                        || objReservation.Status == (long)Constants.ReservationStatus.Modified)
                        && !alreadySentToPMS)
                    {
                        short operationCode = 0;
                        switch (contractReservation.Status)
                        {
                            case 1: operationCode = 1; break;
                            case 2: operationCode = 3; break;
                            case 4: operationCode = 2; break;
                        }

                        this.NotifyConnectors("Reservation", objReservation.UID, objReservation.Property_UID, operationCode, null);
                    }


                    //hpatel
                    //Call Close Sales Async.
                    this.CloseSales(contractReservation);

                    this.MarkReservationsAsViewed(new MarkReservationsAsViewedRequest
                    {
                        Reservation_UIDs = new List<long> { reservation.UID },
                        User_UID = 65,
                        NewValue = false
                    });

                    #region LOG

                    string hostAdress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostName : string.Empty;
                    this.SaveReservationHistory(new ReservationLogRequest
                    {
                        ServiceName = OB.Reservation.BL.Contracts.Data.General.ServiceName.UpdateReservation,
                        OldReservation = reservationBeforeUpdate,
                        Reservation = contractReservation,
                        GuestActivity = objGuestActivity,
                        UserHostAddress = hostAdress,
                        UserID = 0,
                        ReservationTransactionId = Guid.NewGuid().ToString(),
                        ReservationTransactionState = OB.Reservation.BL.Constants.ReservationTransactionStatus.Commited,
                        BigPullAuthRequestor_UID = bigPullAuthId,
                        propertyName = reservationContext.PropertyName,
                        ChannelName = reservationContext.ChannelName,
                    });

                    #endregion LOG

                    var timeAfter = DateTime.Now.Ticks;
                    var timeEllpased = TimeSpan.FromTicks(timeAfter - timeBefore);

                    Debug.WriteLine("Proactive Actions (" + reservation.UID + ") took: " + timeEllpased.TotalMilliseconds + " ms");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "UpdateReservationBackgroundWork");

                Dictionary<string, object> arguments = new Dictionary<string, object>();

                arguments.Add("NewReservation", objReservation);
                arguments.Add("OldReservation", reservationBeforeUpdate);
                arguments.Add("PropertyID", reservationBeforeUpdate.Property_UID);

                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments, null,
                    "Ocorreram erros nas tarefas de background do UpdateReservation");
                throw;
            }
        }

        private void UpdateReservationLogHandler(contractsReservations.Reservation contractReservation, contractsReservations.Reservation reservationBeforeUpdate, ReservationDataContext reservationDataContext, string requestId)
        {
            this.NewLogReservation(OB.Reservation.BL.Contracts.Data.General.ServiceName.UpdateReservation,
                Constants.AdminUserUID,
                Constants.AdminUserName,
                reservationDataContext.PropertyName,
                oldReservation: reservationBeforeUpdate,
                contractReservation,
                channelName: reservationDataContext.ChannelName,
                propertyBaseCurrencyId: reservationDataContext.PropertyBaseCurrency_UID,
                requestId: requestId);
        }

        /// <summary>
        /// Modify Reservation
        /// </summary>
        /// <param name="request">ModifyReservationRequest</param>
        /// <param name="groupRule"></param>
        /// <param name="nextTransactionState"></param>
        /// <param name="hangfireId"></param>
        /// <param name="retries"></param>
        /// <para>No allotment available Error = <b>-500</b></para>
        /// <para>Channel is not on property Error = <b>-501</b></para>
        /// <para>Channel is not on rate Error = <b>-502</b></para>
        /// <para>Property channel mapping not has this combination Error = <b>-503</b></para>
        /// <para>Invalid Agency Error = <b>-504</b></para>
        /// <para>Invalid company code Error = <b>-505</b></para>
        /// <para>Missig days in date range Error = <b>-506</b></para>
        /// <para>Wrong currency Error = <b>-507</b></para>
        /// <para>Bortype type is different in request Error = <b>-509</b></para>
        /// <para>Occupancy Not Available = <b>-510</b></para>
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
        /// <para>Reservation Is Already Cancelled Error = <b>-541</b></para>
        /// <para>One or more invalid roomtypes Error = <b>-542</b></para>
        /// <para>Nothing to update Error = <b>-543</b></para>
        /// <para>One or more Rooms have cancelation costs applied Error = <b>-544</b></para>
        /// <para>Invalid Reservation Transaction Error = <b>-546</b></para>
        /// <returns>ModifyReservationResponse</returns>
        private ModifyReservationResponse ModifyReservation(ModifyReservationRequest request, GroupRule groupRule,
            Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState, long hangfireId, int retries)
        {
            var response = new ModifyReservationResponse();
            domainReservations.Reservation reservation = null;
            contractsReservations.Reservation oldReservation = null;
            ValidateReservationRequest validationRequest = null;
            ReservationsAdditionalData reservationAdditionalData = null;
            List<contractsProperties.Inventory> inventories = new List<contractsProperties.Inventory>();

            try
            {
                var sendOperatorCreditLimitExcededEmail = false;
                var sendTpiCreditLimitExcededEmail = false;

                #region VALIDATE REQUEST

                if (request.ReservationRooms.Count <= 0)
                    throw Errors.InvalidRoom.ToBusinessLayerException();

                if (request.ReservationRooms.Any(x => (!x.AdultCount.HasValue && !x.ChildCount.HasValue && !x.DateFrom.HasValue
                        && !x.DateTo.HasValue && !x.RoomType_UID.HasValue && x.Guest == null)))
                    throw Errors.InvalidRoom.ToBusinessLayerException();

                #endregion VALIDATE REQUEST

                var reservationHelper = Resolve<IReservationHelperPOCO>();

                using (var scope = TransactionManager.BeginTransactionScope(OB.Domain.Reservations.Reservation.DomainScope))
                {
                    using (var unitOfWork = SessionFactory.GetUnitOfWork())
                    {
                        #region GET RESERVATION

                        var reservationRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                        if (request.ReservationUid > 0)
                            reservation = reservationRepo.FindByUIDEagerly(request.ReservationUid);
                        else if (!string.IsNullOrEmpty(request.ReservationNumber))
                            reservation = reservationRepo.FindByReservationNumberAndChannelUID(request.ReservationNumber, request.ChannelId);

                        #endregion GET RESERVATION

                        #region VALIDATE RESERVATION AND CONVERT OLD RESERVATION

                        var propRepo = RepositoryFactory.GetOBPropertyRepository();

                        if (reservation == null)
                            throw Errors.ReservationDoesNotExist.ToBusinessLayerException();

                        if (reservation.Number != request.ReservationNumber)
                            throw Errors.InvalidReservationNumber.ToBusinessLayerException();

                        if (reservation.Status == (int)Reservation.BL.Constants.ReservationStatus.Cancelled || reservation.Status == (int)Reservation.BL.Constants.ReservationStatus.CancelledOnRequest)
                            throw Errors.ReservationIsAlreadyCancelledError.ToBusinessLayerException();

                        if (reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value)
                            throw Errors.ReservationIsOnRequest.ToBusinessLayerException();

                        if (!reservation.ReservationLanguageUsed_UID.HasValue)
                        {
                            reservationHelper.SetLanguageToReservation(reservation, reservation.Property_UID, propRepo);
                        }

                        var converterParameters = GenerateListReservationRequest();

                        oldReservation = ConvertReservationToContractWithLookups(converterParameters, new List<domainReservations.Reservation>() { reservation }).FirstOrDefault();

                        #endregion VALIDATE RESERVATION AND CONVERT OLD RESERVATION

                        #region VALIDATE MODIFICATION

                        var reservationValidator = Resolve<IReservationValidatorPOCO>();
                        var childTermsRepo = RepositoryFactory.GetOBChildTermsRepository();
                        var childTermsResponse = childTermsRepo.ListChildTerms(new Contracts.Requests.ListChildTermsRequest
                        {
                            PropertyUIDs = new List<long>() { reservation.Property_UID },
                            IncludeChildTermCurrencies = true
                        });

                        if (childTermsResponse.Status != OB.BL.Contracts.Responses.Status.Success)
                            throw Errors.ChildTermsError.ToBusinessLayerException();

                        reservationAdditionalData = reservationHelper.GetReservationAdditionalData(reservation.UID);
                        validationRequest = new ValidateReservationRequest()
                        {
                            ChildTerms = childTermsResponse.Result,
                            ValidateCancelationCosts = false,
                            ValidateGuarantee = true,
                            GroupRule = groupRule,
                            ReservationAdditionalData = reservationAdditionalData
                        };

                        var guestChanged = (request.Guest != null && (request.Guest.FirstName != reservation.GuestFirstName || request.Guest.LastName != reservation.GuestLastName
                            || request.Guest.Address1 != reservation.GuestAddress1 || request.Guest.Address2 != reservation.GuestAddress2
                            || request.Guest.City != reservation.GuestCity || request.Guest.CountryId != reservation.GuestCountry_UID
                            || request.Guest.Email != reservation.GuestEmail || request.Guest.IdCardNumber != reservation.GuestIDCardNumber
                            || request.Guest.Phone != reservation.GuestPhone || request.Guest.PostalCode != reservation.GuestPostalCode
                            || request.Guest.StateId != reservation.GuestState_UID));

                        var billingChanged = (request.BillingInfo != null && (request.BillingInfo.Address1 != reservation.BillingAddress1 || request.BillingInfo.Address2 != reservation.BillingAddress2
                            || request.BillingInfo.City != reservation.BillingCity || request.BillingInfo.ContactName != reservation.BillingContactName
                            || request.BillingInfo.CountryId != reservation.BillingCountry_UID || request.BillingInfo.Email != reservation.BillingEmail
                            || request.BillingInfo.Phone != reservation.BillingPhone || request.BillingInfo.PostalCode != reservation.BillingPostalCode
                            || request.BillingInfo.StateId != reservation.BillingState_UID || request.BillingInfo.TaxCardNumber != reservation.BillingTaxCardNumber));

                        if (guestChanged || billingChanged)
                            validationRequest.OnlyChangeGuestName = true;

                        reservationValidator.ValidateModifyReservation(reservation, oldReservation, validationRequest, request.ReservationRooms);

                        #endregion VALIDATE MODIFICATION

                        #region RESERVATION MODIFICATION

                        //TODO: Get property currency on reservation lookups
                        //get property currency
                        var propertyCurrency = propRepo.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest { UIDs = new List<long> { reservation.Property_UID } }).Select(x => x.BaseCurrency_UID).FirstOrDefault();
                        if (propertyCurrency <= 0)
                            Errors.PropertyMustHaveCurrencyDefined.ToBusinessLayerException();

                        var finalReservation = ModifyReservation(request, reservation, oldReservation, validationRequest,
                            out sendOperatorCreditLimitExcededEmail, out sendTpiCreditLimitExcededEmail, nextTransactionState, reservationAdditionalData, propertyCurrency, inventories, groupRule);

                        // Update Reservation Transaction State
                        reservationHelper.UpdateReservationTransactionStatus(request.TransactionId, request.ChannelId, nextTransactionState);

                        #endregion RESERVATION MODIFICATION

                        #region CONVERT RESERVATION AND GET LOOKUPS

                        converterParameters.IncludePromotionalCodes = false;
                        converterParameters.IncludeExtras = true;
                        converterParameters.IncludeGroupCodes = true;
                        converterParameters.IncludeIncentives = true;
                        converterParameters.IncludeReservationAddicionalData = true;
                        converterParameters.IncludeRoomTypes = true;
                        converterParameters.IncludeRates = true;
                        converterParameters.IncludeTaxPolicies = true;
                        converterParameters.IncludeReservationResumeInfo = true;
                        converterParameters.IncludeReservationCurrency = true;
                        converterParameters.IncludePropertyBaseCurrency = true;
                        converterParameters.IncludeReservationBaseCurrency = true;
                        converterParameters.IncludeChannelOperator = true;
                        converterParameters.IncludeGuestActivities = true;
                        converterParameters.IncludeReservationStatusName = true;
                        converterParameters.IncludeReferralSource = true;

                        response.Reservation = ConvertReservationToContractWithLookups(converterParameters, new List<domainReservations.Reservation>() { finalReservation }).FirstOrDefault();

                        if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ReturnSellingPrices))
                            reservationHelper.MapSellingPrices(response.Reservation, true, true);

                        #endregion CONVERT RESERVATION AND GET LOOKUPS

                        #region RESERVATION FILTERS

                        reservationHelper.ModifyReservationFilter(response.Reservation, ServiceName.ModifyReservation);

                        #endregion RESERVATION FILTERS

                        scope.Complete();
                    }
                }

                #region SetJobHangfire

                if (request.TransactionAction == Reservation.BL.Constants.ReservationTransactionAction.Initiate
                            || request.TransactionAction == Reservation.BL.Constants.ReservationTransactionAction.Modify)
                {
                    hangfireId = reservationHelper.SetJobToIgnorePendingReservation(request, hangfireId);
                }

                #endregion SetJobHangfire

                ModifyReservationLogHandler(response.Reservation, oldReservation, request.RequestId);

                #region BACKGROUND OPERATION

                QueueBackgroundWork(() =>
                {
                    ModifyReservationBackgroundOperations(new ReservationBackgroundOperationsRequest
                    {
                        NewReservation = response.Reservation,
                        OldReservation = oldReservation,
                        SendOperatorCreditLimitExcededEmail = sendOperatorCreditLimitExcededEmail,
                        SendTPICreditLimitExcededEmail = sendTpiCreditLimitExcededEmail,
                        ReservationTransactionId = request.TransactionId,
                        ReservationTransactionState = nextTransactionState,
                        ReservationAdditionalDataUID = reservationAdditionalData != null ? reservationAdditionalData.UID : (long?)null,
                        ReservationAdditionalData = reservationAdditionalData,
                        HangfireId = hangfireId,
                        ReservationRequest = request,
                        TransactionRetries = retries,
                        Inventories = inventories,
                        BigPullAuthRequestor_UID = request.BigPullAuthRequestor_UID
                    });
                });

                #endregion BACKGROUND OPERATION
            }
            catch (Exception ex)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return response;
        }

        /// <summary>
        /// Update reservation with the requested modifications
        /// <para>Cancellation Policy Has Change Error = <b>-538</b></para>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="newReservation"></param>
        /// <param name="oldReservation"></param>
        /// <param name="validationRequest"></param>
        /// <param name="sendOperatorCreditLimitExcededEmail"></param>
        /// <param name="sendTpiCreditLimitExcededEmail"></param>
        /// <param name="nextTransactionState"></param>
        /// <param name="reservationAdditionalData"></param>
        /// <returns></returns>
        private domainReservations.Reservation ModifyReservation(ModifyReservationRequest request,
            domainReservations.Reservation newReservation, contractsReservations.Reservation oldReservation, ValidateReservationRequest validationRequest,
                    out bool sendOperatorCreditLimitExcededEmail, out bool sendTpiCreditLimitExcededEmail, Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState,
            ReservationsAdditionalData reservationAdditionalData, long propertyCurrency, List<contractsProperties.Inventory> inventories, GroupRule groupRule, bool checkAvailability = false)
        {
            List<contractsReservations.UpdateRoom> roomsToUpdate = request.ReservationRooms;
            var throughputLimiterPOCO = Resolve<IThroughputLimiterManagerPOCO>();
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();
            var reservationValidatorPoco = Resolve<IReservationValidatorPOCO>();
            var reservationPricesPOCO = Resolve<IReservationPricesCalculationPOCO>();
            List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation> rrdList;
            ThroughputLimiter throughputLimiter = null;
            sendOperatorCreditLimitExcededEmail = false;
            sendTpiCreditLimitExcededEmail = false;
            OB.BL.Contracts.Data.CRM.Guest guestToUpdate = null;
            contractsCRMOB.LoyaltyProgram loyaltyProgram = null;
            contractsCRMOB.LoyaltyLevel loyaltyLevel = null;
            LoyaltyLevelValidationCriteria loyaltyLevelValidationCriteria = null;
            bool HasFlagPullTpiReservationCalculation = (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.PullTpiReservationCalculation));

            #region GET RESERVATIONADDITIONALDATA OBJECT
            var reservationAdditionalDataJsonObj = reservationHelper.GetReservationAdditionalDataJsonObject(ref reservationAdditionalData, newReservation.UID);
            #endregion

            #region GET RULES FROM PO
            IEnumerable<SellRule> rules = null;
            if (HasFlagPullTpiReservationCalculation)
            {
                rules = reservationHelper.GetRulesFromPortal(newReservation.Channel_UID.Value, newReservation.Property_UID,
                    reservationAdditionalDataJsonObj.ExternalTpiId, reservationAdditionalDataJsonObj.ExternalChannelId, newReservation.TPI_UID, newReservation.ReservationCurrency_UID);
                rules = rules.GroupBy(x => x.KeeperType).Select(x => x.OrderByDescending(j => j.RuleType).FirstOrDefault()).ToList();
                rules = rules.OrderByDescending(x => x.KeeperType);
            }
            #endregion

            try
            {
                var reservationStatus = (int)ReservationStateMachine.GetNextReservationState(Reservation.BL.Constants.ReservationAction.Modify, nextTransactionState);
                throughputLimiter = throughputLimiterPOCO.GetThroughputLimiter(_MODIFY_RESERVATION_THROUGHPUTLIMITER_KEY, _MODIFY_RESERVATION_MAX_THREADS,
                    _MODIFY_RESERVATION_MAX_THREADS);

                throughputLimiter.Wait();

                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    if (request.Guest != null || (request.GuestLoyaltyLevelId != null && validationRequest.GroupRule.BusinessRules.HasFlag(BusinessRules.LoyaltyDiscount)))
                    {
                        var crmRepo = RepositoryFactory.GetOBCRMRepository();
                        guestToUpdate = crmRepo.ListGuestsByLightCriteria(new Contracts.Requests.ListGuestLightRequest { UIDs = new List<long> { newReservation.Guest_UID } }).FirstOrDefault();

                        #region Update Guest
                        if (request.Guest != null)
                        {
                            //map guest information
                            BusinessObjectToDomainTypeConverter.Map(request.Guest, newReservation);

                            if (guestToUpdate != null && !guestToUpdate.FirstName.Equals(request.Guest.FirstName)
                                    && !guestToUpdate.LastName.Equals(request.Guest.LastName))
                            {
                                guestToUpdate.FirstName = request.Guest.FirstName;
                                guestToUpdate.LastName = request.Guest.LastName;

                                UpdateGuest(guestToUpdate, null, null, null);
                            }
                        }
                        #endregion Update Guest

                        #region LOYALTY LEVEL TOTAL RESERVATIONS VALIDATION

                        if (request.GuestLoyaltyLevelId != null && validationRequest.GroupRule.BusinessRules.HasFlag(BusinessRules.LoyaltyDiscount))
                        {
                            if (request.GuestLoyaltyLevelId != guestToUpdate.LoyaltyLevel_UID)
                                throw Errors.GuestLoyaltyLevelIsDiferentFromRegistered.ToBusinessLayerException();

                            int total = 0;
                            var loyaltyRsp = crmRepo.ListLoyaltyPrograms(new Contracts.Requests.ListLoyaltyProgramRequest
                            {
                                ReturnTotal = true,
                                Client_UIDs = new List<long> { guestToUpdate.Client_UID },
                                IncludeDeleted = false,
                                IncludeDefaultCurrency = true,
                                IncludeDefaultLanguage = false,
                                IncludeLoyaltyLevels = true,
                                IncludeLoyaltyLevelsCurrencies = true,
                                IncludeLoyaltyLevelsLanguages = false,
                                IncludeLoyaltyProgramLanguages = false,
                                ExcludeInactive = true,
                                LoyaltyLevel_Uids = new List<long> { guestToUpdate.LoyaltyLevel_UID.Value }
                            });

                            if (loyaltyRsp.Results != null && loyaltyRsp.Results.Count > 0)
                            {
                                loyaltyProgram = loyaltyRsp.Results.FirstOrDefault();

                                // Validate Guest Loyalty Level
                                loyaltyLevel = loyaltyProgram.LoyaltyLevels.FirstOrDefault();
                                if (loyaltyLevel != null)
                                {
                                    loyaltyLevelValidationCriteria = new LoyaltyLevelValidationCriteria
                                    {
                                        LoyaltyLevelLimitsPeriodicityValue = loyaltyLevel.LoyaltyLevelLimitsPeriodicityValue,
                                        Guest_UID = guestToUpdate.UID,
                                        IsForNumberOfReservationsActive = loyaltyLevel.IsForNumberOfReservationsActive,
                                        IsForNumberOfReservationsValue = loyaltyLevel.IsForNumberOfReservationsValue,
                                        IsForNightsRoomActive = loyaltyLevel.IsForNightsRoomActive,
                                        IsForNightsRoomValue = loyaltyLevel.IsForNightsRoomValue,
                                        IsForTotalReservationsActive = loyaltyLevel.IsForTotalReservationsActive,
                                        LoyaltyLevelBaseCurrency_UID = loyaltyProgram.DefaultCurrency.UID,
                                        IsForTotalReservationsValue = loyaltyLevel.IsForTotalReservationsValue,
                                        LoyaltyLevel_UID = loyaltyLevel.UID,
                                        IsForReservationValue = loyaltyLevel.IsForReservationValue,
                                        IsForReservationRoomNightsValue = loyaltyLevel.IsForReservationRoomNightsValue,
                                        IsForReservationActive = false,
                                        IsLimitsForPeriodicityActive = loyaltyLevel.IsLimitsForPeriodicityActive,
                                        LoyaltyLevelLimitsPeriodicity_UID = loyaltyLevel.LoyaltyLevelLimitsPeriodicity_UID,
                                    };

                                    reservationValidatorPoco.ValidateGuestLoyaltyLevel(loyaltyLevelValidationCriteria);
                                }
                                else
                                    loyaltyProgram = null;
                            }
                        }

                        #endregion LOYALTY LEVEL TOTAL RESERVATIONS VALIDATION
                    }

                    if (request.BillingInfo != null)
                    {
                        //map billing information
                        BusinessObjectToDomainTypeConverter.Map(request.BillingInfo, newReservation);
                    }

                    // if just the guest name changed, don't do any more operations
                    if (!validationRequest.OnlyChangeGuestName)
                    {
                        // Get TaxPolicies
                        var rateIds = newReservation.ReservationRooms.Where(x => x.Rate_UID.HasValue).Select(x => x.Rate_UID.Value).ToList();                        
                        var taxPoliciesByRate = reservationHelper.GetTaxPoliciesByRateIds(rateIds, newReservation.ReservationBaseCurrency_UID, newReservation.ReservationLanguageUsed_UID);

                        // Get Exchange Rate
                        var exchangeRate = reservationHelper.GetExchangeRateBetweenCurrenciesByPropertyId(newReservation.ReservationBaseCurrency_UID.Value,
                                        propertyCurrency, newReservation.Property_UID);

                        #region Reset Vars

                        newReservation.TotalTax = 0;
                        newReservation.Adults = 0;
                        newReservation.Children = 0;
                        newReservation.RoomsPriceSum = 0;
                        newReservation.RoomsTotalAmount = 0;
                        newReservation.RoomsTax = 0;
                        newReservation.TotalAmount = 0;
                        newReservation.ModifyDate = DateTime.UtcNow;

                        #endregion Reset Vars

                        #region MAP EXTERNAL SELLING INFORMATION CONFIGURATION
                        if (rules != null && rules.Any())
                        {
                            reservationAdditionalDataJsonObj.ExternalSellingReservationInformationByRule = new List<contractsReservations.ExternalSellingReservationInformation>();
                            foreach (var rule in rules)
                            {
                                if (rule.MarkupType == 0)
                                    rule.MarkupType = ExternalApplianceType.Define;
                                if (rule.CommissionType == 0)
                                    rule.CommissionType = ExternalApplianceType.Define;

                                var extSellingReservationInformation = new contractsReservations.ExternalSellingReservationInformation();
                                extSellingReservationInformation.IsPaid = newReservation.IsPaid.HasValue ? newReservation.IsPaid.Value : false;
                                extSellingReservationInformation.KeeperUID = rule.KeeperUid;
                                extSellingReservationInformation.KeeperType = (PO_KeeperType)rule.KeeperType;
                                extSellingReservationInformation.Markup = rule.Markup;
                                extSellingReservationInformation.MarkupType = (int)rule.MarkupType;
                                extSellingReservationInformation.MarkupIsPercentage = rule.MarkupIsPercentage;
                                extSellingReservationInformation.Commission = rule.Commission;
                                extSellingReservationInformation.CommissionType = (int)rule.CommissionType;
                                extSellingReservationInformation.CommissionIsPercentage = rule.CommissionIsPercentage;
                                extSellingReservationInformation.Tax = rule.Tax;
                                extSellingReservationInformation.TaxIsPercentage = rule.TaxIsPercentage;
                                extSellingReservationInformation.CurrencyUID = rule.CurrencyValueUID != 0 ? rule.CurrencyValueUID : rule.CurrencyBaseUID;

                                reservationAdditionalDataJsonObj.ExternalSellingReservationInformationByRule.Add(extSellingReservationInformation);
                            }
                        }
                        #endregion MAP EXTERNAL SELLING INFORMATION CONFIGURATION

                        #region UPDATE RESERVATION ROOMS

                        // Get valid rates for promocode
                        var validatePromoRequest = new ValidatePromocodeForReservationParameters()
                        {
                            ReservationRooms = newReservation.ReservationRooms.Select(rr => new ReservationRoomStayPeriod()
                            {
                                RateUID = rr.Rate_UID.Value,
                                CheckIn = rr.DateFrom.Value,
                                CheckOut = rr.DateTo.Value
                            }).ToList(),
                            OldAppliedDiscountDays = oldReservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).Where(x => x.ReservationRoomDetailsAppliedPromotionalCode != null)
                                .Select(x => x.ReservationRoomDetailsAppliedPromotionalCode.Date.Date).Distinct().ToList(),
                            PromocodeUID = newReservation.PromotionalCode_UID,
                            CurrencyUID = newReservation.ReservationBaseCurrency_UID.Value,
                        };
                        var validPromocodeResponse = reservationHelper.ValidatePromocodeForReservation(validatePromoRequest);

                        if (validPromocodeResponse == null || validPromocodeResponse.RejectReservation)
                            throw Errors.InvalidPromocodeForReservation.ToBusinessLayerException();

                        // Remove promotionalCode_UID from reservation if doesn't have discounts to apply
                        if (validPromocodeResponse.PromoCodeObj == null || !validPromocodeResponse.PromoCodeObj.IsValid ||
                            validPromocodeResponse.NewDaysToApplyDiscount == null || !validPromocodeResponse.NewDaysToApplyDiscount.Any())
                            newReservation.PromotionalCode_UID = null;


                        List<long> modifiedRatesIds = new List<long>();
                        foreach (var roomToUpdate in roomsToUpdate)
                        {
                            var externalSellingRoomInformationByRule = new List<contractsReservations.ExternalSellingRoomInformation>();

                            var room = newReservation.ReservationRooms.FirstOrDefault(x => x.ReservationRoomNo == roomToUpdate.Number);
                            //reservation room details
                            var rrd = room.ReservationRoomDetails.Where(x => x.ReservationRoom_UID == room.UID).OrderBy(x => x.Date).ToList();
                            modifiedRatesIds.Add(room.Rate_UID.Value);

                            #region PARAMETER VALIDATION

                            if (room.Rate_UID == null)
                                throw Errors.InvalidRate.ToBusinessLayerException();

                            if (room.RoomType_UID == null)
                                throw Errors.InvalidRoom.ToBusinessLayerException();

                            if (room.DateFrom == null)
                                throw Errors.InvalidCheckIn.ToBusinessLayerException();

                            if (room.DateTo == null)
                                throw Errors.InvalidCheckOut.ToBusinessLayerException();

                            #endregion PARAMETER VALIDATION

                            #region CALCULATE RESERVATION ROOM PRICES
                            var ages = room.ReservationRoomChilds.Where(x => x.Age.HasValue).Select(x => x.Age.Value).ToList();

                            int? rateModelId = null; //OB gets rateModelId value when a reservation has filled the externalTpiId (TPI PO), send rateModelId null
                            if (!reservationAdditionalDataJsonObj.ExternalTpiId.HasValue)
                                rateModelId = (int?)room.CommissionType;

                            var parameters = new CalculateFinalPriceParameters()
                            {
                                CheckIn = room.DateFrom.Value,
                                CheckOut = room.DateTo.Value,
                                BaseCurrency = newReservation.ReservationBaseCurrency_UID.Value,
                                AdultCount = room.AdultCount.Value,
                                ChildCount = room.ChildCount ?? 0,
                                Ages = ages,
                                ChildTerms = validationRequest.ChildTerms,
                                GroupRule = validationRequest.GroupRule,
                                ExchangeRate = exchangeRate,
                                IsModify = true,
                                PropertyId = newReservation.Property_UID,
                                RateId = room.Rate_UID.Value,
                                RoomTypeId = room.RoomType_UID.Value,
                                TpiId = newReservation.TPI_UID,
                                RateModelId = rateModelId,
                                ChannelId = newReservation.Channel_UID.Value,
                                TPIDiscountIsPercentage = room.TPIDiscountIsPercentage,
                                TPIDiscountIsValueDecrease = room.TPIDiscountIsValueDecrease,
                                TPIDiscountValue = room.TPIDiscountValue,
                                LoyaltyProgram = loyaltyProgram,
                                ValidPromocodeParameters = validPromocodeResponse
                            };

                            rrdList = reservationPricesPOCO.CalculateReservationRoomPrices(parameters);

                            var rrAdditionalData = reservationAdditionalDataJsonObj.ReservationRoomList.FirstOrDefault(x => x.ReservationRoomNo == roomToUpdate.Number);
                            if (rrAdditionalData != null)
                                rrAdditionalData.ReservationRoom_UID = room.UID;
                            else
                                reservationAdditionalDataJsonObj.ReservationRoomList.Add(
                                       new OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomAdditionalData()
                                       {
                                           ReservationRoomNo = roomToUpdate.Number,
                                           ReservationRoom_UID = room.UID
                                       });


                            if (reservationAdditionalDataJsonObj.ExternalTpiId.HasValue)
                            {
                                room.CommissionType = rrdList.First().IsMarkup ? (int)RateModels.Markup : rrdList.First().IsCommission ? (int)RateModels.Commissionable : (int)RateModels.Package;
                                room.CommissionValue = rrdList.First().RateModelValue;
                                rrAdditionalData.CommissionType = (int)RateModels.Commissionable;//reservation has filled the externalTpiId (TPI PO) the commissiontype always is commission (PVP)
                            }

                            #region CALCULATE WITH EXTERNAL MARKUP RULES
                            var rrdListClone = rrdList.Clone();
                            //if there are no rules
                            List<contractsRates.RateRoomDetailReservation> rrdWithExtMarkupRuleList;
                            #endregion

                            #endregion CALCULATE RESERVATION ROOM PRICES

                            #region POLICIES

                            #region DEPOSTI POLICY

                            reservationHelper.SetDepositPolicies(room, rrdList, ref reservationAdditionalDataJsonObj, newReservation.UID, newReservation.ReservationBaseCurrency_UID,
                                newReservation.ReservationLanguageUsed_UID, newReservation.PropertyBaseCurrencyExchangeRate);

                            #endregion DEPOSTI POLICY

                            #region CANCELLATION POLICY

                            reservationHelper.SetCancellationPolicies(room, rrdList, newReservation.ReservationBaseCurrency_UID, newReservation.ReservationLanguageUsed_UID,
                                newReservation.PropertyBaseCurrencyExchangeRate, request.RuleType.Value == OB.Reservation.BL.Constants.RuleType.Pull);

                            #endregion CANCELLATION POLICY

                            #region OTHER POLICIES

                            reservationHelper.SetOtherPolicy(room, room.Rate_UID, newReservation.ReservationLanguageUsed_UID);

                            #endregion OTHER POLICIES

                            #endregion POLICIES

                            #region ROOM MODIFICATIONS
                            var fisrtRrd = rrdList.First();

                            room.ModifiedDate = DateTime.UtcNow;
                            room.ReservationRoomsTotalAmount = 0;
                            room.ReservationRoomsPriceSum = 0;
                            room.TotalTax = 0;
                            room.CommissionType = (fisrtRrd.IsCommission ? 1 : (fisrtRrd.IsPackage ? 5 : 2));
                            room.CommissionValue = fisrtRrd.RateModelValue;

                            #endregion ROOM MODIFICATIONS

                            // Children Already Updated (ReservationRoomChild)
                            // Extras
                            reservationHelper.SetIncludedExtras(room, (newReservation.ReservationLanguageUsed_UID ?? 1));

                            #region CALCULATE ROOM 
                            // SET RESERVATION ROOM DETAILS
                            decimal total = 0;

                            reservationHelper.SetReservationRoomDetails(room, rrdList, out total);
                            room.ReservationRoomsPriceSum = total;

                            // Set Tax Policies
                            reservationHelper.SetReservationRoomTaxPolicies(room, taxPoliciesByRate, exchangeRate, rrdList);

                            #endregion CALCULATE ROOM 

                            if (rules != null && rules.Any())
                            {
                                #region CALCULATE ROOM WITH EXTERNAL MARKUP BY RULE
                                int i = 0;
                                var rulesTemp = rules.Clone();
                                foreach (var rule in rulesTemp)
                                {
                                    if (i > 0 && (rule.KeeperType == (int)PO_KeeperType.Channel || rule.KeeperType == (int)PO_KeeperType.TPI_PO))
                                    {
                                        rule.Markup = rule.Markup + rules.First().Markup;
                                        rule.Tax = rule.Tax + rules.First().Tax;
                                        rule.MarkupCurrencyValue = rule.MarkupCurrencyValue + rules.First().MarkupCurrencyValue;
                                        rule.TaxCurrencyValue = rule.TaxCurrencyValue + rules.First().TaxCurrencyValue;
                                    }

                                    rrdWithExtMarkupRuleList = reservationPricesPOCO.CalculateExternalMarkup(rrdListClone, parameters.GroupRule, rule);

                                    #region Set Tax Policies with markup
                                    contractsReservations.ReservationRoom roomTemp = new contractsReservations.ReservationRoom
                                    {
                                        UID = room.UID,
                                        Rate_UID = room.Rate_UID.Value,
                                        ReservationRoomsPriceSum = rrdWithExtMarkupRuleList.Sum(x => x.FinalPrice),
                                        AdultCount = room.AdultCount,
                                        ChildCount = room.ChildCount,
                                        ReservationRoomDetails = room.ReservationRoomDetails != null ? room.ReservationRoomDetails.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList() : new List<contractsReservations.ReservationRoomDetail>(),
                                    };
                                    reservationHelper.SetReservationRoomTaxPolicies(roomTemp, taxPoliciesByRate, exchangeRate, rrdList);
                                    #endregion Set Tax Policies with markup

                                    var taxesPerDay = new List<contractsReservations.PriceDay>();
                                    //only representatives have taxes
                                    if (rule.KeeperType == (int)PO_KeeperType.Representative)
                                        taxesPerDay = rrdWithExtMarkupRuleList.Where(x => x.PriceAfterExternalTaxes > 0).Select(x => new contractsReservations.PriceDay { Date = x.Date.Date, Price = (x.PriceAfterExternalTaxes - x.PriceAfterExternalMarkup) }).ToList();

                                    // calcule ReservationRoomsPriceSum 
                                    // calcule ReservationRoomsTotalAmount
                                    externalSellingRoomInformationByRule.Add(new OB.Reservation.BL.Contracts.Data.Reservations.ExternalSellingRoomInformation
                                    {
                                        KeeperUID = rule.KeeperUid,
                                        KeeperType = (PO_KeeperType)rule.KeeperType,
                                        PricesPerDay = rrdWithExtMarkupRuleList.Select(x => new OB.Reservation.BL.Contracts.Data.Reservations.PriceDay { Date = x.Date, Price = x.FinalPrice }).ToList(),
                                        TaxesPerDay = taxesPerDay,
                                        ReservationRoomsPriceSum = rrdWithExtMarkupRuleList.Sum(x => x.FinalPrice),
                                        ReservationRoomsTotalAmount = (rrdWithExtMarkupRuleList.Sum(x => x.FinalPrice) + (room.ReservationRoomsExtrasSum ?? 0) + (roomTemp.TotalTax ?? 0)),
                                        ReservationRoomsExtrasSum = room.ReservationRoomsExtrasSum,
                                        TotalTax = roomTemp.TotalTax,
                                        TaxPolicies = room.ReservationRoomTaxPolicies.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList()
                                    });

                                    i++;
                                }

                                #region Set ExternalSellingRoomInformation in ReservationRoomAdditionalData
                                if (!externalSellingRoomInformationByRule.Any())
                                    externalSellingRoomInformationByRule = null;

                                if (rrAdditionalData != null)
                                    rrAdditionalData.ExternalSellingInformationByRule = externalSellingRoomInformationByRule;
                                #endregion Set ExternalSellingRoomInformation in ReservationRoomAdditionalData

                                #endregion
                            }

                            // Room Calculations
                            room.ReservationRoomsTotalAmount = (room.ReservationRoomsPriceSum ?? 0) + (room.ReservationRoomsExtrasSum ?? 0) + (room.TotalTax ?? 0);

                            // Update room Status
                            room.Status = reservationStatus;
                        }

                        #endregion UPDATE RESERVATION ROOMS

                        #region RESERVATION UPDATE

                        newReservation.Adults += (int)newReservation.ReservationRooms.Sum(x => x.AdultCount);
                        newReservation.Children += newReservation.ReservationRooms.Sum(x => x.ChildCount);
                        newReservation.TotalTax += (newReservation.ReservationRooms.Sum(x => x.TotalTax) ?? 0);
                        newReservation.TotalAmount += (newReservation.ReservationRooms.Sum(x => x.ReservationRoomsTotalAmount));
                        newReservation.RoomsPriceSum += (newReservation.ReservationRooms.Sum(x => x.ReservationRoomsPriceSum) ?? 0);
                        newReservation.RoomsTotalAmount += (newReservation.ReservationRooms.Sum(x => x.ReservationRoomsTotalAmount) ?? 0);
                        newReservation.RoomsTax += (newReservation.ReservationRooms.Sum(x => x.TotalTax) ?? 0);
                        newReservation.NumberOfRooms = newReservation.ReservationRooms.Count;

                        // for now, and according to the specified on the Bug 95295 we will only change the PropertyBaseCurrencyExchangeRate
                        // when we modify reservations with only one room
                        if (oldReservation.ReservationRooms.Count <= 1)
                        {
                            newReservation.PropertyBaseCurrencyExchangeRate = exchangeRate;
                        }

                        #endregion RESERVATION UPDATE

                        #region LOYALTY LEVEL RESERVATION VALIDATION

                        if (request.GuestLoyaltyLevelId != null && validationRequest.GroupRule.BusinessRules.HasFlag(BusinessRules.LoyaltyDiscount))
                        {
                            loyaltyLevelValidationCriteria.IsForReservationActive = loyaltyLevel.IsForReservationActive;
                            loyaltyLevelValidationCriteria.IsLimitsForPeriodicityActive = false;
                            reservationValidatorPoco.ValidateGuestLoyaltyLevel(loyaltyLevelValidationCriteria);
                        }

                        #endregion

                        #region ALLOTMENT AND INVENTORY

                        // If RoomType or CheckIn/CheckOut change, inventory and allotment calculations are needed
                        if (validationRequest.ValidateAllotment)
                        {
                            var updateAllotmentAndInventoryParams = new List<UpdateAllotmentAndInventoryDayParameters>();

                            // Old Days
                            var oldDays = oldReservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).Select(x => x.Date.Date);
                            if (oldDays.Any())
                            {
                                foreach (var groupByRateRoom in oldReservation.ReservationRooms.GroupBy(x => new { x.RoomType_UID, x.Rate.AvailabilityType }))
                                {
                                    var rrds = groupByRateRoom.SelectMany(x => x.ReservationRoomDetails).ToList();
                                    foreach (var decrementDay in rrds.GroupBy(x => x.Date.Date))
                                    {
                                        updateAllotmentAndInventoryParams.Add(new UpdateAllotmentAndInventoryDayParameters()
                                        {
                                            Day = decrementDay.Key,
                                            RoomTypeUID = groupByRateRoom.Key.RoomType_UID ?? 0,
                                            AddQty = -decrementDay.Count(),
                                            RateRoomDetailUID = decrementDay.First().RateRoomDetails_UID ?? 0,
                                            RateAvailabilityType = groupByRateRoom.Key.AvailabilityType
                                        });
                                    }
                                }
                            }

                            // New Days
                            var newDays = newReservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).Select(x => x.Date.Date).Distinct();
                            if (newDays.Any())
                            {
                                var ratesRepo = RepositoryFactory.GetOBRateRepository();
                                var availabilityrequest = new OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest
                                {
                                    RatesUIDs = newReservation.ReservationRooms.Where(x => x.Rate_UID.HasValue).Select(x => x.Rate_UID.Value).Distinct().ToList()
                                };
                                Dictionary<long, int> rateAvailabilityTypes = ratesRepo.ListRatesAvailablityType(availabilityrequest);

                                foreach (var groupByRateRoom in newReservation.ReservationRooms.GroupBy(x => new { x.RoomType_UID, AvailabilityType = x.Rate_UID.HasValue ? rateAvailabilityTypes[x.Rate_UID.Value] : 0 }))
                                {
                                    var rrds = groupByRateRoom.SelectMany(x => x.ReservationRoomDetails).ToList();
                                    foreach (var incrementDay in rrds.GroupBy(x => x.Date.Date))
                                    {
                                        var rrd = incrementDay.First();

                                        updateAllotmentAndInventoryParams.Add(new UpdateAllotmentAndInventoryDayParameters()
                                        {
                                            Day = incrementDay.Key,
                                            RoomTypeUID = groupByRateRoom.Key.RoomType_UID ?? 0,
                                            AddQty = incrementDay.Count(),
                                            RateRoomDetailUID = rrd.RateRoomDetails_UID ?? 0,
                                            RateAvailabilityType = groupByRateRoom.Key.AvailabilityType
                                        });
                                    }
                                }
                            }

                            // Updates Allotment and Inventory
                            if (updateAllotmentAndInventoryParams.Any())
                            {
                                bool validate = groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ValidateAllotment);
                                inventories = ValidateUpdateReservationAllotmentAndInventory(groupRule, updateAllotmentAndInventoryParams, validate);
                            }
                        }

                        #endregion ALLOTMENT AND INVENTORY

                        #region EXTERNAL SELLING TOTALS
                        if (rules != null && rules.Any())
                        {
                            #region Copy Prices to Additional Data
                            foreach (var rule in rules)
                            {
                                var externalSellingResInfo = reservationAdditionalDataJsonObj.ExternalSellingReservationInformationByRule.Where(x => (int)x.KeeperType == rule.KeeperType).FirstOrDefault();
                                var allExternalSellingInformationByRule = reservationAdditionalDataJsonObj.ReservationRoomList.Select(x => x.ExternalSellingInformationByRule).ToList();
                                var externalSellingInformationByRule = allExternalSellingInformationByRule.Select(x => x.FirstOrDefault(j => (int)j.KeeperType == rule.KeeperType)).ToList();
                                externalSellingInformationByRule = externalSellingInformationByRule.Where(x => x != null).ToList();
                                if (externalSellingResInfo != null && externalSellingInformationByRule != null && externalSellingInformationByRule.Any())
                                {
                                    externalSellingResInfo.TotalAmount = externalSellingInformationByRule.Sum(x => x.ReservationRoomsTotalAmount);
                                    externalSellingResInfo.RoomsPriceSum = externalSellingInformationByRule.Sum(x => x.ReservationRoomsPriceSum);
                                    externalSellingResInfo.RoomsTotalAmount = externalSellingResInfo.TotalAmount;
                                    externalSellingResInfo.TotalTax = externalSellingInformationByRule.Sum(x => x.TotalTax);
                                    externalSellingResInfo.RoomsTax = externalSellingResInfo.TotalTax;
                                    externalSellingResInfo.TotalPOTax = externalSellingInformationByRule.Sum(x => x.TaxesPerDay.Sum(y => y.Price));

                                    #region MAP EXTERNAL TOTAL COMMISSION
                                    externalSellingResInfo.TotalCommission =
                                        rule.CommissionIsPercentage ?
                                        reservationPricesPOCO.CalculateExternalCommission(rule.Commission, rule.CurrencyBaseUID, null, newReservation.Property_UID, externalSellingResInfo.TotalAmount)
                                        : rule.Commission;
                                    #endregion
                                }
                            }
                            #endregion Copy Prices to Additional Data

                            //apply currency exchange to reservation additional data external selling totals
                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ConvertValuesToPropertyCurrency))
                                reservationHelper.ApplyCurrencyExchangeToReservationForConnectors(newReservation, null, null,
                                    reservationAdditionalDataJsonObj, true, request.ReservationRooms.Select(x => x.Number).ToList());
                        }
                        else
                        {
                            //clean externalselling struct if there no rule
                            reservationAdditionalDataJsonObj.ExternalSellingReservationInformationByRule = null;
                            reservationAdditionalDataJsonObj.ReservationRoomList.ForEach(x => x.ExternalSellingInformationByRule = null);
                        }
                        #endregion

                        #region MAP RESERVATION ADDITIONAL DATA
                        reservationAdditionalData.ReservationAdditionalDataJSON = reservationAdditionalDataJsonObj.ToJSON();
                        #endregion

                        // if total reservation as changed update Credits and payment gateway
                        if (newReservation.TotalAmount != oldReservation.TotalAmount)
                        {
                            #region TPI / OPERATORS CREDITS

                            string channelName;
                            decimal creditLimit = 0;
                            var creditValue = (newReservation.TotalAmount - oldReservation.TotalAmount) ?? 0;

                            // Operator Credit
                            reservationHelper.UpdateOperatorCreditUsed(newReservation.Property_UID, newReservation.Channel_UID, newReservation.PaymentMethodType_UID,
                                false, creditValue, out sendOperatorCreditLimitExcededEmail, out channelName, out creditLimit);

                            // Insert Information in Reservation Internal Notes
                            if (sendOperatorCreditLimitExcededEmail)
                                newReservation.InternalNotes += System.Environment.NewLine + string.Format(Resources.Resources.lblCreditLimitReached, channelName, creditLimit);

                            // Tpi Credit
                            reservationHelper.UpdateTPICreditUsed(newReservation.Property_UID, newReservation.TPI_UID, newReservation.PaymentMethodType_UID,
                                creditValue, out sendTpiCreditLimitExcededEmail);

                            #endregion TPI / OPERATORS CREDITS

                            #region PAYMENT GATEWAY

                            if (validationRequest.GroupRule.BusinessRules.HasFlag(BusinessRules.HandlePaymentGateway) && !string.IsNullOrEmpty(newReservation.PaymentGatewayName))
                            {
                                #region REFUND PAYMENT / CANCEL PAYMENT

                                var domainReservation = BusinessObjectToDomainTypeConverter.Convert(oldReservation, new ListReservationRequest
                                {
                                    IncludeReservationPaymentDetail = true,
                                });
                                var resultCode = this.RefundPaymentGateway(domainReservation,
                                    (decimal)(oldReservation.TotalAmount / oldReservation.PropertyBaseCurrencyExchangeRate),
                                    request.SkipInterestCalculation,
                                    oldReservation.ReservationRooms.Count);

                                if (resultCode < 0)
                                {
                                    throw new BusinessLayerException("Error processing payment", Contracts.Responses.ErrorType.PaymentGatewayError, -545);
                                }

                                #endregion REFUND PAYMENT / CANCEL PAYMENT

                                #region REMAKE PAYMENT WITH NEW VALUE

                                var guest = new contractsCRMOB.Guest
                                {
                                    FirstName = newReservation.GuestFirstName,
                                    LastName = newReservation.GuestLastName,
                                    Email = newReservation.GuestEmail
                                };

                                var paymentReturn = this.MakePaymentWithPaymentGateway(guest, oldReservation.ReservationPaymentDetail, newReservation, request.SkipInterestCalculation, promotionalCode: validPromocodeResponse?.PromoCodeObj);
                                if (paymentReturn < 0)
                                    throw new PaymentGatewayException("Error making payment on gateway- Error Code = " + paymentReturn, Contracts.Responses.ErrorType.PaymentGatewayError,
                                        paymentReturn);

                                #endregion REMAKE PAYMENT WITH NEW VALUE
                            }

                            #endregion PAYMENT GATEWAY
                        }
                    }

                    newReservation.Status = reservationStatus;

                    unitOfWork.Save();
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                throw Errors.OperationWasInterrupted.ToBusinessLayerException();
            }
            catch (OptimisticConcurrencyException)
            {

                throw Errors.OperationWasInterrupted.ToBusinessLayerException();
            }
            catch (Exception ex)
            {

                ExceptionDispatchInfo.Capture(ex).Throw();
            }
            finally
            {
                throughputLimiter.Release();
            }

            return newReservation;
        }

        /// <summary>
        /// Validates and distribute reservation requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual ReservationBaseResponse ReservationCoordinator(ReservationBaseRequest request, Reservation.BL.Constants.ReservationAction action)
        {
            var response = new ReservationBaseResponse();
            var reservationHelper = Resolve<IReservationHelperPOCO>();
            int currentState = 0;

            try
            {
                long reservationId = 0;
                long hangfireId = 0;
                GroupRule rule = null;
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var rulesRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);

                    if (!request.RuleType.HasValue)
                        throw Errors.RuleTypeIsRequired.ToBusinessLayerException();

                    if (request.ChannelId <= 0)
                        throw Errors.ChannelIdIsRequired.ToBusinessLayerException();

                    rule = rulesRepo.GetGroupRule(RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request));
                    if (rule.BusinessRules.HasFlag(BusinessRules.UseReservationTransactions))
                    {
                        if (!request.TransactionType.HasValue)
                            throw Errors.TransactionTypeIsRequired.ToBusinessLayerException();

                        if (!request.TransactionAction.HasValue)
                            throw Errors.TransactionActionIsRequired.ToBusinessLayerException();

                        currentState = reservationHelper.GetReservationTransactionState(request.TransactionId,
                            request.ChannelId, out reservationId, out hangfireId);

                        Logger.Debug("GetReservationTransactionState - hangfireId : {0}", hangfireId);

                        // Transaction is Cancelled
                        if (currentState == (int)Constants.ReservationTransactionStatus.Cancelled)
                            throw Errors.TransactionIsCancelled.ToBusinessLayerException();
                    }
                }

                switch (action)
                {
                    case Reservation.BL.Constants.ReservationAction.Modify:
                        var tmpRequest = (ModifyReservationRequest)request;
                        tmpRequest.ReservationUid = reservationId == 0 ? tmpRequest.ReservationUid : reservationId;
                        response = ModifyCoordinator(tmpRequest, rule, currentState, hangfireId);
                        break;

                    case Reservation.BL.Constants.ReservationAction.Insert:
                        break;
                    case Reservation.BL.Constants.ReservationAction.Update:
                        break;
                    case Reservation.BL.Constants.ReservationAction.Cancel:
                        break;
                    default:
                        response = new ReservationBaseResponse();
                        break;
                }
                response.Succeed();
            }
            catch (BusinessLayerException ex)
            {
                Logger.Warn(ex, "REQUEST:{0}", request.ToJSON());
                switch (action)
                {
                    case Reservation.BL.Constants.ReservationAction.Modify:
                        response = new ModifyReservationResponse();
                        break;

                    default:
                        response = new ReservationBaseResponse();
                        break;
                }

                response.Errors.Add(new Error(ex.ErrorType, ex.ErrorCode, ex.Message));

                if (currentState > 0)
                    response.TransactionStatus = (Reservation.BL.Constants.ReservationTransactionStatus)currentState;

                response.TransactionId = request.TransactionId;
                response.Failed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                switch (action)
                {
                    case Reservation.BL.Constants.ReservationAction.Modify:
                        response = new ModifyReservationResponse();
                        break;

                    default:
                        response = new ReservationBaseResponse();
                        break;
                }

                var exception = ex as BusinessLayerException;
                if (exception != null)
                {
                    response.Errors.Add(new Error(exception.ErrorType, exception.ErrorCode, exception.Message));
                }
                else
                {
                    response.Errors.Add(new Error(ex));
                    var arguments = new Dictionary<string, object>
                    {
                        {"Action", action.ToString()},
                        {"Request", request},
                        {"Exception", ex}
                    };
                    Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.Critical, arguments);
                }

                if (currentState > 0)
                    response.TransactionStatus = (Reservation.BL.Constants.ReservationTransactionStatus)currentState;

                response.TransactionId = request.TransactionId;
                response.Failed();
            }
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;
            return response;
        }

        /// <summary>
        /// Choose action for the reservation modification
        /// Actions could be:
        /// Modify Reservation
        /// Revert modifications using method UpdateReservation (used when transaction is ignored)
        /// Only update reservation/reservationroom status (used on commit)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupRule"></param>
        /// <param name="currentTransactionState"></param>
        /// <param name="hangfireId"></param>
        /// <returns></returns>
        private ModifyReservationResponse ModifyCoordinator(ModifyReservationRequest request, GroupRule groupRule, int currentTransactionState, long hangfireId)
        {
            var response = new ModifyReservationResponse();
            response.TransactionId = request.TransactionId;
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            var reservationHelper = this.Resolve<IReservationHelperPOCO>();
            Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState;
            int retries = 0;

            var checkAvailability = false;

            #region MODIFY POSSIBLE OPERATIONS

            if (groupRule.BusinessRules.HasFlag(BusinessRules.UseReservationTransactions))
            {
                nextTransactionState = ReservationStateMachine.GetNextReservationTransactionState(Reservation.BL.Constants.ReservationAction.Modify, request.TransactionType.Value,
                    request.TransactionAction.Value, (Reservation.BL.Constants.ReservationTransactionStatus)currentTransactionState);

                // Type B Retry
                if (request.TransactionType == Reservation.BL.Constants.ReservationTransactionType.B)
                {
                    // Retries Exceeded
                    if (!reservationHelper.UpdateReservationTransactionRetries(request.TransactionId, request.ChannelId, out retries))
                    {

                        // if reservation is pending ignore the modifications and cancel transaction
                        if (currentTransactionState == (int)Reservation.BL.Constants.ReservationTransactionStatus.Pending)
                        {
                            IgnoreReservationModification(request.TransactionId, request.ChannelId, groupRule, hangfireId, request.SkipInterestCalculation, request.BigPullAuthRequestor_UID,
                                    requestId: request.RequestId, Reservation.BL.Constants.ReservationTransactionStatus.Commited, Reservation.BL.Constants.ReservationTransactionStatus.Cancelled);
                            reservationHelper.SetJobToUnlockReservationTransaction(request.TransactionId, request.ChannelId, Reservation.BL.Constants.ReservationTransactionStatus.Ignored);
                            throw Errors.TransactionIsCancelled.ToBusinessLayerException();
                        }
                        else
                        {
                            reservationHelper.UpdateReservationTransactionStatus(request.TransactionId, request.ChannelId, Reservation.BL.Constants.ReservationTransactionStatus.Cancelled);
                            reservationHelper.SetJobToUnlockReservationTransaction(request.TransactionId, request.ChannelId, (Reservation.BL.Constants.ReservationTransactionStatus)currentTransactionState);
                            throw Errors.TransactionIsCancelled.ToBusinessLayerException();
                        }
                    }
                    // Retries Not Exceeded
                    else
                    {
                        // If reservation is already on required status, return success
                        if ((request.TransactionAction == Reservation.BL.Constants.ReservationTransactionAction.Initiate
                        || request.TransactionAction == Reservation.BL.Constants.ReservationTransactionAction.Commit
                        || request.TransactionAction == Reservation.BL.Constants.ReservationTransactionAction.Ignore)
                        && currentTransactionState == (int)nextTransactionState)
                        {
                            return response;
                        }
                    }
                }
                else // Type A
                {
                    #region Possible operations for commit action

                    // Reservation is already modified, only update reservation status
                    if (request.TransactionAction == Reservation.BL.Constants.ReservationTransactionAction.Commit)
                    {
                        #region CHECK AVAILABILITY

                        var reservation = new domainReservations.Reservation();
                        using (var unitOfWork = SessionFactory.GetUnitOfWork())
                        {
                            // If the property in the reservation needs to check availability (checkAvailability == true), the reservation can not be inserted in OB before knowing if the other channel manager has inventory available for the reservation!
                            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.IsToPreCheckAvailability))
                            {
                                #region GET RESERVATION

                                var reservationRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                                if (request.ReservationUid > 0)
                                    reservation = reservationRepo.FindByUIDEagerly(request.ReservationUid);

                                else if (!string.IsNullOrEmpty(request.ReservationNumber))
                                    reservation = reservationRepo.FindByReservationNumberAndChannelUID(request.ReservationNumber, request.ChannelId);

                                #endregion GET RESERVATION

                                checkAvailability = CheckAvailability(request.RequestId, reservation);

                                unitOfWork.Save();
                            }
                        }

                        #endregion CHECK AVAILABILITY

                        Logger.Debug("CommitTransactionCoordinator - hangfireId : {0}", hangfireId);
                        response.TransactionStatus = CommitTransactionCoordinator(request.ReservationUid, request.ChannelId, request.TransactionId,
                            Reservation.BL.Constants.ReservationAction.Modify, nextTransactionState, hangfireId, request.UserId, currentTransactionState, groupRule, requestId: request.RequestId, checkAvailability, request.BigPullAuthRequestor_UID);
                        return response;
                    }

                    // Ignore all modification and revert to last modified reservation
                    if (request.TransactionAction == Reservation.BL.Constants.ReservationTransactionAction.Ignore)
                    {
                        return IgnoreTransactionCoordinator(nextTransactionState, request, groupRule, hangfireId, request.UserId);
                    }

                    #endregion
                }
            }
            else
                nextTransactionState = Reservation.BL.Constants.ReservationTransactionStatus.Commited;

            response = ModifyReservation(request, groupRule, nextTransactionState, hangfireId, retries);

            #endregion

            response.TransactionStatus = nextTransactionState;
            response.TransactionId = request.TransactionId;
            return response;
        }

        /// <summary>
        /// All commit possible operations distributer
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="channelId"></param>
        /// <param name="transactionId"></param>
        /// <param name="reservationAction"></param>
        /// <param name="nextTransactionState"></param>
        /// <param name="hangfireId"></param>
        /// <returns></returns>
        private Reservation.BL.Constants.ReservationTransactionStatus CommitTransactionCoordinator(long reservationId, long channelId, string transactionId, Reservation.BL.Constants.ReservationAction reservationAction,
            Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState, long hangfireId, long? userId, int currentTransactionState, GroupRule groupRule, string requestId, bool checkAvailability = false, long? bigPullAuthId = null)
        {
            switch (nextTransactionState)
            {
                case Reservation.BL.Constants.ReservationTransactionStatus.Commited:
                    CommitTransaction(reservationId, channelId, transactionId, Reservation.BL.Constants.ReservationAction.Modify, nextTransactionState, hangfireId, userId, currentTransactionState, requestId, true, null, checkAvailability, bigPullAuthId);
                    break;
                case Reservation.BL.Constants.ReservationTransactionStatus.OnRequestAccepted:
                    // Apenas o Omnibees Pode Aceitar reservas on request
                    if (groupRule.RuleType != domainReservations.RuleType.Omnibees)
                        throw Errors.InvalidReservationTransactionStatus.ToBusinessLayerException();

                    CommitTransactionOnRequest(reservationId, channelId, transactionId, Reservation.BL.Constants.ReservationAction.Modify, nextTransactionState, hangfireId, userId, groupRule, bigPullAuthId, requestId);
                    break;
            }

            return nextTransactionState;
        }

        /// <summary>
        /// All Ignore possible operations distributer
        /// </summary>
        /// <param name="nextTransactionState"></param>
        /// <param name="request"></param>
        /// <param name="groupRule"></param>
        /// <param name="hangfireId"></param>
        /// <returns></returns>
        private ModifyReservationResponse IgnoreTransactionCoordinator(Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState,
                            ModifyReservationRequest request,
                            GroupRule groupRule,
                            long hangfireId,
                            long? userId)
        {
            var response = new ModifyReservationResponse();
            response.TransactionId = request.TransactionId;

            switch (nextTransactionState)
            {
                case Reservation.BL.Constants.ReservationTransactionStatus.Ignored:
                    contractsReservations.Reservation reservation = IgnoreReservationModification(request.TransactionId, request.ChannelId, groupRule, hangfireId, request.SkipInterestCalculation, request.BigPullAuthRequestor_UID, requestId: request.RequestId);

                    if (reservation == null)
                        response.Failed();
                    else
                    {
                        response.TransactionStatus = Reservation.BL.Constants.ReservationTransactionStatus.Ignored;
                        response.Reservation = reservation;
                    }
                    break;
                case Reservation.BL.Constants.ReservationTransactionStatus.RefusedOnRequest:
                    IgnoreTransactionOnRequest(request.ReservationUid, request.ChannelId, request.TransactionId, Reservation.BL.Constants.ReservationAction.Modify, nextTransactionState, hangfireId,
                        userId, groupRule, request.SkipInterestCalculation, request.RequestId);
                    break;
            }

            return response;
        }



        /// <summary>
        /// Commit reservation transaction
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="transactionId"></param>
        /// <param name="nextTransactionState"></param>
        private void CommitTransaction(long reservationId, long channelId, string transactionId, Reservation.BL.Constants.ReservationAction reservationAction,
            Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState, long hangfireId, long? userId, int currentTransactionState, string requestId, bool addBackgroundTasks = true, 
            string PaymentGatewayOrderId = null, bool checkAvailability = false, long? bigPullAuthId = null)
        {
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();

            // TODO: Remove When Reservations On Request is passed to Insert (This must happen when insert start to use Transactions)
            if (currentTransactionState == (int)Reservation.BL.Constants.ReservationTransactionStatus.OnRequestAccepted)
                reservationAction = Reservation.BL.Constants.ReservationAction.Insert;

            var reservationStatus = ReservationStateMachine.GetNextReservationState(reservationAction, nextTransactionState);

            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                // Update Reservation Status
                reservationHelper.UpdateReservationStatus(reservationId, reservationStatus, transactionId, nextTransactionState, userId, paymentGatewayOrderId: PaymentGatewayOrderId);
                // Update Transaction Status
                reservationHelper.UpdateReservationTransactionStatus(transactionId, channelId, nextTransactionState);
            }

            // Update Reservation History json
            if (addBackgroundTasks)
            {
                QueueBackgroundWork(() =>
                {
                    reservationHelper.DeleteHangfireJob(hangfireId);
                    CommitTransactionBackgroundWork(reservationId, channelId, transactionId, nextTransactionState, hangfireId, reservationStatus, checkAvailability, bigPullAuthId);
                });
            }
        }

        private void CommitTransactionBackgroundWork(long reservationId, long channelId, string transactionId, Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState,
            long hangfireId, Reservation.BL.Constants.ReservationStatus reservationStatus, bool checkAvailability, long? bigPullAuthId)
        {
            var logger = new Log.DefaultLogger("ReservationBackgroundWork");
            logger.Debug("CommitTransactionBackgroundWork: transactionId: {0} - ReservationUID: {1} - hangfireID: {2} ", transactionId, reservationId, hangfireId);

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    long propertyUID = 0;
                    ReservationLoggingMessage log = null;
                    ReservationLoggingMessage logBeforeChanges = null;
                    var reservationHistoryRepo = RepositoryFactory.GetReservationHistoryRepository(unitOfWork);

                    // Get ReservationHistory
                    var reservationHistory = reservationHistoryRepo.GetQuery(x => x.TransactionUID == transactionId
                            && x.ChannelUID == channelId)
                            .OrderByDescending(x => x.UID).Take(1).FirstOrDefault();

                    if (reservationHistory == null)
                        return;

                    logBeforeChanges = reservationHistory.Message.FromJSON<ReservationLoggingMessage>(); // for new log
                    log = reservationHistory.Message.FromJSON<ReservationLoggingMessage>();

                    if (log == null)
                        return;

                    propertyUID = log.objReservation.Property_UID;
                    log.objReservation.Status = (int)reservationStatus;
                    log.BigPullAuthRequestor_UID = bigPullAuthId;

                    foreach (var item in log.objReservation.ReservationRooms)
                        item.Status = (int)reservationStatus;

                    reservationHistory.Message = log.ToJSON();
                    unitOfWork.Save();


                    // Get PropertyUID if log is invalid
                    if (propertyUID <= 0)
                    {
                        var reservationRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                        propertyUID = reservationRepo.GetQuery(x => x.UID == reservationId).Select(x => x.Property_UID).FirstOrDefault();
                    }

                    AddModifyBookingEmailsToPropertyQueue(reservationId, propertyUID, (long)reservationStatus, (long)Reservation.BL.Constants.ReservationStatus.Pending,
                        nextTransactionState, channelId);

                    #region NOTIFY CONNECTORS

                    //notify connectors async
                    if ((int)reservationStatus == (int)Reservation.BL.Constants.ReservationStatus.Booked
                        || (int)reservationStatus == (int)Reservation.BL.Constants.ReservationStatus.Cancelled
                        || (int)reservationStatus == (int)Reservation.BL.Constants.ReservationStatus.Modified)
                    {
                        short operationCode = 0;
                        switch ((int)reservationStatus)
                        {
                            case 1: operationCode = 1; break;
                            case 2: operationCode = 3; break;
                            case 4: operationCode = 2; break;
                        }

                        this.NotifyConnectors("Reservation", reservationId, propertyUID, operationCode, null, checkAvailability);
                    }

                    #endregion NOTIFY CONNECTORS

                    // NEW LOG
                    this.NewLogReservation(ServiceName.CommitTransaction, Reservation.BL.Constants.AdminUserUID, Reservation.BL.Constants.AdminUserName, log.propertyName, logBeforeChanges.objReservation, log.objReservation);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "CommitTransactionBackgroundWork");

                Dictionary<string, object> arguments = new Dictionary<string, object>();
                arguments.Add("transactionId", transactionId);
                arguments.Add("reservationId", reservationId);
                arguments.Add("channelId", channelId);
                arguments.Add("nextTransactionState", nextTransactionState);
                arguments.Add("hangfireId", hangfireId);

                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments, null,
                    "Ocorreram erros nas tarefas de background do CommitTransaction");
                throw;
            }
        }

        /// <summary>
        /// ingnore onrequest reservation (Change reservation status, reservationroom status, Revert operator/tpi credit, refund payment)
        /// This is used when hotel aprove a onrequest reservation
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="channelId"></param>
        /// <param name="transactionId"></param>
        /// <param name="reservationAction"></param>
        /// <param name="nextTransactionState"></param>
        /// <param name="hangfireId"></param>
        /// <param name="userId"></param>
        private void IgnoreTransactionOnRequest(long reservationId, long channelId, string transactionId, Reservation.BL.Constants.ReservationAction reservationAction,
            Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState, long hangfireId, long? userId, GroupRule groupRule, bool skipInterestCalculation, string requestId)
        {
            var reservationHelper = Resolve<IReservationHelperPOCO>();
            var rsp = this.ListReservations(GenerateListReservationRequest(reservationId)).Result;

            var reservation = rsp?.FirstOrDefault();
            if (reservation == null)
                throw Errors.ReservationDoesNotExist.ToBusinessLayerException();

            var oldReservation = reservation.Clone();

            using (var scope = TransactionManager.BeginTransactionScope(OB.Domain.Reservations.Reservation.DomainScope))
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    #region UPDATE RESERVATION STATUS
                    var reservationStatus = ReservationStateMachine.GetNextReservationState(reservationAction, nextTransactionState);
                    reservation.Status = (int)reservationStatus;

                    // Update Reservation Status
                    reservationHelper.UpdateReservationStatus(reservationId, reservationStatus, transactionId, nextTransactionState, userId, false);
                    // Update Transaction Status
                    reservationHelper.UpdateReservationTransactionStatus(transactionId, channelId, nextTransactionState);
                    #endregion

                    #region TPI / OPERATORS CREDITS

                    string channelName;
                    decimal creditLimit = 0;
                    var creditValue = reservation.TotalAmount * -1 ?? 0;

                    // Operator Credit
                    bool sendOperatorCreditLimitExcededEmail = false;
                    reservationHelper.UpdateOperatorCreditUsed(reservation.Property_UID, reservation.Channel_UID, reservation.PaymentMethodType_UID,
                        false, creditValue, out sendOperatorCreditLimitExcededEmail, out channelName, out creditLimit);

                    // Tpi Credit
                    bool sendTpiCreditLimitExcededEmail = false;
                    reservationHelper.UpdateTPICreditUsed(reservation.Property_UID, reservation.TPI_UID, reservation.PaymentMethodType_UID,
                        creditValue, out sendTpiCreditLimitExcededEmail);

                    #endregion TPI / OPERATORS CREDITS

                    #region PAYMENT GATEWAY
                    if (groupRule.BusinessRules.HasFlag(BusinessRules.HandlePaymentGateway) && !string.IsNullOrEmpty(reservation.PaymentGatewayName))
                    {
                        #region REFUND PAYMENT / CANCEL PAYMENT

                        var domainReservation = BusinessObjectToDomainTypeConverter.Convert(reservation, new ListReservationRequest
                        {
                            IncludeReservationPaymentDetail = true,
                        });
                        var resultCode = this.RefundPaymentGateway(domainReservation, (decimal)(reservation.TotalAmount / reservation.PropertyBaseCurrencyExchangeRate),
                            skipInterestCalculation,
                            reservation.ReservationRooms.Count);

                        if (resultCode < 0)
                        {
                            throw new BusinessLayerException("Error processing payment", Contracts.Responses.ErrorType.PaymentGatewayError, -545);
                        }

                        #endregion REFUND PAYMENT / CANCEL PAYMENT

                        #region REMAKE PAYMENT WITH NEW VALUE

                        var guest = new contractsCRMOB.Guest
                        {
                            FirstName = reservation.GuestFirstName,
                            LastName = reservation.GuestLastName,
                            Email = reservation.GuestEmail
                        };

                        var paymentReturn = this.MakePaymentWithPaymentGateway(guest, reservation.ReservationPaymentDetail, domainReservation, skipInterestCalculation);
                        if (paymentReturn < 0)
                            throw new PaymentGatewayException("Error making payment on gateway- Error Code = " + paymentReturn, Contracts.Responses.ErrorType.PaymentGatewayError,
                                paymentReturn);

                        #endregion REMAKE PAYMENT WITH NEW VALUE

                    }
                    #endregion PAYMENT GATEWAY

                    scope.Complete();
                }
            }

            ModifyReservationLogHandler(reservation, oldReservation, requestId);

            QueueBackgroundWork(() =>
            {
                reservationHelper.DeleteHangfireJob(hangfireId);
                var logger = new Log.DefaultLogger("ReservationBackgroundWork");
                SaveReservationHistory(new ReservationLogRequest
                {
                    Reservation = reservation,
                    OldReservation = oldReservation,
                    UserHostAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostName : string.Empty,
                    ServiceName = ServiceName.ModifyReservation,
                    //UserID = (int)Constants.BookingEngineUserType.Guest,
                    ReservationTransactionId = transactionId,
                    ReservationTransactionState = nextTransactionState,
                    Operation = Operation.Update,
                    MessageType = TypeMessage.ReservationMessage,
                    ReservationAdditionalData = reservation.ReservationAdditionalData,
                    HangfireId = hangfireId
                });
                logger.Debug("Finished IgnoreTransactionOnRequestBackgroundWork: transactionId: {0} - ReservationUID: {1} - hangfire: {2}", transactionId, reservationId, hangfireId);
            });
        }

        /// <summary>
        /// Commit on request reservation (Reserve allotment, change reservation status, reservationroom status, update inventory, notify channels)
        /// This is used when hotel aprove a onrequest reservation
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="channelId"></param>
        /// <param name="transactionId"></param>
        /// <param name="reservationAction"></param>
        /// <param name="nextTransactionState"></param>
        /// <param name="hangfireId"></param>
        /// <param name="userId"></param>
        private void CommitTransactionOnRequest(long reservationId, long channelId, string transactionId, Reservation.BL.Constants.ReservationAction reservationAction,
            Reservation.BL.Constants.ReservationTransactionStatus nextTransactionState, long hangfireId, long? userId, GroupRule groupRule, long? bigPullAuth_UID, string requestId)
        {
            var reservationHelper = Resolve<IReservationHelperPOCO>();
            List<contractsProperties.Inventory> inventories = new List<contractsProperties.Inventory>();

            var rsp = this.ListReservations(GenerateListReservationRequest(reservationId)).Result;

            var reservation = rsp != null ? rsp.FirstOrDefault() : null;
            if (reservation == null)
                throw Errors.ReservationDoesNotExist.ToBusinessLayerException();

            var oldReservation = reservation.Clone();

            using (var scope = TransactionManager.BeginTransactionScope(OB.Domain.Reservations.Reservation.DomainScope))
            {
                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    var daysToIncrement = new List<UpdateAllotmentAndInventoryDayParameters>();
                    foreach (var groupByRateRoom in reservation.ReservationRooms.GroupBy(x => new { x.RoomType_UID, x.Rate.AvailabilityType }))
                    {
                        var rrds = groupByRateRoom.SelectMany(x => x.ReservationRoomDetails).ToList();
                        foreach (var incrementDay in rrds.GroupBy(x => x.Date.Date))
                        {
                            var rrd = incrementDay.First();

                            daysToIncrement.Add(new UpdateAllotmentAndInventoryDayParameters()
                            {
                                Day = incrementDay.Key,
                                RoomTypeUID = groupByRateRoom.Key.RoomType_UID ?? 0,
                                AddQty = incrementDay.Count(),
                                RateRoomDetailUID = rrd.RateRoomDetails_UID ?? 0,
                                RateAvailabilityType = groupByRateRoom.Key.AvailabilityType
                            });
                        }
                    }

                    // Updates Allotment and Inventory
                    inventories = this.ValidateUpdateReservationAllotmentAndInventory(groupRule, daysToIncrement, false);

                    #region UPDATE RESERVATION STATUS

                    // TODO: Remove When Reservations On Request is passed to Insert (This must happen when insert start to use Transactions)
                    if (nextTransactionState == Reservation.BL.Constants.ReservationTransactionStatus.OnRequestAccepted &&
                        reservation.Channel_UID == Reservation.BL.Constants.BookingEngineChannelId)
                    {
                        reservationAction = Reservation.BL.Constants.ReservationAction.Insert;
                        nextTransactionState = Reservation.BL.Constants.ReservationTransactionStatus.Commited;
                    }

                    var reservationStatus = ReservationStateMachine.GetNextReservationState(reservationAction, nextTransactionState);
                    reservation.Status = (int)reservationStatus;
                    reservationHelper.UpdateReservationStatus(reservationId, reservationStatus, transactionId, nextTransactionState, userId, false);

                    // Update Transaction Status
                    reservationHelper.UpdateReservationTransactionStatus(transactionId, channelId, nextTransactionState);

                    #endregion

                    scope.Complete();
                }
            }

            ModifyReservationLogHandler(reservation, oldReservation, requestId, userId, false);

            QueueBackgroundWork(() =>
            {
                reservationHelper.DeleteHangfireJob(hangfireId);

                ModifyReservationBackgroundOperations(new ReservationBackgroundOperationsRequest
                {
                    DecrementInventory = false,
                    NewReservation = reservation,
                    OldReservation = oldReservation,
                    propertyID = reservation.Property_UID,
                    ReservationTransactionId = transactionId,
                    ReservationTransactionState = nextTransactionState,
                    ServiceName = ServiceName.ModifyReservation,
                    UserID = userId ?? 0,
                    UserIsGuest = false,
                    HangfireId = hangfireId,
                    Inventories = inventories,
                    BigPullAuthRequestor_UID = bigPullAuth_UID
                });
            });
        }

        /// <summary>
        /// Revert reservation to last state given in parameters
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="channelId"></param>
        /// <param name="revertToLastStatus">revert to this state</param>
        /// <param name="groupRule"></param>
        /// <returns></returns>
        private contractsReservations.Reservation IgnoreReservationModification(string transactionId, long channelId, GroupRule groupRule, long hangfireId,
            bool skipInterestCalculation, long? bigPullAuthId, string requestId,
            Reservation.BL.Constants.ReservationTransactionStatus revertToLastStatus = Reservation.BL.Constants.ReservationTransactionStatus.Commited,
            Reservation.BL.Constants.ReservationTransactionStatus putTransactionInStatus = Reservation.BL.Constants.ReservationTransactionStatus.Ignored)
        {
            contractsReservations.Reservation finalReservation = null;
            var reservationHelper = Resolve<IReservationHelperPOCO>();

            try
            {
                IgnoreReservationResult ignoreReservationResult = null;
                using (var scope = TransactionManager.BeginTransactionScope(OB.Domain.Reservations.Reservation.DomainScope))
                {
                    ignoreReservationResult = IgnoreReservation(transactionId, channelId, groupRule, hangfireId, skipInterestCalculation, revertToLastStatus, putTransactionInStatus);
                    scope.Complete();
                }

                if (ignoreReservationResult != null)
                {
                    finalReservation = ignoreReservationResult.FinalReservation;

                    ModifyReservationLogHandler(finalReservation, ignoreReservationResult.OldReservation, requestId);

                    QueueBackgroundWork(() =>
                    {
                        reservationHelper.DeleteHangfireJob(hangfireId);

                        ModifyReservationBackgroundOperations(new ReservationBackgroundOperationsRequest
                        {
                            NewReservation = finalReservation,
                            OldReservation = ignoreReservationResult.OldReservation,
                            SendOperatorCreditLimitExcededEmail = ignoreReservationResult.SendOperatorCreditLimitExcededEmail,
                            SendTPICreditLimitExcededEmail = ignoreReservationResult.SendTPICreditLimitExcededEmail,
                            ReservationTransactionId = transactionId,
                            ReservationTransactionState = putTransactionInStatus,
                            ReservationAdditionalDataUID = ignoreReservationResult.Log.ReservationAdditionalDataUID.HasValue ? ignoreReservationResult.Log.ReservationAdditionalDataUID.Value : new Nullable<long>(),
                            ReservationAdditionalData = ignoreReservationResult.AdditionalData,
                            HangfireId = hangfireId,
                            Inventories = ignoreReservationResult.Inventories,
                            BigPullAuthRequestor_UID = bigPullAuthId,
                            ReservationRequest = new ReservationBaseRequest { TransactionAction = ReservationTransactionAction.Ignore }
                        });
                    });
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw Errors.OperationWasInterrupted.ToBusinessLayerException();
            }
            catch (OptimisticConcurrencyException)
            {
                throw Errors.OperationWasInterrupted.ToBusinessLayerException();
            }
            catch (Exception ex)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return finalReservation;
        }

        private IgnoreReservationResult IgnoreReservation(string transactionId, long channelId, GroupRule groupRule, long hangfireId, bool skipInterestCalculation,
           Reservation.BL.Constants.ReservationTransactionStatus revertToLastStatus = Reservation.BL.Constants.ReservationTransactionStatus.Commited,
           Reservation.BL.Constants.ReservationTransactionStatus putTransactionInStatus = Reservation.BL.Constants.ReservationTransactionStatus.Ignored)
        {
            IgnoreReservationResult ignoreReservation = null;
            contractsReservations.Reservation finalReservation = null;
            contractsReservations.Reservation oldReservation = null;
            IUnitOfWork unitOfWork = null;
            domainReservations.Reservation newReservation = null;
            bool sendOperatorCreditLimitExcededEmail = false;
            bool sendTPICreditLimitExcededEmail = false;
            ReservationLoggingMessage log = null;
            OB.BL.Contracts.Data.CRM.Guest guestToUpdate;
            OB.Domain.Reservations.ReservationsAdditionalData additionalData = null;
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();
            List<contractsProperties.Inventory> inventories = new List<contractsProperties.Inventory>();

            try
            {
                using (unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    var reservationRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                    var reservationAdditionalDataRepo = RepositoryFactory.GetRepository<ReservationsAdditionalData>(unitOfWork);
                    var reservationHistoryRepo = RepositoryFactory.GetReservationHistoryRepository(unitOfWork);

                    var converterParameters = GenerateListReservationRequest();

                    #region GET RESERVATION HISTORY

                    var reservationHistory = reservationHistoryRepo.GetQuery(x => x.TransactionUID == transactionId
                        && x.ChannelUID == channelId && x.TransactionState == (int)revertToLastStatus)
                        .OrderByDescending(x => x.UID).Take(1).FirstOrDefault();

                    if (reservationHistory == null)
                        throw Errors.ErrorProcessingIgnorePending.ToBusinessLayerException();

                    log = reservationHistory.Message.FromJSON<ReservationLoggingMessage>();
                    if (log == null)
                        throw Errors.ErrorProcessingIgnorePending.ToBusinessLayerException();

                    #endregion GET RESERVATION HISTORY

                    #region GET CURRENT RESERVATION

                    newReservation = reservationRepo.FindByUIDEagerly(reservationHistory.ReservationUID.Value);
                    oldReservation = ConvertReservationToContractWithLookups(converterParameters, new List<domainReservations.Reservation>() { newReservation }, null, null).FirstOrDefault();

                    #endregion GET CURRENT RESERVATION

                    //Update Guest Name
                    if (newReservation.GuestFirstName != log.objReservation.GuestFirstName
                        || newReservation.GuestLastName != log.objReservation.GuestLastName)
                    {
                        var crmRepo = RepositoryFactory.GetOBCRMRepository();
                        crmRepo.UpdateGuestName(new Contracts.Requests.UpdateGuestNameRequest
                        {
                            Id = newReservation.Guest_UID,
                            FirstName = log.objReservation.GuestFirstName,
                            LastName = log.objReservation.GuestLastName
                        });
                    }

                    reservationHelper.DeleteReservationRooms(newReservation.ReservationRooms);

                    //TODO: retirar este martelinho quando tivermos o processo de migração no reservationhistory para preencher o rateuid no reservationroomdetails                    
                    log.objReservation.ReservationRooms.ToList().ForEach(x => x.ReservationRoomDetails.Where(j => !j.Rate_UID.HasValue).ToList().ForEach(j => j.Rate_UID = x.Rate_UID));

                    // RECREATE RESERVATION
                    newReservation.ReservationRooms = log.objReservation.ReservationRooms.Select(x => BusinessObjectToDomainTypeConverter.Convert(x, converterParameters)).ToList();
                    newReservation.ModifyDate = DateTime.UtcNow;
                    newReservation.Adults = (int)newReservation.ReservationRooms.Sum(x => x.AdultCount);
                    newReservation.Children = newReservation.ReservationRooms.Sum(x => x.ChildCount);
                    newReservation.TotalTax = (newReservation.ReservationRooms.Sum(x => x.TotalTax) ?? 0);
                    newReservation.TotalAmount = (newReservation.ReservationRooms.Sum(x => x.ReservationRoomsTotalAmount));
                    newReservation.RoomsPriceSum = (newReservation.ReservationRooms.Sum(x => x.ReservationRoomsPriceSum) ?? 0);
                    newReservation.RoomsTotalAmount = (newReservation.ReservationRooms.Sum(x => x.ReservationRoomsTotalAmount) ?? 0);
                    newReservation.RoomsTax = (newReservation.ReservationRooms.Sum(x => x.TotalTax) ?? 0);
                    newReservation.NumberOfRooms = newReservation.ReservationRooms.Count;
                    newReservation.Status = log.objReservation.Status;
                    newReservation.GuestFirstName = log.objReservation.GuestFirstName;
                    newReservation.GuestLastName = log.objReservation.GuestLastName;

                    // since we will modify it on the initiiate, acoording to the Bug 95295, we have to revert it on the Ignore
                    if (newReservation.NumberOfRooms <= 1 && log.objReservation.PropertyBaseCurrencyExchangeRate.HasValue)
                        newReservation.PropertyBaseCurrencyExchangeRate = log.objReservation.PropertyBaseCurrencyExchangeRate;

                    // Revert Reservation Addicional Data
                    if (log.ReservationAdditionalDataUID.HasValue && log.ReservationAdditionalData != null)
                    {
                        additionalData = new ReservationsAdditionalData
                        {
                            UID = log.ReservationAdditionalDataUID.Value,
                            Reservation_UID = newReservation.UID,
                            ChannelPartnerID = log.ReservationAdditionalData.ChannelPartnerID,
                            IsFromNewInsert = true,
                            PartnerReservationNumber = log.ReservationAdditionalData.PartnerReservationNumber,
                            ReservationAdditionalDataJSON = log.ReservationAdditionalData.ToJSON(),
                            BigPullAuthRequestor_UID = log.ReservationAdditionalData?.BigPullAuthRequestor_UID,
                            BigPullAuthOwner_UID = log.ReservationAdditionalData?.BigPullAuthOwner_UID
                        };

                        reservationAdditionalDataRepo.AttachAsModified(additionalData);
                    }

                    #region ALLOTMENT AND INVENTORY

                    var ratesRepo = RepositoryFactory.GetOBRateRepository();


                    // Get Rates Availability Types
                    var rateUIDs = newReservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).Where(x => x.Rate_UID > 0).Select(x => x.Rate_UID.Value).ToList();
                    rateUIDs.AddRange(oldReservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).Where(x => x.Rate_UID > 0).Select(x => x.Rate_UID.Value).ToList());
                    Dictionary<long, int> rateAvailabilityTypes = ratesRepo.ListRatesAvailablityType(new OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest { RatesUIDs = rateUIDs.Distinct().ToList() });

                    var updateAllotmentAndInventoryParams = new List<UpdateAllotmentAndInventoryDayParameters>();

                    // Old Days
                    var oldDays = oldReservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).Select(x => x.Date.Date).Distinct();
                    if (oldDays.Any())
                    {
                        foreach (var groupByRateRoom in oldReservation.ReservationRooms.GroupBy(x => new { x.RoomType_UID, AvailabilityType = x.Rate_UID.HasValue ? rateAvailabilityTypes[x.Rate_UID.Value] : 0 }))
                        {
                            var rrds = groupByRateRoom.SelectMany(x => x.ReservationRoomDetails).ToList();
                            foreach (var decrementDay in rrds.GroupBy(x => x.Date.Date))
                            {
                                var rrd = decrementDay.First();

                                updateAllotmentAndInventoryParams.Add(new UpdateAllotmentAndInventoryDayParameters()
                                {
                                    Day = decrementDay.Key,
                                    RoomTypeUID = groupByRateRoom.Key.RoomType_UID ?? 0,
                                    AddQty = -decrementDay.Count(),
                                    RateRoomDetailUID = rrd.RateRoomDetails_UID ?? 0,
                                    RateAvailabilityType = groupByRateRoom.Key.AvailabilityType
                                });
                            }
                        }
                    }

                    // New Days
                    var newDays = newReservation.ReservationRooms.SelectMany(x => x.ReservationRoomDetails).Select(x => x.Date.Date).Distinct();
                    if (newDays.Any())
                    {
                        foreach (var groupByRateRoom in newReservation.ReservationRooms.GroupBy(x => new { x.RoomType_UID, AvailabilityType = x.Rate_UID.HasValue ? rateAvailabilityTypes[x.Rate_UID.Value] : 0 }))
                        {
                            var rrds = groupByRateRoom.SelectMany(x => x.ReservationRoomDetails).ToList();
                            foreach (var incrementDay in rrds.GroupBy(x => x.Date.Date))
                            {
                                var rrd = incrementDay.First();

                                updateAllotmentAndInventoryParams.Add(new UpdateAllotmentAndInventoryDayParameters()
                                {
                                    Day = incrementDay.Key,
                                    RoomTypeUID = groupByRateRoom.Key.RoomType_UID ?? 0,
                                    AddQty = incrementDay.Count(),
                                    RateRoomDetailUID = rrd.RateRoomDetails_UID ?? 0,
                                    RateAvailabilityType = groupByRateRoom.Key.AvailabilityType
                                });
                            }
                        }
                    }

                    // Updates Allotment and Inventory
                    if (updateAllotmentAndInventoryParams.Any())
                    {
                        bool validate = groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ValidateAllotment);
                        inventories = this.ValidateUpdateReservationAllotmentAndInventory(groupRule, updateAllotmentAndInventoryParams, validate);
                    }

                    #endregion ALLOTMENT AND INVENTORY

                    // if total reservation as changed update Credits and payment gateway
                    if (newReservation.TotalAmount != oldReservation.TotalAmount)
                    {
                        #region TPI / OPERATORS CREDITS

                        string channelName;
                        decimal creditLimit = 0;
                        var creditValue = (newReservation.TotalAmount - oldReservation.TotalAmount) ?? 0;

                        // Operator Credit
                        reservationHelper.UpdateOperatorCreditUsed(newReservation.Property_UID, newReservation.Channel_UID, newReservation.PaymentMethodType_UID,
                            false, creditValue, out sendOperatorCreditLimitExcededEmail, out channelName, out creditLimit);

                        // Insert Information in Reservation Internal Notes
                        if (sendOperatorCreditLimitExcededEmail)
                            newReservation.InternalNotes += System.Environment.NewLine + string.Format(Resources.Resources.lblCreditLimitReached, channelName, creditLimit);

                        // Tpi Credit
                        reservationHelper.UpdateTPICreditUsed(newReservation.Property_UID, newReservation.TPI_UID, newReservation.PaymentMethodType_UID,
                            creditValue, out sendTPICreditLimitExcededEmail);

                        #endregion TPI / OPERATORS CREDITS

                        #region PAYMENT GATEWAY

                        if (groupRule.BusinessRules.HasFlag(BusinessRules.HandlePaymentGateway))
                        {
                            if (!string.IsNullOrEmpty(newReservation.PaymentGatewayName))
                            {
                                #region REFUND PAYMENT / CANCEL PAYMENT

                                var domainReservation = BusinessObjectToDomainTypeConverter.Convert(oldReservation, new ListReservationRequest
                                {
                                    IncludeReservationPaymentDetail = true,
                                });
                                var resultCode = this.RefundPaymentGateway(domainReservation,
                                    (decimal)(oldReservation.TotalAmount / oldReservation.PropertyBaseCurrencyExchangeRate),
                                    skipInterestCalculation,
                                    oldReservation.ReservationRooms.Count);

                                if (resultCode < 0)
                                    throw Errors.ErrorProcessingPayment.ToBusinessLayerException();

                                #endregion REFUND PAYMENT / CANCEL PAYMENT

                                #region REMAKE PAYMENT WITH NEW VALUE

                                var guest = new contractsCRMOB.Guest
                                {
                                    FirstName = newReservation.GuestFirstName,
                                    LastName = newReservation.GuestLastName,
                                    Email = newReservation.GuestEmail
                                };

                                var paymentReturn = this.MakePaymentWithPaymentGateway(guest, oldReservation.ReservationPaymentDetail, newReservation, skipInterestCalculation);
                                if (paymentReturn < 0)
                                    throw Errors.ErrorProcessingPayment.ToBusinessLayerException();

                                #endregion REMAKE PAYMENT WITH NEW VALUE
                            }
                        }

                        #endregion PAYMENT GATEWAY
                    }

                    reservationHelper.UpdateReservationTransactionStatus(transactionId, channelId, putTransactionInStatus);

                    unitOfWork.Save();

                    converterParameters.IncludeExtras = true;
                    converterParameters.IncludeRoomTypes = true;
                    converterParameters.IncludeIncentives = true;
                    finalReservation = ConvertReservationToContractWithLookups(converterParameters, new List<domainReservations.Reservation>() { newReservation }).FirstOrDefault();
                    finalReservation.ReservationAdditionalData = log.ReservationAdditionalData;

                    #region RESERVATION FILTERS
                    reservationHelper.ModifyReservationFilter(finalReservation, ServiceName.IgnoreTransaction);
                    #endregion RESERVATION FILTERS

                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw Errors.OperationWasInterrupted.ToBusinessLayerException();
            }
            catch (OptimisticConcurrencyException)
            {
                throw Errors.OperationWasInterrupted.ToBusinessLayerException();
            }
            catch (Exception ex)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            ignoreReservation = new IgnoreReservationResult { FinalReservation = finalReservation, OldReservation = oldReservation, SendTPICreditLimitExcededEmail = sendTPICreditLimitExcededEmail, SendOperatorCreditLimitExcededEmail = sendOperatorCreditLimitExcededEmail, AdditionalData = additionalData, Inventories = inventories, Log = log };
            return ignoreReservation;
        }



        /// <summary>
        /// Background Tasks for the modification process
        /// </summary>
        /// <param name="request"></param>
        private void ModifyReservationBackgroundOperations(ReservationBackgroundOperationsRequest request, bool checkAvailability = false)
        {
            var logger = new Log.DefaultLogger("ReservationBackgroundWork");
            logger.Debug("ModifyReservationBackgroundOperations: RequestGuid: {0} - ReservationUID: {1} - hangfireId: {2} - serviceName: {3} ", request.ReservationRequest?.RequestGuid != null ? request.ReservationRequest.RequestGuid.ToString() : string.Empty, request.NewReservation.UID, request.HangfireId, request.ServiceName);

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
                    var throughputLimiterPOCO = this.Resolve<IThroughputLimiterManagerPOCO>();
                    var reservationHelper = this.Resolve<IReservationHelperPOCO>();
                    var random = new Random();

                    #region INVENTORY / REAL ALLOTMENT / OCCUPANCY LEVELS / CLOSE SALES

                    List<OB.BL.Contracts.Data.Properties.Inventory> inventoriesToUpdateIncrement = new List<OB.BL.Contracts.Data.Properties.Inventory>();

                    // Create New Inventory and allotment
                    if (request.IncrementInventory)
                    {
                        foreach (var rr in request.NewReservation.ReservationRooms)
                        {
                            foreach (var rrd in rr.ReservationRoomDetails)
                            {
                                inventoriesToUpdateIncrement.Add(new OB.BL.Contracts.Data.Properties.Inventory
                                {
                                    Date = rrd.Date,
                                    QtyRoomOccupied = 1,
                                    RoomType_UID = rr.RoomType_UID.Value
                                });
                            }
                        }
                    }

                    var throughputLimiter = throughputLimiterPOCO.GetThroughputLimiter(_CLOSE_SALES_THROUGHPUTLIMITER_KEY,
                                _INSERT_RESERVATION_MAX_THREADS, _INSERT_RESERVATION_MAX_THREADS);

                    throughputLimiter.Wait();
                    try
                    {
                        #region SEND ALLOTMENT FOR CHANNELS

                        SendAllotmentAndInventoryForChannels(request.NewReservation.Property_UID, request.NewReservation.Channel_UID ?? 0, null,
                            request.OldReservation.ReservationRooms, request.NewReservation.ReservationRooms, ServiceName.ModifyReservation, request.Inventories,
                            request.DecrementInventory, request.IncrementInventory);

                        #endregion SEND ALLOTMENT FOR CHANNELS

                        #region OCCUPANCY LEVELS / CLOSE SALES

                        var occLevelsResult = false;
                        if (ConfigurationManager.AppSettings["OccupancyLevelsEnabled"] != null && ConfigurationManager.AppSettings["OccupancyLevelsEnabled"].ToLowerInvariant() == "true")
                        {
                            Stopwatch watch = new Stopwatch();
                            watch.Start();
                            occLevelsResult = propertyRepo.ApplyOccupancyLevels(new Contracts.Requests.ApplyOccupancyLevelsRequest
                            {
                                InventoryToCheck = inventoriesToUpdateIncrement,
                                PropertyUID = request.NewReservation.Property_UID,
                                ReservationUID = request.NewReservation.UID,
                                Operation = "ModifyReservation",
                                GeneratedBy = 1
                            });
                            Logger.Info(string.Format("-------- ApplyOccupancyLevels took: {0} ms", watch.ElapsedMilliseconds));
                            watch.Stop();
                        }

                        // Close Sales
                        // Only do close sales if occupancy alert are not applied
                        if (!occLevelsResult)
                            Retry.Execute(() => this.CloseSales(request.NewReservation), () => TimeSpan.FromMilliseconds(random.Next(300, 5000)), 5,
                                        "ModifyReservation - Close Sales", false);

                        #endregion OCCUPANCY LEVELS / CLOSE SALES
                    }
                    catch (Exception ex)
                    {
                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }
                    finally
                    {
                        throughputLimiter.Release();
                    }

                    #endregion INVENTORY / REAL ALLOTMENT / OCCUPANCY LEVELS / CLOSE SALES

                    #region CREATE PROPERTYQUEUE EMAILS

                    AddModifyBookingEmailsToPropertyQueue(request.NewReservation.UID, request.propertyID, request.NewReservation.Status, request.OldReservation.Status,
                        request.ReservationTransactionState, request.NewReservation.Channel_UID);

                    // Occupancy Alerts Email
                    this.InsOccupancyAlertsInPropertyQueue(request.NewReservation.UID, request.NewReservation.Property_UID, request.NewReservation.Channel_UID, true);

                    if (request.SendTPICreditLimitExcededEmail)
                        this.InsertPropertyQueue(request.NewReservation.Property_UID, (long)Constants.SystemEventsCode.CreditLimit, request.NewReservation.TPI_UID.Value, false);

                    if (request.SendOperatorCreditLimitExcededEmail)
                        this.InsertPropertyQueue(request.NewReservation.Property_UID, (long)Constants.SystemEventsCode.CreditLimit, request.NewReservation.Channel_UID.Value, true);

                    #endregion CREATE PROPERTYQUEUE EMAILS

                    #region NOTIFY CONNECTORS
                    checkAvailability = false;
                    var transactionAction = request.ReservationRequest?.TransactionAction;

                    //notify connectors async
                    if ((request.NewReservation.Status == (int)Constants.ReservationStatus.Booked
                        || request.NewReservation.Status == (int)Constants.ReservationStatus.Cancelled
                        || request.NewReservation.Status == (int)Constants.ReservationStatus.Modified)
                        && !checkAvailability
                        && transactionAction == null)
                    {
                        short operationCode = 0;
                        switch (request.NewReservation.Status)
                        {
                            case 1: operationCode = 1; break;
                            case 2: operationCode = 3; break;
                            case 4: operationCode = 2; break;
                        }

                        this.NotifyConnectors("Reservation", request.NewReservation.UID, request.NewReservation.Property_UID, operationCode, null, checkAvailability);
                    }

                    #endregion NOTIFY CONNECTORS

                    this.MarkReservationsAsViewed(new MarkReservationsAsViewedRequest
                    {
                        Reservation_UIDs = new List<long> { request.NewReservation.UID },
                        User_UID = 65,
                        NewValue = false
                    });

                    #region LOG

                    this.SaveReservationHistory(new ReservationLogRequest
                    {
                        OldReservation = request.OldReservation,
                        Reservation = request.NewReservation,
                        UserHostAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostName : string.Empty,
                        ServiceName = ServiceName.ModifyReservation,
                        ReservationTransactionId = request.ReservationTransactionId,
                        ReservationTransactionState = request.ReservationTransactionState,
                        Operation = Operation.Update,
                        MessageType = TypeMessage.ReservationMessage,
                        ReservationAdditionalDataUID = request.ReservationAdditionalDataUID,
                        ReservationAdditionalData = request.ReservationAdditionalData != null ?
                        request.ReservationAdditionalData.ReservationAdditionalDataJSON.FromJSON<contractsReservations.ReservationsAdditionalData>() : null,
                        HangfireId = request.HangfireId,
                        TransactionRetries = request.TransactionRetries,
                        UserID = request.UserID,
                        BigPullAuthRequestor_UID = request.BigPullAuthRequestor_UID
                    });

                    #endregion LOG

                    unitOfWork.Save();
                }

                #region TRIPADVISOR REVIEW

                if (request.NewReservation.ReservationRooms != null && request.NewReservation.ReservationRooms.Any() && request.NewReservation.ReservationRooms.FirstOrDefault().DateTo.HasValue
                    && request.NewReservation.Channel_UID == (int)Constants.BookingEngineChannelId)
                {
                    SetTripAdvisorReview(request.NewReservation.Property_UID, request.NewReservation.UID, request.NewReservation.GuestEmail,
                                request.NewReservation.ReservationRooms.FirstOrDefault().DateTo.Value, request.NewReservation.GuestCountry_UID, (int)Constants.TripAdvisorReviewAction.Update,
                                request.NewReservation.ReservationLanguageUsed_UID, request.NewReservation.ReservationRooms.FirstOrDefault().DateFrom);
                }

                #endregion TRIPADVISOR REVIEW
            }
            catch (Exception ex)
            {
                logger.Error(ex, "ModifyReservationBackgroundOperations");

                Dictionary<string, object> arguments = new Dictionary<string, object>();
                arguments.Add("RequestGuid", request.ReservationRequest != null ? request.ReservationRequest.RequestGuid.ToString() : string.Empty);
                arguments.Add("NewReservation", request.NewReservation);
                arguments.Add("OldReservation", request.OldReservation);
                arguments.Add("PropertyID", request.propertyID);
                arguments.Add("hangfireId", request.HangfireId);
                arguments.Add("ServiceName", request.ServiceName);

                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments, null,
                    "Ocorreram erros nas tarefas de background do ModifyReservation");
                throw;
            }
        }

        private void ModifyReservationLogHandler(contractsReservations.Reservation contractReservation, contractsReservations.Reservation reservationBeforeUpdate, string requestId, long? userId = null, bool userIsGuest = false)
        {
            var userUid = userId ?? 0;
            var userName = string.Empty;
            if (userUid <= 0 && !userIsGuest)
            {
                userUid = Constants.AdminUserUID;
                userName = Constants.AdminUserName;
            }

            this.NewLogReservation(OB.Reservation.BL.Contracts.Data.General.ServiceName.ModifyReservation,
                userUID: userUid,
                userName: userName,
                propertyName: string.Empty,
                oldReservation: reservationBeforeUpdate,
                newReservation: contractReservation,
                requestId: requestId);
        }

        public void AddModifyBookingEmailsToPropertyQueue(long reservationId, long propertyId, long newStatus, long oldStatus,
            ReservationTransactionStatus reservationTransactionState, long? channelId)
        {
            if (newStatus == (int)OB.Reservation.BL.Constants.ReservationStatus.OnRequestAccepted)
                return;

            //sabre US 3948
            if (newStatus != (int)Reservation.BL.Constants.ReservationStatus.Pending && reservationTransactionState != ReservationTransactionStatus.Ignored)
            {
                // bug 12848 - new reservation
                if (oldStatus == (int)Reservation.BL.Constants.ReservationStatus.Pending && newStatus == (int)Reservation.BL.Constants.ReservationStatus.Booked)
                    InsertPropertyQueue(propertyId, reservationId, channelId,
                        (long)Reservation.BL.Constants.SystemEventsCode.NewBookingArrived);
                else if (newStatus == (int)Reservation.BL.Constants.ReservationStatus.BookingOnRequest)
                    InsertPropertyQueue(propertyId, reservationId, channelId, (long)Reservation.BL.Constants.SystemEventsCode.OnRequestBooking);
                else if (oldStatus == (int)Reservation.BL.Constants.ReservationStatus.BookingOnRequest && newStatus == (int)Reservation.BL.Constants.ReservationStatus.Booked)
                    InsertPropertyQueue(propertyId, reservationId, channelId,
                       (long)Reservation.BL.Constants.SystemEventsCode.NewBookingArrived);
                else
                    InsertPropertyQueue(propertyId, reservationId, channelId,
                        (long)Reservation.BL.Constants.SystemEventsCode.BookingChanged);
            }
        }

        /// <summary>
        /// Update Reservation Transaction Status
        /// </summary>
        /// <returns></returns>
        public void UpdateReservationTransactionStatus(UpdateReservationTransactionStatusRequest request)
        {
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();
            reservationHelper.UpdateReservationTransactionStatus(request.TransactionId, request.ChannelId, request.ReservationTransactionStatus);
        }

        #region Modify Reservation
        private contractsEvents.EntityDelta LoggingReservationRoomExtras(long reservationRoomUID, contractsEvents.EntityState entityState, contractsReservations.ReservationRoomExtra reservationRoomExtra, contractsReservations.ReservationRoomExtra oldReservationRoomExtra = null)
        {
            if (reservationRoomExtra == null)
                return new contractsEvents.EntityDelta();

            var entitiesProperties = new contractsEvents.Portable.Infrastructure.PropertyDictionary();
            entitiesProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ReservationRooms_UID, reservationRoomUID); // Room Name

            // Extra Name
            if (reservationRoomExtra.Extra != null)
            {
                entitiesProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Name,
                    reservationRoomExtra.Extra.Name,
                    (oldReservationRoomExtra != null) ? oldReservationRoomExtra.Extra.Name : null);
            }

            // Quantity
            entitiesProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Quantity,
                reservationRoomExtra.Qty,
                (oldReservationRoomExtra != null) ? (short?)oldReservationRoomExtra.Qty : null);

            // Schedule
            var schedules = (reservationRoomExtra.ReservationRoomExtrasSchedules != null && reservationRoomExtra.ReservationRoomExtrasSchedules.Count > 0) ?
                reservationRoomExtra.ReservationRoomExtrasSchedules.Select(x => x.Date).ToList() : null;
            var oldSchedules = (oldReservationRoomExtra != null && oldReservationRoomExtra.ReservationRoomExtrasSchedules != null && oldReservationRoomExtra.ReservationRoomExtrasSchedules.Count > 0) ?
                oldReservationRoomExtra.ReservationRoomExtrasSchedules.Select(x => x.Date).ToList() : null;
            entitiesProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Schedule, schedules, oldSchedules);

            var entityDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.ReservationRoomExtra)
            {
                EntityKey = reservationRoomExtra.Extra_UID,
                EntityState = entityState,
                EntityProperties = entitiesProperties
            };

            var privilegedProperties = new List<EntityPropertyEnum>()
            {
                EntityPropertyEnum.Name,
                EntityPropertyEnum.ReservationRooms_UID
            };
            return contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(entityDelta, privilegedProperties, true);
        }

        private List<contractsEvents.EntityDelta> LoggingModifiedReservationRoom(contractsReservations.ReservationRoom reservationRoomBeforeModification, contractsReservations.ReservationRoom reservationRoomAfterModification)
        {
            if (reservationRoomAfterModification == null)
                return new List<contractsEvents.EntityDelta>();

            var entityDeltas = new List<contractsEvents.EntityDelta>();
            var roomInfoEntityProperties = new contractsEvents.Portable.Infrastructure.PropertyDictionary();

            // Room Name
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Name,
                this.GetReservationRoomName(reservationRoomAfterModification),
                (reservationRoomBeforeModification != null) ? this.GetReservationRoomName(reservationRoomBeforeModification) : null);

            // Rate Name
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.RateName,
                (reservationRoomAfterModification.Rate != null) ? reservationRoomAfterModification.Rate.Name : null,
                (reservationRoomBeforeModification != null && reservationRoomBeforeModification.Rate != null) ? reservationRoomBeforeModification.Rate.Name : null);

            // Checkin Date
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DateFrom
                , (reservationRoomAfterModification.DateFrom.HasValue) ? reservationRoomAfterModification.DateFrom.Value.Date : new Nullable<DateTime>()
                , (reservationRoomBeforeModification != null && reservationRoomBeforeModification.DateFrom.HasValue) ? reservationRoomBeforeModification.DateFrom.Value.Date : new Nullable<DateTime>());

            // Checkout Date
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DateTo
                , (reservationRoomAfterModification.DateTo.HasValue) ? reservationRoomAfterModification.DateTo.Value.Date : new Nullable<DateTime>()
                , (reservationRoomBeforeModification != null && reservationRoomBeforeModification.DateTo.HasValue) ? reservationRoomBeforeModification.DateTo.Value.Date : new Nullable<DateTime>());

            // Guest Name
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.GuestName,
                reservationRoomAfterModification.GuestName,
                (reservationRoomBeforeModification != null) ? reservationRoomBeforeModification.GuestName : null);

            // Room Status
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Status
                , Enum.GetName(typeof(Constants.ReservationStatus), reservationRoomAfterModification.Status)
                , (reservationRoomBeforeModification != null) ? Enum.GetName(typeof(Constants.ReservationStatus), reservationRoomBeforeModification.Status) : null);

            // Number of Adults
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.AdultCount,
                reservationRoomAfterModification.AdultCount,
                (reservationRoomBeforeModification != null) ? reservationRoomBeforeModification.AdultCount : null);

            // Number of Children
            roomInfoEntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ChildCount,
                reservationRoomAfterModification.ChildCount,
                (reservationRoomBeforeModification != null) ? reservationRoomBeforeModification.ChildCount : null);

            // Create Room Delta
            var roomInfoDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.ReservationRooms)
            {
                EntityState = (reservationRoomBeforeModification != null) ? contractsEvents.EntityState.Modified : contractsEvents.EntityState.Created,
                EntityKey = reservationRoomAfterModification.UID,
                EntityProperties = roomInfoEntityProperties
            };


            // EXTRAS --------------------------------------------------------------
            if (roomInfoDelta.EntityState != contractsEvents.EntityState.Created)
            {
                var extrasBeforeModification = (reservationRoomBeforeModification != null && reservationRoomBeforeModification.ReservationRoomExtras != null) ?
                    new List<contractsReservations.ReservationRoomExtra>(reservationRoomBeforeModification.ReservationRoomExtras) : new List<contractsReservations.ReservationRoomExtra>();
                var extrasAfterModification = (reservationRoomAfterModification.ReservationRoomExtras != null) ?
                    new List<contractsReservations.ReservationRoomExtra>(reservationRoomAfterModification.ReservationRoomExtras) : new List<contractsReservations.ReservationRoomExtra>();

                // Removed Extras
                foreach (var removedExtra in extrasBeforeModification.Except(extrasAfterModification, (x, y) => x.Extra_UID == y.Extra_UID || x.Extra_UID <= 0))
                    entityDeltas.Add(this.LoggingReservationRoomExtras((long)roomInfoDelta.EntityKey, contractsEvents.EntityState.Deleted, removedExtra));

                // Added or Updated Extras
                foreach (var newExtra in extrasAfterModification.Where(x => x.Extra_UID > 0))
                {
                    var oldExtra = extrasBeforeModification.FirstOrDefault(x => x.Extra_UID == newExtra.Extra_UID);

                    // Added Extra
                    if (oldExtra == null)
                        entityDeltas.Add(this.LoggingReservationRoomExtras((long)roomInfoDelta.EntityKey, contractsEvents.EntityState.Created, newExtra));

                    // Updated Extra
                    else
                        entityDeltas.Add(this.LoggingReservationRoomExtras((long)roomInfoDelta.EntityKey, contractsEvents.EntityState.Modified, newExtra, oldExtra));
                }
            }


            // Clean Room Delta
            var reservationRoomExtras = entityDeltas.Where(x => EntityEnum.ReservationRoomExtra.ToString().Equals(x.EntityType)).ToList();
            bool removeRoomDeltaIfNoChanges = !reservationRoomExtras.Any(extra => extra.EntityProperties != null && extra.EntityProperties[EntityPropertyEnum.ReservationRooms_UID.ToString()].CurrentValue == roomInfoDelta.EntityKey); ; // Remove room if have no changes and doens't exist extras to log
            var previligedProperties = new List<EntityPropertyEnum>()
            {
                EntityPropertyEnum.Name,
                EntityPropertyEnum.RateName
            };
            entityDeltas.Add(contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(roomInfoDelta, previligedProperties, removeRoomDeltaIfNoChanges));

            return entityDeltas;
        }

        private List<contractsEvents.EntityDelta> LoggingModifyReservation(contractsReservations.Reservation reservationBeforeModification, contractsReservations.Reservation reservationAfterModification)
        {
            var reservationRoomsEntitiesDeltas = new List<contractsEvents.EntityDelta>();
            if (reservationAfterModification.ReservationRooms != null)
                foreach (var roomAfterModification in reservationAfterModification.ReservationRooms)
                {
                    // Get room before modification
                    var roomBeforeModification = (reservationBeforeModification.ReservationRooms != null) ?
                        reservationBeforeModification.ReservationRooms.FirstOrDefault(r => r.ReservationRoomNo.Equals(roomAfterModification.ReservationRoomNo)) : null;

                    var roomEntityDelta = this.LoggingModifiedReservationRoom(roomBeforeModification, roomAfterModification);
                    reservationRoomsEntitiesDeltas.AddRange(roomEntityDelta);
                }

            return reservationRoomsEntitiesDeltas;
        }
        #endregion


        /// <summary>
        /// RESTful implementation of the UpdateReservationIsPaid operation.
        /// This operation updates IsPaid of Reservation entities.
        /// </summary>
        /// <param name="request">A UpdateReservationIsPaidRequest object containing the criteria to update ispaid of reservation</param>
        /// <returns>A UpdateReservationIsPaidResponse containing the result status and/or list of found errors</returns>
        public UpdateReservationIsPaidResponse UpdateReservationIsPaid(Reservation.BL.Contracts.Requests.UpdateReservationIsPaidRequest request)
        {
            var response = new UpdateReservationIsPaidResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            using (var scope = TransactionManager.BeginTransactionScope(DomainScopes.Omnibees))
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                var res = reservationRepo.GetQuery(x => x.UID == request.ReservationId).FirstOrDefault();

                try
                {
                    // Only decrement credit used if reservation is not cancelled yet
                    if (res.Status != (long)Constants.ReservationStatus.Cancelled)
                    {
                        // If Reservation is From Operator
                        if (VerifyIfReservationIsFromOperator(res))
                        {
                            this.HandleCreditDiscountForOperator(res, request.MakeDiscount);
                        }
                        // If Reservation is From TPI
                        else if (VerifyIfReservationIsFromTPI(res))
                        {
                            this.HandleCreditDiscountForTPI(res);
                        }
                    }

                    res.IsPaid = true;
                    res.IsPaidDecisionUser = request.UserId;
                    res.IsPaidDecisionDate = DateTime.UtcNow;

                    var reservationFilterRepo = RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                    var criteria = new ListReservationFilterCriteria { ReservationUIDs = new List<long> { request.ReservationId } };
                    var domainFilterRes = reservationFilterRepo.FindReservationFilterByCriteria(criteria, out var totalRecords).FirstOrDefault();

                    if (domainFilterRes != null)
                    {
                        domainFilterRes.IsPaid = true;
                        domainFilterRes.ModifiedBy = request.UserId;
                        domainFilterRes.ModifiedDate = DateTime.UtcNow;
                    }

                    unitOfWork.Save();

                    scope.Complete();

                    response.Succeed();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                    // Log to Email of the Errors
                    Dictionary<string, object> args = new Dictionary<string, object>();
                    if (res != null)
                    {
                        args.Add("PropertyUID", res.Property_UID);
                        args.Add("ReservationUID", res.UID);
                        args.Add("ReservationNumber", res.Number);
                        args.Add("ChannelUID", res.Channel_UID);
                        args.Add("TPIUID", res.TPI_UID);
                        args.Add("IsPaid", res.IsPaid);
                    }
                    else
                        args.Add("ReservationObject", null);
                    Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, args);

                    response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
                    response.Failed();
                }
            }

            return response;
        }

        /// <summary>
        /// RESTful implementation of the UpdateReservationIsPaidBulk operation.
        /// This operation updates IsPaid of Reservations entities.
        /// </summary>
        /// <param name="request">A UpdateReservationIsPaidRequest object containing the criteria to update ispaid of reservations</param>
        /// <returns>A UpdateReservationIsPaidResponse containing the result status and/or list of found errors</returns>
        public UpdateReservationIsPaidBulkResponse UpdateReservationIsPaidBulk(Reservation.BL.Contracts.Requests.UpdateReservationIsPaidBulkRequest request)
        {
            var response = new UpdateReservationIsPaidBulkResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            //using (var transaction = TransactionManager.BeginTransactionScope(DomainScopes.Omnibees))
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationsFilterRepo = this.RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                var convertedFilterRequest = new ListReservationFilterCriteria();

                int totalRecords;
                OtherConverter.Map2(request, convertedFilterRequest);
                var reservationIds = reservationsFilterRepo.FindReservationFilterByCriteria(convertedFilterRequest, out totalRecords).ToDictionary(x => x.UID);

                request.ReservationUIDs = reservationIds.Keys.ToList();

                if (!request.ReservationUIDs.Any())
                {
                    response.Succeed();
                    return response;
                }

                var reservationCriteria = new ListReservationCriteria
                {
                    ReservationUIDs = request.ReservationUIDs
                };
                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                var domainReservations = reservationRepo.FindByCriteria(reservationCriteria).ToList();

                if (!domainReservations.Any())
                {
                    response.Succeed();
                    return response;
                }

                var channelRepo = this.RepositoryFactory.GetOBChannelRepository();
                var channelRequest = new Contracts.Requests.ListChannelRequest
                {
                    ChannelIds = new List<long>(domainReservations.Where(x => x.Channel_UID.HasValue).Select(x => x.Channel_UID.Value).Distinct()),
                    IncludeChannelConfiguration = true,
                };
                var channeldict = channelRepo.ListChannel(channelRequest).ToDictionary(x => x.Id);

                var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
                var paymentType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest { UIDs = convertedFilterRequest.PaymentTypeIds }).FirstOrDefault();

                foreach (var res in domainReservations)
                {
                    try
                    {
                        // Only decrement credit used if reservation is not cancelled yet
                        if (res.Status != (long)Constants.ReservationStatus.Cancelled)
                        {
                            channeldict.TryGetValue(res.Channel_UID ?? 0, out var channel);
                            // If Reservation is From Operator
                            if (VerifyIfReservationIsFromOperator(channel))
                            {
                                this.HandleCreditDiscountForOperator(res, true, paymentType);
                            }
                            // If Reservation is From TPI
                            else if (VerifyIfReservationIsFromTPI(res))
                            {
                                this.HandleCreditDiscountForTPI(res, paymentType);
                            }
                        }

                        res.IsPaid = true;
                        res.IsPaidDecisionUser = request.UserId;
                        res.IsPaidDecisionDate = DateTime.UtcNow;

                        if (!reservationIds.TryGetValue(res.UID, out var domainFilterRes))
                            continue;

                        domainFilterRes.IsPaid = true;
                        domainFilterRes.ModifiedBy = request.UserId;
                        domainFilterRes.ModifiedDate = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                        // Log to Email of the Errors
                        Dictionary<string, object> args = new Dictionary<string, object>();
                        if (res != null)
                        {
                            args.Add("PropertyUID", res.Property_UID);
                            args.Add("ReservationUID", res.UID);
                            args.Add("ReservationNumber", res.Number);
                            args.Add("ChannelUID", res.Channel_UID);
                            args.Add("TPIUID", res.TPI_UID);
                            args.Add("IsPaid", res.IsPaid);
                        }
                        else
                            args.Add("ReservationObject", null);

                        Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex,
                            LogSeverity.High, args);

                        response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
                        response.Failed();
                    }
                }

                if (!response.Errors.Any())
                {
                    unitOfWork.Save();

                    #region COMMIT TRANSACTIONS                

                    //transaction.Complete();

                    #endregion COMMIT TRANSACTIONS

                    response.Succeed();
                }
            }

            return response;
        }

        /// <summary>
        /// RESTful implementation of the UpdateReservationCancelReason operation.
        /// This operation update reservation cancel reason.
        /// </summary>
        /// <param name="request">A UpdateReservationCancelReasonRequest object containing the criteria to update reservation cancel reason</param>
        /// <returns>A UpdateReservationCancelReasonResponse containing the result reservation Id</returns>
        public UpdateReservationCancelReasonResponse UpdateReservationCancelReason(Reservation.BL.Contracts.Requests.UpdateReservationCancelReasonRequest request)
        {
            var response = new UpdateReservationCancelReasonResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                var reservationHistoryRepo = this.RepositoryFactory.GetReservationHistoryRepository(unitOfWork);

                var cancelledStatus = Constants.ReservationStatus.Cancelled.ToString();
                var resHistory = reservationHistoryRepo.GetQuery(x => x.Status == cancelledStatus && x.ReservationUID == request.ReservationId).FirstOrDefault();

                var res = reservationRepo.GetQuery(x => x.UID == request.ReservationId).FirstOrDefault();

                try
                {
                    if (res != null)
                    {
                        res.CancelReservationReason_UID = request.CancelReservationReasonID;
                        res.CancelReservationComments = request.CancelReservationComments;

                        if (resHistory != null && !String.IsNullOrEmpty(resHistory.Message))
                        {
                            var history = resHistory.Message.FromJSON<ReservationLoggingMessage>();

                            history.objReservation.CancelReservationReason_UID = request.CancelReservationReasonID;
                            history.objReservation.CancelReservationComments = request.CancelReservationComments;

                            resHistory.Message = history.ToJSON();
                        }

                        response.Result = res.UID;
                        unitOfWork.Save();

                        response.Succeed();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                    // Log to Email of the Errors
                    Dictionary<string, object> args = new Dictionary<string, object>();
                    if (res != null)
                    {
                        args.Add("PropertyUID", res.Property_UID);
                        args.Add("ReservationUID", res.UID);
                        args.Add("ReservationNumber", res.Number);
                        args.Add("ChannelUID", res.Channel_UID);
                        args.Add("TPIUID", res.TPI_UID);
                        args.Add("CancelReservationReason_UID", res.CancelReservationReason_UID);
                        args.Add("CancelReservationComments", res.CancelReservationComments);
                    }
                    else
                        args.Add("ReservationObject", null);
                    Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, args);

                    response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
                    response.Failed();
                }
            }

            return response;
        }

        private int UpdateCreditUsed(long propertyUID, long channelUID, string creditType, string isActiveProperty, decimal creditValue)
        {
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);

            string sql = string.Format(UPDATE_CREDITUSED, creditType, isActiveProperty, propertyUID, channelUID, creditValue.ToString(CultureInfo.InvariantCulture));

            return sqlManager.ExecuteSql(sql);
        }

        private static readonly string UPDATE_CREDITUSED = @"UPDATE ChannelsProperties
                                SET ChannelsProperties.{0} =
		                                (CASE WHEN (ChannelsProperties.{0} IS not null) THEN
					                                (CASE WHEN ({4} + ChannelsProperties.{0}) < 0 THEN 0
						                                ELSE {4} + ChannelsProperties.{0} END)
					                                ELSE {4} END)
                                where ChannelsProperties.Channel_UID = {3}
                                and ChannelsProperties.Property_UID = {2}
                                and ChannelsProperties.{1} > 0";

    }
}


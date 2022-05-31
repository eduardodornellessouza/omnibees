using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Helper.Interfaces;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.TypeConverters;
using OB.DL.Common.Interfaces;
using OB.Domain.ProactiveActions;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Transactions;
using contractProperties = OB.BL.Contracts.Data.Properties;
using contractsProactiveActions = OB.Reservation.BL.Contracts.Data.ProactiveActions;

namespace OB.BL.Operations.Impl
{
    public class PropertyEventsManagerPOCO : BusinessPOCOBase, IPropertyEventsManagerPOCO
    {
        private static readonly SemaphoreSlim __INSERTPROPERTYQUEUE_SEMAPHORE = new SemaphoreSlim(5,5);

        #region REST Methods

        /// <summary>
        /// RESTful implementation of the ListPropertyQueues operation.
        /// This operation searchs for PropertyQueue entities given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListPropertyQueueRequest object containing the search criteria to find PropertyQueues</param>
        /// <returns>A ListPropertyQueueResponse containing the List of PropertyQueue objects that were found for the given criteria</returns
        public ListPropertyQueueResponse ListPropertyQueues(ListPropertyQueueRequest request)
        {
            var response = new ListPropertyQueueResponse();
           
            try
            {
                Contract.Requires(request != null, "Request object instance is expected");
                response.RequestGuid = request.RequestGuid;
                
                int totalRecords = -1;

                using(var unitOfWork = this.SessionFactory.GetUnitOfWork()){
                    var propertyQueueRepo = this.RepositoryFactory.GetPropertyQueueRepository(unitOfWork);


                    var result = propertyQueueRepo.FindByCriteria(out totalRecords, request.UIDs, request.PropertyUIDs,
                        request.PropertyEventUIDs, request.SystemEventUIDs, request.TaskTypeUIDs,
                        request.SystemTemplateUIDs, request.IsProcessing, request.IsProcessed, request.MinRetryNumber,
                        request.MaxRetryNumber,
                        request.TimeSpanSinceLastProcessingDate,
                        request.MinLastProcessingDate,
                        request.MaxLastProcessingDate,
                        request.MinDate,
                        request.MaxDate,
                        request.PageIndex, request.PageSize, request.ReturnTotal);

                    response.Result = new ObservableCollection<contractsProactiveActions.PropertyQueue>(result.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)));
                    
                
                    response.TotalRecords = totalRecords;

                    response.Succeed();
                }
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
           
                Dictionary<string, object> args = new Dictionary<string, object>();
                ex.Data.Add("NumberRetries", request.NumberRetries);
                ex.Data.Add("RetryMinutesInterval", request.RetryMinutesInterval);
                
                this.Logger.Error(ex, "An error occurred while querying the PropertyQueue table");
                //LogEmail.SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.Critical, args);
            }

            return response;
        }

        
        /// <summary>
        /// RESTful implementation of the ListPropertyQueuesForTodayProcessing operation.
        /// This operation gets the PropertyQueue entities that should be processed for today, 
        /// marking them in the database with IsProcessing=true.
        /// </summary>
        /// <param name="request">A ListPropertyQueueForTodayProcessingRequest object containing the filters to find PropertyQueues
        /// including the filter for the SystemEvents.
        /// </param>
        /// <returns>A ListPropertyQueueResponse containing the List of PropertyQueue objects that were found and marked with IsProcessing=true</returns>
        public ListPropertyQueueResponse ListPropertyQueuesForTodayProcessing(ListPropertyQueueForTodayProcessingRequest request)
        {
            var response = new ListPropertyQueueResponse();
           
                try
                {
                    Contract.Requires(request != null, "Request object instance is expected");
                    response.RequestGuid = request.RequestGuid;
                
                    using(var unitOfWork = this.SessionFactory.GetUnitOfWork())
                    {
                        var propertyQueueRepo = this.RepositoryFactory.GetPropertyQueueRepository(unitOfWork);

                        var result = propertyQueueRepo.GetTodaysPropertyQueues(request.NumberRetries, request.RetryMinutesInterval, request.SystemEventUIDs);

                        response.Result = new ObservableCollection<contractsProactiveActions.PropertyQueue>(result.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)));

                        response.Succeed();
                    }
                }
                catch (Exception ex)
                {
                    response.Failed();
                    response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
           
                    Dictionary<string, object> args = new Dictionary<string, object>();
                    ex.Data.Add("NumberRetries", request.NumberRetries);
                    ex.Data.Add("RetryMinutesInterval", request.RetryMinutesInterval);
                
                    this.Logger.Error(ex, "An error occurred while querying the PropertyQueue table");
                    //LogEmail.SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.Critical, args);
                }

            return response;
        }

        /// <summary>
        /// RESTful implementation of the ListPropertyEvents operation.
        /// This operation searchs for PropertyEvent entities given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListPropertyEventRequest object containing the search criteria to find PropertyEvents</param>
        /// <returns>A ListPropertyEventResponse containing the List of PropertyEvent objects that were found for the given criteria</returns
        public ListPropertyEventResponse ListPropertyEvents(ListPropertyEventRequest request)
        {
            return null;
        }

        /// <summary>
        /// RESTful implementation of the ListSystemActions operation.
        /// This operation searchs for SystemAction entities given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListSystemActionRequest object containing the search criteria to find SystemActions</param>
        /// <returns>A ListSystemActionResponse containing the List of SystemAction objects that were found for the given criteria</returns
        public ListSystemActionResponse ListSystemActions(ListSystemActionRequest request)
        {
            return null;
        }

        /// <summary>
        /// RESTful implementation of the ListSystemEvents operation.
        /// This operation searchs for SystemEvent entities given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListSystemEventRequest object containing the search criteria to find SystemEvents</param>
        /// <returns>A ListSystemEventResponse containing the List of SystemEvent objects that were found for the given criteria</returns
        public ListSystemEventResponse ListSystemEvents(ListSystemEventRequest request)
        {
            return null;
        }

        #endregion

         /// <summary>
        /// Created By :: Ronak Shah
        /// Created Date :: 20 Dec 2010
        /// Desc :: Check for particular Property Event and insert record in Property Queue table
        /// </summary>
        public long InsertPropertyQueue(long propertyId, long reservationId, long channelId, long systemEventCode, bool isOperator = false)
        {

            long taskTypeId = reservationId;

            //TMOREIRA: Equivalent to WITH (NOLOCK)
            //It's not necessary to use a transaction, we are only writing on PropertyEvent table, plus if there are records written to the table we
            //don't want to roll them back if there is a failure.
            try
            {
                var sessionFactory = this.Resolve<ISessionFactory>();
                var repoFactory = this.Resolve<IRepositoryFactory>();

                //UnitOfWorks
                var unitOfWork = sessionFactory.GetUnitOfWork(PropertyEvent.DomainScope,
                                                            PropertyQueue.DomainScope);

                //Repositories
                var propEventsRepo = repoFactory.GetPropertyEventsRepository(unitOfWork);


                var propertyQueueRepo = repoFactory.GetRepository<PropertyQueue>(unitOfWork);
                var reservationRepo = repoFactory.GetReservationsRepository(unitOfWork);

                string strSystemEventCode = systemEventCode.ToString();


                //__INSERTPROPERTYQUEUE_SEMAPHORE.Wait();

                //TMOREIRA: optimized!!
                var lstPropertyEvents = propEventsRepo.FindByPropertyUID_And_SystemEventCode(propertyId, strSystemEventCode).ToList();
                //join se in systemEventsRepo.GetQuery(x => x.Code.Equals(strSystemEventCode)) on pe.SystemEvent_UID equals se.UID
                //join pt in systemTemplatesRepo.GetQuery() on pe.PropertyTemplate_UID equals pt.UID
                //join pec in propEventConditionsRepo.GetQuery() on pe.UID equals pec.PropertyEvent_UID
                //select new 
                //{
                //   UID = pe.UID,
                //   ReservationOption = pe.ReservationOption                                                     
                //});

                long reservationChannelUID = channelId;
                bool dirty = false;
                var isReservationFromBE = reservationChannelUID == reservationRepo.GetBookingEngineChannelUID();


                if (lstPropertyEvents != null && lstPropertyEvents.Count > 0)
                {
                    foreach (var objEvent in lstPropertyEvents)
                    {
                        //if (objEvent.SystemEvent_UID == (long)Constants.SystemEventsCode.NewBooking || objEvent.SystemEvent_UID == (long)Constants.SystemEventsCode.BookingChanged || objEvent.SystemEvent_UID == (long)Constants.SystemEventsCode.BookingCancelled)
                        if (systemEventCode == (long)Constants.SystemEventsCode.NewBookingArrived || systemEventCode == (long)Constants.SystemEventsCode.BookingChanged || systemEventCode == (long)Constants.SystemEventsCode.Bookingcancelled)
                        {                                                                                             
                            if (isReservationFromBE)//reservation made from booking engine
                            {
                                if (!objEvent.ReservationOption.HasValue || (objEvent.ReservationOption.HasValue && (objEvent.ReservationOption == (int)Constants.Reservationoption.AllReservation || objEvent.ReservationOption == (int)Constants.Reservationoption.Bookingengine)))
                                {

                                    var objPQ = new PropertyQueue();
                                    objPQ.Date = DateTime.Today.Date;
                                    objPQ.IsProcessed = false;
                                    objPQ.Property_UID = propertyId;
                                    objPQ.PropertyEvent_UID = objEvent.UID;
                                    objPQ.TaskType_UID = taskTypeId;


                                    propertyQueueRepo.Add(objPQ);
                                    dirty |= true;
                                }
                            }
                            else//reservation made from Channels
                            {
                                if (!objEvent.ReservationOption.HasValue || (objEvent.ReservationOption.HasValue && (objEvent.ReservationOption == (int)Constants.Reservationoption.AllReservation || objEvent.ReservationOption == (int)Constants.Reservationoption.Channels)))
                                {
                                    PropertyQueue objPQ = new PropertyQueue();
                                    objPQ.Date = DateTime.Today.Date;
                                    objPQ.IsProcessed = false;
                                    objPQ.Property_UID = propertyId;
                                    objPQ.PropertyEvent_UID = objEvent.UID;
                                    objPQ.TaskType_UID = taskTypeId;
                                    propertyQueueRepo.Add(objPQ);

                                    dirty |= true;
                                }
                            }
                        }
                        else if (systemEventCode == (int)Constants.SystemEventsCode.CreditLimit)
                        {
                            var objPQ = new PropertyQueue();
                            objPQ.Date = DateTime.Today.Date;
                            objPQ.IsProcessed = false;
                            objPQ.Property_UID = propertyId;
                            objPQ.PropertyEvent_UID = objEvent.UID;
                            objPQ.TaskType_UID = taskTypeId;
                            objPQ.MailBody = isOperator ? "operator" : "tpi";
                            propertyQueueRepo.Add(objPQ);

                            dirty |= true;
                        }
                        else
                        {
                            var objPQ = new PropertyQueue();
                            objPQ.Date = DateTime.Today.Date;
                            objPQ.IsProcessed = false;
                            objPQ.Property_UID = propertyId;
                            objPQ.PropertyEvent_UID = objEvent.UID;
                            objPQ.TaskType_UID = taskTypeId;
                            propertyQueueRepo.Add(objPQ);

                            dirty |= true;
                        }
                    }

                }

                //Save Unit Of Work and commit transaction
                if (dirty)
                {
                    unitOfWork.Save();
                }              
                return 1;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> args = new Dictionary<string, object>();
                args.Add("PropertyId", propertyId);
                args.Add("SystemEventCode", systemEventCode);
                args.Add("TaskTypeId", taskTypeId);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, args);
                Logger.Error(ex);
                //ProturRIAServices.Web.Helper.ProjectGeneral.LogSendErrorEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                throw ;
            }
            finally
            {
                //  __INSERTPROPERTYQUEUE_SEMAPHORE.Release();
            }
        
        }


        /// <summary>
        /// Created By :: Ronak Shah
        /// Created Date :: 20 Dec 2010
        /// Desc :: Check for particular Property Event and insert record in Property Queue table
        /// </summary>
        public long InsertPropertyQueue(long propertyId, long systemEventCode, long taskTypeId, bool isOperator = false)
        {
            //TMOREIRA: Equivalent to WITH (NOLOCK)
            //It's not necessary to use a transaction, we are only writing on PropertyEvent table, plus if there are records written to the table we
            //don't want to roll them back if there is a failure.
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {

                try
                {
                    var sessionFactory = this.Resolve<ISessionFactory>();
                    var repoFactory = this.Resolve<IRepositoryFactory>();

                    //UnitOfWorks
                    var unitOfWork = sessionFactory.GetUnitOfWork(PropertyEvent.DomainScope,
                                                                PropertyQueue.DomainScope, OB.Domain.Reservations.Reservation.DomainScope);

                    //Repositories
                    var propEventsRepo = repoFactory.GetPropertyEventsRepository(unitOfWork); 
                    
                   
                    var propertyQueueRepo = repoFactory.GetRepository<PropertyQueue>(unitOfWork);
                    var reservationRepo = repoFactory.GetReservationsRepository(unitOfWork);

                    string strSystemEventCode = systemEventCode.ToString();


                    //__INSERTPROPERTYQUEUE_SEMAPHORE.Wait();

                    //TMOREIRA: optimized!!
                    var lstPropertyEvents =  propEventsRepo.FindByPropertyUID_And_SystemEventCode(propertyId , strSystemEventCode).ToList();
                                                  //join se in systemEventsRepo.GetQuery(x => x.Code.Equals(strSystemEventCode)) on pe.SystemEvent_UID equals se.UID
                                                  //join pt in systemTemplatesRepo.GetQuery() on pe.PropertyTemplate_UID equals pt.UID
                                                  //join pec in propEventConditionsRepo.GetQuery() on pe.UID equals pec.PropertyEvent_UID
                                                  //select new 
                                                  //{
                                                  //   UID = pe.UID,
                                                  //   ReservationOption = pe.ReservationOption                                                     
                                                  //});
                
                    long? reservationChannelUID = null;
                    bool searchedForReservation = false;
                    bool dirty = false;
                 
                    if (lstPropertyEvents != null && lstPropertyEvents.Count > 0)
                    {
                        foreach (var objEvent in lstPropertyEvents)
                        {
                            //if (objEvent.SystemEvent_UID == (long)Constants.SystemEventsCode.NewBooking || objEvent.SystemEvent_UID == (long)Constants.SystemEventsCode.BookingChanged || objEvent.SystemEvent_UID == (long)Constants.SystemEventsCode.BookingCancelled)
                            if (systemEventCode == (long)Constants.SystemEventsCode.NewBookingArrived || systemEventCode == (long)Constants.SystemEventsCode.BookingChanged || systemEventCode == (long)Constants.SystemEventsCode.Bookingcancelled)
                            {
                                //Get if reservation is from channel
                                //TMOREIRA I've changed this (due to the use of different contexts) from:                         
                                //Channel ch = (from res in ObjectContext.Reservations
                                //              join chn in ObjectContext.Channels on res.Channel_UID equals chn.UID
                                //              where res.UID == TaskTypeId && res.Property_UID == PropertyId
                                //              && chn.IsBookingEngine == true
                                //              select
                                //              chn).FirstOrDefault();

                                //Avoid executing the same query for the same ReservationId over and over again.
                                if (!searchedForReservation && !reservationChannelUID.HasValue)
                                {
                                    searchedForReservation = true;
                                    reservationChannelUID = reservationRepo.GetQuery(x => x.UID == taskTypeId)//.GetQuery(x => x.UID == taskTypeId && x.Property_UID == propertyId)
                                                    .Select(x => x.Channel_UID).FirstOrDefault();
                                }
                                                    

                                var isReservationFromChannel = reservationChannelUID.HasValue
                                                            && reservationChannelUID == reservationRepo.GetBookingEngineChannelUID();


                                if (isReservationFromChannel)//reservation made from booking engine
                                {
                                    if (!objEvent.ReservationOption.HasValue || (objEvent.ReservationOption.HasValue && (objEvent.ReservationOption == (int)Constants.Reservationoption.AllReservation || objEvent.ReservationOption == (int)Constants.Reservationoption.Bookingengine)))
                                    {

                                        var objPQ = new PropertyQueue();
                                        objPQ.Date = DateTime.Today.Date;
                                        objPQ.IsProcessed = false;
                                        objPQ.Property_UID = propertyId;
                                        objPQ.PropertyEvent_UID = objEvent.UID;
                                        objPQ.TaskType_UID = taskTypeId;

                                       
                                        propertyQueueRepo.Add(objPQ);
                                        dirty |= true;
                                    }
                                }
                                else//reservation made from Channels
                                {
                                    if (!objEvent.ReservationOption.HasValue || (objEvent.ReservationOption.HasValue && (objEvent.ReservationOption == (int)Constants.Reservationoption.AllReservation || objEvent.ReservationOption == (int)Constants.Reservationoption.Channels)))
                                    {
                                        PropertyQueue objPQ = new PropertyQueue();
                                        objPQ.Date = DateTime.Today.Date;
                                        objPQ.IsProcessed = false;
                                        objPQ.Property_UID = propertyId;
                                        objPQ.PropertyEvent_UID = objEvent.UID;
                                        objPQ.TaskType_UID = taskTypeId;
                                        propertyQueueRepo.Add(objPQ);

                                        dirty |= true;
                                    }
                                }
                            }
                            else if (systemEventCode == (int)Constants.SystemEventsCode.CreditLimit)
                            {
                                var objPQ = new PropertyQueue();
                                objPQ.Date = DateTime.Today.Date;
                                objPQ.IsProcessed = false;
                                objPQ.Property_UID = propertyId;
                                objPQ.PropertyEvent_UID = objEvent.UID;
                                objPQ.TaskType_UID = taskTypeId;
                                objPQ.MailBody = isOperator ? "operator" : "tpi";
                                propertyQueueRepo.Add(objPQ);

                                dirty |= true;
                            }
                            else
                            {
                                var objPQ = new PropertyQueue();
                                objPQ.Date = DateTime.Today.Date;
                                objPQ.IsProcessed = false;
                                objPQ.Property_UID = propertyId;
                                objPQ.PropertyEvent_UID = objEvent.UID;
                                objPQ.TaskType_UID = taskTypeId;
                                propertyQueueRepo.Add(objPQ);
                                
                                dirty |= true;
                            }
                        }

                    }

                    //Save Unit Of Work and commit transaction
                    if (dirty)
                    {
                        unitOfWork.Save();                       
                    }                    

                    return 1;
                }
                catch (Exception ex)
                {
                    Dictionary<string, object> args = new Dictionary<string, object>();
                    args.Add("PropertyId", propertyId);
                    args.Add("SystemEventCode", systemEventCode);
                    args.Add("TaskTypeId", taskTypeId);
                    this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, args);

                    //ProturRIAServices.Web.Helper.ProjectGeneral.LogSendErrorEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                    Logger.Error(ex);
                    throw ;
                }
                finally
                {
                   // __INSERTPROPERTYQUEUE_SEMAPHORE.Release();
                }
            }

        }

        public long CancelPropertyQueueEvent(long PropertyId, long SystemEventCode, long TaskTypeId, string reasonCancelation)
        {          
            try
            {
                
                var unitOfWork = this.SessionFactory.GetUnitOfWork(PropertyQueue.DomainScope);

                //Repositories                
                var propertyQueueRepo = this.RepositoryFactory.GetRepository<PropertyQueue>(unitOfWork);

                string strCode = SystemEventCode.ToString();

                //TMOREIRA: optimized
                List<PropertyQueue> queues = propertyQueueRepo.GetQuery(x => x.Property_UID == PropertyId && x.TaskType_UID == TaskTypeId 
                                                && x.PropertyEvent != null && x.PropertyEvent.SystemEvent != null
                                                  && x.PropertyEvent.SystemEvent.Code.Equals(strCode)).ToList();
                                              //(from propertyqueues in propertyQueueRepo.GetQuery()
                                              //join propertyevents in propEventsRepo.GetQuery() on propertyqueues.PropertyEvent_UID equals propertyevents.UID
                                              //join se in systemEventsRepo.GetQuery() on propertyevents.SystemEvent_UID equals se.UID
                                              //where se.Code.Equals(strCode)
                                              //&& propertyqueues.Property_UID == PropertyId
                                              //&& propertyqueues.TaskType_UID == TaskTypeId
                                              //select propertyqueues).ToList();


                foreach (PropertyQueue q in queues)
                {
                    if (q.IsProcessed != true)
                    {
                        q.IsProcessed = true;
                        q.LastProcessingDate = DateTime.Now;
                        q.ErrorList = reasonCancelation;
                    }
                }

                unitOfWork.Save();
            }
            //TMOREIRA: really bad practice, check with TONI to implement this...
            catch
            {

            }

            return 1;
        }

        /// <summary>
        /// Set From Mail
        /// </summary>
        /// <param name="PropertyUID"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public contractProperties.PropertyMailServerSettings GetFromMail(long PropertyUID, int Type)
        {
            var objmail = new contractProperties.PropertyMailServerSettings();

            //switch (Type)
            //{
            //    case 1:
            //        objmail = (from p in ObjectContext.Properties
            //                   from m in ObjectContext.MailServerSettings.Where(t => t.UID == p.ReservationEmailMailServer_UID).DefaultIfEmpty()
            //                   where p.UID == PropertyUID
            //                   select new PropertyMailServerSettings
            //                   {
            //                       ReservationEmail = (m != null && !string.IsNullOrEmpty(p.ReservationEmail)) ? p.ReservationEmail + "@" + m.DomainName : string.Empty,
            //                       ReservationServerName = m != null ? m.ServerName : string.Empty,
            //                       ReservationPort = m != null ? m.Port : null,
            //                       ReservationUseSSL = m != null ? m.UseSSL : (bool?)null
            //                   }).SingleOrDefault();
            //        break;
            //    case 2:
            //        objmail = (from p in ObjectContext.Properties
            //                   from m in ObjectContext.MailServerSettings.Where(t => t.UID == p.CampaignEmailMailServer_UID).DefaultIfEmpty()
            //                   where p.UID == PropertyUID
            //                   select new PropertyMailServerSettings
            //                   {
            //                       CampaignEmail = (m != null && !string.IsNullOrEmpty(p.CampaignEmail)) ? p.CampaignEmail + "@" + m.DomainName : string.Empty,
            //                       CampaignServerName = m != null ? m.ServerName : string.Empty,
            //                       CampaignPort = m != null ? m.Port : null,
            //                       CampaignUseSSL = m != null ? m.UseSSL : (bool?)null
            //                   }).SingleOrDefault();

            //        break;
            //    case 3:
            //        objmail = (from p in ObjectContext.Properties
            //                   from m in ObjectContext.MailServerSettings.Where(t => t.UID == p.SurveyEmailMailServer_UID).DefaultIfEmpty()
            //                   where p.UID == PropertyUID
            //                   select new PropertyMailServerSettings
            //                   {
            //                       SurveyEmail = (m != null && !string.IsNullOrEmpty(p.SurveyEmail)) ? p.SurveyEmail + "@" + m.DomainName : string.Empty,
            //                       SurveyServerName = m != null ? m.ServerName : string.Empty,
            //                       SurveyPort = m != null ? m.Port : null,
            //                       SurveyUseSSL = m != null ? m.UseSSL : (bool?)null
            //                   }).SingleOrDefault();
            //        break;
            //    default:
            //        objmail = (from p in ObjectContext.Properties
            //                   from m in ObjectContext.MailServerSettings.Where(t => t.UID == p.GeneralEmailMailServer_UID).DefaultIfEmpty()
            //                   where p.UID == PropertyUID
            //                   select new PropertyMailServerSettings
            //                   {
            //                       GeneralEmail = (m != null && !string.IsNullOrEmpty(p.GeneralEmail)) ? p.GeneralEmail + "@" + m.DomainName : string.Empty,
            //                       GeneralServerName = m != null ? m.ServerName : string.Empty,
            //                       GeneralPort = m != null ? m.Port : null,
            //                       GeneralUseSSL = m != null ? m.UseSSL : (bool?)null
            //                   }).SingleOrDefault();

            //        break;
            //}

            //if (objmail == null)
            //    objmail = new PropertyMailServerSettings();

            return objmail;
        }

    
 
    }
}

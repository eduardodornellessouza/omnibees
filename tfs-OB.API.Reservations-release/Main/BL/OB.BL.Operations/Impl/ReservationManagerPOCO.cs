using Castle.Core.Internal;
using Newtonsoft.Json;
using OB.Api.Core;
using OB.BL.Contracts.Data.PMS;
using OB.BL.Contracts.Requests;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Helper.Interfaces;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.BusinessObjects.ModifyClasses;
using OB.BL.Operations.Internal.LogHelper;
using OB.BL.Operations.Internal.TypeConverters;
using OB.DL.Common;
using OB.DL.Common.Criteria;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Domain.Reservations;
using OB.Events.Contracts.Enums;
using OB.Log.Messages;
using OB.Reservation.BL;
using OB.Reservation.BL.Contracts.Data;
using OB.Reservation.BL.Contracts.Data.Channels;
using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Payments;
using OB.Reservation.BL.Contracts.Data.VisualStates;
using OB.Reservation.BL.Contracts.Logs;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using PaymentGatewaysLibrary;
using PaymentGatewaysLibrary.BrasPagGateway.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Configuration;
using static PaymentGatewaysLibrary.BrasPagGateway.Constants;
using CancelReservationRequest = OB.Reservation.BL.Contracts.Requests.CancelReservationRequest;
using contractsCRM = OB.Reservation.BL.Contracts.Data.CRM;
using contractsCRMOB = OB.BL.Contracts.Data.CRM;
using contractsEvents = OB.Events.Contracts;
using contractsProactiveActions = OB.Reservation.BL.Contracts.Data.ProactiveActions;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using contractsRates = OB.BL.Contracts.Data.Rates;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;
using DomainPayments = OB.Domain.Payments;
using domainReservations = OB.Domain.Reservations;
using ExportReservationsToFtpRequest = OB.Reservation.BL.Contracts.Requests.ExportReservationsToFtpRequest;
using GetCancelationCostsRequest = OB.Reservation.BL.Contracts.Requests.GetCancelationCostsRequest;
using IsolationLevel = System.Transactions.IsolationLevel;
using ListLostReservationsRequest = OB.Reservation.BL.Contracts.Requests.ListLostReservationsRequest;
using ListPropertiesWithReservationsForChannelOrTPIRequest = OB.Reservation.BL.Contracts.Requests.ListPropertiesWithReservationsForChannelOrTPIRequest;
using ListPropertiesWithReservationsForChannelsRequest = OB.Reservation.BL.Contracts.Requests.ListPropertiesWithReservationsForChannelsRequest;
using ListReservationHistoryRequest = OB.Reservation.BL.Contracts.Requests.ListReservationHistoryRequest;
using ListReservationNumbersRequest = OB.Reservation.BL.Contracts.Requests.ListReservationNumbersRequest;
using ListReservationsLightRequest = OB.Reservation.BL.Contracts.Requests.ListReservationsLightRequest;
using ListReservationUIDsByPropRateDateOfModifOrStayRequest = OB.Reservation.BL.Contracts.Requests.ListReservationUIDsByPropRateDateOfModifOrStayRequest;
using MarkReservationsAsViewedRequest = OB.Reservation.BL.Contracts.Requests.MarkReservationsAsViewedRequest;
using PaymentGatewayConfiguration = OB.BL.Contracts.Data.Payments.PaymentGatewayConfiguration;
using PaymentGatewayInfoAndModifyReservationOnRequestForTransactionIDRequest = OB.Reservation.BL.Contracts.Requests.PaymentGatewayInfoAndModifyReservationOnRequestForTransactionIDRequest;
using reservationRequest = OB.Reservation.BL.Contracts.Requests.InsertReservationRequest;
using UpdateReservationRequest = OB.Reservation.BL.Contracts.Requests.UpdateReservationRequest;
using ES.API.Contracts.Response;
using ES.API.Contracts.Requests;
using OB.BL.Contracts.Data.PMS;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.Reservation.BL.Contracts.Data.PMS;
using internalErrors = OB.BL.Operations.Internal.BusinessObjects.Errors;

namespace OB.BL.Operations.Impl
{
    /// <summary>
    /// Class that provides the Business Layer operations related with Reservations.
    /// </summary>
    public partial class ReservationManagerPOCO : BusinessPOCOBase, IReservationManagerPOCO
    {
        public static readonly int _INSERT_RESERVATION_MAX_THREADS = 5;
        public static readonly int _UPDATE_RESERVATION_MAX_THREADS = 5;
        public static readonly int _MODIFY_RESERVATION_MAX_THREADS = 5;
        public static readonly string _INSERT_RESERVATION_THROUGHPUTLIMITER_KEY = "___INSERT_RESERVATION_THROUGHPUTLIMITER_KEY";
        public static readonly string _UPDATE_RESERVATION_THROUGHPUTLIMITER_KEY = "___UPDATE_RESERVATION_THROUGHPUTLIMITER_KEY";
        public static readonly string _MODIFY_RESERVATION_THROUGHPUTLIMITER_KEY = "___MODIFY_RESERVATION_THROUGHPUTLIMITER_KEY";
        public static readonly string _CLOSE_SALES_THROUGHPUTLIMITER_KEY = "_CLOSE_SALES_THROUGHPUTLIMITER_KEY";

        private static readonly int maxNumberOfRetriesForInsertReservation = 10;
        private static readonly int maxNumberOfRetriesForUpdateInventory = 10;
        private static readonly int maxNumberOfRetriesForUpdateReservation = 10;
        private static readonly string defaultPaymentGateway = "Bee2Pay";
        private static readonly int BrasilCountryId = 40;
        private static readonly int UnitedStatesCountryId = 202;

        public ReservationManagerPOCO()
        {
        }


        #region RESTful Operations


        /// <summary>
        /// RESTful implementation of the FindReservations operation.
        /// This operation searchs for reservations given a specific request criteria.
        /// </summary>
        /// <param name="request">A FindReservationRequest object containing the Search criteria</param>
        /// <returns>A ListReservationResponse containing the List of matching Reservation objects</returns>
        public ListReservationResponse ListReservations(OB.Reservation.BL.Contracts.Requests.ListReservationRequest request)
        {
            var response = new ListReservationResponse();

            response.Result = new ObservableCollection<contractsReservations.Reservation>();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");

                var isReadOnly = !((request.ReservationUIDs != null && request.ReservationUIDs.Count == 1) ||
                (request.ReservationNumbers != null && request.ReservationNumbers.Count == 1) ||
                (request.PartnerReservationNumbers != null && request.PartnerReservationNumbers.Count == 1));

                using (var unitOfWork = this.SessionFactory.GetUnitOfWork(isReadOnly))
                {
                    //Validate request parameter
                    var rulesRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);
                    var appRepo = RepositoryFactory.GetOBAppSettingRepository();
                    var reservationHelper = Resolve<IReservationHelperPOCO>();
                    var appSetting = appRepo.ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string> { "ListReservationsMaxDaysWithoutFilters" } }).FirstOrDefault();

                    long maxReservationDaysWithoutFilters = 1;
                    var groupRule = request.RuleType != null ?
                        rulesRepo.GetGroupRule(RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request))
                        : null;
                    if (appSetting != null)
                        long.TryParse(appSetting.Value, out maxReservationDaysWithoutFilters);

                    //force include reservationaddicionaldata to return selling prices
                    request.IncludeReservationAddicionalData |= (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ReturnSellingPrices));

                    if (request.IncludeGuestPrefixName && !request.IncludeGuests)
                        response.Warnings.Add(new Warning("To include GuestPrefixName it is necessary to include guests."));

                    if ((request.ReservationNumbers == null || !request.ReservationNumbers.Any()) && (request.ReservationUIDs == null || !request.ReservationUIDs.Any()) && (request.PartnerReservationNumbers == null || !request.PartnerReservationNumbers.Any()))
                    {
                        var createdDateFilterExists = request.NestedFilters?.Filters?.Any(f => NestedFilterContainsField(f, nameof(Domain.Reservations.Reservation.CreatedDate))) == true;

                        if (!createdDateFilterExists && (request.DateFrom == null || request.DateTo == null) && request.PageSize <= 0)
                        {
                            request.DateTo = DateTime.UtcNow.Date;
                            request.DateFrom = request.DateTo.Value.AddDays(-maxReservationDaysWithoutFilters).Date;
                        }
                        else if (request.DateFrom != null && request.DateTo != null && (request.DateTo.Value - request.DateFrom.Value).Days > 60 && request.PageSize <= 0)
                        {
                            request.DateTo = DateTime.UtcNow.Date;
                            request.DateFrom = request.DateTo.Value.AddMonths(-2).Date;
                        }
                    }

                    int totalRecords;

                    var reservations = new List<contractsReservations.Reservation>();

                    #region Transaction readuncommited

                    using (var transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
                    {

                        #region Aux reservation filters
                        var reservationsFilterRepo = this.RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                        var convertedFilterRequest = new ListReservationFilterCriteria();

                        OtherConverter.Map(request, convertedFilterRequest);
                        var reservationIds = reservationsFilterRepo.FindByCriteria(convertedFilterRequest, out totalRecords, request.ReturnTotal);

                        request.ReservationUIDs = reservationIds.ToList();
                        #endregion

                        #region Reservation by uids
                        if (request.ReservationUIDs.Any())
                        {

                            List<PaymentGatewayTransaction> listPaymentGatewayTransaction = new List<PaymentGatewayTransaction>();

                            var reservationsRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                            var convertedRequest = OtherConverter.Convert(request);

                            var result = reservationsRepo.FindByCriteria(convertedRequest);

                            if (request.IncludePaymentGatewayTransactions)
                            {
                                var paymentGatewayTransactions = new ListPaymentGatewayTransactionsResponse();

                                paymentGatewayTransactions = ListPaymentGatewayTransactions(new ListPaymentGatewayTransactionsRequest
                                {
                                    PaymentGatewayAutoGeneratedUIDs = result.Select(x => x.Number).ToList(),

                                });

                                if (paymentGatewayTransactions.Result.Any(x => x.PaymentGatewayName != defaultPaymentGateway))
                                {
                                    listPaymentGatewayTransaction = paymentGatewayTransactions.Result.Where(x => x.PaymentGatewayName != defaultPaymentGateway)
                                        .OrderByDescending(x => x.ServerDate)
                                        .GroupBy(x => x.PaymentGatewayAutoGeneratedUID).Select(x => x.ToList().FirstOrDefault())
                                        .ToList();
                                }
                                else
                                {
                                    listPaymentGatewayTransaction = paymentGatewayTransactions.Result
                                        .OrderByDescending(x => x.ServerDate)
                                        .GroupBy(x => x.PaymentGatewayAutoGeneratedUID).Select(x => x.ToList().FirstOrDefault())
                                        .ToList();
                                }

                            }
                            reservations = ConvertReservationToContractWithLookups(request, result, groupRule, listPaymentGatewayTransaction);
                        }
                        #endregion Reservation by uids

                        transactionScope.Complete();
                    }

                    #endregion Transaction readuncommited
                    // Convert Reservation values
                    if (reservations != null && reservations.Any())
                    {
                        if (groupRule == null)
                            response.Result = reservations;
                        else
                            foreach (var item in reservations)
                            {
                                if (groupRule.BusinessRules.HasFlag(BusinessRules.ReturnSellingPrices))
                                    reservationHelper.MapSellingPrices(item, request.IncludeReservationRooms,
                                        request.IncludeReservationRoomDetails);

                                if (groupRule.BusinessRules.HasFlag(BusinessRules.ConvertValuesToRateCurrency))
                                    reservationHelper.ConvertAllValuesToRateCurrency(item);

                                response.Result.Add(item);
                            }
                    }
                    // Now the ConvertReservationToContractWithLookups have this logic
                    //if (request.IncludeReservationRoomIncentivePeriods && !response.Result.IsNullOrEmpty())
                    //  CalculateReservationRoomsIncentivesPeriods(response);

                    response.TotalRecords = totalRecords;
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                var logMsgBefore = new LogMessageBase
                {
                    MethodName = nameof(ListReservations),
                    RequestId = request.RequestId
                };

                Logger.Error(ex, logMsgBefore);
                response.Failed();
                response.Errors.Add(new Error(ex));
            }
            return response;
        }

        private bool NestedFilterContainsField(OB.Reservation.BL.Contracts.Requests.NestedFilterRequestBase filter, string field)
        {
            if (filter == null)
                return false;

            if (filter.FilterBy?.Equals(field) == true)
                return true;

            return filter.Filters?.Any(f => NestedFilterContainsField(f, field)) == true;
        }

        // Will pass in all reservation rooms to check in which dates the incentives are applied. 
        private static void CalculateReservationRoomsIncentivesPeriods(ListReservationResponse response)
        {
            foreach (var reservation in response.Result)
            {
                if (reservation.ReservationRooms.IsNullOrEmpty())
                    continue;

                foreach (var reservationRoom in reservation.ReservationRooms)
                {
                    reservationRoom.ReservationRoomIncentivesPeriods = new List<contractsReservations.ReservationRoomIncentive>();

                    var reservationRoomsIncentives = new List<contractsReservations.ReservationRoomIncentive>();
                    var rrdAppliedIncentives = reservationRoom.ReservationRoomDetails
                        .SelectMany(x => x.ReservationRoomDetailsAppliedIncentives.Where(ri => ri.ReservationRoomDetails_UID == x.UID).Select(y => new { Date = x.Date, AppliedIncentive = y }))
                        .GroupBy(x => x.AppliedIncentive.Incentive_UID);

                    if (rrdAppliedIncentives.IsNullOrEmpty())
                        continue;

                    foreach (var groupByIncentive in rrdAppliedIncentives)
                    {
                        var dates = groupByIncentive.Select(x => x.Date).ToList();
                        var datePeriods = DatePeriodHelper.CompressDatesIntoIntervals(dates);

                        if (datePeriods.IsNullOrEmpty())
                            continue;

                        //Convert the tuple into a DatesRange object.
                        var dateRangeList = new List<DatesRange>();
                        foreach (var datePeriod in datePeriods)
                        {
                            var dateRange = new DatesRange
                            {
                                StartDate = datePeriod.Item1,
                                EndDate = datePeriod.Item2
                            };
                            dateRangeList.Add(dateRange);
                        }

                        var roomIncentiveToAdd = new contractsReservations.ReservationRoomIncentive
                        {
                            IncentiveId = groupByIncentive.Key,
                            Periods = dateRangeList
                        };
                        reservationRoomsIncentives.Add(roomIncentiveToAdd);
                    }
                    reservationRoom.ReservationRoomIncentivesPeriods.AddRange(reservationRoomsIncentives);
                }
            }
        }

        /// <summary>
        /// RESTful implementation of the ListReservationsFilter operation.
        /// This operation searchs for reservations given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListReservationsFilterRequest object containing the Search criteria</param>
        /// <returns>A ListReservationsFilterResponse containing the List of matching Reservation objects</returns>
        public ListReservationsFilterResponse ListReservationsFilter(OB.Reservation.BL.Contracts.Requests.ListReservationsFilterRequest request)
        {
            var response = new ListReservationsFilterResponse();

            response.Result = new ObservableCollection<contractsReservations.ReservationFilter>();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");

                using (var unitOfWork = this.SessionFactory.GetUnitOfWork(true))
                {
                    //Validate request parameter
                    var appRepo = RepositoryFactory.GetOBAppSettingRepository();
                    var appSetting = appRepo.ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string> { "ListReservationsFilterMaxReservation" } }).FirstOrDefault();

                    long maxReservationWithoutPageSize = 0;
                    if (appSetting != null)
                        long.TryParse(appSetting.Value, out maxReservationWithoutPageSize);

                    if (request.PageSize > maxReservationWithoutPageSize)
                        request.PageSize = (int)maxReservationWithoutPageSize;

                    int totalRecords;

                    List<contractsReservations.ReservationFilter> reservations;

                    #region Transaction readuncommited
                    using (var transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
                    {
                        var reservationsFilterRepo = this.RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
                        var convertedFilterRequest = new ListReservationFilterCriteria();
                        OtherConverter.Map(request, convertedFilterRequest);
                        var result = reservationsFilterRepo.FindReservationFilterByCriteria(convertedFilterRequest, out totalRecords, request.ReturnTotal);

                        reservations = result.Select(x => DomainToBusinessObjectTypeConverter.Convert(x, request.IncludeReservationRoomsFilters)).ToList();

                        transactionScope.Complete();
                    }
                    #endregion Transaction readuncommited

                    // Convert Reservation values
                    if (reservations != null && reservations.Any())
                    {
                        response.Result = reservations;
                    }

                    response.TotalRecords = totalRecords;
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }
            return response;
        }

        /// <summary>
        /// RESTful implementation of the ListReservationsByCheckOut operation.
        /// This operation searchs for reservations given a specific request criteria use checkin and checkout parameter to search checkouts in this period.
        /// </summary>
        /// <param name="request">A FindReservationRequest object containing the Search criteria</param>
        /// <returns>A ListReservationResponse containing the List of matching Reservation objects</returns>
        public ListReservationResponse ListReservationsByCheckOut(OB.Reservation.BL.Contracts.Requests.ListReservationRequest request)
        {
            var response = new ListReservationResponse();

            response.Result = new ObservableCollection<contractsReservations.Reservation>();

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");
                response.RequestGuid = request.RequestGuid;

                int totalRecords;
                using (var transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var unitOfWork = this.SessionFactory.GetUnitOfWork();
                    var reservationsRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                    var result = reservationsRepo.FindByCheckOut(out totalRecords, request.ReservationUIDs, request.PropertyUIDs,
                        request.ChannelUIDs, request.ReservationNumbers,
                        request.ReservationStatus,
                        request.CheckIn, request.CheckOut,
                        request.IncludeReservationRooms || request.IncludeRates, request.IncludeReservationRoomChilds,
                        request.IncludeReservationRoomDetails, request.IncludeReservationRoomDetailsAppliedIncentives,
                        request.IncludeReservationRoomExtras, request.IncludeReservationPaymentDetail,
                        request.IncludeReservationPartialPaymentDetails, request.IncludeReservationRoomTaxPolicies || request.IncludeTaxPolicies,
                        request.PageIndex, request.PageSize,
                        request.ReturnTotal);

                    var reservations = ConvertReservationToContractWithLookups(request, result);
                    foreach (var item in reservations)
                        response.Result.Add(item);
                }

                response.TotalRecords = totalRecords;

                response.Succeed();
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }
            return response;
        }

        /// <summary>
        /// RESTful implementation of the ListReservationUIDsByPropertyRateRoomsDateOfModifOrStay operation.
        /// This operation searchs for reservations given a specific request criteria, using property, rate/room, creation or modify date, checkin and checkout periods to search Reservations matching that criteria.
        /// </summary>
        /// <param name="request">A ListReservationUIDsByPropRateRoomAndDateOfModifOrStayRequest object containing the Search criteria</param>
        /// <returns>A ListReservationsUIDsResponse containing the UID's of matching Reservation objects</returns>
        public ListReservationsUIDsResponse ListReservationUIDsByPropertyRateRoomsDateOfModifOrStay(ListReservationUIDsByPropRateDateOfModifOrStayRequest request)
        {
            var response = new ListReservationsUIDsResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            response.Result = new List<long>();

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");
                using (var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationsRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                    response.Result = reservationsRepo.FindReservationUIDSByRateRoomsAndDateOfModifOrStay(request.PropertyUIDs, request.RateUIDs, request.DateFrom, request.DateTo, request.IsDateFindModifications, request.isDateFindArrivals, request.IsDateFindStays).ToList();
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }
            return response;
        }

        /// <summary>
        /// RESTful implementation of the ReservationsLight operation.
        /// This operation searchs for reservations given a specific request criteria, using uid(s), channel(s), property(s), creation or modify date, checkin and checkout periods to search Reservations matching that criteria.
        /// </summary>
        /// <param name="request">A ListReservationsLightRequest object containing the Search criteria</param>
        /// <returns>A ListReservationsLightResponse containing the UID's of matching ReservationLight objects</returns>
        public ListReservationsLightResponse ListReservationsLight(ListReservationsLightRequest request)
        {
            var response = new ListReservationsLightResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                int totalRecords;

                Contract.Requires(request != null, "Request object instance is expected");
                using (var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationsRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                    var result = reservationsRepo.FindReservationsLightByCriteria(out totalRecords, request.UIDs, request.ChannelsUIDs, request.PropertyUIDs, request.DateFrom, request.DateTo, request.IsDateFindModifications, request.isDateFindArrivals, request.IsDateFindStays, request.IncludeReservationRooms, request.PageIndex, request.PageSize, request.ReturnTotal).ToList();

                    //Search for the Properties.
                    var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
                    var propertyUIDs = result.Select(x => x.Property_UID).Distinct().ToList();

                    var properties = propertyRepo.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest { UIDs = propertyUIDs, IsActive = request.IsPropertyActive });

                    //Search for the Currency ISO codes.
                    var currencyRepo = RepositoryFactory.GetOBCurrencyRepository();
                    var currencyUIDs = result.Select(x => x.ReservationBaseCurrency_UID ?? 0).Distinct().ToList();
                    currencyUIDs.AddRange(properties.Select(x => x.BaseCurrency_UID).Distinct().ToList());
                    currencyUIDs = currencyUIDs.Distinct().ToList();
                    var currencyISOs = currencyRepo.ListCurrencies(new Contracts.Requests.ListCurrencyRequest { UIDs = currencyUIDs }).Select(x => new
                    {
                        UID = x.UID,
                        Symbol = x.Symbol
                    }).ToDictionary(x => x.UID, x => x.Symbol);

                    var currencySymbols = currencyRepo.ListCurrencies(new Contracts.Requests.ListCurrencyRequest { UIDs = currencyUIDs }).Select(x => new
                    {
                        UID = x.UID,
                        CurrencySymbol = x.CurrencySymbol
                    }).ToDictionary(x => x.UID, x => x.CurrencySymbol);



                    response.Result = new ObservableCollection<contractsReservations.ReservationLight>(result.Select(x => DomainToBusinessObjectTypeConverter.ConvertToLight(x)));

                    foreach (var reservation in response.Result)
                    {
                        var property = properties.SingleOrDefault(x => x.UID == reservation.Property_UID);

                        if (request.IsPropertyActive && property == null)
                            continue; //Skip this reservation

                        if (reservation.ReservationBaseCurrency_UID == null)
                        {
                            reservation.ReservationBaseCurrency_UID = property.BaseCurrency_UID;
                            reservation.ReservationBaseCurrency_ISO = currencyISOs[property.BaseCurrency_UID];
                            reservation.ReservationBaseCurrency_Symbol = currencySymbols[property.BaseCurrency_UID];
                        }
                        else
                        {
                            reservation.ReservationBaseCurrency_ISO = currencyISOs[(long)reservation.ReservationBaseCurrency_UID];
                            reservation.ReservationBaseCurrency_Symbol = currencySymbols[(long)reservation.ReservationBaseCurrency_UID];
                        }
                    }
                }

                response.TotalRecords = totalRecords;

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }
            return response;
        }

        private List<contractsReservations.Reservation> ConvertReservationToContractWithLookups(OB.Reservation.BL.Contracts.Requests.ListReservationRequest request, IEnumerable<domainReservations.Reservation> result, GroupRule groupRule = null, List<PaymentGatewayTransaction> paymentGatewayTransactions = null)
        {
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();

            var lookups = reservationHelper.GetReservationLookups(OtherConverter.Convert(request), result);

            List<contractsReservations.Reservation> finalResult = new List<contractsReservations.Reservation>();

            // Convert to Contracts
            var convertedResult = result.Select(x =>
            {
                return DomainToBusinessObjectTypeConverter.Convert(x, request, paymentGatewayTransactions);
            });

            foreach (var item in convertedResult)
            {

                // Reservation Level             
                string transactionId = string.Empty;
                lookups.TransactionsLookup.TryGetValue(item.UID, out transactionId);
                item.TransactionId = transactionId;

                var cancelationCosts = new List<Reservation.BL.Contracts.Data.Reservations.ReservationRoomCancelationCost>();
                if (request.IncludeCancelationCosts)
                {
                    var reservationHelperPoco = Resolve<IReservationHelperPOCO>();
                    cancelationCosts = reservationHelperPoco.GetCancelationCosts(item, groupRule?.BusinessRules.HasFlag(BusinessRules.ConvertValuesToRateCurrency) ?? false);
                }

                if (request.IncludeReservationAddicionalData && lookups.ReservationsAdditionalData.TryGetValue(item.UID, out ReservationsAdditionalData tmpAdditionalData)
                    && !string.IsNullOrEmpty(tmpAdditionalData.ReservationAdditionalDataJSON))
                    item.ReservationAdditionalData = JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(tmpAdditionalData.ReservationAdditionalDataJSON);

                if (request.IncludeGuests && lookups.GuestsLookup.TryGetValue(item.Guest_UID, out Contracts.Data.CRM.Guest guest))
                    item.Guest = OtherConverter.Convert(guest);

                if (request.IncludeGuestStateName && item.GuestState_UID > 0)
                {
                    string outParam = null;
                    lookups.StateNameLookup.TryGetValue(item.GuestState_UID.Value, out outParam);
                    item.GuestStateName = outParam;
                }

                if (request.IncludePromotionalCodes && item.PromotionalCode_UID.HasValue)
                    item.PromotionalCode = OtherConverter.Convert(lookups.PromotionalCodesLookup[item.PromotionalCode_UID.Value]);

                if (request.IncludeGroupCodes && item.GroupCode_UID.HasValue)
                    item.GroupCode = OtherConverter.Convert(lookups.GroupCodesLookup[item.GroupCode_UID.Value]);

                if (request.IncludeGuestActivities && lookups.GuestActivitiesLookup.ContainsKey(item.Guest_UID))
                    item.GuestActivities = lookups.GuestActivitiesLookup[item.Guest_UID];

                if (request.IncludeBESpecialRequests && item.BESpecialRequests1_UID.HasValue)
                {
                    string outParam = null;
                    lookups.BESpecialRequestsLookup.TryGetValue(item.BESpecialRequests1_UID.Value, out outParam);
                    item.BESpecialRequest1 = outParam;
                }

                if (request.IncludeBESpecialRequests && item.BESpecialRequests2_UID.HasValue)
                {
                    string outParam = null;
                    lookups.BESpecialRequestsLookup.TryGetValue(item.BESpecialRequests2_UID.Value, out outParam);
                    item.BESpecialRequest2 = outParam;
                }

                if (request.IncludeBESpecialRequests && item.BESpecialRequests3_UID.HasValue)
                {
                    string outParam = null;
                    lookups.BESpecialRequestsLookup.TryGetValue(item.BESpecialRequests3_UID.Value, out outParam);
                    item.BESpecialRequest3 = outParam;
                }

                if (request.IncludeBESpecialRequests && item.BESpecialRequests4_UID.HasValue)
                {
                    string outParam = null;
                    lookups.BESpecialRequestsLookup.TryGetValue(item.BESpecialRequests4_UID.Value, out outParam);
                    item.BESpecialRequest4 = outParam;
                }

                if (request.IncludeTransferLocation && item.TransferLocation_UID.HasValue)
                {
                    contractsProperties.TransferLocation outParam = null;
                    lookups.TransferLocationsLookup.TryGetValue(item.TransferLocation_UID.Value, out outParam);
                    if (outParam != null)
                    {
                        item.TransferLocation = outParam.Name;
                        item.TransferLocationPrice = outParam.Price;
                    }
                }

                if (request.IncludeReservationBaseCurrency && item.ReservationBaseCurrency_UID.HasValue)
                {
                    Currency outParam = null;
                    lookups.ReservationBaseCurrencyLookup.TryGetValue(item.ReservationBaseCurrency_UID.Value, out outParam);
                    if (outParam != null)
                    {
                        item.ReservationBaseCurrency = outParam;
                        item.ReservationBaseCurrencySymbol = outParam.Symbol;
                        item.ReservationBaseCurrency_UID = outParam.UID;
                    }
                }

                if (request.IncludeReservationCurrency && item.ReservationCurrency_UID.HasValue)
                {
                    Currency outParam = null;
                    lookups.ReservationCurrencyLookup.TryGetValue(item.ReservationCurrency_UID.Value, out outParam);
                    item.ReservationCurrency = outParam;
                }

                if (request.IncludeReservationStatusName
                    && string.IsNullOrEmpty(item.ReservationStatusName)
                    && lookups.ReservationStatusNameLookup != null)
                {
                    string outParam = null;
                    lookups.ReservationStatusNameLookup.TryGetValue(item.Status, out outParam);
                    item.ReservationStatusName = outParam;
                }

                if (request.IncludeChannel && item.Channel_UID.HasValue)
                {
                    Channel outParam = null;
                    lookups.ChannelLookup.TryGetValue(item.Channel_UID.Value, out outParam);
                    item.Channel = outParam.Clone();

                    if (item.Channel != null && outParam.IsBookingEngine)
                        item.Channel.Name = (item.IsMobile.HasValue && item.IsMobile.Value) ? item.Channel.Name + " " + Resources.Resources.lblMobile : item.Channel.Name;
                }

                if (request.IncludeChannelOperator && item.Channel_UID.HasValue)
                {
                    ChannelOperator outParam = null;
                    lookups.ChannelOperatorLookup.TryGetValue(item.Channel_UID.Value, out outParam);
                    item.ChannelOperator = outParam;
                }

                if (request.IncludePropertyBaseCurrency && item.Property_UID > 0)
                {
                    Currency outParam = null;
                    lookups.ReservationPropertyBaseCurrencyLookup.TryGetValue(item.Property_UID, out outParam);
                    if (outParam != null)
                    {
                        item.PropertyBaseCurrencySymbol = outParam.CurrencySymbol;
                        item.PropertyCurrency = outParam;
                    }
                }

                if (request.IncludeReservationReadStatus && lookups.ReservationReadStatusLookup != null)
                {
                    ReservationReadStatus outParam = null;
                    lookups.ReservationReadStatusLookup.TryGetValue(item.UID, out outParam);
                    item.ReservationReadStatus = outParam;
                }

                if ((request.IncludeTPIName || request.IncludeTPILanguageUID) && item.TPI_UID > 0)
                {
                    Tuple<string, string, long> outParam = null;
                    lookups.CorporateAndTravelAgentLookup.TryGetValue(item.TPI_UID.Value, out outParam);
                    if (outParam != null)
                    {
                        item.ReservationCorporateName = outParam.Item1;
                        item.ReservationTPIName = outParam.Item2;
                        item.ReservationTPILanguageUID = outParam.Item3;
                    }
                }

                if (request.IncludeCompanyName && item.Company_UID.HasValue)
                {
                    string outParam = null;
                    lookups.CompanyNameLookup.TryGetValue(item.Company_UID.Value, out outParam);
                    item.ReservationCompanyName = outParam;
                }

                if (request.IncludeReservationPaymentDetail && item.ReservationPaymentDetail != null && item.ReservationPaymentDetail.PaymentMethod_UID.HasValue)
                {
                    string outParam = null;
                    lookups.PaymentMethodLookup.TryGetValue(item.ReservationPaymentDetail.PaymentMethod_UID.Value, out outParam);
                    item.ReservationPaymentDetail.PaymentMethodName = outParam;
                }

                if (request.IncludePaymentMethodType && item.PaymentMethodType_UID.HasValue)
                {
                    PaymentMethodType outParam = null;
                    lookups.PaymentMethodTypesLookup.TryGetValue(item.PaymentMethodType_UID.Value, out outParam);
                    item.PaymentMethodType = outParam;
                }

                if (request.IncludeGuestCountryName && item.GuestCountry_UID > 0)
                {
                    OB.BL.Contracts.Data.General.Country outParam = null;
                    lookups.CountryLookup.TryGetValue(item.GuestCountry_UID.Value, out outParam);
                    item.GuestCountryName = outParam?.Name;
                }

                if (request.IncludeBillingCountryName && item.BillingCountry_UID > 0)
                {
                    OB.BL.Contracts.Data.General.Country outParam = null;
                    lookups.CountryLookup.TryGetValue(item.BillingCountry_UID.Value, out outParam);
                    item.BillingCountry_Name = outParam?.Name;
                }

                if (request.IncludePropertyCountryCode && lookups.PropertyCountryLookup.TryGetValue(item.Property_UID, out long propertyCountryId))
                {
                    lookups.CountryLookup.TryGetValue(propertyCountryId, out var outParam);
                    item.PropertyCountryCode = outParam?.CountryCode;
                }

                if (request.IncludeBillingStateName && item.BillingState_UID > 0)
                {
                    string outParam = null;
                    lookups.StateNameLookup.TryGetValue(item.BillingState_UID.Value, out outParam);
                    item.BillingState_Name = outParam;
                }

                if (request.IncludeOnRequestDecisionUser && item.OnRequestDecisionUser.HasValue)
                {
                    User outParam = null;
                    lookups.OnRequestDecisionUserLookup.TryGetValue(item.OnRequestDecisionUser.Value, out outParam);
                    item.OnRequestDecisionGuest = outParam;
                }

                if (request.IncludeReferralSource && item.ReferralSourceId.HasValue)
                {
                    string outParam = null;
                    lookups.ReferralSourcesLookup.TryGetValue(item.ReferralSourceId.Value, out outParam);
                    item.ReferralSourceName = outParam;
                }

                if (request.IncludeExternalSource && item.ExternalSource_UID.HasValue)
                {
                    string outParam = null;
                    lookups.ExternalSourceLookup.TryGetValue(item.ExternalSource_UID.Value, out outParam);
                    item.ExternalSourceName = outParam;
                }

                if (request.IncludeTPICommissions && item.TPI_UID.HasValue)
                {
                    TPICommission outParam = null;
                    lookups.TPICommissionLookup.TryGetValue(item.UID, out outParam);
                    item.TPICommission = outParam;
                }

                // Reservation Resume Infos
                if (request.IncludeReservationResumeInfo && item.ReservationRooms != null)
                {
                    item.Resume_TotalNrAdults = item.ReservationRooms.Sum(room => room.AdultCount ?? 0);
                    item.Resume_TotalNrChilds = item.ReservationRooms.Sum(room => room.ChildCount ?? 0);

                    var checkinDate = new DateTime();
                    var checkoutDate = new DateTime();

                    checkinDate = DateTime.MaxValue;
                    checkoutDate = DateTime.MinValue;

                    bool ignoreCancelledRooms = item.ReservationRooms.Any(x =>
                        x.Status != (int)Constants.ReservationStatus.Cancelled &&
                        x.Status != (int)Constants.ReservationStatus.CancelledOnRequest &&
                        x.Status != (int)Constants.ReservationStatus.OnRequestChannelCancel);

                    foreach (var room in item.ReservationRooms)
                    {
                        if (ignoreCancelledRooms &&
                            (room.Status == (int)Constants.ReservationStatus.Cancelled ||
                             room.Status == (int)Constants.ReservationStatus.CancelledOnRequest ||
                             room.Status == (int)Constants.ReservationStatus.OnRequestChannelCancel))
                            continue;

                        if (room.DateFrom.HasValue && checkinDate.CompareTo(room.DateFrom) > 0)
                        {
                            item.Resume_CheckIn = room.DateFrom.Value;
                            checkinDate = room.DateFrom.Value;
                        }

                        if (room.DateTo.HasValue && checkoutDate.CompareTo(room.DateTo) < 0)
                        {
                            item.Resume_CheckOut = room.DateTo.Value;
                            checkoutDate = room.DateTo.Value;
                        }
                    }

                    if (item.Resume_CheckIn.HasValue && item.Resume_CheckOut.HasValue)
                    {
                        item.Resume_NrNights = item.ReservationRooms
                            .Where(x => x.DateFrom.HasValue && x.DateTo.HasValue)
                            .Select(x => new { x.DateFrom, x.DateTo })
                            .Distinct()
                            .Sum(x => x.DateTo.Value.Subtract(x.DateFrom.Value).Days);
                    }
                }

                // Room Level
                if (item.ReservationRooms != null)
                {
                    foreach (var room in item.ReservationRooms)
                    {
                        if (request.IncludeCancelationCosts)
                        {
                            room.ReservationRoomCancelationCost =
                                cancelationCosts.FirstOrDefault(x => x.Number == room.ReservationRoomNo);
                        }

                        if (request.IncludeReservationAddicionalData && item.ReservationAdditionalData != null && item.ReservationAdditionalData.ReservationRoomList != null)
                            room.ReservationRoomAdditionalData = item.ReservationAdditionalData.ReservationRoomList.FirstOrDefault(x => x.ReservationRoom_UID == room.UID);

                        if (request.IncludeRates)
                        {
                            var rate = new contractsRates.Rate();

                            if (room.Rate_UID.HasValue && lookups.RatesLookup != null && lookups.RatesLookup.TryGetValue(room.Rate_UID.Value, out rate))
                            {
                                room.Rate = OtherConverter.Convert(rate);

                                if (request.IncludeRateCategories &&
                                    lookups.RateCategoriesLookup != null &&
                                    lookups.RateCategoriesLookup.TryGetValue(rate.RateCategoryId, out contractsRates.RateCategory rateCategory))
                                {
                                    if (request.IncludeOtaCodes &&
                                        rateCategory.OtaCodeId.HasValue &&
                                        lookups.OtaCodesLookup != null &&
                                        lookups.OtaCodesLookup.TryGetValue(rateCategory.OtaCodeId.Value, out contractsRates.OtaCode otaCode))
                                    {
                                        room.Rate.OtaCodeValue = otaCode.CodeValue;
                                    }
                                }
                            }

                            if (room.ReservationRoomDetails != null)
                                foreach (var rrd in room.ReservationRoomDetails.Where(x => (x.Rate_UID ?? 0) > 0).ToList())
                                {
                                    if (lookups.RatesLookup.TryGetValue(rrd.Rate_UID.Value, out rate))
                                        rrd.Rate = OtherConverter.Convert(rate);
                                }
                        }

                        var roomTemp = new contractsProperties.RoomType();
                        if (request.IncludeRoomTypes && room.RoomType_UID.HasValue && lookups.RoomTypeLookup.TryGetValue(room.RoomType_UID.Value, out roomTemp))
                            room.RoomType = OtherConverter.Convert(roomTemp);

                        if (request.IncludeExtras)
                        {
                            contractsRates.Extra extraTemp;
                            var extraBillingListTemp = new List<contractsRates.ExtrasBillingType>();
                            foreach (var extra in room.ReservationRoomExtras)
                            {
                                if (lookups.ExtrasLookup.TryGetValue(extra.Extra_UID, out extraTemp))
                                {
                                    extra.Extra = OtherConverter.Convert(extraTemp);
                                    if (request.IncludeExtrasBillingTypes && lookups.ExtrasBillingTypeLookup.TryGetValue(extra.Extra_UID, out extraBillingListTemp))
                                        extra.Extra.BillingTypes.AddRange(extraBillingListTemp.Select(x => OtherConverter.Convert(x)).ToList());
                                }
                            }
                        }

                        if (request.IncludeTaxPolicies)
                        {
                            var taxTemp = new contractsRates.TaxPolicy();
                            foreach (var tax in room.ReservationRoomTaxPolicies)
                            {
                                if (tax.TaxId.HasValue && lookups.TaxPoliciesLookup.TryGetValue(tax.TaxId.Value, out taxTemp))
                                    tax.TaxPolicy = OtherConverter.Convert(taxTemp);
                            }
                        }

                        if (request.IncludeIncentives)
                        {
                            contractsProperties.Incentive incentiveTemp;
                            foreach (var rrd in room.ReservationRoomDetails)
                            {
                                foreach (var incentive in rrd.ReservationRoomDetailsAppliedIncentives ?? Enumerable.Empty<contractsReservations.ReservationRoomDetailsAppliedIncentive>())
                                {
                                    if (lookups.IncentivesLookup.TryGetValue(incentive.Incentive_UID, out incentiveTemp))
                                        incentive.Incentive = OtherConverter.Convert(incentiveTemp);
                                }
                            }
                        }

                        if (request.IncludeCommissionTypeName && room.CommissionType.HasValue)
                        {
                            string outParam = null;
                            lookups.CommissionTypeNamesLookup.TryGetValue(room.CommissionType.Value, out outParam);
                            room.CommissionTypeName = outParam;
                        }

                        if (request.IncludeReservationRoomIncentivePeriods)
                        {
                            room.ReservationRoomIncentivesPeriods = new List<contractsReservations.ReservationRoomIncentive>();

                            var reservationRoomsIncentives = new List<contractsReservations.ReservationRoomIncentive>();
                            var rrdAppliedIncentives = room.ReservationRoomDetails.Where(x => x.ReservationRoomDetailsAppliedIncentives != null)
                                .SelectMany(x => x.ReservationRoomDetailsAppliedIncentives.Where(ri => ri.ReservationRoomDetails_UID == x.UID).Select(y => new { Date = x.Date, AppliedIncentive = y }))
                                .GroupBy(x => x.AppliedIncentive.Incentive_UID).ToList();

                            if (rrdAppliedIncentives.IsNullOrEmpty())
                                continue;

                            foreach (var groupByIncentive in rrdAppliedIncentives)
                            {
                                var dates = groupByIncentive.Select(x => x.Date).ToList();
                                var datePeriods = DatePeriodHelper.CompressDatesIntoIntervals(dates);

                                if (datePeriods.IsNullOrEmpty())
                                    continue;

                                //Convert the tuple into a DatesRange object.
                                var dateRangeList = new List<DatesRange>();
                                foreach (var datePeriod in datePeriods)
                                {
                                    var dateRange = new DatesRange
                                    {
                                        StartDate = datePeriod.Item1,
                                        EndDate = datePeriod.Item2
                                    };
                                    dateRangeList.Add(dateRange);
                                }

                                var roomIncentiveToAdd = new contractsReservations.ReservationRoomIncentive
                                {
                                    IncentiveId = groupByIncentive.Key,
                                    Periods = dateRangeList
                                };
                                reservationRoomsIncentives.Add(roomIncentiveToAdd);
                            }
                            room.ReservationRoomIncentivesPeriods.AddRange(reservationRoomsIncentives);
                        }
                    }
                }

                finalResult.Add(item);
            }

            return finalResult;
        }

        /// <summary>
        /// RESTful implementation of the ListReservationHistories operation.
        /// This operation searchs for reservations given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListReservationHistoryRequest object containing the Search criteria</param>
        /// <returns>A ListReservationHistoryResponse containing the List of matching ReservationHistory objects</returns>
        public ListReservationHistoryResponse ListReservationHistories(ListReservationHistoryRequest request)
        {
            var response = new ListReservationHistoryResponse();
            response.Result = new ObservableCollection<contractsReservations.ReservationHistory>();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");

                int totalRecords;
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationsRepo = this.RepositoryFactory.GetReservationHistoryRepository(unitOfWork);
                    var reservationStatusLangRepo = this.RepositoryFactory.GetRepository<ReservationStatusLanguage>(unitOfWork);

                    var result = reservationsRepo.FindByCriteria(out totalRecords,
                        request.ReservationHistoryUIDs,
                        request.ReservationUIDs,
                        request.ReservationNumbers,
                        request.Statuses,
                        request.MinChangedDate,
                        request.MaxChangedDate,
                        request.PageIndex, request.PageSize,
                        request.ReturnTotal);

                    if (request.LanguageUID.HasValue)
                    {
                        var reservationStatusUIDs = result.Where(x => x.StatusUID.HasValue).Select(x => x.StatusUID).ToList();
                        var reservationStatusNamesDic = reservationStatusLangRepo.GetQuery(x => reservationStatusUIDs.Contains(x.ReservationStatus_UID) && request.LanguageUID == x.Language_UID)
                            .ToDictionary(x => x.ReservationStatus_UID, x => x.Name);

                        if (reservationStatusNamesDic.Any())
                        {
                            string status = string.Empty;
                            foreach (var item in result)
                                item.Status = reservationStatusNamesDic.TryGetValue(item.StatusUID, out status) ? status : string.Empty;
                        }
                    }

                    response.Result = new ObservableCollection<contractsReservations.ReservationHistory>(result.Select(x => QueryResultObjectToBusinessObjectTypeConverter.Convert(x)));
                }

                response.TotalRecords = totalRecords;

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }
            return response;
        }

        /// <summary>
        /// RESTful implementation of the Get Reservation numbers operation.
        /// Gets a list of reservations numbers to one or multiples properties
        /// </summary>
        /// <param name="request">ListReservationNumbersRequest instance</param>
        /// <returns>A ListReservationNumbersResponse object that contains reservation numbers for one or multiple properties</returns>
        public ListReservationNumbersResponse ListReservationNumbers(ListReservationNumbersRequest request)
        {
            var response = new ListReservationNumbersResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                response.Numbers = new Dictionary<long, List<string>>();

                try
                {
                    Contract.Requires(request != null, "Request object instance is expected");

                    if (request.Values.Count > 0)
                    {
                        foreach (var item in request.Values)
                        {
                            response.Numbers.Add(item.Key, new List<string>());
                            for (int i = 0; i < item.Value; i++)
                            {
                                var number = reservationRepo.GenerateReservationNumber(item.Key);
                                response.Numbers[item.Key].Add(number);
                            }
                        }
                    }

                    response.Succeed();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                    response.Failed();
                    response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
                }
            }
            return response;
        }


        /// <summary>
        /// RESTful implementation of the Get Properties with Reservations for the given Channel_UID and TPI_UID.
        /// Gets a list of Properties UID's and Names's
        /// </summary>
        /// <param name="request">ListPropertiesWithReservationsForChannelOrTPIRequest instance</param>
        /// <returns>A ListPropertiesWithReservationsForChannelOrTPIResponse object that contains a list of Properties UID's and Names's</returns>
        public ListPropertiesWithReservationsForChannelOrTPIResponse ListPropertiesWithReservationsForChannelOrTPI(ListPropertiesWithReservationsForChannelOrTPIRequest request)
        {
            var response = new ListPropertiesWithReservationsForChannelOrTPIResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null || request.Channel_UID > 0, "Request object instance is expected");

                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                    response.Result = new List<contractsReservations.PropertyWithReservationsForChannelOrTPI>(reservationRepo.FindPropertiesWithReservationsForChannelOrTPI(request.Channel_UID, request.TPI_UID, request.PropertyUIDs).Select(x => QueryResultObjectToBusinessObjectTypeConverter.Convert(x)).ToList());
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }

            return response;
        }

        /// <summary>
        /// RESTful implementation of the Get Properties with Reservations for the given multiple Channels.
        /// Gets a list of Properties UID's and Names's
        /// </summary>
        /// <param name="request">ListPropertiesWithReservationsForChannelsRequest instance</param>
        /// <returns>A ListPropertiesWithReservationsForChannelOrTPIResponse object that contains a list of Properties UID's and Names's</returns>
        public ListPropertiesWithReservationsForChannelOrTPIResponse ListPropertiesWithReservationsForChannelsTpis(ListPropertiesWithReservationsForChannelsRequest request)
        {
            var response = new ListPropertiesWithReservationsForChannelOrTPIResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null || request.ChannelsTpis.Count > 0, "Request object instance is expected");

                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                    response.Result = new List<contractsReservations.PropertyWithReservationsForChannelOrTPI>(reservationRepo.FindPropertiesWithReservationsForChannelsTpis(request.PropertyUIDs).Select(x => QueryResultObjectToBusinessObjectTypeConverter.Convert(x)).ToList());

                    response.Result.AddRange(response.Result);
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }

            return response;
        }

        public GetCancelationCostsResponse GetCancelationCosts(GetCancelationCostsRequest request)
        {
            var response = new GetCancelationCostsResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null && request.Reservation != null, "Request object instance is expected");

                if (request.Reservation == null || request.Reservation.ReservationRooms == null)
                {
                    response.Failed();
                    return response;
                }

                var reservationHelperPoco = Resolve<IReservationHelperPOCO>();

                response.ReservationRoomsCancelationCosts = reservationHelperPoco.GetCancelationCosts(request.Reservation, request.ConvertToRateCurrency, request.CancellationDate);

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }

            return response;
        }

        public GetDepositCostsResponse GetDepositCosts(GetDepositCostsRequest request)
        {
            var response = new GetDepositCostsResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null && request.Reservation != null, "Request object instance is expected");

                if (request.Reservation == null || request.Reservation.ReservationRooms == null || request.Reservation.ReservationAdditionalData == null)
                {
                    response.Failed();
                    return response;
                }

                var reservationHelperPoco = Resolve<IReservationHelperPOCO>();

                var internalValues = reservationHelperPoco.GetDepositCosts(request.Reservation, null, request.Reservation.ReservationAdditionalData, request.DepositDate);
                response.ReservationRoomsDepositCosts = internalValues.Select(x =>
                {
                    return new contractsReservations.ReservationRoomDepositCost
                    {
                        DepositCosts = x.DepositCosts,
                        Number = x.Number,
                        Status = x.Status
                    };
                }).ToList();

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new Error(ex));
            }

            return response;
        }

        public InsertInReservationInternalNotesResponse InsertInReservationInternalNotes(InsertInReservationInternalNotesRequest request)
        {
            var response = new InsertInReservationInternalNotesResponse
            {
                RequestId = request.RequestId
            };

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationsRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                    var reservations = reservationsRepo.FindReservationByNumber(request.ReservationNumber);

                    if (reservations.Any())
                    {
                        var reservation = reservations.OrderByDescending(x => x.UID).FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(reservation.InternalNotes))
                            reservation.InternalNotes += "\n" + request.NotesToApped;
                        else
                            reservation.InternalNotes += request.NotesToApped;

                        if (reservation.InternalNotesHistory == null)
                        {
                            reservation.InternalNotesHistory = DateTime.UtcNow + ";" + request.NotesToApped;
                        }
                        else
                        {
                            string internalNotesHistory =
                                reservation.InternalNotesHistory
                                + "----------------------------------------------------;"
                                + DateTime.UtcNow + ";"
                                + request.NotesToApped;
                            reservation.InternalNotesHistory = internalNotesHistory;
                        }

                        unitOfWork.Save();
                    }
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new Error(ex));
            }

            return response;
        }

        public CalculateReservationRoomPricesResponse CalculateReservationRoomPrices(CalculateReservationRoomPricesRequest request)
        {
            var response = new CalculateReservationRoomPricesResponse();

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var pricesCalulcationPoco = Resolve<IReservationPricesCalculationPOCO>();
                    var groupRuleRepo = RepositoryFactory.GetGroupRulesRepository(unitOfWork);

                    var parameters = Newtonsoft.Json.JsonConvert.DeserializeObject<CalculateFinalPriceParameters>(Newtonsoft.Json.JsonConvert.SerializeObject(request));
                    if (request.RuleType.HasValue)
                        parameters.GroupRule = groupRuleRepo.GetGroupRule(RequestToCriteriaConverters.ConvertToGroupRuleCriteria(request));

                    var result = pricesCalulcationPoco.CalculateReservationRoomPrices(parameters);

                    response.ReservationRateRoomDetails = result.Select(x =>
                        Newtonsoft.Json.JsonConvert.DeserializeObject<OB.Reservation.BL.Contracts.Data.Rates.RateRoomDetailReservation>(Newtonsoft.Json.JsonConvert.SerializeObject(x))).ToList();
                    response.Succeed();
                }
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }

            return response;
        }

        public HaveGuestExceededLoyaltyDiscountResponse HaveGuestExceededLoyaltyDiscount(HaveGuestExceededLoyaltyDiscountRequest request)
        {
            var response = new HaveGuestExceededLoyaltyDiscountResponse();
            var reservationValidator = Resolve<IReservationValidatorPOCO>();

            try
            {
                if (!request.Guest_UID.HasValue)
                    throw Errors.GuestIdIsRequired.ToBusinessLayerException();

                response.Result = reservationValidator.HaveGuestExceededLoyaltyDiscount(request.Guest_UID ?? 0);
                response.Succeed();
            }
            catch (BusinessLayerException ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex.ErrorType, ex.ErrorCode, ex.Message));
                Logger.Warn(ex);
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
                Logger.Error(ex);
            }

            return response;
        }

        public CalculateGuestPastReservationsValuesResponse CalculateGuestPastReservationsValues(CalculateGuestPastReservationsValuesRequest request)
        {
            var response = new CalculateGuestPastReservationsValuesResponse();
            var reservationValidator = Resolve<IReservationValidatorPOCO>();

            try
            {
                var result = reservationValidator.CalculateGuestPastReservationsValues(request.Guest_UID, request.PeriodicityLimitType,
                    request.PeriodicityLimitValue, request.LoyaltyLevelBaseCurrency_UID);

                response.ReservationsCount = result.ReservationsCount;
                response.ReservationsTotalAmount = result.ReservationsTotalAmount;
                response.RoomNightsCount = result.RoomNightsCount;

                response.Succeed();
            }
            catch (BusinessLayerException ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex.ErrorType, ex.ErrorCode, ex.Message));
                Logger.Warn(ex);
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
                Logger.Error(ex);
            }

            return response;
        }

        public ValidatePromocodeForReservationResponse ValidatePromocodeForReservation(ValidatePromocodeForReservationRequest request)
        {
            var response = new ValidatePromocodeForReservationResponse();

            try
            {
                var reservationHelperPoco = Resolve<IReservationHelperPOCO>();

                var parameters = JsonConvert.DeserializeObject<ValidatePromocodeForReservationParameters>(JsonConvert.SerializeObject(request));
                var result = reservationHelperPoco.ValidatePromocodeForReservation(parameters);

                response = JsonConvert.DeserializeObject<ValidatePromocodeForReservationResponse>(JsonConvert.SerializeObject(result));
                response.Succeed();
            }
            catch (BusinessLayerException ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex.ErrorType, ex.ErrorCode, ex.Message));
                Logger.Warn(ex);
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
                Logger.Error(ex);
            }

            return response;
        }

        public GetExchangeRatesBetweenCurrenciesResponse GetExchangeRatesBetweenCurrencies(GetExchangeRatesBetweenCurrenciesRequest request)
        {
            var response = new GetExchangeRatesBetweenCurrenciesResponse();

            try
            {
                var reservationHelperPoco = Resolve<IReservationHelperPOCO>();

                response.ExchangeRate = reservationHelperPoco.GetExchangeRateBetweenCurrenciesByPropertyId(request.BaseCurrencyUid, request.CurrencyUid, request.PropertyUid);
                response.Succeed();
            }
            catch (BusinessLayerException ex)
            {
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex.ErrorType, ex.ErrorCode, ex.Message));
                Logger.Warn(ex);
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new Error(ex));
                Logger.Error(ex, "@GetExchangeRatesBetweenCurrencies", request.ToJSON());
            }

            return response;
        }

        public UpdateReservationVCNResponse UpdateReservationVCN(UpdateReservationVCNRequest request)
        {
            // If no request sent
            if (request == null)
                return new UpdateReservationVCNResponse { Errors = new List<Error> { Errors.NoRequest.ToContractError() } };

            var response = new UpdateReservationVCNResponse
            {
                RequestGuid = request.RequestGuid,
                RequestId = request.RequestId,
                Status = Status.Fail
            };

            try
            {
                // Validate request
                if (!IsValidRequest(request, out var errors))
                {
                    response.Errors = errors;
                    return response;
                }

                int updatedResults;
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                    var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
                    var securityRepo = RepositoryFactory.GetOBSecurityRepository();

                    // Get information about property security
                    long propertyId = reservationRepo.GetPropertyIdByReservationId(request.ReservationId);
                    var propertySecurityRequest = new ListPropertySecurityConfigurationRequest
                    {
                        PropertyUIDs = new List<long> { propertyId }
                    };
                    var propertySecurityInfo = propertyRepo.GetPropertySecurityConfiguration(propertySecurityRequest);

                    // Ignore VCN card if Property have Credit Cards Protected with Omnibees
                    if (propertySecurityInfo?.Any(x => x.IsProtectedWithOmnibees == true) == true)
                    {
                        string warnMsg = $"VCN was not generated for Reservation ID: '{request.ReservationId}' because the Property ID: '{propertyId}' have the flag IsProtectedWithOmnibees = true.";
                        Logger.Warn(new LogMessageBase
                        {
                            RequestId = request.RequestId,
                            MethodName = nameof(UpdateReservationVCN),
                            ControllerName = nameof(ReservationManagerPOCO),
                            Description = warnMsg
                        });
                        response.Warnings.Add(new Warning(warnMsg));
                        response.Succeed();
                        return response;
                    }

                    // Encrypt Card Number and CVV
                    var encryptRequest = new ListCreditCardRequest
                    {
                        CreditCards = new List<string> { request.CreditCardNumber, request.CreditCardCVV }
                    };
                    var encryptedCardNumbers = securityRepo.EncryptCreditCards(encryptRequest);
                    if (encryptedCardNumbers?.Count != 2)
                    {
                        response.Errors.Add(Errors.DefaultError.ToContractError(request.RequestId));
                        return response;
                    }

                    string encryptedNumber = encryptedCardNumbers.First();
                    string encryptedCvv = encryptedCardNumbers.Last();

                    // The Expiration date corresponds to the last day of month
                    var expirationDate = new DateTime(
                            request.CreditCardExpirationDate.Year,
                            request.CreditCardExpirationDate.Month,
                            DateTime.DaysInMonth(request.CreditCardExpirationDate.Year, request.CreditCardExpirationDate.Month));

                    // Credit Card HashCode
                    var hashRequest = new Contracts.Requests.GetCreditCardHashRequest()
                    {
                        CardHolder = request.CreditCardHolderName,
                        EncryptedCardNumber = encryptedNumber,
                        EncryptedCVV = encryptedCvv,
                        CardExpiration = expirationDate
                    };
                    var hashCode = securityRepo.GetCreditCardHash(hashRequest);

                    // Updates the VCN info and updates ModifiedDate of Reservations table
                    var criteria = new UpdateReservationVcnCriteria
                    {
                        ReservationId = request.ReservationId,
                        VcnReservationId = request.VcnReservationId,
                        VcnToken = request.VcnTokenId,
                        CreditCardHolderName = request.CreditCardHolderName,
                        CreditCardNumber = encryptedNumber,
                        CreditCardCVV = encryptedCvv,
                        CreditCardExpirationDate = expirationDate,
                        CreditCardHashCode = hashCode
                    };
                    updatedResults = reservationRepo.UpdateReservationVcn(criteria);
                }

                // Reservation doesn't exists
                if (updatedResults == -1)
                {
                    var warning = new Warning(nameof(Errors.ReservationDoesNotExist), (int)Errors.ReservationDoesNotExist,
                        $"Reservation with ID: {request.ReservationId} not found");
                    response.Warnings.Add(warning);
                }
                // Something went wrong
                else if (updatedResults != 2)
                {
                    response.Errors.Add(Errors.DefaultError.ToContractError(request.RequestId));
                    return response;
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "REQUEST:{0}", request?.ToJSON());
                response.Failed();
                response.Errors.Add(Errors.DefaultError.ToContractError(request.RequestId));
            }

            return response;
        }

        private bool IsValidRequest(UpdateReservationVCNRequest request, out List<Error> errors)
        {
            errors = new List<Error>();

            if (request.ReservationId <= 0)
                errors.Add(Errors.ReservationDoesNotExist.ToContractError());

            if (string.IsNullOrWhiteSpace(request.VcnReservationId))
                errors.Add(Errors.RequiredParameter.ToContractError(nameof(request.VcnReservationId)));
            else if (request.VcnReservationId.Length > 100)
                errors.Add(Errors.ParameterMaxLengthExceeded.ToContractError(nameof(request.VcnReservationId), "100"));

            if (string.IsNullOrWhiteSpace(request.VcnTokenId))
                errors.Add(Errors.RequiredParameter.ToContractError(nameof(request.VcnTokenId)));
            else if (request.VcnTokenId.Length > 1000)
                errors.Add(Errors.ParameterMaxLengthExceeded.ToContractError(nameof(request.VcnTokenId), "1000"));

            if (string.IsNullOrWhiteSpace(request.CreditCardHolderName))
                errors.Add(Errors.RequiredParameter.ToContractError(nameof(request.CreditCardHolderName)));

            if (string.IsNullOrWhiteSpace(request.CreditCardNumber))
                errors.Add(Errors.RequiredParameter.ToContractError(nameof(request.CreditCardNumber)));

            if (request.CreditCardExpirationDate == default(DateTime))
                errors.Add(Errors.RequiredParameter.ToContractError(nameof(request.CreditCardExpirationDate)));

            if (string.IsNullOrWhiteSpace(request.CreditCardCVV))
                errors.Add(Errors.RequiredParameter.ToContractError(nameof(request.CreditCardCVV)));

            return !errors.Any();
        }

        #endregion RESTful Operations

        /// <summary>
        ///  Fetch Reservation Detail by ReservationId
        /// </summary>
        public ReservationDetailQR1 GetFullReservationDetail(long ReservationId, long LanguageId, bool basecurrency)
        {
            System.Globalization.CultureInfo culture;

            ReservationDetailQR1 reservation = GetReservationDetail(ReservationId, LanguageId);

            List<ReservationRoomQR1> ReservationRooms;
            List<ReservationRoomDetailQR1> ReservationRoomDetail;
            try
            {
                var unitOfWork = this.SessionFactory.GetUnitOfWork(domainReservations.Reservation.DomainScope);

                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                var currencyRepo = this.RepositoryFactory.GetOBCurrencyRepository();

                ReservationRooms = GetReservationRoom(ReservationId, LanguageId, reservation.PropertyBaseCurrencyExchangeRate);

                ReservationRoomDetail = GetReservationRoomDetail(ReservationId, LanguageId);

                if (ReservationRooms == null)
                    reservation.ReservationRooms = new List<ReservationRoomQR1>();
                if (!ReservationRooms.Any())
                    reservation.ReservationRooms = new List<ReservationRoomQR1>();

                // *****************************  Load the currency
                if (basecurrency == false && reservation.ReservationCurrency_UID != null)
                {
                    culture = currencyRepo.GetCultureInfoByCurrencyUID(new Contracts.Requests.GetCultureInfoRequest { Currency_Id = reservation.ReservationCurrency_UID.Value });
                }
                else
                {
                    if (reservation.BaseCurrency_UID != null)
                        culture = currencyRepo.GetCultureInfoByCurrencyUID(new Contracts.Requests.GetCultureInfoRequest { Currency_Id = reservation.BaseCurrency_UID.Value });
                    else
                    {
                        domainReservations.Reservation r = reservationRepo.Get(ReservationId);
                        if (r != null && r.ReservationBaseCurrency_UID.HasValue)
                            culture = currencyRepo.GetCultureInfoByCurrencyUID(new Contracts.Requests.GetCultureInfoRequest { Currency_Id = r.ReservationBaseCurrency_UID.Value });
                        else
                            culture = Thread.CurrentThread.CurrentCulture;
                    }
                }

                if (reservation != null && ReservationRooms.Any())
                {
                    //ID for Each RateRoom
                    foreach (ReservationRoomQR1 ReservationRoom in ReservationRooms)
                    {
                        List<ReservationRoomDetailQR1> ReservationRoomDetails = ReservationRoomDetail.Where(r => r.ReservationRoom_UID == ReservationRoom.ReservationRoom_UID).ToList();

                        List<ReservationRoomExtraQR1> RoomExtras = GetReservationRoomExtraByRoomUID(ReservationRoom.ReservationRoom_UID, LanguageId);

                        List<ReservationRoomIncentiveQR1> RoomIncentives = GetReservationRoomIncentivesByRoomUID(ReservationRoom.ReservationRoom_UID, LanguageId);

                        if (RoomIncentives.Any())
                            ReservationRoom.ReservationRoomIncentives = RoomIncentives;

                        if (RoomExtras.Any())
                        {
                            if (!ReservationRoom.ReservationRoomExtrasPriceSum.HasValue)
                                ReservationRoom.ReservationRoomExtrasPriceSum = RoomExtras.Sum(e => e.ExtraPrice) ?? 0;
                            ReservationRoom.ReservationRoomExtras = RoomExtras;
                        }
                        //------------------------------ Check Empty Values an Do all The Calculation
                        if (!ReservationRoom.ReservationRoomExtrasPriceSum.HasValue)
                            ReservationRoom.ReservationRoomExtrasPriceSum = 0;

                        if (!ReservationRoom.ReservationRoomPriceSum.HasValue)
                            ReservationRoom.ReservationRoomPriceSum = ReservationRoomDetails.Sum(rr => rr.AdultPrice + rr.ChildPrice);

                        if (!ReservationRoom.ReservationRoomTotalAmount.HasValue)
                            ReservationRoom.ReservationRoomTotalAmount = ReservationRoom.ReservationRoomPriceSum + ReservationRoom.TotalTax + ReservationRoom.ReservationRoomExtrasPriceSum;

                        //------------------------- Attach the Reservation Room Details to the Room
                        ReservationRoom.ReservationRoomDetail = ReservationRoomDetails;
                    }

                    //---------------------------Check Empty Values an Do all The Calculation
                    if (!reservation.ReservationTotalTax.HasValue)
                    {
                        reservation.ReservationTotalTax = reservation.Tax;
                    }

                    if (!reservation.ReservationExtrasTotalPrice.HasValue)
                    {
                        reservation.ReservationExtrasTotalPrice = ReservationRooms.Sum(t => (t.ReservationRoomExtrasPriceSum));
                    }
                    if (!reservation.ReservationRoomsTotalPrice.HasValue)
                    {
                        reservation.ReservationRoomsTotalPrice = ReservationRooms.Sum(t => (t.ReservationRoomPriceSum)) + reservation.ReservationExtrasTotalPrice;
                    }

                    reservation.ReservationRooms = ReservationRooms;
                    if (basecurrency == false)
                    {
                        if (reservation.ReservationCurrencyExchangeRate.HasValue)
                        {
                            decimal convertionValue = 1;

                            if (reservation.ReservationCurrencyExchangeRate.HasValue)
                            {
                                convertionValue = reservation.ReservationCurrencyExchangeRate.Value;
                            }

                            reservation.ReservationExtrasTotalPrice *= convertionValue;
                            reservation.ReservationRoomsTotalPrice *= convertionValue;
                            reservation.ReservationRoomsTotalTax *= convertionValue;
                            reservation.ReservationTotalTax *= convertionValue;
                            reservation.Tax *= convertionValue;
                            reservation.TotalAmount *= convertionValue;
                            foreach (ReservationRoomQR1 ReservationRoom in reservation.ReservationRooms)
                            {
                                ReservationRoom.ReservationRoomExtrasPriceSum *= convertionValue;
                                ReservationRoom.ReservationRoomPriceSum *= convertionValue;
                                ReservationRoom.ReservationRoomTotalAmount *= convertionValue;
                                ReservationRoom.TotalAmount *= convertionValue;
                                ReservationRoom.TotalTax *= convertionValue;

                                foreach (ReservationRoomDetailQR1 rrDetail in ReservationRoom.ReservationRoomDetail)
                                {
                                    rrDetail.AdultPrice *= convertionValue;
                                    rrDetail.ChildPrice *= convertionValue;
                                    rrDetail.DailyTotalPrice *= convertionValue;
                                    rrDetail.RateValue *= convertionValue;
                                    rrDetail.TotalAmount *= convertionValue;
                                    rrDetail.TotalTax *= convertionValue;
                                }

                                if (ReservationRoom.ReservationRoomExtras != null)
                                {
                                    foreach (ReservationRoomExtraQR1 RoomExtra in ReservationRoom.ReservationRoomExtras)
                                    {
                                        RoomExtra.ExtraPrice *= convertionValue;
                                    }
                                }

                                //reservation room taxes
                                if (ReservationRoom.ReservationRoomTaxPolicies != null)
                                {
                                    foreach (ReservationRoomTaxPolicyQR1 tax in ReservationRoom.ReservationRoomTaxPolicies)
                                    {
                                        if (tax.TaxCalculatedValue.HasValue)
                                            tax.TaxCalculatedValue *= convertionValue;
                                    }
                                }
                            }
                        }
                    }

                    //*********** String Formatting

                    reservation.ReservationTotalTaxFormated = reservation.ReservationTotalTax.HasValue && reservation.ReservationTotalTax.Value > 0 ? reservation.ReservationTotalTax.Value.ToString("n", culture) : "-";
                    reservation.ReservationRoomsTotalTaxFormated = reservation.ReservationRoomsTotalTax.HasValue && reservation.ReservationRoomsTotalTax.Value > 0 ? reservation.ReservationRoomsTotalTax.Value.ToString("n", culture) : "-";
                    reservation.ReservationExtrasTotalPriceFormated = reservation.ReservationExtrasTotalPrice.HasValue && reservation.ReservationExtrasTotalPrice.Value > 0 ? reservation.ReservationExtrasTotalPrice.Value.ToString("n", culture) : "-";
                    reservation.ReservationRoomsTotalPriceFormated = reservation.ReservationRoomsTotalPrice.HasValue && reservation.ReservationRoomsTotalPrice.Value > 0 ? reservation.ReservationRoomsTotalPrice.Value.ToString("n", culture) : "-";
                    reservation.TotalAmountFormated = reservation.TotalAmount.HasValue && reservation.TotalAmount.Value > 0 ? reservation.TotalAmount.Value.ToString("n", culture) : "-";
                    reservation.BaseTotalAmountFormated = reservation.BaseTotalAmount.HasValue && reservation.BaseTotalAmount.Value > 0 ? reservation.BaseTotalAmount.Value.ToString("n", culture) : "-";

                    foreach (ReservationRoomQR1 ReservationRoom in reservation.ReservationRooms)
                    {
                        ReservationRoom.TotalTaxFormated = ReservationRoom.TotalTax.HasValue && ReservationRoom.TotalTax.Value > 0 ? ReservationRoom.TotalTax.Value.ToString("n", culture) : "-";
                        ReservationRoom.TotalAmountFormated = ReservationRoom.TotalAmount.HasValue && ReservationRoom.TotalAmount.Value > 0 ? ReservationRoom.TotalAmount.Value.ToString("n", culture) : "-";
                        ReservationRoom.ReservationRoomPriceSumFormated = ReservationRoom.ReservationRoomPriceSum.HasValue && ReservationRoom.ReservationRoomPriceSum.Value > 0 ? ReservationRoom.ReservationRoomPriceSum.Value.ToString("n", culture) : "-";
                        ReservationRoom.ReservationRoomExtrasPriceSumFormated = ReservationRoom.ReservationRoomExtrasPriceSum.HasValue && ReservationRoom.ReservationRoomExtrasPriceSum.Value > 0 ? ReservationRoom.ReservationRoomExtrasPriceSum.Value.ToString("n", culture) : "-";
                        ReservationRoom.ReservationRoomTotalAmountFormated = ReservationRoom.ReservationRoomTotalAmount.HasValue && ReservationRoom.ReservationRoomTotalAmount.Value > 0 ? ReservationRoom.ReservationRoomTotalAmount.Value.ToString("n", culture) : "-";

                        foreach (ReservationRoomDetailQR1 rrDetail in ReservationRoom.ReservationRoomDetail)
                        {
                            rrDetail.RateValueFormated = rrDetail.DailyTotalPrice.HasValue && rrDetail.DailyTotalPrice.Value > 0 ? rrDetail.DailyTotalPrice.Value.ToString("n", culture) : "-";

                            rrDetail.DailyTotalPriceFormated = rrDetail.DailyTotalPrice.HasValue && rrDetail.DailyTotalPrice.Value > 0 ? rrDetail.DailyTotalPrice.Value.ToString("n", culture) : "-";
                        }

                        if (ReservationRoom.ReservationRoomExtras != null)
                        {
                            foreach (ReservationRoomExtraQR1 RoomExtra in ReservationRoom.ReservationRoomExtras)
                            {
                                RoomExtra.ExtraPriceFormated = RoomExtra.ExtraPrice.HasValue && RoomExtra.ExtraPrice.Value > 0 ? RoomExtra.ExtraPrice.Value.ToString("n", culture) : "-";
                            }
                        }

                        //reservation room taxes
                        if (ReservationRoom.ReservationRoomTaxPolicies != null)
                        {
                            foreach (ReservationRoomTaxPolicyQR1 tax in ReservationRoom.ReservationRoomTaxPolicies)
                                tax.TaxCalculatedValueFormated = tax.TaxCalculatedValue.HasValue && tax.TaxCalculatedValue.Value > 0 ? tax.TaxCalculatedValue.Value.ToString("n", culture) : "-";
                        }
                    }
                }

                // bug 6346
                reservation.ReservationRoomsTotalPriceFormated = !string.IsNullOrEmpty(reservation.ReservationRoomsTotalPriceFormated) ? reservation.ReservationRoomsTotalPriceFormated : 0.ToString("n", culture);
                reservation.ReservationTotalTaxFormated = !string.IsNullOrEmpty(reservation.ReservationTotalTaxFormated) ? reservation.ReservationTotalTaxFormated : 0.ToString("n", culture);
                reservation.TotalAmountFormated = !string.IsNullOrEmpty(reservation.TotalAmountFormated) ? reservation.TotalAmountFormated : 0.ToString("n", culture);
                reservation.BaseTotalAmountFormated = !string.IsNullOrEmpty(reservation.BaseTotalAmountFormated) ? reservation.BaseTotalAmountFormated : 0.ToString("n", culture);

                //task 3326
                if (reservation.TotalAdults.HasValue && reservation.TotalAdults.Value == 0)
                    reservation.TotalAdults = null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "@GetFullReservationDetail");
                var arguments = new Dictionary<string, object>();
                arguments.Add("ReservationId", ReservationId);
                arguments.Add("LanguageId", LanguageId);
                arguments.Add("basecurrency", basecurrency);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments);
            }
            return reservation;
        }

        /// <summary>
        ///  Fetch Reservation Detail by ReservationId
        /// </summary>
        public ReservationDetailQR1 GetReservationDetail(long ReservationId, long languageId)
        {
            var unitOfWork = this.SessionFactory.GetUnitOfWork();

            var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
            var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

            OB.BL.Contracts.Data.General.Language langObject = propertyRepo.ListLanguages(new Contracts.Requests.ListLanguageRequest { UIDs = new List<long> { languageId } }).FirstOrDefault();
            string languageIso = string.Empty;
            if (langObject != null)
            {
                if (langObject.Code != string.Empty && langObject.Code.Contains('-'))
                    languageIso = langObject.Code.Split('-')[0].ToLower();
            }

            #region new get main details

            ReservationDetailSearchQR1 spResult = reservationRepo.GetReservationDetail(ReservationId, languageId, languageIso);
            var query = spResult.MainSearch;

            if (query == null)
                return null;

            #endregion new get main details

            #region policies

            // policies
            query.HotelPolicies = spResult.Policies;

            // guest activities
            if ((query.GuestActivities = spResult.GuestActivities) == null)
                query.GuestActivities = new List<GuestActivityQR1>();


            #endregion policies

            #region state translations

            if (query.StateUID.HasValue)
                query.StateName = !string.IsNullOrEmpty(spResult.GuestStateName) ? spResult.GuestStateName : query.StateName;

            if (query.BillingStateUID.HasValue)
                query.BillingStateName = !string.IsNullOrEmpty(spResult.GuestStateName) ? spResult.GuestStateName : query.StateName;


            #endregion state translations

            #region format partial payment

            if (query.IsPartialPayment.HasValue && query.IsPartialPayment.Value && query.NoOfInstallment.HasValue && query.InstallmentAmount.HasValue)
            {
                query.PartialPaymentFormatted = query.NoOfInstallment.Value + "X " + Math.Round(query.InstallmentAmount.Value, 2) + query.BaseCurrentySymbol;
                query.PartialPaymentFormatted += query.InterestRate.HasValue ? " (" + Math.Round(query.InterestRate.Value, 2) + "%)" : String.Empty;
            }

            #endregion format partial payment

            return query;
        }

        /// <summary>
        ///  Fetch Reservation Room Details
        /// </summary>
        /// <param name="ReservationUID"></param>
        public List<ReservationRoomDetailQR1> GetReservationRoomDetail(long ReservationUID, long languageUID)
        {
            var unitOfWork = this.SessionFactory.GetUnitOfWork();

            // check language id and convert to proper culture
            // save current culture
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (languageUID > 0)
            {
                var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();
                var lang = propertyRepo.ListLanguages(new Contracts.Requests.ListLanguageRequest { UIDs = new List<long> { languageUID } }).FirstOrDefault();
                if (lang != null)
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang.Code);
            }

            var reservationRoomDetailRepo = this.RepositoryFactory.GetReservationRoomDetailRepository(unitOfWork);
            var result = reservationRoomDetailRepo.GetReservationRoomDetailsQR1(ReservationUID, languageUID, (int)Constants.ReservationStatus.Cancelled);

            var reservationRoomChildRepo = this.RepositoryFactory.GetRepository<ReservationRoomChild>(unitOfWork);
            foreach (ReservationRoomDetailQR1 reservation in result)
            {
                reservation.ChildAges = reservationRoomChildRepo.GetQuery(c => c.ReservationRoom_UID == reservation.ReservationRoom_UID)
                                            .Select(c => new { c.Age }).DefaultIfEmpty().Select(c => c.Age).ToList();
            }

            // replace current culture
            Thread.CurrentThread.CurrentCulture = currentCulture;

            return result;
        }

        /// <summary>
        ///  Fetch Reservation Room Details
        /// </summary>
        /// <param name="ReservationUID"></param>
        public List<ReservationRoomQR1> GetReservationRoom(long ReservationUID, long languageUID, decimal PropertyBaseCurrencyExchangeRate)
        {
            var unitOfWork = this.SessionFactory.GetUnitOfWork();

            // check language id and convert to proper culture
            // save current culture
            if (languageUID > 0)
            {
                var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

                var lang = propertyRepo.ListLanguages(new Contracts.Requests.ListLanguageRequest { UIDs = new List<long> { languageUID } }).FirstOrDefault();
                if (lang != null)
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang.Code);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(lang.Code);
                }
            }

            var reservationRoomRepo = this.RepositoryFactory.GetReservationRoomRepository(unitOfWork);
            var result = reservationRoomRepo.GetReservationRoomsQR1(ReservationUID, languageUID);

            var reservationRoomChildRepo = this.RepositoryFactory.GetRepository<ReservationRoomChild>(unitOfWork);
            var reservationRoomTaxPolicyRepo = this.RepositoryFactory.GetRepository<ReservationRoomTaxPolicy>(unitOfWork);

            foreach (ReservationRoomQR1 room in result)
            {
                room.NumberDays = room.CheckOut.HasValue && room.CheckIn.HasValue ? (room.CheckOut.Value - room.CheckIn.Value).Days : 0;
                room.ChildAges = reservationRoomChildRepo.GetQuery(c => c.ReservationRoom_UID == room.ReservationRoom_UID).Select(c => c.Age).ToList();
                room.ArrivalTimeFormatted = room.ArrivalTime.HasValue ? string.Format("{0:hh\\:mm}", room.ArrivalTime.Value) : String.Empty;

                #region add taxes

                room.ReservationRoomTaxPolicies = reservationRoomTaxPolicyRepo.GetQuery(x => x.ReservationRoom_UID == room.ReservationRoom_UID && !string.IsNullOrEmpty(x.TaxName) && x.TaxCalculatedValue.HasValue)
                                                    .Select(tp => new ReservationRoomTaxPolicyQR1
                                                    {
                                                        ReservationRoom_UID = tp.ReservationRoom_UID,
                                                        TaxCalculatedValue = tp.TaxCalculatedValue / PropertyBaseCurrencyExchangeRate,
                                                        TaxDefaultValue = tp.TaxDefaultValue / PropertyBaseCurrencyExchangeRate,
                                                        TaxDescription = tp.TaxDescription,
                                                        TaxId = tp.TaxId,
                                                        TaxIsPercentage = tp.TaxIsPercentage,
                                                        TaxName = tp.TaxName,
                                                        UID = tp.UID,
                                                        BillingType = tp.BillingType
                                                    }).ToList();

                #endregion add taxes

                // replace status
                room.StatusFormatted = string.Empty;
                if (room.Status.HasValue)
                {
                    if (room.Status.Value == (int)Constants.ReservationStatus.Cancelled)
                        room.StatusFormatted = Resources.Resources.lblCancelled;
                    if (room.Status.Value == (int)Constants.ReservationStatus.Modified)
                        room.StatusFormatted = Resources.Resources.lblModified;
                }
            }

            return result;
        }

        /// <summary>
        /// Fetch Reservation Room Extra
        /// </summary>
        /// <param name="ReservationRoomUID"></param>
        /// <param name="Language_UID"></param>
        /// <returns></returns>
        public List<ReservationRoomExtraQR1> GetReservationRoomExtraByRoomUID(long ReservationRoomUID, long Language_UID)
        {
            var unitOfWork = this.SessionFactory.GetUnitOfWork();
            var reservationRoomRepo = this.RepositoryFactory.GetRepository<ReservationRoom>(unitOfWork);
            var reservationRoomExtraRepo = this.RepositoryFactory.GetRepository<ReservationRoomExtra>(unitOfWork);
            var extraRepo = this.RepositoryFactory.GetOBExtrasRepository();
            var reservationRoomExtraAvailableDateRepo = this.RepositoryFactory.GetRepository<ReservationRoomExtrasAvailableDate>(unitOfWork);
            List<ReservationRoomExtraQR1> result = new List<ReservationRoomExtraQR1>();

            var reservationRooms = reservationRoomRepo.GetQuery(x => x.UID == ReservationRoomUID && x.Reservation != null).Include(x => x.Reservation);

            if (reservationRooms == null)
                return result;

            ReservationRoom roominfo = reservationRooms.FirstOrDefault();
            var reservation = roominfo.Reservation;
            var reservationRoomExtras = reservationRoomExtraRepo.GetQuery(x => x.ReservationRoom_UID == ReservationRoomUID).ToList();
            var reservationRoomExtraUIDs = reservationRoomExtras.Select(x => x.UID).ToList();
            var extraUIDs = reservationRoomExtras.Select(x => x.Extra_UID).ToList();

            var extras = extraRepo.ListExtras(new Contracts.Requests.ListExtraRequest { UIDs = extraUIDs });
            var extraInfo = extras.Select(e => new
            {
                UID = e.UID,
                Name = e.Name,
                VAT = e.VAT
            }).ToList();

            var extraLanguageInfo = extras.SelectMany(x => x.ExtrasLanguages.Select(el => new
            {
                Extra_UID = x.UID,
                Name = el.Name,
            })).ToList();

            foreach (var reservationRE in reservationRoomExtras)
            {
                var extra = extraInfo.FirstOrDefault(x => x.UID == reservationRE.Extra_UID);
                var extraLanguage = extraLanguageInfo.FirstOrDefault(x => x.Extra_UID == extra.UID);

                result.Add(new ReservationRoomExtraQR1
                {
                    //ReservationRoomExtraUID = re.UID,
                    UID = reservationRE.ReservationRoom_UID,
                    ReservationRoom_UID = reservationRE.ReservationRoom_UID,
                    ExtraID = reservationRE.Extra_UID,
                    ExtraCount = reservationRE.Qty,
                    ExtraPrice = reservationRE.Total_Price / roominfo.Reservation.PropertyBaseCurrencyExchangeRate,
                    ExtraName = extraLanguage != null && !string.IsNullOrEmpty(extraLanguage.Name) ? extraLanguage.Name : extra.Name,
                    VAT = extra.VAT,
                    TotalVat = (decimal)(reservationRE.Total_VAT / reservation.PropertyBaseCurrencyExchangeRate),
                    ExtraIncluded = reservationRE.ExtraIncluded,
                    ExtraAddedOrIncuded = reservationRE.ExtraIncluded ? "(" + Resources.Resources.lblIncluded + ")" : "(" + Resources.Resources.lblAdded + ")"
                });
            }

            if (result.Count > 0 && roominfo != null)
            {
                foreach (ReservationRoomExtraQR1 extra in result)
                {
                    if (extra.ExtraIncluded && roominfo.DateFrom.HasValue && roominfo.DateTo.HasValue)
                    {
                        #region For Booking Engine

                        List<ReservationRoomExtrasAvailableDate> lstExtraAvailableDatesForBE = (from rres in reservationRoomExtraAvailableDateRepo.GetQuery(x => reservationRoomExtraUIDs.Contains(x.ReservationRoomExtra_UID))
                                                                                                where
                                                                                                    rres.ReservationRoomExtra.Extra_UID == extra.ExtraID
                                                                                                && !(rres.DateTo < roominfo.DateFrom.Value || rres.DateFrom > roominfo.DateTo.Value)
                                                                                                select rres).ToList();

                        List<ReservationRoomExtrasAvailableDate> lstDistAvDates = (from c in lstExtraAvailableDatesForBE
                                                                                   group c by new
                                                                                   {
                                                                                       c.DateFrom,
                                                                                       c.DateTo
                                                                                   } into gcs
                                                                                   select new ReservationRoomExtrasAvailableDate()
                                                                                   {
                                                                                       DateFrom = gcs.Key.DateFrom,
                                                                                       DateTo = gcs.Key.DateTo,
                                                                                   }).ToList();

                        if (lstExtraAvailableDatesForBE != null && lstExtraAvailableDatesForBE.Count > 0)
                        {
                            DateTime dtAllStayFrom = roominfo.DateFrom.Value;
                            DateTime dtAllStayTo = roominfo.DateTo.Value;
                            int Difference = (dtAllStayTo.Date - dtAllStayFrom.Date).Days;

                            int counter = 0;

                            while (dtAllStayFrom <= dtAllStayTo)
                            {
                                if (lstExtraAvailableDatesForBE.Any(t => (t.DateFrom <= dtAllStayFrom && t.DateTo >= dtAllStayFrom)))
                                {
                                    counter++;
                                }
                                dtAllStayFrom = dtAllStayFrom.AddDays(1);
                            }

                            if (counter == Difference + 1)
                            {
                                extra.ExtraAvailableDatesInfo = string.Empty;
                            }
                            else
                            {
                                var datesStr = new List<string>();
                                foreach (ReservationRoomExtrasAvailableDate avDate in lstDistAvDates)
                                {
                                    DateTime dtAvailFrom = (avDate.DateFrom < roominfo.DateFrom.Value ? roominfo.DateFrom.Value : avDate.DateFrom);
                                    DateTime dtAvailTo = (avDate.DateTo > roominfo.DateTo.Value ? roominfo.DateTo.Value : avDate.DateTo);

                                    datesStr.Add(string.Format("{0}: {1:MM/dd/yyyy} {2}: {3:MM/dd/yyyy}", Resources.Resources.lblFrom, dtAvailFrom, Resources.Resources.lblTo, dtAvailTo));
                                }
                                extra.ExtraAvailableDatesInfo = $"({string.Join(" | ", datesStr)})";
                            }
                        }
                        else
                        {
                            extra.ExtraAvailableDatesInfo = string.Empty;
                        }

                        #endregion For Booking Engine
                    }

                    var reservationRoomExtraScheduleRepo = this.RepositoryFactory.GetRepository<ReservationRoomExtrasSchedule>(unitOfWork);

                    //Get Extra Schedule Info.
                    List<DateTime> lstExtraScheduleData = reservationRoomExtraScheduleRepo.GetQuery(x => reservationRoomExtraUIDs.Contains(x.ReservationRoomExtra_UID)
                                                            && x.ReservationRoomExtra.Extra_UID == extra.ExtraID)
                                                            .Select(x => x.Date).ToList();

                    int extraScheduleCount = 0;
                    if (lstExtraScheduleData != null && lstExtraScheduleData.Count > 0)
                    {
                        extra.ExtraScheduleInfo = $"({ string.Join(" | ", lstExtraScheduleData.OrderBy(dt => dt.Date)) })";
                        extraScheduleCount = lstExtraScheduleData.Count;
                    }
                    else
                    {
                        extra.ExtraScheduleInfo = string.Empty;
                    }

                    extra.IsExtraScheduleVisible = extraScheduleCount > 0;
                    extra.ExtraNameWithCount = extra.ExtraCount > 1 ? "(" + extra.ExtraCount + "x) " + extra.ExtraName : extra.ExtraName;
                }
            }

            return result;
        }


        /// <summary>
        /// Fetch Reservation Room Extra
        /// </summary>
        /// <param name="ReservationRoomUID"></param>
        /// <param name="Language_UID"></param>
        /// <returns></returns>
        public List<ReservationRoomIncentiveQR1> GetReservationRoomIncentivesByRoomUID(long ReservationRoomUID, long Language_UID)
        {
            var unitOfWork = this.SessionFactory.GetUnitOfWork(ReservationRoomDetailsAppliedIncentive.DomainScope, ReservationRoom.DomainScope, ReservationRoomDetail.DomainScope);

            var reservationRoomRepo = this.RepositoryFactory.GetReservationRoomRepository(unitOfWork);
            var incentiveRepo = this.RepositoryFactory.GetOBIncentiveRepository();
            var reservationRoomDetailAppliedIncentiveRepo = this.RepositoryFactory.GetRepository<ReservationRoomDetailsAppliedIncentive>(unitOfWork);

            ReservationRoom roominfo = reservationRoomRepo.Get(ReservationRoomUID);

            var rRoomDetailAppliedIncentives = reservationRoomDetailAppliedIncentiveRepo.GetQuery(x => x.ReservationRoomDetail.ReservationRoom_UID == ReservationRoomUID).ToList();
            var incentiveUIDs = rRoomDetailAppliedIncentives.Select(x => x.Incentive_UID).Distinct().ToList();
            var incentives = incentiveRepo.ListIncentives(new Contracts.Requests.ListIncentiveRequest { IncentiveUIDs = incentiveUIDs });

            List<ReservationRoomIncentiveQR1> result = (from incentive in incentives
                                                        join resAppliedIncentive in rRoomDetailAppliedIncentives on incentive.UID equals resAppliedIncentive.Incentive_UID
                                                        select new ReservationRoomIncentiveQR1
                                                        {
                                                            Days = resAppliedIncentive.Days != null ? (int)resAppliedIncentive.Days : 0,
                                                            DiscountPercentage = resAppliedIncentive.DiscountPercentage != null ? (int)resAppliedIncentive.DiscountPercentage : 0,
                                                            FreeDays = resAppliedIncentive.FreeDays,
                                                            IncentiveType = "(" + incentive.IncentiveType + ")",
                                                            IncentiveType_UID = incentive.IncentiveType_UID,
                                                            IsDeleted = incentive.IsDeleted,
                                                            IsFreeDaysAtBegin = resAppliedIncentive.IsFreeDaysAtBegin,
                                                            Name = resAppliedIncentive.Name,
                                                            Property_UID = incentive.Property_UID,
                                                            Rate_UID = roominfo.Rate_UID != null ? (int)roominfo.Rate_UID : 0,
                                                            UID = incentive.UID
                                                        }).Distinct().ToList();

            return result;
        }

        public ListLostReservationsResponse ListLostReservations(ListLostReservationsRequest request)
        {
            var response = new ListLostReservationsResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    // get Repository and Criteria

                    var repository = RepositoryFactory.GetLostReservationsRepository(unitOfWork);

                    var criteria = new OB.DL.Common.Criteria.ListLostReservationCriteria
                    {
                        PropertyUids = request.PropertyUids,
                        Uids = request.Uids
                    };

                    // Apply Order in Criteria
                    if (!(request.Orders != null && request.Orders.Any()))
                    {
                        criteria.OrderByDescending = true;
                    }
                    else
                    {
                        criteria.Order = request.Orders.Select(x => OtherConverter.Convert(x)).ToList();
                    }

                    // Apply Filters in Criteria
                    if (request.NestedFilters != null)
                    {
                        criteria.NestedFilters = OtherConverter.Convert(request.NestedFilters);
                    }
                    else if (request.Filters != null && request.Filters.Any())
                    {
                        criteria.Filters = request.Filters.Select(x => OtherConverter.Convert(x)).ToList();
                    }

                    var lostReservations = repository.FindByCriteria(criteria).ToList();

                    // Get the Total Records with Filters
                    response.TotalRecords = lostReservations.Count;

                    // Get the Results Paginated
                    var resultsPaginated = lostReservations.AsQueryable().Paginate(request.PageSize, request.PageIndex).AsEnumerable().Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();

                    if (request.IncludeDetails)
                    {
                        var totalRecords = 0;
                        var couchRepository = RepositoryFactory.GetLostReservationDetailRepository(unitOfWork);
                        var couchBaseDetails = couchRepository.FindByUids(out totalRecords, resultsPaginated.Select(x => x.CouchBaseId).Where(p => !p.IsNullOrEmpty()).ToList()).Where(q => q.Value != null).ToList();

                        resultsPaginated = resultsPaginated.Where(x => x.CouchBaseId != null).ToList();

                        var resultDictionary = resultsPaginated.ToDictionary(x => x.CouchBaseId);

                        foreach (var result_CouchBaseValue in couchBaseDetails)
                        {
                            var result = resultDictionary[result_CouchBaseValue.Value.DocumentId];
                            result.Detail = DomainToBusinessObjectTypeConverter.Convert(result_CouchBaseValue.Value);
                        }
                    }

                    // Give the Result to the Client
                    response.Result = resultsPaginated;
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Errors.Add(new Error(ex));
                response.Failed();
            }

            return response;
        }

        /// <summary>
        /// Generate Password
        /// </summary>
        /// <returns></returns>
        protected string GeneratePassword()
        {
            string password;
            Random number = new Random();
            password = number.Next(000000, 999999).ToString("000000");
            return password;
        }


        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 1 April 2011
        /// Desc :: Insert Guest Detail in Guest table
        /// </summary>
        /// <param name="objguest"></param>
        public OB.BL.Contracts.Data.CRM.Guest InsertGuest(contractsCRMOB.Guest objguest, List<long> guestActivityUIDS, contractsReservations.Reservation objRes, List<contractsReservations.ReservationRoomExtra> objResRoomExtra)
        {
            var crmRepo = this.RepositoryFactory.GetOBCRMRepository();

            if (guestActivityUIDS != null && guestActivityUIDS.Count > 0)
                InsertGuestActivity(objguest.UID, guestActivityUIDS, objguest);
            if (objResRoomExtra != null && objResRoomExtra.Count > 0)
                InsertGuestFavoriteExtras(objResRoomExtra, objguest.UID, objguest);

            InsertGuestFavoriteSpecialrequest(objRes, objguest.UID, objguest);

            if (objRes.Channel_UID == 1)
                objguest.IsFromBe = true;
            var response = crmRepo.InsertGuestReservation(new Contracts.Requests.InsertGuestReservationRequest { Guest = objguest, UserUID = 65 });
            if (response.Status != OB.BL.Contracts.Responses.Status.Success || response.Errors.Any())
                throw new Exception(response.Errors.FirstOrDefault().Description);

            objguest.CreateDate = response.Result.CreateDate;
            objguest.UID = response.Result.UID;

            return objguest;
        }

        /// <summary>
        /// Update Guest Detail
        /// </summary>
        /// <param name="objguest"></param>
        private void UpdateGuest(contractsCRMOB.Guest objguest, List<long> guestActivityUIDS, contractsReservations.Reservation objRes, List<contractsReservations.ReservationRoomExtra> objResRoomExtra)
        {
            var crmRepo = this.RepositoryFactory.GetOBCRMRepository();

            contractsCRMOB.Guest guestdetail = crmRepo.ListGuestsByLightCriteria(new Contracts.Requests.ListGuestLightRequest { UIDs = new List<long> { objguest.UID }, IncludeGuestActivities = true, IncludeGuestFavoriteExtras = true, IncludeGuestFavoriteSpecialRequests = true }).FirstOrDefault();

            if (guestActivityUIDS != null && guestActivityUIDS.Count > 0)
                InsertGuestActivity(guestdetail.UID, guestActivityUIDS, guestdetail);
            if (objResRoomExtra != null && objResRoomExtra.Count > 0)
                InsertGuestFavoriteExtras(objResRoomExtra, guestdetail.UID, guestdetail);

            InsertGuestFavoriteSpecialrequest(objRes, guestdetail.UID, guestdetail);

            guestdetail.FirstName = objguest.FirstName;
            guestdetail.LastName = objguest.LastName;
            guestdetail.Email = objguest.Email;
            if (!String.IsNullOrEmpty(objguest.Address1))
            {
                guestdetail.Address1 = objguest.Address1.Trim();
            }
            if (!String.IsNullOrEmpty(objguest.Address2))
            {
                guestdetail.Address2 = objguest.Address2.Trim();
            }
            if (!String.IsNullOrEmpty(objguest.PostalCode))
            {
                guestdetail.PostalCode = objguest.PostalCode.Trim();
            }
            if (!String.IsNullOrEmpty(objguest.City))
            {
                guestdetail.City = objguest.City.Trim();
            }
            guestdetail.Country_UID = objguest.Country_UID;
            guestdetail.Phone = objguest.Phone;
            guestdetail.FacebookUser = objguest.FacebookUser;
            guestdetail.TwitterUser = objguest.TwitterUser;
            if (objguest.Birthday != null)
            {
                guestdetail.Birthday = objguest.Birthday;
            }
            guestdetail.Prefix = objguest.Prefix;

            //nsantos US 3125
            guestdetail.IDCardNumber = objguest.IDCardNumber;

            guestdetail.State_UID = objguest.State_UID;
            if (!string.IsNullOrEmpty(objguest.State))
                guestdetail.State = objguest.State.Trim();

            // us 3833
            guestdetail.BillingAddress1 = objguest.BillingAddress1;
            guestdetail.BillingAddress2 = objguest.BillingAddress2;
            guestdetail.BillingContactName = objguest.BillingContactName;
            guestdetail.BillingPostalCode = objguest.BillingPostalCode;
            guestdetail.BillingCity = objguest.BillingCity;
            guestdetail.BillingCountry_UID = objguest.BillingCountry_UID;
            guestdetail.BillingPhone = objguest.BillingPhone;
            guestdetail.UseDifferentBillingInfo = objguest.UseDifferentBillingInfo;
            guestdetail.BillingState_UID = objguest.BillingState_UID;
            guestdetail.BillingEmail = objguest.BillingEmail;
            guestdetail.BillingTaxCardNumber = objguest.BillingTaxCardNumber;
            if (!objguest.CreatedDate.HasValue)
                objguest.CreatedDate = guestdetail.CreateDate;

            if (!string.IsNullOrEmpty(objguest.BillingState))
                guestdetail.BillingState = objguest.BillingState.Trim();

            guestdetail.ModifyDate = System.DateTime.UtcNow;

            var response = crmRepo.UpdateGuestReservation(new Contracts.Requests.UpdateGuestReservationRequest { Guest = guestdetail, UserUID = 65 });
            if (response.Status != OB.BL.Contracts.Responses.Status.Success || response.Errors.Any())
                throw new Exception(response.Errors.FirstOrDefault().Description);
        }

        /// <summary>
        ///  Insert Data into GuestFavoriteSpecialrequest
        /// </summary>
        /// <param name="objReservation"></param>
        /// <param name="UID"></param>
        private void InsertGuestFavoriteSpecialrequest(contractsReservations.Reservation objReservation, long UID, contractsCRMOB.Guest guestDetails)
        {
            if (objReservation == null || (objReservation.BESpecialRequests1_UID.GetValueOrDefault() <= 0
                && objReservation.BESpecialRequests2_UID.GetValueOrDefault() <= 0
                && objReservation.BESpecialRequests3_UID.GetValueOrDefault() <= 0
                && objReservation.BESpecialRequests4_UID.GetValueOrDefault() <= 0))
                return;

            contractsCRMOB.GuestFavoriteSpecialRequest obj;
            var result = Enumerable.Empty<long>();

            if (guestDetails.GuestFavoriteSpecialRequests == null)
                guestDetails.GuestFavoriteSpecialRequests = new List<Contracts.Data.CRM.GuestFavoriteSpecialRequest>();

            if (UID > 0 && guestDetails.GuestFavoriteSpecialRequests.Any())
                result = guestDetails.GuestFavoriteSpecialRequests.Select(x => x.BESpecialRequests_UID).ToList();

            if (objReservation.BESpecialRequests1_UID > 0 && !result.Contains((int)objReservation.BESpecialRequests1_UID))
            {
                obj = new contractsCRMOB.GuestFavoriteSpecialRequest();
                obj.BESpecialRequests_UID = (long)objReservation.BESpecialRequests1_UID;
                obj.Guest_UID = UID;

                guestDetails.GuestFavoriteSpecialRequests.Add(obj);
            }
            if (objReservation.BESpecialRequests2_UID > 0 && !result.Contains((int)objReservation.BESpecialRequests2_UID))
            {
                obj = new contractsCRMOB.GuestFavoriteSpecialRequest();
                obj.BESpecialRequests_UID = (long)objReservation.BESpecialRequests2_UID;
                obj.Guest_UID = UID;

                guestDetails.GuestFavoriteSpecialRequests.Add(obj);
            }
            if (objReservation.BESpecialRequests3_UID > 0 && !result.Contains((int)objReservation.BESpecialRequests3_UID))
            {
                obj = new contractsCRMOB.GuestFavoriteSpecialRequest();
                obj.BESpecialRequests_UID = (long)objReservation.BESpecialRequests3_UID;
                obj.Guest_UID = UID;

                guestDetails.GuestFavoriteSpecialRequests.Add(obj);
            }
            if (objReservation.BESpecialRequests4_UID > 0 && !result.Contains((int)objReservation.BESpecialRequests4_UID))
            {
                obj = new contractsCRMOB.GuestFavoriteSpecialRequest();
                obj.BESpecialRequests_UID = (long)objReservation.BESpecialRequests4_UID;
                obj.Guest_UID = UID;

                guestDetails.GuestFavoriteSpecialRequests.Add(obj);
            }
        }

        /// <summary>
        /// Insert Data into GuestFavoriteExtras table
        /// </summary>
        /// <param name="objReservationExtra"></param>
        /// <param name="UID"></param>
        private void InsertGuestFavoriteExtras(List<contractsReservations.ReservationRoomExtra> objReservationExtra, long UID, contractsCRMOB.Guest guestDetails)
        {
            if (objReservationExtra == null || objReservationExtra.Count == 0)
                return;


            if (guestDetails.GuestFavoriteExtras == null)
                guestDetails.GuestFavoriteExtras = new List<Contracts.Data.CRM.GuestFavoriteExtra>();

            contractsCRMOB.GuestFavoriteExtra obj;
            var result = Enumerable.Empty<long>();
            if (UID > 0)
                result = guestDetails.GuestFavoriteExtras.Select(ge => ge.Extras_UID).ToList();

            var addedExtras = new List<contractsCRMOB.GuestFavoriteExtra>();

            foreach (contractsReservations.ReservationRoomExtra ext in objReservationExtra)
            {
                if (!result.Contains(ext.Extra_UID))
                {
                    obj = new contractsCRMOB.GuestFavoriteExtra();
                    obj.Extras_UID = ext.Extra_UID;
                    obj.Guest_UID = UID;

                    addedExtras.Add(obj);
                }
            }
            guestDetails.GuestFavoriteExtras.AddRange(addedExtras);
        }

        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 4 April 2011
        /// Desc: Guest Activity Insert into GuestActivities table
        /// </summary>
        /// <returns></returns>
        public int InsertGuestActivity(long GuestID, List<long> objGuestActivity, contractsCRMOB.Guest guestDetails)
        {
            var unitOfWork = this.SessionFactory.GetUnitOfWork();
            var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

            //Clear old Data
            reservationRepo.DeleteAllActivitiesForGuestUID(GuestID);

            List<contractsCRMOB.GuestActivity> guestActivities = new List<Contracts.Data.CRM.GuestActivity>();
            foreach (long obj in objGuestActivity)
            {
                contractsCRMOB.GuestActivity objguestactivity = new contractsCRMOB.GuestActivity();
                objguestactivity.Guest_UID = GuestID;
                objguestactivity.Activity_UID = obj;
                guestActivities.Add(objguestactivity);
            }

            guestDetails.GuestActivities = new List<Contracts.Data.CRM.GuestActivity>();
            guestDetails.GuestActivities.AddRange(guestActivities);

            return 1;
        }

        [Obsolete("Use UpdateReservationAllotmentAndInventory instead. This method will be removed on OB version 0.9.48", true)]
        private IEnumerable<OB.BL.Contracts.Data.Properties.Inventory> DecrementInventoryDetails(IEnumerable<Tuple<long, DateTime, DateTime>> dateRangeByRoomTypeId)
        {
            var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

            var result = new List<OB.BL.Contracts.Data.Properties.Inventory>();

            var inventorySearchList = dateRangeByRoomTypeId.Select(x => new contractsProperties.InventorySearch { RoomType_UID = x.Item1, DateFrom = x.Item2, DateTo = x.Item3 }).ToList();
            var inventoriesToUpdate = propertyRepo.ListInventory(new Contracts.Requests.ListInventoryRequest
            {
                roomTypeIdsAndDateRange = inventorySearchList,
                Track = true
            });

            result.AddRange(inventoriesToUpdate);

            var entitiesToAdd = new List<OB.BL.Contracts.Data.Properties.Inventory>();

            foreach (var tuple in dateRangeByRoomTypeId)
            {
                var roomTypeId = tuple.Item1;
                var dateFrom = tuple.Item2;
                var dateTo = tuple.Item3;
                while (dateFrom <= dateTo)
                {
                    var inventory = inventoriesToUpdate.FirstOrDefault(x => x.RoomType_UID == tuple.Item1 && x.Date.Date.Equals(dateFrom.Date));
                    if (inventory == null)
                    {
                        OB.BL.Contracts.Data.Properties.Inventory inv = new OB.BL.Contracts.Data.Properties.Inventory();
                        inv.QtyRoomOccupied = 0;
                        inv.RoomType_UID = roomTypeId;
                        inv.Date = dateFrom;
                        inventoriesToUpdate.Add(inv);
                        entitiesToAdd.Add(inv);
                        result.Add(inv);
                    }
                    else
                    {
                        inventory.QtyRoomOccupied = inventory.QtyRoomOccupied > 0 ? inventory.QtyRoomOccupied - 1 : 0;
                    }
                    dateFrom = dateFrom.AddDays(1);
                }
            }

            propertyRepo.UpdateInventoryDetails(new Contracts.Requests.UpdateInventoryDetailsRequest { InventoriesToUpdate = entitiesToAdd });

            return result;
        }

        private void DecrementInventoryDetails(DateTime dateTime, long? roomTypeId)
        {
            var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

            if (roomTypeId.HasValue)
            {
                var inventoryList = propertyRepo.ListInventory(new Contracts.Requests.ListInventoryRequest
                {
                    roomTypeIdsAndDateRange = new List<Contracts.Data.Properties.InventorySearch> { new Contracts.Data.Properties.InventorySearch { RoomType_UID = roomTypeId.HasValue ? roomTypeId.Value : 0,
                    DateFrom = dateTime.Date, DateTo = dateTime.Date} }
                });

                OB.BL.Contracts.Data.Properties.Inventory inventory = null;
                if (inventoryList.Any())
                    inventory = inventoryList.Last();

                if (inventory == null)
                {
                    OB.BL.Contracts.Data.Properties.Inventory inv = new OB.BL.Contracts.Data.Properties.Inventory();
                    inv.QtyRoomOccupied = 0;
                    inv.RoomType_UID = roomTypeId.Value;
                    inv.Date = dateTime.Date;
                    propertyRepo.UpdateInventoryDetails(new Contracts.Requests.UpdateInventoryDetailsRequest { InventoriesToUpdate = new List<contractsProperties.Inventory> { inv } });
                }
                else
                {
                    inventory.QtyRoomOccupied = inventory.QtyRoomOccupied > 0 ? inventory.QtyRoomOccupied - 1 : 0;
                }
            }
        }

        /// <summary>
        /// Created By :: Hitesh Patel
        /// Created Date :: 18 August 2012
        /// Desc :: To insert partial payment details for reservation.
        /// </summary>
        /// <param name="Reservation_UID"></param>
        /// <param name="interestRate"></param>
        /// <param name="installmentAmount"></param>
        /// <param name="parcel"></param>
        /// <returns></returns>
        private List<ReservationPartialPaymentDetail> CreateReservationPartialPaymentDetails(long reservationUID, decimal? interestRate, decimal? installmentAmount, int parcel)
        {
            var result = new List<ReservationPartialPaymentDetail>();

            for (int i = 1; i <= parcel; i++)
            {
                ReservationPartialPaymentDetail obj = new ReservationPartialPaymentDetail();
                obj.Reservation_UID = reservationUID;
                obj.InstallmentNo = i;
                obj.InterestRate = interestRate;
                obj.Amount = installmentAmount ?? 0;
                obj.IsPaid = false;
                obj.CreatedDate = DateTime.UtcNow;
                obj.ModifiedDate = DateTime.UtcNow;

                result.Add(obj);
            }

            return result;
        }

        private void ApplyCreditCardTokenizationRules(domainReservations.Reservation reservation, ReservationPaymentDetail reservationPaymentDetail, byte[] newHashCode, contractsProperties.PropertySecurityConfiguration propertySecConfig, bool paymentIsValid)
        {
            var securityRepo = this.RepositoryFactory.GetOBSecurityRepository();
            bool paymentGatewayTokenizationFailed = !paymentIsValid;
            bool obTokenizationFailed = false;

            try
            {
                // Get Property Security Configuration to know if Property has OB Tokenization enabled
                if (propertySecConfig != null)
                {
                    reservationPaymentDetail.PaymentGatewayTokenizationIsActive = propertySecConfig.IsProtectedWithPaymentGateway == true;
                    reservationPaymentDetail.OBTokenizationIsActive = propertySecConfig.IsProtectedWithOmnibees == true;

                    // If OB Tokenization is active or PaymentGateway tokenization return errors (ProtectedCreditCard Braspag service)
                    if (newHashCode != null && (paymentGatewayTokenizationFailed || reservationPaymentDetail.OBTokenizationIsActive))
                    {
                        // If Credit Card not Saved yet on BraspagProtectedCard
                        if (!string.IsNullOrEmpty(reservationPaymentDetail.CardNumber) && (newHashCode != reservationPaymentDetail.HashCode || string.IsNullOrEmpty(reservationPaymentDetail.CreditCardToken)))
                        {
                            // Decripts card number to Save Card on BraspagProtectedCard
                            var decryptRequest = new Contracts.Requests.ListCreditCardRequest { CreditCards = new List<string> { reservationPaymentDetail.CardNumber } };
                            var decryptedCardNumber = securityRepo.DecryptCreditCards(decryptRequest).FirstOrDefault();

                            var protectedCardService = new PaymentGatewayFactory().BrasPag();
                            var requestSave = new PaymentGatewaysLibrary.BrasPagGateway.Classes.SaveCreditCardRequest()
                            {
                                CustomerName = reservation.GuestFirstName + " " + reservation.GuestLastName,
                                CardHolder = reservationPaymentDetail.CardName,
                                CardNumber = decryptedCardNumber,
                                CardExpiration = reservationPaymentDetail.ExpirationDate,
                            };

                            if (string.IsNullOrWhiteSpace(requestSave.CustomerName))
                                requestSave.CustomerName = requestSave.CardHolder;

                            // Save Credit Card on Braspag
                            var tokenResponse = protectedCardService.SaveCreditCard(requestSave);
                            if (tokenResponse.JustClickKey != Guid.Empty)
                                reservationPaymentDetail.CreditCardToken = tokenResponse.JustClickKey.ToString();

                            // Send Email if Braspag return errors
                            if (tokenResponse.Errors.Any())
                            {
                                obTokenizationFailed = true;
                                var creditCardErrosList = tokenResponse.Errors.Select(x => Convert.ToInt32(x) + " - " + ProjectGeneral.GetEnumDescription(x));

                                var arguments = new Dictionary<string, object>();
                                arguments.Add("Reservation_UID", reservation.UID);
                                arguments.Add("PaymentMethod_UID", reservationPaymentDetail.PaymentMethod_UID);
                                arguments.Add("PropertySecurityConfiguration_UID", propertySecConfig.UID);
                                arguments.Add("SaveCreditCard Errors", string.Join("; ", creditCardErrosList));
                                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), null, LogSeverity.High, arguments, null, "Credit Card Tokenization Failed");

                                Logger.Error("SaveCreditCard Errors (Tokenization)", creditCardErrosList);
                            }
                        }
                        else
                            obTokenizationFailed = true;

                        if (obTokenizationFailed)
                            reservationPaymentDetail.PaymentGatewayTokenizationIsActive = false; // this flag controls if cc number will be erased by a periodically job

                        // Clears Credit Card info
                        // CVV cannot be cleared because Braspag don't store the CVV
                        if (!reservationPaymentDetail.PaymentGatewayTokenizationIsActive || !obTokenizationFailed)
                        {
                            reservationPaymentDetail.CardNumber = null;
                            reservationPaymentDetail.ExpirationDate = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Clears Credit Card info if Braspag return error
                if (propertySecConfig != null && propertySecConfig.IsProtectedWithOmnibees == true)
                {
                    reservationPaymentDetail.CardNumber = null;
                    reservationPaymentDetail.ExpirationDate = null;
                }

                var arguments = new Dictionary<string, object>();
                arguments.Add("OB Tokenization failed", obTokenizationFailed);
                arguments.Add("PaymentGateway Tokenization failed", paymentGatewayTokenizationFailed);
                arguments.Add("Reservation_UID", reservation.UID);
                arguments.Add("PaymentMethod_UID", reservationPaymentDetail.PaymentMethod_UID);
                arguments.Add("PropertySecurityConfiguration_UID", propertySecConfig != null ? (long?)propertySecConfig.UID : null);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments, null, "Credit Card Tokenization Failed");

                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Insert ReservationPaymentDetail in ReservationPaymentDetails table
        /// </summary>
        /// <param name="objReservationPaymentDetail"></param>
        /// <returns></returns>
        private ReservationPaymentDetail CreateReservationPaymentDetails(ReservationDataContext reservationContext, domainReservations.Reservation reservation, contractsReservations.ReservationPaymentDetail objReservationPaymentDetail, contractsProperties.PropertySecurityConfiguration propertySecurityConfigObj, bool paymentIsValid)
        {
            var securityRepo = this.RepositoryFactory.GetOBSecurityRepository();
            long reservationUID = reservationContext.ReservationUID;

            ReservationPaymentDetail reservationPaymentDetail = null;

            if (objReservationPaymentDetail == null)
                return reservationPaymentDetail;

            reservationPaymentDetail = reservation.ReservationPaymentDetails.SingleOrDefault();

            if (reservationPaymentDetail == null)
            {
                //TMOREIRA:
                reservationPaymentDetail = new ReservationPaymentDetail();
                BusinessObjectToDomainTypeConverter.Map(objReservationPaymentDetail, reservationPaymentDetail);

                reservationPaymentDetail.Reservation_UID = reservationUID;
            }
            else
            {
                //TMOREIRA: CHANGED significantly TEST VERY WELL this part
                //Load first and then change.
                Contract.Requires(objReservationPaymentDetail.UID > 0, "Expecting objReservationPaymentDetail with a valid UID");
                //copy all properties from the DTO to the domain model except the ones we are going to change if they aren't null
                BusinessObjectToDomainTypeConverter.Map(objReservationPaymentDetail, reservationPaymentDetail,
                    new HashSet<string>
                    {
                        ReservationPaymentDetail.PROP_NAME_RESERVATION_UID,
                        ReservationPaymentDetail.PROP_NAME_CARDNUMBER,
                        ReservationPaymentDetail.PROP_NAME_CVV,
                        ReservationPaymentDetail.PROP_NAME_CARDNAME,
                        ReservationPaymentDetail.PROP_NAME_EXPIRATIONDATE,
                        ReservationPaymentDetail.PROP_NAME_ACTIVATIONDATE,
                    });
            }

            if (!String.IsNullOrEmpty(objReservationPaymentDetail.CardNumber))
                reservationPaymentDetail.CardNumber = objReservationPaymentDetail.CardNumber;

            if (!String.IsNullOrEmpty(objReservationPaymentDetail.CVV))
                reservationPaymentDetail.CVV = objReservationPaymentDetail.CVV;

            if (!String.IsNullOrEmpty(objReservationPaymentDetail.CardName))
                reservationPaymentDetail.CardName = objReservationPaymentDetail.CardName;

            if (objReservationPaymentDetail.ExpirationDate.HasValue)
            {
                DateTime minValue = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;

                if (minValue < objReservationPaymentDetail.ExpirationDate.Value)
                    reservationPaymentDetail.ExpirationDate = objReservationPaymentDetail.ExpirationDate;
            }

            if (objReservationPaymentDetail.ActivationDate.HasValue)
                reservationPaymentDetail.ActivationDate = objReservationPaymentDetail.ActivationDate;

            // Generate Credit Card Hash Code
            var hashRequest = new Contracts.Requests.GetCreditCardHashRequest()
            {
                CardHolder = reservationPaymentDetail.CardName,
                EncryptedCardNumber = reservationPaymentDetail.CardNumber,
                EncryptedCVV = reservationPaymentDetail.CVV,
                CardExpiration = reservationPaymentDetail.ExpirationDate ?? new DateTime()
            };
            var hashCode = securityRepo.GetCreditCardHash(hashRequest);

            // Tokenization
            this.ApplyCreditCardTokenizationRules(reservation, reservationPaymentDetail, hashCode, propertySecurityConfigObj, paymentIsValid);

            reservationPaymentDetail.HashCode = hashCode;
            reservationPaymentDetail.CreatedDate = DateTime.UtcNow;
            return reservationPaymentDetail;
        }

        //UPDATE DW DATABASE
        private void ProcessDWRatesAvailabilityRealTimeByRateRoomUID(long rateroom_uid)
        {
            try
            {
                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    var ratesRepo = this.RepositoryFactory.GetOBRateRepository();
                    ratesRepo.ExecStoredProc_DW_ProcessRatesAvailabilityRealTime(new Contracts.Requests.ProcessRatesAvailabilityRealTimeRequest { RateRoomUID = rateroom_uid });
                }
            }
            catch (Exception ex)
            {
                var arguments = new Dictionary<string, object>();
                arguments.Add("rateroom_uid", rateroom_uid);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments);
            }
        }

        private void SetReservationRoomPolicyWithLanguageTranslation(long? baseLanguage, long? reservationLanguage, ref contractsReservations.ReservationRoom obj,
            List<contractsReservations.ReservationRoomDetail> objReservationroomdetail, DateTime checkInDate, long? reservationBaseCurrencyId,
            bool handleCancelationCosts, bool handleDepositCosts, domainReservations.Reservation reservation,
            contractsReservations.ReservationsAdditionalData reservationsAdditionalData = null, ReservationDataContext reservationContext = null, bool concatDepositPolicyName = true)
        {
            var otherPolicyRepo = RepositoryFactory.GetOBOtherPolicyRepository();
            var reservationHelper = this.Resolve<IReservationHelperPOCO>();

            if (reservationContext == null)
                reservationContext = new ReservationDataContext();

            if (obj.Rate_UID.HasValue)
            {
                string missedTranlation = "This information does not exist in your language...!";
                var rrdList = new List<OB.BL.Contracts.Data.Rates.RateRoomDetailReservation>();
                foreach (var rrd in objReservationroomdetail)
                {
                    rrdList.Add(new OB.BL.Contracts.Data.Rates.RateRoomDetailReservation
                    {
                        FinalPrice = (rrd.AdultPrice.HasValue ? (rrd.AdultPrice + (rrd.ChildPrice ?? 0)) : rrd.Price) ?? 0,
                        Date = rrd.Date
                    });
                }

                #region DEPOSIT POLICIES

                var tmpDepositPolicy = reservationHelper.GetMostRestrictiveDepositPolicy(obj.DateFrom.Value, obj.DateTo.Value, obj.Rate_UID,
                                        reservationBaseCurrencyId, reservationLanguage, rrdList);

                if (tmpDepositPolicy != null && !reservationContext.DepositPolicies.ContainsKey(obj.Rate_UID ?? 0))
                    reservationContext.DepositPolicies.Add(obj.Rate_UID ?? 0, BusinessObjectTypeToQueryResultObjectConverter.Convert(tmpDepositPolicy));

                if (handleDepositCosts && tmpDepositPolicy != null)
                {
                    string depositPolicyStr = tmpDepositPolicy.Name;
                    if (concatDepositPolicyName)
                    {
                        if (baseLanguage == reservationLanguage)
                            depositPolicyStr = tmpDepositPolicy.Name + " : " + tmpDepositPolicy.Description;
                        else if (!string.IsNullOrEmpty(tmpDepositPolicy.TranslatedDescription))
                            depositPolicyStr = tmpDepositPolicy.TranslatedName + " : " + tmpDepositPolicy.TranslatedDescription;
                        else
                            depositPolicyStr = missedTranlation + " \n " + tmpDepositPolicy.Name;
                    }

                    obj.DepositPolicy = depositPolicyStr;
                    obj.DepositPolicy_UID = tmpDepositPolicy.UID;
                    obj.IsDepositAllowed = tmpDepositPolicy.IsDepositCostsAllowed;
                    obj.DepositCosts = tmpDepositPolicy.DepositCosts;
                    obj.DepositDays = tmpDepositPolicy.Days;
                    obj.DepositPaymentModel = tmpDepositPolicy.PaymentModel;
                    obj.DepositValue = tmpDepositPolicy.Value;

                    obj.DepositValue = reservation == null || obj.DepositPaymentModel == 2 ? obj.DepositValue
                                        : obj.DepositValue * reservation.PropertyBaseCurrencyExchangeRate;

                    obj.DepositNrNights = tmpDepositPolicy.NrNights;
                }
                else if (!handleDepositCosts) // PULL não validam politica de deposito, por isso é necessário verificar
                {
                    // Validate Deposit Policy
                    var sentDepositPolicy = new contractsRates.DepositPolicy
                    {
                        Days = obj.DepositDays,
                        DepositCosts = obj.DepositCosts,
                        IsDepositCostsAllowed = obj.IsDepositAllowed,
                        NrNights = obj.DepositNrNights,
                        PaymentModel = obj.DepositPaymentModel,
                        Value = obj.DepositValue
                    };

                    if (tmpDepositPolicy != null)
                    {
                        if (!reservationHelper.CompareDepositPolicies(tmpDepositPolicy, sentDepositPolicy))
                            throw Errors.DepositPolicyHasChangeError.ToBusinessLayerException();
                    }
                    else if (sentDepositPolicy.PaymentModel != null)
                        throw Errors.DepositPolicyHasChangeError.ToBusinessLayerException();
                }

                #endregion DEPOSIT POLICIES

                #region CANCELATION POLICIES

                var tmpCancellationPolicy = reservationHelper.GetMostRestrictiveCancelationPolicy(obj.DateFrom.Value, obj.DateTo.Value, obj.Rate_UID,
                                        reservationBaseCurrencyId, reservationLanguage, rrdList);

                if (tmpCancellationPolicy != null && !reservationContext.CancellationPolicies.ContainsKey(obj.Rate_UID ?? 0))
                    reservationContext.CancellationPolicies.Add(obj.Rate_UID ?? 0, BusinessObjectTypeToQueryResultObjectConverter.Convert(tmpCancellationPolicy));

                if (handleCancelationCosts)
                {
                    if (tmpCancellationPolicy != null)
                    {
                        if (baseLanguage == reservationLanguage)
                            obj.CancellationPolicy = tmpCancellationPolicy.Name + " : " + tmpCancellationPolicy.Description;
                        else if (!string.IsNullOrEmpty(tmpCancellationPolicy.TranslatedDescription))
                            obj.CancellationPolicy = tmpCancellationPolicy.TranslatedName + " : " + tmpCancellationPolicy.TranslatedDescription;
                        else
                            obj.CancellationPolicy = missedTranlation + " \n " + tmpCancellationPolicy.Name;

                        obj.IsCancellationAllowed = tmpCancellationPolicy.IsCancellationAllowed;
                        obj.CancellationCosts = tmpCancellationPolicy.CancellationCosts ?? false;
                        obj.CancellationPolicyDays = tmpCancellationPolicy.Days;
                        obj.CancellationPaymentModel = tmpCancellationPolicy.PaymentModel;
                        obj.CancellationValue = tmpCancellationPolicy.Value;
                        obj.CancellationNrNights = tmpCancellationPolicy.NrNights;

                        obj.CancellationValue = reservation == null || obj.CancellationPaymentModel == 2 ? obj.CancellationValue
                                            : obj.CancellationValue * reservation.PropertyBaseCurrencyExchangeRate;

                        obj.CancellationNrNights = tmpCancellationPolicy.NrNights;
                    }
                    else
                    {
                        /* na tabela CancellationPolicies o valor por defeito da coluna IsCancelationAllowed é true,
                         * quando a reserva não tem associado uma politica de cancelamento temos que definir estes valores por defeito do reservationRoom
                         */
                        obj.IsCancellationAllowed = false;
                        obj.CancellationPolicyDays = 0;
                    }
                }

                #endregion CANCELATION POLICIES

                #region GENERAL POLICIES

                var otherPolicy = otherPolicyRepo.GetOtherPoliciesByRateId(new Contracts.Requests.GetOtherPoliciesRequest { RateId = obj.Rate_UID, LanguageUID = reservationLanguage });

                if (otherPolicy != null && !reservationContext.OtherPolicies.ContainsKey(obj.Rate_UID ?? 0))
                    reservationContext.OtherPolicies.Add(obj.Rate_UID ?? 0, BusinessObjectTypeToQueryResultObjectConverter.Convert(otherPolicy));

                if (otherPolicy != null && otherPolicy.UID > 0)
                {

                    if (baseLanguage == reservationLanguage)
                    {
                        obj.OtherPolicy = otherPolicy.OtherPolicy_Name + " : " + otherPolicy.OtherPolicy_Description;
                    }
                    else if (!string.IsNullOrEmpty(otherPolicy.TranslatedName))
                    {
                        obj.OtherPolicy = otherPolicy.TranslatedName + " : " + otherPolicy.TranslatedDescription;
                    }
                    else
                        obj.OtherPolicy = missedTranlation + " \n " + otherPolicy.OtherPolicy_Name;

                    if (obj.OtherPolicy != null && obj.OtherPolicy.Length > 4000)
                    {
                        obj.OtherPolicy = obj.OtherPolicy.Substring(0, 3997) + "...";
                    }
                }

                #endregion GENERAL POLICIES
            }
        }

        /// <summary>
        /// <b>Validate Reservation</b> In case of success returns 0, in case of error returns an error code:
        /// <para>Operator CREDIT LIMIT EXCEDED error=<b>-3001</b></para>
        /// <para>Operator PAYMENT METHOD NOT VALID error=<b>-3002</b></para>
        /// </summary>
        /// <param name="objReservation"></param>
        /// <param name="objReservationPaymentDetail"></param>
        /// <param name="objReservationRoom"></param>
        /// <param name="reservationContext"></param>
        /// <returns></returns>
        public long ValidateReservation(contractsReservations.Reservation objReservation, contractsReservations.ReservationPaymentDetail objReservationPaymentDetail,
                        List<contractsReservations.ReservationRoom> objReservationRoom, ReservationDataContext reservationContext)
        {
            long result = 0;

            // Validate if Payment Method Type is active on Operator
            if (!ValidateOperatorsPaymentMethodType(objReservation, objReservationRoom, reservationContext))
                return -3002;

            // Validate Operator Credit Limit
            if (!ValidateOperatorCreditLimit(objReservation, objReservationPaymentDetail, reservationContext))
                return -3001;

            return result;
        }

        /// <summary>
        /// validate operator credit limit
        /// </summary>
        public bool ValidateOperatorCreditLimit(contractsReservations.Reservation objReservation, contractsReservations.ReservationPaymentDetail objReservationPaymentDetail,
            ReservationDataContext reservationContext)
        {
            bool result = true;
            OB.BL.Contracts.Data.Channels.ChannelsProperty channel = null;

            if (objReservation.Channel_UID == null || objReservation.TotalAmount == null || !objReservation.PaymentMethodType_UID.HasValue
                || objReservation.IsOnRequest == true)
                return result;

            var channelId = (long)objReservation.Channel_UID;
            var propertyId = objReservation.Property_UID;

            var repoFactory = this.Resolve<IRepositoryFactory>();
            var propertiesRepo = repoFactory.GetOBPropertyRepository();
            var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();

            var channelName = reservationContext.ChannelName;

            // Faturada
            if (objReservation.PaymentMethodType_UID.HasValue && objReservation.IsOnRequest == false)
            {
                var payType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest
                {
                    UIDs =
                    new List<long>() { objReservation.PaymentMethodType_UID ?? 0 }
                }).FirstOrDefault();
                if (payType != null && payType.Code == (int)Constants.PaymentMethodTypesCode.Invoicing)
                {
                    channel = propertiesRepo.ListChannelsProperty(new Contracts.Requests.ListChannelsPropertyRequest { ChannelUIDs = new List<long> { channelId }, PropertyUIDs = new List<long> { propertyId } }).FirstOrDefault();

                    if (channel != null && channel.IsOperatorsCreditLimit)
                    {
                        if (!channel.OperatorCreditUsed.HasValue)
                            channel.OperatorCreditUsed = 0;

                        if ((channel.OperatorCreditUsed + objReservation.TotalAmount) <= channel.OperatorCreditLimit)
                            result = true;
                        else
                        {
                            result = false;
                            SendCreditLimitExcededEmail(objReservation, channelName, channel.OperatorCreditLimit.Value);
                        }
                    }
                }
                // Pré pagamento
                else if (payType != null && payType.Code == (int)Constants.PaymentMethodTypesCode.PrePayment)
                {
                    channel = propertiesRepo.ListChannelsProperty(new Contracts.Requests.ListChannelsPropertyRequest { ChannelUIDs = new List<long> { channelId }, PropertyUIDs = new List<long> { propertyId } }).FirstOrDefault();

                    if (channel != null && channel.IsActivePrePaymentCredit)
                    {
                        if (!channel.PrePaymentCreditUsed.HasValue)
                            channel.PrePaymentCreditUsed = 0;

                        if ((channel.PrePaymentCreditUsed + objReservation.TotalAmount) <= channel.PrePaymentCreditLimit)
                            result = true;
                        else
                        {
                            result = false;
                            SendCreditLimitExcededEmail(objReservation, channelName, channel.PrePaymentCreditLimit.Value);
                        }
                    }

                    // Pre paid reservation are always paid - US 17506
                    objReservation.IsPaid = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Validates if channel has the payment method type choosen in the reservation.
        /// </summary>
        /// <param name="objReservation">The object reservation.</param>
        /// <param name="objReservationRoom">The object reservation room.</param>
        /// <returns></returns>
        private bool ValidateOperatorsPaymentMethodType(contractsReservations.Reservation objReservation, List<contractsReservations.ReservationRoom> objReservationRoom,
            ReservationDataContext reservationContext)
        {
            var channelType = reservationContext.ChannelOperatorType;
            var channelUID = reservationContext.Channel_UID;

            if (channelUID > 0 && channelType != (int)Constants.OperatorsType.None)
            {
                // Verify if payment type is approved
                if (reservationContext.ChannelProperty_UID.GetValueOrDefault() > 0
                    &&
                    reservationContext.ChannelPropertyOperatorBillingType == (int)Constants.ApprovalBilling.Unapproved
                    &&
                    objReservation.PaymentMethodType_UID == (int)Constants.PaymentMethodTypesCode.Invoicing)
                    return false;

                var rates = objReservationRoom.Select(x => x.Rate_UID.GetValueOrDefault()).Distinct();
                if (reservationContext.RateChannelsAndPaymentsCount < rates.Count())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Sends the credit limit exceded email.
        /// </summary>
        /// <param name="objReservation">The object reservation.</param>
        /// <param name="name">The name of operator or agency</param>
        /// <param name="creditLimit">The credit limit.</param>
        private void SendCreditLimitExcededEmail(contractsReservations.Reservation objReservation, string name, decimal creditLimit)
        {
            // Insert Information in Reservation Internal Notes
            objReservation.InternalNotes += System.Environment.NewLine + string.Format(Resources.Resources.lblCreditLimitReached, name, creditLimit);

            // Send Mail OPERADOR CREDITO LIMITE
            this.InsertPropertyQueue(objReservation.Property_UID, (long)Constants.SystemEventsCode.CreditLimit, objReservation.Channel_UID.Value, true);
        }

        /// <summary>
        /// Increments Credit Used of TPIProperty.
        /// </summary>
        /// <param name="paymentMethodTypesUID">The payment method types uid.</param>
        /// <param name="propertyUID">The property uid.</param>
        /// <param name="reservationValue">The reservation value.</param>
        /// <param name="tpi_uid">The tpi uid.</param>
        /// <returns><c>true</c> if remaining credit (Credit Limit - Credit Used) is negative.</returns>
        bool IncrementTPICreditUsed(long paymentMethodTypesUID, long propertyUID, decimal reservationValue, long tpi_uid)
        {
            decimal remainingValue = 0;

            try
            {
                var crmRepo = this.RepositoryFactory.GetOBCRMRepository();
                var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    var tpiRepo = RepositoryFactory.GetThirdPartyIntermediarySqlRepository(unitOfWork);

                    // Get TPI
                    var requestTpi = new ListThirdPartyIntermediariesLightRequest { UIDs = new List<long> { tpi_uid }, PageSize = 1 };
                    var tpi = crmRepo.ListThirdPartyIntermediariesLight(requestTpi).FirstOrDefault();

                    // If tpi doesn't exists
                    if (tpi == null)
                        return false;

                    // Get TPI Property
                    var tpiPropertyRequest = new Contracts.Requests.ListTPIPropertyRequest { PropertyIds = new List<long> { propertyUID }, TPI_Ids = new List<long> { tpi_uid } };
                    var tpiProperty = crmRepo.ListTpiProperty(tpiPropertyRequest).FirstOrDefault();

                    // If TPI Property doens't exists
                    if (tpiProperty == null)
                        return false;

                    // Get Payment Type
                    var paymentTypeRequest = new Contracts.Requests.ListPaymentMethodTypesRequest { UIDs = new List<long>() { paymentMethodTypesUID }, PageSize = 1 };
                    var payType = paymentMethodTypeRepo.ListPaymentMethodTypes(paymentTypeRequest).FirstOrDefault();

                    // Invoicing
                    if (payType != null && payType.Code == (int)Constants.PaymentMethodTypesCode.Invoicing)
                    {
                        if (tpiProperty.ApprovalBilling_UID == (int)Constants.ApprovalBilling.Unapproved)
                            throw Errors.TpiInvalidPayment.ToBusinessLayerException();

                        // Increments CreditUsed of TPIProperty
                        var updRequest = new DL.Common.Criteria.UpdateCreditCriteria
                        {
                            UpdateCreditUsed = true,
                            PropertyUid = propertyUID,
                            TpiUid = tpi_uid,
                            IncrementValue = reservationValue
                        };
                        var updatedTpiProp = tpiRepo.UpdateTpiPropertyCredit(updRequest);

                        remainingValue = (tpiProperty.CreditLimit.HasValue) ?
                            tpiProperty.CreditLimit.Value - (updatedTpiProp.CreditUsed ?? 0) : updatedTpiProp.CreditUsed ?? 0;
                    }

                    // Pre Payment
                    else if (payType != null && payType.Code == (int)Constants.PaymentMethodTypesCode.PrePayment)
                    {
                        // Increments PrePaidCreditUsed of TPIProperty
                        var updRequest = new DL.Common.Criteria.UpdateCreditCriteria
                        {
                            UpdatePrePaidCreditUsed = true,
                            PropertyUid = propertyUID,
                            TpiUid = tpi_uid,
                            IncrementValue = reservationValue
                        };
                        var updatedTpiProp = tpiRepo.UpdateTpiPropertyCredit(updRequest);

                        remainingValue = (tpiProperty.PrePaidCreditLimit.HasValue) ?
                            tpiProperty.PrePaidCreditLimit.Value - (updatedTpiProp.PrePaidCreditUsed ?? 0) : updatedTpiProp.PrePaidCreditUsed ?? 0;
                    }
                }
            }
            catch (BusinessLayerException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "paymentMethodTypesUID", paymentMethodTypesUID },
                    { "propertyUID", propertyUID },
                    { "reservationValue", reservationValue },
                    { "tpi_uid", tpi_uid }
                };
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, args);
                Logger.Error(ex);
            }

            return (remainingValue < 0);
        }

        private void DecrementCreditUsed(domainReservations.Reservation reservation, bool makeDiscount = true, bool markAsPaid = true)
        {
            if (reservation == null || reservation.IsPaid == true || !reservation.PaymentMethodType_UID.HasValue)
                return;

            var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
            var payType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest
            {
                UIDs = new List<long>() { reservation.PaymentMethodType_UID ?? 0 },
                PageSize = 1
            }).FirstOrDefault();

            if (payType != null && (payType.Code == (int)Constants.PaymentMethodTypesCode.Invoicing || payType.Code == (int)Constants.PaymentMethodTypesCode.PrePayment))
                this.SetReservationIsPaid(reservation, 65, makeDiscount, markAsPaid);
        }


        #region PAYMENT GATEWAY OPERATIONS

        /// <summary>
        /// Verify if property has a payment gateway active an make payment
        /// <para>MaxiPagoGeneralError = <b>-1</b></para><para>Reservation base currency not defined=<b>-3</b></para><para>MaxiPagoAcquirerError = <b>-1022</b></para><para>MaxiPagoParametersError = <b>-1024</b></para><para>MaxiPagoMerchantCredentialError = <b>-1025</b></para><para>MaxiPagoInternalError = <b>-2048</b></para><para>BrasPag - NotAuthorized = <b>-2</b></para><para>Reservation base currency not defined=<b>-3</b></para><para>BrasPag - = TimedOut<b>-99</b></para><para>BrasPag - = CanceldCard = <b>-77</b></para><para>BrasPag - = CardWithProblems = <b>-70</b></para><para>BrasPag - = BlockedCard = <b>-78</b></para><para>BrasPag - = ExpiredCard = <b>-57</b></para><para>BrasPag - General Error = <b>-900</b></para>
        /// </summary>
        /// <param name="objguest">The objguest.</param>
        /// <param name="objReservationPaymentDetail">The object reservation payment detail.</param>
        /// <param name="reservation">The reservation.</param>
        /// <param name="groupRule">The group rule.</param>
        /// <returns></returns>
        private int MakePaymentWithPaymentGateway(contractsCRMOB.Guest objguest, contractsReservations.ReservationPaymentDetail objReservationPaymentDetail,
            domainReservations.Reservation reservation, bool skipInterestCalculation, GroupRule groupRule = null, contractsRates.PromotionalCode promotionalCode = null, string antifraudDeviceFingerPrintId = null,
            string requestId = null, string cookie = null)
        {
            var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();
            var securityRepo = this.RepositoryFactory.GetOBSecurityRepository();
            var appRepo = RepositoryFactory.GetOBAppSettingRepository();

            string cardNumber;
            string cardCVV;

            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.EncryptCreditCard))
            {
                cardNumber = objReservationPaymentDetail.CardNumber;
                cardCVV = objReservationPaymentDetail.CVV;
            }
            else
            {
                cardNumber = securityRepo.DecryptCreditCards(
                        new Contracts.Requests.ListCreditCardRequest
                        {
                            CreditCards = new List<string> { objReservationPaymentDetail.CardNumber }
                        })
                    .FirstOrDefault();

                cardCVV = securityRepo.DecryptCreditCards(new Contracts.Requests.ListCreditCardRequest { CreditCards = new List<string> { objReservationPaymentDetail.CVV } }).FirstOrDefault();
            }

            #region Validate payment information

            // validate if currency if filled
            if (reservation.ReservationBaseCurrency_UID == null)
            {
                var arguments = new Dictionary<string, object>();
                arguments.Add("Reservation", reservation);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Reservation base currency is not defined"), LogSeverity.Critical, arguments);
                return -3;
            }

            #endregion Validate payment information

            // Get Payment Gateway Information
            var paymentGatewayInformation = propertyRepo.GetActivePaymentGatewayConfiguration(new Contracts.Requests.ListActivePaymentGatewayConfigurationRequest
            {
                PropertyId = reservation.Property_UID,
                CurrencyId = reservation.ReservationBaseCurrency_UID.Value,
                PaymentMethodId = objReservationPaymentDetail.PaymentMethod_UID.Value
            });

            // Map to gateway configuration
            var paymentConfig = GetPaymentGatewayConfiguration(paymentGatewayInformation);
            PaymentMessageResult returnMessage = new PaymentMessageResult();

            try
            {
                if (paymentGatewayInformation != null)
                {
                    #region Prepare data to payment gateway

                    int year, month;

                    decimal totalTemp = 0;
                    if (reservation.TotalAmount.HasValue && reservation.PropertyBaseCurrencyExchangeRate.HasValue)
                    {

                        if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ConvertValuesFromClientToRates) && reservation.ReservationBaseCurrency_UID != reservation.ReservationCurrency_UID)
                        {
                            var reservationHelperPoco = Resolve<IReservationHelperPOCO>();
                            var exchangeRate = reservationHelperPoco.GetExchangeRateBetweenCurrenciesByPropertyId((long)reservation.ReservationCurrency_UID, (long)reservation.ReservationBaseCurrency_UID, reservation.Property_UID);
                            totalTemp = Math.Round((decimal)(reservation.TotalAmount * exchangeRate), 2, MidpointRounding.AwayFromZero);
                        }

                        else
                            totalTemp = Math.Round((decimal)(reservation.TotalAmount / reservation.PropertyBaseCurrencyExchangeRate), 2, MidpointRounding.AwayFromZero);

                    }
                    decimal totalValue = totalTemp;
                    int installments = reservation.IsPartialPayment.HasValue && reservation.IsPartialPayment.Value ? reservation.NoOfInstallment ?? 1 : 1;
                    string guestInfo = string.Format("Name: {0} - Email: {1}", string.Format("{0} {1}", objguest.FirstName, objguest.LastName), objguest.Email);

                    var names = objReservationPaymentDetail.CardName.Split(new char[] { ' ' }, 2);
                    string firstName, lastName;
                    firstName = lastName = string.Empty;
                    if (names.Count() > 1)
                    {
                        firstName = names[0];
                        lastName = names[1];
                    }
                    else if (names.Any())
                        firstName = lastName = names[0];

                    #endregion Prepare data to payment gateway

                    if (!skipInterestCalculation)
                        totalValue = GetValueWithInterestRate(totalValue, reservation, objReservationPaymentDetail);

                    var gatewayFactory = Resolve<IPaymentGatewayFactory>();

                    switch (int.Parse(paymentGatewayInformation.GatewayCode))
                    {
                        #region MAXIPAGO

                        case (int)OB.Reservation.BL.Constants.PaymentGateway.MaxiPago:

                            year = objReservationPaymentDetail.ExpirationDate.Value.Year;
                            month = objReservationPaymentDetail.ExpirationDate.Value.Month;

                            if ((reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value)
                                || (paymentGatewayInformation.PaymentAuthorizationTypeId != (int)OB.Reservation.BL.Constants.PaymentGatewayAuthorizationTypes.AuthorizationAndCapture))
                            {
                                returnMessage = gatewayFactory.MaxiPago(paymentConfig).Authorize(reservation.Number, totalValue,
                                                            month, year, cardCVV, cardNumber, installments.ToString(), paymentGatewayInformation.CurrencyIsoCode, guestInfo);

                                return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true,
                                                        objReservationPaymentDetail);
                            }
                            else
                            {
                                returnMessage = gatewayFactory.MaxiPago(paymentConfig).Sale(reservation.Number, totalValue,
                                                            month, year, cardCVV, cardNumber, installments.ToString(), paymentGatewayInformation.CurrencyIsoCode, guestInfo);

                                return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true,
                                                        objReservationPaymentDetail);
                            }

                        #endregion MAXIPAGO

                        #region BRASPAG

                        case (int)OB.Reservation.BL.Constants.PaymentGateway.BrasPag:
                            year = objReservationPaymentDetail.ExpirationDate.Value.Year;
                            month = objReservationPaymentDetail.ExpirationDate.Value.Month;

                            paymentConfig.IsTestEnvironment = WebConfigurationManager.AppSettings["IsBrasPagInTestMode"] != null && bool.Parse(WebConfigurationManager.AppSettings["IsBrasPagInTestMode"]);

                            // validate if country is available for braspag
                            if (string.IsNullOrEmpty(paymentGatewayInformation.CountryIsoCode))
                            {
                                var arguments = new Dictionary<string, object>();
                                arguments.Add("Reservation", reservation);
                                arguments.Add("Payment Gateway Information", paymentGatewayInformation);
                                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Country is not supported in BrasPag Gateway"), LogSeverity.Critical, arguments);
                                return -900;
                            }

                            var gatewayService = gatewayFactory.BrasPag(paymentConfig);

                            // Set Installments Payment Plan
                            var paymentPlan = PaymentGatewaysLibrary.BrasPagGateway.ModelsV2.Payments.Enums.InterestTypeEnum.ByMerchant;
                            if (paymentGatewayInformation.InstallmentPaymentPlanId == (int)PaymentGatewaysLibrary.BrasPagGateway.Constants.PaymentPlans.InstallmentsByCardIssuer)
                                paymentPlan = PaymentGatewaysLibrary.BrasPagGateway.ModelsV2.Payments.Enums.InterestTypeEnum.ByIssuer;
                            else if (paymentGatewayInformation.InstallmentPaymentPlanId == (int)PaymentGatewaysLibrary.BrasPagGateway.Constants.PaymentPlans.InstallmentsByEstablishment)
                                paymentPlan = PaymentGatewaysLibrary.BrasPagGateway.ModelsV2.Payments.Enums.InterestTypeEnum.ByMerchant;

                            //Fill Authorize Request
                            var authorizeRequest = new AuthorizeRequest();
                            FillAuthorizeRequest(authorizeRequest, installments, paymentGatewayInformation, month, year, cardNumber, cardCVV, totalValue, paymentPlan, objguest?.CreatedDate);

                            // Authorize Only
                            if ((reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value)
                                || (paymentGatewayInformation.PaymentAuthorizationTypeId == (int)Constants.PaymentGatewayAuthorizationTypes.Authorization))
                            {
                                returnMessage = gatewayService.Authorize(authorizeRequest);

                                if (returnMessage.IsOnReview)
                                {
                                    reservation.Status = (int)Constants.ReservationStatus.BookingOnRequest;
                                    reservation.IsOnRequest = true;
                                }
                            }
                            else
                            {
                                authorizeRequest.CreditCardPayment.TransactionTypes = PaymentGatewaysLibrary.BrasPagGateway.Constants.TransactionTypes.Capture;
                                returnMessage = gatewayService.Authorize(authorizeRequest);

                                if (returnMessage.IsOnReview)
                                {
                                    reservation.Status = (int)Constants.ReservationStatus.BookingOnRequest;
                                    reservation.IsOnRequest = true;
                                }
                            }

                            Logger.Info(new LogMessageBase
                            {
                                Area = "PaymentGateways",
                                MethodName = nameof(MakePaymentWithPaymentGateway),
                                Code = returnMessage.Errors?.Any() == true ? returnMessage.Errors.First().ErrorCode.ToString() : returnMessage.PaymentGatewayTransactionStatusCode,
                                Description = returnMessage.Errors?.Any() == true ? returnMessage.Errors.First().Description : $"{returnMessage.PaymentGatewayTransactionType} / {returnMessage.PaymentGatewayTransactionStatusDescription}",
                                RequestId = requestId
                            }, new LogEventPropertiesBase
                            {
                                Request = returnMessage.PaymentGatewayRequestXml,
                                Response = returnMessage.PaymentGatewayResponseXml
                            });

                            // Verificar se a transação foi cobrada, mas a reserva não vai entrar
                            var needsRefund = (returnMessage.PaymentGatewayTransactionStatusCode == ((int)GeneralPaymentStatusCodes.Authorized).ToString()
                                                || returnMessage.PaymentGatewayTransactionStatusCode == ((int)GeneralPaymentStatusCodes.PaymentConfirmed).ToString())
                                                && !returnMessage.IsTransactionValid;

                            int returnValue = -1;
                            try
                            {
                                returnValue = RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true,
                                                        objReservationPaymentDetail, requestId, returnMessage.Errors);

                            }
                            catch (PaymentGatewayException ex)
                            {
                                // Estornar Pagamento Caso o pagamento tenha sido efetuado, mas o antifraude tenha dado erro
                                if (needsRefund)
                                {
                                    Logger.Warn(new LogMessageBase
                                    {
                                        Area = "PaymentGateways",
                                        MethodName = nameof(MakePaymentWithPaymentGateway),
                                        Code = ((int)Internal.BusinessObjects.Errors.Errors.PaymentAuthorizedButFailedOnAntifraude).ToString(),
                                        Description = Internal.BusinessObjects.Errors.Errors.PaymentAuthorizedButFailedOnAntifraude.ToString(),
                                        RequestId = requestId
                                    });

                                    Guid.TryParse(returnMessage.PaymentGatewayTransactionID, out Guid transactionId);
                                    var voidReturnMessage = gatewayService.Void(new VoidRequest
                                    {
                                        PaymentId = transactionId,
                                        ReservationNr = reservation.Number,
                                        Amount = totalValue
                                    });

                                    Logger.Info(new LogMessageBase
                                    {
                                        Area = "PaymentGateways",
                                        MethodName = nameof(MakePaymentWithPaymentGateway),
                                        Code = voidReturnMessage.PaymentGatewayTransactionStatusCode,
                                        Description = $"{voidReturnMessage.PaymentGatewayTransactionType} / {voidReturnMessage.PaymentGatewayTransactionStatusDescription}",
                                        RequestId = requestId
                                    });

                                    RegisterPaymentGatewayTransaction(paymentGatewayInformation, voidReturnMessage, totalValue, reservation, false,
                                                            objReservationPaymentDetail, requestId, returnMessage.Errors);
                                }

                                ExceptionDispatchInfo.Capture(ex).Throw();
                            }

                            return returnValue;

                        #endregion BRASPAG

                        #region ADYEN

                        case (int)OB.Reservation.BL.Constants.PaymentGateway.Adyen:

                            int? auxYear = objReservationPaymentDetail.ExpirationDate.HasValue ? (int?)objReservationPaymentDetail.ExpirationDate.Value.Year : null;
                            int? auxMonth = objReservationPaymentDetail.ExpirationDate.HasValue ? (int?)objReservationPaymentDetail.ExpirationDate.Value.Month : null;

                            paymentConfig.IsTestEnvironment = WebConfigurationManager.AppSettings["IsAdyenInTestMode"] != null && bool.Parse(WebConfigurationManager.AppSettings["IsAdyenInTestMode"]);

                            // get data
                            var billingCountry = propertyRepo.ListCountries(new Contracts.Requests.ListCountryRequest { UIDs = new List<long> { reservation.BillingCountry_UID ?? 0 } }).FirstOrDefault();
                            var billingState = propertyRepo.ListCitiesDataTranslated(new Contracts.Requests.ListCityDataRequest { UIDs = new List<long> { reservation.BillingState_UID ?? 0 } }).FirstOrDefault();
                            string billingAddress = reservation.BillingAddress1 ?? string.Empty + " " + reservation.BillingAddress2 ?? string.Empty;
                            billingAddress = billingAddress.Trim();
                            var checkin = reservation?.ReservationRooms?.Min(x => x.DateFrom);
                            var checkout = reservation?.ReservationRooms?.Max(x => x.DateTo);

                            // Authorize Only
                            if ((reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value)
                                || (paymentGatewayInformation.PaymentAuthorizationTypeId != (int)Constants.PaymentGatewayAuthorizationTypes.AuthorizationAndCapture))
                            {
                                returnMessage = gatewayFactory.Adyen(paymentConfig).Authorize(reservation.Number, totalValue,
                                                           auxMonth, auxYear, cardCVV, cardNumber, objReservationPaymentDetail.CardName, (short)installments, paymentGatewayInformation.CurrencyIsoCode,
                                                           reservation.GuestEmail,
                                                           reservation.IPAddress,
                                                           reservation.GuestFirstName,
                                                           reservation.GuestLastName,
                                                           reservation.GuestIDCardNumber,
                                                           reservation.Guest_UID.ToString(),
                                                           billingCountry != null ? billingCountry.CountryCode : null,
                                                           billingState != null ? billingState.asciiname : null,
                                                           reservation.BillingCity,
                                                           billingAddress,
                                                           reservation.BillingPostalCode,
                                                           objReservationPaymentDetail.ClientSideEncryptedCreditCard,
                                                           checkin.GetValueOrDefault(),
                                                           checkout.GetValueOrDefault()
                                                           );

                                return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true,
                                                        objReservationPaymentDetail);
                            }
                            else // Authorize && Capture
                            {
                                returnMessage = gatewayFactory.Adyen(paymentConfig).Authorize(reservation.Number, totalValue,
                                                           auxMonth, auxYear, cardCVV, cardNumber, objReservationPaymentDetail.CardName, (short)installments, paymentGatewayInformation.CurrencyIsoCode,
                                                           reservation.GuestEmail,
                                                           reservation.IPAddress,
                                                           reservation.GuestFirstName,
                                                           reservation.GuestLastName,
                                                           reservation.GuestIDCardNumber,
                                                           reservation.Guest_UID.ToString(),
                                                           billingCountry != null ? billingCountry.CountryCode : null,
                                                           billingState != null ? billingState.asciiname : null,
                                                           reservation.BillingCity,
                                                           billingAddress,
                                                           reservation.BillingPostalCode,
                                                           objReservationPaymentDetail.ClientSideEncryptedCreditCard,
                                                           checkin.GetValueOrDefault(),
                                                           checkout.GetValueOrDefault()
                                                           );

                                var authorize = RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true,
                                                        objReservationPaymentDetail);

                                if (!returnMessage.IsTransactionValid)
                                    return authorize;

                                returnMessage = gatewayFactory.Adyen(paymentConfig).Capture(returnMessage.PaymentGatewayAutoGeneratedUID, returnMessage.PaymentGatewayTransactionID, totalValue,
                                                                                    paymentGatewayInformation.CurrencyIsoCode);

                                return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true,
                                                        objReservationPaymentDetail);
                            }

                        #endregion ADYEN

                        #region BPAG

                        case (int)OB.Reservation.BL.Constants.PaymentGateway.BPag:

                            year = objReservationPaymentDetail.ExpirationDate.Value.Year;
                            month = objReservationPaymentDetail.ExpirationDate.Value.Month;

                            paymentConfig.IsTestEnvironment = WebConfigurationManager.AppSettings["IsAdyenInTestMode"] != null
                                ? bool.Parse(WebConfigurationManager.AppSettings["IsAdyenInTestMode"]) : false;

                            string BPagBellNotificationURL = (paymentGatewayInformation.IsAntiFraudeControlEnable && !string.IsNullOrWhiteSpace(WebConfigurationManager.AppSettings["BPagBellNotificationURL"]))
                                ? WebConfigurationManager.AppSettings["BPagBellNotificationURL"] : "";

                            var appSetting = appRepo.ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string> { "BPagEncryptionKey" } }).FirstOrDefault();
                            var guestCountry = propertyRepo.ListCountries(new Contracts.Requests.ListCountryRequest { UIDs = new List<long> { reservation.GuestCountry_UID ?? 0 } }).FirstOrDefault();
                            var property = propertyRepo.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest { UIDs = new List<long> { reservation.Property_UID } }).FirstOrDefault();
                            var firstRoom = reservation.ReservationRooms.FirstOrDefault();

                            var bPagRequest = new PaymentGatewaysLibrary.BPag.Classes.AuthorizeRequest
                            {
                                ClientOrderId = reservation.Number,
                                TotalAmount = totalValue,
                                CardExpirationMonth = month.ToString(),
                                CardExpirationYear = year.ToString(),
                                CardSecurityCode = securityRepo.EncryptString(appSetting.Value, cardCVV),
                                CardNumber = securityRepo.EncryptString(appSetting.Value, cardNumber),
                                CardType = (PaymentGatewaysLibrary.Common.Constants.CardType)objReservationPaymentDetail.PaymentMethod_UID,
                                Installments = installments,
                                CurrencyIso = paymentGatewayInformation.CurrencyIsoCode,
                                GuestEmail = reservation.GuestEmail,
                                CustomerId = reservation.Guest_UID.ToString(),
                                GuestFirstName = reservation.GuestFirstName,
                                GuestLastName = reservation.GuestLastName,
                                GuestCity = reservation.GuestCity,
                                GuestCountry = guestCountry.CountryCode,
                                GuestState = objguest.State,
                                GuestPhoneNumber = reservation.GuestPhone,
                                GuestPostalCode = reservation.GuestPostalCode,
                                GuestVATNumber = reservation.GuestIDCardNumber,
                                CardHolderFirstName = firstName,
                                CardHolderLastName = lastName,
                                IpAddress = reservation.IPAddress,
                                OrderItems = new List<PaymentGatewaysLibrary.BPag.Classes.OrderItem>(),
                                Address1 = reservation.GuestAddress1,
                                Foreign = property.Country_UID != guestCountry.UID,
                                NumberOfNights = firstRoom != null ? (firstRoom.DateTo.Value - firstRoom.DateFrom.Value).Days : 0,
                                GuestBirthDay = objguest.Birthday
                            };

                            bPagRequest.OrderItems.Add(new PaymentGatewaysLibrary.BPag.Classes.OrderItem
                            {
                                Code = reservation.Number,
                                Units = "1",
                                UnitValue = (totalValue * 100).ToString("0.##"), // multiplicado por 100 para retirar a virgula
                                Description = reservation.Number
                            });

                            // Authorize Only
                            if ((reservation.IsOnRequest.HasValue && reservation.IsOnRequest.Value)
                                || (paymentGatewayInformation.PaymentAuthorizationTypeId != (int)Constants.PaymentGatewayAuthorizationTypes.AuthorizationAndCapture))
                            {
                                returnMessage = gatewayFactory.BPag(paymentConfig).Authorize(bPagRequest, BPagBellNotificationURL);

                                // Change Reservation If payment "Em Análise" -> Booking OnRequest
                                int result;
                                if (returnMessage.IsTransactionValid && (int.TryParse(returnMessage.PaymentGatewayTransactionStatusCode, out result) && result == (int)PaymentGatewaysLibrary.BPagGateway.Constants.PaymentResponseCodes.EmAnalise))
                                {
                                    reservation.Status = (int)Constants.ReservationStatus.BookingOnRequest;
                                    reservation.IsOnRequest = true;
                                }

                                return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true,
                                                        objReservationPaymentDetail);
                            }
                            else // Authorize && Capture
                            {
                                returnMessage = gatewayFactory.BPag(paymentConfig).Authorize(bPagRequest, BPagBellNotificationURL);

                                // Change Reservation If payment "Em Análise" -> Booking OnRequest
                                int result = -1;
                                if (returnMessage.IsTransactionValid && (paymentGatewayInformation.IsAntiFraudeControlEnable && int.TryParse(returnMessage.PaymentGatewayTransactionStatusCode, out result) && result == (int)PaymentGatewaysLibrary.BPagGateway.Constants.PaymentResponseCodes.EmAnalise))
                                {
                                    reservation.Status = (int)Constants.ReservationStatus.BookingOnRequest;
                                    reservation.IsOnRequest = true;
                                }
                                else
                                {
                                    if (returnMessage.IsTransactionValid)
                                    {
                                        // DO CAPTURE
                                        returnMessage = gatewayFactory.BPag(paymentConfig).Capture(returnMessage.PaymentGatewayAutoGeneratedUID, returnMessage.PaymentGatewayTransactionID,
                                            returnMessage.PaymentGatewayOrderID, totalValue);
                                    }
                                }

                                return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true,
                                                                objReservationPaymentDetail);
                            }
                        #endregion BPAG

                        #region PAYU

                        case (int)OB.Reservation.BL.Constants.PaymentGateway.PayU:
                            year = objReservationPaymentDetail.ExpirationDate.Value.Year;
                            month = objReservationPaymentDetail.ExpirationDate.Value.Month;

                            paymentConfig.IsTestEnvironment = WebConfigurationManager.AppSettings["IsPayUInTestMode"] != null && bool.Parse(WebConfigurationManager.AppSettings["IsPayUInTestMode"]);

                            var gatewayServicePayU = gatewayFactory.PayUColombia(paymentConfig);

                            var chargeRequestPayU = new PaymentGatewaysLibrary.PayU.ChargeRequest();
                            FillChargeRequestPayU(chargeRequestPayU, installments, paymentGatewayInformation, month, year, cardNumber, cardCVV, totalValue);

                            returnMessage = gatewayServicePayU.Charge(chargeRequestPayU);

                            if (returnMessage.IsOnReview)
                            {
                                reservation.Status = (int)Constants.ReservationStatus.BookingOnRequest;
                                reservation.IsOnRequest = true;
                            }

                            return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, totalValue, reservation, true, objReservationPaymentDetail, requestId, returnMessage.Errors);


                            #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                int result;
                switch (int.Parse(paymentGatewayInformation.GatewayCode))
                {
                    case (int)OB.Reservation.BL.Constants.PaymentGateway.MaxiPago:
                        result = -1;
                        break;

                    case (int)OB.Reservation.BL.Constants.PaymentGateway.BrasPag:
                        result = -900;
                        break;

                    case (int)OB.Reservation.BL.Constants.PaymentGateway.PayU:
                        result = -1000;
                        break;

                    default:
                        result = -1;
                        break;
                }

                //tokenization
                if (objReservationPaymentDetail != null)
                {
                    objReservationPaymentDetail.CardNumber = null;
                    objReservationPaymentDetail.ExpirationDate = null;
                    objReservationPaymentDetail.CreditCardToken = null;
                    objReservationPaymentDetail.HashCode = null;
                }

                var arguments = new Dictionary<string, object>();
                arguments.Add("objguest", objguest);
                arguments.Add("objReservation", DomainToBusinessObjectTypeConverter.Convert(reservation));
                arguments.Add("objReservationPaymentDetail", objReservationPaymentDetail);
                arguments.Add("GatewayName", paymentGatewayInformation.GatewayName);
                arguments.Add("ExceptionMessage", ex.Message);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.Critical, arguments);

                if (ex is PaymentGatewayException)
                    Logger.Info("MakePaymentWithPaymentGateway: " + ex.Message);
                else
                    Logger.Fatal(ex, "MakePaymentWithPaymentGateway");

                // Pass the Exception to Insert Reservation
                var exception = new PaymentGatewayException(string.IsNullOrEmpty(returnMessage?.ErrorMessage) ? ex.Message : returnMessage?.ErrorMessage, Contracts.Responses.ErrorType.PaymentGatewayError, result, PaymentGatewaysToContractsConverter.MapPaymentGatewayErrorsToReservationErrors(paymentGatewayInformation.GatewayCode, returnMessage?.Errors));
                throw exception;
            }

            #region Aux method for filling authorize Braspag

            void FillAuthorizeRequest(AuthorizeRequest authorizeRequest, int installments,
                PaymentGatewayConfiguration paymentGatewayConfiguration, int month, int year, string s, string cardCvv,
                decimal totalValue, PaymentGatewaysLibrary.BrasPagGateway.ModelsV2.Payments.Enums.InterestTypeEnum paymentPlan, DateTime? guestCreatedDate)
            {
                authorizeRequest.OrderId = reservation.Number;
                authorizeRequest.TotalValue = totalValue;
                authorizeRequest.CreditCardPayment = new CreditCardPaymentRequest();
                authorizeRequest.CreditCardPayment.CurrencyIsoCode = string.IsNullOrEmpty(paymentGatewayConfiguration.CurrencyIsoCode) ? string.Empty : paymentGatewayConfiguration.CurrencyIsoCode;
                authorizeRequest.CreditCardPayment.Installments = (short)installments;
                authorizeRequest.CreditCardPayment.CountryIsoCode = string.IsNullOrEmpty(paymentGatewayConfiguration.CountryIsoCode) ? string.Empty : paymentGatewayConfiguration.CountryIsoCode;
                authorizeRequest.CreditCardPayment.Provider = (PaymentGatewaysLibrary.BrasPagGateway.Constants.ProviderRequestEnum)paymentConfig.ProcessorCode;
                authorizeRequest.CreditCardPayment.Amount = totalValue;
                authorizeRequest.CreditCardPayment.Card = new CardRequest();
                authorizeRequest.CreditCardPayment.Card.CardMonth = month;
                authorizeRequest.CreditCardPayment.Card.CardYear = year;
                authorizeRequest.CreditCardPayment.Card.CardNumber = string.IsNullOrEmpty(s) ? string.Empty : s;
                authorizeRequest.CreditCardPayment.Card.CardSecurityCode = string.IsNullOrEmpty(cardCvv) ? string.Empty : cardCvv;
                authorizeRequest.CreditCardPayment.Card.CardHolderName = objReservationPaymentDetail?.CardName;
                authorizeRequest.CreditCardPayment.Card.CardRequestEnum = (PaymentGatewaysLibrary.Common.Constants.CardType)objReservationPaymentDetail.PaymentMethod_UID;
                authorizeRequest.CreditCardPayment.Interest = paymentPlan;
                authorizeRequest.CustomerRequest = new CustomerRequest();
                authorizeRequest.CustomerRequest.Email = reservation?.GuestEmail;
                authorizeRequest.CustomerRequest.Name = reservation?.GuestFirstName + " " + reservation?.GuestLastName;
                authorizeRequest.CustomerRequest.Phone = reservation.GuestPhone;
                authorizeRequest.CustomerRequest.AddressCity = reservation.GuestCity;
                authorizeRequest.CustomerRequest.AddressZipCode = reservation.GuestPostalCode;
                authorizeRequest.CustomerRequest.Identity = reservation.GuestIDCardNumber;
                authorizeRequest.CustomerRequest.AddressStreet = reservation.GuestAddress1;
                authorizeRequest.CustomerRequest.AddressComplement = reservation.GuestAddress2;

                if (reservation.GuestCountry_UID.HasValue)
                {
                    var guestCountry = propertyRepo.ListCountries(new ListCountryRequest { UIDs = new List<long> { reservation.GuestCountry_UID.Value } }).FirstOrDefault();
                    if (guestCountry != null)
                        authorizeRequest.CustomerRequest.AddressCountry = guestCountry.CountryCode;
                }

                if (reservation.GuestState_UID.HasValue)
                {
                    if (OB.DL.Common.Configuration.EnableFeatureStates128077)
                    {
                        string stateCode = string.Empty;
                        var listStates = propertyRepo.ListStates(
                                new ListStatesRequest
                                {
                                    Country_UID = (long)reservation.GuestCountry_UID
                                }
                        );

                        if (listStates != null)
                        {
                            stateCode = listStates.FirstOrDefault(f => f.UID == reservation.GuestState_UID)?.IsoCode;
                        }

                        if (!string.IsNullOrEmpty(stateCode))
                        {
                            authorizeRequest.CustomerRequest.AddressState = stateCode;
                        }
                        else
                        {
                            authorizeRequest.CustomerRequest.AddressState = authorizeRequest.CustomerRequest.AddressCountry;
                        }
                    }
                    else
                    {
                        string stateCode = string.Empty;
                        if (reservation.GuestCountry_UID.HasValue && reservation.GuestCountry_UID == UnitedStatesCountryId)
                        {
                            stateCode = propertyRepo.ListStates(new ListStatesRequest { Country_UID = (long)reservation.GuestCountry_UID }).FirstOrDefault(f => f.UID == reservation.GuestState_UID)?.Admin1Code;
                        }
                        else if (reservation.GuestCountry_UID.HasValue && reservation.GuestCountry_UID == BrasilCountryId)
                        {
                            stateCode = ((Constants.BrasilStateCodes)reservation.GuestState_UID.Value).ToString();
                        }

                        if (!string.IsNullOrEmpty(stateCode) && stateCode.Length == 2)
                        {
                            authorizeRequest.CustomerRequest.AddressState = stateCode;
                        }
                        else
                        {
                            authorizeRequest.CustomerRequest.AddressState = authorizeRequest.CustomerRequest.AddressCountry;
                        }
                    }
                }

                if (paymentGatewayConfiguration.IsAntiFraudeControlEnable)
                {

                    #region Get promotional code or group code
                    string promoCode = string.Empty;
                    if (reservation.PromotionalCode_UID.HasValue)
                    {
                        if (promotionalCode != null)
                            promoCode = promotionalCode.Code;
                        else
                        {
                            var promotionalCodeRepo = this.RepositoryFactory.GetOBPromotionalCodeRepository();
                            var promoResult = promotionalCodeRepo.ListPromotionalCode(new ListPromotionalCodeRequest { PromoCodeIds = new List<long> { reservation.PromotionalCode_UID.Value } }).FirstOrDefault();
                            if (promoResult != null)
                                promoCode = promoResult.Code;
                        }
                    }
                    else if (reservation.GroupCode_UID.HasValue)
                    {
                        var rateRepo = this.RepositoryFactory.GetOBRateRepository();
                        var groupCode = rateRepo.ListGroupCodesForReservation(new ListGroupCodesForReservationRequest { GroupCodesIds = new List<long> { reservation.GroupCode_UID.Value } }).FirstOrDefault();
                        if (groupCode != null)
                            promoCode = groupCode.Code?.ToString();
                    }
                    #endregion

                    #region Get stars from Hotel
                    var propertyFields = new HashSet<contractsProperties.ListPropertyFields> { contractsProperties.ListPropertyFields.Id, contractsProperties.ListPropertyFields.PropertySettings_Name, contractsProperties.ListPropertyFields.PropertySettings_Stars };
                    var property = propertyRepo.ListProperties(new Contracts.Requests.ListPropertiesRequest { Ids = new List<long> { reservation.Property_UID }, Fields = propertyFields }).FirstOrDefault();
                    #endregion Get stars from Hotel

                    authorizeRequest.AntiFraudRequest = new AntiFraudRequest { TotalOrderAmount = totalValue, OrderDate = DateTime.UtcNow };
                    authorizeRequest.AntiFraudRequest.Browser = new BrowserRequest();
                    authorizeRequest.AntiFraudRequest.Browser.Email = reservation?.GuestEmail;
                    authorizeRequest.AntiFraudRequest.Browser.IpAddress = reservation.IPAddress;
                    authorizeRequest.DeviceFingerPrintId = antifraudDeviceFingerPrintId;

                    var sku = authorizeRequest.OrderId.Replace("RES", ""); //martelo pedido pela braspag
                    authorizeRequest.AntiFraudRequest.Cart = new CartRequest
                    {
                        Items = new List<ItemRequest> {
                            new ItemRequest {
                                    Name = "Reserva",
                                    Sku = sku,
                                    Quantity = 1,
                                    UnitPrice = totalValue
                            }
                        }
                    };
                    var passengers = new List<PassengerRequest>();

                    var passenger = new PassengerRequest
                    {
                        Name = reservation?.GuestFirstName + " " + reservation?.GuestLastName,
                        Email = reservation?.GuestEmail,
                        Identity = reservation?.GuestIDCardNumber,
                        Phone = reservation?.GuestPhone
                    };
                    passengers.Add(passenger);

                    authorizeRequest.AntiFraudRequest.Travel = new TravelRequest
                    {
                        JourneyType = "RoundTrip",
                        DepartureTime = reservation?.ReservationRooms?.Min(x => x.DateFrom).Value,
                        Passengers = passengers
                    };

                    #region MDDs

                    authorizeRequest.AntiFraudRequest.MerchantDefinedFields = new List<MerchantDefinedFieldRequest>();

                    //Installments from request
                    authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                    {
                        Id = "3",
                        Value = installments.ToString()
                    });

                    //Sales Channel 
                    authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                    {
                        Id = "4",
                        Value = "Web"
                    });

                    //Promotional code 
                    if (!String.IsNullOrEmpty(promoCode))
                    {
                        authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                        {
                            Id = "5",
                            Value = promoCode
                        });
                    }

                    //Stars from hotel
                    if (property?.PropertySettings?.Stars.HasValue == true)
                    {
                        authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                        {
                            Id = "11",
                            Value = property.PropertySettings.Stars.Value.ToString()
                        });
                    }

                    //Checkin e Checkout
                    if (reservation?.ReservationRooms?.Any() == true)
                    {
                        authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                        {
                            Id = "12",
                            Value = GetMDD12Value(reservation).ToString()
                        });

                        authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                        {
                            Id = "13",
                            Value = GetMDD13Value(reservation).ToString()
                        });
                    }

                    //Bin (6 first digits) from card
                    if (!string.IsNullOrEmpty(s))
                    {
                        authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                        {
                            Id = "26",
                            Value = s.Substring(0, 6)
                        });
                    }

                    //Card Holder Name
                    if (!string.IsNullOrEmpty(objReservationPaymentDetail?.CardName))
                    {
                        authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                        {
                            Id = "46",
                            Value = objReservationPaymentDetail?.CardName
                        });
                    }

                    //Country Guest
                    authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                    {
                        Id = "47",
                        Value = authorizeRequest.CustomerRequest?.AddressCountry
                    });

                    //Checkin e Checkout
                    if (reservation?.ReservationRooms?.Any() == true)
                    {
                        authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                        {
                            Id = "85",
                            Value = reservation.ReservationRooms.Min(x => x.DateFrom).Value.ToString("yyyy-MM-dd")
                        });

                        authorizeRequest.AntiFraudRequest.MerchantDefinedFields.Add(new MerchantDefinedFieldRequest
                        {
                            Id = "86",
                            Value = reservation.ReservationRooms.Max(x => x.DateTo).Value.ToString("yyyy-MM-dd")
                        });
                    }

                    #endregion MDDs
                }
            }
            #endregion

            #region Aux method for filling charge PayU

            void FillChargeRequestPayU(PaymentGatewaysLibrary.PayU.ChargeRequest chargeRequest, int installments, PaymentGatewayConfiguration paymentGatewayConfiguration,
                int month, int year, string s, string cardCvv, decimal totalValue)
            {

                //Payment
                chargeRequest.CreatePayment = new PaymentGatewaysLibrary.PayU.CreatePaymentRequest();
                chargeRequest.CreatePayment.Amount = totalValue;
                chargeRequest.CreatePayment.Currency = paymentGatewayConfiguration?.CurrencyIsoCode;
                chargeRequest.CreatePayment.StatementSoftDescriptor = $"Reservation number: {reservation?.Number}";
                chargeRequest.CreatePayment.RequestId = requestId;

                //Payment -> Order
                chargeRequest.CreatePayment.Order = new PaymentGatewaysLibrary.PayU.Order();
                chargeRequest.CreatePayment.Order.Id = reservation?.Number;
                //Payment -> BillingAddress
                chargeRequest.CreatePayment.BillingAddress = new PaymentGatewaysLibrary.PayU.BillingAddress();
                chargeRequest.CreatePayment.BillingAddress.City = reservation?.BillingCity ?? reservation?.GuestCity;
                if (reservation.BillingCountry_UID.HasValue || reservation.GuestCountry_UID.HasValue)
                {
                    var countryId = reservation.BillingCountry_UID ?? reservation.GuestCountry_UID;
                    var guestCountry = propertyRepo.ListCountries(new ListCountryRequest { UIDs = new List<long> { countryId.Value } }).FirstOrDefault();
                    chargeRequest.CreatePayment.BillingAddress.Country = guestCountry?.CountryIsoCode;
                }
                chargeRequest.CreatePayment.BillingAddress.Email = reservation?.BillingEmail ?? reservation?.GuestEmail;
                chargeRequest.CreatePayment.BillingAddress.FirstName = reservation?.BillingContactName ?? reservation?.GuestFirstName;
                chargeRequest.CreatePayment.BillingAddress.LastName = reservation?.BillingContactName ?? reservation?.GuestLastName;
                chargeRequest.CreatePayment.BillingAddress.Line1 = reservation?.BillingAddress1 ?? reservation?.GuestAddress1;
                chargeRequest.CreatePayment.BillingAddress.Line2 = reservation?.BillingAddress2 ?? reservation?.GuestAddress2;
                chargeRequest.CreatePayment.BillingAddress.Phone = reservation?.BillingPhone ?? reservation?.GuestPhone;
                chargeRequest.CreatePayment.BillingAddress.ZipCode = reservation?.BillingPostalCode ?? reservation?.GuestPostalCode;
                if (reservation.BillingState_UID.HasValue || reservation.GuestState_UID.HasValue)
                {
                    var stateId = reservation.BillingState_UID ?? reservation.GuestState_UID;
                    var billingState = propertyRepo.ListCitiesDataTranslated(new Contracts.Requests.ListCityDataRequest { UIDs = new List<long> { stateId.Value } }).FirstOrDefault();
                    chargeRequest.CreatePayment.BillingAddress.State = billingState?.name;
                }
                //payment -> customer
                chargeRequest.CreatePayment.CreateCustomer = new PaymentGatewaysLibrary.PayU.CreateCustomerRequest
                {
                    CustomerReference = Guid.NewGuid().ToString(),
                    FirstName = reservation?.GuestFirstName,
                    LastName = reservation?.GuestLastName,
                    Email = reservation?.GuestEmail,
                    RequestId = requestId
                };

                //charge
                chargeRequest.ProviderSpecificData = new Dictionary<string, PaymentGatewaysLibrary.PayU.ProviderSpecificDataRequest>
                {
                    {
                        "payu_latam", new PaymentGatewaysLibrary.PayU.ProviderSpecificDataRequest
                        {
                            AdditionalDetails = new PaymentGatewaysLibrary.PayU.AdditionalDetailsRequest { Cookie = cookie, CustomerNationalIdentifyNumber = reservation?.GuestIDCardNumber, PayerEmail = reservation?.GuestEmail },
                            DeviceFingerPrint = new PaymentGatewaysLibrary.PayU.DeviceFingerPrintRequest { Fingerprint = antifraudDeviceFingerPrintId, Provider= "PayULatam"}
                        }
                    }
                };
                chargeRequest.Installments = new PaymentGatewaysLibrary.PayU.InstallmentsRequest
                {
                    NumberOfInstallments = installments > 0 ? installments : 1
                };
                chargeRequest.PaymentMethod = new PaymentGatewaysLibrary.PayU.PaymentMethodRequest
                {
                    Type = PaymentGatewaysLibrary.PayU.Enum.PaymentMethodTypeEnum.PaymentMethodType.Untokenized,
                    SourceType = PaymentGatewaysLibrary.PayU.Enum.PaymentMethodSourceTypeEnum.PaymentMethodSourceType.CreditCard,
                    HolderName = objReservationPaymentDetail?.CardName,
                    CardNumber = string.IsNullOrEmpty(s) ? string.Empty : s,
                    ExpirationDate = $"{month.ToString("00")}/{year}",
                    CreditCardCvv = string.IsNullOrEmpty(cardCvv) ? string.Empty : cardCvv,
                    CardIdentity = new PaymentGatewaysLibrary.PayU.CardIdentity
                    {
                        Number = reservation?.GuestIDCardNumber
                    }
                };
                chargeRequest.RequestId = chargeRequest.ReconciliationId = requestId;
                chargeRequest.IpAddress = reservation?.IPAddress;
            }

            #endregion

            return 0;
        }

        public int GetMDD12Value(domainReservations.Reservation reservation)
        {
            var checkInDate = reservation.ReservationRooms.Min(x => x.DateFrom).Value.Date;
            var transactionDate = reservation.CreatedDate.Value.Date;

            TimeSpan date = checkInDate - transactionDate;

            return date.Days > 0 ? date.Days : 0;
        }

        public int GetMDD13Value(domainReservations.Reservation reservation)
        {
            var checkInDate = reservation.ReservationRooms.Min(x => x.DateFrom).Value.Date;
            var checkOutDate = reservation.ReservationRooms.Max(x => x.DateTo).Value.Date;

            TimeSpan date = checkOutDate - checkInDate;
            return date.Days > 0 ? date.Days : 0;
        }

        #region Aux method for filling CaptureRequest and VoidRequest

        private static void FillCaptureRequest(domainReservations.Reservation reservation, CaptureRequest captureRequest, decimal totalValue, PaymentMessageResult returnMessage)
        {
            captureRequest.Amount = totalValue;
            captureRequest.PaymentId = Guid.Parse(returnMessage.PaymentGatewayTransactionID);
            captureRequest.ReservationNr = reservation.Number;
        }

        private static void FillVoidRequest(domainReservations.Reservation reservation, decimal refundAmount, VoidRequest voidRequest)
        {
            voidRequest.Amount = refundAmount;
            voidRequest.PaymentId = Guid.Parse(reservation.PaymentGatewayTransactionID);
            voidRequest.ReservationNr = reservation.Number;
        }

        #endregion

        /// <summary>
        /// Refund Money from payment gateway
        /// </summary>
        /// <param name="handleWithGatewayException">This parameter allow to handle with PaymentGatewayException to return the specific paymentgateway errors (only cancelreservation use this parameter now)</param>
        /// <returns></returns>
        private int RefundPaymentGateway(domainReservations.Reservation reservation, decimal refundAmount, bool skipInterestCalculation, int NoCancelRooms = 0, string requestId = null, bool handleWithGatewayException = false)
        {
            //convert to business
            var reservationContract = DomainToBusinessObjectTypeConverter.Convert(reservation);

            var resultCode = 1;
            var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();
            PaymentMessageResult returnMessage = new PaymentMessageResult();
            PaymentGatewayConfig paymentConfig = new PaymentGatewayConfig();

            refundAmount = Math.Round(refundAmount, 2, MidpointRounding.AwayFromZero);

            try
            {
                // Get Payment Gateway Information
                var paymentGatewayInformation = propertyRepo.GetActivePaymentGatewayConfigurationReduced(new Contracts.Requests.ListActivePaymentGatewayConfigurationRequest { PropertyId = reservation.Property_UID, CurrencyId = reservation.ReservationBaseCurrency_UID.Value });

                // Map to gateway configuration
                paymentConfig = GetPaymentGatewayConfiguration(paymentGatewayInformation);

                // PAYPAL
                if (reservation.PaymentGatewayName == Constants.PaymentGateway.Paypal.ToString())
                {
                    #region PAYPAL

                    if (reservation.PaymentMethodType_UID == (int)Constants.PaymentMethodTypesCode.Paypal && !string.IsNullOrEmpty(reservation.PaymentGatewayTransactionID))
                    {
                        var result = false;
                        IPaypalGatewayManagerPOCO paypalManagerPOCO = this.Resolve<IPaypalGatewayManagerPOCO>();
                        paypalManagerPOCO.Initialize(reservation.Property_UID);

                        // Check if is first refund, if not allways do Partial refund
                        var transactionId = reservation.PaymentGatewayTransactionUID.HasValue ? reservation.PaymentGatewayTransactionUID.Value : 0;

                        var IsFirstRefund = false;
                        using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                        {
                            // Get Payment Gateway transaction details repo
                            var paymentGatewayTransactionDetailsRepo = this.RepositoryFactory.GetRepository<OB.Domain.Payments.PaymentGatewayTransactionsDetail>(unitOfWork);
                            IsFirstRefund = !paymentGatewayTransactionDetailsRepo.Any(x => x.PaymentGatewayTransactionId == transactionId && x.RequestType == "RefundTransaction");
                        }

                        // Alter Subscriptions and do Refund
                        if (reservation.IsPartialPayment.HasValue && reservation.IsPartialPayment.Value)
                        {
                            var action = string.Empty;
                            var resultRecurrent = false;
                            if (reservation.Status == (int)Constants.ReservationStatus.Modified)
                                resultRecurrent = paypalManagerPOCO.CancelAndRefundRecurringBilling((long)reservation.PaymentGatewayTransactionUID, reservation.PaymentGatewayTransactionID,
                                    true, reservationContract, action, "");
                            else if (reservation.Status == (int)Constants.ReservationStatus.Cancelled && !IsFirstRefund)
                            {
                                action = "CANCEL";
                                resultRecurrent = paypalManagerPOCO.CancelAndRefundRecurringBilling((long)reservation.PaymentGatewayTransactionUID, reservation.PaymentGatewayTransactionID,
                                    true, reservationContract, action, "");
                            }
                            else if (reservation.Status == (int)OB.Reservation.BL.Constants.ReservationStatus.Cancelled && IsFirstRefund)
                            {
                                action = "CANCEL";
                                resultRecurrent = paypalManagerPOCO.CancelAndRefundRecurringBilling((long)reservation.PaymentGatewayTransactionUID, reservation.PaymentGatewayTransactionID,
                                    false, reservationContract, action, "");
                            }

                            // Cancelation was not possible
                            if (!resultRecurrent && !string.IsNullOrEmpty(action))
                                resultCode = -300;
                        }
                        // Do Refund
                        else
                        {
                            if (reservation.Status == (int)Constants.ReservationStatus.Modified)
                                result = paypalManagerPOCO.RefundPayment(reservationContract, OB.Reservation.BL.Constants.PaypalRefundType.PARTIAL.ToString(),
                                                refundAmount.ToString("0.00", CultureInfo.InvariantCulture));
                            else if (reservation.Status == (int)Constants.ReservationStatus.Cancelled && !IsFirstRefund)
                                result = paypalManagerPOCO.RefundPayment(reservationContract,
                                                OB.Reservation.BL.Constants.PaypalRefundType.PARTIAL.ToString(),
                                                refundAmount.ToString("0.00", CultureInfo.InvariantCulture));

                            // Refund Total Value (Reservation Canceled)
                            else if (reservation.Status == (int)Constants.ReservationStatus.Cancelled && IsFirstRefund)
                                result = paypalManagerPOCO.RefundPayment(reservationContract,
                                                OB.Reservation.BL.Constants.PaypalRefundType.FULL.ToString(),
                                                refundAmount.ToString("0.00", CultureInfo.InvariantCulture));

                            // Refund was not possible
                            if (!result)
                                resultCode = -300;
                        }
                    }

                    #endregion PAYPAL
                }

                if (!skipInterestCalculation)
                {
                    var domainPaymentDetails = reservation?.ReservationPaymentDetails?.FirstOrDefault();
                    refundAmount = GetValueWithInterestRate(refundAmount, reservation,
                        domainPaymentDetails != null ? DomainToBusinessObjectTypeConverter.Convert(domainPaymentDetails) : null,
                        true);
                }

                var gatewayFactory = Resolve<IPaymentGatewayFactory>();
                // MAXIPAGO
                if (reservation.PaymentGatewayName == Constants.PaymentGateway.MaxiPago.ToString())
                {
                    #region MAXIPAGO

                    if (paymentConfig != null)
                    {
                        if (reservation.IsOnRequest.HasValue && !reservation.IsOnRequest.Value)
                        {
                            var projectGeneral = this.Resolve<IProjectGeneral>();
                            // if are not all rooms..and is not the same day(days after)..transactions will be return
                            // if is same day and only some rooms are cancelled..transaction will not occurr should cancel rooms only day after..
                            if (reservation.ReservationRooms.Count() != NoCancelRooms)
                            {
                                // if are not all rooms and is same day..it will not be return any money..only day after is possible
                                if (projectGeneral.IsTheSameDay(reservation.Date.ToUniversalTime(), DateTime.UtcNow))
                                {
                                    // rooms could not be return same day wait till 24:00
                                    return -100;
                                }
                            }

                            if (projectGeneral.IsTheSameDay(reservation.Date.ToUniversalTime(), DateTime.UtcNow))
                            {
                                returnMessage = gatewayFactory.MaxiPago(paymentConfig).Void(reservation.PaymentGatewayTransactionID, reservation.PaymentGatewayAutoGeneratedUID);
                                return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, refundAmount, reservation, true);
                            }
                            else
                            {
                                returnMessage = gatewayFactory.MaxiPago(paymentConfig).Refund(reservation.PaymentGatewayOrderID, reservation.PaymentGatewayAutoGeneratedUID, refundAmount);
                                return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, refundAmount, reservation, true);
                            }
                        }
                    }

                    #endregion MAXIPAGO
                }
                // BrasPag
                else if (reservation.PaymentGatewayName == Constants.PaymentGateway.BrasPag.ToString())
                {
                    #region BRASPAG

                    if (paymentConfig != null)
                    {
                        // Can't cancel rooms on the same day of reservation, its only possible to cancel the reservation
                        if (reservation.ReservationRooms.Count != NoCancelRooms && this.Resolve<IProjectGeneral>().IsTheSameDay(reservation.Date.ToUniversalTime(), DateTime.UtcNow))
                            return -100;

                        //Fill Void request
                        var voidRequest = new VoidRequest();

                        FillVoidRequest(reservation, refundAmount, voidRequest);

                        var gatewayServiceForVoid = gatewayFactory.BrasPag(paymentConfig);

                        returnMessage = gatewayServiceForVoid.Void(voidRequest);

                        return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, refundAmount, reservation, true);

                    }

                    #endregion BRASPAG
                }
                // Ayden
                else if (reservation.PaymentGatewayName == Constants.PaymentGateway.Adyen.ToString())
                {
                    #region ADYEN

                    if (paymentConfig != null)
                    {
                        paymentConfig.IsTestEnvironment = WebConfigurationManager.AppSettings["IsAdyenInTestMode"] != null
                                ? bool.Parse(WebConfigurationManager.AppSettings["IsAdyenInTestMode"]) : false;

                        // Full Reservation Cancelation
                        if (reservation.Status == (int)Constants.ReservationStatus.Cancelled)
                        {
                            var isAlreadyRefunded = false;
                            using (var unitOfWork = SessionFactory.GetUnitOfWork())
                            {
                                var paymentGatewayTransactionRepo = this.RepositoryFactory.GetPaymentGatewayTransactionRepository(unitOfWork);
                                isAlreadyRefunded = paymentGatewayTransactionRepo.Any(x => x.OrderType == "Refund" && x.PaymentGatewayAutoGeneratedUID == reservation.PaymentGatewayAutoGeneratedUID);
                            }

                            // Check if there is already a refund
                            if (isAlreadyRefunded)
                                returnMessage = gatewayFactory.Adyen(paymentConfig).Refund(reservation.PaymentGatewayAutoGeneratedUID, reservation.PaymentGatewayTransactionID,
                                                                                                        refundAmount, paymentGatewayInformation.CurrencyIsoCode);
                            else
                                returnMessage = gatewayFactory.Adyen(paymentConfig).VoidOrRefund(reservation.PaymentGatewayAutoGeneratedUID,
                                                            reservation.PaymentGatewayTransactionID, refundAmount);

                            return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, refundAmount, reservation, true);
                        }
                        // Partial Reservation Cancelation
                        else
                        {
                            // TODO Check if is authorized or refund
                            // When client has authorize with capture delay its no possible to know the state of the transaction
                            // Adyen does not have a service to check transaction state to decide if a cancel or a refund is needed
                            // We always do a partial refund, if transaction is capture refund be accepted if is only captured nothings happens
                            returnMessage = gatewayFactory.Adyen(paymentConfig).Refund(reservation.PaymentGatewayAutoGeneratedUID, reservation.PaymentGatewayTransactionID,
                                                                                                        refundAmount, paymentGatewayInformation.CurrencyIsoCode);
                            return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, refundAmount, reservation, true);
                        }
                    }

                    #endregion ADYEN
                }
                // BPag
                else if (reservation.PaymentGatewayName == Constants.PaymentGateway.BPag.ToString())
                {
                    #region BPag

                    if (paymentConfig != null)
                    {
                        paymentConfig.IsTestEnvironment = WebConfigurationManager.AppSettings["IsAdyenInTestMode"] != null
                                ? bool.Parse(WebConfigurationManager.AppSettings["IsAdyenInTestMode"]) : false;

                        returnMessage = gatewayFactory.BPag(paymentConfig).VoidOrRefund(reservation.PaymentGatewayAutoGeneratedUID,
                                                    reservation.PaymentGatewayTransactionID, reservation.PaymentGatewayOrderID, refundAmount);

                        return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, refundAmount, reservation, true);
                    }

                    #endregion BPag
                }
                //PayU
                else if (reservation.PaymentGatewayName == Constants.PaymentGateway.PayU.ToString())
                {
                    #region PayU
                    if (paymentConfig != null)
                    {
                        paymentConfig.IsTestEnvironment = WebConfigurationManager.AppSettings["IsPayUInTestMode"] != null && bool.Parse(WebConfigurationManager.AppSettings["IsPayUInTestMode"]);

                        var gatewayServices = gatewayFactory.PayUColombia(paymentConfig);

                        var refundReasonDefault = "Reservation cancelled by the guest"; //cancel by BE
                        if (reservation?.CancelReservationReason_UID.HasValue == true)//cancel by omnibees backoffice
                        {
                            var response = ListCancelReservationReasons(new ListCancelReservationReasonRequest { LanguageUID = 1 });
                            if (response.Status == Status.Success && response.Result.Any())
                            {
                                var reason = response.Result.FirstOrDefault(x => x.UID == reservation?.CancelReservationReason_UID);
                                if (reason != null)
                                    refundReasonDefault = reason.Name;
                            }
                        }

                        var request = new PaymentGatewaysLibrary.PayU.RefundRequest
                        {
                            PaymentId = reservation.PaymentGatewayTransactionID,
                            RefundReason = refundReasonDefault,
                            ReconciliationId = requestId,
                            RequestId = requestId,
                            ReservationNumber = reservation?.Number
                        };

                        //when you don't send amount, the payu perform the refund with the full captured amount 
                        //only send amount when you cancel one room from reservation for example.
                        if (reservation.ReservationRooms.Count != NoCancelRooms)
                            request.Amount = refundAmount;

                        returnMessage = gatewayServices.Refund(request);

                        return RegisterPaymentGatewayTransaction(paymentGatewayInformation, returnMessage, refundAmount, reservation, true);
                    }
                    #endregion PayU
                }
            }
            catch (Exception ex)
            {
                int result;
                switch (int.Parse(paymentConfig.GatewayCode))
                {
                    case (int)Constants.PaymentGateway.MaxiPago:
                        result = -1;
                        break;

                    case (int)Constants.PaymentGateway.BrasPag:
                        result = -900;
                        break;

                    case (int)OB.Reservation.BL.Constants.PaymentGateway.PayU:
                        result = -1000;
                        break;

                    default:
                        result = -1;
                        break;
                }

                var arguments = new Dictionary<string, object>();
                arguments.Add("ReservationNumber", reservation.Number);
                arguments.Add("PaymentGatewayConfiguration", paymentConfig);
                arguments.Add("ReturnMessage", returnMessage);
                arguments.Add("RefundAmount", refundAmount);
                arguments.Add("ExceptionMessage", ex.Message);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.Critical, arguments);

                if (handleWithGatewayException)
                {
                    // Pass the Exception to cancel reservation
                    var exception = new PaymentGatewayException(string.IsNullOrEmpty(returnMessage?.ErrorMessage) ? ex.Message : returnMessage?.ErrorMessage, Contracts.Responses.ErrorType.PaymentGatewayError, result, PaymentGatewaysToContractsConverter.MapPaymentGatewayErrorsToReservationErrors(paymentConfig.GatewayCode, returnMessage?.Errors));
                    throw exception;
                }

                return result;
            }

            return resultCode;
        }

        /// <summary>
        /// Get payment gateway configuration
        /// </summary>
        /// <param name="paymentConfig">The payment configuration.</param>
        /// <returns></returns>
        private PaymentGatewayConfig GetPaymentGatewayConfiguration(OB.BL.Contracts.Data.Payments.PaymentGatewayConfiguration paymentConfig)
        {
            PaymentGatewayConfig paymentGatewayConfiguration = null;

            if (paymentConfig != null)
            {
                paymentGatewayConfiguration = new PaymentGatewayConfig();
                paymentGatewayConfiguration.GatewayCode = paymentConfig.GatewayCode;
                paymentGatewayConfiguration.Commission = paymentConfig.Comission ?? 0;
                paymentGatewayConfiguration.GatewayName = paymentConfig.GatewayName;
                paymentGatewayConfiguration.IsTestEnvironment = GetTransactionEnvironment(paymentConfig.ProcessorCode);
                paymentGatewayConfiguration.MerchantId = paymentConfig.MerchantID;
                paymentGatewayConfiguration.MerchantKey = paymentConfig.MerchantKey;
                paymentGatewayConfiguration.ProcessorCode = paymentConfig.ProcessorCode ?? 0;
                paymentGatewayConfiguration.ProcessorName = paymentConfig.ProcessorName;
                paymentGatewayConfiguration.PaymentMethodCode = paymentConfig.PaymentMethodCode;
                paymentGatewayConfiguration.ApiVersion = paymentConfig.ApiVersion;
                paymentGatewayConfiguration.MerchantAccount = paymentConfig.MerchantAccount;
                paymentGatewayConfiguration.ApiSignatureKey = paymentConfig.ApiSignatureKey;
            }

            return paymentGatewayConfiguration;
        }

        /// <summary>
        /// Log and thread payment gateway response
        /// </summary>
        /// <param name="paymentGatewayService"></param>
        /// <param name="paymentConfig"></param>
        /// <param name="returnMessage"></param>
        /// <param name="totalValue"></param>
        /// <param name="objReservation"></param>
        /// <param name="operationName">Authorize, Capture, Void, Refund</param>
        /// <param name="updateReservationInformation">If true reservation will be updated with new payment gateway transation values</param>
        /// <param name="objReservationPaymentDetail"></param>
        /// <returns></returns>
        /// <exception cref="PaymentGatewayException"></exception>
        private int RegisterPaymentGatewayTransaction(OB.BL.Contracts.Data.Payments.PaymentGatewayConfiguration paymentConfig, PaymentMessageResult returnMessage,
                                                        decimal totalValue, domainReservations.Reservation objReservation, bool updateReservationInformation,
                                                        contractsReservations.ReservationPaymentDetail objReservationPaymentDetail = null, string requestId = null,
                                                        List<PaymentGatewaysLibrary.Common.Error> gatewayErrors = null)
        {
            string guestInfo = string.Format("Name: {0} {1} - Email: {2}", objReservation.GuestFirstName, objReservation.GuestLastName, objReservation.GuestEmail);

            // Log Transation
            var svc = this.Resolve<IPaymentGatewayManagerPOCO>();
            svc.LogPaymentTransactionInformationCustom(paymentConfig, returnMessage, guestInfo);

            bool isTransactionValid = returnMessage.IsTransactionValid;

            if (!isTransactionValid)
            {
                // cannot return positive value
                //check responseCode @ http://www.maxipago.com/docs/maxiPago_API_Ultima.pdf
                string exceptionMessage;

                if (string.IsNullOrWhiteSpace(returnMessage.ErrorMessage) && gatewayErrors?.Any() == true)
                    exceptionMessage = "Errors on " + paymentConfig.GatewayName + " gateway request.";
                else
                    exceptionMessage = string.Format("{0} - {1}", returnMessage.PaymentGatewayTransactionStatusCode, returnMessage.ErrorMessage);

                throw new PaymentGatewayException(exceptionMessage, PaymentGatewaysToContractsConverter.MapPaymentGatewayErrorsToReservationErrors(paymentConfig.GatewayCode, gatewayErrors));
            }
            else
            {
                if (updateReservationInformation)
                {
                    objReservation.PaymentGatewayTransactionStatusCode = returnMessage.PaymentGatewayTransactionStatusCode;
                    objReservation.PaymentGatewayName = returnMessage.PaymentGatewayName;
                    objReservation.PaymentGatewayProcessorName = returnMessage.PaymentGatewayProcessorName;

                    if (PaymentGatewaysLibrary.Common.Constants.AUTHORIZATION_TRANSACTION == returnMessage.PaymentGatewayTransactionType
                        || PaymentGatewaysLibrary.Common.Constants.SALE_TRANSACTION == returnMessage.PaymentGatewayTransactionType)
                    {
                        objReservation.PaymentGatewayTransactionID = returnMessage.PaymentGatewayTransactionID;
                        objReservation.PaymentGatewayOrderID = returnMessage.PaymentGatewayOrderID;
                    }

                    if (int.Parse(paymentConfig.GatewayCode) == (int)Constants.PaymentGateway.PayU &&
                        (PaymentGatewaysLibrary.Common.Constants.REFUND_TRANSACTION == returnMessage.PaymentGatewayTransactionType
                        || PaymentGatewaysLibrary.Common.Constants.CHARGE_TRANSACTION == returnMessage.PaymentGatewayTransactionType))
                    {
                        objReservation.PaymentGatewayTransactionID = returnMessage.PaymentGatewayTransactionID;
                        objReservation.PaymentGatewayOrderID = returnMessage.PaymentGatewayOrderID;
                    }

                    if (PaymentGatewaysLibrary.Common.Constants.FRAUD_ANALYSIS == returnMessage.PaymentGatewayTransactionType)
                    {
                        objReservation.PaymentGatewayTransactionID = returnMessage.PaymentGatewayTransactionID;
                    }

                    if (PaymentGatewaysLibrary.Common.Constants.AUTHORIZATION_WITH_CAPTURE_TRANSACTION == returnMessage.PaymentGatewayTransactionType
                        || PaymentGatewaysLibrary.Common.Constants.CAPTURE_TRANSACTION == returnMessage.PaymentGatewayTransactionType)
                    {
                        if (string.IsNullOrWhiteSpace(objReservation.PaymentGatewayTransactionID))
                            objReservation.PaymentGatewayTransactionID = returnMessage.PaymentGatewayTransactionID;
                        if (string.IsNullOrWhiteSpace(objReservation.PaymentGatewayOrderID) || int.Parse(paymentConfig.GatewayCode) == (int)Constants.PaymentGateway.BrasPag)
                            objReservation.PaymentGatewayOrderID = returnMessage.PaymentGatewayOrderID;
                    }

                    objReservation.PaymentAmountCaptured = returnMessage.PaymentAmountCaptured;
                    objReservation.PaymentGatewayTransactionDateTime = returnMessage.PaymentGatewayTransactionDateTime;
                    objReservation.PaymentGatewayAutoGeneratedUID = returnMessage.PaymentGatewayAutoGeneratedUID;
                    objReservation.PaymentGatewayTransactionMessage = returnMessage.TransactionMessage;
                    objReservation.PaymentGatewayTransactionID = returnMessage.PaymentGatewayTransactionID;

                }
                return 1;
            }
        }



        /// <summary>
        /// Get transaction environment
        /// </summary>
        /// <param name="processorCode"></param>
        /// <returns>TEST - for test environment, LIVE for live environment</returns>
        private bool GetTransactionEnvironment(int? processorCode)
        {
            return (processorCode == 1);
        }



        /// <summary>
        /// RESTful implementation of the PaymentGatewayInfoAndModifyReservationOnRequestForTransactionID.
        /// This operation returns Info about the Reservation and ReservationTransactionStatus and Commits or Cancels the Reservation
        /// </summary>
        /// <param name="request">PaymentGatewayCommitOrCancelReservationOnRequestForTransactionIDRequest</param>
        /// <returns>PaymentGatewayCommitOrCancelReservationOnRequestForTransactionIDResponse</returns>
        public PaymentGatewayInfoForTransactionIDResponse PaymentGatewayInfoAndModifyReservationOnRequestForTransactionID(PaymentGatewayInfoAndModifyReservationOnRequestForTransactionIDRequest request)
        {
            PaymentGatewayInfoForTransactionIDResponse response = new PaymentGatewayInfoForTransactionIDResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");

                Contract.Requires(request != null && !string.IsNullOrWhiteSpace(request.TransactionID), "Request.TransactionID object instance is expected");

                Contract.Requires(request != null && !(request.IsCommitOnRequest && request.IsCancelOnRequest), "Booleans Request.IsCommitOnRequest and Request.IsCancelOnRequest are expected and only one should be True");

                if (string.IsNullOrWhiteSpace(request.TransactionID))
                    return response;

                DomainPayments.PaymentGatewayTransaction transaction = null;
                ReservationTransactionStatusBasicInfoForReservationUidQR1 resTransactionBasicInfo = null;
                ReservationBasicInfoForTransactionIdQR1 reservationBasicInfo = null;
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var reservationsRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);

                    // Get Transaction
                    var svc = this.RepositoryFactory.GetRepository<Domain.Payments.PaymentGatewayTransaction>(unitOfWork);
                    transaction = svc.GetQuery(x => x.TransactionID == request.TransactionID).OrderByDescending(x => x.UID).FirstOrDefault();

                    // Find ReservationBasicInfo for this transaction
                    reservationBasicInfo = reservationsRepo.GetReservationBasicInfoForPaymentGatewayForTransaction(transaction.Property, transaction.TransactionID);

                    if (reservationBasicInfo != null)
                    {
                        // Get ReservationTransactionBasicInfo
                        resTransactionBasicInfo = reservationsRepo.GetReservationTransactionStatusBasicInfoForReservationUID(reservationBasicInfo.UID);

                        if (resTransactionBasicInfo != null)
                        {
                            // Fill Infos In Response
                            response.Result = new contractsReservations.PaymentGatewayCommitOrCancelReservationOnRequestResult()
                            {
                                Reservation_UID = reservationBasicInfo.UID,
                                Channel_UID = reservationBasicInfo.Channel_UID,
                                ReservationCurrency_UID = reservationBasicInfo.ReservationCurrency_UID,
                                TransactionUID = resTransactionBasicInfo.TransactionUID,
                                HangfireID = resTransactionBasicInfo.HangfireID,
                                TransactionState = resTransactionBasicInfo.TransactionState
                            };
                        }
                    }
                }

                if (transaction != null && reservationBasicInfo != null && resTransactionBasicInfo != null)
                {
                    if (request.IsCommitOnRequest)
                    {
                        // Commit Reservation OnRequest
                        CommitTransaction(reservationBasicInfo.UID, reservationBasicInfo.Channel_UID, resTransactionBasicInfo.TransactionUID, OB.Reservation.BL.Constants.ReservationAction.Update,
                            OB.Reservation.BL.Constants.ReservationTransactionStatus.Commited, resTransactionBasicInfo.HangfireID, 65, resTransactionBasicInfo.TransactionState, requestId: request.RequestId, PaymentGatewayOrderId: transaction.PaymentGatewayOrderID);

                        response.Result.WasCommited = true;
                    }
                    else if (request.IsCancelOnRequest)
                    {
                        // Cancel Reservation OnRequest
                        CancelReservationRequest cancelResRQ = new CancelReservationRequest()
                        {
                            ChannelId = 1,
                            ReservationUID = reservationBasicInfo.UID,
                            OBUserType = 65,
                            RuleType = OB.Reservation.BL.Constants.RuleType.Omnibees,
                        };

                        if (request.AlreadyCancelledFromPaymentGateway)
                            cancelResRQ.AlreadyCancelledFromPaymentGateway = true;

                        var cancelResRS = CancelReservation(cancelResRQ);

                        if (cancelResRS.Status == Status.Success && cancelResRS.Result == 1)
                        {
                            //DeleteHangfireJob
                            if (resTransactionBasicInfo.HangfireID > 0)
                            {
                                var reservationHelper = this.Resolve<IReservationHelperPOCO>();
                                reservationHelper.DeleteHangfireJob(response.Result.HangfireID);
                            }

                            response.Result.WasCancelled = true;
                        }
                        else if (cancelResRS.Status == Status.Fail && cancelResRS.Result == 0 && cancelResRS.Errors != null && (cancelResRS.Errors.Count() == 1 && cancelResRS.Errors.First().ErrorType == Errors.ReservationIsAlreadyCancelledError.ToString()))
                        {
                            response.Result.WasCancelled = true;
                        }
                    }

                    response.Succeed();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                var error = new OB.Reservation.BL.Contracts.Responses.Error(ex);
                if (ex is BusinessLayerException)
                {
                    error.ErrorType = ((BusinessLayerException)ex).ErrorType;
                    error.ErrorCode = ((BusinessLayerException)ex).ErrorCode;
                }
                response.Errors.Add(error);
            }

            return response;
        }


        #endregion PAYMENT GATEWAY OPERATIONS

        public void CloseSales(contractsReservations.Reservation reservation)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("TracingMode") && ConfigurationManager.AppSettings["TracingMode"].ToUpper() == "DEBUG")
            {
                var arguments = new Dictionary<string, object>();
                arguments.Add("resid", reservation.UID);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Async close sales tracing"), LogSeverity.Log, arguments, null, "AsycnCloseSales Tracing");
            }
            //Don't need try catch, has one inside
            this.CloseSalesAsPerOccupancyAlerts(reservation);
        }

        private void NotifyConnectors(string tableName, long uid, long propertyUID, short operation, string JsonContent, bool checkAvail = false, string requestId = null)
        {
            var rateRepo = this.RepositoryFactory.GetOBRateRepository();
            if (uid > 0 && !checkAvail)
            {
                try
                {
                    rateRepo.ConnectorEventQueueInsert(new Contracts.Requests.ConnectorEventQueueInsertRequest { TableName = tableName, TableKey = uid, PropertyUID = propertyUID, Operation = operation, JsonContent = JsonContent });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, new Log.Messages.LogMessageBase
                    {
                        MethodName = nameof(NotifyConnectors),
                        Description = $"There were errors to notify connectors - ReservationId: {uid} | Operation: {operation}",
                        RequestId = requestId
                    });

                    var args = new Dictionary<string, object>();
                    args.Add("table", tableName);
                    args.Add("uid", uid);
                    args.Add("propertyUID", propertyUID);
                    args.Add("operation", operation);
                    args.Add("jsonContenxt", JsonContent);
                    this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, args);

                    //TMOREIRA: Do some logging in here!!!!!!!
                    throw;
                }
            }
        }

        private void InsOccupancyAlertsInPropertyQueue(long reservationId, long propertyId, long? channelId, bool validateIfExistsRooms)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("TracingMode") && ConfigurationManager.AppSettings["TracingMode"].ToUpper() == "DEBUG")
            {
                var arguments = new Dictionary<string, object>();
                arguments.Add("reservationUID", reservationId);
                arguments.Add("propertyUID", propertyId);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Insert Close Sales in Property Queue"),
                    LogSeverity.High, arguments, null, "Insert Close Sales in Property Queue");
            }

            if (reservationId <= 0)
                return;

            try
            {
                var unitOfWork = this.SessionFactory.GetUnitOfWork();
                var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                string strCode = ((long)Constants.SystemEventsCode.Availability).ToString();

                //Notify Alerts
                bool notify = !validateIfExistsRooms || reservationRepo.FindAnyRoomTypesBy_ReservationUID_And_SystemEventCode(reservationId, strCode);

                if (notify)
                    InsertPropertyQueue(propertyId, reservationId, channelId, (long)Constants.SystemEventsCode.Availability);
            }
            catch (Exception ex)
            {
                var arguments = new Dictionary<string, object>();
                arguments.Add("reservationUID", reservationId);
                arguments.Add("propertyUID", propertyId);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments);

                throw;
            }
        }

        private void CloseSalesAsPerOccupancyAlerts(contractsReservations.Reservation reservation)
        {
            var totalTimeStopWatch = new Stopwatch();
            totalTimeStopWatch.Start();

            try
            {
                string strCode = ((long)Constants.SystemEventsCode.Availability).ToString();

                var distinctRoomtypes = new List<contractsProactiveActions.PEOccupancyAlertCustom>();
                foreach (var room in reservation.ReservationRooms)
                {
                    foreach (var rrd in room.ReservationRoomDetails)
                    {
                        var tmp = new contractsProactiveActions.PEOccupancyAlertCustom()
                        {
                            RateRoomDetails_UID = rrd.RateRoomDetails_UID.GetValueOrDefault(),
                            Rate_UID = rrd.Rate_UID.HasValue ? rrd.Rate_UID.Value : room.Rate_UID.GetValueOrDefault(),
                            RoomType_UID = room.RoomType_UID.GetValueOrDefault(),
                            Date = rrd.Date,
                            Property_UID = reservation.Property_UID
                        };
                        distinctRoomtypes.Add(tmp);
                    }
                }

                Dictionary<long, List<long>> lstChannelIdsByRateUID = new Dictionary<long, List<long>>();

                if (distinctRoomtypes != null && distinctRoomtypes.Any())
                {
                    var unitOfWork = this.SessionFactory.GetUnitOfWork();
                    var list = new List<contractsRates.RateRoomDetailsDataWithChildRecordsCustom>();

                    var reservationRepo = this.RepositoryFactory.GetReservationsRepository(unitOfWork);
                    var rateRepo = this.RepositoryFactory.GetOBRateRepository();
                    var bookingEngineChannelUID = reservationRepo.GetBookingEngineChannelUID();

                    var distinctRoomTypesGroup = distinctRoomtypes.Where(x => x.RoomType_UID != 0).GroupBy(x => x.RoomType_UID);
                    var occupancyAlertsByRoomType = new Dictionary<long, List<PEOccupancyAlertQR1>>();

                    //Fetch all OccupancyAlerts grouped by RoomType.
                    foreach (var group in distinctRoomTypesGroup)
                    {
                        var firstItem = group.FirstOrDefault();
                        if (firstItem != null)
                        {
                            var datesList = group.Select(x => x.Date).Distinct().ToList();
                            var occupancyAlerts = reservationRepo.FindByRoomTypeUID_And_Code_And_PropertyUID_And_RoomTypeDate(
                                firstItem.RoomType_UID, firstItem.Property_UID, datesList, strCode).ToList();
                            occupancyAlertsByRoomType.Add(group.Key, occupancyAlerts);
                        }
                    }

                    //Fetch all Channels (including BE) by Rate UID for all room types.
                    var rateUIDs = occupancyAlertsByRoomType.SelectMany(x => x.Value).Select(x => x.Rate_UID).Distinct().ToList();

                    if (rateUIDs.Any())
                        lstChannelIdsByRateUID = rateRepo.GetRateChannelsList(new Contracts.Requests.GetRateChannelsListRequest { RateUIDs = rateUIDs, IncludeBEChannel = true });

                    foreach (var roomTypeGroup in distinctRoomTypesGroup)
                    {
                        var roomType = roomTypeGroup.Key;

                        //Get occupancy alerts for day and RoomType.
                        List<PEOccupancyAlertQR1> occupancyAlertsForRoomType;
                        if (!occupancyAlertsByRoomType.TryGetValue(roomType, out occupancyAlertsForRoomType) || occupancyAlertsForRoomType == null || !occupancyAlertsForRoomType.Any())
                            continue;

                        var groupedByRateId = occupancyAlertsForRoomType.GroupBy(x => x.Rate_UID.ToString() + "_" + x.RoomType_UID.ToString());

                        foreach (var group in groupedByRateId)
                        {
                            var samplePEAlert = group.FirstOrDefault();
                            var rateUID = samplePEAlert.Rate_UID;
                            var roomTypeUID = samplePEAlert.RoomType_UID;
                            var dates = group.Select(x => x.Date).ToList();

                            var datePeriods = DatePeriodHelper.CompressDatesIntoIntervals(dates);

                            foreach (var datePeriod in datePeriods)
                            {
                                List<long> lstChannelIds = new List<long>();

                                //Get Channels by Rate ID
                                if (lstChannelIdsByRateUID.ContainsKey(rateUID))
                                    lstChannelIds = lstChannelIdsByRateUID[rateUID];

                                //Check if BE is required to include.
                                if (!samplePEAlert.IsActivateCloseSalesInBE)
                                {
                                    lstChannelIds = lstChannelIds.ToList();
                                    lstChannelIds.Remove(bookingEngineChannelUID);
                                }

                                if (lstChannelIds != null && lstChannelIds.Count > 0)
                                {
                                    // Create Object to Update Rate
                                    var tmp = new contractsRates.RateRoomDetailsDataWithChildRecordsCustom()
                                    {
                                        AdultPriceList = new List<decimal>(),
                                        AdultPriceVariationIsPercentage = new List<bool>(),
                                        AdultPriceVariationIsValueDecrease = new List<bool>(),
                                        AdultPriceVariationValue = new List<decimal>(),
                                        Allocation = null,
                                        Allotment = -1,
                                        arrClosedDays = new List<bool> { true, true, true, true, true, true, true },
                                        ChildPriceList = new List<decimal>(),
                                        ChildPriceVariationIsPercentage = new List<bool>(),
                                        ChildPriceVariationIsValueDecrease = new List<bool>(),
                                        ChildPriceVariationValue = new List<decimal>(),
                                        closedDays = new List<bool> { true, true, true, true, true, true, true },
                                        CreatedBy = 65, // Admin
                                        depClosedDays = new List<bool> { true, true, true, true, true, true, true },
                                        ExtraBedPrice = null,
                                        IsBlocked = true,
                                        IsClosedOnArr = null,
                                        IsClosedOnDep = null,
                                        IsFriday = true,
                                        IsMonday = true,
                                        IsPriceVariation = false,
                                        IsSaturday = true,
                                        IsSunday = true,
                                        IsThursday = true,
                                        IsTuesday = true,
                                        IsWednesday = true,
                                        MaxDays = null,
                                        MinDays = null,
                                        NoOfAdultList = new List<int>(),
                                        NoOfChildsList = new List<int>(),
                                        Price = -1,
                                        PriceVariationExtraBedValue = 0,
                                        PriceVariationIsExtraBedPercentage = false,
                                        PriceVariationIsExtraBedValueDecrease = false,
                                        PriceVariationIsPercentage = false,
                                        PriceVariationIsValueDecrease = false,
                                        PriceVariationValue = 0,
                                        RateId = rateUID,
                                        RateRoomDetailChangedCustom_IsAllotmentChanged = false,
                                        RateRoomDetailChangedCustom_IsCancellationPolicyChanged = false,
                                        RateRoomDetailChangedCustom_IsClosedOnArrivalChanged = false,
                                        RateRoomDetailChangedCustom_IsClosedOnDepartureChanged = false,
                                        RateRoomDetailChangedCustom_IsDepositPolicyChanged = false,
                                        RateRoomDetailChangedCustom_IsExtraBedPriceChanged = false,
                                        RateRoomDetailChangedCustom_IsMaxDaysChanged = false,
                                        RateRoomDetailChangedCustom_IsMinDaysChanged = false,
                                        RateRoomDetailChangedCustom_IsPriceChanged = false,
                                        RateRoomDetailChangedCustom_IsReleaseDaysChanged = false,
                                        RateRoomDetailChangedCustom_IsStayThroughChanged = false,
                                        RateRoomDetailChangedCustom_IsStoppedSaleChanged = true,
                                        ReleaseDays = null,
                                        RoomTypeId = roomTypeUID,
                                        RRDFromDate = datePeriod.Item1.Ticks,
                                        RRDToDate = datePeriod.Item2.Ticks,
                                        SelectedChannelList = lstChannelIds,
                                        StayThrough = null
                                    };
                                    list.Add(tmp);
                                }
                            }
                        }
                    }

                    if (list.Any())
                    {
                        string correlationUID = Guid.NewGuid().ToString();

                        var updateRatesService = RepositoryFactory.GetOBRateRoomDetailsForReservationRoomRepository();
                        list.First().GeneratedBy = 4;

                        var updateRrdRequest = new UpdateRateRoomDetailsRequest
                        {
                            RateList = list,
                            Property_UID = reservation.Property_UID,
                            correlationUID = correlationUID
                        };
                        var updateRrdResponse = Retry.Execute(() => updateRatesService.UpdateRateRoomDetails(updateRrdRequest),
                            new RetryOptions
                            {
                                LogKey = Logger.Name,
                                LogLevel = Log.LogLevel.Debug,
                                RepeatOnFailedResponse = true,
                                RetryInterval = () => TimeSpan.FromMilliseconds(new Random(Guid.NewGuid().GetHashCode()).Next(OB.DL.Common.Configuration.CloseSalesRrdRetriesDelayMin_ms, OB.DL.Common.Configuration.CloseSalesRrdRetriesDelayMax_ms)),
                                RetryCount = OB.DL.Common.Configuration.CloseSalesRetriesRrdCount,
                                HandleDataLayerUpdateException = true,
                                RepeatOnBusinessLayerException = true,
                                RepeatOnPartialSuccess = true
                            });

                        if (updateRrdResponse.Status != Contracts.Responses.Status.Success)
                            LogFailedUpdateRateRoomDetails(updateRrdRequest, updateRrdResponse, reservation, nameof(CloseSalesAsPerOccupancyAlerts));
                    }
                }
            }
            catch (Exception ex)
            {
                var arguments = new Dictionary<string, object>();
                arguments.Add("resid", reservation.UID);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments);
                Logger.Fatal(ex, "RESERVATION:{0}", reservation.ToJSON());
                ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }

        void LogFailedUpdateRateRoomDetails(UpdateRateRoomDetailsRequest request, Contracts.Responses.UpdateRateRoomDetailsResponse response, contractsReservations.Reservation reservation, string methodName)
        {
            var logObj = new UpdateRateRoomDetailsLog
            {
                RequestId = request.RequestId,
                Environment = DL.Common.Configuration.Environment,
                AppName = DL.Common.Configuration.AppName,
                ErrorCode = ((int)Errors.AvailabilityControlCloseSalesFailed).ToString(),
                ErrorName = nameof(Errors.AvailabilityControlCloseSalesFailed),
                CorrelationId = request.correlationUID,
                MethodName = methodName,
                PropertyId = reservation.Property_UID,
                ReservationNumber = reservation.Number
            };

            if (response.FixedPriceResults != null && response.FixedPriceResults.TryGetValue(Contracts.Responses.OperationStatus.Failed, out var failedFixedPrices))
                logObj.FailedFixedPrices = failedFixedPrices;
            if (response.VariationResults != null && response.VariationResults.TryGetValue(Contracts.Responses.OperationStatus.Failed, out var failedVariationPrices))
                logObj.FailedVariationPrices = failedVariationPrices;

            // Log all request if there are no failed results
            var eventProperties = new LogEventPropertiesBase();
            if (logObj.FailedFixedPrices.IsNullOrEmpty() && logObj.FailedVariationPrices.IsNullOrEmpty())
                eventProperties.Request = request;

            Logger.Fatal(logObj, eventProperties);
        }

        private IEnumerable<ReservationRoomDetailRealAllotment> DecrementRealAllotmentRateRoomDetailsByReservationRoom(long reservationRoomUID)
        {
            var unitOfWork = this.SessionFactory.GetUnitOfWork();
            var reservationRoomDetailRepo = this.RepositoryFactory.GetRepository<ReservationRoomDetail>(unitOfWork);

            var result = new List<ReservationRoomDetailRealAllotment>();

            var rrd_list = reservationRoomDetailRepo.GetQuery(s => s.ReservationRoom_UID == reservationRoomUID).ToList();
            foreach (ReservationRoomDetail rrd in rrd_list.Where(x => x.RateRoomDetails_UID > 0))
            {
                // US 11579
                var newDetailRealAllotment = CreateReservationRoomDetailsRealAllotment(rrd);
                if (newDetailRealAllotment != null)
                    result.Add(newDetailRealAllotment);
            }
            return result;
        }

        private IEnumerable<ReservationRoomDetailRealAllotment> DecrementRealAllotmentRateRoomDetailsByReservationRoom(IEnumerable<ReservationRoomDetail> reservationRoomDetails)
        {
            var result = new List<ReservationRoomDetailRealAllotment>();

            foreach (ReservationRoomDetail rrd in reservationRoomDetails.Where(x => x.RateRoomDetails_UID > 0))
            {
                // US 11579
                var newDetailRealAllotment = CreateReservationRoomDetailsRealAllotment(rrd);
                if (newDetailRealAllotment != null)
                    result.Add(newDetailRealAllotment);
            }
            return result;
        }

        /// <summary>
        /// increments credit limit
        /// </summary>
        private void IncrementOperatorCreditUsed(domainReservations.Reservation objReservation, contractsReservations.ReservationPaymentDetail objReservationPaymentDetail, decimal? amount,
                                                ReservationDataContext reservationContext = null)
        {
            // Faturada
            decimal creditValue = 0;
            if (amount != null)
                creditValue = amount.Value;
            else
                if (objReservation.TotalAmount != null)
                creditValue = objReservation.TotalAmount.Value;

            if (!objReservation.Channel_UID.HasValue)
                return;

            // Check if Channel handles invoicing credit
            // used in insert
            if (reservationContext != null && reservationContext.ChannelHandleCredit == false)
                return;

            // Used in cancelation
            else
            {
                var channelRepo = this.RepositoryFactory.GetOBChannelRepository();
                var channelRequest = new Contracts.Requests.ListChannelRequest
                {
                    ChannelUIDs = new List<long> { objReservation.Channel_UID ?? 0 },
                    IncludeChannelConfiguration = true,
                    PageSize = 1
                };
                var channel = channelRepo.ListChannel(channelRequest).FirstOrDefault();
                if (channel.ChannelConfiguration != null && !channel.ChannelConfiguration.HandleCredit)
                    return;
            }

            var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();

            if (objReservation.PaymentMethodType_UID.HasValue && objReservation.IsOnRequest == false)
            {
                var payType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest { UIDs = new List<long>() { objReservation.PaymentMethodType_UID ?? 0 } }).FirstOrDefault();

                // Faturada
                if (payType != null && payType.Code == (int)Constants.PaymentMethodTypesCode.Invoicing)
                {
                    UpdateCreditUsed(objReservation.Property_UID, objReservation.Channel_UID.Value, "OperatorCreditUsed", "IsOperatorsCreditLimit", creditValue);
                }
                // Pré Pagamento
                else if (payType != null && payType.Code == (int)Constants.PaymentMethodTypesCode.PrePayment)
                {
                    UpdateCreditUsed(objReservation.Property_UID, objReservation.Channel_UID.Value, "PrePaymentCreditUsed", "IsActivePrePaymentCredit", creditValue);
                }
            }
        }

        /// <summary>
        /// increments credit limit
        /// </summary>
        private void DecrementOperatorCreditUsed(domainReservations.Reservation reservation, decimal totalAmount = -1)
        {
            if (reservation.IsPaid != true) // Verified if is already paid
            {
                ReservationPaymentDetail objReservationPaymentDetail = reservation.ReservationPaymentDetails.FirstOrDefault();

                var reservationPaymentDetail = objReservationPaymentDetail != null ? DomainToBusinessObjectTypeConverter.Convert(objReservationPaymentDetail) : null;

                decimal? valueToDecrement = (totalAmount == -1) ? reservation.TotalAmount : totalAmount;
                IncrementOperatorCreditUsed(reservation, reservationPaymentDetail, -valueToDecrement);
            }
        }

        #region MARK AS PAID METHOD AND HELPERS

        /// <summary>
        /// Set Single Reservation as Paid By Reservation UID
        /// </summary>
        /// <param name="res">The resource.</param>
        /// <param name="userId">User Id that Changes the Paid Variable</param>
        /// <param name="makeDiscount">if set to <c>true</c> [make discount].</param>
        /// <param name="markAsPaid">if set to <c>true</c> [mark as paid].</param>
        private void SetReservationIsPaid(domainReservations.Reservation res, long userId, bool makeDiscount = true, bool markAsPaid = true)
        {
            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                // If Reservation is From Operator
                if (VerifyIfReservationIsFromOperator(res))
                    this.HandleCreditDiscountForOperator(res, makeDiscount);

                // If Reservation is From TPI
                else if (VerifyIfReservationIsFromTPI(res))
                    this.HandleCreditDiscountForTPI(res);

                if (markAsPaid)
                {
                    res.IsPaid = true;
                    res.IsPaidDecisionUser = userId;
                    res.IsPaidDecisionDate = DateTime.UtcNow;
                }

                unitOfWork.Save();
            }
        }

        /// <summary>
        /// Verify if the Reservation is From a Travel Agency or Company
        /// </summary>
        /// <returns>If Not Return False</returns>
        private bool VerifyIfReservationIsFromTPI(domainReservations.Reservation _reservation)
        {
            return _reservation.TPI_UID.HasValue;
        }

        /// <summary>
        /// Verify if the Reservation is from a Tour Operator or HoteisNET Operator
        /// </summary>
        /// <returns>If Not Return False</returns>
        private bool VerifyIfReservationIsFromOperator(domainReservations.Reservation _reservation)
        {
            var channelRepo = this.RepositoryFactory.GetOBChannelRepository();
            var channelRequest = new Contracts.Requests.ListChannelRequest
            {
                ChannelUIDs = new List<long> { _reservation.Channel_UID ?? 0 },
                IncludeChannelConfiguration = true,
                PageSize = 1
            };
            var _resChannel = channelRepo.ListChannel(channelRequest).FirstOrDefault();

            return VerifyIfReservationIsFromOperator(_resChannel);
        }

        private bool VerifyIfReservationIsFromOperator(Contracts.Data.Channels.Channel channel)
        {
            return (channel?.ChannelConfiguration == null || channel?.ChannelConfiguration?.HandleCredit == true);
        }

        /// <summary>
        /// Discount Credit from Travel Agency or Company
        /// </summary>
        /// <returns>The Total Used Credit</returns>
        private void HandleCreditDiscountForTPI(domainReservations.Reservation res, Contracts.Data.Payments.PaymentMethodType paymentType = null)
        {
            if (!(res.TPI_UID > 0) || !(res.PaymentMethodType_UID > 0))
                return;

            using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
            {
                var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
                var tpiRepo = RepositoryFactory.GetThirdPartyIntermediarySqlRepository(unitOfWork);

                // Get payment type
                if (paymentType == null)
                {
                    var paymentRequest = new ListPaymentMethodTypesRequest
                    { UIDs = new List<long>() { res.PaymentMethodType_UID.Value }, PageSize = 1 };
                    paymentType = paymentMethodTypeRepo.ListPaymentMethodTypes(paymentRequest).FirstOrDefault();
                }

                // Decrements Credit Used of TPIProperty
                var tpiCreditRequest = new DL.Common.Criteria.UpdateCreditCriteria
                {
                    UpdateCreditUsed = (paymentType?.Code == (int)Constants.PaymentMethodTypesCode.Invoicing),
                    UpdatePrePaidCreditUsed = (paymentType?.Code == (int)Constants.PaymentMethodTypesCode.PrePayment),
                    PropertyUid = res.Property_UID,
                    TpiUid = res.TPI_UID.Value,
                    IncrementValue = -res.TotalAmount ?? 0
                };
                tpiRepo.UpdateTpiPropertyCredit(tpiCreditRequest);
            }
        }

        /// <summary>
        /// Discount Credit from Tour Operator or HoteisNET Operator
        /// </summary>
        /// <returns>The Total Used Credit</returns>
        private void HandleCreditDiscountForOperator(domainReservations.Reservation _reservation, bool makeDiscount, Contracts.Data.Payments.PaymentMethodType paymentType = null)
        {
            try
            {
                var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();
                var paymentMethodTypeRepo = RepositoryFactory.GetOBPaymentMethodTypeRepository();
                var channelRepo = this.RepositoryFactory.GetOBChannelRepository();
                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    var operatorSqlRepo = this.RepositoryFactory.GetOperatorSqlRepository(unitOfWork);
                    // Get the Operator
                    OB.BL.Contracts.Data.Channels.ChannelsProperty _channelRelated = propertyRepo.ListChannelsProperty(new Contracts.Requests.ListChannelsPropertyRequest
                    {
                        PropertyUIDs = new List<long> { _reservation.Property_UID },
                        ChannelUIDs = new List<long> { _reservation.Channel_UID ?? 0 }
                    }).FirstOrDefault();

                    if (paymentType == null)
                        paymentType = paymentMethodTypeRepo.ListPaymentMethodTypes(new Contracts.Requests.ListPaymentMethodTypesRequest { UIDs = new List<long> { _reservation.PaymentMethodType_UID ?? 0 } }).FirstOrDefault();

                    // Verify if Object Exists
                    if (_channelRelated != null)
                    {
                        // Verify if has Credit and if its not null
                        if (_channelRelated.OperatorCreditUsed == null)
                        {
                            _channelRelated.OperatorCreditUsed = 0;
                        }

                        // Calculate the Final Result
                        if (makeDiscount && paymentType != null && paymentType.IsBilled.HasValue && paymentType.IsBilled.Value)
                            _channelRelated.OperatorCreditUsed = _channelRelated.OperatorCreditUsed - _reservation.TotalAmount;

                        // If the Final Calculation is Negative throw an Error.
                        if (_channelRelated.OperatorCreditUsed < 0)
                        {
                            _channelRelated.OperatorCreditUsed = 0;

                            OB.BL.Contracts.Data.Channels.Channel _channel = channelRepo.ListChannel(new Contracts.Requests.ListChannelRequest { ChannelUIDs = new List<long> { _reservation.Channel_UID ?? 0 } }).FirstOrDefault();
                            Logger.Error(String.Format("Credit Used cannot be Negative for Operator: {0}.", _channel.Name));
                        }

                        //update operator credit used
                        operatorSqlRepo.UpdateOperatorCredit(new DL.Common.Criteria.UpdateCreditCriteria
                        {
                            ChannelUid = (_reservation.Channel_UID ?? 0),
                            PropertyUid = _reservation.Property_UID,
                            IncrementValue = (_channelRelated.OperatorCreditUsed ?? 0),
                            UpdateCreditUsed = true,
                            UpdatePrePaidCreditUsed = false
                        });
                    }
                    else
                    {
                        throw new ApplicationException("Relationship between Properties and Channels not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                var args = new Dictionary<string, object>();
                args.Add("_reservation", _reservation);
                args.Add("makeDiscount", makeDiscount);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.Critical, args);
            }
        }

        #endregion MARK AS PAID METHOD AND HELPERS

        private ReservationRoomDetailRealAllotment CreateReservationRoomDetailsRealAllotment(ReservationRoomDetail rrd)
        {
            ReservationRoom resRoom = rrd.ReservationRoom;
            if (resRoom != null)
            {
                ReservationRoomDetailRealAllotment newEntry = new ReservationRoomDetailRealAllotment();
                newEntry.Date = rrd.Date;
                newEntry.RoomType_UID = resRoom.RoomType_UID;
                newEntry.Rate_UID = resRoom.Rate_UID;
                return newEntry;
            }
            return null;
        }

        private OB.BL.Contracts.Data.Properties.ReservationRoomDetailRealAllotment CreateReservationRoomDetailsRealAllotment(DateTime date, long? roomTypeId, long? rateId)
        {
            OB.BL.Contracts.Data.Properties.ReservationRoomDetailRealAllotment newEntry = new OB.BL.Contracts.Data.Properties.ReservationRoomDetailRealAllotment();
            newEntry.Date = date;
            newEntry.RoomType_UID = roomTypeId;
            newEntry.Rate_UID = rateId;
            return newEntry;
        }

        /// <summary>
        /// Export Reservations To Ftp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PagedResponseBase ExportReservationsToFtp(ExportReservationsToFtpRequest request)
        {
            var rsp = new PagedResponseBase();

            try
            {
                var result = false;
                switch (request.ExternalSourceCode)
                {
                    case (int)Constants.ExternalSystemsCodes.Perot:
                        result = ExportReservationToPerot(request.Property_UID, request.DateFrom, request.DateTo, rsp);
                        break;

                    default:
                        break;
                }

                if (result)
                    rsp.Succeed();
                else
                    rsp.Failed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());

                rsp.Failed();
                var error = new OB.Reservation.BL.Contracts.Responses.Error(ex);
                if (ex is BusinessLayerException)
                {
                    error.ErrorType = ((BusinessLayerException)ex).ErrorType;
                    error.ErrorCode = ((BusinessLayerException)ex).ErrorCode;
                }

                rsp.Errors.Add(error);
            }

            return rsp;
        }

        /// <summary>
        /// Export reservations from a property between a peiod of time to perot ftp
        /// </summary>
        /// <param name="property_UID">Property id</param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns>true if success, false otherwise</returns>
        private bool ExportReservationToPerot(long property_UID, DateTime dateFrom, DateTime dateTo, PagedResponseBase rsp)
        {
            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
                var currencyRepo = RepositoryFactory.GetOBCurrencyRepository();
                var crmRepo = this.RepositoryFactory.GetOBCRMRepository();
                var reservationAdditionalDataRepo = this.RepositoryFactory.GetRepository<domainReservations.ReservationsAdditionalData>(unitOfWork);
                var propertyExternalSources = propertyRepo.ListPropertiesExternalSource(new Contracts.Requests.ListPropertiesExternalSourceRequest
                {
                    ReturnTotal = true,
                    FilterPropertyUid = new List<long> { property_UID },
                    IncludeExternalSourcesPermissions = true,
                    ActivePropertiesExternalSourcesOnly = true,
                    IncludeExternalSourceType = true
                });

                var perot = propertyExternalSources.FirstOrDefault(x => x.Code == (int)Constants.ExternalSystemsCodes.Perot);
                if (perot == null)
                    throw new BusinessLayerException("Perot is not active for this hotel");

                var channelsFilter = perot.PropertyExternalSourcePermissions.Where(x => x.Channel_UID.HasValue).Select(x => x.Channel_UID.Value).ToList();

                if (!channelsFilter.Any())
                    throw new BusinessLayerException("Need to activate at least one channel");

                // Get List of Reservations including Rooms
                var reservationsRsp = ListReservationsByCheckOut(new OB.Reservation.BL.Contracts.Requests.ListReservationRequest
                {
                    CheckIn = dateFrom,
                    CheckOut = dateTo,
                    PropertyUIDs = new List<long>() { property_UID },
                    ChannelUIDs = channelsFilter,
                    IncludeReservationRooms = true,
                    IncludeGuests = true,
                    ReservationStatus = new List<long>() { (long)Constants.ReservationStatus.Booked, (long)Constants.ReservationStatus.Modified, (long)Constants.ReservationStatus.Cancelled }
                });

                // Get PropertyBase Currency Code
                var propertyCurrency = propertyRepo.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest { UIDs = new List<long> { property_UID } }).Select(x => x.BaseCurrency_UID).FirstOrDefault();
                string currencyCode = string.Empty;
                if (propertyCurrency > 0)
                    currencyCode = currencyRepo.ListCurrencies(new Contracts.Requests.ListCurrencyRequest { UIDs = new List<long> { propertyCurrency } }).Select(x => x.Symbol).FirstOrDefault();

                // Get reservation additional data
                var reservationIds = reservationsRsp.Result.Select(x => x.UID).ToList();
                var additionalData = reservationAdditionalDataRepo.GetQuery(x => reservationIds.Contains(x.Reservation_UID))
                    .ToDictionary(x => x.Reservation_UID);

                // Get Tpis
                var tpiIds = reservationsRsp.Result.Where(x => x.TPI_UID.HasValue).Select(x => x.TPI_UID.Value).Distinct().ToList();
                var tpis = crmRepo.ListThirdPartyIntermediariesByLightCriteria(new OB.BL.Contracts.Requests.ListThirdPartyIntermediaryLightRequest { UIDs = tpiIds });
                var tpisDic = tpis.ToDictionary(x => x.UID);

                // Get Countries
                var countriesIds = tpis.Where(x => x.Country_UID > 0).Select(x => x.Country_UID).Distinct().ToList();
                var countriesDic = propertyRepo.ListCountries(new Contracts.Requests.ListCountryRequest { UIDs = countriesIds }).Select(x => new
                {
                    UID = x.UID,
                    CountryCode = (string)x.CountryCode
                }).ToDictionary(x => x.UID);

                var exportList = new List<contractsReservations.ExportReservation>();

                foreach (var item in reservationsRsp.Result)
                {
                    var tmp = new contractsReservations.ExportReservation();
                    ReservationsAdditionalData additionalTmp;
                    bool invalidReservation = true;

                    if (item.TPI_UID.HasValue)
                    {
                        invalidReservation = false;
                        OB.BL.Contracts.Data.CRM.TPICustom tpiTmp;

                        if (!tpisDic.TryGetValue(item.TPI_UID.Value, out tpiTmp))
                            continue;

                        tmp.PayeeId = tpiTmp.IATA;
                        tmp.PayeeLegalName = tpiTmp.Name;
                        tmp.PayeeName = tpiTmp.Name;
                        tmp.PayeePostalCode = tpiTmp.PostalCode;
                        tmp.PayeeStateCode = string.Empty;
                        tmp.PayeeAddress = tpiTmp.Address1;
                        tmp.PayeeCity = tpiTmp.City;

                        if (tpiTmp.Country_UID > 0)
                        {
                            string countryCode = countriesDic[tpiTmp.Country_UID].CountryCode;
                            tmp.PayeeCountryCode = countryCode;
                        }
                        else
                            tmp.PayeeCountryCode = string.Empty;
                    }

                    if (additionalData.TryGetValue(item.UID, out additionalTmp) && !string.IsNullOrEmpty(additionalTmp.ReservationAdditionalDataJSON))
                    {
                        invalidReservation = false;
                        var reservationAdditionalData = Newtonsoft.Json.JsonConvert.DeserializeObject<contractsReservations.ReservationsAdditionalData>(additionalTmp.ReservationAdditionalDataJSON);

                        tmp.PayeeId = !string.IsNullOrWhiteSpace(reservationAdditionalData.AgencyIATA) ?
                            reservationAdditionalData.AgencyIATA : !string.IsNullOrWhiteSpace(reservationAdditionalData.CompanyIATA) ?
                            reservationAdditionalData.CompanyIATA : tmp.PayeeId;

                        tmp.PayeeLegalName = !string.IsNullOrWhiteSpace(reservationAdditionalData.AgencyName) ?
                            reservationAdditionalData.AgencyName : !string.IsNullOrWhiteSpace(reservationAdditionalData.CompanyName) ?
                            reservationAdditionalData.CompanyName : tmp.PayeeLegalName;

                        tmp.PayeeName = !string.IsNullOrWhiteSpace(reservationAdditionalData.AgencyName) ?
                            reservationAdditionalData.AgencyName : !string.IsNullOrWhiteSpace(reservationAdditionalData.CompanyName) ?
                        reservationAdditionalData.CompanyName : tmp.PayeeName;
                        tmp.PayeeAddress = !string.IsNullOrWhiteSpace(reservationAdditionalData.AgencyAddress) ?
                            reservationAdditionalData.AgencyAddress : !string.IsNullOrWhiteSpace(reservationAdditionalData.CompanyAddress) ?
                            reservationAdditionalData.CompanyAddress : tmp.PayeeAddress;
                    }
                    // Payee Information
                    //else 
                    // if required fields are missing don't send reservation
                    if (invalidReservation)
                        continue;

                    tmp.CreatedDate = item.CreatedDate.HasValue ? item.CreatedDate.Value.ToString("yyyyMMdd") : string.Empty;
                    var room = item.ReservationRooms.FirstOrDefault();
                    if (room != null)
                    {
                        tmp.DateFrom = room.DateFrom.HasValue ? room.DateFrom.Value.ToString("yyyyMMdd") : string.Empty;
                        tmp.DateTo = room.DateTo.HasValue ? room.DateTo.Value.ToString("yyyyMMdd") : string.Empty;

                        // if required fields are missing don't send reservation
                        if (string.IsNullOrEmpty(tmp.DateFrom) || string.IsNullOrEmpty(tmp.DateTo))
                            continue;

                        tmp.NumberOfDays = room.DateTo.Value.Subtract(room.DateFrom.Value).Days;
                        tmp.CommissionRate = room.CommissionValue != null ? Math.Round(room.CommissionValue.Value, 2).ToString().Replace(",", ".") : "0";
                    }

                    if (string.IsNullOrEmpty(item.GuestLastName))
                    {
                        if (item.Guest != null)
                        {
                            tmp.GuestFirstName = string.IsNullOrEmpty(item.Guest.FirstName) ? string.Empty : item.Guest.FirstName.ToUpper();
                            tmp.GuestLastName = string.IsNullOrEmpty(item.Guest.LastName) ? string.Empty : item.Guest.LastName.ToUpper();
                        }
                    }
                    else
                    {
                        tmp.GuestFirstName = string.IsNullOrEmpty(item.GuestFirstName) ? string.Empty : item.GuestFirstName.ToUpper();
                        tmp.GuestLastName = item.GuestLastName.ToUpper();
                    }

                    tmp.NumberOfRooms = item.NumberOfRooms ?? 0;
                    tmp.Number = item.Number;
                    tmp.NumberOfAdults = item.Adults;
                    tmp.NumberOfChildren = item.Children ?? 0;
                    tmp.Property_UID = item.Property_UID;
                    tmp.CurrencyCode = currencyCode;
                    tmp.CommissionableRevenue = item.TotalAmount != null ? Math.Round(item.TotalAmount.Value, 2).ToString().Replace(",", ".") : "0";

                    if (item.Status == (long)Constants.ReservationStatus.Cancelled)
                    {
                        tmp.CommissionAmount = "0";
                        tmp.TransactionCode = "NA";
                    }

                    exportList.Add(tmp);
                }

                if (!exportList.Any())
                    return true;

                rsp.TotalRecords = exportList.Count;
                var perotGroupId = WebConfigurationManager.AppSettings["perotGroupId"];

                var date = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string fileName = perotGroupId + "." + property_UID.ToString().PadLeft(5, '0') + "." + date + ".TRANS.TXT";

                StringBuilder dataToEncrypt = new StringBuilder();
                // File Header
                dataToEncrypt.AppendLine(@"BH|" + fileName + "|" + date + "|" + perotGroupId + "|OMNIBEES");
                // File Content
                var tmpString = TemplateEngine<contractsReservations.ExportReservation>.GenerateFile("Perot", exportList);

                var lastIndex = tmpString.LastIndexOf(System.Environment.NewLine, StringComparison.InvariantCulture);
                if (lastIndex > 0)
                    tmpString = tmpString.Remove(tmpString.LastIndexOf(System.Environment.NewLine, StringComparison.InvariantCulture));

                dataToEncrypt.AppendLine(tmpString);

                // File Trailer
                dataToEncrypt.AppendLine(@"BT|" + reservationsRsp.Result.Count + "|||||");

                // Encrypt Data
                var appSettingsRepo = RepositoryFactory.GetOBAppSettingRepository();
                var key = appSettingsRepo.ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string> { "PerotPublicKey" } }).FirstOrDefault();

                if (key == null)
                    throw new ArgumentNullException("AppSettings Perot Key", "PerotPublicKey does not exist in appsettings");

                // Uncomment to encrypt data
                //var encryptedStream = PgpEncrypt.Encrypt(dataToEncrypt.ToString(), key.Value, true, false);

                #region TEST

                //            var encryptedStream = PgpEncrypt.Encrypt(dataToEncrypt.ToString(), @"mQENBFUr8ksBCACJ5DWwDD9oC6Z3OWngYMo0HqLt4zgGOGu0m1GccaYaKLjU3kcD
                //                                    s6/8B+/oFWaG1q307VQzCm/o08NMBunH0d5FglHtISNV9jm0J4DCCxLLBmbtUJ9d
                //                                    2OtzN9bOruT1SHG6apE3GVkzGau2AB6+lepZfxmpqk/9IkM/qqvzUNFtS4o8eDU4
                //                                    ziJqBSyBh59KlG/KBDPcf5pKYwRe+fBBagfTeOQz4EOCSiz9Kj8ehwp8mRMn+CEA
                //                                    9h6YTdiIlzoM/ZpNH9XM3k0JU+kBxgvuxo0YPsZsFn5iLnJp74d81tgWUIZCxuNs
                //                                    EtF444K/UcqR1dwEJUrlS3WraFRbvq1BRXl1ABEBAAG0AIkBHAQQAQIABgUCVSvy
                //                                    SwAKCRAgkzDRjlPAbUKVB/9M3KgCG/vhPAQoEUZ4o4mtgyQn1SV8RL2BpXzU8kaE
                //                                    KcyGSLN6irmBvsN6Z3YavwEWe9r/JFgzk3r8Nqw6PpjDUPOretfqKDGOUkEh4GSN
                //                                    02p/8sDPFahMZtppX4b1x6I6OcSiRKomE3NvZ67kJbhTvLZAggxzQ4fFCOYqKD9C
                //                                    Zmtxo5YSjgk5t8LksW7gUbPWcKwrQD2NFeLWi5tix3smu3DuvF9gFNRqZVgoGVgG
                //                                    LQVntiRt8w1oL2bU5iEK2ozDADC6OixjXs/E8hI3BoJTu5BGU6IZKDsyHIRY241w
                //                                    kzcPLXlJDOB7FNb9jQQKRYPw99q20r46XM84U/77uYu1
                //                                    =Foph", true, false);

                //using (BinaryWriter writer = new BinaryWriter(File.Open(@"C:\temp\encrypted.PGP", FileMode.Create)))
                //{
                //    writer.Write(IoHelper.GetBytes(encryptedStream));
                //}

                //var result = true;

                #endregion TEST

                // Send to Ftp
                var host = WebConfigurationManager.AppSettings["perotHost"];
                int port = Convert.ToInt32(WebConfigurationManager.AppSettings["perotPort"]);
                var user = WebConfigurationManager.AppSettings["perotUsername"];
                var pass = WebConfigurationManager.AppSettings["perotPassword"];

                var ftp = new FTPManager(host, port, user, pass);
                var result = ftp.UploadFile("TO_TACS/" + fileName, dataToEncrypt.ToString());

                #region TEST

                //Save file to HardDrive Unencrypted - REMOVE IN PRODUCTION
                //            using (System.IO.BinaryReader read = new System.IO.BinaryReader(File.Open(@"C:\temp\" + fileName, FileMode.Open)))
                //            {
                //                var decrypted = PgpEncrypt.Decrypt(read.BaseStream, @"lQOsBFUr8ksBCACJ5DWwDD9oC6Z3OWngYMo0HqLt4zgGOGu0m1GccaYaKLjU3kcD
                //                                            s6/8B+/oFWaG1q307VQzCm/o08NMBunH0d5FglHtISNV9jm0J4DCCxLLBmbtUJ9d
                //                                            2OtzN9bOruT1SHG6apE3GVkzGau2AB6+lepZfxmpqk/9IkM/qqvzUNFtS4o8eDU4
                //                                            ziJqBSyBh59KlG/KBDPcf5pKYwRe+fBBagfTeOQz4EOCSiz9Kj8ehwp8mRMn+CEA
                //                                            9h6YTdiIlzoM/ZpNH9XM3k0JU+kBxgvuxo0YPsZsFn5iLnJp74d81tgWUIZCxuNs
                //                                            EtF444K/UcqR1dwEJUrlS3WraFRbvq1BRXl1ABEBAAH/AwMC+hdOkQspTeNgpFot
                //                                            l2xHXm4IYLEemEb2mwbHeZ6zfFJ4xQO/TOcDsYEVCLgrXxKQLAjQSuAAWmOIBFxC
                //                                            d35jd1rzxYfCjtyIhfaaqmIUrg6RWzRXRSw7HS/YybQZlcr60wTMZwMR7buIfleA
                //                                            KnciabnBNdsoL0GOodRWyqbUHN2LHwusl3Y8Crc4avDEX4nNIO/PCjVtLugbSb10
                //                                            3QZ6BE/DBiXbYh7ZWprfQ/OGKaRr8/Njws8C41wvRVPMiDrY4rW6JdlUgpCAJSGm
                //                                            PItk3B85VZoYbOW/wcOyVN9RXXLpzGJY9q6okv9SfOi5xSbE81RoYJV8x9gw7PA1
                //                                            hscKnmm1ZzQcVeZpWy6h9u3P8W4zM3XxbK8oPiGYytlk96pExeBGcTSTe9MW8CM+
                //                                            tlx5zUtUWkbgiTXzuyt/lGBJ61h0nMBbLRz7qiUPZU7J4dze1YvfwouDDYLUmjC+
                //                                            iNZU0kMMi5fGwuUzv8TboHlH5KUIgudvoG5JIh8mU6c13Mx29XJjClCs4/QrU4qW
                //                                            1Tm5uIOSxzvyfvxaLpbywV+6Zbxjne4321iqog1UfNGDpE4ws0fMDIRVKZw3JKK7
                //                                            efiN+uA0NuaopjnXxW5dZTtNuFBXxNDBoyt7IhiAQoxrJZhapz3mXfDNDu17wWgq
                //                                            plRu2uBMdbyFJRDPHVvjFXIjKU0EDEGTjblmVBRpVJWgzrMNDXayrc7d69bcGLpm
                //                                            iWsiJbzuN0GGS8PO7QgmQmMMd8Zxg6mp2O/eR6Mt5V+msZAoaVbc09PLcFG2ZF54
                //                                            MuZAx8Lq1hMoIUtiCW/VX3IKv27zwVC6fci30wiiuE+gUaS01/tVmABjZRgkInSD
                //                                            upr2SGObMKIxaVLpSWM3HsaGk659sL8Ax0ZMhxJuCrQAiQEcBBABAgAGBQJVK/JL
                //                                            AAoJECCTMNGOU8BtQpUH/0zcqAIb++E8BCgRRnijia2DJCfVJXxEvYGlfNTyRoQp
                //                                            zIZIs3qKuYG+w3pndhq/ARZ72v8kWDOTevw2rDo+mMNQ86t61+ooMY5SQSHgZI3T
                //                                            an/ywM8VqExm2mlfhvXHojo5xKJEqiYTc29nruQluFO8tkCCDHNDh8UI5iooP0Jm
                //                                            a3GjlhKOCTm3wuSxbuBRs9ZwrCtAPY0V4taLm2LHeya7cO68X2AU1GplWCgZWAYt
                //                                            BWe2JG3zDWgvZtTmIQrajMMAMLo6LGNez8TyEjcGglO7kEZTohkoOzIchFjbjXCT
                //                                            Nw8teUkM4HsU1v2NBApFg/D32rbSvjpczzhT/vu5i7U=
                //                                            =R1PJ", "123456");

                //                //Save file to HardDrive Unencrypted - REMOVE IN PRODUCTION
                //                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\temp\decrypted.txt"))
                //                {
                //                    file.WriteLine(IoHelper.GetString(decrypted));
                //                }

                //}

                #endregion TEST

                if (result)
                {
                    perot.LastExportDate = dateTo;
                    var response = propertyRepo.UpdatePropertiesExternalSources(new UpdatePropertyExternalSourceRequest { PropertyExternalSource = new List<contractsProperties.PropertiesExternalSource> { perot } });
                    if (response.Status == Contracts.Responses.Status.Fail && response.Errors.Any())
                        throw new Exception(response.Errors.First().Description);
                }

                return result;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MarkReservationsAsViewedResponse MarkReservationsAsViewed(MarkReservationsAsViewedRequest request, bool IsDisposed = true)
        {
            var response = new MarkReservationsAsViewedResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                var readStatus = new OB.Reservation.BL.Contracts.Data.VisualStates.ReservationReadStatus(request.User_UID, DateTime.UtcNow, request.NewValue);
                var newValue = Newtonsoft.Json.JsonConvert.SerializeObject(readStatus);

                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    using (var transaction = unitOfWork.BeginTransaction(domainReservations.Reservation.DomainScope))
                    {
                        var vsRepository = this.RepositoryFactory.GetVisualStateRepository(unitOfWork);
                        var repositoryResFilter = this.RepositoryFactory.GetReservationsFilterRepository(unitOfWork);

                        var query = vsRepository.GetQuery();
                        var reservationUidsString = request.Reservation_UIDs.Select(q => q.ToString()).ToList();
                        var existingLines = query.Where(q => reservationUidsString.Contains(q.LookupKey_1)).ToList();
                        var appliedReservations = new List<long>();

                        var reservationsFilter = repositoryResFilter.FindByReservationUIDs(request.Reservation_UIDs).ToList();
                        if (reservationsFilter != null)
                            reservationsFilter.ForEach(x => x.IsReaded = readStatus.Read);

                        if (existingLines.Any())
                        {
                            foreach (var line in existingLines)
                            {
                                line.JSONData = newValue;
                                appliedReservations.Add(Convert.ToInt64(line.LookupKey_1));
                            }
                        }

                        // Create the Lines
                        var reservationsWithoutLines = request.Reservation_UIDs.Except(appliedReservations);

                        if (reservationsWithoutLines.Any())
                        {
                            foreach (var reservation in reservationsWithoutLines)
                            {
                                var newViewState = new domainReservations.VisualState();
                                newViewState.JSONData = newValue;
                                newViewState.LookupKey_1 = reservation.ToString();
                                newViewState.StateType = "ReservationReadStatus";
                                vsRepository.Add(newViewState);
                            }
                        }

                        unitOfWork.Save();
                        transaction.Commit();
                    }
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
                response.Failed();
            }

            return response;
        }

        /// <summary>
        /// Gets all instances of ReservationReadStatus given a search Criteria (ListMarkReservationsAsViewedRequest)  returning a ListMarkReservationsAsViewedResponse instance with all ReservationReadStatus instances
        /// that match the search criteria or none if not found any in the database that matches the given criteria..
        /// </summary>
        /// <param name="request">ListMarkReservationsAsViewedRequest instance</param>
        /// <returns>A ListMarkReservationsAsViewedResponse object that contains a collection of ReservationReadStatus instances.</returns>
        public ListMarkReservationsAsViewedResponse ListMarkReservationsAsViewed(ListMarkReservationsAsViewedRequest request)
        {
            var response = new ListMarkReservationsAsViewedResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");

                using (var unitOfWork = SessionFactory.GetUnitOfWork(true))
                {
                    var visualStatesRepo = this.RepositoryFactory.GetVisualStateRepository(unitOfWork);

                    var reservationUIDsStr = request.ReservationUIDs != null
                        ? request.ReservationUIDs.Select(x => x.ToString()).ToList()
                        : null;
                    var result = visualStatesRepo.FindVisualStateByCriteria(request.UIDs, reservationUIDsStr);

                    response.Result = result.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }
            return response;
        }


        /// <summary>
        /// RESTful implementation of the ListPaymentGatewayTransactions operation.
        /// This operation searchs for PaymentGatewayTransactions given a specific request criteria to search PaymentGatewayTransactions matching that criteria.
        /// </summary>
        /// <param name="request">A ListPaymentGatewayTransactionsRequest object containing the Search criteria</param>
        /// <returns>A ListPaymentGatewayTransactionsResponse containing the UID's of matching PaymentGatewayTransaction objects</returns>
        public ListPaymentGatewayTransactionsResponse ListPaymentGatewayTransactions(ListPaymentGatewayTransactionsRequest request)
        {
            var response = new ListPaymentGatewayTransactionsResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");

                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var paymentGatewayRepo = this.RepositoryFactory.GetPaymentGatewayTransactionRepository(unitOfWork);

                    var criteria = RequestToCriteriaConverters.Convert(request);

                    var result = paymentGatewayRepo.FindByCriteria(criteria);

                    response.Result = result.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }

            return response;
        }

        #region ALLOTMENT And INVENTORY

        private void SendAllotmentAndInventoryForChannels(long propertyUID, long? channelUID, string correlationId, IList<contractsReservations.ReservationRoom> oldObjReservationRoom, IList<contractsReservations.ReservationRoom> objReservationRoom, ServiceName service, List<contractsProperties.Inventory> inventories, bool notifyDecrementInventory = true, bool notifyIncrementAllotment = true)
        {
            var propertyRepo = RepositoryFactory.GetOBPropertyRepository();

            int propertyAvailabilityType = 0;
            var property = propertyRepo.ListPropertiesLight(new ListPropertyRequest { UIDs = new List<long> { propertyUID } }).FirstOrDefault();
            if (property != null)
                propertyAvailabilityType = property.AvailabilityType;

            var allotmentToInsertOrUpdate = new List<OB.BL.Contracts.Data.Properties.ReservationRoomDetailRealAllotment>();

            if (notifyIncrementAllotment)
            {
                var rateRooms = new List<contractsRates.RateRoom>();
                //Get all rates with availability type inventory - PropertyAvailabilityType is Mixed and RateAvailabilityType is Allotment 
                if (propertyAvailabilityType == (int)Contracts.Data.PropertyAvailabilityTypes.Mixed)
                {
                    rateRooms = GetAllRatesWithAvailabilityInventoryByRoomUID(objReservationRoom, service, inventories);
                }

                foreach (var rr in objReservationRoom)
                {
                    foreach (var rrd in rr.ReservationRoomDetails)
                    {
                        if (service == ServiceName.InsertReservation || (service != ServiceName.InsertReservation && rrd.Date.Date >= DateTime.Today.Date.AddDays(-1))) // BUG 26593
                        {
                            allotmentToInsertOrUpdate.Add(CreateReservationRoomDetailsRealAllotment(rrd.Date, rr.RoomType_UID,
                                (propertyAvailabilityType != (int)Contracts.Data.PropertyAvailabilityTypes.Inventory)
                                ? (rrd.Rate_UID ?? rr.Rate_UID) : null));

                            //PropertyAvailabilityType is Mixed and RateAvailabilityType is Allotment or Inventory
                            if (rateRooms.Any())
                                rateRooms.ForEach(x => allotmentToInsertOrUpdate.Add(CreateReservationRoomDetailsRealAllotment(rrd.Date, x.RoomType_UID, x.Rate_UID)));
                        }
                    }
                }
            }

            if ((service == ServiceName.ModifyReservation || service == ServiceName.UpdateReservation) && notifyDecrementInventory)
            {
                var rateRooms = new List<contractsRates.RateRoom>();
                //Get all rates with availability type inventory - PropertyAvailabilityType is Mixed and RateAvailabilityType is Allotment 
                if (propertyAvailabilityType == (int)Contracts.Data.PropertyAvailabilityTypes.Mixed)
                {
                    rateRooms = GetAllRatesWithAvailabilityInventoryByRoomUID(oldObjReservationRoom, service, inventories);
                }

                foreach (var rr in oldObjReservationRoom)
                {
                    foreach (var rrd in rr.ReservationRoomDetails)
                    {
                        if (rrd.Date.Date >= DateTime.Today.Date.AddDays(-1)) // BUG 26593
                            allotmentToInsertOrUpdate.Add(CreateReservationRoomDetailsRealAllotment(rrd.Date, rr.RoomType_UID,
                                (propertyAvailabilityType != (int)Contracts.Data.PropertyAvailabilityTypes.Inventory)
                                ? (rrd.Rate_UID ?? rr.Rate_UID) : null));

                        //PropertyAvailabilityType is Mixed and RateAvailabilityType is Allotment or Inventory
                        if (rateRooms.Any())
                            rateRooms.ForEach(x => allotmentToInsertOrUpdate.Add(CreateReservationRoomDetailsRealAllotment(rrd.Date, x.RoomType_UID, x.Rate_UID)));
                    }
                }
            }

            // async apply real allotment/inventory - realtime and AsyncTask to Push
            propertyRepo.ApplyRealAllotmentToAllChannels(new Contracts.Requests.ApplyRealAllotmentToChannelsRequest
            {
                ReservationRoomDetailsRealAllotment = allotmentToInsertOrUpdate,
                PropertyUID = propertyUID,
                FromChannel = channelUID ?? 0,
                CorrelationID = correlationId,
                IsRatePerOccupany = true
            });
        }

        private List<Contracts.Data.Rates.RateRoom> GetAllRatesWithAvailabilityInventoryByRoomUID(IList<contractsReservations.ReservationRoom> objReservationRoom, ServiceName service, List<contractsProperties.Inventory> inventories)
        {
            var rateRepo = RepositoryFactory.GetOBRateRepository();
            var rateRooms = new List<Contracts.Data.Rates.RateRoom>();

            var roomtypeUIds = objReservationRoom.Where(x => x.RoomType_UID.HasValue &&
            ((x.Rate != null && (x.Rate.AvailabilityType == (int)Contracts.Data.RateAvailabilityTypes.Allotment || x.Rate.AvailabilityType == (int)Contracts.Data.RateAvailabilityTypes.Inventory)) ||
            (x.ReservationRoomDetails.Any(j => j.Rate != null && (j.Rate.AvailabilityType == (int)Contracts.Data.RateAvailabilityTypes.Allotment || j.Rate.AvailabilityType == (int)Contracts.Data.RateAvailabilityTypes.Inventory
            || RateAllotmentInventoryImpactNotifyAllRatesInventory(j.Rate, j.Date, x.RoomType_UID.Value, service, inventories)))))
            ).Select(x => x.RoomType_UID.Value).Distinct().ToList();

            if (roomtypeUIds.Any())
            {
                rateRooms = rateRepo.ListRateRooms(new ListRateRoomsRequest { RoomTypeUIDs = roomtypeUIds, ExcludeDeleteds = true, RateAvailabilityTypes = new List<int> { (int)Contracts.Data.RateAvailabilityTypes.Inventory } });

                foreach (var rr in objReservationRoom)
                    foreach (var rrd in rr.ReservationRoomDetails)
                        rateRooms = rateRooms.Where(x => x.Rate_UID != (rrd.Rate_UID ?? rr.Rate_UID ?? 0)).ToList();
            }

            return rateRooms;
        }

        private bool RateAllotmentInventoryImpactNotifyAllRatesInventory(OB.Reservation.BL.Contracts.Data.Rates.Rate rate, DateTime date, long roomType_Uid, ServiceName service, List<contractsProperties.Inventory> inventories)
        {
            bool notify = false;

            if (rate != null && rate.AvailabilityType == (int)Contracts.Data.RateAvailabilityTypes.AllotmentInventoryImpact && inventories?.Any() == true)
            {
                if (service == ServiceName.InsertReservation)
                {
                    notify = inventories.Any(x => x.Date.Date == date.Date && x.RoomType_UID == roomType_Uid && (x.QtyContractedAllotmentTotal - x.QtyContractedAllotmentOccupied) < 0);
                }
                else //cancel, modify and update
                {

                    notify = inventories.Any(x => x.Date.Date == date.Date
                    && x.RoomType_UID == roomType_Uid
                    && x.parametersPerDayToUpdate.FirstOrDefault(p => p.Day.Date == x.Date.Date && p.RoomTypeUID == roomType_Uid) != null
                    && (x.QtyContractedAllotmentTotal - (x.QtyContractedAllotmentOccupied + x.parametersPerDayToUpdate.FirstOrDefault(p => p.Day.Date == x.Date.Date && p.RoomTypeUID == roomType_Uid).AddQty)) <= 0);
                }
            }

            return notify;
        }

        public List<OB.BL.Contracts.Data.Properties.Inventory> ValidateUpdateReservationAllotmentAndInventory(GroupRule groupRule, List<UpdateAllotmentAndInventoryDayParameters> parametersPerDay, bool validate,
                                                    bool update = true, string correlationId = null, bool isUpdateReservation = false, bool ignoreAvailability = false)
        {
            var updatedDays = new List<OB.BL.Contracts.Data.Properties.Inventory>();

            // If reservation is from pms and has the rule to not update inventory
            if (groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.IgnoreAvailability) && ignoreAvailability == true)
                return updatedDays;

            // Group by Day, RoomType and Rate to optimize query
            var parameters = parametersPerDay.GroupBy(x => new { x.Day, x.RoomTypeUID, x.RateRoomDetailUID, x.RateAvailabilityType })
                                .Select(x => new UpdateAllotmentAndInventoryDayParameters()
                                {
                                    Day = x.Key.Day,
                                    RoomTypeUID = x.Key.RoomTypeUID,
                                    AddQty = x.Sum(y => y.AddQty),
                                    RateRoomDetailUID = x.Key.RateRoomDetailUID,
                                    RateAvailabilityType = x.Key.RateAvailabilityType
                                }).Where(x => x.AddQty != 0).ToList();

            // The days to decrement Allotment/Inventory are never validated
            // Days to decrement must be execute before days to increment to avoid falses unavalabilities
            var daysToDecrement = parameters.Where(x => x.AddQty < 0).ToList();
            if (daysToDecrement.Any())
                updatedDays = ValidateUpdateReservationAllotmentAndInventoryAux(groupRule, daysToDecrement, false, update, correlationId, isUpdateReservation);

            var daysToIncrement = parameters.Except(daysToDecrement).ToList();
            if (daysToIncrement.Any())
                updatedDays.AddRange(ValidateUpdateReservationAllotmentAndInventoryAux(groupRule, daysToIncrement, validate, update, correlationId, isUpdateReservation));

            foreach (var item in updatedDays)
                item.parametersPerDayToUpdate = parameters.Where(x => x.Day.Date == item.Date.Date && x.RoomTypeUID == item.RoomType_UID).Select(x =>
                    new contractsProperties.UpdateAllotmentAndInventoryDayParameters { AddQty = x.AddQty, RateAvailabilityType = x.RateAvailabilityType, Day = x.Day, RateRoomDetailUID = x.RateRoomDetailUID, RoomTypeUID = x.RoomTypeUID }).ToList();

            return updatedDays;
        }

        List<OB.BL.Contracts.Data.Properties.Inventory> ValidateUpdateReservationAllotmentAndInventoryAux(GroupRule groupRule, List<UpdateAllotmentAndInventoryDayParameters> parametersPerDay, bool validate, bool update, string correlationId, bool isUpdateReservation = false)
        {
            var inventories = new List<OB.BL.Contracts.Data.Properties.Inventory>();
            bool validationRuleIsSet = groupRule != null && groupRule.BusinessRules.HasFlag(BusinessRules.ValidateAllotment);
            bool isInvalid = false;

            // Group parameter by RateAvailabilityType to apply update rules (Rates of Inventory type are updated first to avoid overbooking)
            foreach (var availabilityTypeGroup in parametersPerDay.GroupBy(x => x.RateAvailabilityType).OrderBy(x => x.Key))
            {
                // Update Allotment
                if (Enum.IsDefined(typeof(OB.BL.Contracts.Data.RateAvailabilityTypes), availabilityTypeGroup.Key))
                {
                    if (availabilityTypeGroup.Key != (int)OB.BL.Contracts.Data.RateAvailabilityTypes.Inventory)
                    {
                        Dictionary<long, int> rateRoomDetailsVsAllotment = availabilityTypeGroup.GroupBy(x => x.RateRoomDetailUID).ToDictionary(k => k.Key, v => v.Sum(x => x.AddQty));

                        // Update or Validate Allotment
                        int rateRoomDetailsCount = update ? UpdateRateRoomDetailAllotments(rateRoomDetailsVsAllotment, validate, correlationId, isUpdateReservation)
                            : CountRateRoomDetailsAllotmentToUpdate(rateRoomDetailsVsAllotment);

                        isInvalid |= validationRuleIsSet && validate && availabilityTypeGroup.Count() != rateRoomDetailsCount;
                    }
                }
                // Rejects reservation if channel isn't Push/GDS and RateAvailabilityType is invalid
                else if (validationRuleIsSet && validate)
                    throw Errors.InvalidRateAvailabilityType.ToBusinessLayerException();

                // Update Inventory for each roomType
                foreach (var roomTypeGroup in availabilityTypeGroup.GroupBy(x => x.RoomTypeUID))
                {
                    Dictionary<DateTime, int> inventoriesToUpdate = roomTypeGroup.GroupBy(x => x.Day).ToDictionary(k => k.Key, v => v.Sum(x => x.AddQty));

                    if (update)
                    {
                        var updatedInventories = UpdateReservationRoomInventories(roomTypeGroup.Key, inventoriesToUpdate, availabilityTypeGroup.Key, validate);
                        isInvalid |= validationRuleIsSet && validate && inventoriesToUpdate.Count != updatedInventories.Count;
                        inventories.AddRange(updatedInventories);
                    }
                    else if (validate && validationRuleIsSet)
                    {
                        int inventoriesCount = CountReservationRoomInventoriesToUpdate(roomTypeGroup.Key, inventoriesToUpdate);
                        isInvalid |= inventoriesToUpdate.Count != inventoriesCount;
                    }
                }
            }

            if (isInvalid)
                throw new InvalidAllotmentException();

            return inventories;
        }

        public int CountRateRoomDetailsAllotmentToUpdate(Dictionary<long, int> rateRoomDetailsVsAllotmentToAdd)
        {
            string DB_Name = ConfigurationManager.AppSettings["DB_Name"];
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);

            StringBuilder batchStatement = new StringBuilder();
            foreach (var rrdAllot in rateRoomDetailsVsAllotmentToAdd)
            {
                batchStatement.AppendFormat(COUNT_VALID_ALLOTMENT_TO_UPDATE_BATCH_QUERY, rrdAllot.Key, rrdAllot.Value, DB_Name);
                batchStatement.AppendLine();
            }

            return sqlManager.ExecuteSql<int>(batchStatement.ToString(), null).FirstOrDefault();
        }

        //[Obsolete("Moved to repository")] had bug, reverted
        public int UpdateRateRoomDetailAllotments(Dictionary<long, int> rateRoomDetailsVsAllotmentToAdd, bool validateAllotment, string correlationId = null, bool isUpdateReservation = false)
        {
            string DB_Name = ConfigurationManager.AppSettings["DB_Name"];
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);

            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            string query = (isUpdateReservation && !validateAllotment) ? UPDATE_ALLOTMENT_BATCH_QUERY_UPDATE_RESERVATION : (validateAllotment ? UPDATE_ALLOTMENT_BATCH_QUERY_WITH_VALIDATION : UPDATE_ALLOTMENT_BATCH_QUERY);

            StringBuilder batchStatement = new StringBuilder();
            foreach (var rrdAllot in rateRoomDetailsVsAllotmentToAdd)
            {
                batchStatement.AppendFormat(query, rrdAllot.Key, rrdAllot.Value, correlationId, DB_Name);
                batchStatement.AppendLine();
            }

            return sqlManager.ExecuteSql(batchStatement.ToString());
        }

        public int CountReservationRoomInventoriesToUpdate(long roomTypeUID, Dictionary<DateTime, int> daysAddQty)
        {
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            string DB_Name = ConfigurationManager.AppSettings["DB_Name"];

            StringBuilder batchStatement = new StringBuilder();
            foreach (var dayQty in daysAddQty)
            {
                batchStatement.AppendFormat(COUNT_INVENTORY_TO_UPDATE_BATCH_QUERY, DB_Name, roomTypeUID, dayQty.Key, dayQty.Value);
                batchStatement.AppendLine();
            }

            return sqlManager.ExecuteSql<int>(batchStatement.ToString(), null).FirstOrDefault();
        }

        public List<OB.BL.Contracts.Data.Properties.Inventory> UpdateReservationRoomInventories(long roomTypeUID, Dictionary<DateTime, int> daysAddQty, int rateAvailabilityType, bool canValidateInventory)
        {
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            string DB_Name = ConfigurationManager.AppSettings["DB_Name"];

            string query = UPSERT_INVENTORY_BATCH_QUERY;
            string qtyContractedStr = "0";

            if (rateAvailabilityType == (int)OB.BL.Contracts.Data.RateAvailabilityTypes.AllotmentInventoryImpact)
                qtyContractedStr = null;

            else if (rateAvailabilityType == (int)OB.BL.Contracts.Data.RateAvailabilityTypes.Inventory && canValidateInventory)
            {
                query = UPSERT_INVENTORY_BATCH_QUERY_WITH_VALIDATION + "{4}"; // Add fourth parameter to be replaced in AppendFormat method to avoid throw an exception
                qtyContractedStr = string.Empty;
            }

            StringBuilder batchStatement = new StringBuilder();
            batchStatement.AppendLine(@"DECLARE @TmpInventory TABLE
                                        ( 
                                            UID INT, 
                                            Date DATETIME,
	                                        RoomType_UID BIGINT,
	                                        QtyRoomOccupied INT,
	                                        QtyContractedAllotmentTotal INT,
	                                        QtyContractedAllotmentOccupied INT
                                        );");

            foreach (var dayQty in daysAddQty)
            {
                string qtyContractedOccupied = qtyContractedStr ?? dayQty.Value.ToString();
                batchStatement.AppendFormat(query, DB_Name, roomTypeUID, dayQty.Key, dayQty.Value, qtyContractedOccupied);
                batchStatement.AppendLine();
            }

            batchStatement.AppendLine("SELECT * from @TmpInventory");

            return sqlManager.ExecuteSql<OB.BL.Contracts.Data.Properties.Inventory>(batchStatement.ToString(), null).ToList();
        }

        static readonly string COUNT_VALID_ALLOTMENT_TO_UPDATE_BATCH_QUERY = @"
            SELECT COUNT(UID) FROM {2}.dbo.RateRoomDetails
            WHERE {2}.dbo.RateRoomDetails.UID = {0}
            AND (({2}.dbo.RateRoomDetails.AllotmentUsed is null AND ({2}.dbo.RateRoomDetails.Allotment - {1}) >= 0)
                  OR
                 ({2}.dbo.RateRoomDetails.Allotment - {2}.dbo.RateRoomDetails.AllotmentUsed - {1}) >= 0)";

        static readonly string UPDATE_ALLOTMENT_BATCH_QUERY_WITH_VALIDATION = @"
            UPDATE {3}.dbo.RateRoomDetails
            SET {3}.dbo.RateRoomDetails.AllotmentUsed = (CASE WHEN ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed, 0) + {1} < 0 THEN 0 ELSE (ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed,0) + {1}) END),
            {3}.dbo.RateRoomDetails.ModifyDate = GETUTCDATE(),
            {3}.dbo.RateRoomDetails.correlationID = '{2}'
            WHERE {3}.dbo.RateRoomDetails.UID = {0}
            AND (({3}.dbo.RateRoomDetails.AllotmentUsed is null AND ({3}.dbo.RateRoomDetails.Allotment - {1}) >= 0)
                  OR
                 ({3}.dbo.RateRoomDetails.Allotment - {3}.dbo.RateRoomDetails.AllotmentUsed - {1}) >= 0)";

        static readonly string UPDATE_ALLOTMENT_BATCH_QUERY_UPDATE_RESERVATION = @"
            UPDATE {3}.dbo.RateRoomDetails
            SET {3}.dbo.RateRoomDetails.AllotmentUsed = ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed,0) + {1},
            {3}.dbo.RateRoomDetails.ModifyDate = GETUTCDATE(),
            {3}.dbo.RateRoomDetails.correlationID = '{2}'
            WHERE {3}.dbo.RateRoomDetails.UID = {0}";

        static readonly string UPDATE_ALLOTMENT_BATCH_QUERY = @"
            UPDATE {3}.dbo.RateRoomDetails
            SET {3}.dbo.RateRoomDetails.AllotmentUsed = (CASE WHEN ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed, 0) + {1} < 0 THEN 0 ELSE (ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed,0) + {1}) END),
            {3}.dbo.RateRoomDetails.ModifyDate = GETUTCDATE(),
            {3}.dbo.RateRoomDetails.correlationID = '{2}'
            WHERE {3}.dbo.RateRoomDetails.UID = {0}";

        static readonly string COUNT_INVENTORY_TO_UPDATE_BATCH_QUERY = @"
            IF (EXISTS (SELECT TOP(1) UID FROM {0}.dbo.Inventory I where I.RoomType_UID = {1} and I.[Date] = '{2:yyyy-MM-dd}'))
            BEGIN
                SELECT COUNT(I.UID) FROM {0}.dbo.Inventory I
                INNER JOIN {0}.dbo.RoomTypes RT ON I.RoomType_UID = RT.UID
                LEFT JOIN {0}.dbo.RoomTypePeriods RTP ON RT.UID = RTP.RoomType_UID AND I.[Date] >= RTP.DateFrom AND I.[Date] <= RTP.DateTo
                WHERE RT.UID = {1} AND I.[Date] = '{2:yyyy-MM-dd}' AND 
                (ISNULL(RTP.QTY, RT.Qty) - I.QtyRoomOccupied) >= ({3} +
                CASE 
	                WHEN I.QtyContractedAllotmentTotal < I.QtyContractedAllotmentOccupied THEN 0 
	                ELSE I.QtyContractedAllotmentTotal - I.QtyContractedAllotmentOccupied
                END)
            END
            ELSE
            BEGIN
	            SELECT TOP 1 1 FROM {0}.dbo.RoomTypes RT
	            LEFT JOIN {0}.dbo.RoomTypePeriods RTP ON RT.UID = RTP.RoomType_UID AND '{2:yyyy-MM-dd}' >= RTP.DateFrom AND '{2:yyyy-MM-dd}' <= RTP.DateTo
	            WHERE RT.UID = {1} AND ISNULL(RTP.QTY, RT.Qty) >= {3}
            END";

        static readonly string UPSERT_INVENTORY_BATCH_QUERY = @"
            IF (EXISTS (SELECT TOP(1) UID FROM {0}.dbo.Inventory I where I.RoomType_UID = {1} and I.[Date] = '{2:yyyy-MM-dd}')) 
                BEGIN
                    UPDATE {0}.dbo.Inventory
		            SET QtyRoomOccupied += {3}, QtyContractedAllotmentOccupied += {4}
                    OUTPUT INSERTED.UID, INSERTED.[Date], INSERTED.RoomType_UID, INSERTED.QtyRoomOccupied, INSERTED.QtyContractedAllotmentTotal, INSERTED.QtyContractedAllotmentOccupied INTO @TmpInventory
		            WHERE RoomType_UID = {1} and [Date] = '{2:yyyy-MM-dd}'
                END
            ELSE 
                BEGIN
                    INSERT INTO {0}.dbo.Inventory (RoomType_UID, [Date], QtyRoomOccupied, QtyContractedAllotmentOccupied)
                    OUTPUT INSERTED.UID, INSERTED.[Date], INSERTED.RoomType_UID, INSERTED.QtyRoomOccupied, INSERTED.QtyContractedAllotmentTotal, INSERTED.QtyContractedAllotmentOccupied INTO @TmpInventory
		            VALUES ({1}, '{2:yyyy-MM-dd}', {3}, {4})
                END";

        static readonly string UPSERT_INVENTORY_BATCH_QUERY_WITH_VALIDATION = @"
            IF (EXISTS (SELECT TOP(1) UID FROM {0}.dbo.Inventory I where I.RoomType_UID = {1} and I.[Date] = '{2:yyyy-MM-dd}'))
            BEGIN
                UPDATE {0}.dbo.Inventory
                SET QtyRoomOccupied += {3}
                OUTPUT INSERTED.UID, INSERTED.[Date], INSERTED.RoomType_UID, INSERTED.QtyRoomOccupied, INSERTED.QtyContractedAllotmentTotal, INSERTED.QtyContractedAllotmentOccupied INTO @TmpInventory
                FROM {0}.dbo.Inventory I
                INNER JOIN {0}.dbo.RoomTypes RT ON I.RoomType_UID = RT.UID
                LEFT JOIN {0}.dbo.RoomTypePeriods RTP ON RT.UID = RTP.RoomType_UID AND I.[Date] >= RTP.DateFrom AND I.[Date] <= RTP.DateTo
                WHERE RT.UID = {1} AND I.[Date] = '{2:yyyy-MM-dd}' AND 
                (ISNULL(RTP.QTY, RT.Qty) - I.QtyRoomOccupied) >= ({3} +
                CASE 
	                WHEN I.QtyContractedAllotmentTotal < I.QtyContractedAllotmentOccupied THEN 0 
	                ELSE I.QtyContractedAllotmentTotal - I.QtyContractedAllotmentOccupied
                END)
            END
            ELSE
            BEGIN
                INSERT INTO {0}.dbo.Inventory (RoomType_UID, [Date], QtyRoomOccupied, QtyContractedAllotmentOccupied, QtyContractedAllotmentTotal)
                OUTPUT INSERTED.UID, INSERTED.[Date], INSERTED.RoomType_UID, INSERTED.QtyRoomOccupied, INSERTED.QtyContractedAllotmentTotal, INSERTED.QtyContractedAllotmentOccupied INTO @TmpInventory
                SELECT TOP 1 {1}, '{2:yyyy-MM-dd}', {3}, 0, 0
                FROM {0}.dbo.RoomTypes RT
                LEFT JOIN {0}.dbo.RoomTypePeriods RTP ON RT.UID = RTP.RoomType_UID AND '{2:yyyy-MM-dd}' >= RTP.DateFrom AND '{2:yyyy-MM-dd}' <= RTP.DateTo
                WHERE RT.UID = {1} AND ISNULL(RTP.QTY, RT.Qty) >= {3}
            END";

        #endregion ALLOTMENT And INVENTORY

        #region LOGGING

        public bool OldLogIsEnable()
        {
            var enableLogValue = RepositoryFactory.GetOBAppSettingRepository().ListSettings(new Contracts.Requests.ListSettingRequest { Names = new List<string> { "NewLogs_Properties" } }).FirstOrDefault();
            if (enableLogValue == null || !"All".Equals(enableLogValue.Value))
                return true;

            return false;
        }

        #region NEW LOG Reservation

        private void NewLogReservation(ServiceName reservationOperation, long userUID, string userName, string propertyName,
            contractsReservations.Reservation oldReservation, contractsReservations.Reservation newReservation, string channelName = null, long? propertyBaseCurrencyId = null, string requestId = null)
        {
            if ((reservationOperation != ServiceName.InsertReservation && oldReservation == null) || newReservation == null)
                return;
            contractsEvents.Messages.Notification notification = null;
            try
            {
                #region ASSOCIATED ENTITIES

                var associatedEntities = new Dictionary<string, List<long>>();
                associatedEntities.Add(contractsEvents.Enums.EntityEnum.Reservation.ToString(), new List<long>() { newReservation.UID }); // Reservation UID
                associatedEntities.Add(contractsEvents.Enums.EntityEnum.ReservationRooms.ToString(), newReservation.ReservationRooms.Select(x => x.UID).ToList()); // Cancelled Rooms UIDs
                associatedEntities.Add(contractsEvents.Enums.EntityEnum.Guest.ToString(), new List<long>() { newReservation.Guest_UID }); // Guest UID
                associatedEntities.Add(contractsEvents.Enums.EntityEnum.ReservationStatus.ToString(), new List<long>() { newReservation.Status }); // Reservation Status UID

                // Channel UID
                if (newReservation.Channel_UID.HasValue)
                    associatedEntities.Add(contractsEvents.Enums.EntityEnum.Channel.ToString(), new List<long>() { newReservation.Channel_UID.Value });

                // TPI UID
                if (newReservation.TPI_UID.HasValue)
                    associatedEntities.Add(contractsEvents.Enums.EntityEnum.TravelAgent.ToString(), new List<long>() { newReservation.TPI_UID.Value });

                // Cancelation Reason UID
                if (newReservation.CancelReservationReason_UID.HasValue)
                    associatedEntities.Add(contractsEvents.Enums.EntityEnum.CancelReservationReason.ToString(), new List<long>() { newReservation.CancelReservationReason_UID.Value });

                #endregion

                #region ENTITY DELTAS

                var entityDeltas = new List<contractsEvents.EntityDelta>();
                entityDeltas.Add(this.LoggingReservationDetails(oldReservation, newReservation, channelName, propertyBaseCurrencyId)); // Reservation Details

                // Get Deltas
                contractsEvents.Operations operation;
                switch (reservationOperation)
                {
                    case ServiceName.CancelReservation:
                        operation = (newReservation.Status == (long)Constants.ReservationStatus.Cancelled ||
                            newReservation.Status == (long)Constants.ReservationStatus.CancelledOnRequest ||
                            newReservation.Status == (long)Constants.ReservationStatus.CancelledPending ||
                            newReservation.Status == (long)Constants.ReservationStatus.OnRequestChannelCancel) ? contractsEvents.Operations.Delete : contractsEvents.Operations.Modify;
                        entityDeltas.AddRange(this.LoggingCancelReservation(oldReservation, newReservation));
                        break;

                    case ServiceName.UpdateReservation:
                    case ServiceName.ModifyReservation:
                        operation = contractsEvents.Operations.Modify;
                        entityDeltas.AddRange(this.LoggingModifyReservation(oldReservation, newReservation));
                        break;

                    case ServiceName.InsertReservation:
                        operation = contractsEvents.Operations.Create;
                        entityDeltas.AddRange(this.LoggingInsertReservation(newReservation));
                        break;

                    // CommitTransaction only changes the reservation status so log is equal to CancelReservation
                    case ServiceName.CommitTransaction:
                        operation = contractsEvents.Operations.Modify;
                        entityDeltas.AddRange(this.LoggingCancelReservation(oldReservation, newReservation));
                        break;

                    default: return;
                }

                #endregion

                // NOTIFICATION
                notification = new contractsEvents.Messages.Notification()
                {
                    EntityKey = newReservation.UID,
                    CreatedDate = DateTime.UtcNow,
                    Action = contractsEvents.Action.Reservation,
                    Operation = operation,
                    PropertyUID = newReservation.Property_UID,
                    PropertyName = propertyName,
                    Description = newReservation.Number,
                    CreatedBy = userUID,
                    CreatedByName = userName,
                    AssociatedEntities = associatedEntities,
                    EntityDeltas = contractsEvents.Helper.NotificationHelper.ClearNullOrEmptyEntityDeltas(entityDeltas) // clears null entity deltas
                };

                // Send log to worker for save on DB
                Resolve<IEventSystemManagerPOCO>().SendMessage(notification);
            }
            catch (Exception ex)
            {
                base.Logger.Error(ex, new LogMessageBase
                {
                    MethodName = nameof(NewLogReservation),
                    Description = internalErrors.Errors.SendNotificationError.ToString(),
                    Code = $"{(int)internalErrors.Errors.SendNotificationError}",
                    RequestId = requestId
                }, new LogEventPropertiesBase
                {
                    Request = notification,
                    OtherInfo = new Dictionary<string, object>
                    {
                        { "EventId", notification?.NotificationGuid },
                        { "EntityKey", newReservation.UID },
                        { "EntityName", contractsEvents.Action.Reservation.ToString()},
                        { "ReservationNumber", newReservation.Number }
                    }
                });
            }
        }

        // Gets minimum DateFrom(Checkin Date) and maximum DateTo(Checkout Date) of Reservation
        private bool GetReservationCheckinCheckoutDates(IList<contractsReservations.ReservationRoom> reservationRooms, out DateTime checkinDate, out DateTime checkoutDate)
        {
            checkinDate = new DateTime();
            checkoutDate = new DateTime();

            if (reservationRooms == null || reservationRooms.Count <= 0)
                return false;

            checkinDate = DateTime.MaxValue;
            checkoutDate = DateTime.MinValue;

            bool ignoreCancelledRooms = reservationRooms.Any(x =>
                x.Status != (int)Constants.ReservationStatus.Cancelled &&
                x.Status != (int)Constants.ReservationStatus.CancelledOnRequest &&
                x.Status != (int)Constants.ReservationStatus.OnRequestChannelCancel);

            foreach (var room in reservationRooms)
            {
                if (ignoreCancelledRooms &&
                    (room.Status == (int)Constants.ReservationStatus.Cancelled ||
                    room.Status == (int)Constants.ReservationStatus.CancelledOnRequest ||
                    room.Status == (int)Constants.ReservationStatus.OnRequestChannelCancel))
                    continue;

                if (checkinDate.CompareTo(room.DateFrom) > 0 && room.DateFrom.HasValue)
                    checkinDate = room.DateFrom.Value;

                if (checkoutDate.CompareTo(room.DateTo) < 0 && room.DateTo.HasValue)
                    checkoutDate = room.DateTo.Value;
            }

            return true;
        }

        // Gets the reservation room name
        private string GetReservationRoomName(contractsReservations.ReservationRoom reservationRoom)
        {
            return !string.IsNullOrEmpty(reservationRoom.RoomName) ?
                reservationRoom.RoomName : (reservationRoom.RoomType != null) ? reservationRoom.RoomType.Name : null;
        }

        // Get Reservation Details
        private contractsEvents.EntityDelta LoggingReservationDetails(contractsReservations.Reservation oldReservation, contractsReservations.Reservation newReservation, string channelName, long? propertyBaseCurrencyId)
        {
            var channelRepo = this.RepositoryFactory.GetOBChannelRepository();
            var currencyRepo = this.RepositoryFactory.GetOBCurrencyRepository();
            var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();

            var reservationDetailsEntities = new contractsEvents.Portable.Infrastructure.PropertyDictionary();
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.NumberOfRooms, newReservation.NumberOfRooms, oldReservation != null ? oldReservation.NumberOfRooms : null); // Number of Rooms
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsPaid, newReservation.IsPaid, oldReservation != null ? oldReservation.IsPaid : null); // Is paid
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancelReservationComments, newReservation.CancelReservationComments, oldReservation != null ? oldReservation.CancelReservationComments : null); // Cancellation Reason
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationPolicy, newReservation.CancellationPolicy, oldReservation != null ? oldReservation.CancellationPolicy : null); // Cancellation Policy
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CancellationPolicyDays, newReservation.CancellationPolicyDays, oldReservation != null ? oldReservation.CancellationPolicyDays : null); // Cancellation Policy Days

            // Checkin and Checkout Dates
            DateTime newCheckin, newCheckout, oldCheckin, oldCheckout;
            this.GetReservationCheckinCheckoutDates(oldReservation != null ? oldReservation.ReservationRooms : null, out oldCheckin, out oldCheckout);
            this.GetReservationCheckinCheckoutDates(newReservation.ReservationRooms, out newCheckin, out newCheckout);
            DateTime newCheckinValue = newCheckin;
            DateTime newCheckoutValue = newCheckout;

            // If Reservation Is Cancelled Use Old Checkin and Checkout
            if (newReservation.Status == (int)Constants.ReservationStatus.Cancelled ||
                    newReservation.Status == (int)Constants.ReservationStatus.CancelledOnRequest ||
                    newReservation.Status == (int)Constants.ReservationStatus.OnRequestChannelCancel)
            {
                newCheckinValue = oldCheckin;
                newCheckoutValue = oldCheckout;
            }

            DateTime? oldCheckinTemp = null;
            DateTime? oldCheckoutTemp = null;
            if (oldCheckin != DateTime.MinValue)
                oldCheckinTemp = oldCheckin;
            if (oldCheckout != DateTime.MinValue)
                oldCheckinTemp = oldCheckout;
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DateFrom, newCheckinValue.Date, oldCheckinTemp.HasValue ? oldCheckinTemp.Value.Date : oldCheckinTemp); // Reservation Checkin
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DateTo, newCheckoutValue.Date, oldCheckoutTemp.HasValue ? oldCheckoutTemp.Value.Date : oldCheckoutTemp); // Reservation Checkout

            // Channel Name
            if (string.IsNullOrEmpty(channelName))
                channelName = newReservation.Channel_UID.HasValue && newReservation.Channel_UID > 0 ?
                    channelRepo.ListChannel(new Contracts.Requests.ListChannelRequest
                    {
                        ChannelIds = new List<long> { newReservation.Channel_UID.Value },
                        EntityStates = Contracts.Requests.Base.EntityStates.Active | Contracts.Requests.Base.EntityStates.InActive,
                        Fields = new HashSet<Contracts.Data.Channels.ChannelFields> { Contracts.Data.Channels.ChannelFields.Name }
                    }).FirstOrDefault()?.Name : string.Empty;

            if (!string.IsNullOrEmpty(channelName))
                reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.ChannelName, channelName, channelName);

            // Guest Name
            string oldGuestName = oldReservation != null ? oldGuestName = (string.IsNullOrEmpty(oldReservation.GuestFirstName) && oldReservation.Guest != null) ?
                        string.Format("{0} {1}", oldReservation.Guest.FirstName, oldReservation.Guest.LastName) :
                        string.Format("{0} {1}", oldReservation.GuestFirstName, oldReservation.GuestLastName) : null;
            string newGuestName = (string.IsNullOrEmpty(newReservation.GuestFirstName) && newReservation.Guest != null) ?
                    string.Format("{0} {1}", newReservation.Guest.FirstName, newReservation.Guest.LastName) :
                    string.Format("{0} {1}", newReservation.GuestFirstName, newReservation.GuestLastName);
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.GuestName, newGuestName, oldGuestName);

            // Reservation Status
            long? status = null;
            if (oldReservation != null)
                status = oldReservation.Status;
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Status
                , Enum.GetName(typeof(Constants.ReservationStatus), newReservation.Status)
                , status.HasValue ? Enum.GetName(typeof(Constants.ReservationStatus), status) : null);

            // Reservation Total Amount
            if (!propertyBaseCurrencyId.HasValue)
                propertyBaseCurrencyId = propertyRepo.ListProperties(new Contracts.Requests.ListPropertiesRequest
                {
                    Ids = new List<long> { newReservation.Property_UID },
                    EntityStates = Contracts.Requests.Base.EntityStates.Active | Contracts.Requests.Base.EntityStates.InActive,
                    Fields = new HashSet<contractsProperties.ListPropertyFields> { contractsProperties.ListPropertyFields.PropertySettings_BaseCurrencyId }
                }).FirstOrDefault()?.PropertySettings?.BaseCurrencyId;

            OB.BL.Contracts.Data.General.Currency currency = null;
            if (propertyBaseCurrencyId.HasValue)
                currency = currencyRepo.ListCurrencies(new Contracts.Requests.ListCurrencyRequest { UIDs = new List<long> { propertyBaseCurrencyId.Value } }).FirstOrDefault();
            var oldReservationTotal = oldReservation != null ? ((currency != null) ? string.Format("{0:0.00} {1}({2})", oldReservation.TotalAmount, currency.CurrencySymbol, currency.Symbol) : Convert.ToString(oldReservation.TotalAmount)) : null;
            var newReservationTotal = (currency != null) ? string.Format("{0:0.00} {1}({2})", newReservation.TotalAmount, currency.CurrencySymbol, currency.Symbol) : Convert.ToString(newReservation.TotalAmount);
            reservationDetailsEntities.AddProperty(contractsEvents.Enums.EntityPropertyEnum.TotalReservationAmount, newReservationTotal, oldReservationTotal); // Total Amount

            var reservationDetailsDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.Reservation)
            {
                EntityKey = newReservation.UID,
                EntityProperties = reservationDetailsEntities
            };

            var privilegedProperties = new List<EntityPropertyEnum>()
            {
                EntityPropertyEnum.Name,
                EntityPropertyEnum.DateFrom,
                EntityPropertyEnum.DateTo,
                EntityPropertyEnum.GuestName,
                EntityPropertyEnum.Status,
                EntityPropertyEnum.ChannelName,
                EntityPropertyEnum.TotalReservationAmount
            };
            return contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(reservationDetailsDelta, privilegedProperties);
        }

        #endregion

        #region RESERVATION

        private void SaveReservationHistory(ReservationLogRequest request)
        {

            if (request == null || request.Reservation == null)
                return;

            try
            {
                using (var unitOfWork = this.SessionFactory.GetUnitOfWork(ReservationStatus.DomainScope))
                {
                    var crmRepo = this.RepositoryFactory.GetOBCRMRepository();
                    var channelsRepo = this.RepositoryFactory.GetOBChannelRepository();
                    var propertyRepo = this.RepositoryFactory.GetOBPropertyRepository();
                    var reservationHistoryRepo = this.RepositoryFactory.GetReservationHistoryRepository(unitOfWork);

                    if (string.IsNullOrEmpty(request.propertyName))
                        request.propertyName = propertyRepo.ListProperties(new Contracts.Requests.ListPropertiesRequest
                        {
                            Ids = new List<long> { request.Reservation.Property_UID },
                            EntityStates = Contracts.Requests.Base.EntityStates.Active | Contracts.Requests.Base.EntityStates.InActive,
                            Fields = new HashSet<contractsProperties.ListPropertyFields> { contractsProperties.ListPropertyFields.PropertySettings_Name }
                        }).FirstOrDefault()?.PropertySettings?.Name;

                    ReservationLoggingMessage log = new ReservationLoggingMessage();
                    log.propertyID = request.Reservation.Property_UID;
                    log.propertyName = request.propertyName;
                    log.MessageDate = DateTime.UtcNow;
                    log.ServiceName = request.ServiceName;
                    log.GroupRule = request.GroupRule;

                    if (request.Reservation.Guest == null)
                    {
                        var guestOb = request.Guest != null ? request.Guest : crmRepo.ListGuestsByLightCriteria(new Contracts.Requests.ListGuestLightRequest { UIDs = new List<long> { request.Reservation.Guest_UID } }).FirstOrDefault();
                        request.Reservation.Guest = OtherConverter.Convert(guestOb);
                    }

                    if (request.UserID > 0 && !request.UserIsGuest)
                    {
                        var user = crmRepo.ListUsers(new Contracts.Requests.ListUserRequest { UIDs = new List<long> { request.UserID } }).FirstOrDefault();
                        if (user != null)
                        {
                            log.UserName = "user : " + user.UserName;
                            log.UserID = request.UserID;
                        }
                    }
                    else
                    {
                        if (request.Reservation.Guest != null)
                        {
                            log.UserName = "guest:" + request.Reservation.Guest.FirstName + ' ' + request.Reservation.Guest.LastName + ' ' + request.Reservation.Guest.Email;
                            log.UserID = request.Reservation.Guest.UID;
                        }
                        else
                            log.UserName = "guest:" + request.Reservation.GuestFirstName + ' ' + request.Reservation.GuestLastName + ' ' + request.Reservation.GuestEmail;
                    }

                    log.CorrelationID = !string.IsNullOrEmpty(request.CorrelationID) ? request.CorrelationID : Guid.NewGuid().ToString();
                    log.Operation = request.Operation;
                    log.MessageType = request.MessageType;
                    log.UserHostAddress = request.UserHostAddress;
                    log.BigPullAuthRequestor_UID = request.BigPullAuthRequestor_UID;

                    log.objReservation = request.Reservation;

                    if (request.GuestActivity != null)
                        log.objGuestActivity = request.GuestActivity;

                    if (request.ReservationAdditionalDataUID.HasValue)
                        log.ReservationAdditionalDataUID = request.ReservationAdditionalDataUID;

                    if (request.ReservationAdditionalData != null)
                        log.ReservationAdditionalData = request.ReservationAdditionalData;

                    // Clears Credit Card info
                    if (log.objReservationPaymentDetail != null)
                    {
                        log.objReservationPaymentDetail.CardNumber = null;
                        log.objReservationPaymentDetail.ExpirationDate = null;
                    }
                    if (log.objReservation != null && log.objReservation.ReservationPaymentDetail != null)
                    {
                        log.objReservation.ReservationPaymentDetail.CardNumber = null;
                        log.objReservation.ReservationPaymentDetail.ExpirationDate = null;
                    }

                    string xmlLog = JsonConvert.SerializeObject(log, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

                    if (!string.IsNullOrEmpty(xmlLog))
                    {
                        if (string.IsNullOrEmpty(request.ChannelName) && log.objReservation.Channel_UID.HasValue)
                            request.ChannelName = channelsRepo.ListChannel(new Contracts.Requests.ListChannelRequest
                            {
                                ChannelIds = new List<long> { log.objReservation.Channel_UID.Value },
                                EntityStates = Contracts.Requests.Base.EntityStates.Active | Contracts.Requests.Base.EntityStates.InActive,
                                Fields = new HashSet<Contracts.Data.Channels.ChannelFields> { Contracts.Data.Channels.ChannelFields.Name }
                            }).FirstOrDefault()?.Name;

                        string status = ((Constants.ReservationStatus)request.Reservation.Status).ToString();

                        #region SAVE RESERVATION HISTORY

                        ReservationsHistory hist = new ReservationsHistory();

                        hist.ChangedDate = DateTime.UtcNow;
                        hist.Channel = request.ChannelName;
                        hist.Message = xmlLog;
                        hist.ReservationNumber = request.Reservation.Number;
                        hist.ReservationUID = request.Reservation.UID;
                        hist.Status = status;
                        hist.StatusUID = request.Reservation.Status;
                        hist.UserName = log.UserName;
                        hist.UserUID = log.UserID;
                        hist.ChannelUID = (int)request.Reservation.Channel_UID;
                        hist.HangfireId = request.HangfireId;
                        hist.Retries = request.TransactionRetries;

                        hist.TransactionUID = string.IsNullOrEmpty(request.ReservationTransactionId)
                                                    ? (request.Reservation.Channel_UID == 1 ? request.Reservation.Number : Guid.NewGuid().ToString())
                                                    : request.ReservationTransactionId;

                        hist.TransactionState = (int)request.ReservationTransactionState;
                        reservationHistoryRepo.Add(hist);

                        #endregion SAVE RESERVATION HISTORY

                        unitOfWork.Save();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, new Log.Messages.LogMessageBase
                {
                    MethodName = nameof(SaveReservationHistory),
                    Description = $"There were errors to save reservation history - ReservationId: {request.Reservation.UID}",
                });

                var arguments = new Dictionary<string, object>();
                arguments.Add("serviceName", request.ServiceName);
                arguments.Add("objguest", request.Reservation.Guest);
                arguments.Add("objReservation", request.Reservation);
                arguments.Add("objGuestActivity", request.GuestActivity);
                arguments.Add("UserHostAddress", request.UserHostAddress);
                arguments.Add("userID", request.UserID);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments);
            }
        }

        private void LoggingInformation(ServiceName serviceName, ReservationDataContext reservationContext, domainReservations.Reservation reservation, contractsCRM.Guest objguest,
            contractsReservations.Reservation objReservation, List<long> objGuestActivity, List<contractsReservations.ReservationRoom> objReservationRoom,
            List<contractsReservations.ReservationRoomDetail> objReservationRoomDetail, List<contractsReservations.ReservationRoomExtra> objReservationRoomExtras,
            List<contractsReservations.ReservationRoomChild> objReservationRoomChild, contractsReservations.ReservationPaymentDetail objReservationPaymentDetail,
            List<contractsReservations.ReservationRoomExtrasSchedule> objReservationExtraSchedule, string UserHostAddress, long userID,
            string transactionUID, Constants.ReservationTransactionStatus transactionState = Constants.ReservationTransactionStatus.Commited,
            List<contractsReservations.ReservationRoomExtrasAvailableDate> objReservationRoomExtrasAvailableDates = null)
        {
            try
            {
                var unitOfWork = this.SessionFactory.GetUnitOfWork(ReservationStatus.DomainScope);

                var reservationStatusRepo = this.RepositoryFactory.GetReservationStatusRepository(unitOfWork);
                var crmRepo = this.RepositoryFactory.GetOBCRMRepository();

                var propUID = objReservation != null ? objReservation.Property_UID : reservation != null ? reservation.Property_UID : default(long);
                var propName = reservationContext != null ? reservationContext.PropertyName ?? string.Empty : string.Empty;

                List<ReservationStatus> statusList = reservationStatusRepo.GetAll().ToList();

                ReservationLoggingMessage log = new ReservationLoggingMessage();

                log.propertyID = propUID;
                log.propertyName = propName;
                log.MessageDate = DateTime.UtcNow;
                log.ServiceName = serviceName;

                if (objguest != null)
                    userID = objguest.UID;

                if (userID > 0)
                {
                    //if is 1 is from be admin
                    if (userID != 1 && objguest == null)
                    {
                        var user = crmRepo.ListUsers(new Contracts.Requests.ListUserRequest { UIDs = new List<long> { userID } }).FirstOrDefault();
                        if (null != user)
                        {
                            log.UserName = "user : " + user.UserName;
                            log.UserID = userID;
                        }
                    }
                    else
                    {
                        if (null != reservation)
                        {
                            if (objguest != null)
                                log.UserName = "guest:" + objguest.FirstName + ' ' + objguest.LastName + ' ' + objguest.Email;
                            else
                                log.UserName = "guest:" + reservation.GuestFirstName + ' ' + reservation.GuestLastName + ' ' + reservation.GuestEmail;

                            log.UserID = userID;
                        }
                    }
                }
                else
                {
                    log.UserName = "admin user";
                    log.UserID = 65;//admin user
                }
                log.CorrelationID = Guid.NewGuid().ToString();
                log.Operation = Operation.Update;
                log.MessageType = TypeMessage.ReservationMessage;
                log.UserHostAddress = UserHostAddress;

                if (objguest != null)
                    log.objguest = objguest;

                if (reservation != null)
                    log.objReservation = DomainToBusinessObjectTypeConverter.Convert(reservation);

                if (objGuestActivity != null)
                    log.objGuestActivity = objGuestActivity;

                if (objReservationRoom != null)
                    log.objReservationRoom = objReservationRoom;

                if (objReservationRoomDetail != null)
                    log.objReservationRoomDetail = objReservationRoomDetail;

                if (objReservationRoomExtras != null)
                    log.objReservationRoomExtras = objReservationRoomExtras;

                if (objReservationRoomChild != null)
                    log.objReservationRoomChild = objReservationRoomChild;

                if (objReservationPaymentDetail != null)
                    log.objReservationPaymentDetail = objReservationPaymentDetail;

                if (objReservationExtraSchedule != null)
                    log.objReservationExtraSchedule = objReservationExtraSchedule;

                if (objReservationRoomExtrasAvailableDates != null)
                    log.objReservationRoomExtrasAvailableDates = objReservationRoomExtrasAvailableDates;

                // Clears Credit Card info
                if (log.objReservationPaymentDetail != null)
                {
                    log.objReservationPaymentDetail.CardNumber = null;
                    log.objReservationPaymentDetail.ExpirationDate = null;
                }
                if (log.objReservation != null && log.objReservation.ReservationPaymentDetail != null)
                {
                    log.objReservation.ReservationPaymentDetail.CardNumber = null;
                    log.objReservation.ReservationPaymentDetail.ExpirationDate = null;
                }

                string xmlLog = JsonConvert.SerializeObject(log);

                if (!string.IsNullOrEmpty(xmlLog))
                {
                    if (null != log.objReservation)
                    {
                        var channelsRepo = this.RepositoryFactory.GetOBChannelRepository();

                        string channel = (log.objReservation.Channel_UID.HasValue) ? channelsRepo.ListChannelLight(new Contracts.Requests.ListChannelLightRequest { ChannelUIDs = new List<long> { log.objReservation.Channel_UID.Value } }).First().Name : string.Empty;

                        ReservationStatus rstatus = statusList.Where(s => s.UID == log.objReservation.Status).SingleOrDefault();

                        string status = (rstatus != null && !string.IsNullOrEmpty(rstatus.Name)) ? rstatus.Name : "?";

                        #region SAVE RESERVATION HISTORY

                        ReservationsHistory hist = new ReservationsHistory();

                        hist.ChangedDate = DateTime.UtcNow;
                        hist.Channel = channel;
                        hist.Message = xmlLog;
                        hist.ReservationNumber = reservation.Number;
                        hist.ReservationUID = reservation.UID;
                        hist.Status = status;
                        hist.StatusUID = reservation.Status;
                        hist.UserName = log.UserName;
                        hist.UserUID = log.UserID;
                        hist.ChannelUID = (int)reservation.Channel_UID;
                        hist.TransactionUID = transactionUID;
                        hist.TransactionState = (int)transactionState;

                        var reservationHistoryRepo = this.RepositoryFactory.GetRepository<ReservationsHistory>(unitOfWork);

                        reservationHistoryRepo.Add(hist);

                        #endregion SAVE RESERVATION HISTORY

                        unitOfWork.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                var arguments = new Dictionary<string, object>();
                arguments.Add("serviceName", serviceName);
                arguments.Add("objguest", objguest);
                arguments.Add("objReservation", objReservation);
                arguments.Add("objGuestActivity", objGuestActivity);
                arguments.Add("objReservationRoom", objReservationRoom);
                arguments.Add("objReservationRoomDetail", objReservationRoomDetail);
                arguments.Add("objReservationRoomExtras", objReservationRoomExtras);
                arguments.Add("objReservationRoomChild", objReservationRoomChild);
                arguments.Add("objReservationPaymentDetail", objReservationPaymentDetail);
                arguments.Add("objReservationExtraSchedule", objReservationExtraSchedule);
                arguments.Add("UserHostAddress", UserHostAddress);
                arguments.Add("userID", userID);
                this.Resolve<ILogEmail>().SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, LogSeverity.High, arguments);
            }
        }

        #endregion RESERVATION

        #region LOG Lost Reservation
        private void LogLostReservation(ServiceName reservationOperation, string userName, contractsReservations.LostReservation lostReservation, string requestId)
        {
            if (reservationOperation != ServiceName.LostReservation || lostReservation == null)
                return;

            try
            {
                // NOTIFICATION
                var notification = new contractsEvents.Messages.Notification()
                {
                    EntityKey = lostReservation.UID,
                    CreatedDate = DateTime.UtcNow,
                    Action = contractsEvents.Action.LostReservation,
                    Operation = contractsEvents.Operations.Create,
                    PropertyUID = lostReservation.Property_UID,
                    CreatedByName = userName,
                    RequestId = requestId,
                };

                // Send log to worker for save on DB
                Resolve<IEventSystemManagerPOCO>().SendMessage(notification);
            }
            catch (Exception ex)
            {
                base.Logger.Error(ex, new LogMessageBase
                {
                    MethodName = nameof(LogLostReservation),
                    RequestId = requestId,
                    Description = "Error to send the lost reservation message"
                });
            }
        }

        #endregion LOG Lost Reservation

        #endregion LOGGING

        #region MyAccount
        public ListMyAccountReservationsOverviewResponse ListMyAccountReservationsOverview(ListMyAccountReservationsOverviewRequest request)
        {
            var response = new ListMyAccountReservationsOverviewResponse();
            response.RequestGuid = request.RequestGuid;
            response.RequestId = request.RequestId;

            try
            {
                if (request.UserUID <= 0 || request.UserType <= 0)
                {
                    response.Errors = new List<Error> { new Error { Description = OB.BL.Contracts.Responses.ErrorType.InvalidRequest } };
                    response.Failed();
                    return response;
                }

                using (var unitOfWork = this.SessionFactory.GetUnitOfWork())
                {
                    var reservationsRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);
                    response.Result = reservationsRepo.ListMyAccountReservationsOverview(request.UserUID, request.UserType, request.DateFrom, request.DateTo).Select(s => QueryResultObjectToBusinessObjectTypeConverter.Convert(s)).ToList();
                }

                response.Succeed();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new OB.Reservation.BL.Contracts.Responses.Error(ex));
            }

            return response;
        }
        #endregion

        public ApproveOrRefuseOnRequestReservationResponse ApproveOrRefuseOnRequestReservation(Reservation.BL.Contracts.Requests.ApproveOrRefuseOnRequestReservationRequest request)
        {
            var response = new ApproveOrRefuseOnRequestReservationResponse(request);
            try
            {
                var requestModify = new Reservation.BL.Contracts.Requests.ModifyReservationRequest();
                requestModify.ChannelId = request.ChannelId ?? 0;
                requestModify.RuleType = Reservation.BL.Constants.RuleType.Omnibees;
                requestModify.ReservationUid = request.ReservationId;
                requestModify.TransactionId = request.TransactionId;
                requestModify.TransactionType = Reservation.BL.Constants.ReservationTransactionType.A;
                requestModify.UserId = request.UserId;

                var channelsRepo = RepositoryFactory.GetOBChannelRepository();
                int? channelType = channelsRepo.ListChannel(new ListChannelRequest
                {
                    ChannelIds = new List<long> { request.ChannelId ?? 0 },
                    Fields = new HashSet<OB.BL.Contracts.Data.Channels.ChannelFields> {
                        OB.BL.Contracts.Data.Channels.ChannelFields.Type
                    }
                }).Select(c => c.Type).FirstOrDefault();

                if (channelType == null)
                    throw Errors.InvalidChannel.ToBusinessLayerException((request.ChannelId ?? 0).ToString());

                if (channelType.Value == (int)Constants.ChannelType.Pull)
                {
                    requestModify.TransactionAction = (request.IsApprove ?? false) ? Reservation.BL.Constants.ReservationTransactionAction.Commit :
                                                                            Reservation.BL.Constants.ReservationTransactionAction.Ignore;
                    var rsp = ReservationCoordinator(requestModify, Reservation.BL.Constants.ReservationAction.Modify);

                    response.Warnings = rsp.Warnings;
                    response.Status = rsp.Status;
                    response.Errors = rsp.Errors;
                    if (response.Status != Status.Success && response.Errors.Any())
                    {
                        response.Failed();
                        return response;
                    }
                }
                else
                {
                    if (request.IsApprove ?? false)
                    {
                        requestModify.TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Commit;
                        var rsp = ReservationCoordinator(requestModify, Reservation.BL.Constants.ReservationAction.Modify);

                        response.Warnings = rsp.Warnings;
                        response.Status = rsp.Status;
                        response.Errors = rsp.Errors;

                        if (response.Status != Status.Success && response.Errors.Any())
                        {
                            response.Failed();
                            return response;
                        }
                    }
                    else
                    {
                        var rspCancel = CancelReservation(new CancelReservationRequest
                        {
                            ReservationUID = request.ReservationId,
                            UserId = request.UserId,
                            ChannelId = request.ChannelId ?? 0,
                            TransactionId = request.TransactionId,
                            OBUserType = request.OBUserType,
                            UserType = (int)request.UserId
                        });

                        response.Warnings = rspCancel.Warnings;
                        response.Status = rspCancel.Status;
                        response.Errors = rspCancel.Errors;

                        if (response.Status != Status.Success && response.Errors.Any())
                        {
                            response.Failed();
                            return response;
                        }
                    }
                }
                response.Succeed();
            }
            catch (BusinessLayerException e)
            {
                Logger.Error(e, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new Error(e.ErrorType, e.ErrorCode, e.Message));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "REQUEST:{0}", request.ToJSON());
                response.Failed();
                response.Errors.Add(new Error(ex));
            }
            return response;
        }


        public async Task<ValidateReservationRestricionsResponse> ValidateReservationRestrictions(Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest request)
        {
            var reservationValidator = this.Resolve<IReservationValidatorPOCO>();

            var response = new ValidateReservationRestricionsResponse();

            try
            {
                ValidateReservationRestrictionsRequest(request, response);

                if (response.Errors.Count == 0)
                {
                    var ratesRepo = RepositoryFactory.GetOBRateRoomDetailsForReservationRoomRepository();

                    response.Result = false;

                    foreach (var req in request.Items)
                    {
                        var rateRoomList = await ratesRepo.ListRateRoomDetailsAsync(OtherConverter.Convert(req, request.PropertyId));

                        var totalDays = (req.CheckOut - req.CheckIn).Days + 1;

                        if (rateRoomList.Count == totalDays)
                        {
                            foreach (var rateRoom in rateRoomList)
                            {
                                if (reservationValidator.ValidateReservationRestrictions(rateRoom, req.CheckIn, req.CheckOut, request.ChannelId))
                                    response.Result = true;
                                else
                                {
                                    response.Result = false;
                                    break;
                                }
                            }
                        }
                    }
                    response.Succeed();
                }
                else
                {
                    response.Failed();
                }

            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new Error(ex));
            }

            return response;
        }

        private decimal GetValueWithInterestRate(decimal value, domainReservations.Reservation reservation, contractsReservations.ReservationPaymentDetail objReservationPaymentDetail, bool IsRefund = false)
        {
            if (!(reservation.IsPartialPayment.GetValueOrDefault() && reservation.NoOfInstallment.HasValue))
                return value; //If this reservation is not partial payment, just return the current value

            decimal interestRate = reservation.InterestRate ?? 0;

            if (!IsRefund)
            {
                if ((int)OB.Reservation.BL.Constants.PaymentMethodTypesCode.CreditCard != reservation.PaymentMethodType_UID)
                    throw new NotImplementedException("At the date of this feature, no parcelation could be done with types different of 'Credit Card'.");

                if (objReservationPaymentDetail == null)
                    throw new BusinessLayerException("Cannot found a ReservationPaymentDetail associated to this Reservation");

                using (IUnitOfWork unitOfWork = this.SessionFactory.GetUnitOfWork(true))
                {
                    IOBBeSettingsRepository obBeSettings = RepositoryFactory.GetOBBeSettingsRepository();
                    var obBeSettingsResponse = obBeSettings.ListBeSettings(new ListBeSettingsRequest
                    {
                        PropertyIds = new List<long> { reservation.Property_UID }
                    });

                    if (obBeSettingsResponse.Status == Contracts.Responses.Status.Fail || obBeSettingsResponse.Result?.Any() != true)
                        throw new Exception($"Failed to obtained Partial Payment Minimum Allowed from OB API. Cannot apply these rates to the reservation {reservation.UID}" +
                                            $", Property {reservation.Property_UID}, Payment Method Type {reservation.PaymentMethodType_UID}" +
                                            $" and Parcels {reservation.NoOfInstallment.Value}");

                    var setting = obBeSettingsResponse.Result.SingleOrDefault();
                    if (setting.PartialPaymentMinimumAllowed > value)
                        throw new BusinessLayerException($"Reservation value {value} is lower than the one set on for the partial payment {setting.PartialPaymentMinimumAllowed} for this property {reservation.Property_UID}.");

                    if (!setting.AllowPartialPayment)
                        throw new BusinessLayerException($"Property {reservation.Property_UID} is not allowing Partial Payment.");



                    IOBBePartialPaymentCcMethodRepository bePPCMRepository = RepositoryFactory.GetOBBePartialPaymentCcMethodRepository();
                    Contracts.Responses.ListBePartialPaymentCcMethodResponse response = bePPCMRepository.ListBePartialPaymentCcMethods(new ListBePartialPaymentCcMethodRequest
                    {
                        PropertyIds = new List<long> { reservation.Property_UID },
                        PaymentMethodsIds = new List<long?> { objReservationPaymentDetail.PaymentMethod_UID.Value },
                        Parcels = new List<int> { reservation.NoOfInstallment.Value }
                    });

                    if (response.Status == Contracts.Responses.Status.Fail || response.Result?.Any() != true)
                        throw new Exception($"Failed to obtained interest rates from OB API. Cannot apply these rates to the reservation {reservation.UID}" +
                                            $", Property {reservation.Property_UID}, Payment Method Type {reservation.PaymentMethodType_UID}" +
                                            $" and Parcels {reservation.NoOfInstallment.Value}");

                    Contracts.Data.Payments.BePartialPaymentCcMethod result = response.Result.SingleOrDefault();

                    if (result.InterestRate != reservation.InterestRate)
                        throw new BusinessLayerException($"Property configured interest rate for NoOfInstallment with value {reservation.NoOfInstallment.Value} is not equal to the one received from the reservation ({reservation.InterestRate}).");

                    if (result.PartialPaymentMinimumValue.HasValue && value < result.PartialPaymentMinimumValue)
                        throw new BusinessLayerException($"Reservation does not meet the minimum requirement set by this property. Cannot apply this rate to the reservation {reservation.UID}.");

                    if (!result.InterestRate.HasValue)
                        throw new BusinessLayerException($"Interest rate obtained from OB API has no value. Cannot apply this interest rate to the reservation {reservation.UID}.");

                    interestRate = result.InterestRate.Value;
                    reservation.InterestRate = interestRate;
                }
            }

            //Calculate the Rates for Partial Payments - US 51901
            var tax = value * interestRate / (decimal)100;
            return value + tax;
        }

        private void ValidateReservationRestrictionsRequest(Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest request, ValidateReservationRestricionsResponse response)
        {
            if (request.PropertyId == 0)
                response.Errors.Add(Errors.RequiredParameter.ToContractError(nameof(request.PropertyId)));

            foreach (var item in request.Items)
            {
                if (item.CheckIn == new DateTime())
                    response.Errors.Add(Errors.RequiredParameter.ToContractError(nameof(item.CheckIn)));
                if (item.CheckOut == new DateTime())
                    response.Errors.Add(Errors.RequiredParameter.ToContractError(nameof(item.CheckOut)));
                if (item.RateId == 0)
                    response.Errors.Add(Errors.RequiredParameter.ToContractError(nameof(item.RateId)));
                if (item.RoomTypeId == 0)
                    response.Errors.Add(Errors.RequiredParameter.ToContractError(nameof(item.RoomTypeId)));
                if (item.CheckIn != new DateTime() && item.CheckOut != new DateTime() && item.CheckOut < item.CheckIn)
                    response.Errors.Add(Errors.StartDateExceedsEndDate.ToContractError(item.CheckIn.ToString("yyyy-MM-dd"), item.CheckOut.ToString("yyyy-MM-dd")));
            }
        }

        public ListReservationStatusesResponse ListReservationStatuses(Reservation.BL.Contracts.Requests.ListReservationStatusesRequest request)
        {
            var response = new ListReservationStatusesResponse();

            if (request == null)
            {
                response.Errors.Add(Errors.NoRequest.ToContractError());
                response.Failed();
                return response;
            }

            response.RequestId = request.RequestId;

            using (var unitOfWork = SessionFactory.GetUnitOfWork(true))
            {
                var repo = RepositoryFactory.GetReservationStatusRepository(unitOfWork);
                var criteria = new ListReservationStatusCriteria
                {
                    IncludeLanguages = request.LanguageUID > 0,
                    UIDs = request.Ids
                };

                var statuses = repo.FindByCriteria(criteria);

                response.Result = statuses.Select(x => DomainToBusinessObjectTypeConverter.Convert(x, request.LanguageUID)).ToList();

                response.TotalRecords = statuses.Count();
            }

            response.Succeed();
            return response;
        }

        //public ReservationDataContext GetReservationContext(string existingReservationNumber, long channelUID, long tpiUID, long companyUID, long propertyUID,
        //    long reservationUID, long currencyUID, long paymentMethodTypeUID, IEnumerable<long> rateUIDs, string guestFirstName,
        //    string guestLastName, string guestEmail, string guestUsername, long? languageId, string requestId)
        //{
        //    using (var unitOfWork = SessionFactory.GetUnitOfWork(true))
        //    {
        //        var omnibeesRepo = RepositoryFactory.GetOmnibeesSqlRepository(unitOfWork);
        //        var reservationsFilterRepo = RepositoryFactory.GetReservationsFilterRepository(unitOfWork);
        //        var rateRepo = RepositoryFactory.GetOBRateRepository();

        //        var resultTask = omnibeesRepo.GetReservationContext(existingReservationNumber, channelUID, tpiUID,
        //                        companyUID, propertyUID, reservationUID, currencyUID,
        //                        paymentMethodTypeUID,
        //                        rateUIDs,
        //                        guestFirstName, guestLastName, guestEmail, guestUsername, languageId, requestId);

        //        var request = new ListReservationsFilterRequest
        //        {
        //            ChannelUIDs = new List<long> { channelUID },
        //            ReservationNumbers = new List<string> { existingReservationNumber },
        //            PropertyUIDs = new List<long> { propertyUID }
        //        };

        //        var convertedFilterRequest = new ListReservationFilterCriteria();
        //        OtherConverter.Map(request, convertedFilterRequest);
        //        var existingReservationTask = reservationsFilterRepo.FirstOrDefaultAsync(x => x.ChannelUid == channelUID
        //                                    && x.Number == existingReservationNumber && x.PropertyUid == propertyUID);

        //        // Get rates availability type
        //        Task rateAvailabilityTask = null;
        //        Dictionary<long, int> ratesAvailabilityTypes = null;
        //        if (!rateUIDs.IsNullOrEmpty())
        //        {
        //            var availabilityRequest = new ListRateAvailabilityTypeRequest { RatesUIDs = rateUIDs.Distinct().ToList(), RequestId = requestId };
        //            rateAvailabilityTask = StartConcurrentWork(() => ratesAvailabilityTypes = rateRepo.ListRatesAvailablityType(availabilityRequest));
        //        }

        //        if (rateAvailabilityTask == null)
        //            Task.WaitAll(resultTask, existingReservationTask);
        //        else
        //            Task.WaitAll(resultTask, existingReservationTask, rateAvailabilityTask);

        //        var result = resultTask.Result;
        //        result.IsExistingReservation = existingReservationTask?.Result != null ? true : false;
        //        result.RatesAvailabilityType = ratesAvailabilityTypes ?? new Dictionary<long, int>();
        //        result.ReservationUID = result.IsExistingReservation ? existingReservationTask.Result.UID : reservationUID;

        //        return result;
        //    }
        //}

        internal bool CheckIfPropertyNeedsToCheckAvailability(long propertyId, string requestId)
        {
            bool checkAvail = false;

            if (propertyId > 0)
            {
                var propertyRepo = RepositoryFactory.GetOBPropertyRepository();

                var requestToCheckAvail = new ListPropertiesRequest()
                {
                    Fields = new HashSet<contractsProperties.ListPropertyFields> { contractsProperties.ListPropertyFields.IsToPreCheckAvail },
                    Ids = new List<long> { propertyId },
                    RequestId = requestId,
                    ReturnTotal = true
                };

                var responseToCheckAvail = propertyRepo.ListProperties(requestToCheckAvail).FirstOrDefault();

                if (responseToCheckAvail != null)
                    checkAvail = responseToCheckAvail.IsToPreCheckAvail;
            }

            return checkAvail;
        }

        internal bool CheckIfPropertyHasBookingServiceActive(long propertyId, string requestId)
        {
            if (propertyId > 0)
            {
                var servicePropertyMappingRepo = RepositoryFactory.GetOBPMSRepository();

                var pmsServicePropertyMappingRequest = new ListPMSServicesPropertyMappingRequest
                {
                    PropertyUIDs = new List<long> { propertyId },
                    RequestId = requestId,
                    IsActive = true,
                    IsDeleted = false,
                    ReturnTotal = true
                };

                var pmsspm = servicePropertyMappingRepo.ListPMSServicesPropertyMappings(pmsServicePropertyMappingRequest);

                if (pmsspm.Status != Contracts.Responses.Status.Fail && pmsspm.TotalRecords > 0)
                {
                    if (pmsspm.Result.Any(x => x.ServiceName.Equals("SBRQ")) || pmsspm.Result.Any(x => x.ServiceName.Equals("GBRQ")))
                        return true;
                }
            }

            return false;
        }

        internal bool CheckAvailability(string requestId, domainReservations.Reservation domainReservation)
        {
            Logger.Debug("[{2}]: CheckAvailability - reservationId: {1} ->  requestId : {0}", requestId, domainReservation.UID, DateTime.UtcNow);

            var checkAvailabilityResponse = CheckAvailabilityFromExternalSource(requestId, domainReservation);

            if (checkAvailabilityResponse.PreCheckStatus == ES.API.Contracts.PreCheckStatus.IsAllowed)
            {

                if (checkAvailabilityResponse != null && checkAvailabilityResponse?.ReservationExternalIdentifier != null && (checkAvailabilityResponse.Errors == null || !checkAvailabilityResponse.Errors.Any()))
                {
                    Logger.Debug("[{2}]: MapPmsNumbers - ReservationId {0} -> requestId : {1}", domainReservation.UID, requestId, DateTime.UtcNow);

                    // Update Pms Numbers in reservation
                    domainReservation = MapPmsNumbers(domainReservation, checkAvailabilityResponse.ReservationExternalIdentifier);
                }

                else
                    ValidateCheckAvailabilityResponse(checkAvailabilityResponse);

                #region INSERT RESERVATION PMS HISTORY

                // Update Pms Reservations History

                Logger.Debug("[{2}]: PmsReservationsHistory - ReservationId {0} -> requestId : {1}", domainReservation.UID, requestId, DateTime.UtcNow);

                var insertPmsReservationsHistoryResponse = InsertNewExternalNumbersHistory(domainReservation, checkAvailabilityResponse.ReservationExternalIdentifier, requestId);

                if (insertPmsReservationsHistoryResponse.Warnings?.Any() == true)
                {
                    Logger.Warn("[{2}]: InsertPmsReservationsHistory - ReservationId {0} -> requestId : {1}", domainReservation.UID, requestId, DateTime.UtcNow);

                    checkAvailabilityResponse.Warnings = new List<ES.API.Contracts.Warning>
                    {
                        new ES.API.Contracts.Warning
                        {
                            Code = ES.API.Contracts.ErrorCode.InvalidPMSNumber,
                            Description = string.Format("{0} -> {1}", insertPmsReservationsHistoryResponse.Warnings.First(x => x.WarningType == "InsertPMSReservationsHistory").WarningType, insertPmsReservationsHistoryResponse.Warnings.First(x => x.WarningType == "InsertPMSReservationsHistory").Description),
                            Type = ES.API.Contracts.ErrorType.Rest,
                        }
                    };
                }

                #endregion INSERT RESERVATION PMS HISTORY
            }

            var res = (checkAvailabilityResponse.PreCheckStatus == ES.API.Contracts.PreCheckStatus.IsAllowed && checkAvailabilityResponse.Status == ES.API.Contracts.Status.Success)
                                    || (checkAvailabilityResponse.PreCheckStatus == ES.API.Contracts.PreCheckStatus.RateNotAllowed && checkAvailabilityResponse.Status == ES.API.Contracts.Status.Fail);

            return res;
        }

        internal CheckRemoteAvailabilityResponse CheckAvailabilityFromExternalSource(string requestId, domainReservations.Reservation reservation)
        {
            var propertyId = reservation.Property_UID;
            var checkAvailabilityResponse = new CheckRemoteAvailabilityResponse();

            var converterParameters = new Reservation.BL.Contracts.Requests.ListReservationRequest
            {
                IncludeReservationRooms = true,
                IncludeGuests = true,
                IncludeReservationPartialPaymentDetails = true,
                IncludeReservationPaymentDetail = true,
                IncludePaymentGatewayTransactions = true
            };

            Logger.Debug("[{2}]: CheckIfPropertyNeedsToCheckAvailability - reservationId: {1} ->  requestId : {0}", requestId, reservation.UID, DateTime.UtcNow);
            var checkAvailability = CheckIfPropertyNeedsToCheckAvailability(propertyId, requestId);

            if (checkAvailability)
            {
                checkAvailability = CheckIfPropertyHasBookingServiceActive(propertyId, requestId);

                if (checkAvailability)
                {
                    var contractReservation = DomainToBusinessObjectTypeConverter.Convert(reservation, converterParameters);

                    checkAvailabilityResponse = CheckRemoteAvailability(requestId, contractReservation);

                    ValidateCheckAvailabilityResponse(checkAvailabilityResponse);
                }
                else
                {
                    checkAvailabilityResponse.Status = ES.API.Contracts.Status.Fail;
                    checkAvailabilityResponse.PreCheckStatus = ES.API.Contracts.PreCheckStatus.RateNotAllowed;
                }
            }
            else
            {
                checkAvailabilityResponse.Status = ES.API.Contracts.Status.Success;
                checkAvailabilityResponse.PreCheckStatus = ES.API.Contracts.PreCheckStatus.RateNotAllowed;
            }

            return checkAvailabilityResponse;
        }

        internal CheckRemoteAvailabilityResponse CheckRemoteAvailability(string requestId, contractsReservations.Reservation reservation)
        {
            var decriptedCC = TryDecryptCreditCard(reservation, requestId);
            var tempEncriptedCC = new List<string>();
            string reservationRequestJson = null;

            if (decriptedCC != null && decriptedCC.Any())
            {
                tempEncriptedCC.Add(reservation.ReservationPaymentDetail.CardNumber);
                tempEncriptedCC.Add(reservation.ReservationPaymentDetail.CVV);
                reservation.ReservationPaymentDetail.CardNumber = decriptedCC.FirstOrDefault();
                reservation.ReservationPaymentDetail.CVV = decriptedCC.LastOrDefault();

                reservationRequestJson = JsonConvert.SerializeObject(reservation);

                reservation.ReservationPaymentDetail.CardNumber = tempEncriptedCC.FirstOrDefault();
                reservation.ReservationPaymentDetail.CVV = tempEncriptedCC.LastOrDefault();
            }
            else
            {
                reservationRequestJson = JsonConvert.SerializeObject(reservation);
            }

            CheckRemoteAvailabilityRequest checkAvailabilityRequest = new CheckRemoteAvailabilityRequest
            {
                MessageIdentifierToken = requestId,
                RemoteReservation = reservationRequestJson
            };

            var esRepo = RepositoryFactory.GetExternalSystemsRepository();

            var response = esRepo.CheckRemoteAvailability(checkAvailabilityRequest);

            return response;
        }

        #region Insert Pms Reservation History

        internal Contracts.Responses.InsertPMSReservationsHistoryResponse InsertNewExternalNumbersHistory(domainReservations.Reservation reservation, ES.API.Contracts.Reservations.ReservationExternalIdentifier esReservationExternalIdentifier, string requestId)
        {
            var checkIn = reservation.ReservationRooms?.OrderBy(t => t.DateFrom).Select(t => t.DateFrom).FirstOrDefault();
            var checkInDate = checkIn ?? DateTime.UtcNow;
            var isByReservationRoom = esReservationExternalIdentifier.IsByReservationRoom;
            var statusRooms = reservation.ReservationRooms != null ? reservation.ReservationRooms.Select(x => x.Status).ToList() : new List<int?>();

            var request = new InsertExternalNumbersHistoryParameters();

            request.CheckInDate = checkInDate;
            request.IsByReservationRoom = isByReservationRoom;
            request.PropertyId = reservation.Property_UID;
            request.ClientId = esReservationExternalIdentifier.ClientId;
            request.PmsId = esReservationExternalIdentifier.PmsId;
            request.RequestId = requestId;
            request.ReservationExternalNumbers = MapToReservationExternalIdentifier(esReservationExternalIdentifier);
            request.ReservationRoomExternalNumbers = MapToReservationRoomExternalIdentifier(esReservationExternalIdentifier.ReservationRoomExternalIdentifiers);
            request.ReservationStatus = reservation.Status;
            request.ReservationRoomStatus = statusRooms;

            var insertPmsReservationsHistoryResponse = InsertExternalNumbersHistory(request);

            if (insertPmsReservationsHistoryResponse.Status == Contracts.Responses.Status.Fail)
            {
                Logger.Error("[{2}]: InsertPMSReservationsHistory - ReservationId {0} -> requestId : {1}", reservation.UID, requestId, DateTime.UtcNow);

                if (insertPmsReservationsHistoryResponse.Errors?.Any() == true || insertPmsReservationsHistoryResponse.Warnings?.Any() == true)
                {
                    var warning = new Contracts.Responses.Warning
                    {
                        Description = string.Format("Could not update Pms Reservations History [ReservationId: {0}] / [requestId: {1}]", reservation.UID, requestId),
                        WarningCode = 0,
                        WarningType = "InsertPMSReservationsHistory",
                    };

                    if (insertPmsReservationsHistoryResponse.Warnings != null)
                        insertPmsReservationsHistoryResponse.Warnings.Add(warning);

                    else
                        insertPmsReservationsHistoryResponse.Warnings = new List<Contracts.Responses.Warning> { warning };
                }
            }

            return insertPmsReservationsHistoryResponse;
        }

        public Contracts.Responses.InsertPMSReservationsHistoryResponse InsertExternalNumbersHistory(InsertExternalNumbersHistoryParameters parameters)
        {
            var response = new Contracts.Responses.InsertPMSReservationsHistoryResponse();

            var reservationId = parameters.ReservationExternalNumbers.InternalId;
            var resExternalNumber = parameters.ReservationExternalNumbers.ExternalNumber;
            var resRoomExternalNumbers = parameters.ReservationRoomExternalNumbers;
            var nowDate = DateTime.UtcNow;

            Logger.Debug("[{2}]: InsertExternalNumbersHistory - reservationId: {1} ->  requestId : {0}", parameters.RequestId, reservationId, nowDate);

            InsertPMSReservationsHistoryRequest insertPmsReservationsHistoryRequest = new InsertPMSReservationsHistoryRequest
            {
                RequestId = parameters.RequestId,
                PMSReservationsHistories = new List<PMSReservationsHistory>()
            };

            var pmsServicesRepo = this.RepositoryFactory.GetOBPMSRepository();

            if (!parameters.IsByReservationRoom)
            {
                var pmsReservationsHistory = MapToPmsReservationsHistory(nowDate, parameters.PropertyId, parameters.ClientId, parameters.PmsId, resExternalNumber, reservationId, parameters.ReservationStatus, parameters.CheckInDate);

                insertPmsReservationsHistoryRequest.PMSReservationsHistories = new List<PMSReservationsHistory> { pmsReservationsHistory };
            }
            else
            {
                foreach (var reservationRoomExternalNumber in resRoomExternalNumbers)
                {
                    var pmsReservationsRoomHistory = new PMSReservationsHistory();

                    long status = reservationRoomExternalNumber.Status;
                    var resRoomumber = reservationRoomExternalNumber.ExternalNumber;
                    pmsReservationsRoomHistory = MapToPmsReservationsHistory(nowDate, parameters.PropertyId, parameters.ClientId, parameters.PmsId, resRoomumber, reservationId, status, parameters.CheckInDate);
                    insertPmsReservationsHistoryRequest.PMSReservationsHistories.Add(pmsReservationsRoomHistory);
                }
            }

            var responseInsertPMSHistory = pmsServicesRepo.InsertPMSReservationsHistory(insertPmsReservationsHistoryRequest);

            if (responseInsertPMSHistory.Status == Contracts.Responses.Status.Success)
                response.Succeed();

            return response;
        }

        #region Mapping
        private PMSReservationsHistory MapToPmsReservationsHistory(DateTime nowDate, long propertyId, long clientId, long pmsId, string pmsReservationNumber, long id, long status, DateTime checkInDate)
        {
            PMSReservationsHistory pmsReservationsHistory = new PMSReservationsHistory
            {
                Reservation_UID = id,
                PMSReservationNumber = pmsReservationNumber,
                Status = (int)status,
                Date = nowDate,
                PMS_UID = pmsId,
                Checkin = checkInDate,
                Property_UID = propertyId,
                Client_UID = clientId
            };

            return pmsReservationsHistory;
        }

        private ReservationExternalIdentifier MapToReservationExternalIdentifier(ES.API.Contracts.Reservations.ReservationExternalIdentifier reservationExternalIdentifier)
        {
            var resExternalIdentifier = new ReservationExternalIdentifier();

            resExternalIdentifier.ExternalNumber = reservationExternalIdentifier.ExternalNumber;
            resExternalIdentifier.InternalId = reservationExternalIdentifier.InternalId;
            resExternalIdentifier.IsByReservationRoom = reservationExternalIdentifier.IsByReservationRoom;
            resExternalIdentifier.ReservationRoomExternalIdentifiers = MapToReservationRoomExternalIdentifier(reservationExternalIdentifier.ReservationRoomExternalIdentifiers);

            return resExternalIdentifier;
        }

        private List<ReservationRoomExternalIdentifier> MapToReservationRoomExternalIdentifier(List<ES.API.Contracts.Reservations.ReservationRoomExternalIdentifier> reservationRoomExternalIdentifier)
        {
            var resRoomExternalIdentifier = new List<ReservationRoomExternalIdentifier>();

            foreach (var resRoom in reservationRoomExternalIdentifier)
            {
                var resRoomExt = new ReservationRoomExternalIdentifier();
                resRoomExt.ExternalNumber = resRoom.ExternalNumber;
                resRoomExt.InternalId = resRoom.InternalId;
                resRoomExt.Status = resRoom.Status;

                resRoomExternalIdentifier.Add(resRoomExt);
            }

            return resRoomExternalIdentifier;
        }

        internal domainReservations.Reservation MapPmsNumbers(domainReservations.Reservation reservation, ES.API.Contracts.Reservations.ReservationExternalIdentifier reservationExternalIdentifier)
        {
            //Update PmsNumber in reservation
            reservation.PmsRservationNumber = reservationExternalIdentifier.ExternalNumber;

            //Update PmsNumbers in reservation rooms
            if (reservation.ReservationRooms?.Any() == true && reservationExternalIdentifier.ReservationRoomExternalIdentifiers?.Any() == true)
            {
                foreach (var externalNumberRoom in reservationExternalIdentifier.ReservationRoomExternalIdentifiers)
                {
                    ReservationRoom room = null;

                    if (externalNumberRoom.InternalId != 0)
                        room = reservation.ReservationRooms.FirstOrDefault(x => x.UID == externalNumberRoom.InternalId);

                    if (room == null)
                        room = reservation.ReservationRooms.FirstOrDefault(x => x.ReservationRoomNo.Equals(externalNumberRoom.ChannelNumber));

                    if (room != null)
                        room.PmsRservationNumber = externalNumberRoom.ExternalNumber;
                }
            }

            return reservation;
        }

        #endregion Mapping

        #region Validations

        internal void ValidateCheckAvailabilityResponse(CheckRemoteAvailabilityResponse response)
        {
            if (response != null)
            {
                if (response.Status != ES.API.Contracts.Status.Success)
                {
                    if (response.PreCheckStatus == ES.API.Contracts.PreCheckStatus.RateNotAllowed)
                    {
                        return;
                    }

                    if (response.Errors?.Any() == true)
                    {
                        var errorCode = response.Errors.FirstOrDefault().Code;

                        switch (errorCode)
                        {
                            case ES.API.Contracts.ErrorCode.NoAvailability:
                                throw Errors.InvalidCheckAvailability.ToBusinessLayerException();

                            case ES.API.Contracts.ErrorCode.InvalidPriceDefined:
                                throw Errors.ReservationPricesAreInvalid.ToBusinessLayerException();

                            default:
                                throw Errors.InvalidReservationFromExternalSource.ToBusinessLayerException();
                        }
                    }
                    else
                        throw Errors.InvalidReservationFromExternalSource.ToBusinessLayerException();
                }

                if (response.ReservationExternalIdentifier == null)
                    throw Errors.InvalidReservationExternalIdentifier.ToBusinessLayerException();

                if (response.PreCheckStatus == ES.API.Contracts.PreCheckStatus.IsNotAllowed)
                    throw Errors.InvalidCheckAvailability.ToBusinessLayerException();
            }
            else
                throw Errors.InvalidReservationFromExternalSource.ToBusinessLayerException();
        }

        internal SaveReservationExternalIdentifierResponse ValidateRequest(OB.Reservation.BL.Contracts.Requests.SaveReservationExternalIdentifierRequest request)
        {
            var response = new SaveReservationExternalIdentifierResponse();

            if (request == null)
            {
                response.Errors.Add(new Error(ErrorType.InvalidRequest, 2, "Request object instance is expected"));
                return response;
            }

            response.RequestId = request.RequestId;

            if (request.PropertyId <= 0)
                response.Errors.Add(new Error(ErrorType.InvalidRequest, 199, "SaveReservationExternalIdentifierRequest - PropertyId is null or empty"));

            if (request.ClientId <= 0)
                response.Errors.Add(new Error(ErrorType.InvalidRequest, 199, "SaveReservationExternalIdentifierRequest - ClientId is null or empty"));

            if (request.PmsId <= 0)
                response.Errors.Add(new Error(ErrorType.InvalidRequest, 199, "SaveReservationExternalIdentifierRequest - PmsId is null or empty"));

            if (request.ReservationExternalIdentifier == null)
                response.Errors.Add(new Error(ErrorType.InvalidRequest, 199, "SaveReservationExternalIdentifierRequest - ReservationExternalIdentifier is null or empty"));

            return response;
        }
        #endregion Validations

        #endregion Insert Pms Reservation History

        /// <summary>
        /// RESTful implementation of the SaveExternalReservationIdentifier operation.
        /// This operation updates the Reservation or ReservationRoom entities with the given PmsReservationNumbers in the request object.
        /// </summary>
        /// <param name="request">A SaveReservationExternalIdentifierRequest object containing the mapping between Reservations/PMSNumbers and the ReservationRooms/PMSNumber</param>
        /// <returns>A SaveReservationExternalIdentifierResponse containing the result status and/or list of found errors</returns>
        public SaveReservationExternalIdentifierResponse SaveReservationExternalIdentifier(OB.Reservation.BL.Contracts.Requests.SaveReservationExternalIdentifierRequest request)
        {
            Contract.Requires(request != null, "Request object instance is expected");

            var response = ValidateRequest(request);

            if (response.Errors.Any())
                return response;

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var propertyId = request.PropertyId;
                    var clientId = request.ClientId;
                    var pmsId = request.PmsId;
                    var nowDate = DateTime.UtcNow;
                    var checkInDate = new DateTime();

                    InsertPMSReservationsHistoryRequest insertPMSReservationsHistoryRequest = new InsertPMSReservationsHistoryRequest
                    {
                        PMSReservationsHistories = new List<PMSReservationsHistory>()
                    };

                    var reservationRoomRepo = this.RepositoryFactory.GetReservationRoomRepository(unitOfWork);
                    var pmsServicesRepo = this.RepositoryFactory.GetOBPMSRepository();
                    var reservationsRepo = RepositoryFactory.GetReservationsRepository(unitOfWork);

                    var reservationIds = new List<long>();

                    if (request.ReservationExternalIdentifier.InternalId != 0)
                        reservationIds = new List<long> { request.ReservationExternalIdentifier.InternalId };

                    //Get Reservation & its Reservation Rooms
                    var reservation = reservationsRepo.FindByReservationUIDs(reservationIds, true).FirstOrDefault();

                    if (reservation != null)
                    {
                        //Update Reservation PmsNumber
                        reservation.PmsRservationNumber = request.ReservationExternalIdentifier.ExternalNumber;

                        var listReservationRoomExternalIdentifier = new List<ReservationRoomExternalIdentifier>();
                        var reservationsRoomIds = new List<long>();
                        var reservationRooms = new List<ReservationRoom>();

                        if (request.ReservationExternalIdentifier.ReservationRoomExternalIdentifiers?.Any() == true)
                        {
                            listReservationRoomExternalIdentifier = request.ReservationExternalIdentifier.ReservationRoomExternalIdentifiers;

                            //Get ReservationRooms Ids
                            reservationsRoomIds = listReservationRoomExternalIdentifier.Select(x => x.InternalId).ToList();

                            //Get ReservationRooms in the Repository
                            reservationRooms = reservationRoomRepo.FindByCriteria(reservationsRoomIds);

                            foreach (var reservationRoomPmsNumberToUpdate in listReservationRoomExternalIdentifier)
                            {
                                //Get Reservation Room To Update
                                var reservationRoomToUpdate = reservationRooms.FirstOrDefault(x => x.UID.Equals(reservationRoomPmsNumberToUpdate.InternalId));

                                //Update ReservationRoom PmsNumber
                                if (reservationRoomToUpdate != null)
                                    reservationRoomToUpdate.PmsRservationNumber = reservationRoomPmsNumberToUpdate.ExternalNumber;

                                else
                                {
                                    Warning warning = new Warning
                                    {
                                        WarningCode = 0,
                                        WarningType = "InvalidReservationRoom",
                                        Description = String.Format("Could not find the reservation room [Id: {0}] in the reservation [Id: {1}]", reservationRoomPmsNumberToUpdate.InternalId.ToString(), reservation.UID.ToString())
                                    };
                                    response.Warnings.Add(warning);
                                }
                            }
                        }
                    }

                    var reservationRoomsInReservation = reservation.ReservationRooms?.ToList() ?? null;
                    var reservationRoomsStatus = reservationRoomsInReservation?.Any() == true ? reservationRoomsInReservation.Select(x => x.Status).ToList() : new List<int?>();

                    checkInDate = TryToGetRealCheckInDate(reservationRoomsInReservation, reservationIds);

                    var insertPmsReservationHistoryRequest = new InsertExternalNumbersHistoryParameters()
                    {
                        CheckInDate = checkInDate,
                        IsByReservationRoom = request.ReservationExternalIdentifier.IsByReservationRoom,
                        PropertyId = propertyId,
                        ClientId = clientId,
                        PmsId = pmsId,
                        RequestId = request.RequestId,
                        ReservationExternalNumbers = request.ReservationExternalIdentifier,
                        ReservationRoomExternalNumbers = request.ReservationExternalIdentifier.ReservationRoomExternalIdentifiers,
                        ReservationStatus = reservation.Status,
                        ReservationRoomStatus = reservationRoomsStatus
                    };

                    var savePmsReservationNumber = InsertExternalNumbersHistory(insertPmsReservationHistoryRequest);

                    var responseInsertPMSHistory = pmsServicesRepo.InsertPMSReservationsHistory(insertPMSReservationsHistoryRequest);

                    if (responseInsertPMSHistory.Status == Contracts.Responses.Status.Success)
                        response.Succeed();

                    unitOfWork.Save();
                }
            }

            catch (BusinessLayerException ex)
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

        private DateTime TryToGetRealCheckInDate(List<ReservationRoom> reservationRooms, List<long> reservationsIds)
        {
            var checkInDate = DateTime.UtcNow;

            using (var unitOfWork = SessionFactory.GetUnitOfWork())
            {
                var reservationRoomRepo = this.RepositoryFactory.GetReservationRoomRepository(unitOfWork);

                if (reservationRooms.IsNullOrEmpty())
                {
                    var altReservationRooms = reservationRoomRepo.FindByCriteria(reservationsIds);

                    checkInDate = altReservationRooms.Any() ? (DateTime)altReservationRooms.Min(q => q.DateFrom) : checkInDate;
                }
                else
                    checkInDate = reservationRooms.Any() ? (DateTime)reservationRooms.Min(q => q.DateFrom) : checkInDate;
            }

            return checkInDate;
        }

        private List<string> TryDecryptCreditCard(contractsReservations.Reservation request, string requestId)
        {
            var responseCC = new List<string>();

            if (request == null)
                return responseCC;

            if (request.ReservationPaymentDetail != null && request.ReservationPaymentDetail.PaymentMethod_UID.HasValue && request.ReservationPaymentDetail.PaymentMethod_UID == 1 && !string.IsNullOrWhiteSpace(request.ReservationPaymentDetail.CardNumber))
            {
                var requestCC = new ListCreditCardRequest
                {
                    CreditCards = new List<string> { request.ReservationPaymentDetail.CardNumber, request.ReservationPaymentDetail.CVV },
                    RequestId = requestId
                };

                var securityRepository = RepositoryFactory.GetOBSecurityRepository();

                if (!request.ReservationPaymentDetail.CardNumber.Contains("**"))
                    responseCC = securityRepository.DecryptCreditCards(requestCC);
            }

            return responseCC;
        }
    }
}
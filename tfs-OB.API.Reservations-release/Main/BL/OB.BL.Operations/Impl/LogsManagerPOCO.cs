using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.DL.Common;
using Couchbase.Linq.Extensions;
using OB.Events.NotificationsInterpreter;
using OB.Events.NotificationsInterpreter.BaseLogDetailInterpreter;
using System.Globalization;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Interfaces;
using System.Threading;
using OB.Events.Contracts;
using OB.Domain.Reservations;

namespace OB.BL.Operations.Impl
{
    public class LogsManagerPOCO : BusinessPOCOBase, ILogsManagerPOCO
    {
        public ListLogsResponse ListLogs(ListLogsRequest request)
        {
            var response = new ListLogsResponse();

            try
            {
                using (var unitOfWork = SessionFactory.GetUnitOfWork())
                {
                    var propertyRepo = RepositoryFactory.GetOBPropertyRepository();
                    var notificationBaseRepository = RepositoryFactory.GetNotificationBaseRepository();
                    var query = notificationBaseRepository.GetQuery();

                    // Filter By Uids
                    if (request.UIDs != null && request.UIDs.Any())
                        query = query.UseKeys(request.UIDs);

                    // Filter By PropertyUids
                    if (request.PropertyUIDs != null && request.PropertyUIDs.Any())
                        query = query.Where(q => request.PropertyUIDs.Contains(q.PropertyUID));

                    // Get Results
                    var queryResults = query.ToList();

                    var notificationsBase = queryResults.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();

                    response.Results = new List<Reservation.BL.Contracts.Data.BaseLogDetails.BaseLogDetail>();

                    // Get culture
                    CultureInfo culture = null;
                    var lang = (request.LanguageId != null) ? propertyRepo.ListLanguages(new Contracts.Requests.ListLanguageRequest { UIDs = new List<long> { request.LanguageId ?? 0 } }).FirstOrDefault() : null;
                    if (lang != null)
                        culture = CultureInfo.CreateSpecificCulture(lang.Code);

                    // Group Notification by Property UID
                    foreach (var groupedNotification in notificationsBase.GroupBy(x => x.PropertyUID))
                    {
                        // Get Property TimeZone
                        TimeZoneInfo propertyTimeZone = null;

                        if (request.ApplicationId == Reservation.BL.Contracts.Requests.Base.ApplicationEnum.Omnibees)
                        {
                            var timezoneId = propertyRepo.ListPropertiesLight(new Contracts.Requests.ListPropertyRequest { UIDs = new List<long> { groupedNotification.Key } }).Select(x => x.TimeZone_UID).FirstOrDefault();
                            var timeZoneId = MasterTimeZones.FindByUID(timezoneId.Value).TimeZoneName;
                            propertyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                        }

                        var logsList = new List<Reservation.BL.Contracts.Data.BaseLogDetails.BaseLogDetail>();
                        foreach (var notification in groupedNotification)
                        {
                            var requestHandler = new NotificationHandlerRequest
                            {
                                ApplicationType =  (OB.Events.Contracts.Enums.ApplicationEnum)((int)request.ApplicationId),
                                Notification = notification,
                                CurrentCulture = culture,
                                ConvertDatesToTimeZone = propertyTimeZone,
                                ChannelUID = request.ChannelId,
                                TpiUID = request.TpiId
                            };

                            using (var notificationHandler = new NotificationHandler(requestHandler))
                            {
                                var log = EventsToContractsConverter.Convert(notificationHandler.GetFriendlyNotification());
                                logsList.Add(log);
                            }
                        }
                        response.Results.AddRange(logsList);
                    }                    
                }
                response.Succeed();
            }
            catch (Exception ex)
            {
                response.Errors.Add(new Error(ex));
                response.Failed();
            }

            return response;
        }

        public ListReservationGridLogsResponse ListReservationGridLogs(ListFriendlyLogsRequest request)
        {
            var response = new ListReservationGridLogsResponse();
            var propertyRepo = RepositoryFactory.GetOBPropertyRepository();

            var DateFromTmp = request.StartDate;
            var DateToTmp = request.EndDate;

            try
            {
                if (!request.UseUtcDates && request.PropertyUID.HasValue && request.PropertyUID > 0)
                {
                    request = propertyRepo.ConvertToPropertyTimeZone(request.PropertyUID.Value, request, new List<string> { "StartDate", "EndDate" });
                }

                var notificationBaseRepository = RepositoryFactory.GetReservationLogRepository();
                var query = notificationBaseRepository.GetQuery();

                if (request.PropertyUID != null && request.PropertyUID > 0)
                    query = query.Where(x => x.PropertyId == request.PropertyUID);

                if (request.StartDate != null)
                    query = query.Where(x => x.CreatedDate >= request.StartDate);

                if (request.EndDate != null)
                    query = query.Where(x => x.CreatedDate <= request.EndDate);

                // Order By Grid
                if (request.Orders != null && request.Orders.Any())
                {
                    var orders = request.Orders.Select((xx => OtherConverter.Convert(xx))).ToList();
                    query = query.OrderBy(orders);
                }
                else
                {
                    // Default Order (Created Date)
                    query = query.OrderByDescending(q => q.CreatedDate);
                }

                // Filters By Grid
                if (request.Filters != null && request.Filters.Any())
                {
                    var filters = request.Filters.Select(xx => OtherConverter.Convert(xx)).ToList();
                    query = query.FilterBy(filters);
                }

                var results = query.ToList();

                if (request.UserIds != null && request.UserIds.Count > 0)
                    results = results.Where(x => request.UserIds.Contains(x.CreatedBy)).ToList();

                if (request.ReturnTotal)
                    response.TotalRecords = results.Count();

                if (request.PageIndex > 0 && request.PageSize > 0)
                    results = results.AsQueryable().Skip(request.PageIndex * request.PageSize).ToList();

                if (request.PageSize > 0)
                    results = results.AsQueryable().Take(request.PageSize).ToList();

                var finalResult = results.Select(x => DomainToBusinessObjectTypeConverter.Convert(x)).ToList();

                if (!request.UseUtcDates)
                {
                    finalResult = propertyRepo.ConvertToPropertyTimeZoneList(request.PropertyUID.Value, finalResult, new List<string> { "CreatedDate" });
                }

                finalResult = finalResult.Where(x => x.CreatedDate >= DateFromTmp && x.CreatedDate <= DateToTmp).ToList();
                finalResult = finalResult.OrderByDescending(s => s.CreatedDate).ToList();

                response.Results = finalResult;
                response.Succeed();
            }
            catch (Exception ex)
            {
                response.Errors.Add(new Error(ex));
                response.Failed();
            }

            return response;
        }


    }
}

using OB.DL.Common.Interfaces;
using OB.Domain.ProactiveActions;
using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Text;

namespace OB.DL.Common.Impl
{
    internal class PropertyQueueRepository : Repository<PropertyQueue>, IPropertyQueueRepository
    {
        public PropertyQueueRepository(IObjectContext context)
            : base(context)
        {
        }

        #region Finders

        /// <summary>
        /// Finds all PropertyQueue instances for the given criteria.
        /// </summary>
        /// <param name="totalRecords"></param>
        /// <param name="uids"></param>
        /// <param name="propertyUIDs"></param>
        /// <param name="propertyEventUIDs"></param>
        /// <param name="systemEventUIDs"></param>
        /// <param name="taskTypeUIDs"></param>
        /// <param name="systemTemplateUIDs"></param>
        /// <param name="isProcessing"></param>
        /// <param name="isProcessed"></param>
        /// <param name="minRetryNumber"></param>
        /// <param name="maxRetryNumber"></param>
        /// <param name="timeSpanSinceLastProcessingDate"></param>
        /// <param name="minLastProcessingDate"></param>
        /// <param name="maxLastProcessingDate"></param>
        /// <param name="minDate"></param>
        /// <param name="maxDate"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="returnTotal"></param>
        /// <returns>A collection of PropertyQueue instances.</returns>
        public IEnumerable<PropertyQueue> FindByCriteria(out int totalRecords,
            List<long> uids = null,
            List<long> propertyUIDs = null, List<long> propertyEventUIDs = null,
            List<long> systemEventUIDs = null,
            List<long> taskTypeUIDs = null,
            List<long> systemTemplateUIDs = null,
            bool? isProcessing = null, bool? isProcessed = null,
            int? minRetryNumber = null, int? maxRetryNumber = null,
            TimeSpan? timeSpanSinceLastProcessingDate = null,
            DateTime? minLastProcessingDate = null, DateTime? maxLastProcessingDate = null,
            DateTime? minDate = null, DateTime? maxDate = null,
            int pageIndex = 0, int pageSize = 0, bool returnTotal = false)
        {
            var result = GetQuery();
            totalRecords = -1;

            if (uids != null && uids.Count > 0)
                result = result.Where(x => uids.Contains(x.UID));

            if (propertyUIDs != null && propertyUIDs.Count > 0)
                result = result.Where(x => propertyUIDs.Contains(x.Property_UID));

            if (propertyEventUIDs != null && propertyEventUIDs.Count > 0)
                result = result.Where(x => x.PropertyEvent_UID.HasValue && propertyEventUIDs.Contains(x.PropertyEvent_UID.Value));

            if (systemEventUIDs != null && systemEventUIDs.Count > 0)
                result = result.Where(x => x.SystemEvent_UID.HasValue && systemEventUIDs.Contains(x.SystemEvent_UID.Value));

            if (taskTypeUIDs != null && taskTypeUIDs.Count > 0)
                result = result.Where(x => x.TaskType_UID.HasValue && taskTypeUIDs.Contains(x.TaskType_UID.Value));

            if (systemTemplateUIDs != null && systemTemplateUIDs.Count > 0)
                result = result.Where(x => x.SystemTemplate_UID.HasValue && systemTemplateUIDs.Contains(x.SystemTemplate_UID.Value));

            if (isProcessing.HasValue)
                result = result.Where(x => x.IsProcessing == isProcessing.Value);

            if (isProcessed.HasValue)
                result = result.Where(x => x.IsProcessed == isProcessed.Value);

            if (minRetryNumber.HasValue || maxRetryNumber.HasValue)
                result = result.Where(x => x.Retry.HasValue &&
                                 ((!minRetryNumber.HasValue || x.Retry.Value >= minRetryNumber)
                                  &&
                                  (!maxRetryNumber.HasValue || x.Retry.Value <= maxRetryNumber)));

            if (timeSpanSinceLastProcessingDate.HasValue)
            {
                DateTime now = DateTime.Now;
                var totalMinutes = timeSpanSinceLastProcessingDate.Value.Minutes;
                result = result.Where(x => x.LastProcessingDate.HasValue &&
                                         SqlMethods.DateDiffMinute(x.LastProcessingDate, now) >= totalMinutes);
            };

            if (minDate.HasValue || maxDate.HasValue)
                result = result.Where(x => ((!minDate.HasValue || x.Date >= minDate)
                                  &&
                                  (!maxDate.HasValue || x.Date <= maxDate)));

            if (minLastProcessingDate.HasValue || maxLastProcessingDate.HasValue)
                result = result.Where(x => x.LastProcessingDate.HasValue &&
                                 ((!minLastProcessingDate.HasValue || x.LastProcessingDate >= minLastProcessingDate)
                                  &&
                                  (!maxLastProcessingDate.HasValue || x.LastProcessingDate <= maxLastProcessingDate)));

            if (returnTotal)
                totalRecords = result.Count();

            result = this.ApplyPaging(result, x => x.UID, pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Fetcheds the PropertyQueues wich have not yet start processing and also the ones that were lost (IsProcessing = 1 && ([LastProcessingDate] - NOW > 30 minutes))
        /// marking the fetched PropertyQueue records with IsProcessing=1.
        /// </summary>
        /// <param name="numberRetries"></param>
        /// <param name="retryMinutesInterval"></param>
        /// <param name="systemEventUIDs">List of SystemEvent UIDs to filter the property queues (optional)</param>
        /// <returns></returns>
        public List<PropertyQueue> GetTodaysPropertyQueues(int numberRetries, int retryMinutesInterval,
                                                          List<long> systemEventUIDs = null)
        {
            StringBuilder query = null;

            if (systemEventUIDs != null && systemEventUIDs.Count > 0)
            {
                query = new StringBuilder(" DECLARE @SystemEventUIDs TABLE(BIGINT UID) INSERT INTO @SystemEventUIDs(UID) VALUES(" + systemEventUIDs.First() + ") ");
                foreach (var uid in systemEventUIDs.Skip(1))
                    query.AppendFormat(" ,({0})", uid);
            }
            else query = new StringBuilder();

            query.AppendLine();

            query.AppendFormat(@" SELECT [UID]
                          ,[Property_UID]
                          ,[PropertyEvent_UID]
                          ,[Date]
                          ,[IsProcessed]
                          ,[TaskType_UID]
                          ,[IsProcessing]
                          ,[Retry]
                          ,[ErrorList]
                          ,[LastProcessingDate]
                          ,[MailTo]
                          ,[MailFrom]
                          ,[MailSubject]
                          ,[MailBody]
                          ,[SystemEvent_UID]
                           ,[SystemTemplate_UID]
                           ,[ChannelActivityErrorDateFrom]
                            ,[ChannelActivityErrorDateTo]
                      FROM [PropertyQueue] WHERE ");

            string whereClause = string.Empty;

            if (systemEventUIDs != null && systemEventUIDs.Count > 0)
            {
                whereClause = " (SystemEvent_UID IN (SELECT UID from @SystemEventUIDs)) AND ";
            }

            whereClause += @"  (  isnull(isProcessing,0)<>1 AND isnull(isProcessed,0)<>1 AND isnull([Retry],0)=0
                                  OR (isnull(isProcessed,0)<>1 AND isProcessing=0 AND DATEDIFF(minute,[LastProcessingDate],getdate())>{0} AND Retry>0 AND Retry<={1})
                                  --process lost ones if exist in 1/2 hours
                                  OR (isProcessed<>1 AND isProcessing=1 AND DATEDIFF(minute,[LastProcessingDate],getdate())>30 AND Retry<={2}) ) ";

            query.AppendFormat(whereClause, retryMinutesInterval.ToString(), numberRetries.ToString(), numberRetries.ToString());

            query.Append(" UPDATE [PropertyQueue] SET isProcessing=1, [LastProcessingDate]=GETDATE(),Retry=isnull(Retry,0) + 1 WHERE ");
            query.Append(whereClause);

            return this._objectSet.SqlQuery(query.ToString()).ToList();
        }

        /// <summary>
        /// Marks the PropertyQueues with the given UIDs with the column IsProcessing with TRUE.
        /// </summary>
        /// <param name="uids"></param>
        /// <returns>The number of records that were changed</returns>
        public int SetPropertyQueueProcessing(List<long> uids)
        {
            StringBuilder query = new StringBuilder(" DECLARE @UIDs TABLE(BIGINT UID)");

            if (uids != null && uids.Count > 0)
            {
                query.AppendFormat(" INSERT INTO @UIDs(UID) VALUES({0})", uids.First());
            }

            foreach (var uid in uids.Skip(1))
            {
                query.AppendFormat(" ,({0})", uid);
            }

            query.Append(" UPDATE [PropertyQueue] SET isProcessing=1, [LastProcessingDate]=GETDATE(),Retry=isnull(Retry,0) + 1 ");
            query.Append(@"   WHERE  isnull(isProcessing,0)<>1 AND isnull(isProcessed,0)<>1 AND isnull([Retry],0)=0
                                  OR (isnull(isProcessed,0)<>1 AND isProcessing=0 AND UID IN (select UID from @UIDs)");
            //            @"   WHERE  isnull(isProcessing,0)<>1 AND isnull(isProcessed,0)<>1 AND isnull([Retry],0)=0
            //               OR (isnull(isProcessed,0)<>1 AND isProcessing=0 AND DATEDIFF(minute,[LastProcessingDate],getdate())>{0} AND Retry>0 AND Retry<={1})
            //               --process lost ones if exist in 1/2 hours
            //               OR (isProcessed<>1 AND isProcessing=1 AND DATEDIFF(minute,[LastProcessingDate],getdate())>30 AND Retry<={2}) ";

            return this._context.Context.Database.ExecuteSqlCommand(query.ToString());
        }

        /// <summary>
        /// Marks the PropertyQueues with the given UIDs with the column IsProcessed with TRUE.
        /// </summary>
        /// <param name="uids"></param>
        /// <returns>The number of records that were changed</returns>
        public int SetPropertyQueueProcessed(Dictionary<long, string> uidsAndErrors)
        {
            StringBuilder query = new StringBuilder();

            if (uidsAndErrors != null && uidsAndErrors.Count > 0)
            {
                foreach (var uidAndError in uidsAndErrors)
                {
                    query.AppendFormat(" UPDATE [PropertyQueue] SET isProcessed=1,isProcessing = false, [LastProcessingDate]=GETDATE(), ErrorList = '{1}' WHERE UID = {0}\r\n", uidAndError.Key, uidAndError.Value);
                }
                return this._context.Context.Database.ExecuteSqlCommand(query.ToString());
            }

            return 0;
        }

        /// <summary>
        /// Sets the errors in the PropertyQueues with the given UIDs marking the PropertyQueue as Processed and updating the LastProcessingDate and Retry.
        /// </summary>
        /// <param name="uids"></param>
        /// <returns>The number of records that were changed</returns>
        public int SetPropertyQueueProcessedError(Dictionary<long, string> errorsByPropertyQueueUID)
        {
            StringBuilder query = new StringBuilder();

            if (errorsByPropertyQueueUID != null && errorsByPropertyQueueUID.Count > 0)
            {
                foreach (var uidAndError in errorsByPropertyQueueUID)
                {
                    query.AppendFormat(" UPDATE [PropertyQueue] SET isProcessed=1,isProcessing = false, [LastProcessingDate]=GETDATE(), ErrorList = '{1}', Retry = (CASE WHEN LEN('{1}')=0 THEN 0 ELSE Retry END) WHERE UID = {0}\r\n", uidAndError.Key, uidAndError.Value);
                }
                return this._context.Context.Database.ExecuteSqlCommand(query.ToString());
            }

            return 0;
        }

        #endregion Finders
    }
}
using OB.Domain.ProactiveActions;
using System;
using System.Collections.Generic;

namespace OB.DL.Common.Interfaces
{
    public interface IPropertyQueueRepository : IRepository<PropertyQueue>
    {
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
        IEnumerable<PropertyQueue> FindByCriteria(out int totalRecords,
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
            int pageIndex = 0, int pageSize = 0, bool returnTotal = false);

        /// <summary>
        /// Fetcheds the PropertyQueues wich have not yet start processing and also the ones that were lost (IsProcessing = 1 && ([LastProcessingDate] - NOW > 30 minutes))
        /// marking the fetched PropertyQueue records with IsProcessing=1.
        /// </summary>
        /// <param name="numberRetries"></param>
        /// <param name="retryMinutesInterval"></param>
        /// <param name="systemEventUIDs">List of SystemEvent UIDs to filter the property queues (optional)</param>
        /// <returns></returns>
        List<PropertyQueue> GetTodaysPropertyQueues(int numberRetries, int retryMinutesInterval,
                                                          List<long> systemEventUIDs = null);

        /// <summary>
        /// Marks the PropertyQueues with the given UIDs with the column IsProcessing with TRUE.
        /// </summary>
        /// <param name="uids"></param>
        /// <returns>The number of records that were changed</returns>
        int SetPropertyQueueProcessing(List<long> uids);

        /// <summary>
        /// Marks the PropertyQueues with the given UIDs with the column IsProcessed with TRUE.
        /// </summary>
        /// <param name="uids"></param>
        /// <returns>The number of records that were changed</returns>
        int SetPropertyQueueProcessed(Dictionary<long, string> uidsAndErrors);

        /// <summary>
        /// Sets the errors in the PropertyQueues with the given UIDs marking the PropertyQueue as Processed and updating the LastProcessingDate and Retry.
        /// </summary>
        /// <param name="uids"></param>
        /// <returns>The number of records that were changed</returns>
        int SetPropertyQueueProcessedError(Dictionary<long, string> errorsByPropertyQueueUID);
    }
}
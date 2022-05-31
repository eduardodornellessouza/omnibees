using OB.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.Domain.Reservations;

namespace OB.BL.Operations.Interfaces
{
    public interface IPropertyEventsManagerPOCO  : IBusinessPOCOBase
    {
        /// <summary>
        /// RESTful implementation of the ListPropertyQueues operation.
        /// This operation searchs for PropertyQueue entities given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListPropertyQueueRequest object containing the search criteria to find PropertyQueues</param>
        /// <returns>A ListPropertyQueueResponse containing the List of PropertyQueue objects that were found for the given criteria</returns
        ListPropertyQueueResponse ListPropertyQueues(ListPropertyQueueRequest request);

        /// <summary>
        /// RESTful implementation of the ListPropertyEvents operation.
        /// This operation searchs for PropertyEvent entities given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListPropertyEventRequest object containing the search criteria to find PropertyEvents</param>
        /// <returns>A ListPropertyEventResponse containing the List of PropertyEvent objects that were found for the given criteria</returns
        ListPropertyEventResponse ListPropertyEvents(ListPropertyEventRequest request);

        /// <summary>
        /// RESTful implementation of the ListSystemActions operation.
        /// This operation searchs for SystemAction entities given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListSystemActionRequest object containing the search criteria to find SystemActions</param>
        /// <returns>A ListSystemActionResponse containing the List of SystemAction objects that were found for the given criteria</returns
        ListSystemActionResponse ListSystemActions(ListSystemActionRequest request);

        /// <summary>
        /// RESTful implementation of the ListSystemEvents operation.
        /// This operation searchs for SystemEvent entities given a specific request criteria.
        /// </summary>
        /// <param name="request">A ListSystemEventRequest object containing the search criteria to find SystemEvents</param>
        /// <returns>A ListSystemEventResponse containing the List of SystemEvent objects that were found for the given criteria</returns
        ListSystemEventResponse ListSystemEvents(ListSystemEventRequest request);

        /// <summary>
        /// RESTful implementation of the ListPropertyQueuesForTodayProcessing operation.
        /// This operation gets the PropertyQueue entities that should be processed for today, 
        /// marking them in the database with IsProcessing=true.
        /// </summary>
        /// <param name="request">A ListPropertyQueueForTodayProcessingRequest object containing the filters to find PropertyQueues
        /// including the filter for the SystemEvents.
        /// </param>
        /// <returns>A ListPropertyQueueResponse containing the List of PropertyQueue objects that were found and marked with IsProcessing=true</returns>
        ListPropertyQueueResponse ListPropertyQueuesForTodayProcessing(ListPropertyQueueForTodayProcessingRequest request);

        long InsertPropertyQueue(long propertyId, long reservationId, long channelId, long systemEventCode, bool isOperator = false);

        long InsertPropertyQueue(long PropertyId, long SystemEventCode, long TaskTypeId, bool isOperator = false);
   
        long CancelPropertyQueueEvent(long PropertyId, long SystemEventCode, long TaskTypeId, string reasonCancelation);

        
        /// <summary>
        /// Set From Mail
        /// </summary>
        /// <param name="PropertyUID"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        PropertyMailServerSettings GetFromMail(long PropertyUID, int Type);

       
   
    }
}

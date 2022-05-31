using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Response class used in update operations for the entity type ConnectorEventQueue.
    /// </summary>
    [DataContract]
    public class UpdateConnectorEventQueueResponse : ResponseBase
    {
        /// <summary>
        /// List with the record UIDs that were updated successfully.
        /// </summary>
        [DataMember]
        public List<long> UpdatedRecords { get; set; }
    }
}
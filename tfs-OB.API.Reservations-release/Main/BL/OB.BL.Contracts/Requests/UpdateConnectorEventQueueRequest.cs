using OB.Reservation.BL.Contracts.Data.Channels;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request update class, for update operations of the entity type ConnectorEventQueue.
    /// </summary>
    [DataContract]
    public class UpdateConnectorEventQueueRequest : RequestBase
    {
        /// <summary>
        /// List of ConnectorEventQueue data transfer objects with the values to replace in the Database.
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<ConnectorEventQueue> Values { get; set; }

        /// <summary>
        /// Gets or sets Property filter used by the update operation. If it's true, the JsonContent property will be updated in the database for all the given ConnectorEventQueues in the
        /// Values collection with the matching UIDs.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UpdateJsonContent { get; set; }

        /// <summary>
        /// Gets or sets Property filter used by the update operation. If it's true, the MessageUID property will be updated in the database for all the given ConnectorEventQueues in the
        /// Values collection with the matching UIDs.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UpdateMessageUID { get; set; }

        /// <summary>
        /// Gets or sets Property filter used by the update operation. If it's true, the IsProcessed property will be updated in the database for all the given ConnectorEventQueues in the
        /// Values collection with the matching UIDs.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UpdateIsProcessed { get; set; }

        /// <summary>
        /// Gets or sets Property filter used by the update operation. If it's true, the Operation property will be updated in the database for all the given ConnectorEventQueues in the
        /// Values collection with the matching UIDs.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UpdateOperation { get; set; }

        /// <summary>
        /// Gets or sets Property filter used by the update operation. If it's true, the OperationDateTime property will be updated in the database for all the given ConnectorEventQueues in the
        /// Values collection with the matching UIDs.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UpdateOperationDateTime { get; set; }

        /// <summary>
        /// Gets or sets Property filter used by the update operation. If it's true, the PropertyUID property will be updated in the database for all the given ConnectorEventQueues in the
        /// Values collection with the matching UIDs.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UpdateProperty_UID { get; set; }

        /// <summary>
        /// Gets or sets Property filter used by the update operation. If it's true, the TableKey property will be updated in the database for all the given ConnectorEventQueues in the
        /// Values collection with the matching UIDs.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UpdateTableKey { get; set; }

        /// <summary>
        /// Gets or sets Property filter used by the update operation. If it's true, the TableName property will be updated in the database for all the given ConnectorEventQueues in the
        /// Values collection with the matching UIDs.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool UpdateTableName { get; set; }
    }
}
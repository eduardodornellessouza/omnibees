using OB.Reservation.BL.Contracts.Data.Channels;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that stores the arguments for the InsertChannelActivity RESTfull operation.
    /// The InsertChannelActivity Request object that contains all the required objects
    /// (ConnectorMessage) to create and insert a ConnectorMessage into the database.
    /// </summary>
    [DataContract]
    public class InsertChannelActivityRequest : RequestBase
    {
        /// <summary>
        /// List of ConnectorMessage instances that are used to insert records into the ConnectorMessage or ConnectorMessage_OB depending on the boolean value of IsChannelActivity.
        /// </summary>
        [DataMember]
        public List<ConnectorMessage> ConnectorMessages;

        /// <summary>
        /// List of booleans that indicates if the given ConnectorMessage with the same index is to insert into the ConnectorMessage table or ConnectorMessage_OB table (IsChannelActivity = true).
        /// If this property is null then it is assumed that all the given ConnectorMessage instances are to be saved into the ConnectorMessage table, otherwise,
        /// for each given ConnectorMessage instance in the same index, if the boolean value is true, then it is saved into the ConnectorMessage_OB table.
        /// </summary>
        [DataMember]
        public List<bool> IsChannelActivity;
    }
}
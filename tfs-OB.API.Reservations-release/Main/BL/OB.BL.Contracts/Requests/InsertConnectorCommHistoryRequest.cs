using OB.Reservation.BL.Contracts.Data.PMS;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that stores the arguments for the InsertConnectorCommHistory RESTfull operation.
    /// The InsertConnectorCommHistory Request object that contains all the required objects
    /// (InsertConnectorCommHistory) to create and insert a InsertConnectorCommHistory into the database.
    /// </summary>
    [DataContract]
    public class InsertConnectorCommHistoryRequest : RequestBase
    {
        /// <summary>
        /// List of ConnectorCommHistory instances that are used to insert records into the ConnectorCommHistory.
        /// </summary>
        [DataMember]
        public List<ConnectorCommHistory> ConnectorMessages;
    }
}
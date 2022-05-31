using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that stores the arguments for the ListConnectorMessageErrorCode RESTfull operation.
    /// The ListConnectorMessageErrorCode Request object that contains all the
    /// to list the ConnectorMessage existing Error Codes and descriptions that are stored in the Database.
    /// </summary>
    [DataContract]
    public class ListConnectorMessageErrorCodeRequest : RequestBase
    {
        /// <summary>
        /// List of ConnectorMessageErrorCode int codes to filter.
        /// </summary>
        [DataMember]
        public List<int> Codes;
    }
}
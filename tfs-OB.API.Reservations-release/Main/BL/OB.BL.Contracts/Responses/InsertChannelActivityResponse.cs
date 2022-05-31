using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an InsertChannelActivity RESTful operation.
    /// It provides the Result field that contains the List of UIDs of the created ConnectorMessage.
    /// </summary>
    [DataContract]
    public class InsertChannelActivityResponse : ResponseBase
    {
        /// <summary>
        /// List of UIDs of the created ConnectorMessage records if sucessfull otherwise it's a negative integer.
        /// </summary>
        [DataMember]
        public List<long> Result { get; set; }
    }
}
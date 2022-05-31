using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request reservation numbers
    /// </summary>
    [DataContract]
    public class ListReservationNumbersRequest : RequestBase
    {
        /// <summary>
        /// Dictionary containing a property Id as key, and number of reservation numbers as value
        /// </summary>
        [DataMember(IsRequired = true)]
        public Dictionary<long, int> Values { get; set; }
    }
}
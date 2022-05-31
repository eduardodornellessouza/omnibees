using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Response reservation numbers
    /// </summary>
    [DataContract]
    public class ListReservationsUIDsResponse : ResponseBase
    {
        /// <summary>
        /// Dictionary containing a property Id as key, and number of reservation numbers as value
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> Result { get; set; }
    }
}
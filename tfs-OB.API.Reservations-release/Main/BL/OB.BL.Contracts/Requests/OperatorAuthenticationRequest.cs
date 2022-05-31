using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Guests.
    /// </summary>
    [DataContract]
    public class OperatorAuthenticationRequest : PagedRequestBase
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }
    }
}
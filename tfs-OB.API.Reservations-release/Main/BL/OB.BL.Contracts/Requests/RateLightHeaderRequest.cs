using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a light set of Rates.
    /// </summary>
    [DataContract]
    public class RateLightHeaderRequest : PagedRequestBase
    {
        [DataMember]
        public List<RateHeaderLight> Rates { get; set; }

        [DataMember]
        public long UserUID { get; set; }
    }
}
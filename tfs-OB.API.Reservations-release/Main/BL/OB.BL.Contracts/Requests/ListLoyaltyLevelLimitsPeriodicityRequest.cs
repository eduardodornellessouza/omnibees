using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of ListLoyaltyLevelLimitsPeriodicities.
    /// </summary>
    [DataContract]
    public class ListLoyaltyLevelLimitsPeriodicityRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the ListLoyaltyLevelLimitsPeriodicities to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// The list of Names for the ListLoyaltyLevelLimitsPeriodicities to return.
        /// </summary>
        [DataMember]
        public List<string> Names { get; set; }

    }
}

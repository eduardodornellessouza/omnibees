using System.Collections.Generic;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.Reservations;
using OB.Reservation.BL.Contracts.Data.Rates;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for ReservationsExternalSources.
    /// </summary>
    [DataContract]
    public class InsertOrUpdateRateExternalSourceRequest : PagedRequestBase
    {

        /// <summary>
        /// The list of RatessExternalSource's for which to Insert/Update.
        /// </summary>
        [DataMember]
        public List<RateExternalSource> RateExternalSources { get; set; }
    }
}
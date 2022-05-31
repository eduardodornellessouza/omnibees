using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using OB.Reservation.BL.Contracts.Data.Reservations;
using OB.Reservation.BL.Contracts.Responses;
using OB.Reservation.BL.Contracts.Data.Rates;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Request class to be used in operations that search for a set of ReservationsExternalSource.
    /// </summary>
    [DataContract]
    public class ListRateExternalSourceResponse : PagedResponseBase
    {
        [DataMember]
        public IList<RateExternalSource> Result { get; set; }
    }
}
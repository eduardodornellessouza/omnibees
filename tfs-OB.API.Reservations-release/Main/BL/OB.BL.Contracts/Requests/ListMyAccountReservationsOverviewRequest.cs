using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a specific Reservation or set of Reservations.
    /// </summary>
    [DataContract]
    public class ListMyAccountReservationsOverviewRequest : GridPagedRequest
    {
        [DataMember]
        public long UserUID { get; set; }

        [DataMember]
        public int UserType { get; set; }

        [DataMember]
        public DateTime? DateFrom { get; set; }

        [DataMember]
        public DateTime? DateTo { get; set; }

    }
}

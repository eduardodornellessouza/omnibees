using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListReservationStatusesRequest : GridPagedRequest
    {
        /// <summary>
        /// The list of Ids for the ReservationStatuses to return
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public HashSet<long> Ids { get; set; }
    }
}

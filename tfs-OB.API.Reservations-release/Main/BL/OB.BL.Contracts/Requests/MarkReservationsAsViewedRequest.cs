using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// The request sent to mark a resevation as viewed.
    /// </summary>
    [DataContract]
    public class MarkReservationsAsViewedRequest : RequestBase
    {
        /// <summary>
        /// The reservations Ids that should be marked as viewed.
        /// </summary>
        [DataMember(IsRequired=false)]
        public List<long> Reservation_UIDs { get; set; }
        
        /// <summary>
        /// The NewValue conaining the state of the reservation. True if the reservation should be marked as viewed, false otherwise.
        /// </summary>
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public bool NewValue { get; set; }

        /// <summary>
        /// The affected User Id.
        /// </summary>
        [DataMember(IsRequired=true, EmitDefaultValue=false)]
        public long User_UID { get; set; }

        public MarkReservationsAsViewedRequest()
        {
            RequestGuid = Guid.NewGuid();
            Reservation_UIDs = new List<long>();
            NewValue = false;
        }
    }
}

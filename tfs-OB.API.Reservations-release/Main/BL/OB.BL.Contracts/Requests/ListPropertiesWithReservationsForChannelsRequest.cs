using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for ListPropertiesWithReservationsForChannels.
    /// </summary>
    [DataContract]
    public class ListPropertiesWithReservationsForChannelsRequest : PagedRequestBase
    {

        /// <summary>
        /// The pairs of ChannelTpis for which to find the Properties with Reservations.
        /// </summary>
        [DataMember]
        public List<KeyValuePair<long,long?>> ChannelsTpis { get; set; }

        [DataMember]
        public List<long> PropertyUIDs { get; set; }

    }
}
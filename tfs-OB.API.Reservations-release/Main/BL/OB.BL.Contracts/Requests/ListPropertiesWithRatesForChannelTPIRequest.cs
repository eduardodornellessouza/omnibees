using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPropertiesWithRatesForChannelTPIRequest : RequestBase
    {
        [DataMember]
        public long Channel_UID { get; set; }

        [DataMember]
        public long TPI_UID { get; set; }
    }
}
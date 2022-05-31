using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPropertiesWithRatesForChannelsTPIsRequest : RequestBase
    {
        [DataMember]
        public List<long> ChannelUids {get; set;}

        [DataMember]
        public List<long> TPIUids { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    public class ListChannelOperatorsRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> ChannelUIDs { get; set; }
        // Added to populate channel name as ChannelOperator has a connection to channel but needed to be included
        [DataMember]
        public bool IncludeChannelInfo { get; set; }
    }
}

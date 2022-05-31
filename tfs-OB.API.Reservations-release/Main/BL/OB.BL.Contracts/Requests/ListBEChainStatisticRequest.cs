using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListBEChainStatisticRequest : RequestBase
    {
        public ListBEChainStatisticRequest() { }
        
        [DataMember]
        public List<long> ClientUIDs { get; set; }
    }
}

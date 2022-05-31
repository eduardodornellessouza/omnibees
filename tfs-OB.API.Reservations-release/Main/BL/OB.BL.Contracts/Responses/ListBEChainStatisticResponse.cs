using OB.Reservation.BL.Contracts.Data.BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListBEChainStatisticResponse : ResponseBase
    {
        public ListBEChainStatisticResponse() { }

        [DataMember]
        public List<BEChainStatistic> Results { get; set; }
    }
}

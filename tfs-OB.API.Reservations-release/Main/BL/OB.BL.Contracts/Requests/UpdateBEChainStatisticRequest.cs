using OB.Reservation.BL.Contracts.Data.BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateBEChainStatisticRequest : RequestBase
    {
        public UpdateBEChainStatisticRequest() { }

        [DataMember]
        public List<BEChainStatistic> BEChainStatistics { get; set; }
    }
}

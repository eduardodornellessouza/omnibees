using OB.Reservation.BL.Contracts.Data.BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class UpdateBEChainStatisticResponse : ResponseBase
    {
        public UpdateBEChainStatisticResponse() { }

        [DataMember]
        public List<BEChainStatistic> UpdatedBEChainStatistics { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class LogRateAndAvailabilitiesOpenCloseSalesRequest : RequestBase
    {
        [DataMember]
        public string CorrelationUID { get; set; }
        [DataMember]
        public List<long> RateRoomDetailsIds { get; set; }

        [DataMember]
        public long PropertyId { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public int BlockType { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class UpdateRateChannelResponse : ResponseBase
    {
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public List<long> UpdatedRatesChannelsUIDs { get; set; }
    }
}

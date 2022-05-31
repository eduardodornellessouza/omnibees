using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPropertyQueueForTodayProcessingRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int NumberRetries { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RetryMinutesInterval { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> SystemEventUIDs { get; set; }
    }
}
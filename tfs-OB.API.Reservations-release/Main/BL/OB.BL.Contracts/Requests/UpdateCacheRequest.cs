using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateCacheRequest : RequestBase
    {
        [DataMember]
        public bool? RefreshAllEntries { get; set; }

        [DataMember]
        public List<string> CacheEntryKeys { get; set; }

        [DataMember]
        public bool? ForceUpdate { get; set; }
    }
}
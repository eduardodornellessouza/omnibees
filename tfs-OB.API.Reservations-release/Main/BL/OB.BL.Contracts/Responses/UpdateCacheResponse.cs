using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class UpdateCacheResponse : ResponseBase
    {
        [DataMember]
        public List<string> UpdatedCacheEntries { get; set; }

        [DataMember]
        public int NumberOfUpdatedCacheEntries { get; set; }
    }
}
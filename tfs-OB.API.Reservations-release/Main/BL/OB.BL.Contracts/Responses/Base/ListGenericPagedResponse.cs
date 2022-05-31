using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListGenericPagedResponse<T> : PagedResponseBase where T : ContractBase
    {
        [DataMember]
        public IList<T> Result { get; set; }
    }
}
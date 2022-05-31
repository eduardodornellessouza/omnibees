using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPagedResponse : PagedResponseBase
    {
        [DataMember]
        public ObservableCollection<ContractBase> Result { get; set; }
    }
}
using OB.Reservation.BL.Contracts.Data.PMS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListConnectorCommHistoryResponse : PagedResponseBase
    {
        [DataMember]
        public List<ConnectorCommHistory> Result { get; set; }
    }
}
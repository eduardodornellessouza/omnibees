using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListInventoryResponse : ResponseBase
    {
        [DataMember]
        public IList<Inventory> Result { get; set; }
    }
}
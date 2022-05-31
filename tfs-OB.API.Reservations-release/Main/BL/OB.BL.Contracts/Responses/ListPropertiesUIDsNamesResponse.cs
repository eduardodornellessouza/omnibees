using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPropertiesUIDsNamesResponse : PagedResponseBase
    {
        [DataMember]
        public IList<PropertyUIDName> Result { get; set; }
    }
}
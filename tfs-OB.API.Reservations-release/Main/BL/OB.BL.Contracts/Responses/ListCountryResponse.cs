using OB.Reservation.BL.Contracts.Data.General;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListCountryResponse : PagedResponseBase
    {
        [DataMember]
        public IList<Country> Result { get; set; }
    }
}
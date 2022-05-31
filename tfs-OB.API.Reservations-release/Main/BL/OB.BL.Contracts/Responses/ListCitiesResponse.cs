using OB.Reservation.BL.Contracts.Data.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListCitiesResponse : PagedResponseBase
    {
        [DataMember]
        public IList<City> Result { get; set; }
    }
}

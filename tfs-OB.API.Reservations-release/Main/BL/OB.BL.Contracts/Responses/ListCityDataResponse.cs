using OB.Reservation.BL.Contracts.Data.General;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListCityDataResponse : PagedResponseBase
    {
        [DataMember]
        public IList<CityData> Result { get; set; }
    }
}
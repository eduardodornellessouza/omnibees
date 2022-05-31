using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPropertyCurrencyResponse : PagedResponseBase
    {
        [DataMember]
        public IList<PropertyCurrency> Result { get; set; }
    }
}
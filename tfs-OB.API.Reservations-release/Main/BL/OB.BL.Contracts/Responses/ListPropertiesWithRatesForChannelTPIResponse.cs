using OB.Reservation.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPropertiesWithRatesForChannelTPIResponse : ResponseBase
    {
        [DataMember]
        public List<PropertyWithRatesForChannelTPI> Result { get; set; }
    }
}
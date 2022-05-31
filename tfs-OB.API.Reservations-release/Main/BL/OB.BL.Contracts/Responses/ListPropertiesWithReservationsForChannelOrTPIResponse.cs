using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPropertiesWithReservationsForChannelOrTPIResponse : ResponseBase
    {
        [DataMember]
        public List<PropertyWithReservationsForChannelOrTPI> Result { get; set; }
    }
}
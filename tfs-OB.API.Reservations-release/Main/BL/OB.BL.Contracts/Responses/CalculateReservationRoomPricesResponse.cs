using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class CalculateReservationRoomPricesResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<RateRoomDetailReservation> ReservationRateRoomDetails { get; set; }
    }
}

using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GetDepositCostsResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ReservationRoomDepositCost> ReservationRoomsDepositCosts { get; set; }
    }
}

using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ModifyReservationResponse : ReservationBaseResponse
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public contractsReservation.Reservation Reservation { get; set; }
    }
}

using OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GetCancelationCostsResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ReservationRoomCancelationCost> ReservationRoomsCancelationCosts { get; set; }
    }
}

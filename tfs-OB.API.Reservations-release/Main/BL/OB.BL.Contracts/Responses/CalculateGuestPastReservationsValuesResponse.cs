using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class CalculateGuestPastReservationsValuesResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int ReservationsCount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RoomNightsCount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ReservationsTotalAmount { get; set; }
    }
}

﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class CreateRecurringBillingRequest: RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Token { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PropertyName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PaypalAutoGeneratedId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public contractsReservations.Reservation Reservation { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<contractsReservations.ReservationRoom> ReservationRooms { get; set; }

    }
}
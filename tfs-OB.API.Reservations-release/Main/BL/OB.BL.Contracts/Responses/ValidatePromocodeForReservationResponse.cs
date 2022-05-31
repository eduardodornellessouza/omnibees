using OB.Reservation.BL.Contracts.Data.Rates;
using OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ValidatePromocodeForReservationResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<ReservationRoomStayPeriod> ReservationRoomsPeriods { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Data.Rates.PromotionalCode PromoCodeObj { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool RejectReservation { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<DateTime> OldDaysAppliedDiscount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<DateTime> NewDaysToApplyDiscount { get; set; }
    }
}

using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ValidateReservationRestricions : ContractBase
    {
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public DateTime CheckIn { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public DateTime CheckOut { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long RateId { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long RoomTypeId { get; set; }
    }
}

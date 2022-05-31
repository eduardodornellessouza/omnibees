using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationRoomStayPeriod : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckIn { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckOut { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RateUID { get; set; }
    }
}

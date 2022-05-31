using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ValidateReservationRestricionsRequest : RequestBase
    {
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long PropertyId { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long ChannelId { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public List<ValidateReservationRestricions> Items { get; set; }
    }
}

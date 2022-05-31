using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    [DataContract]
    public class ReservationGridLineDetail : BaseGridLineDetail
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CheckIn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CheckOut { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuestName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string ChannelName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int ReservationStatus { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationTotal { get; set; }
    }
}

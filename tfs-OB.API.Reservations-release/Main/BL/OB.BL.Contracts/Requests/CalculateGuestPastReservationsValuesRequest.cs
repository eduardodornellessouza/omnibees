using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class CalculateGuestPastReservationsValuesRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Guest_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int PeriodicityLimitType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int PeriodicityLimitValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long LoyaltyLevelBaseCurrency_UID { get; set; }
    }
}

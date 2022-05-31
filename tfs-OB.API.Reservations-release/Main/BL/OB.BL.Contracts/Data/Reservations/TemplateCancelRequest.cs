using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class TemplateCancelRequest : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PropertyName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuestName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuestEmail { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuestPhone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Comments { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GetGuestActiveResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Client_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string FirstName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LastName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Phone { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string IDCardNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? Country_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? LoyaltyLevel_UID { get; set; }

    }
}

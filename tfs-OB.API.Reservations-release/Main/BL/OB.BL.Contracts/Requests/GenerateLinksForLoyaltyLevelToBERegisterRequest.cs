using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GenerateLinksForLoyaltyLevelToBERegisterRequest : RequestBase
    {

        [DataMember(IsRequired = true)]
        public long LoyaltyLevel_UID { get; set; }

        [DataMember(IsRequired = true)]
        public long Client_UID { get; set; }

        [DataMember(IsRequired = true)]
        public long User_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte Qty { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class GuestSocialProvider : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long SocialProvider_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Guest_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ProviderUserID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }
    }
}

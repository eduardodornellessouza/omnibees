using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    public class ListPropertySecurityConfigurationRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> UIDS { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDs { get; set; }

    }
}

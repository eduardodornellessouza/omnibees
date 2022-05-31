using OB.Reservation.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    public class UpdatePropertySecurityConfigurationRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PropertySecurityConfiguration> PropertySecurityConfigurationList { get; set; }

        [DataMember]
        public long UserUID { get; set; }

    }
}

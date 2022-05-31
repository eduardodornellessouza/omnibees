using OB.Reservation.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    public class ListPropertySecurityConfigurationResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PropertySecurityConfiguration> Result { get; set; }

    }
}

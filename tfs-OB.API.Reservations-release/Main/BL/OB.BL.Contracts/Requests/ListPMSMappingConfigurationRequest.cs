using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPMSMappingConfigurationRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PMS_UIDs { get; set; }

        public ListPMSMappingConfigurationRequest()
        {
            PMS_UIDs = new List<long>();
        }
    }
}

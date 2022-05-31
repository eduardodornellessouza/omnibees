using OB.Reservation.BL.Contracts.Data.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPMSMappingConfigurationResponse : PagedResponseBase
    {
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public List<DynamicTableSimpleResult> Result { get; set; }

        public ListPMSMappingConfigurationResponse()
        {
            Result = new List<DynamicTableSimpleResult>();
        }
    }
}

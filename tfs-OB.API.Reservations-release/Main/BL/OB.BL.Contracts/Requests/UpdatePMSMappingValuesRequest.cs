using OB.Reservation.BL.Contracts.Data.PMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdatePMSMappingValuesRequest : RequestBase
    {
        [DataMember]
        public List<PMSMappingValue> PMSMappingValues { get; set; }

        public UpdatePMSMappingValuesRequest()
        {
            PMSMappingValues = new List<PMSMappingValue>();
        }
    }
}

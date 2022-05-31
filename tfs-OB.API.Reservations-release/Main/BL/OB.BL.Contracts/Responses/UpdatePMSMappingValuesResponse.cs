using OB.Reservation.BL.Contracts.Data.PMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class UpdatePMSMappingValuesResponse : ResponseBase
    {
        [DataMember]
        public List<PMSMappingValue> PMSMappingValues { get; set; }

        public UpdatePMSMappingValuesResponse()
        {
            PMSMappingValues = new List<PMSMappingValue>();
        }
    }
}

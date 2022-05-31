using OB.Reservation.BL.Contracts.Data.PMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPMSMappingValueResponse : PagedResponseBase
    {
        [DataMember]
        public IList<PMSMappingValue> Result { get; set; }

        public ListPMSMappingValueResponse()
        {
            Result = new List<PMSMappingValue>();
        }
    }
}

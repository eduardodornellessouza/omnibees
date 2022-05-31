using OB.Reservation.BL.Contracts.Data.BaseLogDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListLogsResponse : ResponseBase
    {
        [DataMember(IsRequired=true, EmitDefaultValue=false)]
        public List<BaseLogDetail> Results { get; set; }
        

        public ListLogsResponse()
        {
            Results = new List<BaseLogDetail>();
        }
    }
}

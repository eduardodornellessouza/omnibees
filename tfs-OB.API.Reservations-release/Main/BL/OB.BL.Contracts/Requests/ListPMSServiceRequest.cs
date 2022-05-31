using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    public class ListPMSServiceRequest : PagedRequestBase
    {
        public ListPMSServiceRequest()
        {
        }

        [DataMember]
        public List<long> PMSServiceUIDs { get; set; }

        [DataMember]
        public List<long> PMSUIDs { get; set; }

        [DataMember]
        public List<string> ServiceNames { get; set; }

        [DataMember]
        public bool IncludePMSs { get; set; }
    }
}
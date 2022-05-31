using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPaymentGatewayConfigurationRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> PropertyIds { get; set; }

        [DataMember]
        public List<long> GatewayIds { get; set; }

        [DataMember]
        public List<string> GatewayNames { get; set; }

        [DataMember]
        public List<string> GatewayCodes { get; set; }
    }
}
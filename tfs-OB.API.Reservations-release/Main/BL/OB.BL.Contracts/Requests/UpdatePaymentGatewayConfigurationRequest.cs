using OB.Reservation.BL.Contracts.Data.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdatePaymentGatewayConfigurationRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PaymentGatewayConfiguration> ConfigurationsToUpdate { get; set; }
    }
}

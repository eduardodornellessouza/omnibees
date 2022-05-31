using OB.Reservation.BL.Contracts.Data.Payments;
using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPaymentGatewayConfigurationResponse : PagedResponseBase
    {
        public ListPaymentGatewayConfigurationResponse()
        {
            Result = new List<PaymentGatewayConfiguration>();
        }

        [DataMember]
        public IList<PaymentGatewayConfiguration> Result { get; set; }
    }
}
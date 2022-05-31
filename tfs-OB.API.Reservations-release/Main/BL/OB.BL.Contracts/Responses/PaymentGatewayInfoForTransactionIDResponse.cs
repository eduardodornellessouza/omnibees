using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class PaymentGatewayInfoForTransactionIDResponse : ResponseBase
    {
        public PaymentGatewayInfoForTransactionIDResponse()
        {
        }

        [DataMember]
        public PaymentGatewayCommitOrCancelReservationOnRequestResult Result { get; set; }

    }
}
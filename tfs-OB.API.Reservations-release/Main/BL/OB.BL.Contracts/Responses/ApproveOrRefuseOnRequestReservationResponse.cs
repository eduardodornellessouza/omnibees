using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// The response sent from a request to Approve or cancel a On Request Reservation.
    /// </summary>
    [DataContract]
    public class ApproveOrRefuseOnRequestReservationResponse : ResponseBase
    {
        public ApproveOrRefuseOnRequestReservationResponse() : base() { }
        public ApproveOrRefuseOnRequestReservationResponse(RequestBase request) : base(request){}
    }
}

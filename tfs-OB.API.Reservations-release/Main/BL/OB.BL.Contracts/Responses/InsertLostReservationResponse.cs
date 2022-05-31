using OB.Reservation.BL.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class InsertLostReservationResponse : ResponseBase
    {
        public InsertLostReservationResponse()
        {

        }

        public InsertLostReservationResponse(RequestBase request)
        {
            this.RequestGuid = request.RequestGuid;
        }
    }
}

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ValidateReservationRestricionsResponse : ResponseBase
    {

        [DataMember]
        public bool Result { get; set; }
    }
}

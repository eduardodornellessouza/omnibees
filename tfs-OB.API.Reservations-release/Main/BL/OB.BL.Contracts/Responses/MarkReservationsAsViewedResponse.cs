using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// The response sent from a request to mark a reservation as viewed.
    /// </summary>
    [DataContract]
    public class MarkReservationsAsViewedResponse : ResponseBase
    {

    }
}

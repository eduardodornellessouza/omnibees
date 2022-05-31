using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a response from an UpdatePMSReservationNumber request.
    /// </summary>
    [DataContract]
    public class UpdatePMSReservationNumberResponse : ResponseBase
    {
        public UpdatePMSReservationNumberResponse()
        {
        }
    }
}
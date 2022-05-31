using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetReservationReadStatusRequest : RequestBase
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long ReservationUID { get; set; }

    }
}

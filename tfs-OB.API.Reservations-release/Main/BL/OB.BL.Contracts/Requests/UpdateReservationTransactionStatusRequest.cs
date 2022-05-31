using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateReservationTransactionStatusRequest : RequestBase
    {
        [DataMember]
        public string TransactionId { get; set; }

        [DataMember]
        public long ChannelId { get; set; }

        [DataMember]
        public Reservation.BL.Constants.ReservationTransactionStatus ReservationTransactionStatus { get; set; }
    }
}
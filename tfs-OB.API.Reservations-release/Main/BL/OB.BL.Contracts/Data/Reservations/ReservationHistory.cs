using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationHistory : ContractBase
    {
        public ReservationHistory()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long? ReservationUID { get; set; }

        [DataMember]
        public string ReservationNumber { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Channel { get; set; }

        [DataMember]
        public long? StatusUID { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime? ChangedDate { get; set; }
    }
}
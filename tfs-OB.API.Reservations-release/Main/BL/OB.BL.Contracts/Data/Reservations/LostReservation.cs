using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class LostReservation
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CheckIn { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CheckOut { get; set; }

        [DataMember]
        public string GuestName { get; set; }

        [DataMember]
        public string GuestEmail { get; set; }

        [DataMember]
        public Nullable<int> NumberOfRooms { get; set; }

        [DataMember]
        public string ReservationTotal { get; set; }

        [DataMember]
        public string CouchBaseId { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public LostReservationDetail Detail { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public Decimal TotalReservation { get; set; }
    }
}

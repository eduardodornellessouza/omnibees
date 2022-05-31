namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoomChild : ContractBase
    {
        public ReservationRoomChild()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public Nullable<long> ReservationRoom_UID { get; set; }

        [DataMember]
        public Nullable<int> ChildNo { get; set; }

        [DataMember]
        public Nullable<int> Age { get; set; }

        public virtual ReservationRoom ReservationRoom { get; set; }
    }
}
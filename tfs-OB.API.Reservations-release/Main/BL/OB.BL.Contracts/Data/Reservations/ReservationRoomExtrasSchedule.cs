namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoomExtrasSchedule : ContractBase
    {
        public ReservationRoomExtrasSchedule()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long ReservationRoomExtra_UID { get; set; }

        [DataMember]
        public System.DateTime Date { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        //public virtual ReservationRoomExtra ReservationRoomExtra { get; set; }
    }
}
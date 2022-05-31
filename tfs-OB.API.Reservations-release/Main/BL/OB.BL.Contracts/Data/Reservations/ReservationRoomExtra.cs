namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoomExtra : ContractBase
    {
        public ReservationRoomExtra()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Extra_UID { get; set; }

        [DataMember]
        public long ReservationRoom_UID { get; set; }

        [DataMember]
        public short Qty { get; set; }

        [DataMember]
        public decimal Total_Price { get; set; }

        [DataMember]
        public decimal Total_VAT { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember]
        public bool ExtraIncluded { get; set; }

        //public virtual ReservationRoom ReservationRoom { get; set; }

        [DataMember]
        public virtual ICollection<ReservationRoomExtrasAvailableDate> ReservationRoomExtrasAvailableDates { get; set; }
        
        [DataMember]
        public virtual IList<ReservationRoomExtrasSchedule> ReservationRoomExtrasSchedules { get; set; }

        [DataMember]
        public Rates.Extra Extra { get; set; }

        [DataMember]
        public IList<long> LinkedExtraByReservationRoomNumber { get; set; }
    }
}
namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using OB.Reservation.BL.Contracts.Data.Properties;
    using OB.Reservation.BL.Contracts.Data.Rates;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoomFilter : ContractBase
    {
        public ReservationRoomFilter()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long ReservationId { get; set; }

        [DataMember]
        public string ReservationRoomNo { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CheckIn { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CheckOut { get; set; }

        [DataMember]
        public Nullable<bool> ApplyDepositPolicy { get; set; }

        [DataMember]
        public Nullable<decimal> DepositCost { get; set; }

        [DataMember]
        public Nullable<int> DepositNumberOfNight { get; set; }

        [DataMember]
        public Nullable<long> Status { get; set; }

        [DataMember]
        public string GuestName { get; set; }

        [DataMember]
        public Nullable<decimal> CancellationCost { get; set; }
    }
}
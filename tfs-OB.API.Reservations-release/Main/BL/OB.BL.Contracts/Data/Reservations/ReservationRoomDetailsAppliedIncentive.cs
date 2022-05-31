namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using OB.Reservation.BL.Contracts.Data.Properties;
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoomDetailsAppliedIncentive : ContractBase
    {
        public ReservationRoomDetailsAppliedIncentive()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long ReservationRoomDetails_UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Nullable<int> Days { get; set; }

        [DataMember]
        public Nullable<int> FreeDays { get; set; }

        [DataMember]
        public Nullable<int> DiscountPercentage { get; set; }

        [DataMember]
        public Nullable<bool> IsFreeDaysAtBegin { get; set; }

        [DataMember]
        public long Incentive_UID { get; set; }

        [DataMember]
        public Nullable<decimal> DiscountValue { get; set; }

        public virtual ReservationRoomDetail ReservationRoomDetail { get; set; }

        [DataMember]
        public Incentive Incentive { get; set; }
    }
}
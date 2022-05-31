using OB.Reservation.BL.Contracts.Data.Rates;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoomDetail : ContractBase
    {
        public ReservationRoomDetail()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public Nullable<long> RateRoomDetails_UID { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public long ReservationRoom_UID { get; set; }

        [DataMember]
        public Nullable<decimal> AdultPrice { get; set; }

        [DataMember]
        public Nullable<decimal> ChildPrice { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember]
        public System.DateTime Date { get; set; }

        [DataMember]
        public Nullable<long> Rate_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Rate Rate { get; set; }


        public virtual ReservationRoom ReservationRoom { get; set; }

        [DataMember]
        public IList<ReservationRoomDetailsAppliedIncentive> ReservationRoomDetailsAppliedIncentives { get; set; }

        [DataMember]
        public ReservationRoomDetailsAppliedPromotionalCode ReservationRoomDetailsAppliedPromotionalCode { get; set; }
    }
}
namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using OB.Reservation.BL.Contracts.Data.Properties;
    using OB.Reservation.BL.Contracts.Data.Rates;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationRoomLight : ContractBase
    {
        public ReservationRoomLight()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public long Reservation_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<long> RoomType_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string GuestName { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<System.DateTime> DateFrom { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<System.DateTime> DateTo { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> AdultCount { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> ChildCount { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> TotalTax { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<int> Status { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string RoomName { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<long> Rate_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> ReservationRoomsPriceSum { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> ReservationRoomsExtrasSum { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> ReservationRoomsTotalAmount { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string PmsRservationNumber { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<long> CommissionType { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Nullable<decimal> CommissionValue { get; set; }

    }
}
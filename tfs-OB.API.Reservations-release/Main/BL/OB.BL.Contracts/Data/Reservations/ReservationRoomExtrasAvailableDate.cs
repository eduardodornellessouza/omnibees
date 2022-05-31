using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationRoomExtrasAvailableDate : ContractBase
    {
        public ReservationRoomExtrasAvailableDate()
        {
        }

        [DataMember(EmitDefaultValue=false, IsRequired=false)]
        public long UID { get; set; }
        
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public long ReservationRoomExtra_UID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public System.DateTime DateFrom { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public System.DateTime DateTo { get; set; }
    }
}
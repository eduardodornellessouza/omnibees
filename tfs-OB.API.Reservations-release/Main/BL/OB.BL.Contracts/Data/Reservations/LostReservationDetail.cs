using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public partial class LostReservationDetail : ContractBase
    {
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public LostReservationGuest Guest { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LostReservationRoom> Rooms { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal Total { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CurrencyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckIn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckOut { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Comments { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Request1 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Request2 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Request3 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Request4 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string IPAddress { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencyName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal RoomsTotalAmountOnlyRates { get; set; }
    }
}

using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class CancelReservationReason : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int UID { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
    }
}

using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationExternalSource : ContractBase
    {
        public ReservationExternalSource()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Reservation_UID { get; set; }

        [DataMember]
        public long ExternalSource_UID { get; set; }

        [DataMember]
        public string ExternalReservationID { get; set; }

        [DataMember]
        public System.DateTime CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

    }
}
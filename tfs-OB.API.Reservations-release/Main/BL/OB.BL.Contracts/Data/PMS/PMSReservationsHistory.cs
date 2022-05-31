using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.PMS
{
    [DataContract]
    public class PMSReservationsHistory : ContractBase
    {
        public PMSReservationsHistory()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Reservation_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PMSReservationNumber { get; set; }

        [DataMember]
        public int Status { get; set; }

        [DataMember]
        public System.DateTime Date { get; set; }

        [DataMember]
        public long PMS_UID { get; set; }

        [DataMember]
        public System.DateTime Checkin { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public long Client_UID { get; set; }

        [DataMember]
        public bool IsProcessed { get; set; }
    }
}

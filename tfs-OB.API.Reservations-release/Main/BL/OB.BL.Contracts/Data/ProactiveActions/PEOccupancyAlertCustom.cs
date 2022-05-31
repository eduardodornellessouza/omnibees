using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.ProactiveActions
{
    [DataContract]
    public class PEOccupancyAlertCustom
    {
        public PEOccupancyAlertCustom()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int CloseSalesOn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActivateCloseSales { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActivateCloseSalesInBE { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsNotify { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int NotifyOn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyEvent_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RoomType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RateRoomDetails_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Rate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Channel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Date { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }
    }
}
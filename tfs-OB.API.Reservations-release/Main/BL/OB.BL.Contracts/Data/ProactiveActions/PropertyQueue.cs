using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.ProactiveActions
{
    /// <summary>
    /// Data Transfer Object for the PropertyQueue entities.
    /// </summary>
    [DataContract]
    public class PropertyQueue : ContractBase
    {
        public PropertyQueue()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> PropertyEvent_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public System.DateTime Date { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsProcessed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> Retry { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> LastProcessingDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> TaskType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> SystemEvent_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> SystemTemplate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ChannelActivityErrorDateFrom { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ChannelActivityErrorDateTo { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPropertyQueueRequest : GenericListPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int NumberRetries { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RetryMinutesInterval { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyEventUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> SystemEventUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> TaskTypeUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> SystemTemplateUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsProcessing { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsProcessed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? MinRetryNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? MaxRetryNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TimeSpan? TimeSpanSinceLastProcessingDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? MinLastProcessingDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? MaxLastProcessingDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? MinDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? MaxDate { get; set; }
    }
}
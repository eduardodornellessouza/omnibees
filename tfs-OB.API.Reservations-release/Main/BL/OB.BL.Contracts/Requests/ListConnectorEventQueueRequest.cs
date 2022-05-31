using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListConnectorEventQueueRequest : PagedRequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> TableKeys { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> TableNames { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<int> Operations { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Guid> MessageGUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsProcessed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IncludeJsonContent { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? MinOperationDateTime { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? MaxOperationDateTime { get; set; }
    }
}
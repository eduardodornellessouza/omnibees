using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListExternalSourcesCommunicationsRequest : GridPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Guid> Uids { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ExternalSource_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? DateFrom { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? DateTo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ExternalCommMessageTypeCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? ExternalCommMessageStatus_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Language_UID { get; set; }
    }
}
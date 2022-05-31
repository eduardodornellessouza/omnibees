using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPMSCommunicationsRequest : GridPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Guid> Uids { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Channel_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> Channels { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? State_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<DateTime> DateFrom { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<DateTime> DateTo { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PMSCommMessageTypeCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? CommType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Language_UID { get; set; }
    }
}
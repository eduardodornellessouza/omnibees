using OB.Reservation.BL.Contracts.Requests.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListLogsRequest : GridPagedRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApplicationEnum ApplicationId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? ChannelId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? TpiId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? LanguageId { get; set; }

        public ListLogsRequest()
        {
            UIDs = new List<string>();
            PropertyUIDs = new List<long>();
            RequestGuid = Guid.NewGuid();
        }
    }
}

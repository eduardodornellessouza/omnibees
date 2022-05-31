using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPMSMappingValueRequest : RequestBase
    {
        [DataMember(EmitDefaultValue=false, IsRequired=false)]
        public List<long> UIDs { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<long> PMSMapping_UIDS { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool IsDeleted { get; set; }

        public ListPMSMappingValueRequest()
        {
            UIDs = new List<long>();
            PMSMapping_UIDS = new List<long>();
            IsDeleted = false;
        }
    }
}
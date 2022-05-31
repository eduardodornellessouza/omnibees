using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPMSMappingRequest : RequestBase
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<long> UIDs { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<long> PMSServicesPropertyMapping_UIDs { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<long> Property_UIDs { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<long> PMS_UIDs { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool WithTranslations { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public long Language_UID { get; set; }

        public ListPMSMappingRequest()
        {
            UIDs = new List<long>();
            PMSServicesPropertyMapping_UIDs = new List<long>();
            Property_UIDs = new List<long>();
            PMS_UIDs = new List<long>();
            WithTranslations = false;
        }
    }
}
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for PMS Service Property Mappings.
    /// </summary>
    [DataContract]
    public class ListPMSServicesPropertyMappingRequest : PagedRequestBase
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string PMSName { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string PmsHotelCode { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string PMSServiceName { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<long> PropertyUIDs { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public long? MaxPropertyUID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public long? MinPropertyUID { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool? IsActive { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool? IncludePMSs { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool? IncludePMSServices { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool? IncludePMSConfigurations { get; set; }
    }
}
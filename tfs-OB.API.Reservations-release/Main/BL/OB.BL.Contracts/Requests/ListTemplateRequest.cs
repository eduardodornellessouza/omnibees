using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListTemplateRequest : RequestBase
    {
        [DataMember(IsRequired = true)]
        public List<TemplateDescriptor> TemplateDescriptors { get; set; }
    }
}
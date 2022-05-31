using OB.Reservation.BL.Contracts.Data.General;
using OB.Reservation.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of PropertiesExternalSource configuration objects.
    /// </summary>
    [DataContract]
    public class ListPropertiesExternalSourceForOmnibeesResponse : PagedResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<PropertiesExternalSourceForOmnibees> Result { get; set; }
    }
}
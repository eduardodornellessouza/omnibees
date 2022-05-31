using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that returns inserted or updated PropertyExternalSource
    /// </summary>
    [DataContract]
    public class UpdatePropertyExternalSourceResponse : ResponseBase
    {
        [DataMember]
        public List<PropertiesExternalSource> PropertyExternalSource { get; set; }
    }
}

using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPropertyLightResponse : PagedResponseBase
    {
        public ListPropertyLightResponse()
        {
            Result = new List<PropertyLight>();
        }

        [DataMember]
        public IList<PropertyLight> Result { get; set; }
    }
}
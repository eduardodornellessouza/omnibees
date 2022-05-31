using OB.Reservation.BL.Contracts.Data.PMS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of PMSServicesPropertyMappings configuration objects.
    /// </summary>
    [DataContract]
    public class ListPMSServicesPropertyMappingResponse : PagedResponseBase
    {
        public ListPMSServicesPropertyMappingResponse()
        {
            Result = new ObservableCollection<PMSServicesPropertyMapping>();
            PMSLookup = new Dictionary<long, PMS>();
        }

        [DataMember]
        public IList<PMSServicesPropertyMapping> Result { get; set; }

        [DataMember]
        public Dictionary<long, PMS> PMSLookup { get; set; }

        [DataMember]
        public Dictionary<long, PMSService> PMSServiceLookup { get; set; }
    }
}
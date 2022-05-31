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
    public class ListPMSRateRoomMappingLightResponse : PagedResponseBase
    {
        public ListPMSRateRoomMappingLightResponse()
        {
            Result = new ObservableCollection<PMSRateRoomMappingLight>();
        }

        [DataMember]
        public IList<PMSRateRoomMappingLight> Result { get; set; }
    }
}
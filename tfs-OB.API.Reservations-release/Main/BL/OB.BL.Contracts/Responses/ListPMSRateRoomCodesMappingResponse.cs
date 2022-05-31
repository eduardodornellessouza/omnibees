using OB.Reservation.BL.Contracts.Data.PMS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of PMSRateRoomCodesMappings configuration objects.
    /// </summary>
    [DataContract]
    public class ListPMSRateRoomCodesMappingResponse : PagedResponseBase
    {
        public ListPMSRateRoomCodesMappingResponse()
        {
            ResultOfPMSRoomCodes = new ObservableCollection<PMSRoomCode>();

            ResultOfPMSRateCodes = new ObservableCollection<PMSRateCode>();
        }

        [DataMember]
        public IList<PMSRoomCode> ResultOfPMSRoomCodes { get; set; }

        [DataMember]
        public IList<PMSRateCode> ResultOfPMSRateCodes { get; set; }
    }
}
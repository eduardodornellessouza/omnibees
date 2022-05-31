using System.Collections.Generic;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.CRM;
using contractsPMS = OB.Reservation.BL.Contracts.Data.PMS;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for TPIsExternalSources.
    /// </summary>
    [DataContract]
    public class InsertOrUpdatePMSRateRoomMappingsRequest : PagedRequestBase
    {

        /// <summary>
        /// The list of PMSRateRoomMappings's for which to Insert/Update.
        /// </summary>
        [DataMember]
        public List<contractsPMS.PMSRateRoomMappingLight> PMSRateRoomMappings { get; set; }

        [DataMember]
        public long UserUID { get; set; }
    }
}
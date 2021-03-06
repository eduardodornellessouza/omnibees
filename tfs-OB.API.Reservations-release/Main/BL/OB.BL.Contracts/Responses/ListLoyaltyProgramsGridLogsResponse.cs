using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListLoyaltyProgramsGridLogsResponse : PagedResponseBase
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<OB.Reservation.BL.Contracts.Data.BaseLogDetails.LoyaltyProgramsGridLineDetail> Results { get; set; }
    }
}

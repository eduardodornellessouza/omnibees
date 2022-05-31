using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OB.Reservation.BL.Contracts.Data.CRM;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListLoyaltyLevelsResponse : PagedResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LoyaltyLevel> Results { get; set; }
    }
}

using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListLoyaltyProgramResponse : PagedResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LoyaltyProgram> Results { get; set; }
    }
}

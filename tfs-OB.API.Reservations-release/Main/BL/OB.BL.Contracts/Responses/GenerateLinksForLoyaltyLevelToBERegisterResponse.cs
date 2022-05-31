using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OB.Reservation.BL.Contracts.Data.CRM;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GenerateLinksForLoyaltyLevelToBERegisterResponse : PagedResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LoyaltyLevelsGeneratedLinksForBERegister> Result { get; set; }
    }
}

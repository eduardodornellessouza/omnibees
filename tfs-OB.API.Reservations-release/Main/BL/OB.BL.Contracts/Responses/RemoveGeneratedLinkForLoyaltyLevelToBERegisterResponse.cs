using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OB.Reservation.BL.Contracts.Data.CRM;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class RemoveGeneratedLinkForLoyaltyLevelToBERegisterResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public LoyaltyLevelsGeneratedLinksForBERegister Result { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool WasDeleted { get; set; }
    }
}

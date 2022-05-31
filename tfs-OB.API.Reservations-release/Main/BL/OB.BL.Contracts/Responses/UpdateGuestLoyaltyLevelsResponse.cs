using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class UpdateGuestLoyaltyLevelsResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<OB.Reservation.BL.Contracts.Data.CRM.Guest> UpdatedGuests { get; set; }
    }
}

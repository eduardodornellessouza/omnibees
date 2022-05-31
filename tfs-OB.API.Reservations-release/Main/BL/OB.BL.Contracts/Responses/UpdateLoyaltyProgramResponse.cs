using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class UpdateLoyaltyProgramResponse : ResponseBase
    {
        [DataMember]
        public List<LoyaltyProgram> Results { get; set; }
    }
}

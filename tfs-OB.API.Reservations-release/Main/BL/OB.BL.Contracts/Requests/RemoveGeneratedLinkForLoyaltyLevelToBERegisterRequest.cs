using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class RemoveGeneratedLinkForLoyaltyLevelToBERegisterRequest : RequestBase
    {

        [DataMember(IsRequired = true)]
        public Guid GUID { get; set; }

    }
}

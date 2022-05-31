using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateGuestLoyaltyLevelsRequest : RequestBase
    {
        /// <summary>
        /// Migrate guests from a level to another level.
        /// Key - Old LoyaltyLevel UID,
        /// Value - New LoyaltyLevel UID
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<long,long?> OldVsNewLevelsUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ClientUID { get; set; }
    }
}

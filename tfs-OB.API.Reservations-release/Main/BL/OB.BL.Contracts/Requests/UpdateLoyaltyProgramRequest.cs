using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateLoyaltyProgramRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LoyaltyProgram> LoyaltyPrograms { get; set; }

        /// <summary>
        /// Used for LOG.
        /// The Property UID of the property that modified the program.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? ModifiedByPropertyUID { get; set; }

        /// <summary>
        /// Used for LOG.
        /// The name of the property that modified the program.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ModifiedByPropertyName { get; set; }

        /// <summary>
        /// Used for LOG.
        /// Migrate guests from a specific levelNr to another LevelNr.
        /// Key - Is level Nr of From Level.
        /// Value - Is level Nr of To Level.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<byte, byte> MigrateGuestsBetweenLevels { get; set; }
    }
}

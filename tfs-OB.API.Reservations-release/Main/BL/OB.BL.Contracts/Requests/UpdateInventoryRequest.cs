using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateInventoryRequest : RequestBase
    {
        /// <summary>
        /// The UID of the Property for which the Inventory is going to be updated.
        /// </summary>
        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public List<RoomInventory> Values { get; set; }

        [DataMember]
        public bool IsBOUser { get; set; }

        [DataMember]
        public string SenderIdentifier { get; set; }
    }
}
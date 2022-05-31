using OB.Reservation.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListInventoryRequest : RequestBase
    {
        /// <summary>
        /// Colection of Tuples with RoomTypeID and Date Range.
        /// </summary>
        [DataMember]
        public List<InventorySearch> roomTypeIdsAndDateRange { get; set; }

    }
}
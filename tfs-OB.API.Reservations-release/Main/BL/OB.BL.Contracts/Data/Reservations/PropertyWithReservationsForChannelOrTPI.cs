using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class PropertyWithReservationsForChannelOrTPI : ContractBase
    {
        public PropertyWithReservationsForChannelOrTPI()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

    }
}
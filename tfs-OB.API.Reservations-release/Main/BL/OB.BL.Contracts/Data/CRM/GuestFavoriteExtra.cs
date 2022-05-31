using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class GuestFavoriteExtra : ContractBase
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Extras_UID { get; set; }

        [DataMember]
        public long Guest_UID { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Rates
{
    [DataContract]
   public class ExtrasBillingType : ContractBase
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public long Extras_UID { get; set; }
    }
}

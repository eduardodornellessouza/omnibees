using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetPropertyBaseCurrencyByPropertyUIDRequest : RequestBase
    {
        [DataMember]
        public long PropertyUID { get; set; }

        [DataMember]
        public bool? IsDemo { get; set; }

        [DataMember]
        public bool? IsActive { get; set; }
    }
}

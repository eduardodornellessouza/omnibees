using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListPropertyCurrencyRequest : PagedRequestBase
    {
        [DataMember]
        public List<long> UIDs { get; set; }

        [DataMember]
        public List<long> PropertyUIDs { get; set; }

        [DataMember]
        public List<long> CurrencyUIDs { get; set; }

        [DataMember]
        public bool? IsAutomaticExchangeRate { get; set; }

        [DataMember]
        public bool? IsActive { get; set; }
    }
}
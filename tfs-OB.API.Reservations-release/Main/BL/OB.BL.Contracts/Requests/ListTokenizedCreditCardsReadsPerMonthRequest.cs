using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListTokenizedCreditCardsReadsPerMonthRequest : PagedRequestBase
    {

        [DataMember]
        public List<long> UIDs { get; set; }

        [DataMember]
        public List<long> Property_UIDs { get; set; }

        [DataMember]
        public List<int> Years { get; set; }

        [DataMember]
        public List<short> Months { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{

    [DataContract]
    public class RegistTokenizedCreditCardsReadsPerMonthRequest : RequestBase
    {

        [DataMember]
        public Dictionary<long,long> PropertyUIDs_NrReadsToIncrement { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class TokenizedCreditCardsReadsPerMonth : ContractBase
    {

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Property_UID { get; set; }

        [DataMember]
        public int YearNr { get; set; }

        [DataMember]
        public short MonthNr { get; set; }

        [DataMember]
        public long NrOfCreditCardReads { get; set; }

        [DataMember]
        public Nullable<System.DateTime> LastModifiedDate { get; set; }

    }
}

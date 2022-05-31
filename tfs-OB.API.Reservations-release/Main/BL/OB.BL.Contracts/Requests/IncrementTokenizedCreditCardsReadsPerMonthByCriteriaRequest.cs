using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long YearNr { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long MonthNr { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long countsToIncrement { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime time { get; set; }

    }
}

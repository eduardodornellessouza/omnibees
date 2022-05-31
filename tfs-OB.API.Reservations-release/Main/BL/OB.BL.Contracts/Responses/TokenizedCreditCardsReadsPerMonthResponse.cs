using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class TokenizedCreditCardsReadsPerMonthResponse : ResponseBase
    {
        [DataMember]
        public IEnumerable <TokenizedCreditCardsReadsPerMonth> Result { get; set; }

        [DataMember]
        public long ResultIncrement { get; set; }
    }
}

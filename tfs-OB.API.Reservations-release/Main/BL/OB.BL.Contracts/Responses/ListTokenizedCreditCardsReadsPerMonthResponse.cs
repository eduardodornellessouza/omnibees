using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;


namespace OB.Reservation.BL.Contracts.Responses
{

    [DataContract]
    public class ListTokenizedCreditCardsReadsPerMonthResponse : PagedResponseBase
    {
        [DataMember]
        public List<TokenizedCreditCardsReadsPerMonth> Results { get; set; }
    }

}
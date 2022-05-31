using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class IncrementTokenizedCreditCardsReadsPerMonthByCriteriaResponse : ResponseBase
    {
        [DataMember]
        public long Result { get; set; }
    }
}

using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GetExchangeRatesBetweenCurrenciesResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal ExchangeRate { get; set; }
    }
}

using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetExchangeRatesBetweenCurrenciesRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long BaseCurrencyUid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CurrencyUid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyUid { get; set; }
    }
}

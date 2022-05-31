using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.General
{
    [DataContract]
    public class Currency : ContractBase
    {
        public Currency()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Symbol { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencySymbol { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> DefaultPositionNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PaypalCurrencyCode { get; set; }
    }
}
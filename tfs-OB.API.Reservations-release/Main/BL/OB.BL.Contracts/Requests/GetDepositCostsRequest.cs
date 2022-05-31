using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetDepositCostsRequest : RequestBase
    {
        /// <summary>
        /// The reservation to calculate rooms cancelation costs.
        /// The reservaion must contains reservation rooms and PropertyBaseCurrencyExchangeRate in case of ConvertToRateCurrency == true.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Data.Reservations.Reservation Reservation { get; set; }

        /// <summary>
        /// When is true the cost will be converted to Rate currency.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool ConvertToRateCurrency { get; set; }

        /// <summary>
        /// Optional. Simulate the cost of the deposit on a given date.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? DepositDate { get; set; }
    }
}

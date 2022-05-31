using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetCancelationCostsRequest : RequestBase
    {
        /// <summary>
        /// The reservation to calculate rooms cancelation costs.
        /// The reservaion must contains reservation rooms and PropertyBaseCurrencyExchangeRate in case of ConvertToRateCurrency == true.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public OB.Reservation.BL.Contracts.Data.Reservations.Reservation Reservation { get; set; }

        /// <summary>
        /// When is true the cost will be converted to Rate currency.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool ConvertToRateCurrency { get; set; }

        /// <summary>
        /// Optional. Simulate the cost of the cancellation on a given date.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CancellationDate { get; set; }
    }
}

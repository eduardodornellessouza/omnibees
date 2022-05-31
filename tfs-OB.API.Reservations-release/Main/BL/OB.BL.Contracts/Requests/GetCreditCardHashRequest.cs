using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetCreditCardHashRequest : RequestBase
    {
        /// <summary>
        /// The name of Credit Card Holder.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CardHolder { get; set; }

        /// <summary>
        /// The Encrypted Card Number.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string EncryptedCardNumber { get; set; }

        /// <summary>
        /// The Encrypted CVV number.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string EncryptedCVV { get; set; }

        /// <summary>
        /// The Credit Card Expiration Date.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CardExpiration { get; set; }
    }
}

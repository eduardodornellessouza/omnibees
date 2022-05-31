using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class CreditCardResponse : ResponseBase
    {
        /// <summary>
        /// The Card Holder Name.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CardHolder { get; set; }

        /// <summary>
        /// The Decrypted Card Number.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CardNumber { get; set; }

        /// <summary>
        /// The Decrypted CVV number.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CVV { get; set; }

        /// <summary>
        /// The Card Expiration Date.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CardExpiration { get; set; }
    }
}

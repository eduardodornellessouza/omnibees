using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListDecryptedCreditCardResponse : ResponseBase
    {
        /// <summary>
        /// The Info of decrypted cards.
        /// Key - ReservationUID
        /// Value - Decrypted Credit Card info
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<long, CreditCardResponse> DecryptedCards { get; set; }
    }
}

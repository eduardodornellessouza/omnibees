using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListDecryptedCreditCardRequest : RequestBase
    {
        /// <summary>
        /// Key - The Reservation UID.
        /// Value - The Card Number Hash (Encrypted number or Token).
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<long, string> ReservationCardNumbersHashes { get; set; }
    }
}

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListCreditCardResponse : ResponseBase
    {
        /// <summary>
        /// List of Credit Card Strings
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<string> CreditCards { get; set; }
    }
}
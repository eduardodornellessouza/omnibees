using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListCreditCardRequest : RequestBase
    {
        /// <summary>
        /// List of Credit Card Strings
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> CreditCards { get; set; }

        public ListCreditCardRequest()
        {
            CreditCards = new List<string>();
        }
    
    }
}
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of Currencies.
    /// </summary>
    [DataContract]
    public class ListCurrencyRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the Currencies to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        [DataMember]
        public bool GetOnlyClientCurrencies { get; set; }

        /// <summary>
        /// The list of Client UID's for return all currencies of all client properties.
        /// </summary>
        [DataMember]
        public List<long> ClientUIDs { get; set; }
        
        /// <summary>
        /// The list of Names for the Currencies to return.
        /// </summary>
        [DataMember]
        public List<string> Names { get; set; }
        
        /// <summary>
        /// The list of CountryCodes in ISO Format for the Currencies to return.
        /// </summary>
        [DataMember]
        public List<string> Symbols { get; set; }

        /// <summary>
        /// The list of CurrencySymbols for the Currencies to return.
        /// </summary>
        [DataMember]
        public List<string> CurrencySymbols { get; set; }

        /// <summary>
        /// The list of PaypalCurrencyCodes for the Currencies to return.
        /// </summary>
        [DataMember]
        public List<string> PaypalCurrencyCodes { get; set; }
    }
}
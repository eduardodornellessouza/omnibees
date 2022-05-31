using OB.Reservation.BL.Contracts.Data.General;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListCurrencyResponse : PagedResponseBase
    {
        [DataMember]
        public IList<Currency> Result { get; set; }

        /// <summary>
        /// Key: ClientUID
        /// Value: List of currencies
        /// </summary>
        [DataMember]
        public Dictionary<long, List<Currency>> ClientPropertiesCurrencies { get; set; }
    }
}
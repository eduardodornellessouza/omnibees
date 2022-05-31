using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListIncentiveResponse : PagedResponseBase
    {
        [DataMember]
        public IList<Incentive> Result { get; set; }

        /// <summary>
        /// Dictionary of IncentiveLanguage instances by IncentiveLanguage UID.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<long, IncentiveLanguage> IncentiveLanguageLookup { get; set; }
    }
}
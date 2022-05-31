using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that search for a set of RatesChannels.
    /// </summary>
    [DataContract]
    public class ListRatesChannelsRequest : PagedRequestBase
    {
        /// <summary>
        /// The list of UID's for the RatesChannels to return.
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }

        /// <summary>
        /// List of RateUID's for which to list the RatesChannels.
        /// </summary>
        [DataMember]
        public List<long> RateUIDs { get; set; }

        /// <summary>
        /// List of ChannelUID's for which to list the RatesChannels.
        /// </summary>
        [DataMember]
        public List<long> ChannelUIDs { get; set; }

        /// <summary>
        /// Boolean to select to exclude deleted RatesChannels from results.
        /// </summary>
        [DataMember]
        public bool? ExcludeDeleteds { get; set; }

        /// <summary>
        /// Include Payment types available for the chanes and the rate
        /// </summary>
        [DataMember]
        public bool IncludePaymentTypes { get; set; }
    }
}
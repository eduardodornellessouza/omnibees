using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateRateChannelRequest : RequestBase
    {
        /// <summary>
        /// Gets or sets the channel UI ds to delete.
        /// </summary>
        /// <value>
        /// The channel UIDs to set RateChannels as IsDeleted = true.
        /// </value>
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public List<long> ChannelUIDsToDelete { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> PropertyUIDsToDelete { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool DeleteRatesChannels { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool DeleteChannelsProperties { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ModifiedByUserUID { get; set; }
    }
}

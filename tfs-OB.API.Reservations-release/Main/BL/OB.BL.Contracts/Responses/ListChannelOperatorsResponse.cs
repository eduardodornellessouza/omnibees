using OB.Reservation.BL.Contracts.Data.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of ChannelOperator objects.
    /// </summary>
    public class ListChannelOperatorsResponse : PagedResponseBase
    {
        [DataMember]
        public IList<ChannelOperator> Result { get; set; }
    }
}

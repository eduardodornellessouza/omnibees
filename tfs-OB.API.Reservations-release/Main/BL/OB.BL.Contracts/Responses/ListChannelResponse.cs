using OB.Reservation.BL.Contracts.Data.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of Channel objects.
    /// </summary>
    [DataContract]
    public class ListChannelResponse : PagedResponseBase
    {
        [DataMember]
        public IList<Channel> Result { get; set; }
    }
}
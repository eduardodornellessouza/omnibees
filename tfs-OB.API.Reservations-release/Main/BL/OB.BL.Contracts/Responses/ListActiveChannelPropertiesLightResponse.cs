using OB.Reservation.BL.Contracts.Data.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of ChannelPropertyLight objects.
    /// </summary>
    [DataContract]
    public class ListActiveChannelPropertiesLightResponse : ResponseBase
    {
        [DataMember]
        public IList<ChannelPropertyLight> Result { get; set; }
    }
}
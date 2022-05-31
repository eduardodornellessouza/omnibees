using OB.Reservation.BL.Contracts.Data.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an ListChannelRoomCodes RESTful operation.
    /// It provides the Result field that contains the ChannelRoomCodes for the given list criteria.
    /// </summary>
    [DataContract]
    public class ListChannelRoomCodeResponse : PagedResponseBase
    {
        [DataMember]
        public IList<ChannelRoomCode> Result { get; set; }
    }
}
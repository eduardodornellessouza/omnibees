using OB.Reservation.BL.Contracts.Data.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an ListChannelRoomTypeCodes RESTful operation.
    /// It provides the Result field that contains the ChannelRoomTypeCodes for the given list criteria.
    /// </summary>
    [DataContract]
    public class ListChannelRoomTypeCodeResponse : PagedResponseBase
    {
        [DataMember]
        public IList<ChannelRoomTypeCode> Result { get; set; }
    }
}
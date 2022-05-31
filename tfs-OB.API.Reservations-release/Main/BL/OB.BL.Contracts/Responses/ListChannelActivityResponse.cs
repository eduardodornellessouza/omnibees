using OB.Reservation.BL.Contracts.Data.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListChannelActivityResponse : ResponseBase
    {
        [DataMember]
        public IList<ConnectorMessage> Result { get; set; }

        [DataMember]
        public int TotalRecords { get; set; }
    }
}
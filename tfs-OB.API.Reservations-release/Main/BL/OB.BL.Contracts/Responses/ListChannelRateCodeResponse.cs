using OB.Reservation.BL.Contracts.Data.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListChannelRateCodeResponse : PagedResponseBase
    {
        [DataMember]
        public IList<ChannelRateCode> Result { get; set; }
    }
}
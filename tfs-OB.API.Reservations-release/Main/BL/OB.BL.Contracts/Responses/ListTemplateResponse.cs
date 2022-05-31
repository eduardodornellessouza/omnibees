using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListTemplateResponse : ResponseBase
    {
        [DataMember(IsRequired = true)]
        public List<string> Result { get; set; }
    }
}
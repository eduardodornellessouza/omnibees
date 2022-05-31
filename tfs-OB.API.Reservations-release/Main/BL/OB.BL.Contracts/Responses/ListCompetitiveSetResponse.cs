using OB.Reservation.BL.Contracts.Data.SRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListCompetitiveSetResponse : ResponseBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<CompetitiveSetProperty> Result { get; set; }
    }
}

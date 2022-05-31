using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListCompetitiveSetRequest : RequestBase
    {
        /// <summary>
        /// The Property_UID in the CompetitiveSetProperty to return
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }
    }
}

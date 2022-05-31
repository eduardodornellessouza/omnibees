using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class GetBETemplateResponse : ResponseBase
    {
        /// <summary>
        /// Template Id
        /// </summary>
        [DataMember]
        public long UID { get; set; }

        /// <summary>
        /// Template URL
        /// </summary>
        [DataMember]
        public string BETemplateTypeURL { get; set; }
    }
}

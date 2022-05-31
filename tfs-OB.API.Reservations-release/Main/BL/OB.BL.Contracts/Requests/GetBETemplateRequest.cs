using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetBETemplateRequest : RequestBase
    {
        /// <summary>
        /// If True will be return a property template, if false a client template
        /// </summary>
        [DataMember]
        public bool IsProperty { get; set; }

        /// <summary>
        /// PropertyId if IsProperty is true, client id otherwise
        /// </summary>
        [DataMember]
        public long UID { get; set; }
    }
}

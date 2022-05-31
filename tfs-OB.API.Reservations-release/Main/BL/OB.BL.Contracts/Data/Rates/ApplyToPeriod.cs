using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class ApplyToPeriod
    {
        [DataMember]
        public System.DateTime DateFrom { get; set; }

        [DataMember]
        public System.DateTime DateTo { get; set; }
    }
}

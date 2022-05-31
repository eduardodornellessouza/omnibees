using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class UpdatePeriod : ContractBase
    {
        public UpdatePeriod()
        { 
        }

        [DataMember]
        public DateTime DateFrom { get; set; }

        [DataMember]
        public DateTime DateTo { get; set; }
    }
}

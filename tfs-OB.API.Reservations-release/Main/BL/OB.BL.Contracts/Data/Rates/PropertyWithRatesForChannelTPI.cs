using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class PropertyWithRatesForChannelTPI : ContractBase
    {
        public PropertyWithRatesForChannelTPI()
        {
        }

        [DataMember]
        public long Property_UID { get; set; }


        [DataMember]
        public bool IsPropertyActiveForTPI { get; set; }

    }
}

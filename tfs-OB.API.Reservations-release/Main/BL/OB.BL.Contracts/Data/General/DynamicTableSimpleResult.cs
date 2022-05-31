using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.BL.Contracts.Data.General
{
    [DataContract]
    public class DynamicTableSimpleResult
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}

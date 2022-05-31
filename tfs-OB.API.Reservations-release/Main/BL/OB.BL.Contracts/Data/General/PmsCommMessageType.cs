using System;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.General
{
    [DataContract]
    public class PMSCommMessageType : ContractBase
    {
        public PMSCommMessageType()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
using System;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class BoardType : ContractBase
    {
        public BoardType()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
    }
}
using System.Runtime.Serialization;
namespace OB.BL.Contracts.Data.General
{
    public class Prefix : ContractBase
    {
        public Prefix()
        {
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public long UID { get; set; }
    }
}
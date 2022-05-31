using System.Runtime.Serialization;
namespace OB.BL.Contracts.Data.General
{
    public class PrefixLanguage : ContractBase
    {
        public PrefixLanguage()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public long Prefix_UID { get; set; }

        [DataMember]
        public long Language_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Language Language { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Prefix Prefix { get; set; }
    }
}
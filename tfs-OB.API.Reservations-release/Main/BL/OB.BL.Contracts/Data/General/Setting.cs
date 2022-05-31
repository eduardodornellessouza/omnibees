using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.General
{
    [DataContract]
    public class Setting : ContractBase
    {
        public Setting()
        {
        }

        public string Name { get; set; }

        public string Category { get; set; }
    }
}
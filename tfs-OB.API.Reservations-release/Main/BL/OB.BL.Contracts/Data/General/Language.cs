using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.General
{
    [DataContract]
    public class Language : ContractBase
    {
        public Language()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Code { get; set; }
    }
}
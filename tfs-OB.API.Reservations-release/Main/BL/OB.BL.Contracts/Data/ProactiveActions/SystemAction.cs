using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.ProactiveActions
{
    [DataContract]
    public class SystemAction : ContractBase
    {
        public SystemAction()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Code { get; set; }
    }
}
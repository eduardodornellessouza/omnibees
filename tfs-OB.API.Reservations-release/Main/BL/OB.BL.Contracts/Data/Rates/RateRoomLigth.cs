using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateRoomLight : ContractBase
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long RoomType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }
    }
}
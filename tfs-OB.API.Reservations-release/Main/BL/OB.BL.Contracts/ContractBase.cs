using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts
{
    [DataContract]
    public abstract class ContractBase
    {
        //[DataMember]
        //public Guid Guid { get; set; }
        public ContractBase()
        {
        }
    }
}
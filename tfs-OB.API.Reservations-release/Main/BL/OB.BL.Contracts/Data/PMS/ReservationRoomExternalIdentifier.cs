using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.PMS
{
    [DataContract]
    public class ReservationRoomExternalIdentifier : ContractBase
    {
        /// <summary>
        /// String to indicate the Pms/External System reservation room number
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ExternalNumber { get; set; }

        /// <summary>
        /// OB reservation room Id
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long InternalId { get; set; }

        /// <summary>
        /// OB reservation room Status
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long Status { get; set; }
    }
}

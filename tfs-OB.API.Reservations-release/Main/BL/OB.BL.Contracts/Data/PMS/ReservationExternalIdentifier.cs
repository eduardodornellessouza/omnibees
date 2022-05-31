using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.PMS
{
    [DataContract]
    public class ReservationExternalIdentifier : ContractBase
    {
        public ReservationExternalIdentifier()
        {
            ReservationRoomExternalIdentifiers = new List<ReservationRoomExternalIdentifier>();
        }

        /// <summary>
        /// String to indicate the Pms/External System reservation number
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ExternalNumber { get; set; }

        /// <summary>
        /// OB reservation Id
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long InternalId { get; set; }

        /// <summary>
        /// Bool to indicate if the reservation is processed by reservation or by reservation room
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsByReservationRoom { get; set; }

        /// <summary>
        /// List of reservation rooms to be updated
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<ReservationRoomExternalIdentifier> ReservationRoomExternalIdentifiers { get; set; }
    }
}

using OB.Reservation.BL.Contracts.Data.PMS;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class to be used in operations that saves or updates Pms numbers / External Systems identifiers.
    /// </summary>
    [DataContract]
    public class SaveReservationExternalIdentifierRequest : RequestBase
    {
        /// <summary>
        /// OB property Id
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long PropertyId { get; set; }

        /// <summary>
        /// OB client Id
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long ClientId { get; set; }

        /// <summary>
        /// OB pms Id
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long PmsId { get; set; }

        /// <summary>
        /// reservation in OB to update the pms/external system number
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ReservationExternalIdentifier ReservationExternalIdentifier { get; set; }
    }
}

using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class InsertInReservationInternalNotesRequest : RequestBase
    {
        /// <summary>
        /// The reservation number that will used.
        /// </summary>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public string ReservationNumber { get; set; }

        /// <summary>
        /// String that will be added to notes.
        /// </summary>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public string NotesToApped { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Reservations
{
    /// <summary>
    /// The entity that represents the association between a Rate entity and a TravelAgent entity.
    /// </summary>
    [DataContract]
    public class ReservationStatus : ReservationStatusBase
    {
        /// <summary>
        /// The unique identifier of ReservationStatus.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Id { get; set; }

        /// <summary>
        /// The Name of the ReservationStatus.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
    }

    /// <summary>
    /// The ReservationStatus entity linked to other entity.
    /// </summary>
    [DataContract]
    public class LinkedReservationStatus : ReservationStatusBase
    {
        /// <summary>
        /// Unique identifier of the related item
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Key { get; set; }

        /// <summary>
        /// <c>True</c> if the item is deleted.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Base property fields of RateChannel
    /// </summary>
    [DataContract]
    public abstract class ReservationStatusBase : DataContractBase
    {

    }

    public enum ReservationStatusFields
    {
        Id = 1,
        Name = 2,
    }
}

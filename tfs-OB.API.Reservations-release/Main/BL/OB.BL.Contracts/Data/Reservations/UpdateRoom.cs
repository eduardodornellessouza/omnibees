namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class UpdateRoom : ContractBase
    {
        public UpdateRoom()
        {
        }

        /// <summary>
        /// Room id associated with the reservation room
        /// </summary>
        [DataMember]
        public long? RoomType_UID { get; set; }

        /// <summary>
        /// Checkin
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Checkout
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Adult count
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int? AdultCount { get; set; }

        /// <summary>
        /// Child count
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int? ChildCount { get; set; }

        /// <summary>
        /// Child ages
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<int> ChildAges { get; set; }

        /// <summary>
        /// Number associated with the reservation room
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string Number { get; set; }

        /// <summary>
        /// Guest information associated with the reservation room
        /// Only FirstName and LastName are valide to update reservation room
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ReservationGuest Guest { get; set; }

        /// <summary>
        /// Rate id associated with the reservation room
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? Rate_UID { get; set; }
    }
}
using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ModifyReservationRequest : ReservationBaseRequest
    {
        /// <summary>
        /// Uid of Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ReservationUid { get; set; }

        /// <summary>
        /// Number of Reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationNumber { get; set; }

        /// <summary>
        /// Guest information associated with the reservation
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ReservationGuest Guest { get; set; }

        /// <summary>
        /// Guest information associated with the reservation
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public BillingInfo BillingInfo { get; set; }

        /// <summary>
        /// ReservationRooms from reservation to modify
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<UpdateRoom> ReservationRooms { get; set; }

        /// <summary>
        /// Loyalty level Id associated with guest
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? GuestLoyaltyLevelId { get; set; }

        /// <summary>
        /// When it's true, parcelation with interest rate (if applicable) will not be calculated when sending the price to the gateway.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool SkipInterestCalculation { get; set; }

        /// <summary>
        /// The Big Pull Authentication User that modified the reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? BigPullAuthRequestor_UID { get; set; }

        /// <summary>
        /// The Big Pull Authentication User reservation owner.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? BigPullAuthOwner_UID { get; set; }
    }
}
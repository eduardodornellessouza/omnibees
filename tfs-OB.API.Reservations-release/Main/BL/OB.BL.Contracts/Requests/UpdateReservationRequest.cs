using OB.Reservation.BL.Contracts.Attributes;
using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Update request class for update operations of the Reservation entity.
    /// </summary>
    [DataContract]
    [ContainsCC]
    public class UpdateReservationRequest : ReservationBaseRequest
    {
        [DataMember]
        public virtual Guest Guest { get; set; }

        [DataMember]
        [RecursiveMask]
        public virtual Reservation.BL.Contracts.Data.Reservations.Reservation Reservation { get; set; }

        [DataMember]
        public virtual List<long> GuestActivities { get; set; }

        [DataMember]
        public virtual List<ReservationRoom> ReservationRooms { get; set; }

        [DataMember]
        public virtual List<ReservationRoomDetail> ReservationRoomDetails { get; set; }

        [DataMember]
        public virtual List<ReservationRoomExtra> ReservationRoomExtras { get; set; }

        [DataMember]
        public virtual List<ReservationRoomChild> ReservationRoomChilds { get; set; }

        [DataMember]
        public virtual ReservationPaymentDetail ReservationPaymentDetail { get; set; }

        [DataMember]
        public virtual List<ReservationRoomExtrasSchedule> ReservationExtraSchedules { get; set; }

        [DataMember]
        public virtual bool HandleCancelationCost { get; set; }

        [DataMember]
        public virtual bool HandleDepositCost { get; set; }

        [DataMember]
        public virtual bool ValidateAllotment { get; set; }


        /// <summary>
        /// When it's true, the reservation won't deduct inventory/allotment in the property.
        /// </summary>
        [DataMember]
        public virtual bool IgnoreAvailability { get; set; }

        /// <summary>
        /// Validate guarantee in reservation
        /// </summary>
        [DataMember]
        public virtual bool ValidateGuarantee { get; set; }

        /// <summary>
        /// When it's true, parcelation with interest rate (if applicable) will not be calculated when sending the price to the gateway.
        /// </summary>
        [DataMember]
        public virtual bool SkipInterestCalculation { get; set; }

        /// <summary>
        /// When it's true, the reservation won't be sent for the PMS, because it has already been sent by CVC.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual bool AlreadySentToPMS { get; set; }

        /// <summary>
        /// The Big Pull Authentication User that modified the reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual long? BigPullAuthRequestor_UID { get; set; }

        /// <summary>
        /// The Big Pull Authentication User reservation owner.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? BigPullAuthOwner_UID { get; set; }
    }
}
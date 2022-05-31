using OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class CancelReservationRequest : ReservationBaseRequest
    {
        [DataMember]
        public long PropertyUID { get; set; }

        /// <summary>
        /// Optional. Either you use the ReservationNo field or the ReservationUID field.
        /// </summary>
        [DataMember]
        public long? ReservationUID { get; set; }

        [DataMember]
        public string ReservationNo { get; set; }

        [DataMember]
        public string ReservationRoomNo { get; set; }

        [DataMember]
        public int UserType { get; set; }

        [DataMember]
        public int? OBUserType { get; set; }

        [DataMember]
        public int? CancelReservationReason_UID { get; set; }

        [DataMember]
        public string CancelReservationComments { get; set; }

        /// <summary>
        /// When its true, the cancelation costs are calculated, but the reservation is not cancelled
        /// </summary>
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public bool GetCancelationCostsOnly { get; set; }

        /// <summary>
        /// Used to validate maximum check out found on reservation rooms for reservation
        /// </summary>
        [DataMember]
        public DateTime? MaxCheckOut { get; set; }

        /// <summary>
        /// Used to validate minimum check in found on reservation rooms for reservation
        /// </summary>
        [DataMember]
        public DateTime? MinCheckIn { get; set; }

        /// <summary>
        /// When its true, in the case of BrasPag gateway, it will skip the process of void in the case of cancelation since has already been voided from their side.
        /// </summary>
        [DataMember]
        public bool AlreadyCancelledFromPaymentGateway { get; set; }

        /// <summary>
        /// When it's true, parcelation with interest rate (if applicable) will not be calculated when sending the price to the gateway.
        /// </summary>
        [DataMember]
        public bool SkipInterestCalculation { get; set; }

        /// <summary>
        /// When it's true, the reservation won't be sent for the PMS, because it has already been sent by CVC.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool AlreadySentToPMS { get; set; }

        /// <summary>
        /// Booking engine send the template info to inform the hotel for handle the cancelation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TemplateCancelRequest TemplateCancelRequest { get; set; }

        /// <summary>
        /// When it's true, the reservation won't deduct inventory/allotment in the property.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IgnoreAvailability { get; set; }

        /// <summary>
        /// The Big Pull Authentication User that cancels the reservation.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? BigPullAuthRequestor_UID { get; set; }

        /// <summary>
        /// The Big Pull Authentication User reservation owner.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> BigPullAuthOwner_UID { get; set; }
    }
}
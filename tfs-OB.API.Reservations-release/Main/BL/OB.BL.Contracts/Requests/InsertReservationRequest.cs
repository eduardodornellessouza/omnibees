using Newtonsoft.Json;
using OB.Reservation.BL.Contracts.Attributes;
using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that stores the arguments for the InsertReservation RESTfull operation.
    /// The InsertReservation Request object that contains all the required objects
    /// (Guest, Reservation , ReservationRooms, ReservationRoomDetails, ReservationRoomExtras, ReservationRoomChilds, ReservationPaymentDetails, ReservationPartialPaymentDetails)
    /// to create and insert a Reservation into the database.
    /// The relationship between the objects are maintained using the UIDs despite the fact that they are not Database UIDs but logical UIDs (valid in the scope of the object graph).
    /// </summary>
    [DataContract]
    [ContainsCC] 
    public class InsertReservationRequest : ReservationBaseRequest
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

        [DefaultValue(true)]
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate | DefaultValueHandling.Include)]
        public virtual bool HandleCancelationCost { get; set; }

        [DefaultValue(true)]
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate | DefaultValueHandling.Include)]
        public virtual bool HandleDepositCost { get; set; }

        [DataMember]
        public virtual bool ValidateAllotment { get; set; }


        /// <summary>
        /// When it's true, the reservation won't deduct inventory/allotment in the property.
        /// </summary>
        [DataMember]
        public virtual bool IgnoreAvailability { get; set; }

        /// <summary>
        /// Reservation additional information
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual ReservationsAdditionalData ReservationsAdditionalData { get; set; }

        /// <summary>
        /// Validate guarantee in reservation
        /// </summary>
        [DataMember]
        public virtual bool ValidateGuarantee { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual bool? UsePaymentGateway { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string AntifraudDeviceFingerPrintId { get; set; }

        /// <summary>
        /// When it's true, parcelation with interest rate (if applicable) will not be calculated when sending the price to the gateway.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual bool SkipInterestCalculation { get; set; }
    }
}
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an UpdateReservation RESTful operation.
    /// It provides the Result field that contains the UID of the updated Reservation or business error code if there was one.
    /// </summary>
    [DataContract]
    public class UpdateReservationResponse : ResponseBase
    {
        /// <summary>
        /// UID of the updated Reservation when sucessfull otherwise it returns 0.
        /// You need to check the Status field and Errors collection if the Result is 0.
        /// Update reservation error code:
        /// <para/>INSERT GENERAL ERROR= 0 
        /// <para/>Invalid Credit Card error= -4000 
        /// <para/>Error must occur when CardNumber field is empty= -4100 
        /// <para/>Invalid guarantee type, DepositInformation is empty= -4101
        /// <para/>Guarantee type is not selected in OB= -4102
        /// <para/>Error must occur when PaymentMethodType_UID field is null= -4103
        /// <para/>ReservationError-Reservation detail must be provided= -4104
        /// This was done to maintain retrocompatibility with old Clients.
        /// </summary>
        /// <see cref="ResponseBase.Errors"/>
        /// <see cref="ResponseBase.Status"/>
        [DataMember]
        public long Result { get; set; }
    }
}
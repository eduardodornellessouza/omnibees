using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an InsertReservation RESTful operation.
    /// It provides the Result field that contains the UID of the created Reservation or business error code if there was one.
    /// </summary>
    [DataContract]
    public class InsertReservationResponse : ResponseBase
    {
        /// <summary>
        /// UID of the created Reservation if sucessfull otherwise it's a negative integer then defines a specific business code:
        /// <para/>INSERT GENERAL ERROR= 0 
        /// <para/>MaxiPagoGeneralError = -1 
        /// <para/>OBInternalError = -250 
        /// <para/>InvalidAllotment = -500 
        /// <para/>MaxiPagoAcquirerError = -1022 
        /// <para/>MaxiPagoParametersError = -1024 
        /// <para/>MaxiPagoMerchantCredentialError = -1025 
        /// <para/>MaxiPagoInternalError = -2048 
        /// <para/>TPI CREDIT LIMIT EXCEDED error= -3000 
        /// <para/>Operator CREDIT LIMIT EXCEDED error= -3001 
        /// <para/>Invalid Payment Method Type error= -3002 
        /// <para/>Invalid Credit Card error= -4000 
        /// <para/>Error must occur when CardNumber field is empty= -4100 
        /// <para/>Invalid guarantee type, DepositInformation is empty= -4101
        /// <para/>Guarantee type is not selected in OB= -4102
        /// <para/>Error must occur when PaymentMethodType_UID field is null= -4103
        /// <para/>ReservationError-Reservation detail must be provided= -4104
        /// 
        /// <para/>less than 0, it's the MaxiPago payment result message error code.
        /// <para/>0 , you need to check the Status and Errors Collection (an Exception was thrown).
        /// This was done to maintain retrocompatibility with old Clients.
        /// </summary>
        [DataMember]
        public long Result { get; set; }

        [DataMember]
        public string Number { get; set; }

        [DataMember]
        public OB.Reservation.BL.Constants.ReservationStatus ReservationStatus { get; set; }
    }
}
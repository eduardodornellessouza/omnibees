﻿using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System.Collections.Generic;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.BL.Operations.Interfaces
{
    public interface IPaypalGatewayManagerPOCO
    {
        void Initialize(long propertyId);

        string PaypalStartPayment(Reservation.BL.Contracts.Requests.GetPayPalPaymentURLRequest request);

        /// <summary>
        /// Verify if authorization was successfull
        /// </summary>
        /// <param name="token"></param>
        /// <param name="paypalAutoGeneratedId"></param>
        /// <param name="objReservation"></param>
        /// <param name="objReservationRoom"></param>
        /// <returns></returns>
        string PaypalVerifyAuthorization(string token, string paypalAutoGeneratedId, contractsReservations.Reservation objReservation, List<contractsReservations.ReservationRoom> objReservationRoom);

        /// <summary>
        /// RESTful implementation of the CapturePayment operation.
        /// This operation capture Paypal Payment
        /// </summary>
        /// <remarks>
        /// <para>Required Parameters:</para>
        /// <para>- PayerId</para>
        /// <para>- Token</para>
        /// <para>- PaypalAutoGeneratedId</para>
        /// <para>- Reservation</para>
        /// <para>- ReservationRooms</para>
        /// </remarks>
        /// <param name="request">The request containing the criteria</param>
        /// <returns>An CreateRecurringBillingResponse that contains the PaypalTransation Id of the operation.</returns>
        CapturePaymentResponse CapturePayment(CapturePaymentRequest request);

        /// <summary>
        /// RESTful implementation of the PaypalVerifyInstallmentsAuthorization operation.
        /// This operation verify if authorization was successfull
        /// </summary>
        /// <remarks>
        /// <para>Required Parameters:</para>
        /// <para>- Token</para>
        /// <para>- PaypalAutoGeneratedId</para>
        /// <para>Optional Parameters:</para>
        /// <para>- Reservation</para>
        /// <para>- ReservationRooms</para>
        /// </remarks>
        /// <param name="request">The request containing the criteria</param>
        /// <returns>An PaypalVerifyInstallmentsAuthorizationResponse that contains the status of the operation.</returns>
        PaypalVerifyInstallmentsAuthorizationResponse PaypalVerifyInstallmentsAuthorization(PaypalVerifyInstallmentsAuthorizationRequest request);

        /// <summary>
        /// RESTful implementation of the CreateRecurringBilling operation.
        /// This operation create a recurring billing 
        /// </summary>
        /// <remarks>
        /// <para>Required Parameters:</para>
        /// <para>- Token</para>
        /// <para>- PropertyName</para>
        /// <para>- PaypalAutoGeneratedId</para> 
        /// <para>- Reservation</para>
        /// <para>- ReservationRooms</para>
        /// </remarks>
        /// <param name="request">The request containing the criteria</param>
        /// <returns>An CreateRecurringBillingResponse that contains the Profile Id of the operation.</returns>
        CreateRecurringBillingResponse CreateRecurringBilling(CreateRecurringBillingRequest request);

        /// <summary>
        /// RESTful implementation of the CancelAndRefundRecurringBilling operation.
        /// This operation cancel a Recurring payment, refund if paid
        /// </summary>
        /// <remarks>
        /// <para>Required Parameters:</para>
        /// <para>- PaymentGatewayTransactionId</para>
        /// <para>- ProfileId</para>
        /// <para>- IsPartialRefund</para> 
        /// <para>- Reservation</para>
        /// <para>- Action [CANCEL, SUSPEND, REACTIVATE]</para>
        /// <para>- Note</para>
        /// </remarks>
        /// <param name="request">The request containing the criteria</param>
        /// <returns>An CancelAndRefundRecurringBillingResponse that contains the status of the operation.</returns>
        CancelAndRefundRecurringBillingResponse CancelAndRefundRecurringBilling(CancelAndRefundRecurringBillingRequest request);

        /// <summary>
        /// RESTful implementation of the RefundPayment operation.
        /// This operation refund Paypal Payment
        /// </summary>
        /// <remarks>
        /// <para>Required Parameters:</para>
        /// <para>- TransactionId</para>
        /// <para>- PaypalTransactionId</para>
        /// <para>- RefundType [OTHER,FULL,PARTIAL,EXTERNALDISPUTE]</para> 
        /// <para>- RefundAmount</para>
        /// <para>- ReservationCurrency</para>
        /// <para>- ReservationNumber</para>
        /// </remarks>
        /// <param name="request">The request containing the criteria</param>
        /// <returns>An RefundPaymentResponse that contains the status of the operation.</returns>
        RefundPaymentResponse RefundPayment(RefundPaymentRequest request);

        /// <summary>
        /// Refund Paypal Payment
        /// </summary>
        /// <returns></returns>
        bool RefundPayment(contractsReservations.Reservation reservation, string refundType, string refundAmount, string transactionIdRecurringBillingPaypal = null);

        /// <summary>
        /// Cancel a Recurring payment, refund if paid
        /// </summary>
        /// <param name="paypalAutoGeneratedId"></param>
        /// <param name="profileId"></param>
        /// <param name="action">CANCEL, SUSPEND, REACTIVATE</param>
        /// <param name="note"></param>
        /// <returns></returns>
        bool CancelAndRefundRecurringBilling(long paymentGatewayTransactionId, string profileId, bool isPartialRefund, contractsReservations.Reservation reservation, string action = "", string note = "");

        /// <summary>
        /// Refund Paypal Recurring Payments
        /// </summary>
        /// <returns></returns>
        bool RefundRecurringPayment(string transactionId, string refundType, string refundAmount, long reservationCurrency, long propertyId);

        /// <summary>
        /// RESTful implementation of the GetPayPalPaymentURL operation.
        /// This operation get paypal payment url.
        /// </summary>
        /// <param name="request">A GetPayPalPaymentURLRequest object containing the criteria</param>
        /// <returns>A GetPayPalPaymentURLResponse containing the URL and Token</returns>
        GetPayPalPaymentURLResponse GetPayPalPaymentURL(Reservation.BL.Contracts.Requests.GetPayPalPaymentURLRequest request);

        LogTransactionResponse LogPaypalTransaction(Reservation.BL.Contracts.Requests.LogTransactionRequest request);

    }
}
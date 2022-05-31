namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReservationPaymentDetail : ContractBase
    {
        public ReservationPaymentDetail()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public Nullable<long> PaymentMethod_UID { get; set; }

        [DataMember]
        public string PaymentMethodName { get; set; }

        [DataMember]
        public long Reservation_UID { get; set; }

        [DataMember]
        public Nullable<decimal> Amount { get; set; }

        [DataMember]
        public Nullable<long> Currency_UID { get; set; }

        [DataMember]
        public string CVV { get; set; }

        [DataMember]
        public string CardName { get; set; }

        [DataMember]
        public string CardNumber { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ExpirationDate { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ActivationDate { get; set; }

        [DataMember]
        public string ClientSideEncryptedCreditCard { get; set; }

        [DataMember]
        public bool PaymentGatewayTokenizationIsActive { get; set; }

        [DataMember]
        public bool OBTokenizationIsActive { get; set; }

        [DataMember]
        public string CreditCardToken { get; set; }

        [DataMember]
        public byte[] HashCode { get; set; }

        /// <summary>
        /// The reservation ID generate when Virtual Card Number (VCN) is generated.
        /// </summary>
        [DataMember]
        public string VCNReservationId { get; set; }

        /// <summary>
        /// The token of Virtual Card Number (VCN).
        /// This token can be used to retrieve the detailed information about VCN.
        /// </summary>
        [DataMember]
        public string VCNToken { get; set; }
    }
}